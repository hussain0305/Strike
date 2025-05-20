using System.Collections.Generic;
using UnityEngine;

public class GauntletLoader : LevelLoader
{
    public override void LoadLevel()
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

        List<GameObject> spawnedDangerObjects = new List<GameObject>();
        int highestActiveDangerPinIndex = -1;
        int currentDangerPinIndex = 0;
        
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

            else if (collectibleData.dangerTokenType != DangerTokenType.None)
            {
                collectibleObject = PoolingManager.Instance.GetObject(collectibleData.dangerTokenType);
            }

            if (collectibleObject == null)
            {
                Debug.LogWarning($"Failed to load collectible of type {collectibleData.type}");
                continue;
            }
            
            collectibleObject.transform.SetParent(collectiblesParent);
            collectibleObject.transform.position = collectibleData.position;
            collectibleObject.transform.rotation = collectibleData.rotation;
            collectibleObject.transform.localScale = Vector3.one;

            Collectible collectibleScript = collectibleObject.GetComponent<Collectible>();
            if (collectibleScript != null)
            {
                collectibleScript.OverrideDefaultLocalScale(Vector3.one);
                collectibleScript.InitializeAndSetup(GameManager.Context, collectibleData.value, collectibleData.numTimesCanBeCollected, collectibleData.pointDisplayType);
            }
            bool collectibleMoves = collectibleData.path != null && collectibleData.path.Length > 1;
            ContinuousMovement cmScript = collectibleObject.GetComponent<ContinuousMovement>();

            if (collectibleMoves)
            {
                if (!cmScript)
                    cmScript = collectibleObject.AddComponent<ContinuousMovement>();

                cmScript.pointA = collectibleData.path[0];
                cmScript.pointB = collectibleData.path[1];
                cmScript.speed  = collectibleData.movementSpeed;
                
                Rigidbody rBody = collectibleObject.GetComponent<Rigidbody>();
                rBody.isKinematic = true;
            }
            else if (cmScript)
            {
                Destroy(cmScript);
            }
            
            DangerToken dangerScript = collectibleObject.GetComponent<DangerToken>();
            if (dangerScript)
            {
                dangerScript.dangerTokenIndex = currentDangerPinIndex;
                spawnedDangerObjects.Add(collectibleObject);
                if (collectibleData.activeOnStart && currentDangerPinIndex > highestActiveDangerPinIndex)
                {
                    highestActiveDangerPinIndex = currentDangerPinIndex;
                }
                currentDangerPinIndex++;
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

        PortalPair[] allPortalPairs = portalsParent.GetComponentsInChildren<PortalPair>(true);
        int i;
        for (i = 0; i < levelData.portals.Count; i++)
        {
            LevelExporter.PortalSet portalSet = levelData.portals[i];
            PortalPair portalPair = allPortalPairs[i];
            portalPair.gameObject.SetActive(true);

            ApplyPortalData(portalPair.portalA, portalSet.portalA);
            ApplyPortalData(portalPair.portalB, portalSet.portalB);
        }

        for (; i < allPortalPairs.Length; i++)
        {
            allPortalPairs[i].gameObject.SetActive(false);
        }

        foreach (LevelExporter.ObstacleData obstacleData in levelData.obstacles)
        {
            GameObject obstacleObject = PoolingManager.Instance.GetObject(obstacleData.type);

            if (obstacleObject == null)
            {
                Debug.LogWarning($"Failed to load collectible of type {obstacleData.type}");
                continue;
            }

            obstacleObject.transform.SetParent(obstacleData.positioning == Positioning.OnPlatform ? obstaclesParentPlatform : obstaclesParentWorld);
            obstacleObject.transform.position = obstacleData.position;
            obstacleObject.transform.rotation = obstacleData.rotation;

            Obstacle obstacleScript = obstacleObject.GetComponent<Obstacle>();
            if (obstacleScript != null)
            {
                obstacleScript.InitializeAndSetup(GameManager.Context, obstacleData);
            }
        }
        
        if (levelData.platformRotationSpeed != 0f)
        {
            EventBus.Publish(new LevelPlatformHasRotation(levelData.platformRotationSpeed));
        }
        
        EventBus.Publish(new InitializeDangerTokens(spawnedDangerObjects.ToArray(), highestActiveDangerPinIndex));
        Debug.Log($"Level {levelNumber} loaded successfully!");
    }
        
    void ApplyPortalData(Portal portal, LevelExporter.PortalData portalData)
    {
        GameObject portalObject = portal.gameObject;
        portalObject.transform.position = portalData.position;
        portalObject.transform.rotation = portalData.rotation;

        bool portalMoves = portalData.path != null && portalData.path.Length > 1;
        var cmScript = portalObject.GetComponent<ContinuousMovement>();

        if (portalMoves)
        {
            if (!cmScript) cmScript = portalObject.AddComponent<ContinuousMovement>();
            cmScript.pointA = portalData.path[0];
            cmScript.pointB = portalData.path[1];
            cmScript.speed  = portalData.movementSpeed;
        }
        else if (cmScript)
        {
            Destroy(cmScript);
        }

        bool portalRotates = portalData.rotationSpeed != 0;
        var crScript = portalObject.GetComponent<ContinuousRotation>();

        if (portalRotates)
        {
            if (!crScript) crScript = portalObject.AddComponent<ContinuousRotation>();
            crScript.rotationAxis = portalData.rotationAxis;
            crScript.rotationSpeed = portalData.rotationSpeed;
        }
        else if (crScript)
        {
            Destroy(crScript);
        }
    }
    
    public int GetTargetPoints()
    {
        return targetPoints;
    }

}
