using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class Collectible : MonoBehaviour
{
    public enum PointDisplayType
    {
        None,
        FloatingBoard,
        InBody
    };
    
    public delegate void CollectibleHit(CollectibleType type, int value);
    public static event CollectibleHit OnCollectibleHit;

    [Header("Components")]
    public Rigidbody rBody;
    
    [Header("Info")]
    public CollectibleType type;
    public int value;
    public int numTimesCanBeCollected = 1;
    
    private int numTimesCollected = 0;
    private bool accountedForInThisShot = false;

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    
    [Header("Header")]
    public float heightMultipleForOffsetCalculation = 1;
    public Transform body;
    public TextMeshPro inBodyPointDisplay;

    [FormerlySerializedAs("pointIndicator")] [Header("Feedback and Visual Indicators")]
    public PointDisplayType pointDisplay;

    public bool SetupFloatingBoard => pointDisplay == PointDisplayType.FloatingBoard;
    public bool SetupInBodyBoard => pointDisplay == PointDisplayType.InBody;
    public bool HasPointBoard => pointDisplay != PointDisplayType.None;
    
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

    public void InitPointDisplay()
    {
        if (HasPointBoard) SetupPointBoard();
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
        }
    }
}
