using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

[System.Serializable]
public struct EdgeDefinition
{
    public Transform end1;
    public Transform end2;
}

public struct AreaWorldBound
{
    public float xMin;
    public float xMax;
    public float zMin;
    public float zMax;

    public AreaWorldBound(float _xMin, float _xMax, float _zMin, float _zMax)
    {
        xMin = _xMin;
        xMax = _xMax;
        zMin = _zMin;
        zMax = _zMax;
    }

    public Vector3 Center()
    {
        return new Vector3((xMin + xMax) / 2, 0, (zMin + zMax) / 2);
    }
}

public struct SectorInfo
{
    SectorCoord sectorCoord;
    public int numPointTokens;
    public int numObstacles;

    public SectorInfo(SectorCoord _sectorCoord, SectorSpawnPayload payload)
    {
        sectorCoord = _sectorCoord;
        numPointTokens = 0;
        numObstacles = 0;
        
        if (payload == null)
            return;
        
        foreach (var entry in payload.Entries)
        {
            if (entry.pointTokenType != PointTokenType.None)
            {
                numPointTokens++;
            }
            if (entry.obstacleType != ObstacleType.None)
            {
                numObstacles++;
            }
        }
    }
}

public struct SectorCoord
{
    public int x;
    public int z;

    public SectorCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }
    
    public bool Equals(SectorCoord other) => x == other.x && z == other.z;

    public override bool Equals(object obj)=> obj is SectorCoord other && Equals(other);

    public override int GetHashCode() => System.HashCode.Combine(x, z);

    public static bool operator ==(SectorCoord a, SectorCoord b) => a.Equals(b);

    public static bool operator !=(SectorCoord a, SectorCoord b) => !a.Equals(b);
}

public struct AreaBoundingCoord
{
    public int xMin;
    public int xMax;
    public int zMin;
    public int zMax;

    public int xDimension;
    public int zDimension;

    public AreaBoundingCoord(int _xMin, int _zMin, int _xMax, int _zMax)
    {
        xMin = _xMin;
        xMax = _xMax;
        zMin = _zMin;
        zMax = _zMax;
        
        xDimension = xMax - xMin;
        zDimension = zMax - zMin;
    }
    
    public AreaBoundingCoord(SectorCoord[] sectors)
    {
        xMin = sectors.Min(s => s.x);
        xMax = sectors.Max(s => s.x);
        zMin = sectors.Min(s => s.z);
        zMax = sectors.Max(s => s.z);
        
        xDimension = xMax - xMin;
        zDimension = zMax - zMin;
    }

    public bool IsSquare()
    {
        return xDimension == zDimension;
    }

    public SectorCoord CenterCoord()
    {
        return new SectorCoord((xMin + xMax) / 2, (zMin + zMax) / 2);
    }
}

[System.Serializable]
public struct EndlessLevelWalls
{
    public Transform LeftSlat;
    public Transform[] LeftWall;
    public Transform[] LeftCap;
    public Transform[] RightCap;
    public Transform[] RightWall;
    public Transform RightSlat;
}

class SpawnedToken
{
    public Collectible collectible;
    public SectorInfo sectorInfo;
    public Vector3 position;
}

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
    private HashSet<SectorCoord> blacklistedSectors;
    private int difficulty = 1;
    public int Difficulty => difficulty;
    private PinBehaviourPerTurn pinBehaviour = PinBehaviourPerTurn.Reset;
    private readonly List<SpawnedToken> tokenCache = new List<SpawnedToken>();
    
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
        targetPoints = difficulty * 50;
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
        SpawnPayloadEngine.Refresh();
        SectorLayoutEngine.Refresh();

        GetSectorsToFill(out SectorCoord[] loneSectorsToFill, out SectorCoord[][] areasToFill);
        
        EndlessSectorPopulator.PopulateSectors(loneSectorsToFill);
        EndlessAreaPopulator.PopulateAreas(areasToFill);
        RandomizedGutterWall.Setup(Difficulty);

        {
            string s = "Spawning in lone sectors ";
            foreach (var ss in loneSectorsToFill)
            {
                s += $" ({ss.x},{ss.z}),";
            }
            Debug.Log(s);
        }
        
        AssignPointsToTokens();
        SpawnMultipliersAndStars();
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
                Debug.Log(">>> DELETING OLD MODE!!");
                Destroy(oldDefault.gameObject);
            }
            
            switch (pinBehaviour)
            {
                case PinBehaviourPerTurn.Reset:
                    currentGameMode = gameModeObject.AddComponent<RegularMode>();
                    break;
                case PinBehaviourPerTurn.DisappearUponCollection:
                    currentGameMode = gameModeObject.AddComponent<DisappearingMode>();
                    break;
            }
            
            diContainer.Inject(currentGameMode);
            diContainer
                .Rebind<GameMode>()
                .FromInstance(currentGameMode)
                .AsSingle()
                .NonLazy();
            
            Debug.Log(">>> REBOUND!!");
            if (diContainer == sceneContext.Container)
            {
                Debug.Log(">>> I Got the scene context!!");
            }
            
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
        float minLoneChance = 0.4f;
        float loneChance    = Mathf.Lerp(minLoneChance, 1f, difficultyFactor);
        float fillPercent = Mathf.Lerp(0.25f, 0.95f, difficultyFactor);
        int targetCount = Mathf.RoundToInt(availableCount * fillPercent);

        var occupied = new HashSet<SectorCoord>();
        var loneList = new List<SectorCoord>();
        var areaList = new List<SectorCoord[]>();
        
        int spanZ = Mathf.Max(2, Mathf.Min(gridZ, Mathf.CeilToInt(Mathf.Sqrt(targetCount))));
        int spanX = Mathf.Max(2, Mathf.Min(gridX, Mathf.CeilToInt((float)targetCount / spanZ)));

        int startX = (gridX - spanX) / 2;
        int startZ = (gridZ - spanZ) / 2;
        
        for (int secCoordX = startX; secCoordX < startX + spanX; secCoordX++)
        {
            if (occupied.Count >= targetCount)
                break;
            
            for (int secCoordZ = startZ; secCoordZ < startZ + spanZ; secCoordZ++)
            {
                if (occupied.Count >= targetCount)
                    break;
                
                SectorCoord thisSector = new SectorCoord(secCoordX, secCoordZ);
                if (blacklistedSectors.Contains(thisSector))
                {
                    continue;
                }

                int maxW = sectorGridSize.x - secCoordX;
                int maxH = sectorGridSize.z - secCoordZ;
                bool canSpawnArea = maxW > 1 || maxH > 1;
                bool tryToSpawnArea = canSpawnArea && Random.value < (difficultyFactor / 2);
                
                if (tryToSpawnArea)
                {
                    int w = Random.Range(1, maxW + 1);
                    int hMin = (w == 1) ? 2 : 1;
                    int h = Random.Range(hMin, maxH + 1);
                    
                    int rectSize = w * h;
                    if (occupied.Count + rectSize <= targetCount)
                    {
                        SectorCoord[] thisArea = new SectorCoord[rectSize];
                        bool bad = false;
                        int index = 0;
                        for (int dx = 0; dx < w && !bad; dx++)
                        {
                            for (int dz = 0; dz < h; dz++)
                            {
                                SectorCoord areaSector = new SectorCoord(secCoordX + dx, secCoordZ + dz);
                                if (blacklistedSectors.Contains(areaSector) || occupied.Contains(areaSector))
                                {
                                    bad = true;
                                    break;
                                }
                                thisArea[index] = areaSector;
                                index++;
                            }
                        }
                        if (!bad)
                        {
                             if (SectorGridHelper.IsValidArea(thisArea) && SectorGridHelper.IsAreaContainedOnGrid(thisArea))
                             {
                                 foreach (var c in thisArea)
                                     occupied.Add(c);
                                 areaList.Add(thisArea.ToArray());
                                 continue;
                             }
                        }
                    }
                }
                
                bool tryToGetLoneSector = Random.value < loneChance && occupied.Count < targetCount;
                if (!tryToGetLoneSector)
                {
                    continue;
                }

                SectorCoord loneSector = new SectorCoord(secCoordX, secCoordZ);
                if (!blacklistedSectors.Contains(loneSector) && !occupied.Contains(loneSector))
                {
                    occupied.Add(loneSector);
                    loneList.Add(loneSector);
                }
            }
        }
        
        loneSectors = loneList.ToArray();
        areas = areaList.ToArray();
    }

    public void SpawnPointToken(PointTokenType tokenType, Vector3 pos, Quaternion rot, SectorInfo sectorInfo)
    {
        var obj = poolingManager.GetObject(tokenType);
        obj.transform.SetParent(collectiblesParent, false);
        obj.transform.position = pos;
        obj.transform.rotation = rot;

        targetPoints = difficulty * 50;
        
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
            LevelExporter.ObstacleData obstacleData = new LevelExporter.ObstacleData();
            obstacleData.movementSpeed = 0;
            obstacleData.position = pos;
            obstacleData.rotation = rot;
            obstacleData.type = obstacleType;
            if (obstacleType == ObstacleType.SmallFan || obstacleType == ObstacleType.Fan)
            {
                obstacleData.rotationAxis = new Vector3(0, 0, 1);
                obstacleData.rotationSpeed = 360;
            }
            obstacle.InitializeAndSetup(gameManager.Context, obstacleData);
        }
    }

    public void AssignPointsToTokens()
    {
        float obstacleWeight = 1f;
        float backWeight = 1f;
        int platformDepth = sectorGridSize.z - 1;
        
        var scoredByDifficultyToCollectList = tokenCache.Select(st => new {
                st.collectible,
                Score = (st.sectorInfo.numObstacles * obstacleWeight) + ((st.position.z / platformDepth) * backWeight)})
            .OrderByDescending(x => x.Score)
            .ToList();
        
        int maxHighestValueHits = Mathf.Clamp(difficulty, 1, 4);
        int highValueCount = Mathf.Min(maxHighestValueHits, scoredByDifficultyToCollectList.Count);
        int highValuePerHit = targetPoints / highValueCount;
        
        int n = scoredByDifficultyToCollectList.Count;
        if (n == 0) return;

        float minVal = highValuePerHit / 5f;
        float maxVal = highValuePerHit;

        for (int i = 0; i < n; i++)
        {
            float t = (n == 1) ? 1f : (1f - (float)i / (n - 1));
            float raw = Mathf.Lerp(minVal, maxVal, t);
            int points = Mathf.CeilToInt(raw / 5f) * 5;

            var collectible = scoredByDifficultyToCollectList[i].collectible;
            collectible.InitializeAndSetup(gameManager.Context, points, 1, Collectible.PointDisplayType.InBody);
        }
    }

    public void SpawnMultipliersAndStars()
    {
        float zMin = SectorLinesZ[1].position.z;
        float zMax = SectorLinesZ[SectorLinesZ.Length - 2].position.z;
        float difficultyFactor = difficulty / 10f;

        int numMultipliers = Mathf.Max(0, difficulty - 5);
        int numStars = Mathf.Clamp((difficulty - 6) / 2 + 1, 1, 3);
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
                var picker = new WeightedRandomPicker<int>();
                picker.AddChoice(2, w2);
                picker.AddChoice(3, w3);
                // picker.AddChoice(4, w4);
                int multiple = picker.Pick();

                collectibleScript.InitializeAndSetup(gameManager.Context, multiple, 1, Collectible.PointDisplayType.InBody);
            }

        }
    }
}