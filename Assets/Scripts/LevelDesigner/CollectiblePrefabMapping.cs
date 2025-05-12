using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "CollectiblePrefabMapping", menuName = "Game/Collectible Prefab Mapping")]
public class CollectiblePrefabMapping : ScriptableObject
{
    [System.Serializable]
    public class PointTokenPrefab
    {
        public PointTokenType pointTokenType;
        public GameObject prefab;
        public Vector3 dimensions;
    }

    [System.Serializable]
    public class MultiplierTokenPrefab
    {
        public MultiplierTokenType multiplierTokenType;
        public GameObject prefab;
        public Vector3 dimensions;
    }

    [System.Serializable]
    public class DangerTokenPrefab
    {
        public DangerTokenType dangerTokenType;
        public GameObject prefab;
        public Vector3 dimensions;
    }

    [System.Serializable]
    public class ObstaclePrefab
    {
        public ObstacleType obstacleType;
        public GameObject prefab;
        public Vector3 dimensions;
    }

    public PointTokenPrefab[] pointTokenPrefabs;
    public MultiplierTokenPrefab[] multiplierTokenPrefabs;
    public DangerTokenPrefab[] dangerTokenPrefabs;
    public ObstaclePrefab[] obstaclePrefabs;
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
    
    public GameObject GetDangerTokenPrefab(DangerTokenType dangerTokenType)
    {
        foreach (var entry in dangerTokenPrefabs)
        {
            if (entry.dangerTokenType == dangerTokenType)
                return entry.prefab;
        }
        Debug.LogError($"Prefab not found for DangerTokenType: {dangerTokenType}");
        return null;
    }
    
    public GameObject GetObstaclePrefab(ObstacleType obstacleType)
    {
        foreach (var entry in obstaclePrefabs)
        {
            if (entry.obstacleType == obstacleType)
                return entry.prefab;
        }
        Debug.LogError($"Prefab not found for DangerTokenType: {obstacleType}");
        return null;
    }
    
    public GameObject GetStarPrefab()
    {
        if (starPrefab != null)
            return starPrefab;
        
        Debug.LogError("Star prefab not assigned!");
        return null;
    }
    
    public Vector3 GetPointTokenDimension(PointTokenType pointTokenType)
    {
        foreach (var entry in pointTokenPrefabs)
        {
            if (entry.pointTokenType == pointTokenType)
                return entry.dimensions;
        }
        return Vector3.zero;
    }

    public Vector3 GetMultiplierTokenDimension(MultiplierTokenType multiplierTokenType)
    {
        foreach (var entry in multiplierTokenPrefabs)
        {
            if (entry.multiplierTokenType == multiplierTokenType)
                return entry.dimensions;
        }
        return Vector3.zero;
    }
    
    public Vector3 GetDangerTokenDimension(DangerTokenType dangerTokenType)
    {
        foreach (var entry in dangerTokenPrefabs)
        {
            if (entry.dangerTokenType == dangerTokenType)
                return entry.dimensions;
        }
        return Vector3.zero;
    }
    
    public Vector3 GetObstacleDimension(ObstacleType obstacleType)
    {
        foreach (var entry in obstaclePrefabs)
        {
            if (entry.obstacleType == obstacleType)
                return entry.dimensions;
        }
        return Vector3.zero;
    }

}