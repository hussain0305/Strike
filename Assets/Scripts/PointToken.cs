using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public enum CollectibleShape
{
    None,
    Cuboid,
    HexagonPointTop,
    HexagonFlatTop
}

public class PointToken : Collectible
{
    public PointTokenType pointTokenType;

    [Header("Blockification")]
    public CollectibleShape shape;
    public Vector3 blockifyDimensions;
}