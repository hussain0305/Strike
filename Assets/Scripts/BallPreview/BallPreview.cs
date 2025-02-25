using UnityEngine;

public class BallPreview : MonoBehaviour
{
    public delegate void ResetPreview();
    public static event ResetPreview OnResetPreview;

    public void TriggerResetPreview()
    {
        OnResetPreview?.Invoke();
    }
}
