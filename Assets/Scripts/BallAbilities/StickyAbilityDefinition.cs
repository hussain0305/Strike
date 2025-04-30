using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Sticky")]
public class StickyAbilityDefinition : AbilityModuleDefinition
{
    public override IBallAbilityModule CreateInstance()
    {
        return new StickyModule();
    }
}