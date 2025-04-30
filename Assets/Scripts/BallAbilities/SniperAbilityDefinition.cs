using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Sniper")]
public class SniperAbilityDefinition : AbilityModuleDefinition
{
    public Material aimDotMaterial;
    
    public override IBallAbilityModule CreateInstance()
    {
        return new SniperModule(aimDotMaterial);
    }
}