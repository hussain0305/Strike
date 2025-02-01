using UnityEngine;

public class Collectible : MonoBehaviour
{
    public delegate void CollectibleHit(CollectibleType type, int value);
    public static event CollectibleHit OnCollectibleHit;

    [Header("Components")]
    public Rigidbody rBody;
    
    [Header("Info")]
    public CollectibleType type;
    public int value;
    public int numTimesCanBeCollected = 1;
    
    public bool Collected { get; set; } = false;
    private int numTimesCollected = 0;
    private bool accountedForInThisShot = false;

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    
    [Header("Header")]
    public float heightMultipleForOffsetCalculation = 1;
    public Transform body;
    
    [Header("Feedback and Visual Indicators")]
    public bool spawnHeader = true;
    
    private CollectibleHeader header;
    
    public void Awake()
    {
        SaveDefaults();
    }

    public void OnEnable()
    {
        GameManager.OnBallShot += BallShot;
        GameManager.OnNextShotCued += NextShotCued;
    }

    public void OnDisable()
    {
        GameManager.OnBallShot -= BallShot;
        GameManager.OnNextShotCued -= NextShotCued;
    }

    public void SaveDefaults()
    {
        defaultPosition = transform.position;
        defaultRotation = transform.rotation;
    }

    public void Start()
    {
        if (spawnHeader) SpawnHeader();
    }

    public void OnCollisionEnter(Collision other)
    {
        if (!(other != null && other.gameObject != null && other.gameObject.GetComponent<Ball>()))
        {
            return;
        }
        if (numTimesCollected >= numTimesCanBeCollected || accountedForInThisShot)
        {
            return;
        }
    
        numTimesCollected++;
        accountedForInThisShot = true;
        OnCollectibleHit?.Invoke(type, value);
        header?.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!(other != null && other.gameObject != null && other.gameObject.GetComponent<Ball>()))
        {
            return;
        }
        if (numTimesCollected >= numTimesCanBeCollected || accountedForInThisShot)
        {
            return;
        }

        numTimesCollected++;
        accountedForInThisShot = true;
        Debug.Log("multiplier collected");
        OnCollectibleHit?.Invoke(type, value);
        header?.gameObject.SetActive(false);
    }

    public void BallShot()
    {
        
    }

    public void NextShotCued()
    {
        if (GameManager.Instance.pinBehaviour == PinBehaviourPerTurn.Reset)
        {
            numTimesCollected = 0;
            accountedForInThisShot = false;
            rBody.linearVelocity = Vector3.zero;
            rBody.angularVelocity = Vector3.zero;
            transform.position = defaultPosition;
            transform.rotation = defaultRotation;
        }
        header?.gameObject.SetActive(true);
    }

    public void SpawnHeader()
    {
        header = Instantiate(RoundDataManager.Instance.collectibleHeaderPrefab, RoundDataManager.Instance.collectibleHeadersParent);
        header.SetText(value);
        float headerOffset = (transform.position.y + (body.localScale.y * heightMultipleForOffsetCalculation) + 0.5f);
        header.transform.position = transform.position;
        header.transform.localScale = Mathf.Max(1, (header.transform.position.z / 30)) * header.transform.localScale;
        header.transform.position = new Vector3(header.transform.position.x, headerOffset, header.transform.position.z);
        header?.StartAnimation();
    }
}
