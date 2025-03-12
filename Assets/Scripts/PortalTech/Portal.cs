using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal linkedPortal;
    public Transform frontSide;
    public Transform backSide;
    
    private HashSet<GameObject> exitingObjects = new HashSet<GameObject>();

    public void OnObjectEnterPortal(PortalTraveler traveler, bool enteredFromFront)
    {
        // if (exitingObjects.Contains(traveler.gameObject))
        // {
        //     return;
        // }
        //
        // Transform exitSide = enteredFromFront ? linkedPortal.backSide : linkedPortal.frontSide;
        //
        // Vector3 newWorldPosition = linkedPortal.TransformPointToLinkedPortal(traveler.transform.position);
        // Quaternion newRotation = linkedPortal.transform.rotation * Quaternion.Inverse(transform.rotation) * traveler.transform.rotation;
        //
        // linkedPortal.MarkAsExiting(traveler.gameObject);
        //
        // if (traveler.GetComponent<Rigidbody>().isKinematic)
        // {
        //     traveler.Teleport(newWorldPosition, newRotation);
        // }
        // else
        // {
        //     Rigidbody rb = traveler.GetComponent<Rigidbody>();
        //     Vector3 localVelocity = rb.velocity;
        //     rb.velocity = newRotation * localVelocity;
        //     traveler.Teleport(newWorldPosition, newRotation);
        // }
    }

    public void MarkAsExiting(GameObject obj)
    {
        exitingObjects.Add(obj);
    }

    public Vector3 TransformToLinkedPortal(Vector3 point, Vector3 entryDirection)
    {
        // Convert the point to local space relative to this portal
        Vector3 localPoint = transform.InverseTransformPoint(point);

        // Determine if entry was from front or back
        bool enteredFromFront = Vector3.Dot(transform.forward, entryDirection) > 0;

        // Get the exit portal's transform
        Transform exitTransform = linkedPortal.transform;

        // Transform the local point to the exit portal
        Vector3 worldPoint = exitTransform.TransformPoint(localPoint);

        // If entered from behind, flip it around exit portal's forward
        if (!enteredFromFront)
        {
            Vector3 offset = worldPoint - exitTransform.position;
            worldPoint = exitTransform.position - offset;
        }

        return worldPoint;
    }
}
