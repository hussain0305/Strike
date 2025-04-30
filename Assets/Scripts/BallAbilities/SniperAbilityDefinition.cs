using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Sniper")]
public class SniperAbilityDefinition : AbilityModuleDefinition
{
    public override IBallAbilityModule CreateInstance()
    {
        return new SniperModule();
    }
}