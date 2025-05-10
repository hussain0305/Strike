using System.Collections.Generic;
using UnityEngine;

public class MirrorBall : Ball
{
    public override void InitAbilityDriver(List<IBallAbilityModule> additionalModules)
    {
        var ballProperty = Balls.Instance.GetBall("mirrorBall");
        var moduleInstance = ballProperty.CreateModuleInstance();
        var modules = new List<IBallAbilityModule> { moduleInstance };
        
        if (additionalModules != null)
            modules.AddRange(additionalModules);

        AbilityDriver.Configure(this, context, modules);
    }
}
