using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Mirror")]
public class MirrorAbilityDefinition : AbilityModuleDefinition
{
    public GameObject mirrorPrefab;
    
    public override IBallAbilityModule CreateInstance()
    {
        MirrorModule mm = new MirrorModule();
        GameObject shadow = GameObject.Instantiate(mirrorPrefab);
        shadow.SetActive(false);
        mm.SetShadowBall(shadow);
        return mm;
    }
}