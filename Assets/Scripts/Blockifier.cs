using System.Collections.Generic;
using UnityEngine;

public static class Blockifier
{
    static Blockifier()
    {
        EventBus.Subscribe<GameEndedEvent>(Cleanup);
        EventBus.Subscribe<GameExitedEvent>(Cleanup);
        EventBus.Subscribe<NextShotCuedEvent>(Cleanup);
    }
    
    private static PoolingManager poolingManager;
    private static int fineDetailBlockRadius = 1;
    private static float blockifyForce = 3;
    private static List<BuildingBlock> active1XBlocks = new();
    private static List<BuildingBlock> active2XBlocks = new();

    public static void SpawnDestructionChunks(Vector3 worldImpactPoint, PointToken pointTokenScript, Vector3 impactVelocity)
    {
        Transform transform = pointTokenScript.gameObject.transform;
        
        Vector3 localImpact = transform.InverseTransformPoint(worldImpactPoint);
        Vector3Int collectibleSize = new Vector3Int(
            (int)pointTokenScript.blockifyDimensions.x,
            (int)pointTokenScript.blockifyDimensions.y,
            (int)pointTokenScript.blockifyDimensions.z);
        
        int xSize = collectibleSize.x;
        int ySize = collectibleSize.y;
        int zSize = collectibleSize.z;

        bool[,,] occupied = new bool[xSize, ySize, zSize];

        int impactX = Mathf.Clamp(Mathf.RoundToInt(localImpact.x + xSize / 2f - 0.5f), 0, xSize - 1);
        int impactY = Mathf.Clamp(Mathf.RoundToInt(localImpact.y - 0.5f), 0, ySize - 1);
        int impactZ = Mathf.Clamp(Mathf.RoundToInt(localImpact.z + zSize / 2f - 0.5f), 0, zSize - 1);

        int beginX = Mathf.Max(0, impactX - fineDetailBlockRadius);
        int endX = Mathf.Min(xSize - 1, impactX + fineDetailBlockRadius);
        int beginY = Mathf.Max(0, impactY - fineDetailBlockRadius);
        int endY = Mathf.Min(ySize - 1, impactY + fineDetailBlockRadius);
        int beginZ = Mathf.Max(0, impactZ - fineDetailBlockRadius);
        int endZ = Mathf.Min(zSize - 1, impactZ + fineDetailBlockRadius);

        void Place1xBlock(int x, int y, int z)
        {
            occupied[x, y, z] = true;
            Vector3 localCenter = new Vector3(x + 0.5f - xSize / 2f, y, z + 0.5f - zSize / 2f);
            Vector3 worldPos = transform.TransformPoint(localCenter);

            BuildingBlock block = poolingManager.Get1xBlocks(1)[0];
            
            if (pointTokenScript.value < 0)
                block.SetNegativeBlock();
            else
                block.SetPositiveBlock();
            
            block.transform.position = worldPos;
            block.transform.rotation = transform.rotation;
            // Vector3 direction = (worldPos - worldImpactPoint).normalized;
            // block.linearVelocity = direction * blockifyForce * impactVelocity.sqrMagnitude;
            
            Vector3 direction = (worldPos - worldImpactPoint).normalized;
            Vector3 blended = Vector3.Lerp(impactVelocity.normalized, direction, 0.6f).normalized;
            Vector3 randomOffset = Random.insideUnitSphere * 0.2f;
            Vector3 finalVelocity = (blended + randomOffset).normalized * impactVelocity.magnitude;
            block.rBody.linearVelocity = finalVelocity;

            active1XBlocks.Add(block);
        }
        
        void Place2xBlock(int x, int y, int z)
        {
            for (int dx = 0; dx < 2; dx++)
            for (int dy = 0; dy < 2; dy++)
            for (int dz = 0; dz < 2; dz++)
                occupied[x + dx, y + dy, z + dz] = true;

            Vector3 localCenter = new Vector3(x + 1f - xSize / 2f, y, z + 1f - zSize / 2f);
            Vector3 worldPos = transform.TransformPoint(localCenter);

            BuildingBlock block = poolingManager.Get2xBlocks(1)[0];
            block.transform.position = worldPos;
            block.transform.rotation = transform.rotation;
            // Vector3 direction = (worldPos - worldImpactPoint).normalized;
            // block.linearVelocity = direction * blockifyForce * 0.75f;
            Vector3 direction = (worldPos - worldImpactPoint).normalized;
            Vector3 blended = Vector3.Lerp(impactVelocity.normalized, direction, 0.4f).normalized;
            Vector3 randomOffset = Random.insideUnitSphere * 0.1f;
            Vector3 finalVelocity = (blended + randomOffset).normalized * (impactVelocity.magnitude * 0.75f);

            block.rBody.linearVelocity = finalVelocity;
            active2XBlocks.Add(block);
        }

        void FillGapWith1xBlocks(int x, int y, int z)
        {
            for (int dx = 0; dx < 2; dx++)
            {
                for (int dy = 0; dy < 2; dy++)
                {
                    for (int dz = 0; dz < 2; dz++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        int nz = z + dz;

                        if (nx < xSize && ny < ySize && nz < zSize && !occupied[nx, ny, nz])
                            Place1xBlock(nx, ny, nz);
                    }
                }
            }
        }

        bool SpaceFreeFor2xBlock(int x, int y, int z)
        {
            for (int dx = 0; dx < 2; dx++)
            {
                for (int dy = 0; dy < 2; dy++)
                {
                    for (int dz = 0; dz < 2; dz++)
                    {
                        if (occupied[x + dx, y + dy, z + dz])
                            return false;
                    }
                }
            }
            
            return true;
        }
        
        for (int x = beginX; x <= endX; x++)
        {
            for (int y = beginY; y <= endY; y++)
            {
                for (int z = beginZ; z <= endZ; z++)
                {
                    Place1xBlock(x, y, z);
                }
            }
        }

        for (int x = 0; x < xSize; x += 2)
        {
            for (int y = 0; y < ySize; y += 2)
            {
                for (int z = 0; z < zSize; z += 2)
                {
                    if (x + 1 >= xSize || y + 1 >= ySize || z + 1 >= zSize)
                    {
                        FillGapWith1xBlocks(x, y, z);
                        continue;
                    }

                    if (SpaceFreeFor2xBlock(x, y, z))
                        Place2xBlock(x, y, z);
                    
                    else
                        FillGapWith1xBlocks(x, y, z);
                }
            }
        }
        
        pointTokenScript.ToggleVisibility(false);
    }

    public static void AssignPoolingManager(PoolingManager _poolingManager)
    {
        poolingManager = _poolingManager;
    }
    
    private static void Cleanup<T>(T e)
    {
        poolingManager.Return1xBlocks(active1XBlocks);
        poolingManager.Return2xBlocks(active2XBlocks);
        active1XBlocks.Clear();
        active2XBlocks.Clear();
    }
}
