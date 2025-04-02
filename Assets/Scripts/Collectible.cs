using TMPro;
using UnityEngine;

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

public class Collectible : MonoBehaviour
{
    public enum PointDisplayType
    {
        None,
        FloatingBoard,
        InBody
    };
    
    [Header("Components")]
    public Rigidbody rBody;
    
    [Header("Info")]
    public CollectibleType type;
    public int value;
    public int numTimesCanBeCollected = 1;
    
    [Header("Header")]
    public float heightMultipleForOffsetCalculation = 1;
    public Transform body;
    public TextMeshPro inBodyPointDisplay;

    [Header("Feedback and Visual Indicators")]
    public PointDisplayType pointDisplay;

    public bool SetupFloatingBoard => pointDisplay == PointDisplayType.FloatingBoard;
    public bool SetupInBodyBoard => pointDisplay == PointDisplayType.InBody;
    public bool HasPointBoard => pointDisplay != PointDisplayType.None;

    public Material RegularFontColor => value >= 0
        ? GlobalAssets.Instance.positiveCollectibleTextMaterial
        : GlobalAssets.Instance.negativeCollectibleTextMaterial;
    
    public Material HitFontColor => value > 0
        ? GlobalAssets.Instance.positiveCollectibleHitTextMaterial
        : GlobalAssets.Instance.negativeCollectibleHitTextMaterial;
    
    private CollectibleHeader header;

    private int collectingLayer;
    private int numTimesCollected = 0;
    private bool accountedForInThisShot = false;

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;

    private CollectibleHitReaction hitReaction;

    private IContextProvider context;
    
    public void Awake()
    {
        SaveDefaults();
        hitReaction = GetComponent<CollectibleHitReaction>();
    }

    public void Start()
    {
        collectingLayer = LayerMask.GetMask("Ball", "OtherCollectingObject");
    }

    public void OnEnable()
    {
        EventBus.Subscribe<BallShotEvent>(BallShot);
        EventBus.Subscribe<NextShotCuedEvent>(NextShotCued);
    }

    public void OnDisable()
    {
        EventBus.Unsubscribe<BallShotEvent>(BallShot);
        EventBus.Unsubscribe<NextShotCuedEvent>(NextShotCued);
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
        
        SaveDefaults();
        InitAppearance();
    }

    public void SaveDefaults()
    {
        defaultPosition = transform.position;
        defaultRotation = transform.rotation;
    }

    public void InitAppearance()
    {
        if (HasPointBoard) SetupPointBoard();
        
        GetComponent<CollectibleHitReaction>()?.SetDefaultVisuals(null);
    }

    public void OnCollisionEnter(Collision other)
    {
        if (other != null && other.gameObject && (collectingLayer & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }
        if (numTimesCollected >= numTimesCanBeCollected || accountedForInThisShot)
        {
            return;
        }
    
        numTimesCollected++;
        accountedForInThisShot = true;
        EventBus.Publish(new CollectibleHitEvent(type, value, other.transform.position));
        header?.gameObject.SetActive(false);
        hitReaction?.CheckIfHitsExhasuted(numTimesCollected, numTimesCanBeCollected);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.gameObject && (collectingLayer & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }
        if (numTimesCollected >= numTimesCanBeCollected || accountedForInThisShot)
        {
            return;
        }

        numTimesCollected++;
        accountedForInThisShot = true;
        EventBus.Publish(new CollectibleHitEvent(type, value, other.transform.position));
        header?.gameObject.SetActive(false);
    }

    public void BallShot(BallShotEvent e)
    {
        
    }

    public void NextShotCued(NextShotCuedEvent e)
    {
        if (context.GetPinResetBehaviour() == PinBehaviourPerTurn.Reset)
        {
            numTimesCollected = 0;
            accountedForInThisShot = false;
            // rBody.linearVelocity = Vector3.zero;
            // rBody.angularVelocity = Vector3.zero;
            transform.position = defaultPosition;
            transform.rotation = defaultRotation;
        }
        header?.gameObject.SetActive(true);
    }

    public void SetupPointBoard()
    {
        if (SetupFloatingBoard)
        {
            inBodyPointDisplay?.gameObject.SetActive(false);
            header = Instantiate(RoundDataManager.Instance.collectibleHeaderPrefab, RoundDataManager.Instance.collectibleHeadersParent);
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
}
