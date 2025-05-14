using NUnit.Framework.Internal;
using UnityEngine;

public class RandomizerSpawner : MonoBehaviour
{
    protected EndlessModeLoader EndlessMode;
    
    public void Initialize(EndlessModeLoader _randomizerloader)
    {
        EndlessMode = _randomizerloader;
    }
}
