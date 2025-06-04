using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Shotgun")]
public class ShotgunAbilityDefinition : AbilityModuleDefinition
{
    public GameObject pelletPrefab;
    public Vector2 spread = new Vector2(0.5f, 0.5f);
    public int pelletPoolCount = 25;
    public int pelletsToFire = 20;

    public override IBallAbilityModule CreateInstance()
    {
        return new ShotgunModule(pelletPrefab, spread, pelletPoolCount, pelletsToFire);
    }
}