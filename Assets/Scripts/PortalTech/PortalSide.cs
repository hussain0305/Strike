using UnityEngine;

public class PortalSide : MonoBehaviour
{
    public Portal parentPortal;
    public bool isFrontSide;
    
    private void OnTriggerEnter(Collider other)
    {
        PortalTraveler traveler = other.GetComponent<PortalTraveler>();
        if (traveler)
        {
            parentPortal.OnObjectEnterPortal(traveler, isFrontSide);
        }
    }
}