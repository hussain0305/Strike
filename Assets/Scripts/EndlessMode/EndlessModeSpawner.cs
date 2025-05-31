using NUnit.Framework.Internal;
using UnityEngine;

public class EndlessModeSpawner : MonoBehaviour
{
    protected EndlessModeLoader endlessMode;
    protected SectorGridHelper sectorGridHelper;
    
    public void Initialize(EndlessModeLoader _randomizerloader)
    {
        endlessMode = _randomizerloader;
        sectorGridHelper = endlessMode.SectorGridHelper;
    }
}
