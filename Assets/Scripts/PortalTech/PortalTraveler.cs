using UnityEngine;

public class PortalTraveler : MonoBehaviour
{
    private Rigidbody rBody;

    private void Awake()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public void Teleport(Vector3 newPosition, Quaternion newRotation)
    {
        Quaternion oldRot = transform.rotation;
        Vector3 oldVel = Vector3.zero;

        if (rBody != null)
            oldVel = rBody.linearVelocity;

        transform.SetPositionAndRotation(newPosition, newRotation);

        Vector3 localVel = Quaternion.Inverse(oldRot) * oldVel;
        Vector3 newWorldVel = newRotation * localVel;

        if (rBody != null)
            rBody.linearVelocity = newWorldVel;
    }
}