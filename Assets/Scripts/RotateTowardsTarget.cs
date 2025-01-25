using UnityEngine;

public class RotateTowardsTarget : MonoBehaviour
{
    [Tooltip("The target object to face.")]
    public Transform target;

    [Tooltip("Set true to have the Z-axis face the target, false for the negative Z-axis.")]
    public bool faceWithZAxis = true;

    private void Update()
    {
        if (target == null)
            return;

        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0;

        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(
                faceWithZAxis ? directionToTarget : -directionToTarget
            );

            transform.rotation = desiredRotation;
        }
    }
}