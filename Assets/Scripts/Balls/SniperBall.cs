using System.Collections.Generic;
using UnityEngine;

public class SniperBall : Ball
{
    public override void InitAbilityDriver(List<IBallAbilityModule> additionalModules)
    {
        var ballProperty = Balls.Instance.GetBall("sniperBall");
        var moduleInstance = ballProperty.CreateModuleInstance();
        var modules = new List<IBallAbilityModule> { moduleInstance };

        if (additionalModules != null)
            modules.AddRange(additionalModules);

        AbilityDriver.Configure(this, context, modules);
    }
    
    public override void InitTrajectoryCalcualtor()
    {
        trajectoryCalculator = new NonGravitationalTrajectory();
        trajectoryCalculator.Initialize(context, this, 0, trajectoryDefinition);
    }
}
