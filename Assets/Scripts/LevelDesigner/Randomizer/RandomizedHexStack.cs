using System.Collections.Generic;
using UnityEngine;

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

    public void SpawnHexStacksWithCenter(PointTokenType tokenType, Vector3 center, float radius, float height, float d, int rings, int levels, Transform parent)
    {
        for (int lvl = 0; lvl < levels; lvl++)
        {
            float heightY = lvl * height;
            Vector3 levelCenter = new Vector3(center.x, center.y + heightY, center.z);

            var centerObj = PoolingManager.Instance.GetObject(tokenType);
            SpawnObject(centerObj, levelCenter, Quaternion.identity, parent);

            for (int ring = 1; ring <= rings - (lvl + 1); ring++)
            {
                var coords = GetRingCoords(ring);
                foreach (var ax in coords)
                {
                    Vector3 relOffset = AxialToOffset(ax, radius, d);
                    Vector3 spawnPos = levelCenter + relOffset;

                    Vector3 dirToCenter = (new Vector3(center.x, spawnPos.y, center.z) - spawnPos).normalized;
                    Quaternion faceCenter = Quaternion.LookRotation(dirToCenter, Vector3.up);
                    Quaternion finalRot = faceCenter;

                    var obj = PoolingManager.Instance.GetObject(tokenType);
                    SpawnObject(obj, spawnPos, finalRot, parent);
                }
            }
        }
    }

    private void SpawnObject(GameObject obj, Vector3 position, Quaternion rotation, Transform parent)
    {
        obj.transform.SetParent(parent, false);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        var collectible = obj.GetComponent<Collectible>();
        if (collectible != null)
        {
            collectible.InitializeAndSetup(
                GameManager.Context,
                5,
                1,
                Collectible.PointDisplayType.InBody);
        }
    }
}
