using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal linkedPortal;
    
    public void OnObjectEnterPortal(PortalTraveler traveler, bool enteredFromFront)
    {
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var traveler = other.GetComponent<PortalTraveler>();
        if (traveler == null || linkedPortal == null) return;

        Vector3 localPos = transform.InverseTransformPoint(traveler.transform.position);
        Quaternion localRot = Quaternion.Inverse(transform.rotation) * traveler.transform.rotation;

        Vector3 exitPos = linkedPortal.transform.TransformPoint(localPos);
        Quaternion exitRot = linkedPortal.transform.rotation * localRot;

        exitPos += linkedPortal.transform.forward * 0.1f;

        traveler.Teleport(exitPos, exitRot);
    }
}
