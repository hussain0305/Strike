using System.Collections.Generic;
using UnityEngine;

public class ShotgunBall : Ball
{
    public override void InitAbilityDriver()
    {
        AbilityDriver.Configure(this, context, new List<IBallAbilityModule>()
        {
            new ShotgunModule()
        });
    }
}
