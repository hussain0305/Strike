using System.Collections.Generic;
using UnityEngine;

public class SectorSpawnPayload
{
    public struct Entry
    {
        public PointTokenType pointTokenType;
        public MultiplierTokenType multiplierTokenType;
        public ObstacleType obstacleType;
    }
    public List<Entry> Entries;
}

public static class EndlessModeSpawnEngine
{
private static float difficultyFactor;
    
    private static int tokenCount;
    private static int obstacleCount;
    private static readonly Dictionary<PointTokenType,int> tokenTypeCounts = new Dictionary<PointTokenType,int>();
    private static readonly Dictionary<ObstacleType,int> obstacleTypeCounts = new Dictionary<ObstacleType,int>();
    private static readonly List<int> bigObstaclesXTracker = new List<int>();
    private static readonly List<int> bigObstaclesZTracker = new List<int>();

    private static readonly List<ObstacleType> loneSectorObstacleTypes = new()
    {
        ObstacleType.SmallWallSeeThrough,
        ObstacleType.WallSeeThrough,
        ObstacleType.ForcePad,
        ObstacleType.SmallFan,
        ObstacleType.Window,
    };

    public static readonly HashSet<ObstacleType> BigObstacles = new() {
        ObstacleType.WallSeeThrough,
        ObstacleType.SwitchDoor,
        ObstacleType.Window,
        ObstacleType.Wall,
    };

    public static readonly PointTokenType[] allPointTokenOptions = new[]
    {
        PointTokenType.Cuboid_4x2,
        PointTokenType.Cube_2x2,
        PointTokenType.Pin_2x,
        PointTokenType.Pin_1x,
        PointTokenType.Brick,
    };
    
    private static readonly float imbalanceFactor = 0.4f;

    private static void GetTokenParams(PointTokenType token, out float baseW, out float slope)
    {
        switch (token)
        {
            case PointTokenType.Brick:
                baseW = 2f;
                slope = 3f;
                break;
            case PointTokenType.Cube_2x2:
                baseW = 2f;
                slope = 2f;
                break;
            case PointTokenType.Cuboid_4x2:
                baseW = 2f;
                slope = 0f;
                break;
            case PointTokenType.Pin_1x:
                baseW = 3f;
                slope = -1f;
                break;
            case PointTokenType.Pin_2x:
                baseW = 3f;
                slope = -2f;
                break;
            case PointTokenType.Pin_4x:
                baseW = 3f;
                slope = -3f;
                break;
            default:
                baseW = 1f;
                slope = 0f;
                break;
        }
    }

    private static void GetObstacleParams(ObstacleType obstacle, out float baseW, out float slope)
    {
        switch (obstacle)
        {
            case ObstacleType.SmallFan:
                baseW = 1f;
                slope = 1f;
                break;
            case ObstacleType.SmallWallSeeThrough:
                baseW = 1f;
                slope = 1f;
                break;
            case ObstacleType.Window:
                baseW = 0.5f;
                slope = 0.25f;
                break;
            case ObstacleType.Fan:
                baseW = 1f;
                slope = 0.5f;
                break;
            case ObstacleType.ForcePad:
                baseW = 1f;
                slope = 1f;
                break;
            case ObstacleType.SwitchDoor:
                baseW = 1.7f;
                slope = 0.5f;
                break;
            case ObstacleType.Wall:
                baseW = 0.2f;
                slope = 0.5f;
                break;
            default:
                baseW = 1f;
                slope = 0f;
                break;
        }
    }
    
    public static void Refresh()
    {
        tokenCount = 0;
        obstacleCount = 0;
        tokenTypeCounts.Clear();
        obstacleTypeCounts.Clear();
        bigObstaclesXTracker.Clear();
        bigObstaclesZTracker.Clear();
    }

    private static float GetPickerWeight(float baseW, float slope)
    {
        return Mathf.Max(0.1f, baseW + (slope * difficultyFactor));
    }
    
    public static SectorSpawnPayload GetSectorSpawnPayload(SectorCoord sectorCoord, int difficulty, int maxXCoord, int maxZCoord)
    {
        var payload = new SectorSpawnPayload();
        payload.Entries = new List<SectorSpawnPayload.Entry>();

        difficultyFactor = difficulty / 10f;
        bool isBackSector = (sectorCoord.z > (maxZCoord * 0.5f));

        int localObstacleCount = 0;
        int maxLocalObstacles = 2;
        
        var totalObjectsPicker = new WeightBasedPicker<int>();
        totalObjectsPicker.AddChoice(1, 1);
        totalObjectsPicker.AddChoice(2, 2);
        totalObjectsPicker.AddChoice(3, 3);
        totalObjectsPicker.AddChoice(4, 3);
        totalObjectsPicker.AddChoice(5, 2);
        int totalObjects = totalObjectsPicker.Pick();
        
        for (int i = 0; i < totalObjects; i++)
        {
            float imbalance = (obstacleCount - tokenCount) * imbalanceFactor;
            var kindPicker = new WeightBasedPicker<bool>();

            kindPicker.AddChoice(true, Mathf.Max(0.1f, (2f - difficultyFactor + (isBackSector ? 1f : 0f)) + imbalance));
            float obstacleBaseProbability = Mathf.Max(0.1f, (difficultyFactor + (isBackSector ? 0f : 0.5f) - imbalance));
            kindPicker.AddChoice(false, (localObstacleCount >= maxLocalObstacles) ? 0f : obstacleBaseProbability);
            bool spawnPoint = kindPicker.Pick();
            
            if (spawnPoint)
            {
                tokenCount++;
                var tokenPicker = new WeightBasedPicker<PointTokenType>();
                foreach (var type in allPointTokenOptions)
                {
                    GetTokenParams(type, out float baseWeight, out float slope);
                    float w = GetPickerWeight(baseWeight, slope);
                    int used = tokenTypeCounts.TryGetValue(type, out var c) ? c : 0;
                    w *= 1f / (1f + used * 0.2f);
                    tokenPicker.AddChoice(type, w);
                }
                var choice = tokenPicker.Pick();
                payload.Entries.Add(new SectorSpawnPayload.Entry {
                    pointTokenType = choice,
                    obstacleType = ObstacleType.None,
                });
                
                tokenTypeCounts[choice] = tokenTypeCounts.GetValueOrDefault(choice) + 1;
            }
            else
            {
                obstacleCount++;
                localObstacleCount++;
                
                var obsPicker = new WeightBasedPicker<ObstacleType>();

                foreach (var obs in loneSectorObstacleTypes)
                {
                    if (BigObstacles.Contains(obs) && (bigObstaclesZTracker.Contains(sectorCoord.z) || bigObstaclesXTracker.Contains(sectorCoord.x)))
                        continue;
                    
                    GetObstacleParams(obs, out float baseWeight, out float slope);
                    float w = GetPickerWeight(baseWeight, slope);
                    int used = obstacleTypeCounts.GetValueOrDefault(obs);
                    w *= 1f / (1f + used * 0.2f);
                    obsPicker.AddChoice(obs, w);
                }
                
                var choice = obsPicker.Pick();
                payload.Entries.Add(new SectorSpawnPayload.Entry {
                    pointTokenType = PointTokenType.None,
                    obstacleType = choice,
                });

                if (BigObstacles.Contains(choice))
                {
                    bigObstaclesXTracker.Add(sectorCoord.x);
                    bigObstaclesZTracker.Add(sectorCoord.z);
                }
                
                obstacleTypeCounts[choice] = obstacleTypeCounts.GetValueOrDefault(choice) + 1;
            }
        }

        return payload;
    }
    public static SectorSpawnPayload AreaSpawnPayloadPicker(SectorCoord[] sectors, int difficulty, int maxXCoord, int maxZCoord)
    {
        var combined = new SectorSpawnPayload();
        combined.Entries = new List<SectorSpawnPayload.Entry>();

        foreach (var sec in sectors)
        {
            var sectorPayload = GetSectorSpawnPayload(sec, difficulty, maxXCoord, maxZCoord );
            combined.Entries.AddRange(sectorPayload.Entries);
        }

        return combined;
    }
}
