using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public Tee tee;
    public BallParameterController ballParameterController;
    public TrajectoryButton trajectoryButton;
    public GameObject trajectoryButtonSection;
    public TrajectorySegmentVisuals[] trajectories;

    public Button fireButton;
    public Button nextButton;
    public Image nextButtonFill;
    public GameObject angleIndicator;

    private float gravity;
    private float launchForce;
    private Vector2 spinVector;
    private Quaternion launchAngle;

    private Ball ball;
    private BallState ballState = BallState.OnTee;
    private Coroutine minTimePerShotRoutine;
    private Coroutine optTimePerShotRoutine;
    private Coroutine trajectoryViewRoutine;

    private TutorialContext TutorialContext;
    private static ITrajectoryModifier trajectoryModifier;
    public static ITrajectoryModifier TrajectoryModifier
    {
        get
        {
            if (trajectoryModifier == null)
            {
                trajectoryModifier = new PortalTrajectoryModifier();
            }
            return trajectoryModifier;
        }
    }

    private bool showTrajectory;
    
    private void Start()
    {
        InitTutorial();
        SetupUI();
        gravity = -Physics.gravity.y;
    }

    private void OnEnable()
    {
        showTrajectory = false;
        EventBus.Subscribe<TrajectoryEnabledEvent>(TrajectoryEnabled);
        EventBus.Subscribe<TutorialResetEvent>(TutorialReset);
        EventBus.Subscribe<GameExitedEvent>(ReturnToMainMenu);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<TrajectoryEnabledEvent>(TrajectoryEnabled);
        EventBus.Unsubscribe<TutorialResetEvent>(TutorialReset);
        EventBus.Unsubscribe<GameExitedEvent>(ReturnToMainMenu);
    }

    private void TrajectoryEnabled(TrajectoryEnabledEvent e)
    {
        showTrajectory = true;
    }

    private void TutorialReset(TutorialResetEvent e)
    {
        showTrajectory = false;
    }

    private void Update()
    {
        if (ball != null && ballState == BallState.OnTee)
        {
            launchForce = ballParameterController.powerInput.Power;
            launchAngle = ballParameterController.angleInput.cylinderPivot.rotation;
            spinVector = ballParameterController.spinInput.SpinVector;

            if (!trajectoryButtonSection.activeSelf && launchForce > 20 && launchAngle.eulerAngles.sqrMagnitude > 1)
            {
                trajectoryButtonSection.SetActive(true);
                trajectoryButton.ShowButton(100);
            }

            List<Vector3> trajectoryPoints = ball.CalculateTrajectory();
            if (showTrajectory)
            {
                DrawTrajectory(ball.trajectoryModifier.ModifyTrajectory(trajectoryPoints));
            }
        }
    }

    public void InitTutorial()
    {
        BallProperties selected = Balls.Instance.GetBall(SaveManager.GetEquippedBall());
        GameObject spawned = Instantiate(selected.prefab, tee.ballPosition.position, Quaternion.identity, tee.transform);
        ball = spawned.GetComponent<Ball>();
        CameraController.Instance.cameraFollow.Ball = ball.transform;
        
        GameStateManager.Instance.SetGameState(GameState.InGame);
        EventBus.Publish(new InGameEvent());
        InputManager.Instance.SetContext(GameContext.InGame);
        gravity = -Physics.gravity.y;
        TutorialContext = new TutorialContext();
        TutorialContext.InitTutorial(ball, ballParameterController, tee);
        ball.Initialize(TutorialContext, TrajectoryModifier);
    }

    private void SetupUI()
    {
        fireButton.onClick.RemoveAllListeners();
        fireButton.onClick.AddListener(FireButtonPressed);

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(CueNextShot);

        angleIndicator.SetActive(true);
        nextButton.gameObject.SetActive(false);
        fireButton.gameObject.SetActive(true);
    }

    public void FireButtonPressed()
    {
        if (ballState == BallState.OnTee)
        {
            ShootBall();
        }
    }
    
    public void ShootBall()
    {
        ball.Shoot();
        EventBus.Publish(new BallShotEvent());
        DisableRelevantElementsDuringShot();
        StartMinTimePerShotPeriod();
        ballState = BallState.InControlledMotion;
    }

    public void DisableRelevantElementsDuringShot()
    {
        angleIndicator.SetActive(false);
        fireButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        DisableTrajectory();
    }

    public void StartMinTimePerShotPeriod()
    {
        IEnumerator<WaitForSeconds> MinTimeRoutine()
        {
            yield return new WaitForSeconds(2);
            minTimePerShotRoutine = null;
            StartOptionalTimePerShotPeriod();
        }

        if (minTimePerShotRoutine != null)
        {
            StopCoroutine(minTimePerShotRoutine);
        }

        minTimePerShotRoutine = StartCoroutine(MinTimeRoutine());
    }
    
    public void StartOptionalTimePerShotPeriod()
    {
        IEnumerator<WaitForSeconds> OptionalTimeRoutine()
        {
            nextButton.gameObject.SetActive(true);
            float timePassed = 0;
            float optionalTime = 5;
            while (timePassed <= optionalTime)
            {
                timePassed += Time.deltaTime;
                nextButtonFill.fillAmount = timePassed / optionalTime;
                yield return null;
            }
            optTimePerShotRoutine = null;
            CueNextShot();
        }

        if (optTimePerShotRoutine != null)
        {
            StopCoroutine(optTimePerShotRoutine);
        }

        optTimePerShotRoutine = StartCoroutine(OptionalTimeRoutine());
    }
    
    public void CueNextShot()
    {
        EventBus.Publish(new NextShotCuedEvent());
        angleIndicator.SetActive(true);
        fireButton.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(false);
        ballState = BallState.OnTee;
    }

    public void DrawTrajectory(List<List<Vector3>> segments)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            trajectories[i].trajectory.positionCount = segments[i].Count;
            trajectories[i].trajectory.SetPositions(segments[i].ToArray());
            trajectories[i].trajectory.enabled = true;

            trajectories[i].segmentEnd.transform.position = segments[i][^1];
            trajectories[i].segmentEnd.enabled = true;
        }

        for (int i = segments.Count; i < trajectories.Length; i++)
        {
            trajectories[i].trajectory.enabled = false;
            trajectories[i].segmentEnd.enabled = false;
        }
    }
    
    private void DisableTrajectory()
    {
        foreach (var t in trajectories)
        {
            t.trajectory.enabled = false;
            t.segmentEnd.enabled = false;
        }
        trajectoryButtonSection.SetActive(false);
    }
    
    public void ReturnToMainMenu(GameExitedEvent e)
    {
        SceneManager.LoadScene(0);
    }
}
