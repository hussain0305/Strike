using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "CollectiblePrefabMapping", menuName = "Game/Collectible Prefab Mapping")]
public class CollectiblePrefabMapping : ScriptableObject
{
    private static CollectiblePrefabMapping _instance;
    public static CollectiblePrefabMapping Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<CollectiblePrefabMapping>("CollectiblePrefabMapping");
                if (_instance == null)
                {
                    Debug.LogError("CollectiblePrefabMapping instance not found. Please create one in the Resources folder.");
                }
            }
            return _instance;
        }
    }

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

    [Header("Blockification")]
    public GameObject block1x;
    public GameObject block2x;
    
    private Dictionary<PointTokenType, PointTokenPrefab> pointTokenDict = new ();
    private Dictionary<MultiplierTokenType, MultiplierTokenPrefab> multiplierTokenDict = new ();
    private Dictionary<DangerTokenType, DangerTokenPrefab> dangerTokenDict = new ();
    private Dictionary<ObstacleType, ObstaclePrefab> obstacleDict = new ();

    private void OnEnable()
    {
        BuildDictionaries();
    }

    private void BuildDictionaries()
    {
        pointTokenDict = new Dictionary<PointTokenType, PointTokenPrefab>();
        foreach (var entry in pointTokenPrefabs)
        {
            pointTokenDict.TryAdd(entry.pointTokenType, entry);
        }

        multiplierTokenDict = new Dictionary<MultiplierTokenType, MultiplierTokenPrefab>();
        foreach (var entry in multiplierTokenPrefabs)
        {
            multiplierTokenDict.TryAdd(entry.multiplierTokenType, entry);
        }

        dangerTokenDict = new Dictionary<DangerTokenType, DangerTokenPrefab>();
        foreach (var entry in dangerTokenPrefabs)
        {
            dangerTokenDict.TryAdd(entry.dangerTokenType, entry);
        }

        obstacleDict = new Dictionary<ObstacleType, ObstaclePrefab>();
        foreach (var entry in obstaclePrefabs)
        {
            obstacleDict.TryAdd(entry.obstacleType, entry);
        }
    }

    public GameObject GetPointTokenPrefab(PointTokenType pointTokenType)
    {
        if (pointTokenDict.TryGetValue(pointTokenType, out var entry))
            return entry.prefab;

        Debug.LogError($"Prefab not found for PointTokenType: {pointTokenType}");
        return null;
    }

    public GameObject GetMultiplierTokenPrefab(MultiplierTokenType multiplierTokenType)
    {
        if (multiplierTokenDict.TryGetValue(multiplierTokenType, out var entry))
            return entry.prefab;
        
        Debug.LogError($"Prefab not found for MultiplierTokenType: {multiplierTokenType}");
        return null;
    }
    
    public GameObject GetDangerTokenPrefab(DangerTokenType dangerTokenType)
    {
        if (dangerTokenDict.TryGetValue(dangerTokenType, out var entry))
            return entry.prefab;
        
        Debug.LogError($"Prefab not found for DangerTokenType: {dangerTokenType}");
        return null;
    }
    
    public GameObject GetObstaclePrefab(ObstacleType obstacleType)
    {
        if (obstacleDict.TryGetValue(obstacleType, out var entry))
            return entry.prefab;
        
        Debug.LogError($"Prefab not found for ObstacleType: {obstacleType}");
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
        if (pointTokenDict.TryGetValue(pointTokenType, out var entry))
            return entry.dimensions;
        
        return Vector3.zero;
    }

    public Vector3 GetMultiplierTokenDimension(MultiplierTokenType multiplierTokenType)
    {
        if (multiplierTokenDict.TryGetValue(multiplierTokenType, out var entry))
            return entry.dimensions;
        
        return Vector3.zero;
    }
    
    public Vector3 GetDangerTokenDimension(DangerTokenType dangerTokenType)
    {
        if (dangerTokenDict.TryGetValue(dangerTokenType, out var entry))
            return entry.dimensions;
        
        return Vector3.zero;
    }
    
    public Vector3 GetObstacleDimension(ObstacleType obstacleType)
    {
        if (obstacleDict.TryGetValue(obstacleType, out var entry))
            return entry.dimensions;
        
        return Vector3.zero;
    }
}