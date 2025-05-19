using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        return difficulty * 50;
    }

    public override void LoadLevel()
    {
        EndlessGenerationSettings settings = ModeSelector.Instance.EndlessGenerationSettings;
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
    }

    public void SetupPinBehaviour()
    {
        EndlessGenerationSettings settings = ModeSelector.Instance.EndlessGenerationSettings;
        if (settings.TryGetValue(RandomizerParameterType.PinBehavior, out object pbSetting))
        {
            int pinBehaviourIndex = (int)pbSetting;
            RandomizerEnum[] pinBehaviours = EndlessModePinBehaviour.Instance.pinBehaviours;
            pinBehaviour = pinBehaviours[pinBehaviourIndex].pinBehaviour;
            
            switch (pinBehaviour)
            {
                case PinBehaviourPerTurn.Reset:
                    gameModeObject.AddComponent<RegularMode>();
                    break;
                case PinBehaviourPerTurn.DisappearUponCollection:
                    gameModeObject.AddComponent<DisappearingMode>();
                    break;
            }
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

    public void SpawnPointToken(PointTokenType tokenType, Vector3 pos, Quaternion rot)
    {
        var obj = PoolingManager.Instance.GetObject(tokenType);
        obj.transform.SetParent(collectiblesParent, false);
        obj.transform.position = pos;
        obj.transform.rotation = rot;

        if (obj.TryGetComponent<Collectible>(out var collectible))
        {
            collectible.InitializeAndSetup(GameManager.Context, 5, 1, Collectible.PointDisplayType.InBody);
        }
    }

    public void SpawnObstacle(ObstacleType obstacleType, Vector3 pos, Quaternion rot)
    {
        var obj = PoolingManager.Instance.GetObject(obstacleType);
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
            obstacle.InitializeAndSetup(GameManager.Context, obstacleData);
        }
    }
}