using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct EdgeDefinition
{
    public Transform end1;
    public Transform end2;
}

public struct SectorBounds
{
    public float xMin;
    public float xMax;
    public float yMin;
    public float yMax;

    public SectorBounds(float _xMin, float _xMax, float _yMin, float _yMax)
    {
        xMin = _xMin;
        xMax = _xMax;
        yMin = _yMin;
        yMax = _yMax;
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
    public EdgeDefinition[] hexGutterCupSides;
    public Transform[] SectorLinesX;
    public Transform[] SectorLinesZ;
    public EndlessLevelWalls wallLocations;
    public int maxObjects = 50;

    [HideInInspector]
    public Vector3 platformCenter = new Vector3(0, 0, 50);
    private Vector3 xSpacing = new Vector3(0.2f, 0, 0);
    private Vector3 ySpacing = new Vector3(0, 0.2f, 0);
    private Vector3 zSpacing = new Vector3(0, 0, 0.2f);

    private RandomizedHexStack randomizedHexStack;
    private RandomizedGutterWall randomizedGutterWall;

    private SectorCoord sectorGridSize;

    private void Awake()
    {
        randomizedHexStack = gameObject.AddComponent<RandomizedHexStack>();
        randomizedHexStack.Initialize(this);
        
        randomizedGutterWall = gameObject.AddComponent<RandomizedGutterWall>();
        randomizedGutterWall.Initialize(this);

        sectorGridSize = new SectorCoord(SectorLinesX.Length - 1, SectorLinesZ.Length - 1);
    }

    public override int GetTargetPoints()
    {
        return 10;
    }

    override public void LoadLevel()
    {
        //Make Decisions here
        bool spawnHexStack = true;
        if (spawnHexStack)
        {
            SpawnHexStack(Vector3.zero);
        }
        randomizedGutterWall.SpawnOnSides(4, 3, RandomizedMovementOptions.SomeMoving, new IncludeTypesInRandomization(true, true));
    }

    public void PopulateSector(SectorCoord sector)
    {
        if (sectorGridSize.x >= sector.x || sectorGridSize.z >= sector.z || sectorGridSize.x < 0 || sectorGridSize.z < 0)
        {
            return;
        }
        
    }
    
    public void SpawnHexStack(Vector3 offsetFromCenter)
    {
        PointTokenType[] allHexTokens = new PointTokenType[] { PointTokenType.Pin_1x , PointTokenType.Pin_2x, PointTokenType.Pin_4x};
        PointTokenType randomHexToken = allHexTokens[Random.Range(0, allHexTokens.Length)];
        Vector3 dimensions = CollectiblePrefabMapping.Instance.GetPointTokenDimension(randomHexToken);

        int numRings = 2;
        int numLevels = 3;

        switch (randomHexToken)
        {
            case PointTokenType.Pin_1x:
                numRings = Random.Range(3,5);
                numLevels = Mathf.Min(Random.Range(3,5), numRings);
                break;
            case PointTokenType.Pin_2x:
                numRings = Random.Range(2,4);
                numLevels = Mathf.Min(Random.Range(2,4), numRings);
                break;
            case PointTokenType.Pin_4x:
                numRings = 2;
                numLevels = Mathf.Min(Random.Range(1,2), numRings);
                break;
        }
        
        randomizedHexStack.SpawnHexStacksWithCenter(randomHexToken, platformCenter + ySpacing, dimensions.x / 2, dimensions.y, 
            xSpacing.x, numRings, numLevels, collectiblesParent);
    }
    
    

    public bool GetSectorBounds(SectorCoord sector, out SectorBounds sectorBounds)
    {
        if (!IsSectorContainedOnGrid(sector))
        {
            sectorBounds = new SectorBounds();
            return false;
        }
        
        sectorBounds = new SectorBounds(
            SectorLinesX[sector.x].transform.position.x,
            SectorLinesX[sector.x + 1].transform.position.x,
            SectorLinesZ[sector.z].transform.position.z,
            SectorLinesZ[sector.z + 1].transform.position.z
            );
        return true;
    }
    
    public bool GetSectorBounds(SectorCoord[] sectors, out SectorBounds sectorBounds)
    {
        if (!IsValidArea(sectors) || !IsAreaContainedOnGrid(sectors))
        {
            sectorBounds = new SectorBounds();
            return false;
        }
        
        int minX = sectors.Min(s => s.x);
        int maxX = sectors.Max(s => s.x);
        int minZ = sectors.Min(s => s.z);
        int maxZ = sectors.Max(s => s.z);
        
        sectorBounds = new SectorBounds(
            SectorLinesX[minX].transform.position.x,
            SectorLinesX[maxX + 1].transform.position.x,
            SectorLinesZ[minZ].transform.position.z,
            SectorLinesZ[maxZ + 1].transform.position.z
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

        int minX = sectors.Min(s => s.x);
        int maxX = sectors.Max(s => s.x);
        int minZ = sectors.Min(s => s.z);
        int maxZ = sectors.Max(s => s.z);

        int width  = maxX - minX + 1;
        int height = maxZ - minZ + 1;
        int expectedCount = width * height;

        if (sectors.Length != expectedCount)
            return false;

        var lookup = new HashSet<(int x, int z)>(
            sectors.Select(s => (s.x, s.z))
        );

        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                if (!lookup.Contains((x, z)))
                    return false;
            }
        }

        return true;
    }
}