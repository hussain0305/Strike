using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class SpinInput : MonoBehaviour
{
    public RectTransform controlArea;
    public RectTransform pointer;
    public float spinMultiplier = 2f;
    public TextMeshProUGUI spinValueText;
    
    public Vector2 SpinVector { get; private set; }

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

            // Check if the mouse click is inside the control area
            if (controlArea.rect.Contains(localMousePosition))
            {
                isInteracting = true;

                Vector3 offset = localMousePosition - center;
                float radius = controlArea.rect.width / 2f;

                if (offset.magnitude > radius)
                    offset = offset.normalized * radius;

                offset.z = RestingPosition.z;
                pointer.localPosition = offset;
                SpinVector = Vector2.ClampMagnitude(offset / radius, 1f);
                SpinVector *= spinMultiplier;
                string spinText = $"{SpinVector.x:F2}, {SpinVector.y:F2}";
                spinValueText.text = spinText;
            }
        }
        else if (isInteracting)
        {
            isInteracting = false;
        }
    }

    public void ResetPointer()
    {
        pointer.localPosition = RestingPosition;
        SpinVector = Vector2.zero;
        spinValueText.text = "";
    }
}