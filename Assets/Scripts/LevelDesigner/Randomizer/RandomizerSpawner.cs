using NUnit.Framework.Internal;
using UnityEngine;

public class RandomizerSpawner : MonoBehaviour
{
    protected RandomizerLoader randomizer;
    
    public void Initialize(RandomizerLoader _randomizerloader)
    {
        randomizer = _randomizerloader;
    }
}
