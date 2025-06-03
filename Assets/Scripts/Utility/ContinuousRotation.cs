using UnityEngine;

public class ContinuousRotation : MonoBehaviour
{
    public Vector3 rotationAxis = Vector3.up;
    public float rotationSpeed = 30f;
    private Space rotationSpace = Space.Self;

    private void Update()
    {
        transform.Rotate(rotationAxis.normalized * (rotationSpeed * Time.deltaTime), rotationSpace);
    }

    public void RotateAroundWorld()
    {
        rotationSpace = Space.World;
    }
}
