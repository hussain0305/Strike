using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    public static bool IsGameplayActive => Instance != null;
    
    public delegate void BallShot();
    public static event BallShot OnBallShot;
    
    public delegate void NextShotCued();
    public static event NextShotCued OnNextShotCued;

    public delegate void InGame();
    public static event InGame OnGotInGame;

    public delegate void GameEndedEvent();
    public static event GameEndedEvent OnGameEnded;
    public delegate void GameExitedEvent();
    public static event GameExitedEvent OnGameExitedPrematurely;

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
    public LineRenderer trajectory;
    
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

    private float gravity;
    private float ballMass;
    private float launchForce;
    private Vector2 spinVector;
    private Vector3 startPosition;
    private Quaternion launchAngle;
    private const int TRAJECTORY_DEFINITION = 100;

    private int numPlayersInGame;
    private int volleyNumber = 1;
    private bool showTrajectory = false;
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
        gravity = -Physics.gravity.y;
        
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
        GameStateManager.Instance.SetGameState(GameStateManager.GameState.InGame);
        OnGotInGame?.Invoke();
        InputManager.Instance.SetContext(GameContext.InGame);
    }

    public void InitBall()
    {
        BallProperties selectionBall = Balls.Instance.GetBall(SaveManager.GetSelectedBall());
        GameObject spawnedBall = Instantiate(selectionBall.prefab, tee.ballPosition.position, Quaternion.identity, tee.transform);
        ball = spawnedBall.GetComponent<Ball>();
        startPosition = tee.ballPosition.position;
        ballMass = ball.rb.mass;
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
                List<Vector3> trajectoryPoints = CalculateTrajectoryPoints();
                DrawTrajectory(trajectoryPoints.ToArray());
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
        ball.ResetBall();
        powerInput.powerSlider.value = 0;
        spinInput.ResetPointer();
        angleInput.ResetPointer();
        GameManager.BallState = BallState.OnTee;
        TogglePlayer();
        OnNextShotCued?.Invoke();
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
        OnBallShot?.Invoke();
        DisableRelevantElementsDuringShot();
        StartMinTimePerShotPeriod();
    }

    public void DisableRelevantElementsDuringShot()
    {
        angleIndicator.SetActive(false);
        fireButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        DisableTrajectory();
    }
    
    public List<Vector3> CalculateTrajectoryPoints()
    {
        List<Vector3> trajectoryPoints = new List<Vector3>();
        float timeStep = 0.1f;

        float launchVelocity = launchForce / ballMass;
        Vector3 initialVelocity = launchAngle * Vector3.forward * launchVelocity;

        Vector2 spin = spinInput.SpinVector;
        float sideSpin = spin.x;
        float topSpin = spin.y;

        float curveScale = Mathf.Clamp(ball.spinEffect * Mathf.Abs(sideSpin), 0, ball.curveClamp);
        float dipScale = -Mathf.Clamp(ball.spinEffect * Mathf.Abs(topSpin), 0, ball.dipClamp);

        float curveDuration = 2.0f;

        bool isCurving = true;
        Vector3 previousPoint;
        Vector3 currentPoint = Vector3.zero;
        Vector3 lastVelocity;
        float x = 0, y = 0, z = 0;

        for (int i = 0; i < TRAJECTORY_DEFINITION; i++)
        {
            float t = i * timeStep;
            previousPoint = currentPoint;
            currentPoint = new Vector3(x, y, z);
            lastVelocity = (currentPoint - previousPoint) / timeStep;

            if (isCurving)
            {
                x = startPosition.x + initialVelocity.x * t;
                z = startPosition.z + initialVelocity.z * t;
                y = startPosition.y + initialVelocity.y * t - 0.5f * gravity * t * t;

                float curveFactor = Mathf.Sin(t * Mathf.PI / curveDuration) * curveScale * Mathf.Sign(sideSpin);
                float dipFactor = Mathf.Sin(t * Mathf.PI / curveDuration) * dipScale * Mathf.Sign(topSpin);

                x += curveFactor;
                y += dipFactor;

                if (t >= curveDuration)
                {
                    isCurving = false;
                }
            }
            else
            {
                x = currentPoint.x + lastVelocity.x * timeStep;
                z = currentPoint.z + lastVelocity.z * timeStep;
                y = currentPoint.y + lastVelocity.y * timeStep - 0.5f * gravity * timeStep * timeStep;
            }

            trajectoryPoints.Add(new Vector3(x, y, z));

            if (y < 0)
            {
                break;
            }
        }

        return trajectoryPoints;
    }
    
    private void DrawTrajectory(Vector3[] trajectoryPoints)
    {
        trajectory.positionCount = trajectoryPoints.Length;
        trajectory.SetPositions(trajectoryPoints);

        trajectory.startWidth = 0.25f;
        trajectory.endWidth = 0.25f;
        // trajectory.material = new Material(Shader.Find("Sprites/Default")); // Basic material
        trajectory.startColor = Color.green;
        trajectory.endColor = Color.red;
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
        trajectory.gameObject.SetActive(true);
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
        trajectory.gameObject.SetActive(false);
        trajectoryButtonSection.gameObject.SetActive(false);
    }

    public void CheckToShowTrajectoryHistoryButton()
    {
        trajectoryHistoryButton.gameObject.SetActive(TrajectoryHistoryViewer.Instance.GetIsTrajectoryHistoryAvailable());
    }

    public void GameExitedPrematurely()
    {
        OnGameExitedPrematurely?.Invoke();
    }
    
    public void GameEnded()
    {
        OnGameEnded?.Invoke();
        CameraController.Instance.ResetCamera();
        SetupResults();
    }

    public void SetupResults()
    {
        GameStateManager.Instance.SetGameState(GameStateManager.GameState.OnResultScreen);
        resultScreen.gameObject.SetActive(true);
        ResultsScreen.Instance.SetupResults();
    }
}