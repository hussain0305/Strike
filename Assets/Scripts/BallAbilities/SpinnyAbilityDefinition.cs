using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Spinny")]
public class SpinnyAbilityDefinition : AbilityModuleDefinition
{
    public float spin;
    
    public override IBallAbilityModule CreateInstance()
    {
        return new SpinnyModule(spin);
    }
}