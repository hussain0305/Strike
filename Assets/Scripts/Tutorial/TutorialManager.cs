using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

public class TutorialManager : MonoBehaviour
{
    [Inject]
    SceneContext sceneContext;

    public Tee tee;
    public ShotInput shotInput;
    public TrajectoryButton trajectoryButton;
    public GameObject trajectoryButtonSection;
    public TrajectorySegmentVisuals[] trajectories;

    public Button fireButton;
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

    private string tutorialBallID = "soccBall";
    
    private GameStateManager gameStateManager;
    private InputManager inputManager;
    
    [Inject]
    public void Construct(GameStateManager _gameStateManager, InputManager _inputManager)
    {
        gameStateManager = _gameStateManager;
        inputManager = _inputManager;
    }

    private void Start()
    {
        InitTutorial();
        SetupUI();
        gravity = -Physics.gravity.y;
    }

    private void OnEnable()
    {
        showTrajectory = false;
        EventBus.Subscribe<HUDAction_CheckTrajectory>(TrajectoryEnabled);
        EventBus.Subscribe<TutorialResetEvent>(TutorialReset);
        EventBus.Subscribe<GameExitedEvent>(ReturnToMainMenu);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<HUDAction_CheckTrajectory>(TrajectoryEnabled);
        EventBus.Unsubscribe<TutorialResetEvent>(TutorialReset);
        EventBus.Unsubscribe<GameExitedEvent>(ReturnToMainMenu);
    }

    private void TrajectoryEnabled(HUDAction_CheckTrajectory e)
    {
        showTrajectory = true;
    }

    private void TutorialReset(TutorialResetEvent e)
    {
        showTrajectory = false;
        ball?.ResetBall();
    }

    private void Update()
    {
        if (ball != null && ballState == BallState.OnTee)
        {
            launchForce = shotInput.powerInput.Power;
            launchAngle = shotInput.angleInput.cylinderPivot.rotation;
            spinVector = shotInput.spinInput.SpinVector;

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
        BallProperties selected = Balls.Instance.GetBall(tutorialBallID);
        GameObject spawned = Instantiate(selected.prefab, tee.ballPosition.position, Quaternion.identity, tee.transform);
        sceneContext.Container.InjectGameObject(spawned);
        ball = spawned.GetComponent<Ball>();
        
        gameStateManager.SetGameState(GameState.InGame);
        EventBus.Publish(new InGameEvent());
        inputManager.SetContext(GameContext.InGame);
        gravity = -Physics.gravity.y;
        TutorialContext = new TutorialContext();
        TutorialContext.InitTutorial(ball, shotInput, tee);
        ball.Initialize(TutorialContext, TrajectoryModifier, selected);
    }

    private void SetupUI()
    {
        fireButton.onClick.RemoveAllListeners();
        fireButton.onClick.AddListener(FireButtonPressed);

        angleIndicator.SetActive(true);
        fireButton.gameObject.SetActive(false);
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
        // StartMinTimePerShotPeriod();
        ballState = BallState.InControlledMotion;
    }

    public void DisableRelevantElementsDuringShot()
    {
        angleIndicator.SetActive(false);
        fireButton.gameObject.SetActive(false);
        DisableTrajectory();
    }
    
    public void CueNextShot()
    {
        EventBus.Publish(new NextShotCuedEvent());
        angleIndicator.SetActive(true);
        fireButton.gameObject.SetActive(true);
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
