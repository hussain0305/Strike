using System.Collections.Generic;
using UnityEngine;

public enum HexStackShape
{
    None,
    Uniform,
    Pyramid,
    PeripheryWithInner
}

public class RandomizedHexStack : EndlessModeSpawner
{
    private float sqrt3 = Mathf.Sqrt(3f);
    private static readonly Vector2Int[] hexDirections = {
        new Vector2Int(1, 0),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1)
    };

    public List<Vector2Int> GetRingCoords(int radius)
    {
        var results = new List<Vector2Int>(6 * radius);
        var coord = hexDirections[4] * radius;

        for (int side = 0; side < 6; side++)
        {
            for (int step = 0; step < radius; step++)
            {
                results.Add(coord);
                coord += hexDirections[side];
            }
        }
        return results;
    }

    private Vector3 AxialToOffset(Vector2Int hex, float R, float d)
    {
        float centerDist = 2f * R + d;
        float sizeEff = centerDist / sqrt3;
        float x = sizeEff * sqrt3 * (hex.x + hex.y * 0.5f);
        float z = sizeEff * 1.5f * hex.y;
        return new Vector3(x, 0f, z);
    }

    public void PrepareHexStack(SectorCoord[] sectors)
    {
        AreaWorldBound area;
        AreaBoundingCoord areaBoundingCoord = new AreaBoundingCoord(sectors);
        if (!sectorGridHelper.GetAreaBounds(sectors, out area))
        {
            return;
        }

        WeightBasedPicker<HexStackShape> hexShapePicker = SpawnWeights.HexShapePicker(sectors.Length);
        HexStackShape selectedStackShape = hexShapePicker.Pick();
        
        WeightBasedPicker<PointTokenType> tokenPicker = SpawnWeights.HexTokenPicker(selectedStackShape);
        PointTokenType selectedToken = tokenPicker.Pick();
        Vector3 dimensions = CollectiblePrefabMapping.Instance.GetPointTokenDimension(selectedToken);

        float xLength = area.xMax - area.xMin;
        float zLength = area.zMax - area.zMin;

        WeightBasedPicker<(int, int)> dimensionsPicker = SpawnWeights.HexStackDimensionsPicker(xLength, zLength, selectedToken);
        (int numRings, int numLevels) = dimensionsPicker.Pick();
        
        //TODO: REMOVE THE areaBoundingCoord parameter when debug messages are removed
        SpawnHexStack(selectedToken, area.Center() + sectorGridHelper.ySpacing, dimensions.x / 2, dimensions.y, 
            sectorGridHelper.xSpacing.x, numRings, numLevels, endlessMode.collectiblesParent, selectedStackShape, areaBoundingCoord);
    }

    public void SpawnHexStack(PointTokenType tokenType, Vector3 center, float radius, float height, float spacing, int rings, int levels, Transform parent, HexStackShape shape, AreaBoundingCoord areaBoundingCoord)
    {
        for (int lvl = 0; lvl < levels; lvl++)
        {
            float levelY = center.y + lvl * height;
            Vector3 levelCenter = new Vector3(center.x, levelY, center.z);
            
            switch (shape)
            {
                case HexStackShape.Uniform:
                    SpawnRingsUpTo(rings, levelCenter, radius, spacing, tokenType, areaBoundingCoord);
                    break;

                case HexStackShape.Pyramid:
                    int maxRing = Mathf.Max(rings - lvl, 0);
                    if (maxRing > 0)
                        SpawnRingsUpTo(maxRing, levelCenter, radius, spacing, tokenType, areaBoundingCoord);
                    break;

                case HexStackShape.PeripheryWithInner:
                    // Outer wall only at the outermost ring
                    SpawnRing(rings, levelCenter, radius, spacing, tokenType, areaBoundingCoord);
                    // Inner small stack of radius 2
                    if (lvl == 0)
                    {
                        SpawnRingsUpTo(1, levelCenter, radius, spacing, tokenType, areaBoundingCoord);
                    }
                    break;
            }
        }
    }

    private void SpawnRingsUpTo(int maxRings, Vector3 levelCenter, float R, float d, PointTokenType tokenType, AreaBoundingCoord areaBoundingCoord)
    {
        for (int ring = 0; ring <= maxRings; ring++)
        {
            // ring=0 is the center
            if (ring == 0)
            {
                endlessMode.SpawnPointToken(tokenType, levelCenter, Quaternion.identity, new SectorInfo(areaBoundingCoord.CenterCoord(), null));
            }
            else
            {
                SpawnRing(ring, levelCenter, R, d, tokenType, areaBoundingCoord);
            }
        }
    }

    private void SpawnRing(int ring, Vector3 levelCenter, float R, float d, PointTokenType tokenType, AreaBoundingCoord areaBoundingCoord)
    {
        var coords = GetRingCoords(ring);
        foreach (var ax in coords)
        {
            Vector3 relOffset = AxialToOffset(ax, R, d);
            Vector3 spawnPos = levelCenter + relOffset;

            Vector3 dirToCenter = (new Vector3(levelCenter.x, spawnPos.y, levelCenter.z) - spawnPos).normalized;
            Quaternion faceCenter = Quaternion.LookRotation(dirToCenter, Vector3.up);

            endlessMode.SpawnPointToken(tokenType, spawnPos, faceCenter, new SectorInfo());
        }
    }
}
