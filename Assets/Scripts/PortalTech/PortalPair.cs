using System;
using System.Collections.Generic;
using UnityEngine;

public class PortalTravelStatus
{
    public Portal entryPortal;
    public Portal exitPortal;
    public bool enterOnEntryPortalRegistered;
    public bool exitFromEntryPortalRegistered;
    public bool enterOnExitPortalRegistered;
    public bool exitFromExitPortalRegistered;

    public bool TeleportationResolved()
    {
        return enterOnEntryPortalRegistered && exitFromEntryPortalRegistered && 
               enterOnExitPortalRegistered && exitFromExitPortalRegistered;
    }
}

public class PortalPair : MonoBehaviour
{
    public Portal portalA;
    public Portal portalB;

    public Dictionary<PortalTraveler, PortalTravelStatus> currentTravelers;

    private void OnEnable()
    {
        currentTravelers = new Dictionary<PortalTraveler, PortalTravelStatus>();
    }

    public void EnteredPortal(PortalTraveler traveler, Portal portal)
    {
        if (!currentTravelers.ContainsKey(traveler))
        {
            PortalTravelStatus status = new PortalTravelStatus();
            status.entryPortal = portal;
            status.enterOnEntryPortalRegistered = true;
            currentTravelers.Add(traveler, status);
        }
        else
        {
            PortalTravelStatus status = currentTravelers[traveler];
            status.enterOnExitPortalRegistered = true;
            status.exitPortal = portal;
        }
    }

    public void ExitedPortal(PortalTraveler traveler, Portal portal)
    {
        if (currentTravelers.TryGetValue(traveler, out var status))
        {
            if (status.enterOnEntryPortalRegistered && status.entryPortal == portal)
            {
                status.exitFromEntryPortalRegistered = true;
            }
            else if (status.enterOnExitPortalRegistered && status.exitPortal == portal)
            {
                status.exitFromExitPortalRegistered = true;

                if (status.TeleportationResolved())
                {
                    currentTravelers.Remove(traveler);
                }
            }
        }
    }

    public bool IsTraveling(PortalTraveler traveler)
    {
        return currentTravelers.ContainsKey(traveler) && !currentTravelers[traveler].TeleportationResolved();
    }
}
