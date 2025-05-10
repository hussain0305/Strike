using System.Collections.Generic;
using UnityEngine;

public class RicochetBall : Ball
{
    public override void InitAbilityDriver(List<IBallAbilityModule> additionalModules)
    {
        var modules = new List<IBallAbilityModule> { new RicochetModule() };

        if (additionalModules != null)
            modules.AddRange(additionalModules);

        AbilityDriver.Configure(this, context, modules);
    }
}
