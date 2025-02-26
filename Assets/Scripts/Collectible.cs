using TMPro;
using UnityEngine;

public class CollectibleHitEvent
{
    public CollectibleType Type { get; }
    public int Value { get; }

    public CollectibleHitEvent(CollectibleType type, int value)
    {
        Type = type;
        Value = value;
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
    
    private CollectibleHeader header;

    private int collectingLayer;
    private int numTimesCollected = 0;
    private bool accountedForInThisShot = false;

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;

    private CollectibleHitReaction hitReaction;
    
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

    public void SaveDefaults()
    {
        defaultPosition = transform.position;
        defaultRotation = transform.rotation;
    }

    public void InitPointDisplay()
    {
        if (HasPointBoard) SetupPointBoard();
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
        EventBus.Publish(new CollectibleHitEvent(type, value));
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
        EventBus.Publish(new CollectibleHitEvent(type, value));
        header?.gameObject.SetActive(false);
    }

    public void BallShot(BallShotEvent e)
    {
        
    }

    public void NextShotCued(NextShotCuedEvent e)
    {
        if (GameManager.Instance.pinBehaviour == PinBehaviourPerTurn.Reset)
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
            hitReaction?.UpdatePoints(value);
        }
    }
}
