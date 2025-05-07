using UnityEngine;

public class DangerToken : Collectible
{
    [Header("Danger Token")]
    public DangerTokenType dangerTokenType;
    
    [HideInInspector]
    public int dangerTokenIndex;

}
