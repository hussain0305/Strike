using System;
using UnityEngine;

public class DisappearingMode : GameMode
{
    private void Start()
    {
        pinBehaviour = PinBehaviourPerTurn.DisappearUponCollection;
    }
}
