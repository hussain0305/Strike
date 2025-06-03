using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GauntletLoader : LevelLoader
{
    [Inject]
    private ModeSelector modeSelector;
    [Inject]
    private PoolingManager poolingManager;
    [Inject]
    private GameManager gameManager;
    
    public override void LoadLevel()
    {
        if (poolingManager == null)
        {
            Debug.LogError("PoolingManager is not assigned.");
            return;
        }

        int levelNumber = 1;
        GameModeType gameMode = GameModeType.Pins;
        if (modeSelector)
        {
            levelNumber = modeSelector.GetSelectedLevel();
            gameMode = modeSelector.GetSelectedGameMode();
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

        SpawnCollectibles(levelData.collectibles);
        SpawnStars(levelData.stars);
        SpawnPortals(levelData.portals);
        SpawnObstacle(levelData.obstacles);
        
        if (levelData.platformRotationSpeed != 0f)
        {
            EventBus.Publish(new LevelPlatformHasRotation(levelData.platformRotationSpeed));
        }
        
        Debug.Log($"Level {levelNumber} loaded successfully!");
        gameManager.LevelSetupComplete();
    }

    public void SpawnCollectibles(List<LevelExporter.CollectibleData> allCollectibleData)
    {
        List<GameObject> spawnedDangerObjects = new List<GameObject>();
        int highestActiveDangerPinIndex = -1;
        int currentDangerPinIndex = 0;

        foreach (var collectibleData in allCollectibleData)
        {
            GameObject collectibleObject = null;
            if (collectibleData.pointTokenType != PointTokenType.None)
                collectibleObject = poolingManager.GetObject(collectibleData.pointTokenType);
            else if (collectibleData.multiplierTokenType != MultiplierTokenType.None)
                collectibleObject = poolingManager.GetObject(collectibleData.multiplierTokenType);
            else if (collectibleData.dangerTokenType != DangerTokenType.None)
                collectibleObject = poolingManager.GetObject(collectibleData.dangerTokenType);

            if (collectibleObject == null)
            {
                Debug.LogWarning($"Failed to load collectible of type {collectibleData.type}");
                return;
            }
            
            collectibleObject.transform.SetParent(collectiblesParent);
            collectibleObject.transform.position = collectibleData.position;
            collectibleObject.transform.rotation = collectibleData.rotation;
            collectibleObject.transform.localScale = Vector3.one;

            // CheckForContinuousMovement(collectibleObject, collectibleData.path, collectibleData.movementSpeed);
            // CheckForContinuousRotation(collectibleObject, collectibleData.rotationAxis, collectibleData.rotationSpeed);

            Rigidbody rBody = collectibleObject.GetComponent<Rigidbody>();
            //TODO: Dirty 'if' below. See if there's time to retrospectively fix existing level data with multi-tokens gravity info and remove this check, just make it universal for all types
            if (collectibleData.pointTokenType != PointTokenType.None)
            {
                rBody.isKinematic = collectibleData.isKinematic;
            }
            
            Collectible collectibleScript = collectibleObject.GetComponent<Collectible>();
            if (collectibleScript != null)
            {
                collectibleScript.OverrideDefaultLocalScale(Vector3.one);
                collectibleScript.InitializeAndSetup(gameManager.Context, collectibleData);
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
        EventBus.Publish(new InitializeDangerTokens(spawnedDangerObjects.ToArray(), highestActiveDangerPinIndex, gameManager.NumPlayersInGame));
    }

    public void SpawnStars(List<LevelExporter.StarData> stars)
    {
        SaveManager.GetStarsCollectedStatus(modeSelector.GetSelectedGameMode(),
            modeSelector.GetSelectedLevel(), out bool[] starStatus);
        foreach (LevelExporter.StarData starData in stars)
        {
            if (starStatus[starData.index])
            {
                //Don't spawn the star if it was already collected
                continue;
            }
            GameObject starObject = poolingManager.GetStar();
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
    }
    
    public void SpawnObstacle(List<LevelExporter.ObstacleData> allObstacleData)
    {
        foreach (LevelExporter.ObstacleData obstacleData in allObstacleData)
        {
            GameObject obstacleObject = poolingManager.GetObject(obstacleData.type);

            if (obstacleObject == null)
            {
                Debug.LogWarning($"Failed to load collectible of type {obstacleData.type}");
                continue;
            }

            obstacleObject.transform.SetParent(obstacleData.positioning == Positioning.OnPlatform ? obstaclesParentPlatform : obstaclesParentWorld);
            obstacleObject.transform.position = obstacleData.position;
            obstacleObject.transform.rotation = obstacleData.rotation;
            
            // CheckForContinuousMovement(obstacleObject, obstacleData.path, obstacleData.movementSpeed);
            // CheckForContinuousRotation(obstacleObject, obstacleData.rotationAxis, obstacleData.rotationSpeed);
            
            Obstacle obstacleScript = obstacleObject.GetComponent<Obstacle>();
            if (obstacleScript != null)
            {
                obstacleScript.InitializeAndSetup(gameManager.Context, obstacleData, gameManager.NumPlayersInGame);
            }
        }
    }

    public void SpawnPortals(List<LevelExporter.PortalSet> portals)
    {
        PortalPair[] allPortalPairs = portalsParent.GetComponentsInChildren<PortalPair>(true);
        int i;
        for (i = 0; i < portals.Count; i++)
        {
            LevelExporter.PortalSet portalSet = portals[i];
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
    
    void ApplyPortalData(Portal portal, LevelExporter.PortalData portalData)
    {
        GameObject portalObject = portal.gameObject;
        portalObject.transform.position = portalData.position;
        portalObject.transform.rotation = portalData.rotation;

        CheckForContinuousMovement(portalObject, portalData.path, portalData.movementSpeed);
        CheckForContinuousRotation(portalObject, portalData.rotationAxis, portalData.rotationSpeed);
    }
    
    public override int GetTargetPoints()
    {
        return targetPoints;
    }

}
