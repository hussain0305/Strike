using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LevelExporter : MonoBehaviour
{
    [System.Serializable]
    public class CollectibleData
    {
        public CollectibleType type;
        public PointTokenType pointTokenType;
        public MultiplierTokenType multiplierTokenType;
        public DangerTokenType dangerTokenType;
        public Vector3 position;
        public Quaternion rotation;
        public int value;
        public int numTimesCanBeCollected;
        public Collectible.PointDisplayType pointDisplayType;
        public Vector3[] path;
        public float movementSpeed;
        public bool activeOnStart;
    }

    [System.Serializable]
    public class StarData
    {
        public int index;
        public Vector3 position;
    }
    
    [System.Serializable]
    public class PortalData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3[] path;
        public float movementSpeed;
        public Vector3 rotationAxis;
        public float rotationSpeed;
    }
    
    [System.Serializable]
    public class PortalSet
    {
        public PortalData portalA;
        public PortalData portalB;
    }
    
    [System.Serializable]
    public class ObstacleData
    {
        public ObstacleType type;
        public Positioning positioning;
        public float movementSpeed;
        public float rotationSpeed;
        public Vector3 position;
        public Vector3 rotationAxis;
        public Quaternion rotation;
        public Vector3[] path;
    }

    [System.Serializable]
    public class LevelData
    {
        public GameModeType gameMode; 
        public int levelNumber;
        public int targetPoints;
        public List<CollectibleData> collectibles;
        public List<StarData> stars;
        public List<PortalSet> portals;
        public List<ObstacleData> obstacles;
        public float platformRotationSpeed;
    }

    public int level;
    public int targetPoints;
    public GameModeType gameMode;
    public Transform starsParent;
    public Transform portalsParent;
    public Transform obstaclesParentPlatform;
    public Transform obstaclesParentWorld;
    public Transform collectibleParent;
    public float platformRotationSpeed;

    public void ExportLevel()
    {
        LevelData levelData = new LevelData
        {
            levelNumber = this.level,
            targetPoints = this.targetPoints,
            gameMode = this.gameMode,
            collectibles = new List<CollectibleData>(),
            stars = new List<StarData>(),
            portals = new List<PortalSet>(),
            obstacles = new List<ObstacleData>(),
            platformRotationSpeed = this.platformRotationSpeed,
        };

        foreach (Transform collectible in collectibleParent)
        {
            Collectible collectibleScript = collectible.GetComponent<Collectible>();
            if (collectibleScript == null)
            {
                Debug.LogWarning($"Collectible script missing on object {collectible.name}, skipping.");
                continue;
            }

            DangerToken dangerTokenScript = collectible.GetComponent<DangerToken>();
            DangerTokenType dtt = dangerTokenScript != null ? dangerTokenScript.dangerTokenType : DangerTokenType.None;
            
            MultiplierToken multiplierTokenScript = collectible.GetComponent<MultiplierToken>();
            MultiplierTokenType mtt = multiplierTokenScript != null ? multiplierTokenScript.multiplierTokenType : MultiplierTokenType.None;
            
            PointToken pointTokenScript = collectible.GetComponent<PointToken>();
            PointTokenType ptt = pointTokenScript != null ? pointTokenScript.pointTokenType : PointTokenType.None;

            Collectible.PointDisplayType displayType = pointTokenScript ? pointTokenScript.pointDisplay :
                multiplierTokenScript ? multiplierTokenScript.pointDisplay : Collectible.PointDisplayType.None;

            ContinuousMovement moveScript = collectible.GetComponent<ContinuousMovement>();
            float movementSpeed = 0;
            Vector3[] path = null;
            if (moveScript != null)
            {
                path = new[] { moveScript.pointATransform.position, moveScript.pointBTransform.position };
                movementSpeed = moveScript.speed;
            }
            CollectibleData collectibleData = new CollectibleData
            {
                type = collectibleScript.type,
                pointTokenType = ptt,
                multiplierTokenType = mtt,
                dangerTokenType = dtt,
                position = collectible.position,
                rotation = collectible.rotation,
                value = collectibleScript.value,
                numTimesCanBeCollected = collectibleScript.numTimesCanBeCollected,
                pointDisplayType = displayType,
                movementSpeed = movementSpeed,
                path = path,
                activeOnStart = collectibleScript.activeOnStart
            };

            levelData.collectibles.Add(collectibleData);
        }
        
        foreach (Transform star in starsParent)
        {
            Star starScript = star.GetComponent<Star>();
            if (starScript == null)
            {
                Debug.LogWarning($"Star script missing on object {star.name}, skipping.");
                continue;
            }
            
            StarData starData = new StarData
            {
                index = starScript.index,
                position = star.position
            };

            levelData.stars.Add(starData);
        }
        
        foreach (PortalPair portalPair in portalsParent.GetComponentsInChildren<PortalPair>())
        {
            if (!portalPair.gameObject.activeSelf)
            {
                continue;
            }

            PortalSet portalSet = new PortalSet
            {
                portalA = ExtractPortalData(portalPair.portalA),
                portalB = ExtractPortalData(portalPair.portalB)
            };

            levelData.portals.Add(portalSet);
        }

        foreach (Transform obstacle in obstaclesParentPlatform)
        {
            levelData.obstacles.Add(ExtractObstacleData(obstacle, Positioning.OnPlatform));
        }
        
        foreach (Transform obstacle in obstaclesParentWorld)
        {
            levelData.obstacles.Add(ExtractObstacleData(obstacle, Positioning.InWorld));
        }
        
        string json = JsonUtility.ToJson(levelData, true);
        Directory.CreateDirectory("Assets/Resources/Levels");
        File.WriteAllText($"Assets/Resources/Levels/Level_{gameMode.ToString()}_{level}.json", json);
        Debug.Log($"Level {level} exported!");
        
        if (GameModeLevelMapping.Instance != null)
        {
            var gameModeEntry = GameModeLevelMapping.Instance.gameModeLevels.Find(entry => entry.gameMode == gameMode);

            if (gameModeEntry == null)
            {
                gameModeEntry = new GameModeLevelInfo
                {
                    gameMode = gameMode,
                    levels = new List<int>()
                };
                GameModeLevelMapping.Instance.gameModeLevels.Add(gameModeEntry);
            }

            int existingLevel = gameModeEntry.levels.Find(l => l == levelData.levelNumber);
            if (gameModeEntry.levels.Contains(existingLevel))
            {
                Debug.LogWarning($"Level {levelData.levelNumber} for GameMode {gameMode} already exists. Overwriting.");
                gameModeEntry.levels.Remove(existingLevel);
            }

            gameModeEntry.levels.Add(levelData.levelNumber);

            Debug.Log($"Updated GameModeLevelMapping with Level {levelData.levelNumber} for GameMode {gameMode}.");
        }
        else
        {
            Debug.LogError("Level mapping ScriptableObject is not assigned!");
        }
    }
    
    PortalData ExtractPortalData(Portal portal)
    {
        PortalData portalData = new PortalData
        {
            position = portal.transform.position,
            rotation = portal.transform.rotation,
            path = null,
            movementSpeed = 0,
            rotationAxis = Vector3.zero,
            rotationSpeed = 0
        };

        ContinuousMovement movementScript = portal.GetComponent<ContinuousMovement>();
        if (movementScript)
        {
            portalData.path = new[] { movementScript.pointATransform.position, movementScript.pointBTransform.position };
            portalData.movementSpeed = movementScript.speed;
        }

        ContinuousRotation rotationScript = portal.GetComponent<ContinuousRotation>();
        if (rotationScript)
        {
            portalData.rotationAxis = rotationScript.rotationAxis;
            portalData.rotationSpeed = rotationScript.rotationSpeed;
        }

        return portalData;
    }

    public ObstacleData ExtractObstacleData(Transform obstacle, Positioning _positioning)
    {
        Obstacle obstacleScript = obstacle.GetComponent<Obstacle>();

        ObstacleData obstacleData = new ObstacleData()
        {
            type = obstacleScript.type,
            positioning = _positioning,
            movementSpeed = 0,
            rotationSpeed = 0,
            rotationAxis = Vector3.zero,
            position = obstacle.position,
            rotation = obstacle.rotation,
            path = null
        };
            
        ContinuousMovement movementScript = obstacle.GetComponent<ContinuousMovement>();
        if (movementScript)
        {
            obstacleData.path = new[] { movementScript.pointATransform.position, movementScript.pointBTransform.position };
            obstacleData.movementSpeed = movementScript.speed;
        }

        ContinuousRotation rotationScript = obstacle.GetComponent<ContinuousRotation>();
        if (rotationScript)
        {
            obstacleData.rotationAxis = rotationScript.rotationAxis;
            obstacleData.rotationSpeed = rotationScript.rotationSpeed;
        }

        return obstacleData;
    }
}
