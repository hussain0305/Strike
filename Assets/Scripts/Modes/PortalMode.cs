using UnityEngine;

public class PortalMode : GameMode
{
    private void Start()
    {
        pinBehaviour = PinBehaviourPerTurn.RefreshPin;
        maxTimePerShot = 15;
    }
}
