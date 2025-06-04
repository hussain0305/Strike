using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Disco")]
public class DiscoAbilityDefinition : AbilityModuleDefinition
{
    public GameObject[] discoPelletPrefabs;
    public Vector2 spread = new Vector2(0.5f, 0.5f);
    public int pelletPoolCount = 25;
    public int pelletsToFire = 20;

    public override IBallAbilityModule CreateInstance()
    {
        return new DiscoModule(discoPelletPrefabs, spread, pelletPoolCount, pelletsToFire);
    }
}
