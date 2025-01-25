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
    }

    [System.Serializable]
    public class LevelData
    {
        public GameModeType gameMode; 
        public int levelNumber;
        public int targetPoints;
        public List<CollectibleData> collectibles;
    }

    public int level;
    public int targetPoints;
    public GameModeType gameMode;
    public Transform collectibleParentWorld;
    public Transform collectibleParentUI;
    public GameModeLevelMapping levelMapping;
    
    public void ExportLevel()
    {
        LevelData levelData = new LevelData
        {
            levelNumber = this.level,
            targetPoints = this.targetPoints,
            gameMode = this.gameMode,
            collectibles = new List<CollectibleData>()
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
                parent = CollectibleParent.World
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

            CollectibleData collectibleData = new CollectibleData
            {
                type = collectibleScript.type,
                pointTokenType = ptt,
                multiplierTokenType = mtt,
                position = collectible.position,
                rotation = collectible.rotation,
                value = collectibleScript.value,
                numTimesCanBeCollected = collectibleScript.numTimesCanBeCollected,
                parent = CollectibleParent.UI
            };

            levelData.collectibles.Add(collectibleData);
        }

        string json = JsonUtility.ToJson(levelData, true);
        Directory.CreateDirectory("Assets/Resources/Levels");
        File.WriteAllText($"Assets/Resources/Levels/Level_{gameMode.ToString()}_{level}.json", json);
        Debug.Log($"Level {level} exported!");
        
        if (levelMapping != null)
        {
            var gameModeEntry = levelMapping.gameModeLevels.Find(entry => entry.gameMode == gameMode);

            if (gameModeEntry == null)
            {
                gameModeEntry = new GameModeLevelInfo
                {
                    gameMode = gameMode,
                    levels = new List<int>()
                };
                levelMapping.gameModeLevels.Add(gameModeEntry);
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
}
