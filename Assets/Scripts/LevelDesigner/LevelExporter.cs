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
        public Vector3 position;
        public Quaternion rotation;
        public int value;
        public int numTimesCanBeCollected;
        public CollectibleParent parent;
        public Collectible.PointDisplayType pointDisplayType;
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
    public class LevelData
    {
        public GameModeType gameMode; 
        public int levelNumber;
        public int targetPoints;
        public List<CollectibleData> collectibles;
        public List<StarData> stars;
        public List<PortalSet> portals;
    }

    public int level;
    public int targetPoints;
    public GameModeType gameMode;
    public Transform starsParent;
    public Transform portalsParent;
    public Transform collectibleParentUI;
    public Transform collectibleParentWorld;

    public void ExportLevel()
    {
        LevelData levelData = new LevelData
        {
            levelNumber = this.level,
            targetPoints = this.targetPoints,
            gameMode = this.gameMode,
            collectibles = new List<CollectibleData>(),
            stars = new List<StarData>(),
            portals = new List<PortalSet>()
        };

        foreach (Transform collectible in collectibleParentWorld)
        {
            Collectible collectibleScript = collectible.GetComponent<Collectible>();
            if (collectibleScript == null)
            {
                Debug.LogWarning($"Collectible script missing on object {collectible.name}, skipping.");
                continue;
            }

            MultiplierToken multiplierTokenScript = collectible.GetComponent<MultiplierToken>();
            MultiplierTokenType mtt = multiplierTokenScript != null ? multiplierTokenScript.multiplierTokenType : MultiplierTokenType.None;
            
            PointToken pointTokenScript = collectible.GetComponent<PointToken>();
            PointTokenType ptt = pointTokenScript != null ? pointTokenScript.pointTokenType : PointTokenType.None;

            CollectibleData collectibleData = new CollectibleData
            {
                type = collectibleScript.type,
                pointTokenType = ptt,
                multiplierTokenType = mtt,
                position = collectible.position,
                rotation = collectible.rotation,
                value = collectibleScript.value,
                numTimesCanBeCollected = collectibleScript.numTimesCanBeCollected,
                parent = CollectibleParent.World,
                pointDisplayType = pointTokenScript.pointDisplay
            };

            levelData.collectibles.Add(collectibleData);
        }
        
        foreach (Transform collectible in collectibleParentUI)
        {
            Collectible collectibleScript = collectible.GetComponent<Collectible>();
            if (collectibleScript == null)
            {
                Debug.LogWarning($"Collectible script missing on object {collectible.name}, skipping.");
                continue;
            }

            MultiplierToken multiplierTokenScript = collectible.GetComponent<MultiplierToken>();
            MultiplierTokenType mtt = multiplierTokenScript != null ? multiplierTokenScript.multiplierTokenType : MultiplierTokenType.None;
            
            PointToken pointTokenScript = collectible.GetComponent<PointToken>();
            PointTokenType ptt = pointTokenScript != null ? pointTokenScript.pointTokenType : PointTokenType.None;

            Collectible.PointDisplayType displayType = pointTokenScript ? pointTokenScript.pointDisplay :
                multiplierTokenScript ? multiplierTokenScript.pointDisplay : Collectible.PointDisplayType.None;
            
            CollectibleData collectibleData = new CollectibleData
            {
                type = collectibleScript.type,
                pointTokenType = ptt,
                multiplierTokenType = mtt,
                position = collectible.position,
                rotation = collectible.rotation,
                value = collectibleScript.value,
                numTimesCanBeCollected = collectibleScript.numTimesCanBeCollected,
                parent = CollectibleParent.UI,
                pointDisplayType = displayType
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
}
