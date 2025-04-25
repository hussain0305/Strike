using System.Collections.Generic;

public class GhostBall : Ball
{
    public override void InitAbilityDriver(List<IBallAbilityModule> additionalModules)
    {
        var modules = new List<IBallAbilityModule> { new GhostModule() };

        if (additionalModules != null)
            modules.AddRange(additionalModules);

        AbilityDriver.Configure(this, context, modules);
    }
}
