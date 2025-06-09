using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class EndlessModeLoader : LevelLoader
{
    [Header("Randomized Level")]
    public Transform platformCenter;
    public EdgeDefinition[] hexGutterCupSides;
    public Transform[] SectorLinesX;
    public Transform[] SectorLinesZ;
    public EndlessLevelWalls wallLocations;
    public int maxObjects = 50;
    public GameObject gameModeObject;
    public EndlessLevelInstaller endlessLevelInstaller;
    
    private RandomizedHexStack randomizedHexStack;
    public RandomizedHexStack RandomizedHexStack
    {
        get
        {
            if (randomizedHexStack == null)
            {
                randomizedHexStack = gameObject.AddComponent<RandomizedHexStack>();
                randomizedHexStack.Initialize(this);
            }
            return randomizedHexStack;
        }
    }
    
    private RandomizedGutterWall randomizedGutterWall;
    private RandomizedGutterWall RandomizedGutterWall
    {
        get
        {
            if (randomizedGutterWall == null)
            {
                randomizedGutterWall = gameObject.AddComponent<RandomizedGutterWall>();
                randomizedGutterWall.Initialize(this);
                diContainer.Inject(randomizedGutterWall);
            }
            return randomizedGutterWall;
        }
    }
    
    private SectorGridHelper sectorGridHelper;
    public SectorGridHelper SectorGridHelper => sectorGridHelper;

    private EndlessSectorPopulator endlessSectorPopulator;
    private EndlessSectorPopulator EndlessSectorPopulator
    {
        get
        {
            if (endlessSectorPopulator == null)
            {
                endlessSectorPopulator = new EndlessSectorPopulator();
                endlessSectorPopulator.Initialize(this, SectorGridHelper);
            }
            return endlessSectorPopulator;
        }
    }
    
    private EndlessAreaPopulator endlessAreaPopulator;
    private EndlessAreaPopulator EndlessAreaPopulator
    {
        get
        {
            if (endlessAreaPopulator == null)
            {
                endlessAreaPopulator = new EndlessAreaPopulator();
                endlessAreaPopulator.Initialize(this, SectorGridHelper);
            }
            return endlessAreaPopulator;
        }
    }
    
    private SectorCoord sectorGridSize;
    private SectorCoord centerSector;
    private HashSet<SectorCoord> blacklistedSectors;
    private int difficulty = 1;
    public int Difficulty => difficulty;
    private const int TARGET_TO_DIFFICULTY_FACTOR = 75;
    private PinBehaviourPerTurn pinBehaviour = PinBehaviourPerTurn.Reset;
    private readonly List<SpawnedToken> tokenCache = new List<SpawnedToken>();

    private SectorCoord[] loneSectorsToFill;
    private SectorCoord[][] areasToFill;
    
    private GameMode currentGameMode;
    [Inject]
    DiContainer diContainer;
    
    [InjectOptional]
    SceneContext sceneContext;
    
    [Inject]
    private GameManager gameManager;
    [Inject]
    private ModeSelector modeSelector;
    [Inject]
    private PoolingManager poolingManager;
    
    private void Awake()
    {
        sectorGridSize = new SectorCoord(SectorLinesX.Length - 1, SectorLinesZ.Length - 1);
        centerSector = new SectorCoord(sectorGridSize.x / 2, sectorGridSize.z / 2);
        blacklistedSectors = new HashSet<SectorCoord>()
        {
            new SectorCoord(0, 0),
            new SectorCoord(0, sectorGridSize.z - 1),
            new SectorCoord(sectorGridSize.x - 1, sectorGridSize.z - 1),
            new SectorCoord(sectorGridSize.x - 1, 0),
        };
        
        sectorGridHelper = new SectorGridHelper(SectorLinesX, SectorLinesZ, sectorGridSize, blacklistedSectors);
    }

    public override int GetTargetPoints()
    {
        targetPoints = difficulty * TARGET_TO_DIFFICULTY_FACTOR;
        return targetPoints;
    }

    public override void LoadLevel()
    {
        EndlessGenerationSettings settings = modeSelector.EndlessGenerationSettings;
        if (settings.TryGetValue(RandomizerParameterType.Dificulty, out object diffSetting))
        {
            difficulty = (int)diffSetting;
            SaveManager.RecordEndlessPlayed(difficulty);
        }

        SetupPinBehaviour();
        EndlessModeSpawnEngine.Refresh();
        SectorLayoutEngine.Refresh();

        StartCoroutine(EndlessModeSetupProcess());
    }

    private IEnumerator EndlessModeSetupProcess()
    {
        float messageDurations = 0.25f;
        EventBus.Publish(new EndlessModeLoadingProgress("Zoning areas to spawn tokens in", 1));

        GetSectorsToFill(out loneSectorsToFill, out areasToFill);
        yield return new WaitForSeconds(messageDurations);
        
        EndlessSectorPopulator.PopulateSectors(loneSectorsToFill);
        EventBus.Publish(new EndlessModeLoadingProgress("Populating sectors on the grid", 2));
        yield return new WaitForSeconds(messageDurations);
        
        EndlessAreaPopulator.PopulateAreas(areasToFill);
        EventBus.Publish(new EndlessModeLoadingProgress("Populating larger areas on the grid", 3));
        yield return new WaitForSeconds(messageDurations);
        
        RandomizedGutterWall.Setup(Difficulty);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 
        EventBus.Publish(new EndlessModeLoadingProgress("Setting up walls", 4));
        yield return new WaitForSeconds(messageDurations);
        
        AssignPointsToTokens();
        EventBus.Publish(new EndlessModeLoadingProgress("Wrapping up...", 5));
        yield return new WaitForSeconds(messageDurations);

        SpawnMultipliersAndStars();
        EventBus.Publish(new EndlessModeLoadingProgress(true));
        gameManager.LevelSetupComplete();
    }

    public void SetupPinBehaviour()
    {
        EndlessGenerationSettings settings = modeSelector.EndlessGenerationSettings;
        if (settings.TryGetValue(RandomizerParameterType.PinBehavior, out object pbSetting))
        {
            int pinBehaviourIndex = (int)pbSetting;
            RandomizerEnum[] pinBehaviours = EndlessModePinBehaviour.Instance.pinBehaviours;
            pinBehaviour = pinBehaviours[pinBehaviourIndex].pinBehaviour;
            
            var oldDefault = FindFirstObjectByType<RegularMode>();
            if (oldDefault != null)
            {
                Destroy(oldDefault.gameObject);
            }
            
            EndlessGameMode endlessGameMode = gameModeObject.AddComponent<EndlessGameMode>();
            currentGameMode = endlessGameMode;
            
            switch (pinBehaviour)
            {
                case PinBehaviourPerTurn.Reset:
                    endlessGameMode.SetAsResetMode();
                    break;
                case PinBehaviourPerTurn.DisappearUponCollection:
                    endlessGameMode.SetAsDisappearingPinMode();
                    break;
            }
            
            diContainer.Inject(currentGameMode);
            diContainer
                .Rebind<GameMode>()
                .FromInstance(currentGameMode)
                .AsSingle()
                .NonLazy();
            
            endlessLevelInstaller.ReinjectAll();
        }

    }

    public void GetSectorsToFill(out SectorCoord[] loneSectors, out SectorCoord[][] areas)
    {
        int gridX = sectorGridSize.x;
        int gridZ = sectorGridSize.z;
        int totalSectors = gridX * gridZ;
        int availableCount = totalSectors - blacklistedSectors.Count;

        float difficultyFactor = difficulty / 10f;
        float fillPercent = Mathf.Lerp(0.25f, 0.95f, difficultyFactor);
        int targetCount = Mathf.RoundToInt(availableCount * fillPercent);

        int targetLone = targetCount / 2;
        int targetArea = targetCount - targetLone;

        int expansionRing = 0;
        List<int> currentXsInPlay = new List<int>(){centerSector.x};
        List<int> currentZsInPlay = new List<int>(){centerSector.z};
        HashSet<SectorCoord> effectiveGridSectors = new HashSet<SectorCoord>() { centerSector };
        bool expandAlongX = true;

        while (effectiveGridSectors.Count < targetCount)
        {
            if (expandAlongX)
            {
                int xLeft = Mathf.Clamp(centerSector.x - expansionRing, 0, gridX - 1);
                int xRight = Mathf.Clamp(centerSector.x + expansionRing, 0, gridX - 1);
                foreach (int z in currentZsInPlay)
                {
                    var c1 = new SectorCoord(xLeft, z);
                    if (!blacklistedSectors.Contains(c1) && !effectiveGridSectors.Contains(c1))
                        effectiveGridSectors.Add(c1);

                    var c2 = new SectorCoord(xRight, z);
                    if (!blacklistedSectors.Contains(c2) && !effectiveGridSectors.Contains(c2))
                        effectiveGridSectors.Add(c2);
                }
                currentXsInPlay.Add(xLeft);
                currentXsInPlay.Add(xRight);
            }
            else
            {
                int zFront = Mathf.Clamp(centerSector.z - expansionRing, 0, gridZ - 1);
                int zBack = Mathf.Clamp(centerSector.z + expansionRing, 0, gridZ - 1);
                foreach (int x in currentXsInPlay)
                {
                    var r1 = new SectorCoord(x, zFront);
                    if (!blacklistedSectors.Contains(r1) && !effectiveGridSectors.Contains(r1))
                        effectiveGridSectors.Add(r1);
                    
                    var r2 = new SectorCoord(x, zBack);
                    if (!blacklistedSectors.Contains(r2) && !effectiveGridSectors.Contains(r2))
                        effectiveGridSectors.Add(r2);
                }
                currentZsInPlay.Add(zFront);
                currentZsInPlay.Add(zBack);
                expansionRing++;
            }
            expandAlongX = !expandAlongX;
        }
        
        AreaBoundingCoord effectiveGrid = new AreaBoundingCoord(effectiveGridSectors.ToArray());
        
        var occupied = new HashSet<SectorCoord>(blacklistedSectors);
        var areaSizes = new List<Vector2Int>();
        var picker = new WeightBasedPicker<Vector2Int>();

        while (targetArea > 1)
        {
            picker.AddChoice(new Vector2Int(1, 2), 1f);
            picker.AddChoice(new Vector2Int(2, 1), 1f);

            if (targetArea >= 4)
            {
                picker.AddChoice(new Vector2Int(2, 2), 1f);
            }
            if (targetArea >= 6)
            {
                picker.AddChoice(new Vector2Int(2, 3), 0.5f);
                picker.AddChoice(new Vector2Int(3, 2), 0.5f);
            }
            
            if (targetArea >= 9)
            {
                picker.AddChoice(new Vector2Int(3, 3), 0.075f);
            }
            Vector2Int size = picker.Pick();
            int areaCount = size.x * size.y;
            
            if (areaCount > targetArea)
            {
                //Failsafe but this condition should actually never hit. Investigate if the execution comes here ever
                targetLone += targetArea;
                targetArea = 0;
                break;
            }

            areaSizes.Add(size);
            targetArea -= areaCount;
        }

        //Note to future self: Ideally targetArea here will be 0 and no transfer needs to happen, but just converting leftover area budget to lone sectors just in case
        targetLone += targetArea;
        targetArea = 0;

        var shuffledEffectiveGridSectors = new List<SectorCoord>(effectiveGridSectors);
        for (int i = 0; i < shuffledEffectiveGridSectors.Count; i++)
        {
            int j = Random.Range(i, shuffledEffectiveGridSectors.Count);
            (shuffledEffectiveGridSectors[i], shuffledEffectiveGridSectors[j]) = (shuffledEffectiveGridSectors[j], shuffledEffectiveGridSectors[i]);
        }
        
        var placedAreas = new List<SectorCoord[]>();
        var remainingAreas = new List<Vector2Int>(areaSizes);
        
        foreach (var baseCoord in shuffledEffectiveGridSectors)
        {
            if (remainingAreas.Count == 0)
                break;
            
            for (int remainingAreaIndex = 0; remainingAreaIndex < remainingAreas.Count; remainingAreaIndex++)
            {
                var area = remainingAreas[remainingAreaIndex];
                var coords = new List<SectorCoord>();
                bool valid = true;
                for (int dx = 0; dx < area.x && valid; dx++)
                {
                    for (int dz = 0; dz < area.y; dz++)
                    {
                        var currentSector = new SectorCoord(baseCoord.x + dx, baseCoord.z + dz);
                        if (!effectiveGridSectors.Contains(currentSector) || occupied.Contains(currentSector))
                        {
                            valid = false;
                            break;
                        }
                        coords.Add(currentSector);
                    }
                }
                if (valid)
                {
                    foreach (var c in coords)
                    {
                        occupied.Add(c);
                    }
                    placedAreas.Add(coords.ToArray());
                    remainingAreas.RemoveAt(remainingAreaIndex);
                    break;
                }
            }
        }
        List<SectorCoord> freeSectors = shuffledEffectiveGridSectors.Where(p => !occupied.Contains(p)).ToList();
        var loneList = freeSectors.Take(targetLone).ToList();
        // loneList = EvenlyDistributeSectors(loneList, placedAreas.ToArray(), gridX, gridZ, blacklistedSectors);

        loneSectors = loneList.ToArray();
        areas = placedAreas.ToArray();
    }

    public void SpawnPointToken(PointTokenType tokenType, Vector3 pos, Quaternion rot, SectorInfo sectorInfo)
    {
        var obj = poolingManager.GetObject(tokenType);
        obj.transform.SetParent(collectiblesParent, false);
        obj.transform.position = pos;
        obj.transform.rotation = rot;

        ContinuousMovement movementScript = obj.GetComponent<ContinuousMovement>();
        if (movementScript)
            Destroy(movementScript);

        ContinuousRotation rotationScript = obj.GetComponent<ContinuousRotation>();
        if (rotationScript)
            Destroy(rotationScript);

        targetPoints = difficulty * TARGET_TO_DIFFICULTY_FACTOR;
        
        if (obj.TryGetComponent<Collectible>(out var collectible))
        {
            tokenCache.Add(new SpawnedToken { collectible = collectible, sectorInfo = sectorInfo, position = pos });
            // collectible.InitializeAndSetup(GameManager.Context, 5, 1, Collectible.PointDisplayType.InBody);
        }
    }

    public void SpawnObstacle(ObstacleType obstacleType, Vector3 pos, Quaternion rot)
    {
        var obj = poolingManager.GetObject(obstacleType);
        obj.transform.SetParent(obstaclesParentPlatform, false);
        obj.transform.position = pos;
        obj.transform.rotation = rot;

        if (obj.TryGetComponent<Obstacle>(out var obstacle))
        {
            obstacle.InitializeAndSetup(gameManager.Context, CreateDataFor(obstacleType, pos, rot), gameManager.NumPlayersInGame);
        }
    }
    
    private LevelExporter.ObstacleData CreateDataFor(ObstacleType obstacleType, Vector3 pos, Quaternion rot)
    {
        LevelExporter.ObstacleData obstacleData = new LevelExporter.ObstacleData();
        obstacleData.movementSpeed = 0;
        obstacleData.position = pos;
        obstacleData.rotation = rot;
        obstacleData.type = obstacleType;

        switch (obstacleType)
        {
            case ObstacleType.ForcePad:
                bool makeItSwivel = Random.value > 0.6f;
                if (makeItSwivel)
                {
                    obstacleData.rotationAxis = Vector3.up;
                    obstacleData.rotationSpeed = 30;
                }
                break;
            
            case ObstacleType.Fan:
            case ObstacleType.SmallFan:
                obstacleData.rotationAxis = new Vector3(0, 0, 1);
                obstacleData.rotationSpeed = 45;
                break;
        }
        return obstacleData;
    }

    public void AssignPointsToTokens()
    {
        float obstacleWeight = 1f;
        float backWeight = 1.5f;
        int platformDepth = sectorGridSize.z - 1;
        
        var scoredByDifficultyToCollectList = tokenCache.Select(st => new {
                st.collectible,
                st.sectorInfo,
                Score = (st.sectorInfo.numObstacles * obstacleWeight) + ((st.position.z / platformDepth) * backWeight)})
                        .OrderByDescending(x => x.Score)
                        .ToList();
        
        int maxHighestValueHits = Mathf.Clamp((int)(difficulty / 1.25f), 1, 5);
        int highValueCount = Mathf.Min(maxHighestValueHits, scoredByDifficultyToCollectList.Count);
        int highValuePerHit = (int)(targetPoints / (highValueCount * 1.5f));//just an arbitrary factor because in a crowded space multiple tokens were being hit so this is an attempt to make highValuePerHit value lower
        
        int n = scoredByDifficultyToCollectList.Count;
        if (n == 0)
            return;

        float minVal = highValuePerHit / 8f;
        float maxVal = highValuePerHit;

        List<SectorCoord> sectorCoordsWithNegativePoints = GetSectorsWithNegativeValueTokens();
        HashSet<SectorCoord> sectorCoordsWithNegativePointsAssigned = new HashSet<SectorCoord>();
        float extraNegativeTokenProbability = 0;
        if (difficulty >= 4)
        {
            float t = (difficulty - 4f) / 6f;
            extraNegativeTokenProbability = Mathf.Lerp(0.05f, 0.25f, t);
        }

        for (int i = 0; i < n; i++)
        {
            float t = (n == 1) ? 1f : (1f - (float)i / (n - 1));
            float raw = Mathf.Lerp(minVal, maxVal, t);
            int points = Mathf.CeilToInt(raw / 5f) * 5;

            var collectible = scoredByDifficultyToCollectList[i].collectible;
            SectorInfo sector = scoredByDifficultyToCollectList[i].sectorInfo;
            bool isNegativeValueSector = sectorCoordsWithNegativePoints.Contains(sector.sectorCoord);
            bool negativeAlreadyAssigned = sectorCoordsWithNegativePointsAssigned.Contains(sector.sectorCoord);
            if (isNegativeValueSector && !negativeAlreadyAssigned)
            {
                points *= -1;
                sectorCoordsWithNegativePointsAssigned.Add(sector.sectorCoord);
            }
            else if (negativeAlreadyAssigned && Random.value <= extraNegativeTokenProbability)
            {
                points = -Mathf.Abs(points);
            }
            
            LevelExporter.CollectibleData collectibleData = new LevelExporter.CollectibleData();
            collectibleData.value = points;
            collectibleData.numTimesCanBeCollected = 1;
            collectibleData.pointDisplayType = Collectible.PointDisplayType.InBody;
            collectible.InitializeAndSetup(gameManager.Context, collectibleData);
        }
    }

    public void SpawnMultipliersAndStars()
    {
        float zMin = SectorLinesZ[1].position.z;
        float zMax = SectorLinesZ[SectorLinesZ.Length - 2].position.z;
        float difficultyFactor = difficulty / 10f;

        int numMultipliers = Mathf.Max(0, difficulty - 5);
        int numStars = Mathf.Clamp((difficulty / 2) - 2, 0, 3);
        int totalPositionsRequired = numStars + numMultipliers;
        if (totalPositionsRequired <= 0)
            return;
        
        List<Vector3> allPositions = SectorGridHelper.GetRandomIntersections(totalPositionsRequired, new Vector3(0, 20, 5));
        List<Vector3> starPositions = allPositions.GetRange(0, numStars);
        List<Vector3> multiplierPositions = allPositions.GetRange(numStars, allPositions.Count - numStars);

        int starIndex = 0;
        foreach (var starPosition in starPositions)
        {
            GameObject starObject = poolingManager.GetStar();
            if (starObject == null)
            {
                Debug.LogWarning($"Failed to load star");
                continue;
            }
            
            starObject.transform.SetParent(starsParent);
            starObject.transform.position = starPosition;
            Star starScript = starObject.GetComponent<Star>();
            starScript.index = starIndex;
            starIndex++;
        }
        
        foreach (var multiplierPosition in multiplierPositions)
        {
            GameObject collectibleObject = poolingManager.GetObject(MultiplierTokenType.CircularTrigger);
            collectibleObject.transform.SetParent(collectiblesParent);
            collectibleObject.transform.position = multiplierPosition;
            collectibleObject.transform.rotation = Quaternion.identity;
            collectibleObject.transform.localScale = 2 * Vector3.one;

            Collectible collectibleScript = collectibleObject.GetComponent<Collectible>();
            if (collectibleScript != null)
            {
                collectibleScript.OverrideDefaultLocalScale(2 * Vector3.one);
                float zNorm = Mathf.InverseLerp(zMin, zMax, multiplierPosition.z);
                float frontness = 1f - zNorm;
                float w2 = 1f + difficultyFactor * (1f - frontness);
                float w3 = 1f + difficultyFactor * 0.5f;
                float w4 = 1f + difficultyFactor * frontness * 0.3f;
                var picker = new WeightBasedPicker<int>();
                picker.AddChoice(2, w2);
                picker.AddChoice(3, w3);
                // picker.AddChoice(4, w4);
                int multiple = picker.Pick();

                LevelExporter.CollectibleData collectibleData = new LevelExporter.CollectibleData();
                collectibleData.value = multiple;
                collectibleData.numTimesCanBeCollected = 1;
                collectibleData.pointDisplayType = Collectible.PointDisplayType.InBody;

                collectibleScript.InitializeAndSetup(gameManager.Context, collectibleData);
            }

        }
    }

    public List<SectorCoord> GetSectorsWithNegativeValueTokens()
    {
        var allSectors = new List<SectorCoord>();
        
        if (loneSectorsToFill != null)
            allSectors.AddRange(loneSectorsToFill);
        
        if (areasToFill != null)
        {
            foreach (var area in areasToFill)
            {
                if (area != null)
                    allSectors.AddRange(area);
            }
        }

        float sectorPercent = 0;
        if (Difficulty >= 4)
        {
            float t = (difficulty - 4f) / 6f;
            sectorPercent = Mathf.Lerp(0.25f, 1f, t);
        }
        
        int totalSectors = allSectors.Count;
        int numToPick = Mathf.CeilToInt(sectorPercent * totalSectors);
        numToPick = Mathf.Clamp(numToPick, 0, totalSectors);
        
        var shuffledSectors = allSectors.OrderBy(_ => Random.value).ToList();
        return shuffledSectors.Take(numToPick).ToList();
    }
}