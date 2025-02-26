using System;
using UnityEngine;
using System.IO;

public class LevelLoader : MonoBehaviour
{
    public Transform colletiblesParentWorld;
    public Transform colletiblesParentUI;
    public Transform starsParent;

    private int targetPoints;
    
    private static LevelLoader instance;
    public static LevelLoader Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    
    public void LoadLevel()
    {
        if (PoolingManager.Instance == null)
        {
            Debug.LogError("PoolingManager is not assigned.");
            return;
        }

        int levelNumber = 1;
        GameModeType gameMode = GameModeType.Pins;
        if (ModeSelector.Instance)
        {
            levelNumber = ModeSelector.Instance.GetSelectedLevel();
            gameMode = ModeSelector.Instance.GetSelectedGameMode();
        }
        else
        {
            levelNumber = LevelSetup_EditorUseOnly.Instance.levelToLoad;
            gameMode = LevelSetup_EditorUseOnly.Instance.gameMode;
        }
        
        string resourcePath = $"Levels/Level_{gameMode}_{levelNumber}";
        TextAsset jsonText = Resources.Load<TextAsset>(resourcePath);

        if (jsonText == null)
        {
            Debug.LogError($"Level file not found in Resources at path: {resourcePath}");
            return;
        }

        string json = jsonText.text;
        
        LevelExporter.LevelData levelData = JsonUtility.FromJson<LevelExporter.LevelData>(json);

        targetPoints = levelData.targetPoints;

        foreach (LevelExporter.CollectibleData collectibleData in levelData.collectibles)
        {
            GameObject collectibleObject = null;
            if (collectibleData.pointTokenType != PointTokenType.None)
            {
                collectibleObject = PoolingManager.Instance.GetObject(collectibleData.pointTokenType);
            }
            else if (collectibleData.multiplierTokenType != MultiplierTokenType.None)
            {
                collectibleObject = PoolingManager.Instance.GetObject(collectibleData.multiplierTokenType);
            }

            if (collectibleObject == null)
            {
                Debug.LogWarning($"Failed to load collectible of type {collectibleData.type}");
                continue;
            }
            
            collectibleObject.transform.SetParent(collectibleData.parent == CollectibleParent.UI ? colletiblesParentUI : colletiblesParentWorld);
            collectibleObject.transform.position = collectibleData.position;
            collectibleObject.transform.rotation = collectibleData.rotation;

            Collectible collectibleScript = collectibleObject.GetComponent<Collectible>();
            if (collectibleScript != null)
            {
                collectibleScript.value = collectibleData.value;
                collectibleScript.numTimesCanBeCollected = collectibleData.numTimesCanBeCollected;
                collectibleScript.pointDisplay = collectibleData.pointDisplayType;
                collectibleScript.Initialize(GameManager.Context);
                collectibleScript.SaveDefaults();
                collectibleScript.InitPointDisplay();
            }
        }

        SaveManager.GetStarsCollectedStatus(ModeSelector.Instance.GetSelectedGameMode(),
            ModeSelector.Instance.GetSelectedLevel(), out bool[] starStatus);
        foreach (LevelExporter.StarData starData in levelData.stars)
        {
            if (starStatus[starData.index])
            {
                //Don't spawn the star if it was already collected
                continue;
            }
            GameObject starObject = PoolingManager.Instance.GetStar();
            if (starObject == null)
            {
                Debug.LogWarning($"Failed to load star");
                continue;
            }
            
            starObject.transform.SetParent(starsParent);
            starObject.transform.position = starData.position;
            Star starScript = starObject.GetComponent<Star>();
            starScript.index = starData.index;
        }

        Debug.Log($"Level {levelNumber} loaded successfully!");
    }

    public int GetTargetPoints()
    {
        return targetPoints;
    }
}
