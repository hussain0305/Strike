using System;
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

public static class SpawnPayloadEngine
{
    private static float difficultyFactor;
    
    private static int tokenCount;
    private static int obstacleCount;
    private static readonly Dictionary<PointTokenType,int> tokenTypeCounts = new Dictionary<PointTokenType,int>();
    private static readonly Dictionary<ObstacleType,int> obstacleTypeCounts = new Dictionary<ObstacleType,int>();
    private static readonly List<int> bigObstaclesZTracker = new List<int>();

    private static readonly List<ObstacleType> loneSectorObstacleTypes = new()
    {
        ObstacleType.SmallFan,
        ObstacleType.SmallWallSeeThrough,
        ObstacleType.Window,
    };

    public static readonly HashSet<ObstacleType> BigObstacles = new() {
        ObstacleType.SwitchDoor,
        ObstacleType.Window,
        ObstacleType.Wall,
    };
    
    private static readonly float imbalanceFactor = 0.4f;

    private static (float baseW, float slope) TokenParams(PointTokenType t)
    {
        return t switch
        {
            PointTokenType.Brick      => (2f, +3f),
            PointTokenType.Cube_2x2   => (2f, +2f),
            PointTokenType.Cuboid_4x2 => (2f,  0f),
            PointTokenType.Pin_1x     => (3f, -1f),
            PointTokenType.Pin_2x     => (3f, -2f),
            PointTokenType.Pin_4x     => (3f, -3f),
            _                         => (1f, 0f),
        };
    }

    private static (float baseW, float slope) ObstacleParams(ObstacleType o)
    {
        return o switch
        {
            ObstacleType.SmallFan    => (1.0f, +1.00f),
            ObstacleType.SmallWallSeeThrough   => (1.0f, +1.00f),
            ObstacleType.Window      => (0.5f, +0.25f),
            ObstacleType.Fan         => (1.0f, +0.50f),
            ObstacleType.SwitchDoor  => (1.7f, +0.50f),
            ObstacleType.Wall        => (0.2f, +0.50f),
            _                        => (1.0f, 0f),
        };
    }
    
    public static void Refresh()
    {
        tokenCount = 0;
        obstacleCount = 0;
        tokenTypeCounts.Clear();
        obstacleTypeCounts.Clear();
        bigObstaclesZTracker.Clear();
    }

    private static float GetPickerWeight(float baseW, float slope)
    {
        return Mathf.Max(0.1f, baseW + (slope * difficultyFactor));
    }
    
    public static SectorSpawnPayload SectorSpawnPayloadPicker(SectorCoord sectorCoord, int difficulty, int maxXCoord, int maxZCoord)
    {
        var payload = new SectorSpawnPayload();
        payload.Entries = new List<SectorSpawnPayload.Entry>();

        difficultyFactor = difficulty / 10f;
        bool isBackSector = (sectorCoord.z > (maxZCoord * 0.5f));

        int localObsCount = 0;
        int maxLocalObs = 2;
        
        var totalObjectsPicker = new WeightedRandomPicker<int>();
        totalObjectsPicker.AddChoice(1, 1);
        totalObjectsPicker.AddChoice(2, 2);
        totalObjectsPicker.AddChoice(3, 3);
        totalObjectsPicker.AddChoice(4, 3);
        totalObjectsPicker.AddChoice(5, 2);
        int totalObjects = totalObjectsPicker.Pick();
        
        for (int i = 0; i < totalObjects; i++)
        {
            float imbalance = (obstacleCount - tokenCount) * imbalanceFactor;
            var kindPicker = new WeightedRandomPicker<bool>();
            // true → point, false → obstacle
            // easier → favor points; harder → favor obstacles; back → favor obstacles too
            kindPicker.AddChoice(true, Mathf.Max(0.1f, (2f - difficultyFactor + (isBackSector ? 1f : 0f)) + imbalance));
            float obsBase = Mathf.Max(0.1f, (difficultyFactor + (isBackSector ? 0f : 0.5f) - imbalance));
            kindPicker.AddChoice(false, (localObsCount >= maxLocalObs) ? 0f : obsBase);
            bool spawnPoint = kindPicker.Pick();
            
            if (spawnPoint)
            {
                tokenCount++;
                var tokenPicker = new WeightedRandomPicker<PointTokenType>();
                foreach (var type in new[]{ 
                             PointTokenType.Brick,
                             PointTokenType.Cube_2x2,
                             PointTokenType.Cuboid_4x2,
                             PointTokenType.Pin_1x,
                             PointTokenType.Pin_2x })
                {
                    var (baseWeight, slope) = TokenParams(type);
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
                localObsCount++;
                
                var obsPicker = new WeightedRandomPicker<ObstacleType>();

                foreach (var obs in loneSectorObstacleTypes)
                {
                    
                    if (BigObstacles.Contains(obs) && bigObstaclesZTracker.Contains(sectorCoord.z))
                        continue;
                    
                    var (b, s) = ObstacleParams(obs);
                    float w = GetPickerWeight(b, s);
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
                    bigObstaclesZTracker.Add(sectorCoord.z);
                
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
            var sectorPayload = SectorSpawnPayloadPicker(sec, difficulty, maxXCoord, maxZCoord );
            combined.Entries.AddRange(sectorPayload.Entries);
        }

        return combined;
    }
}
