using UnityEngine;

public abstract class AbilityModuleDefinition : ScriptableObject
{
    public abstract IBallAbilityModule CreateInstance();
}