using System.Collections.Generic;
using UnityEngine;

public enum HexStackShape
{
    None,
    Uniform,
    Pyramid,
    PeripheryWithInner
}

public class RandomizedHexStack : RandomizerSpawner
{
    private float sqrt3 = Mathf.Sqrt(3f);
    private static readonly Vector2Int[] _hexDirections = {
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
        var coord = _hexDirections[4] * radius;

        for (int side = 0; side < 6; side++)
        {
            for (int step = 0; step < radius; step++)
            {
                results.Add(coord);
                coord += _hexDirections[side];
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

    //While spawning hex stacks, we receive the area -
    //Based on that, first choose the shape to draw
    //Then the token
    
    public void SpawnHexStack(PointTokenType tokenType, Vector3 center, float radius, float height, float spacing, int rings, int levels, Transform parent, HexStackShape shape = HexStackShape.Uniform)
    {
        for (int lvl = 0; lvl < levels; lvl++)
        {
            float levelY = center.y + lvl * height;
            Vector3 levelCenter = new Vector3(center.x, levelY, center.z);
            
            // Determine current max ring based on shape
            switch (shape)
            {
                case HexStackShape.Uniform:
                    SpawnRingsUpTo(rings, levelCenter, radius, spacing, tokenType, parent);
                    break;

                case HexStackShape.Pyramid:
                    int maxRing = Mathf.Max(rings - lvl, 0);
                    if (maxRing > 0)
                        SpawnRingsUpTo(maxRing, levelCenter, radius, spacing, tokenType, parent);
                    break;

                case HexStackShape.PeripheryWithInner:
                    // Outer wall only at the outermost ring
                    SpawnRing(rings, levelCenter, radius, spacing, tokenType, parent);
                    // Inner small stack of radius 2
                    if (lvl == 0)
                    {
                        SpawnRingsUpTo(1, levelCenter, radius, spacing, tokenType, parent);
                    }
                    break;
            }
        }
    }

    private void SpawnRingsUpTo(int maxRings, Vector3 levelCenter, float R, float d, PointTokenType tokenType, Transform parent)
    {
        for (int ring = 0; ring <= maxRings; ring++)
        {
            // ring=0 is the center
            if (ring == 0)
            {
                SpawnAt(levelCenter, tokenType, parent);
            }
            else
            {
                SpawnRing(ring, levelCenter, R, d, tokenType, parent);
            }
        }
    }

    private void SpawnRing(int ring, Vector3 levelCenter, float R, float d, PointTokenType tokenType, Transform parent)
    {
        var coords = GetRingCoords(ring);
        foreach (var ax in coords)
        {
            Vector3 relOffset = AxialToOffset(ax, R, d);
            Vector3 spawnPos = levelCenter + relOffset;

            Vector3 dirToCenter = (new Vector3(levelCenter.x, spawnPos.y, levelCenter.z) - spawnPos).normalized;
            Quaternion faceCenter = Quaternion.LookRotation(dirToCenter, Vector3.up);

            SpawnAt(spawnPos, tokenType, parent, faceCenter);
        }
    }

    private void SpawnAt(Vector3 pos, PointTokenType tokenType, Transform parent, Quaternion? rot = null)
    {
        var obj = PoolingManager.Instance.GetObject(tokenType);
        obj.transform.SetParent(parent, false);
        obj.transform.position    = pos;
        obj.transform.rotation    = rot ?? Quaternion.identity;

        if (obj.TryGetComponent<Collectible>(out var collectible))
        {
            collectible.InitializeAndSetup(
                GameManager.Context,
                5,
                1,
                Collectible.PointDisplayType.InBody);
        }
    }
}
