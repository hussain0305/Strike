using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Ricochet")]
public class RicochetAbilityDefinition : AbilityModuleDefinition
{
    public override IBallAbilityModule CreateInstance()
    {
        return new RicochetModule();
    }
}