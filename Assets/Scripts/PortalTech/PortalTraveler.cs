using UnityEngine;

public class PortalTraveler : MonoBehaviour
{
    private Rigidbody rBody;

    [HideInInspector]
    public bool isPassingThroughPortal;
    
    private void Awake()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public void Teleport(Transform entryPortal, Transform exitPortal)
    {
        Vector3 oldVel = rBody != null ? rBody.linearVelocity : Vector3.zero;
        Quaternion oldRot = transform.rotation;
        
        Vector3 localVel = entryPortal.InverseTransformDirection(oldVel);
        
        Vector3 pointInEntryLocal = entryPortal.InverseTransformPoint(transform.position);
        Vector3 newWorldPos = exitPortal.TransformPoint(pointInEntryLocal);
        Quaternion relativeRot = Quaternion.Inverse(entryPortal.rotation) * oldRot;
        Quaternion newWorldRot = exitPortal.rotation * relativeRot;

        transform.SetPositionAndRotation(newWorldPos + (exitPortal.forward * 0.1f), newWorldRot);

        if (rBody != null)
            rBody.linearVelocity = exitPortal.TransformDirection(localVel);
    }
}