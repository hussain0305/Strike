using System.Collections.Generic;
using UnityEngine;

public class ShotgunBall : Ball
{
    public override void InitAbilityDriver(List<IBallAbilityModule> additionalModules)
    {
        var modules = new List<IBallAbilityModule> { new ShotgunModule() };

        if (additionalModules != null)
            modules.AddRange(additionalModules);

        AbilityDriver.Configure(this, context, modules);
    }
}
