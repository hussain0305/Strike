using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Shotgun")]
public class ShotgunAbilityDefinition : AbilityModuleDefinition
{
    public override IBallAbilityModule CreateInstance()
    {
        return new ShotgunModule();
    }
}