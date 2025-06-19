using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

[System.Serializable]
public class TrajectorySegmentVisuals
{
    public LineRenderer trajectory;
    public MeshRenderer segmentStart;
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
    public TrajectorySegmentVisuals[] trajectories;
    public GameObject angleIndicator;
    public Tee tee;
    
    [HideInInspector]
    public Ball ball;
    
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
    
    public PowerInput PowerInput => shotInput.powerInput;
    public SpinInput SpinInput => shotInput.spinInput;
    public AngleInput AngleInput => shotInput.angleInput;
    public float Gravity => gravity;
    public float LaunchForce => PowerInput.Power;
    public Vector2 SpinVector => SpinInput.SpinVector;
    public Quaternion LaunchAngle => AngleInput.cylinderPivot.rotation;
    public const int TRAJECTORY_DEFINITION = 100;

    private float gravity;

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
    private bool showBallLanding => shotInput.powerInput.Power > 10 && 
        (showBallLandingOverride || (!showTrajectory && Time.time > lastBallLandingShownAt + BALL_LANDING_INDICATOR_INTERVAL));
    private float lastBallLandingShownAt = 0;
    private const float BALL_LANDING_INDICATOR_INTERVAL = 2;
    
    public AdditionalVolleyOffer additionalVolleyOffer;
    private int additionalVolleys;
    private int numTimesAdditionalVolleysGranted;
    public int TotalVolleysAvailable => additionalVolleys + gameMode.NumVolleys;
    private Coroutine cueNextShotRoutine;

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
    [Inject]
    private ShotInput shotInput;
    [Inject]
    private IHUD playerHUD;
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
        
        SetupPlayers();
        levelLoader.LoadLevel();
        playerHUD.ShowLevelInfo();
        currentPlayerTurn = 0;
        roundDataManager.SetCurrentShotTaker();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<HUDAction_CheckTrajectory>(TrajectoryButtonPressed);
        EventBus.Subscribe<HUDAction_Fire>(FireButtonPressed);
        EventBus.Subscribe<HUDAction_Next>(NextButtonPressed);
        
        EventBus.Subscribe<CueNextShotEvent>(CueNextShot);
        EventBus.Subscribe<StoppedShotInput>(StoppedInputtingShot);
        EventBus.Subscribe<GameRestartedEvent>(GameRestarted);
        EventBus.Subscribe<TrajectoryAdAttemptedEvent>(TryWatchingTrajectoryAd);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<HUDAction_CheckTrajectory>(TrajectoryButtonPressed);
        EventBus.Unsubscribe<HUDAction_Fire>(FireButtonPressed);
        EventBus.Unsubscribe<HUDAction_Next>(NextButtonPressed);
        
        EventBus.Unsubscribe<CueNextShotEvent>(CueNextShot);
        EventBus.Unsubscribe<StoppedShotInput>(StoppedInputtingShot);
        EventBus.Unsubscribe<GameRestartedEvent>(GameRestarted);
        EventBus.Unsubscribe<TrajectoryAdAttemptedEvent>(TryWatchingTrajectoryAd);
    }

    public void LevelSetupComplete()
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

        ball.Initialize(Context, TrajectoryModifier, props, extras);
    }
    
    public void SetupPlayers()
    {
        numPlayersInGame = modeSelector.GetNumPlayers();
        roundDataManager.CreatePlayers(numPlayersInGame);
    }


    void Update()
    {
#if UNITY_EDITOR
        // showTrajectory = true;
#endif
        if (BallShootable)
        {
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

    public void NextButtonPressed(HUDAction_Next e)
    {
        if (optTimePerShotRoutine != null)
        {
            StopCoroutine(optTimePerShotRoutine);
            ShotComplete();
        }
    }

    public void ShotComplete()
    {
        EventBus.Publish(new ShotCompleteEvent(ball.CapturedTrajectoryPoints));
    }
    
    public void CueNextShot(CueNextShotEvent e)
    {
        if (gameMode.ShouldEndGame())
        {
            GameEnded();
            return;
        }

        StartCoroutine(CueNextShotRoutine());
    }

    private IEnumerator CueNextShotRoutine()
    {
        showTrajectory = false;
        
        yield return CheckIfNextShotAvailable();
        
        angleIndicator.SetActive(true);
        BallState = BallState.OnTee;
        
        TogglePlayer();
        
        yield return StaggeredCueNextShot();
    }

    private IEnumerator CheckIfNextShotAvailable()
    {
        if(!CanGrantAdditionalVolleys())
            yield break;

        int pointsRemaining = gameMode.PointsRequired - roundDataManager.GetPointsForPlayer(0);
        
        if (volleyNumber >= TotalVolleysAvailable && pointsRemaining > 0)
        {
            string messageString = $"YOU ARE {pointsRemaining} POINTS SHORT. WATCH AN AD FOR {gameMode.NumAdditionalVolleysPerGrant} ADDITIONAL VOLLEYS?";
            additionalVolleyOffer.Show(messageString, (offerTaken) => { });
                    
            yield return new WaitWhile(() => additionalVolleyOffer.offerActive);

            if (additionalVolleyOffer.offerTaken)
            {
                GrantAdditionalVolleys();
            }
        }
    }
    
    public bool CanGrantAdditionalVolleys()
    {
        return modeSelector.IsPlayingSolo && numTimesAdditionalVolleysGranted < gameMode.NumAdditionalVolleyGrants;
    }

    public void GrantAdditionalVolleys()
    {
        additionalVolleys += gameMode.NumAdditionalVolleysPerGrant;
        numTimesAdditionalVolleysGranted++;
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

                if (volleyNumber > TotalVolleysAvailable)
                {
                    GameEnded();
                    return;
                }
            }

        }while (roundDataManager.EliminationOrder.Contains(currentPlayerTurn) && currentPlayerTurn != lastTurn);

        roundDataManager.SetCurrentShotTaker();
    }
    
    public void FireButtonPressed(HUDAction_Fire e)
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
        DisableTrajectory();
    }
    
    public void DrawTrajectory(List<List<Vector3>> trajectorySegments)
    {
        trajectorySegments = TrajectoryPruner.PruneByCollision(trajectorySegments);
        for (int i = 0; i < trajectorySegments.Count; i++)
        {
            LineRenderer line = trajectories[i].trajectory;
            line.positionCount = trajectorySegments[i].Count;
            line.SetPositions(trajectorySegments[i].ToArray());
            line.enabled = true;

            trajectories[i].segmentStart.transform.position = trajectorySegments[i][0];
            trajectories[i].segmentStart.enabled = true;
            trajectories[i].segmentEnd.transform.position = trajectorySegments[i][^1];
            trajectories[i].segmentEnd.enabled = true;
        }

        for (int i = trajectorySegments.Count; i < trajectories.Length; i++)
        {
            trajectories[i].segmentStart.enabled = false;
            trajectories[i].segmentEnd.enabled = false;
            trajectories[i].trajectory.enabled = false;
        }
    }

    public void StoppedInputtingShot(StoppedShotInput e)
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
            playerHUD.SetNextButtonActive(true);
            float timePassed = 0;
            float optionalTime = gameMode.GetOptionalTimePerShot();
            while (timePassed <= optionalTime)
            {
                timePassed += Time.deltaTime;
                playerHUD.UpdateOptionalShotPhaseProgress(timePassed / optionalTime);
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
    
    public void TrajectoryButtonPressed(HUDAction_CheckTrajectory e)
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
        playerHUD.UpdateTrajectoryViewTimeRemaining(secondsRemaining);
        while (secondsRemaining >= 0)
        {
            secondsRemaining--;
            yield return new WaitForSeconds(1);
            playerHUD.UpdateTrajectoryViewTimeRemaining(secondsRemaining);
        }

        showTrajectory = false;
        DisableTrajectory();
        playerHUD.CheckToShowTrajectoryButton();
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
            trajectory.segmentStart.enabled = false;
            trajectory.segmentEnd.enabled = false;
            trajectory.trajectory.enabled = false;
        }
        
        playerHUD.SetTrajectoryButtonSectionActive(false);
    }
    
    public void TryWatchingTrajectoryAd(TrajectoryAdAttemptedEvent e)
    {
        if (roundDataManager.GetTrajectoryViewsRemaining() != 0)
        {
            return;
        }
        
        playerHUD.SetTrajectoryAdButtonSectionActive(false);
        
        AdManager.ShowRewardedAd((success) =>
            {
                if (success)
                {
                    roundDataManager.AddTrajectoryView();
                }
                playerHUD.CheckToShowTrajectoryButton();
            });
    }


    public void StarCollected(int index)
    {
        starsCollected.Add(index);
    }

    public void GameRestarted(GameRestartedEvent e)
    {
        PostGameStuff();
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
            SaveManager.SetLevelCompleted(modeSelector.GetSelectedGameMode(), modeSelector.GetSelectedLevel(), false);
            foreach (int starIndex in starsCollected)
            {
                SaveManager.SetStarCollected((int)modeSelector.GetSelectedGameMode(), modeSelector.GetSelectedLevel(), starIndex, false);
            }
            SaveManager.AddStars(starsCollected.Count);
        }

        if (modeSelector.IsPlayingEndlessMode())
        {
            int difficulty = modeSelector.GetEndlessModeDifficulty();
            if (levelCleared)
                SaveManager.RecordEndlessWin(difficulty);
            else
                SaveManager.RecordEndlessLoss(difficulty);
        }

        EventBus.Publish(new ResultDeterminedEvent(modeSelector.IsPlayingSolo, levelCleared));
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