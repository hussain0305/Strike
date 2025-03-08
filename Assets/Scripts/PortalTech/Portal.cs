using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal linkedPortal;

    public Transform frontSide;
    public Transform backSide;

    public void OnObjectEnterPortal(PortalTraveler traveler, bool enteredFromFront)
    {
        Transform exitSide = enteredFromFront ? linkedPortal.backSide : linkedPortal.frontSide;

        Vector3 localOffset = transform.InverseTransformPoint(traveler.transform.position);
        Vector3 newWorldPosition = linkedPortal.transform.TransformPoint(localOffset);

        Quaternion relativeRotation = Quaternion.Inverse(transform.rotation) * traveler.transform.rotation;
        Quaternion newRotation = linkedPortal.transform.rotation * relativeRotation;

        traveler.Teleport(newWorldPosition, newRotation);
    }
}