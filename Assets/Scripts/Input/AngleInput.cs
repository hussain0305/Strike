using TMPro;
using UnityEngine;

public class AngleInput : MonoBehaviour
{
    public Transform cylinderPivot;
    public RectTransform controlArea;
    public RectTransform pointer;
    public float rotationSpeed = 1f;
    public TextMeshProUGUI angleValueText;
    public Vector2 pitchLimits = new Vector2(-75, 5);
    public Vector2 yawLimits = new Vector2(-75, 75);
    public float planeDistance = 2.0f;
    public Vector2 InputVector { get; private set; }

    private Vector3 RestingPosition => new(0, 0, -0.1f);

    private Vector2 center;
    private bool isInteracting;

    private void Start()
    {
        center = controlArea.rect.center;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                controlArea,
                Input.mousePosition,
                Camera.main,
                out Vector2 localMousePosition
            );

            if (controlArea.rect.Contains(localMousePosition))
            {
                isInteracting = true;

                Vector3 offset = localMousePosition - center;
                float radius = controlArea.rect.width / 2f;

                if (offset.magnitude > radius)
                    offset = offset.normalized * radius;

                offset.z = RestingPosition.z;
                pointer.localPosition = offset;
                InputVector = offset;
            }
        }
        else if (isInteracting)
        {
            isInteracting = false;
            pointer.localPosition = RestingPosition;
            InputVector = Vector2.zero;
        }

        if (InputVector.sqrMagnitude > 0f)
        {
            RotatePointer();
            ClampRotation();
        }

        Vector2 angle = CalculateProjectedAngle();
        angleValueText.text = $"{angle.x}, {angle.y}";
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
        Vector3 eulerAngles = cylinderPivot.localRotation.eulerAngles;

        eulerAngles.x = (eulerAngles.x > 180) ? eulerAngles.x - 360 : eulerAngles.x;
        eulerAngles.y = (eulerAngles.y > 180) ? eulerAngles.y - 360 : eulerAngles.y;

        eulerAngles.x = Mathf.Clamp(eulerAngles.x, pitchLimits.x, pitchLimits.y);
        eulerAngles.y = Mathf.Clamp(eulerAngles.y, yawLimits.x, yawLimits.y);

        cylinderPivot.localRotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, 0);
    }
    
    private void RotatePointer()
    {
        float horizontalInput = InputVector.x;
        float verticalInput = -InputVector.y;

        cylinderPivot.Rotate(Vector3.up * (horizontalInput * rotationSpeed * Time.deltaTime));
        cylinderPivot.Rotate(Vector3.right * (verticalInput * rotationSpeed * Time.deltaTime));
    }
    
    public void ResetPointer()
    {
        pointer.localPosition = RestingPosition;
        angleValueText.text = "";
        cylinderPivot.rotation = Quaternion.identity;
    }
}