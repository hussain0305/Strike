using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Bouncy")]
public class BouncyAbilityDefinition : AbilityModuleDefinition
{
    public PhysicsMaterial physicsMaterial;
    
    public override IBallAbilityModule CreateInstance()
    {
        return new BouncyModule(physicsMaterial);
    }
}