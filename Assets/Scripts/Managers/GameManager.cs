using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

public class BallShotEvent { }
public class CueNextShotEvent { }
public class PreNextShotCuedEvent { }
public class InGameEvent { }
public class GameEndedEvent { }
public class GameExitedEvent { }
public class TrajectoryEnabledEvent { }
public class LevelSetupCompleteEvent { }

public class ShotCompleteEvent
{
    public List<Vector3> ShotTrajectory;
    public ShotCompleteEvent(List<Vector3> capturedTrajectory)
    {
        ShotTrajectory = capturedTrajectory;
    }
}

public class ProjectilesSpawnedEvent
{
    public GameObject[] projectiles;
    public ProjectilesSpawnedEvent(GameObject[] _projectiles)
    {
        projectiles = _projectiles;
    }
}

public class NewGameStartedEvent
{
    public int NumPlayers;

    public NewGameStartedEvent(int _numPlayers)
    {
        NumPlayers = _numPlayers;
    }
}

public class NextShotCuedEvent
{
    public int CurrentPlayerTurn;
    public NextShotCuedEvent()
    {
        CurrentPlayerTurn = 0;
    }
    public NextShotCuedEvent(int _currentPlayerTurn)
    {
        CurrentPlayerTurn = _currentPlayerTurn;
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

public class GameManager : MonoBehaviour, IInitializable, IDisposable
{
    [HideInInspector]
    [Inject]
    public SceneContext sceneContext;
    
    public bool IsGameplayActive { get; private set; }
    
    [Header("Level Setup")]
    public LevelLoader levelLoader;
    public TrajectoryHistoryViewer trajectoryHistoryViewer;
    
    [Header("Level Objects")]
    public Tee tee;
    [HideInInspector]
    public Ball ball;

    [Header("Controls Panel")]
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
    public ResultsScreen resultScreen;
    
    private static BallState ballState = BallState.OnTee;
    public static BallState BallState
    {
        set => ballState = value;
        get => ballState;
    }
    public static bool BallShootable => BallState == BallState.OnTee;

    private int currentPlayerTurn;
    public int CurrentPlayerTurn => currentPlayerTurn;
    
    private ITrajectoryModifier trajectoryModifier;
    public ITrajectoryModifier TrajectoryModifier
    {
        get
        {
            if (trajectoryModifier == null)
            {
                Debug.Log("Determining trajectory modifier");
                switch (modeSelector.CurrentSelectedMode)
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

    [InjectOptional]
    private ModeSelector modeSelector;
    [InjectOptional]
    private GameStateManager gameStateManager;
    [InjectOptional]
    private InputManager inputManager;
    [InjectOptional]
    private CameraController cameraController;
    [InjectOptional]
    private RoundDataManager roundDataManager;
    [Inject]
    private GameMode gameMode;
    [InjectOptional]
    private InGameContext context;
    public InGameContext Context => context;
    
    public void Initialize()
    {
        IsGameplayActive = true;
    }

    public void Dispose()
    {
        IsGameplayActive = false;
    }
    
    private void Start()
    {
        InitGame();
        InitBall();
        
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(NextButtonPressed);
        
        fireButton.onClick.RemoveAllListeners();
        fireButton.onClick.AddListener(FireButtonPressed);
        
        SetupPlayers();
        levelLoader.LoadLevel();
        ShowLevelInfo();
        currentPlayerTurn = 0;
        roundDataManager.SetCurrentShotTaker();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<TrajectoryEnabledEvent>(TrajectoryButtonPressed);
        EventBus.Subscribe<CueNextShotEvent>(CueNextShot);
        EventBus.Subscribe<StoppedBallParameterInput>(StoppedInputtingBallParameter);
        EventBus.Subscribe<LevelSetupCompleteEvent>(LevelSetupComplete);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<TrajectoryEnabledEvent>(TrajectoryButtonPressed);
        EventBus.Unsubscribe<CueNextShotEvent>(CueNextShot);
        EventBus.Unsubscribe<StoppedBallParameterInput>(StoppedInputtingBallParameter);
        EventBus.Unsubscribe<LevelSetupCompleteEvent>(LevelSetupComplete);
    }

    private void LevelSetupComplete(LevelSetupCompleteEvent e)
    {
        EventBus.Publish(new NewGameStartedEvent(NumPlayersInGame));
    }

    public void InitGame()
    {
        ballState = BallState.OnTee;
        gameStateManager.SetGameState(GameState.InGame);
        EventBus.Publish(new InGameEvent());
        inputManager.SetContext(GameContext.InGame);
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
        sceneContext.Container.InjectGameObject(spawned);

        ball = spawned.GetComponent<Ball>();
        startPosition = tee.ballPosition.position;
        ballMass = ball.rb.mass;

        ball.Initialize(Context, TrajectoryModifier, extras);
    }
    
    public void SetupPlayers()
    {
        numPlayersInGame = modeSelector.GetNumPlayers();
        roundDataManager.CreatePlayers(numPlayersInGame);
    }

    public void ShowLevelInfo()
    {
        volleyText.text = "Volley 1";

        switch (gameMode.GetWinCondition())
        {
            case WinCondition.PointsRequired:
                objectiveText.text = $"Target : {gameMode.PointsRequired}";
                break;
            case WinCondition.PointsRanking:
                objectiveText.text = $"Maximum total points wins";
                break;
            case WinCondition.Survival:
                objectiveText.text = $"Survive All Volleys";
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
        EventBus.Publish(new ShotCompleteEvent(ball.CapturedTrajectoryPoints));
    }
    
    public void CueNextShot(CueNextShotEvent e)
    {
        if (gameMode.ShouldEndGame())
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
        EventBus.Publish(new NextShotCuedEvent(CurrentPlayerTurn));
    }

    public void TogglePlayer()
    {
        int lastTurn = currentPlayerTurn;

        do
        {
            currentPlayerTurn++;
            if (currentPlayerTurn >= numPlayersInGame)
            {
                currentPlayerTurn = 0;
                volleyNumber++;
                EventBus.Publish(new NewRoundStartedEvent(volleyNumber));
                volleyText.text = $"Volley {volleyNumber}";

                if (volleyNumber > gameMode.NumVolleys)
                {
                    GameEnded();
                    return;
                }
            }

        }while (roundDataManager.EliminationOrder.Contains(currentPlayerTurn) && currentPlayerTurn != lastTurn);

        roundDataManager.SetCurrentShotTaker();
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
        roundDataManager.StartLoggingShotInfo();
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
            yield return new WaitForSeconds(gameMode.MinTimePerShot);
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
            float optionalTime = gameMode.GetOptionalTimePerShot();
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
        int trajectoryViewsRemaining = roundDataManager.GetTrajectoryViewsRemaining();
        if (trajectoryViewsRemaining > 0)
        {
            trajectoryButtonSection.gameObject.SetActive(true);
            trajectoryButton.ShowButton(trajectoryViewsRemaining);
        }
    }

    public void TrajectoryButtonPressed(TrajectoryEnabledEvent e)
    {
        if (roundDataManager.GetTrajectoryViewsRemaining() <= 0)
        {
            return;
        }
        
        roundDataManager.TrajectoryViewUsed();
        DiscardTrajectoryViewRoutine();
        trajectoryViewRoutine = StartCoroutine(ShowTrajectory());
    }

    public IEnumerator ShowTrajectory()
    {
        showTrajectory = true;
        int secondsRemaining = gameMode.ProjectileViewDuration;
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
        trajectoryHistoryButton.gameObject.SetActive(trajectoryHistoryViewer.GetIsTrajectoryHistoryAvailable());
    }

    public void StarCollected(int index)
    {
        starsCollected.Add(index);
    }
    
    public void GameEnded()
    {
        EventBus.Publish(new GameEndedEvent());
        cameraController.ResetCamera();
        PostGameStuff();
        SetupResults();
    }

    public void PostGameStuff()
    {
        bool levelCleared = modeSelector.IsPlayingSolo && gameMode.LevelCompletedSuccessfully();
        
        if (levelCleared)
        {
            SaveManager.SetLevelCompleted(modeSelector.GetSelectedGameMode(), modeSelector.GetSelectedLevel());
            foreach (int starIndex in starsCollected)
            {
                SaveManager.SetStarCollected((int)modeSelector.GetSelectedGameMode(), modeSelector.GetSelectedLevel(), starIndex);
            }
            SaveManager.AddStars(starsCollected.Count);
        }

        if (modeSelector.IsGauntletMode())
        {
            int difficulty = modeSelector.GetEndlessModeDifficulty();
            if (levelCleared)
                SaveManager.RecordEndlessWin(difficulty);
            else
                SaveManager.RecordEndlessLoss(difficulty);
        }
    }

    public void SetupResults()
    {
        if (modeSelector.IsPlayingSolo)
        {
            resultScreen.SetupResult(gameMode.LevelCompletedSuccessfully());
        }
        else
        {
            resultScreen.SetupResult(roundDataManager.GetPlayerRankings());
        }
        
        gameStateManager.SetGameState(GameState.OnResultScreen);
        resultScreen.gameObject.SetActive(true);
    }
    
    private void DiscardGameContext()
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