using UnityEditor;
using UnityEngine;
using System.IO;

public class LevelEditorWindow : EditorWindow
{
    private int levelNumber = 1;
    private GameModeType gameMode = GameModeType.Pins;
    private Transform collectibleParentWorld;
    private Transform collectibleParentUI;
    private LevelExporter.LevelData loadedLevelData;

    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Editor", EditorStyles.boldLabel);

        gameMode = (GameModeType)EditorGUILayout.EnumPopup("Game Mode", gameMode);
        levelNumber = EditorGUILayout.IntField("Level Number", levelNumber);

        if (GUILayout.Button("Load Level"))
        {
            LoadLevelInScene();
        }

        if (GUILayout.Button("Save Level"))
        {
            SaveLevel();
        }
    }

    private void LoadLevelInScene()
    {
        string resourcePath = $"Levels/Level_{gameMode}_{levelNumber}";
        TextAsset jsonText = Resources.Load<TextAsset>(resourcePath);

        if (jsonText == null)
        {
            Debug.LogError($"Level file not found in Resources at path: {resourcePath}");
            return;
        }

        string json = jsonText.text;
        loadedLevelData = JsonUtility.FromJson<LevelExporter.LevelData>(json);

        ClearExistingCollectibles();

        LevelExporter levelExporter = FindObjectOfType<LevelExporter>();
        if (levelExporter == null)
        {
            Debug.LogError("LevelExporter not found in scene!");
            return;
        }

        collectibleParentWorld = levelExporter.collectibleParentWorld;
        collectibleParentUI = levelExporter.collectibleParentUI;
        
        CollectiblePrefabMapping prefabMapping = Resources.Load<CollectiblePrefabMapping>("CollectiblePrefabMapping");
        if (prefabMapping == null)
        {
            Debug.LogError("CollectiblePrefabMapping not found in Resources!");
            return;
        }
        
        foreach (var collectibleData in loadedLevelData.collectibles)
        {
            GameObject prefab = null;
            if (collectibleData.pointTokenType != PointTokenType.None) // Assuming there's a way to check if it's a PointToken
            {
                prefab = prefabMapping.GetPointTokenPrefab(collectibleData.pointTokenType);
            }
            else if (collectibleData.multiplierTokenType != MultiplierTokenType.None) // Assuming there's a way to check if it's a MultiplierToken
            {
                prefab = prefabMapping.GetMultiplierTokenPrefab(collectibleData.multiplierTokenType);
            }

            if (prefab == null)
            {
                Debug.LogError($"No prefab found for {collectibleData.type}");
                return;
            }

            GameObject collectibleObject = Instantiate(prefab,
                collectibleData.parent == CollectibleParent.World ? collectibleParentWorld : collectibleParentUI);
            collectibleObject.transform.position = collectibleData.position;
            collectibleObject.transform.rotation = collectibleData.rotation;

            Collectible collectibleScript = collectibleObject.GetComponent<Collectible>();
            if (!collectibleScript)
            {
                collectibleScript = collectibleObject.AddComponent<Collectible>();
            }
            collectibleScript.type = collectibleData.type;
            collectibleScript.value = collectibleData.value;
            collectibleScript.numTimesCanBeCollected = collectibleData.numTimesCanBeCollected;

            collectibleObject.transform.SetParent(collectibleData.parent == CollectibleParent.UI ? collectibleParentUI : collectibleParentWorld);
        }

        Debug.Log($"Level {levelNumber} loaded in Scene View!");
    }

    private void SaveLevel()
    {
        if (loadedLevelData == null)
        {
            Debug.LogError("No level data loaded to save.");
            return;
        }

        loadedLevelData.collectibles.Clear();

        foreach (Transform collectible in collectibleParentWorld)
        {
            Collectible collectibleScript = collectible.GetComponent<Collectible>();
            if (collectibleScript == null) continue;

            LevelExporter.CollectibleData data = new LevelExporter.CollectibleData
            {
                type = collectibleScript.type,
                position = collectible.position,
                rotation = collectible.rotation,
                value = collectibleScript.value,
                numTimesCanBeCollected = collectibleScript.numTimesCanBeCollected,
                parent = CollectibleParent.World
            };

            loadedLevelData.collectibles.Add(data);
        }

        foreach (Transform collectible in collectibleParentUI)
        {
            Collectible collectibleScript = collectible.GetComponent<Collectible>();
            if (collectibleScript == null) continue;

            LevelExporter.CollectibleData data = new LevelExporter.CollectibleData
            {
                type = collectibleScript.type,
                position = collectible.position,
                rotation = collectible.rotation,
                value = collectibleScript.value,
                numTimesCanBeCollected = collectibleScript.numTimesCanBeCollected,
                parent = CollectibleParent.UI
            };

            loadedLevelData.collectibles.Add(data);
        }

        string json = JsonUtility.ToJson(loadedLevelData, true);
        string path = $"Assets/Resources/Levels/Level_{gameMode}_{levelNumber}.json";
        File.WriteAllText(path, json);

        Debug.Log($"Level {levelNumber} saved!");
    }

    private void ClearExistingCollectibles()
    {
        if (collectibleParentWorld != null)
        {
            Debug.Log($"Deleting everything under {collectibleParentWorld.gameObject.name}!");
            while (collectibleParentWorld.childCount > 0)
            {
                Transform child = collectibleParentWorld.GetChild(0);
                Debug.Log($"Deleting {child.gameObject.name}!");
                DestroyImmediate(child.gameObject);
            }
        }

        if (collectibleParentUI != null)
        {
            Debug.Log($"Deleting everything under {collectibleParentUI.gameObject.name}!");
            while (collectibleParentUI.childCount > 0)
            {
                Transform child = collectibleParentUI.GetChild(0);
                Debug.Log($"Deleting {child.gameObject.name}!");
                DestroyImmediate(child.gameObject);
            }
        }
    }
}
