using UnityEngine;

[CreateAssetMenu(fileName = "CollectiblePrefabMapping", menuName = "Game/Collectible Prefab Mapping")]
public class CollectiblePrefabMapping : ScriptableObject
{
    [System.Serializable]
    public class PointTokenPrefab
    {
        public PointTokenType pointTokenType;
        public GameObject prefab;
    }

    [System.Serializable]
    public class MultiplierTokenPrefab
    {
        public MultiplierTokenType multiplierTokenType;
        public GameObject prefab;
    }

    public PointTokenPrefab[] pointTokenPrefabs;
    public MultiplierTokenPrefab[] multiplierTokenPrefabs;
    public GameObject starPrefab;
    
    public GameObject GetPointTokenPrefab(PointTokenType pointTokenType)
    {
        foreach (var entry in pointTokenPrefabs)
        {
            if (entry.pointTokenType == pointTokenType)
                return entry.prefab;
        }
        Debug.LogError($"Prefab not found for PointTokenType: {pointTokenType}");
        return null;
    }

    public GameObject GetMultiplierTokenPrefab(MultiplierTokenType multiplierTokenType)
    {
        foreach (var entry in multiplierTokenPrefabs)
        {
            if (entry.multiplierTokenType == multiplierTokenType)
                return entry.prefab;
        }
        Debug.LogError($"Prefab not found for MultiplierTokenType: {multiplierTokenType}");
        return null;
    }
    
    public GameObject GetStarPrefab()
    {
        if (starPrefab != null)
            return starPrefab;
        
        Debug.LogError("Star prefab not assigned!");
        return null;
    }
}