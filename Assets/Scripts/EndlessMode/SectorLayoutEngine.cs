using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnInstruction
{
    public Vector3 Position;
    public Quaternion Rotation;
    public SectorSpawnPayload.Entry Entry;
    
    public SpawnInstruction(Vector3 pos, Quaternion rot, SectorSpawnPayload.Entry entry)
    {
        Position = pos;
        Rotation = rot;
        Entry = entry;
    }
}

public static class SectorLayoutEngine
{
    private static readonly HashSet<int> bigRowsUsed = new();
    
    public static IList<SpawnInstruction> LayoutSector(IList<SectorSpawnPayload.Entry> entries, AreaWorldBound bounds, SectorCoord sectorCoord, SectorCoord gridSize)
    {
        var instructions = new List<SpawnInstruction>(entries.Count);
        
        foreach (var e in entries.Where(e => e.obstacleType != ObstacleType.None))
        {
            var inst = PlaceSingleObstacle(e, bounds, instructions, sectorCoord, gridSize);
            if (inst != null)
                instructions.Add(inst);
        }
        
        foreach (var e in entries.Where(e => e.pointTokenType != PointTokenType.None))
        {
            var inst = PlaceSingleToken(e, bounds, instructions);
            if (inst != null)
                instructions.Add(inst);
        }
        
        return instructions;
    }

    private static SpawnInstruction PlaceSingleObstacle(SectorSpawnPayload.Entry entry, AreaWorldBound bounds, IList<SpawnInstruction> existing, 
        SectorCoord sectorCoord, SectorCoord gridSize)
    {
        bool isBig = EndlessModeSpawnEngine.BigObstacles.Contains(entry.obstacleType);
        int z = sectorCoord.z;
        if (isBig && !bigRowsUsed.Contains(z))
        {
            bool towardFront = z < gridSize.z / 2;
            float xMin = bounds.xMin, xMax = bounds.xMax;
            float zLine = towardFront ? bounds.zMax : bounds.zMin;
            Vector3 pos = new Vector3((xMin + xMax) * 0.5f, 0, zLine);
            Vector3 dir = towardFront ? Vector3.forward : Vector3.back;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            bigRowsUsed.Add(z);
            return new SpawnInstruction(pos, rot, entry);
        }
        Vector3 dim = CollectiblePrefabMapping.Instance.GetObstacleDimension(entry.obstacleType);
        var sizeXZ = new Vector2(dim.x, dim.z);

        for (int attempt = 0; attempt < 10; attempt++)
        {
            float cornerX = Random.Range(bounds.xMin, bounds.xMax - sizeXZ.x);
            float cornerZ = Random.Range(bounds.zMin, bounds.zMax - sizeXZ.y);
            var pos = new Vector3(cornerX + sizeXZ.x/2, 0, cornerZ + sizeXZ.y/2);
            
            if (!IsFree(pos, sizeXZ, existing))
                continue;
            
            Quaternion rot = Quaternion.identity;
            
            bool flushLeft = Mathf.Approximately(cornerX, bounds.xMin);
            bool flushRight = Mathf.Approximately(cornerX + sizeXZ.x, bounds.xMax);
            bool flushBottom = Mathf.Approximately(cornerZ, bounds.zMin);
            bool flushTop = Mathf.Approximately(cornerZ + sizeXZ.y, bounds.zMax);

            if (flushLeft || flushRight || flushBottom || flushTop)
            {
                Vector3 edgeDir;
                if (flushLeft || flushRight)
                    edgeDir = Vector3.forward;
                else
                    edgeDir = Vector3.right;

                rot = Quaternion.LookRotation(edgeDir, Vector3.up);
            }
            else if (entry.obstacleType == ObstacleType.Wall)
            {
                var center = bounds.Center();
                pos = new Vector3(center.x, 0, center.z);

                Vector3 diag = (new Vector3(bounds.xMax, 0, bounds.zMax) - new Vector3(bounds.xMin, 0, bounds.zMin)).normalized;
                rot = Quaternion.LookRotation(diag, Vector3.up);
            }
            return new SpawnInstruction(pos, rot, entry);
        }
        return null;
    }

    private static SpawnInstruction PlaceSingleToken(SectorSpawnPayload.Entry entry, AreaWorldBound b, IList<SpawnInstruction> existing)
    {
        Vector3 dim = CollectiblePrefabMapping.Instance.GetPointTokenDimension(entry.pointTokenType);
        var sizeXZ = new Vector2(dim.x, dim.z);
        
        float insetX = sizeXZ.x / 2;
        float insetZ = sizeXZ.y / 2;
        float minX = b.xMin + insetX;
        float maxX = b.xMax - insetX;
        float minZ = b.zMin + insetZ;
        float maxZ = b.zMax - insetZ;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);
            var pos = new Vector3(x, 0, z);

            if (IsFree(pos, sizeXZ, existing))
                return new SpawnInstruction(pos, Quaternion.identity, entry);
        }
        return null;
    }

    private static bool IsFree(Vector3 pos, Vector2 sizeXZ,IEnumerable<SpawnInstruction> existing)
    {
        var r1 = new Rect(pos.x - sizeXZ.x/2, pos.z - sizeXZ.y/2, sizeXZ.x, sizeXZ.y);

        foreach (var e in existing)
        {
            Vector3 eDimension = e.Entry.obstacleType != ObstacleType.None ? CollectiblePrefabMapping.Instance.GetObstacleDimension(e.Entry.obstacleType) : 
                CollectiblePrefabMapping.Instance.GetPointTokenDimension(e.Entry.pointTokenType);
            var sz = new Vector2(eDimension.x, eDimension.z);

            var r2 = new Rect(e.Position.x - sz.x/2, e.Position.z - sz.y/2, sz.x, sz.y);

            if (r1.Overlaps(r2))
                return false;
        }
        return true;
    }

    public static void Refresh()
    {
        bigRowsUsed.Clear();
    }
}
