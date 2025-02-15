using UnityEngine;

public class ContinuousRotation : MonoBehaviour
{
    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 30f;

    private void Update()
    {
        transform.Rotate(rotationAxis.normalized * (rotationSpeed * Time.deltaTime));
    }
}
