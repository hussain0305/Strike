using TMPro;
using UnityEngine;

public class AngleInput : MonoBehaviour
{
    public Transform cylinderPivot;
    public float rotationSpeed = 1f;
    public TextMeshProUGUI[] angleValueText;
    public float planeDistance = 2.0f;
    public Vector2 InputVector { get; private set; }

    private Vector3 RestingPosition => new(0, 0, -0.1f);

    private bool isInteracting;
    private Vector2 startTouch;
    private bool isDragging = false;
    private Vector2 accumulatedAngles;
    private MinMaxInt pitchLimits = new (-5, 75);
    private MinMaxInt yawLimits = new (-75, 75);

    private BallParameterController ballParameterController;
    private BallParameterController BallParameterController
    {
        get
        {
            if (!ballParameterController)
            {
                ballParameterController = GetComponent<BallParameterController>();
            }
            return ballParameterController;
        }
    }
    
    private void OnEnable()
    {
        EventBus.Subscribe<NextShotCuedEvent>(ResetPointer);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<NextShotCuedEvent>(ResetPointer);
    }

    private void Update()
    {
        if (!BallParameterController.IsInputtingAngle())
        {
            return;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            StartSwipe(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            UpdateSwipe(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndSwipe();
        }

        Vector2 angle = CalculateProjectedAngle();
        foreach (TextMeshProUGUI valText in angleValueText)
        {
            valText.text = $"{100 * angle.x}, {100 * angle.y}";
        }
    }

    void StartSwipe(Vector2 position)
    {
        startTouch = position;
        isDragging = true;
    }

    void UpdateSwipe(Vector2 position)
    {
        if (!isDragging) return;

        Vector2 swipeVector = position - startTouch;

        float yawDelta = Mathf.Atan2(swipeVector.x, Screen.width) * Mathf.Rad2Deg;
        float pitchDelta = Mathf.Atan2(swipeVector.y, Screen.height) * Mathf.Rad2Deg;

        accumulatedAngles.y += yawDelta;
        accumulatedAngles.x += pitchDelta;

        accumulatedAngles.x = Mathf.Clamp(accumulatedAngles.x, pitchLimits.Min, pitchLimits.Max);
        accumulatedAngles.y = Mathf.Clamp(accumulatedAngles.y, yawLimits.Min, yawLimits.Max);

        cylinderPivot.rotation = Quaternion.Euler(-accumulatedAngles.x, accumulatedAngles.y, 0);

        startTouch = position;
    }

    void EndSwipe()
    {
        isDragging = false;
    }

    public Vector2 CalculateProjectedAngle()
    {
        Vector3 forwardDirection = cylinderPivot.forward;
        Vector3 planePoint = cylinderPivot.position + forwardDirection * planeDistance;
        Vector3 localHitPoint = planePoint - cylinderPivot.position;

        float xAngle = Mathf.Round(localHitPoint.x * 100f) / 100f;
        float yAngle = Mathf.Round(localHitPoint.y * 100f) / 100f;

        return new Vector2(xAngle, yAngle);
    }

    private void ClampRotation()
    {
        accumulatedAngles.x = Mathf.Clamp(accumulatedAngles.x, pitchLimits.Min, pitchLimits.Max);
        accumulatedAngles.y = Mathf.Clamp(accumulatedAngles.y, yawLimits.Min, yawLimits.Max);

        cylinderPivot.rotation = Quaternion.Euler(accumulatedAngles.x, accumulatedAngles.y, 0);
    }

    public void ResetPointer(NextShotCuedEvent e)
    {
        cylinderPivot.rotation = Quaternion.identity;
        accumulatedAngles = Vector2.zero;
    }
}
