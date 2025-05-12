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

    /// <summary>
    /// Generates axial coordinates for a single hex ring at the given radius.
    /// </summary>
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

    /// <summary>
    /// Converts axial coords (q,r) into world-space offsets so that adjacent
    /// hex centers are spaced by distance (2*R + d).
    /// Returns a vector relative to the origin (0,0,0).
    /// </summary>
    private Vector3 AxialToOffset(Vector2Int hex, float R, float d)
    {
        float centerDist = 2f * R + d;
        float sizeEff = centerDist / sqrt3;
        float x = sizeEff * sqrt3 * (hex.x + hex.y * 0.5f);
        float z = sizeEff * 1.5f * hex.y;
        return new Vector3(x, 0f, z);
    }

    /// <summary>
    /// Spawns a stack of hex rings around a central point, with multiple vertical levels.
    /// Each level is rotated by 30° * levelIndex to interlock.
    /// </summary>
    /// <param name="center">World-space center of the base hex.</param>
    /// <param name="R">Inscribed circle radius (apothem).</param>
    /// <param name="d">Gap between adjacent hex sides.</param>
    /// <param name="rings">Number of radial rings around the center.</param>
    /// <param name="levels">Number of vertical levels (<= rings).</param>
    /// <param name="yOffset">Vertical distance between each level.</param>
    /// <param name="parent">Parent transform for spawned hexes.</param>
    public void SpawnHexStacksWithCenter(
        Vector3 center,
        float R,
        float d,
        int rings,
        int levels,
        float yOffset,
        Transform parent)
    {
        for (int lvl = 0; lvl < levels; lvl++)
        {
            float heightY = lvl * yOffset;
            float angleDeg = lvl * 30f;
            Quaternion levelRot = Quaternion.Euler(0f, angleDeg, 0f);
            Vector3 levelCenter = new Vector3(center.x, center.y + heightY, center.z);

            // Spawn center hex at this level
            var centerObj = PoolingManager.Instance.GetObject(PointTokenType.Pin_1x);
            SpawnObject(centerObj, levelCenter, levelRot, parent);

            // Spawn each ring around
            for (int ring = 1; ring <= rings - (lvl + 1); ring++)
            {
                var coords = GetRingCoords(ring);
                foreach (var ax in coords)
                {
                    Vector3 relOffset = AxialToOffset(ax, R, d);
                    Vector3 rotatedOffset = levelRot * relOffset;
                    Vector3 spawnPos = levelCenter + rotatedOffset;

                    // Face each hex's Z-axis toward the central column axis
                    Vector3 dirToCenter = (new Vector3(center.x, spawnPos.y, center.z) - spawnPos).normalized;
                    Quaternion faceCenter = Quaternion.LookRotation(dirToCenter, Vector3.up);
                    Quaternion finalRot = levelRot * faceCenter;

                    var obj = PoolingManager.Instance.GetObject(PointTokenType.Pin_1x);
                    SpawnObject(obj, spawnPos, finalRot, parent);
                }
            }
        }
    }

    /// <summary>
    /// Helper to position and initialize a pooled collectible object.
    /// </summary>
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
                5, // example score/value
                1, // example multiplier
                Collectible.PointDisplayType.InBody);
        }
    }
}
