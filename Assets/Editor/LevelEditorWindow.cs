using UnityEditor;
using UnityEngine;
using System.IO;

public class LevelEditorWindow : EditorWindow
{
    private int levelNumber = 1;
    private GameModeType gameMode = GameModeType.Pins;
    private Transform starParent;
    private Transform portalsParent;
    private Transform collectibleParent;
    private Transform obstaclesParentWorld;
    private Transform obstaclesParentPlatform;
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

        LevelExporter levelExporter = FindAnyObjectByType<LevelExporter>();
        if (levelExporter == null)
        {
            Debug.LogError("LevelExporter not found in scene!");
            return;
        }

        collectibleParent = levelExporter.collectibleParent;
        starParent = levelExporter.starsParent;
        portalsParent = levelExporter.portalsParent;
        obstaclesParentPlatform = levelExporter.obstaclesParentPlatform;
        obstaclesParentWorld = levelExporter.obstaclesParentWorld;
        
        CollectiblePrefabMapping prefabMapping = Resources.Load<CollectiblePrefabMapping>("CollectiblePrefabMapping");
        if (prefabMapping == null)
        {
            Debug.LogError("CollectiblePrefabMapping not found in Resources!");
            return;
        }
        
        foreach (var collectibleData in loadedLevelData.collectibles)
        {
            GameObject prefab = null;
            if (collectibleData.pointTokenType != PointTokenType.None)
            {
                prefab = prefabMapping.GetPointTokenPrefab(collectibleData.pointTokenType);
            }
            else if (collectibleData.multiplierTokenType != MultiplierTokenType.None)
            {
                prefab = prefabMapping.GetMultiplierTokenPrefab(collectibleData.multiplierTokenType);
            }
            else if (collectibleData.dangerTokenType != DangerTokenType.None)
            {
                prefab = prefabMapping.GetDangerTokenPrefab(collectibleData.dangerTokenType);
            }

            if (prefab == null)
            {
                Debug.LogError($"No prefab found for {collectibleData.type}");
                return;
            }

            GameObject collectibleObject = Instantiate(prefab, collectibleParent);
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
            collectibleScript.activeOnStart = collectibleData.activeOnStart;
            collectibleScript.pointDisplay = collectibleData.pointDisplayType;
            
            collectibleObject.transform.SetParent(collectibleParent);
        }

        foreach (var starData in loadedLevelData.stars)
        {
            GameObject prefab = prefabMapping.GetStarPrefab();

            if (prefab == null)
            {
                Debug.LogError($"No prefab found for star");
                return;
            }

            GameObject starObject = Instantiate(prefab, starParent);
            starObject.transform.position = starData.position;

            Star starScript = starObject.GetComponent<Star>();
            starScript.index = starData.index;
        }
        
        if (loadedLevelData.portals != null && loadedLevelData.portals.Count > 0)
        {
            PortalPair[] allPortalPairs = portalsParent.GetComponentsInChildren<PortalPair>(true);

            int i;
            for (i = 0; i < loadedLevelData.portals.Count; i++)
            {
                LevelExporter.PortalSet portalSet = loadedLevelData.portals[i];
                PortalPair portalPair = allPortalPairs[i];
                portalPair.gameObject.SetActive(true);

                ApplyPortalData(portalPair.portalA, portalSet.portalA);
                ApplyPortalData(portalPair.portalB, portalSet.portalB);
            }

            for (; i < allPortalPairs.Length; i++)
            {
                allPortalPairs[i].gameObject.SetActive(false);
            }
        }
        
        foreach (LevelExporter.ObstacleData obstacleData in loadedLevelData.obstacles)
        {
            GameObject obstaclePrefab = prefabMapping.GetObstaclePrefab(obstacleData.type);

            if (obstaclePrefab == null)
            {
                Debug.LogWarning($"Failed to load obstacle of type {obstacleData.type}");
                continue;
            }
            GameObject obstacleObject = Instantiate(obstaclePrefab, collectibleParent);
            obstacleObject.transform.SetParent(obstacleData.positioning == Positioning.OnPlatform ? obstaclesParentPlatform : obstaclesParentWorld);
            obstacleObject.transform.position = obstacleData.position;
            obstacleObject.transform.rotation = obstacleData.rotation;

            Obstacle obstacleScript = obstacleObject.GetComponent<Obstacle>();
            if (obstacleScript != null)
            {
                obstacleScript.InitializeAndSetup(null, obstacleData);//TODO: Revisit this later if passing null is acceptable
            }
        }
        
        Debug.Log($"Level {levelNumber} loaded in Scene View!");
    }

    void ApplyPortalData(Portal portal, LevelExporter.PortalData portalData)
    {
        GameObject portalObject = portal.gameObject;
        Undo.RecordObject(portalObject.transform, "Move Portal");
        portalObject.transform.position = portalData.position;
        portalObject.transform.rotation = portalData.rotation;

        bool portalMoves = portalData.path != null && portalData.path.Length > 1;
        var cmScript = portalObject.GetComponent<ContinuousMovement>();

        if (portalMoves)
        {
            if (!cmScript) cmScript = Undo.AddComponent<ContinuousMovement>(portalObject);
            Undo.RecordObject(cmScript, "Modify Portal Movement");
            cmScript.pointA = portalData.path[0];
            cmScript.pointB = portalData.path[1];
        }
        else if (cmScript)
        {
            Undo.DestroyObjectImmediate(cmScript);
        }

        bool portalRotates = portalData.rotationSpeed != 0;
        var crScript = portalObject.GetComponent<ContinuousRotation>();

        if (portalRotates)
        {
            if (!crScript) crScript = Undo.AddComponent<ContinuousRotation>(portalObject);
            Undo.RecordObject(crScript, "Modify Portal Rotation");
            crScript.rotationAxis = portalData.rotationAxis;
            crScript.rotationSpeed = portalData.rotationSpeed;
        }
        else if (crScript)
        {
            Undo.DestroyObjectImmediate(crScript);
        }

        EditorUtility.SetDirty(portalObject);
    }
    
    private void SaveLevel()
    {
        if (loadedLevelData == null)
        {
            Debug.LogError("No level data loaded to save.");
            return;
        }

        loadedLevelData.collectibles.Clear();

        foreach (Transform collectible in collectibleParent)
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
                activeOnStart = collectibleScript.activeOnStart
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
        if (collectibleParent != null)
        {
            Debug.Log($"Deleting everything under {collectibleParent.gameObject.name}!");
            while (collectibleParent.childCount > 0)
            {
                Transform child = collectibleParent.GetChild(0);
                Debug.Log($"Deleting {child.gameObject.name}!");
                DestroyImmediate(child.gameObject);
            }
        }
    }
}
