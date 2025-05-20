using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SectorGridHelper
{
    public readonly Transform[] SectorLinesX;
    public readonly Transform[] SectorLinesZ;
    public readonly SectorCoord sectorGridSize;
    public readonly HashSet<SectorCoord> blacklistedSectors;
    
    public Vector3 xSpacing = new Vector3(0.2f, 0, 0);
    public Vector3 ySpacing = new Vector3(0, 0.2f, 0);
    public Vector3 zSpacing = new Vector3(0, 0, 0.2f);
    
    public SectorGridHelper(Transform[] sectorLinesX, Transform[] sectorLinesZ, SectorCoord gridSize, HashSet<SectorCoord> _blacklistedSectors)
    {
        SectorLinesX = sectorLinesX;
        SectorLinesZ = sectorLinesZ;
        sectorGridSize = gridSize;
        blacklistedSectors = _blacklistedSectors;
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
    
    public List<Vector3> GetRandomIntersections(int numRequired, Vector3 offset)
    {
        var result = new List<Vector3>(numRequired);
        if (numRequired == 0 || SectorLinesX.Length < 3 || SectorLinesZ.Length < 3)
            return result;

        int midZ = (SectorLinesZ.Length - 1) / 2;
        float yArchOffset = -2;
        
        var coords = new List<Vector2Int>();
        for (int i = 1; i < SectorLinesX.Length - 1; i++)
            for (int j = 0; j < SectorLinesZ.Length - 2; j++)
                coords.Add(new Vector2Int(i, j));

        for (int k = coords.Count - 1; k > 0; k--)
        {
            int r = Random.Range(0, k + 1);
            (coords[k], coords[r]) = (coords[r], coords[k]);
        }

        int take = Mathf.Min(numRequired, coords.Count);
        for (int k = 0; k < take; k++)
        {
            var intersection = coords[k];
            int xIndex = intersection.x;
            int zIndex = intersection.y;
            float x = SectorLinesX[xIndex].position.x;
            float z = SectorLinesZ[zIndex].position.z + offset.z;
            float y = offset.y + (Mathf.Abs(midZ - zIndex) * yArchOffset);
            result.Add(new Vector3(x, y, z));
        }

        return result;
    }
}
