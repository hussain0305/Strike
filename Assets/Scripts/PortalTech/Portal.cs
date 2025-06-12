using System;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal linkedPortal;
    private PortalPair portalBridge;
    private PortalPair PortalBridge => portalBridge ??= GetComponentInParent<PortalPair>();
    
    private void OnTriggerEnter(Collider other)
    {
        var traveler = other.GetComponentInParent<PortalTraveler>();
        if (traveler == null || linkedPortal == null || PortalBridge == null)
            return;

        if (PortalBridge.IsTraveling(traveler))
        {
            PortalBridge.EnteredPortal(traveler, this);
            return;
        }
        traveler.isPassingThroughPortal = true;
        
        if (other.GetComponent<Ball>())
            return;
        
        PortalBridge.EnteredPortal(traveler, this);
        traveler.Teleport(transform, linkedPortal.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        var traveler = other.GetComponent<PortalTraveler>();
        if (traveler == null)
            return;

        traveler.isPassingThroughPortal = false;
        
        if (other.GetComponent<Ball>())
            return;

        PortalBridge.ExitedPortal(traveler, this);
    }
}
