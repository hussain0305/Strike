using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BallShotEvent { }
public class NextShotCuedEvent { }
public class InGameEvent { }
public class GameEndedEvent { }
public class GameExitedEvent { }

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

    [Header("Controls Panel")]
    public PowerInput powerInput;
    public SpinInput spinInput;
    public AngleInput angleInput;
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

    public PinBehaviourPerTurn pinBehaviour = PinBehaviourPerTurn.Reset;
    
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
    private int volleyNumber = 1;
    private bool showTrajectory = false;
    private List<int> starsCollected = new List<int>();
    private Coroutine minTimePerShotRoutine;
    private Coroutine optTimePerShotRoutine;
    private Coroutine trajectoryViewRoutine;
    
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

    public void InitGame()
    {
        GameStateManager.Instance.SetGameState(GameState.InGame);
        EventBus.Publish(new InGameEvent());
        InputManager.Instance.SetContext(GameContext.InGame);
        gravity = -Physics.gravity.y;
    }

    public void InitBall()
    {
        BallProperties selectionBall = Balls.Instance.GetBall(SaveManager.GetEquippedBall());
        GameObject spawnedBall = Instantiate(selectionBall.prefab, tee.ballPosition.position, Quaternion.identity, tee.transform);
        ball = spawnedBall.GetComponent<Ball>();
        startPosition = tee.ballPosition.position;
        ballMass = ball.rb.mass;
        ball.Initialize(Context, TrajectoryModifier);
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
                objectiveText.text = $"Target : {GameMode.Instance.pointsRequired}";
                break;
            case WinCondition.PointsRanking:
                objectiveText.text = $"Maximum total points wins";
                break;
        }
    }

    void Update()
    {
        showTrajectory = true;
        if (BallShootable)
        {
            launchForce = powerInput.Power;
            launchAngle = angleInput.cylinderPivot.rotation;
            spinVector = spinInput.SpinVector;

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
        }
    }

    public void NextButtonPressed()
    {
        if (optTimePerShotRoutine != null)
        {
            StopCoroutine(optTimePerShotRoutine);
            CueNextShot();
        }
    }
    
    public void CueNextShot()
    {
        angleIndicator.SetActive(true);
        nextButton.gameObject.SetActive(false);
        fireButton.gameObject.SetActive(true);
        powerInput.powerSlider.value = 0;
        spinInput.ResetPointer();
        angleInput.ResetPointer();
        GameManager.BallState = BallState.OnTee;
        TogglePlayer();
        EventBus.Publish(new NextShotCuedEvent());
        CheckToShowTrajectoryHistoryButton();
    }

    public void TogglePlayer()
    {
        currentPlayerTurn++;
        if (currentPlayerTurn >= numPlayersInGame)
        {
            currentPlayerTurn = 0;
            volleyNumber++;
            volleyText.text = $"Volley {volleyNumber}";

            if (volleyNumber > GameMode.Instance.numVolleys)
            {
                GameEnded();
            }
        }
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
    
    public void StartMinTimePerShotPeriod()
    {
        IEnumerator<WaitForSeconds> MinTimeRoutine()
        {
            yield return new WaitForSeconds(GameMode.Instance.minTimePerShot);
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
            CueNextShot();
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

    public void TrajectoryButtonPressed()
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
        int secondsRemaining = GameMode.Instance.projectileViewDuration;
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

    public void GameExitedPrematurely()
    {
        EventBus.Publish(new GameExitedEvent());
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
                            RoundDataManager.Instance.GetPointsForPlayer(0) >= GameMode.Instance.pointsRequired;
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
    
    private void OnDestroy()
    {
        DiscardGameContext();
    }
}