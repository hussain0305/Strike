using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    private Vector3 xSpacing = new Vector3(0.2f, 0, 0);
    private Vector3 ySpacing = new Vector3(0, 0.2f, 0);
    private Vector3 zSpacing = new Vector3(0, 0, 0.2f);

    private RandomizedHexStack randomizedHexStack;
    private RandomizedHexStack RandomizedHexStack
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

    private SectorCoord sectorGridSize;
    private List<SectorCoord> blacklistedSectors;
    private int difficulty = 1;
    private Dictionary<PointTokenType, Vector3> hexPins;
    private void Awake()
    {
        sectorGridSize = new SectorCoord(SectorLinesX.Length - 1, SectorLinesZ.Length - 1);
        blacklistedSectors = new List<SectorCoord>()
        {
            new SectorCoord(0, 0),
            new SectorCoord(0, sectorGridSize.z - 1),
            new SectorCoord(sectorGridSize.x - 1, sectorGridSize.z - 1),
            new SectorCoord(sectorGridSize.x - 1, 0),
        };
        
        hexPins = new Dictionary<PointTokenType, Vector3>();
        hexPins.Add(PointTokenType.Pin_1x, CollectiblePrefabMapping.Instance.GetPointTokenDimension(PointTokenType.Pin_1x));
        hexPins.Add(PointTokenType.Pin_2x, CollectiblePrefabMapping.Instance.GetPointTokenDimension(PointTokenType.Pin_2x));
        hexPins.Add(PointTokenType.Pin_4x, CollectiblePrefabMapping.Instance.GetPointTokenDimension(PointTokenType.Pin_4x));
    }

    public override int GetTargetPoints()
    {
        return 10;
    }

    override public void LoadLevel()
    {
        EndlessGenerationSettings settings = ModeSelector.Instance.EndlessGenerationSettings;
        if (settings.TryGetValue(RandomizerParameterType.Dificulty, out object setting))
        {
            difficulty = (int)setting;
            Debug.Log(">>> Difficulty retrieved to be " + difficulty);
        }
        
        GetSectorsToFill(out SectorCoord[] loneSectorsToFill, out SectorCoord[][] areasToFill);
        foreach (var loneSector in loneSectorsToFill)
        {
            PopulateSector(loneSector);
        }

        foreach (SectorCoord[] areaToFill in areasToFill)
        {
            PopulateArea(areaToFill);
        }
        
        RandomizedGutterWall.SpawnOnSides(4, 3, RandomizedMovementOptions.SomeMoving, new IncludeTypesInRandomization(true, true));
    }

    public void PopulateSector(SectorCoord sector)
    {
        //Re-instate check if needed, commenting it out right now because the info coming through current channels is pre-vetted
        // if (!IsSectorContainedOnGrid(sector))
        // {
        //     return;
        // }
        
    }
    
    public void PopulateArea(SectorCoord[] sectors)
    {
        //Re-instate check if needed, commenting it out right now because the info coming through current channels is pre-vetted
        // if (!IsAreaContainedOnGrid(area))
        // {
        //     return;
        // }

        AreaBoundingCoord areaBoundingCoord = new AreaBoundingCoord(sectors);
        bool spawnHexStack = areaBoundingCoord.IsSquare(); //and other conditions
        if (spawnHexStack)
        {
            SpawnHexStack(sectors);
        }
    }
    
    public void SpawnHexStack(SectorCoord[] sectors)
    {
        AreaWorldBound area;
        if (!GetAreaBounds(sectors, out area))
        {
            return;
        }

        {
            string s = "Spawning Hex Stacks in area of sectors ";
            foreach (var ss in sectors)
            {
                s += $" ({ss.x},{ss.z}),";
            }
            Debug.Log(s + " | center of this area is " + area.Center());
        }
        
        List<PointTokenType> possibleTokens = new List<PointTokenType>();
            
        float xLength = area.xMax - area.xMin;
        float zLength = area.zMax - area.zMin;

        foreach (PointTokenType token in hexPins.Keys)
        {
            if (hexPins[token].x < xLength && hexPins[token].z < zLength)
            {
                possibleTokens.Add(token);
            }
        }

        if (possibleTokens.Count == 0)
        {
            Debug.Log($">>> No possible tokens to spawn in area ({xLength}, {zLength})");
            return;
        }

        PointTokenType selectedToken = possibleTokens[Random.Range(0, possibleTokens.Count)];

        Vector3 dimensions = hexPins[selectedToken];

        int numRings = Mathf.FloorToInt(Mathf.Min(xLength / (2 * dimensions.x), zLength / (2 * dimensions.z)));
        int numLevels = Random.Range(0, numRings);
            
        RandomizedHexStack.SpawnHexStacksWithCenter(selectedToken, area.Center() + ySpacing, dimensions.x / 2, dimensions.y, 
            xSpacing.x, numRings, numLevels, collectiblesParent);
    }
    
    public void GetSectorsToFill(out SectorCoord[] loneSectors, out SectorCoord[][] areas)
    {
        int gridX = sectorGridSize.x;
        int gridZ = sectorGridSize.z;
        int totalSectors = gridX * gridZ;

        var blacklist = new HashSet<(int x, int z)>(
            blacklistedSectors.Select(b => (b.x, b.z))
        );
        int availableCount = totalSectors - blacklist.Count;

        float t = (difficulty - 1) / 9f;
        float fillPct = Mathf.Lerp(0.10f, 0.90f, t);
        int targetCount = Mathf.RoundToInt(availableCount * fillPct);

        var chosen = new HashSet<(int x, int z)>();
        var loneList = new List<SectorCoord>();
        var areaList = new List<SectorCoord[]>();

        int attempts = 0;
        int maxAttempts = availableCount * 5;

        while (chosen.Count < targetCount && attempts++ < maxAttempts)
        {
            if (Random.value < t && chosen.Count < targetCount)
            {
                int w = Random.Range(2, gridX + 1);
                int h = Random.Range(2, gridZ + 1);
                
                if (w == gridX && h == gridZ)
                    continue;

                int rectSize = w * h;
                if (chosen.Count + rectSize > targetCount)
                    continue;

                int ox = Random.Range(0, gridX - w + 1);
                int oz = Random.Range(0, gridZ - h + 1);

                var cells = new List<(int x, int z)>(rectSize);
                bool bad = false;
                for (int dx = 0; dx < w && !bad; dx++)
                {
                    for (int dz = 0; dz < h; dz++)
                    {
                        var c = (ox + dx, oz + dz);
                        if (blacklist.Contains(c) || chosen.Contains(c))
                        {
                            bad = true;
                            break;
                        }
                        cells.Add(c);
                    }
                }

                if (!bad)
                {
                    foreach (var c in cells)
                        chosen.Add(c);
                    areaList.Add(cells.Select(c => new SectorCoord(c.x, c.z)).ToArray());
                    continue;
                }
            }

            int x = Random.Range(0, gridX);
            int z = Random.Range(0, gridZ);
            var coord = (x, z);
            if (!blacklist.Contains(coord) && !chosen.Contains(coord))
            {
                chosen.Add(coord);
                loneList.Add(new SectorCoord(x, z));
            }
        }

        loneSectors = loneList.ToArray();
        areas = areaList.ToArray();
    }
    
    public bool GetSectorBounds(SectorCoord sector, out AreaWorldBound areaWorldBound)
    {
        if (!IsSectorContainedOnGrid(sector))
        {
            areaWorldBound = new AreaWorldBound();
            return false;
        }
        
        areaWorldBound = new AreaWorldBound(
            SectorLinesX[sector.x].transform.position.x,
            SectorLinesX[sector.x + 1].transform.position.x,
            SectorLinesZ[sector.z].transform.position.z,
            SectorLinesZ[sector.z + 1].transform.position.z
            );
        return true;
    }
    
    public bool GetAreaBounds(SectorCoord[] sectors, out AreaWorldBound areaWorldBound)
    {
        if (!IsValidArea(sectors) || !IsAreaContainedOnGrid(sectors))
        {
            areaWorldBound = new AreaWorldBound();
            return false;
        }

        AreaBoundingCoord area = new AreaBoundingCoord(sectors);
        
        areaWorldBound = new AreaWorldBound(
            SectorLinesX[area.xMin].transform.position.x,
            SectorLinesX[area.xMax + 1].transform.position.x,
            SectorLinesZ[area.zMin].transform.position.z,
            SectorLinesZ[area.zMax + 1].transform.position.z
        );
        return true;
    }

    public bool IsSectorContainedOnGrid(SectorCoord sector)
    {
        if (sector.x < 0 || sector.x >= sectorGridSize.x) 
            return false;
        if (sector.z < 0 || sector.z >= sectorGridSize.z)
            return false;
        return true;
    }

    public bool IsAreaContainedOnGrid(SectorCoord[] sectors)
    {
        foreach (var sector in sectors)
        {
            if (!IsSectorContainedOnGrid(sector))
            {
                return false;
            } 
        }
        return true;
    }
    
    public bool IsValidArea(SectorCoord[] sectors)
    {
        if (sectors == null || sectors.Length == 0)
            return false;

        AreaBoundingCoord area = new AreaBoundingCoord(sectors);

        int width  = area.xMax - area.xMin + 1;
        int height = area.zMax - area.zMin + 1;
        int expectedCount = width * height;

        if (sectors.Length != expectedCount)
            return false;

        var lookup = new HashSet<(int x, int z)>(
            sectors.Select(s => (s.x, s.z))
        );

        for (int x = area.xMin; x <= area.xMax; x++)
        {
            for (int z = area.zMin; z <= area.zMax; z++)
            {
                if (!lookup.Contains((x, z)))
                    return false;
            }
        }

        return true;
    }
}