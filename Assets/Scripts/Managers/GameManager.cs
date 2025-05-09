using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BallShotEvent { }
public class ShotCompleteEvent { }
public class CueNextShotEvent { }
public class PreNextShotCuedEvent { }
public class NextShotCuedEvent { }
public class InGameEvent { }
public class GameEndedEvent { }
public class GameExitedEvent { }
public class TrajectoryEnabledEvent { }

public class ProjectilesSpawnedEvent
{
    public GameObject[] projectiles;
    public ProjectilesSpawnedEvent(GameObject[] _projectiles)
    {
        projectiles = _projectiles;
    }
}

public class NewRoundStartedEvent
{
    public int RoundNumber;
    public NewRoundStartedEvent(int roundNumber)
    {
        this.RoundNumber = roundNumber;
    }
}

[System.Serializable]
public class TrajectorySegmentVisuals
{
    public LineRenderer trajectory;
    public MeshRenderer segmentEnd;
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    public static bool IsGameplayActive => Instance != null;
    
    [Header("Level Objects")]
    public Tee tee;
    [HideInInspector]
    public Ball ball;

    [FormerlySerializedAs("ballInputController")] [Header("Controls Panel")]
    public BallParameterController ballParameterController;
    public TrajectoryButton trajectoryButton;
    public GameObject trajectoryButtonSection;
    public GameObject trajectoryHistoryButton;
    public TrajectorySegmentVisuals[] trajectories;
    
    [Header("Game Screen")]
    public Button fireButton;
    public Button nextButton;
    public Image nextButtonFill;
    public GameObject angleIndicator;
    public TextMeshProUGUI volleyText;
    public TextMeshProUGUI objectiveText;
    
    [Header("Results Screen")]
    public Transform resultScreen;
    
    private BallState ballState = BallState.OnTee;
    public static BallState BallState
    {
        set => Instance.ballState = value;
        get => Instance.ballState;
    }

    private int currentPlayerTurn;
    public int CurrentPlayerTurn => currentPlayerTurn;

    public static bool BallShootable => BallState == BallState.OnTee;
    
    private static InGameContext context;
    public static InGameContext Context
    {
        get
        {
            if (context == null)
            {
                context = new InGameContext();
            }
            return context;
        }
    }

    private static ITrajectoryModifier trajectoryModifier;
    public static ITrajectoryModifier TrajectoryModifier
    {
        get
        {
            if (trajectoryModifier == null)
            {
                Debug.Log("Determining trajectory modifier");
                switch (ModeSelector.Instance.CurrentSelectedMode)
                {
                    case GameModeType.Portals:
                        trajectoryModifier = new PortalTrajectoryModifier();
                        break;
                    default:
                        trajectoryModifier = new DefaultTrajectoryModifier();
                        break;
                }
            }

            return trajectoryModifier;
        }
    }
    
    private BallLandingIndicator ballLandingIndicator;
    private BallLandingIndicator BallLandingIndicator
    {
        get
        {
            if (ballLandingIndicator == null)
            {
                GameObject go = Instantiate(GlobalAssets.Instance.ballLandingIndicatorPrefab);
                ballLandingIndicator = go.GetComponent<BallLandingIndicator>();
                ballLandingIndicator.transform.forward = Vector3.up;
                go.SetActive(false);
            }
            return ballLandingIndicator;
        }
    }
    
    public PowerInput PowerInput => ballParameterController.powerInput;
    public SpinInput SpinInput => ballParameterController.spinInput;
    public AngleInput AngleInput => ballParameterController.angleInput;
    public float Gravity => gravity;
    public float LaunchForce => launchForce;
    public Vector2 SpinVector => spinVector;
    public Quaternion LaunchAngle => launchAngle;
    public const int TRAJECTORY_DEFINITION = 100;

    private float gravity;
    private float ballMass;
    private float launchForce;
    private Vector2 spinVector;
    private Vector3 startPosition;
    private Quaternion launchAngle;

    private int numPlayersInGame;
    public int NumPlayersInGame => numPlayersInGame;
    private int volleyNumber = 1;
    public int VolleyNumber => volleyNumber;
    private bool showTrajectory = false;
    private List<int> starsCollected = new List<int>();
    private Coroutine minTimePerShotRoutine;
    private Coroutine optTimePerShotRoutine;
    private Coroutine trajectoryViewRoutine;
    
    private bool showBallLandingOverride = false;
    private bool showBallLanding => ballParameterController.powerInput.Power > 10 && 
        (showBallLandingOverride || (!showTrajectory && Time.time > lastBallLandingShownAt + BALL_LANDING_INDICATOR_INTERVAL));
    private float lastBallLandingShownAt = 0;
    private const float BALL_LANDING_INDICATOR_INTERVAL = 2;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        InitGame();
        InitBall();
        
        //===== Above this might need changing. Was written in the prototyping stage
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextButtonPressed);
        
        fireButton.onClick.RemoveAllListeners();
        fireButton.onClick.AddListener(FireButtonPressed);
        
        LevelLoader.Instance.LoadLevel();
        SetupPlayers();
        ShowLevelInfo();
        currentPlayerTurn = 0;
        RoundDataManager.Instance.SetCurrentShotTaker();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<TrajectoryEnabledEvent>(TrajectoryButtonPressed);
        EventBus.Subscribe<CueNextShotEvent>(CueNextShot);
        EventBus.Subscribe<StoppedBallParameterInput>(StoppedInputtingBallParameter);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<TrajectoryEnabledEvent>(TrajectoryButtonPressed);
        EventBus.Unsubscribe<CueNextShotEvent>(CueNextShot);
        EventBus.Unsubscribe<StoppedBallParameterInput>(StoppedInputtingBallParameter);
    }

    public void InitGame()
    {
        GameStateManager.Instance.SetGameState(GameState.InGame);
        EventBus.Publish(new InGameEvent());
        InputManager.Instance.SetContext(GameContext.InGame);
        gravity = -Physics.gravity.y;
    }

    public void InitBall()
    {
        string primaryID = SaveManager.GetEquippedBall();
        List<IBallAbilityModule> extras = null;

        if (SaveManager.IsFusionEquipped())
        {
            var parts = SaveManager.GetSelectedFusion().Split('+');
            primaryID = parts[0];
            string secID = parts[1];

            var secProps = Balls.Instance.GetBall(secID);
            var secModule = secProps.CreateModuleInstance();
            if (secModule != null)
                extras = new List<IBallAbilityModule> { secModule };
        }

        var props = Balls.Instance.GetBall(primaryID);
        var spawned = Instantiate(props.prefab, tee.ballPosition.position, Quaternion.identity, tee.transform);

        ball = spawned.GetComponent<Ball>();
        startPosition = tee.ballPosition.position;
        ballMass = ball.rb.mass;

        ball.Initialize(Context, TrajectoryModifier, extras);
    }
    
    public void SetupPlayers()
    {
        if (ModeSelector.Instance)
        {
            numPlayersInGame = ModeSelector.Instance.GetNumPlayers();
        }
        else
        {
            numPlayersInGame = 1;
        }
        RoundDataManager.Instance.CreatePlayers(numPlayersInGame);
    }

    public void ShowLevelInfo()
    {
        volleyText.text = "Volley 1";

        switch (GameMode.Instance.GetWinCondition())
        {
            case WinCondition.PointsRequired:
                objectiveText.text = $"Target : {GameMode.Instance.PointsRequired}";
                break;
            case WinCondition.PointsRanking:
                objectiveText.text = $"Maximum total points wins";
                break;
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        // showTrajectory = true;
#endif
        if (BallShootable)
        {
            launchForce = PowerInput.Power;
            launchAngle = AngleInput.cylinderPivot.rotation;
            spinVector = SpinInput.SpinVector;

            if (!trajectoryButtonSection.gameObject.activeSelf && launchForce > 20 && launchAngle.eulerAngles.sqrMagnitude > 1)
            {
                CheckToShowTrajectoryButton();
            }

            if (showTrajectory)
            {
                List<Vector3> trajectoryPoints = ball.CalculateTrajectory();
                List<List<Vector3>> finalizedTrajectory = ball.trajectoryModifier.ModifyTrajectory(trajectoryPoints);
                DrawTrajectory(finalizedTrajectory);
            }
            else if (showBallLanding)
            {
                ShowBallLanding();
            }
        }
    }

    public void NextButtonPressed()
    {
        if (optTimePerShotRoutine != null)
        {
            StopCoroutine(optTimePerShotRoutine);
            ShotComplete();
        }
    }

    public void ShotComplete()
    {
        nextButton.gameObject.SetActive(false);
        EventBus.Publish(new ShotCompleteEvent());
    }
    
    public void CueNextShot(CueNextShotEvent e)
    {
        if (GameMode.Instance.ShouldEndGame())
        {
            GameEnded();
            return;
        }
        
        showTrajectory = false;
        angleIndicator.SetActive(true);
        nextButton.gameObject.SetActive(false);
        fireButton.gameObject.SetActive(true);
        BallState = BallState.OnTee;
        TogglePlayer();
        StartCoroutine(StaggeredCueNextShot());
        CheckToShowTrajectoryHistoryButton();
    }

    private IEnumerator StaggeredCueNextShot()
    {
        EventBus.Publish(new PreNextShotCuedEvent());
        yield return null;
        EventBus.Publish(new NextShotCuedEvent());
    }

    public void TogglePlayer()
    {
        int first = currentPlayerTurn;

        do
        {
            currentPlayerTurn++;
            if (currentPlayerTurn >= numPlayersInGame)
            {
                currentPlayerTurn = 0;
                volleyNumber++;
                EventBus.Publish(new NewRoundStartedEvent(volleyNumber));
                volleyText.text = $"Volley {volleyNumber}";

                if (volleyNumber > GameMode.Instance.NumVolleys)
                {
                    GameEnded();
                    return;
                }
            }

        }
        while (RoundDataManager.Instance.EliminationOrder.Contains(currentPlayerTurn)
               && currentPlayerTurn != first);

        RoundDataManager.Instance.SetCurrentShotTaker();
    }
    
    public void FireButtonPressed()
    {
        if (BallShootable)
        {
            ShootBall();
        }
    }

    public void ShootBall()
    {
        BallLandingIndicator.gameObject.SetActive(false);
        ball.Shoot();
        EventBus.Publish(new BallShotEvent());
        DisableRelevantElementsDuringShot();
        StartMinTimePerShotPeriod();
        RoundDataManager.Instance.StartLoggingShotInfo();
        BallState = BallState.InControlledMotion;
    }

    public void DisableRelevantElementsDuringShot()
    {
        angleIndicator.SetActive(false);
        fireButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        DisableTrajectory();
    }
    
    public void DrawTrajectory(List<List<Vector3>> trajectorySegments)
    {
        for (int i = 0; i < trajectorySegments.Count; i++)
        {
            LineRenderer line = trajectories[i].trajectory;
            line.positionCount = trajectorySegments[i].Count;
            line.SetPositions(trajectorySegments[i].ToArray());
            line.enabled = true;

            trajectories[i].segmentEnd.transform.position = trajectorySegments[i][^1];
            trajectories[i].segmentEnd.enabled = true;
        }

        for (int i = trajectorySegments.Count; i < trajectories.Length; i++)
        {
            trajectories[i].segmentEnd.enabled = false;
            trajectories[i].trajectory.enabled = false;
        }
    }

    public void StoppedInputtingBallParameter(StoppedBallParameterInput e)
    {
        BallLandingIndicator.ResetAnimation();
        showBallLandingOverride = true;
    }
    
    public void ShowBallLanding()
    {
        showBallLandingOverride = false;
        lastBallLandingShownAt = Time.time;
        List<Vector3> trajectory = ball.CalculateTrajectory();

        BallLandingIndicator.gameObject.SetActive(false);

        for (int i = 1; i < trajectory.Count; i++)
        {
            Vector3 prev = trajectory[i - 1];
            Vector3 curr = trajectory[i];

            if (!(prev.y > 0f && curr.y <= 0f))
                continue;

            Vector3 dir = curr - prev;
            float dist = dir.magnitude;
            Ray ray = new Ray(prev, dir.normalized);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, dist, Global.GroundSurface))
            {
                BallLandingIndicator.transform.position = hit.point;
            }
            else
            {
                float t = prev.y / (prev.y - curr.y);
                Vector3 landingPos = Vector3.Lerp(prev, curr, t);
                BallLandingIndicator.transform.position = landingPos;
            }

            // BallLandingIndicator.transform.position = new Vector3(BallLandingIndicator.transform.position.x, 
            //     BallLandingIndicator.transform.position.y + 0.02f, BallLandingIndicator.transform.position.z);
            BallLandingIndicator.gameObject.SetActive(true);
            BallLandingIndicator.Animate();
            return;
        }
    }
    
    public void StartMinTimePerShotPeriod()
    {
        IEnumerator<WaitForSeconds> MinTimeRoutine()
        {
            yield return new WaitForSeconds(GameMode.Instance.MinTimePerShot);
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
            float optionalTime = GameMode.Instance.GetOptionalTimePerShot();
            while (timePassed <= optionalTime)
            {
                timePassed += Time.deltaTime;
                nextButtonFill.fillAmount = timePassed / optionalTime;
                yield return null;
            }
            optTimePerShotRoutine = null;
            ShotComplete();
        }

        if (optTimePerShotRoutine != null)
        {
            StopCoroutine(optTimePerShotRoutine);
        }

        optTimePerShotRoutine = StartCoroutine(OptionalTimeRoutine());
    }
    
    public void CheckToShowTrajectoryButton()
    {
        int trajectoryViewsRemaining = RoundDataManager.Instance.GetTrajectoryViewsRemaining();
        if (trajectoryViewsRemaining > 0)
        {
            trajectoryButtonSection.gameObject.SetActive(true);
            trajectoryButton.ShowButton(trajectoryViewsRemaining);
        }
    }

    public void TrajectoryButtonPressed(TrajectoryEnabledEvent e)
    {
        if (RoundDataManager.Instance.GetTrajectoryViewsRemaining() <= 0)
        {
            return;
        }
        
        RoundDataManager.Instance.TrajectoryViewUsed();
        DiscardTrajectoryViewRoutine();
        trajectoryViewRoutine = StartCoroutine(ShowTrajectory());
    }

    public IEnumerator ShowTrajectory()
    {
        showTrajectory = true;
        int secondsRemaining = GameMode.Instance.ProjectileViewDuration;
        trajectoryButton.SetCountdownText(secondsRemaining);
        while (secondsRemaining >= 0)
        {
            secondsRemaining--;
            yield return new WaitForSeconds(1);
            trajectoryButton.SetCountdownText(secondsRemaining);
        }

        showTrajectory = false;
        DisableTrajectory();
        CheckToShowTrajectoryButton();
    }

    public void DiscardTrajectoryViewRoutine()
    {
        if (trajectoryViewRoutine != null)
        {
            StopCoroutine(trajectoryViewRoutine);
        }
    }

    public void DisableTrajectory()
    {
        DiscardTrajectoryViewRoutine();
        foreach (var trajectory in trajectories)
        {
            trajectory.segmentEnd.enabled = false;
            trajectory.trajectory.enabled = false;
        }
        trajectoryButtonSection.gameObject.SetActive(false);
    }

    public void CheckToShowTrajectoryHistoryButton()
    {
        trajectoryHistoryButton.gameObject.SetActive(TrajectoryHistoryViewer.Instance.GetIsTrajectoryHistoryAvailable());
    }

    public void StarCollected(int index)
    {
        starsCollected.Add(index);
    }
    
    public void GameEnded()
    {
        EventBus.Publish(new GameEndedEvent());
        CameraController.Instance.ResetCamera();
        PostGameStuff();
        SetupResults();
    }

    public void PostGameStuff()
    {
        bool levelCleared = GameMode.Instance.GetWinCondition() == WinCondition.PointsRequired &&
                            RoundDataManager.Instance.GetPointsForPlayer(0) >= GameMode.Instance.PointsRequired;
        if (levelCleared)
        {
            foreach (int starIndex in starsCollected)
            {
                SaveManager.SetStarCollected((int)ModeSelector.Instance.GetSelectedGameMode(), ModeSelector.Instance.GetSelectedLevel(), starIndex);
            }
            SaveManager.AddStars(starsCollected.Count);
        }
    }

    public void SetupResults()
    {
        GameStateManager.Instance.SetGameState(GameState.OnResultScreen);
        resultScreen.gameObject.SetActive(true);
        ResultsScreen.Instance.SetupResults();
    }
    
    private static void DiscardGameContext()
    {
        context = null;
        trajectoryModifier = null;
    }

    public void DestroyRemnants()
    {
        if (ball)
            Destroy(ball.gameObject);
    }
    
    private void OnDestroy()
    {
        DestroyRemnants();
        DiscardGameContext();
    }
}