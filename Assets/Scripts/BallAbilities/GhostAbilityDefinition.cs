using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Ghost")]
public class GhostAbilityDefinition : AbilityModuleDefinition
{
    public override IBallAbilityModule CreateInstance()
    {
        return new GhostModule();
    }
}