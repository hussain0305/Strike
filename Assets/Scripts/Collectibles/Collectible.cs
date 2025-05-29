using TMPro;
using UnityEngine;
using Zenject;

public class CollectibleHitEvent
{
    public CollectibleType Type { get; }
    public int Value { get; }
    public Vector3 HitPosition { get; }

    public CollectibleHitEvent(CollectibleType type, int value, Vector3 hitPosition)
    {
        Type = type;
        Value = value;
        HitPosition = hitPosition;
    }
}

public class Collectible : MonoBehaviour, ICollectible
{
    public enum PointDisplayType
    {
        None,
        FloatingBoard,
        InBody
    };
    
    [Header("Info")]
    public CollectibleType type;
    public int value;
    public int numTimesCanBeCollected = 1;
    public bool activeOnStart = true;

    [Header("Header")]
    public float heightMultipleForOffsetCalculation = 1;
    public Transform body;
    public TextMeshPro inBodyPointDisplay;

    [Header("Feedback and Visual Indicators")]
    public PointDisplayType pointDisplay;

    public bool SetupFloatingBoard => pointDisplay == PointDisplayType.FloatingBoard;
    public bool SetupInBodyBoard => pointDisplay == PointDisplayType.InBody;
    public bool HasPointBoard => pointDisplay != PointDisplayType.None;

    public Material RegularFontColor => type == CollectibleType.Danger
        ? GlobalAssets.Instance.dangerCollectibleTextMaterial
        : value >= 0
        ? GlobalAssets.Instance.positiveCollectibleTextMaterial
        : GlobalAssets.Instance.negativeCollectibleTextMaterial;
    
    public Material HitFontColor => type == CollectibleType.Danger
        ? GlobalAssets.Instance.dangerCollectibleHitTextMaterial
        : value >= 0
        ? GlobalAssets.Instance.positiveCollectibleHitTextMaterial
        : GlobalAssets.Instance.negativeCollectibleHitTextMaterial;
    
    private CollectibleHeader header;
    private ContinuousMovement continuousMovement;

    private Rigidbody rBody;
    public Rigidbody RBody => rBody ??= GetComponent<Rigidbody>();
    
    private int collectingLayer;
    private int numTimesCollected = 0;
    private bool accountedForInThisShot = false;
    
    private int numPlayers = -1;
    private int currentPlayer = -1;
    private bool[] stateForPlayers;
    private bool isPlayingSolo = true;
    private bool defaultIsKinematic;
    private bool IsDisappearingMode => context?.GetPinResetBehaviour() == PinBehaviourPerTurn.DisappearUponCollection;
    
    private Quaternion defaultRotation;
    private Vector3 defaultPosition;
    private Vector3 defaultLocalScale = Vector3.one;
    public  Vector3 DefaultLocalScale => defaultLocalScale;

    private ICollectibleHitReaction hitReaction;
    private IContextProvider context;
    
    private GameStateManager gameStateManager;
    private RoundDataManager roundDataManager;
    
    [Inject]
    public void Construct(GameStateManager _gameStateManager, RoundDataManager _roundDataManager)
    {
        gameStateManager = _gameStateManager;
        roundDataManager = _roundDataManager;
    }

    public void Awake()
    {
        SaveDefaults();
        hitReaction = GetComponent<ICollectibleHitReaction>();
    }

    public void Start()
    {
        collectingLayer = LayerMask.GetMask("Ball", "OtherCollectingObject");
    }

    public void OnEnable()
    {
        EventBus.Subscribe<BallShotEvent>(BallShot);
        EventBus.Subscribe<NextShotCuedEvent>(NextShotCued);
        EventBus.Subscribe<NewGameStartedEvent>(NewGameStarted);
    }

    public void OnDisable()
    {
        EventBus.Unsubscribe<BallShotEvent>(BallShot);
        EventBus.Unsubscribe<NextShotCuedEvent>(NextShotCued);
        EventBus.Unsubscribe<NewGameStartedEvent>(NewGameStarted);
    }

    public void Initialize(IContextProvider _contextProvider)
    {
        context = _contextProvider;
    }

    public void InitializeAndSetup(IContextProvider _contextProvider, int _value, int _numTimesCanBeCollected, PointDisplayType _displayType)
    {
        value = _value;
        numTimesCanBeCollected = _numTimesCanBeCollected;
        pointDisplay = _displayType;
        context = _contextProvider;
        
        numTimesCollected = 0;
        accountedForInThisShot = false;
        hitReaction = GetComponent<ICollectibleHitReaction>();

        SaveDefaults();
        InitAppearance();
        
        defaultIsKinematic = RBody.isKinematic;
        continuousMovement = GetComponent<ContinuousMovement>();
    }

    public void SaveDefaults()
    {
        defaultPosition = transform.position;
        defaultRotation = transform.rotation;
    }

    public void InitAppearance()
    {
        if (HasPointBoard)
            SetupPointBoard();
        
        hitReaction?.SetDefaultVisuals(null);
    }

    public void NewGameStarted(NewGameStartedEvent e)
    {
        numPlayers = e.NumPlayers;
        stateForPlayers = new bool[numPlayers];
        isPlayingSolo = numPlayers == 1;
        currentPlayer = 0;
    }

    public void OnCollisionEnter(Collision other)
    {
        ProcessHit(other.gameObject, other.transform.position);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        ProcessHit(other.gameObject, other.transform.position);
    }

    private void ProcessHit(GameObject collidingObject, Vector3 hitPosition)
    {
        if (collidingObject != null && (collectingLayer & (1 << collidingObject.layer)) == 0)
            return;

        if (numTimesCollected >= numTimesCanBeCollected || accountedForInThisShot)
            return;

        numTimesCollected++;
        accountedForInThisShot = true;
        
        stateForPlayers[currentPlayer] = true;
        
        EventBus.Publish(new CollectibleHitEvent(type, value, hitPosition));
        header?.gameObject.SetActive(false);
        hitReaction?.CheckIfHitsExhasuted(numTimesCollected, numTimesCanBeCollected);
    }

    public void BallShot(BallShotEvent e)
    {
        
    }

    public void NextShotCued(NextShotCuedEvent e)
    {
        currentPlayer = e.CurrentPlayerTurn;
        header?.gameObject.SetActive(true);

        if (context == null)
            return;

        switch (context?.GetPinResetBehaviour())
        {
            case PinBehaviourPerTurn.Reset:
                ResetPin();
                break;

            case PinBehaviourPerTurn.DisappearUponCollection:
                PerformDisappearingModeResetChecks();
                break;
        }
    }

    protected virtual void SetupPointBoard()
    {
        if (SetupFloatingBoard)
        {
            inBodyPointDisplay?.gameObject.SetActive(false);
            header = Instantiate(roundDataManager.collectibleHeaderPrefab, roundDataManager.collectibleHeadersParent);
            header.SetText(value);
            float headerOffset = (transform.position.y + (body.localScale.y * heightMultipleForOffsetCalculation) + 0.5f);
            header.transform.position = transform.position;
            header.transform.localScale = Mathf.Max(1, (header.transform.position.z / 30)) * header.transform.localScale;
            header.transform.position = new Vector3(header.transform.position.x, headerOffset, header.transform.position.z);
            header?.StartAnimation();
        }
        else if (SetupInBodyBoard && inBodyPointDisplay)
        {
            inBodyPointDisplay.gameObject.SetActive(true);
            inBodyPointDisplay.text = value.ToString();
            inBodyPointDisplay.fontMaterial = RegularFontColor;
            hitReaction?.UpdatePoints(value);
        }
    }

    public void PerformDisappearingModeResetChecks()
    {
        if (isPlayingSolo)
        {
            if (accountedForInThisShot)
                gameStateManager.ReturnCollectible(transform);
            else
                ResetPin();
        }
        else
        {
            if (stateForPlayers[currentPlayer])
                Stow();
            else
                ResetPin();
        }
    }
    
    private void Stow()
    {
        RBody.isKinematic = true;
        if (continuousMovement)
            continuousMovement.enabled = false;

        transform.position = new Vector3(500, 500, 500);
    }
    
    public void ResetPin()
    {
        RBody.isKinematic = defaultIsKinematic;
        if (!RBody.isKinematic)
        {
            RBody.angularVelocity = Vector3.zero;
            RBody.linearVelocity = Vector3.zero;
        }
        numTimesCollected = 0;
        accountedForInThisShot = false;
        transform.position = defaultPosition;
        transform.rotation = defaultRotation;
        
        if (continuousMovement)
            continuousMovement.enabled = true;
    }

    public bool CanBeCollected()
    {
        return numTimesCollected < numTimesCanBeCollected;
    }

    public void OverrideDefaultLocalScale(Vector3 scale)
    {
        defaultLocalScale = scale;
    }
}
