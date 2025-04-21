using System.Collections.Generic;
using UnityEngine;

public class StickyBall : Ball
{
    public override void InitAbilityDriver()
    {
        AbilityDriver.Configure(this, context, new List<IBallAbilityModule>()
        {
            new StickyModule()
        });
    }
}
