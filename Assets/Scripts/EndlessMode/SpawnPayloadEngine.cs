using System.Collections.Generic;
using UnityEngine;

public class SectorSpawnPayload
{
    public struct Entry
    {
        public PointTokenType pointTokenType;
        public MultiplierTokenType multiplierTokenType;
        public ObstacleType obstacleType;
        public int count;
    }
    public List<Entry> Entries;
}

public class SpawnPayloadEngine : MonoBehaviour
{
    private static float difficultyFactor;
    
    private static float GetPickerWeight(float baseW, float slope)
    {
        return Mathf.Max(0.1f, baseW + (slope * difficultyFactor));
    }
    
    public static SectorSpawnPayload SectorSpawnPayloadPicker(SectorCoord sectorCoord, AreaWorldBound area, int difficulty, int maxXCoord, int maxZCoord)
    {
        var payload = new SectorSpawnPayload();
        payload.Entries = new List<SectorSpawnPayload.Entry>();

        difficultyFactor = difficulty / 10f;
        bool isBackSector = (area.zMax > (maxZCoord * 0.5f));

        var totalObjectsPicker = new WeightedRandomPicker<int>();
        totalObjectsPicker.AddChoice(1, 1);
        totalObjectsPicker.AddChoice(2, 2);
        totalObjectsPicker.AddChoice(3, 2);
        totalObjectsPicker.AddChoice(4, 3);
        totalObjectsPicker.AddChoice(5, 2);
        totalObjectsPicker.AddChoice(6, 1);
        int totalObjects = totalObjectsPicker.Pick();

        var kindPicker = new WeightedRandomPicker<bool>();
        // true → point, false → obstacle
        // easier → favor points; harder → favor obstacles; back → favor obstacles too
        kindPicker.AddChoice(true,  2f - difficultyFactor + (isBackSector ? 1f : 0f));
        kindPicker.AddChoice(false, difficultyFactor + (isBackSector ? 0f : 0.5f));

        for (int i = 0; i < totalObjects; i++)
        {
            bool spawnPoint = kindPicker.Pick();

            if (spawnPoint)
            {
                var tokenPicker = new WeightedRandomPicker<PointTokenType>();
                tokenPicker.AddChoice(PointTokenType.Brick,      GetPickerWeight(2f, +3f));
                tokenPicker.AddChoice(PointTokenType.Cube_2x2,   GetPickerWeight(2f, +2f));
                tokenPicker.AddChoice(PointTokenType.Cuboid_4x2, GetPickerWeight(2f, -0f));
                tokenPicker.AddChoice(PointTokenType.Pin_1x,     GetPickerWeight(3f, -1f));
                tokenPicker.AddChoice(PointTokenType.Pin_2x,     GetPickerWeight(3f, -2f));

                var choice = tokenPicker.Pick();
                payload.Entries.Add(new SectorSpawnPayload.Entry {
                    pointTokenType = choice,
                    obstacleType = ObstacleType.None,
                    count = 1
                });
            }
            else
            {
                var obsPicker = new WeightedRandomPicker<ObstacleType>();
                obsPicker.AddChoice(ObstacleType.SmallFan, GetPickerWeight(1f, 0.25f));
                obsPicker.AddChoice(ObstacleType.Window, GetPickerWeight(1f, 0.25f));

                
                var choice = obsPicker.Pick();
                payload.Entries.Add(new SectorSpawnPayload.Entry {
                    pointTokenType = PointTokenType.None,
                    obstacleType = choice,
                    count = 1
                });
            }
        }

        return payload;
    }
    public static SectorSpawnPayload AreaSpawnPayloadPicker(SectorCoord[] sectors, int difficulty, int maxXCoord, int maxZCoord)
    {
        var payload = new SectorSpawnPayload();
        var bounds  = new AreaBoundingCoord(sectors);

        difficultyFactor = difficulty / 10f;

        // decide total count based on area size
        // int areaSize = (bounds.xMax - bounds.xMin + 1) * (bounds.zMax - bounds.zMin + 1);
        int areaSize = (bounds.xMax - bounds.xMin) * (bounds.zMax - bounds.zMin);
        int totalCount = Mathf.Clamp(
            Mathf.RoundToInt(areaSize * 0.3f * difficultyFactor),
            3,
            areaSize
        );

        // e.g. 60% chance per slot to be an obstacle at high diff
        var kindPicker = new WeightedRandomPicker<bool>();
        kindPicker.AddChoice(true,  1f - difficultyFactor);
        kindPicker.AddChoice(false, difficultyFactor);

        // prepare token‐type picker, now allowing everything
        var tokenPicker = new WeightedRandomPicker<PointTokenType>();
        tokenPicker.AddChoice(PointTokenType.Brick,      GetPickerWeight(2f, +3f));
        tokenPicker.AddChoice(PointTokenType.Cube_2x2,   GetPickerWeight(2f, +2f));
        tokenPicker.AddChoice(PointTokenType.Cube_3x3,   GetPickerWeight(2f, -1f));
        tokenPicker.AddChoice(PointTokenType.Cuboid_4x2, GetPickerWeight(2f, -0f));
        tokenPicker.AddChoice(PointTokenType.Pin_1x,     GetPickerWeight(3f, -1f));
        tokenPicker.AddChoice(PointTokenType.Pin_2x,     GetPickerWeight(3f, -2f));
        tokenPicker.AddChoice(PointTokenType.Pin_4x,     GetPickerWeight(3f, -2f));

        // obstacle picker now includes Window/Fan
        var obsPicker = new WeightedRandomPicker<ObstacleType>();
        obsPicker.AddChoice(ObstacleType.SmallFan,   GetPickerWeight(1f, +1.25f));
        obsPicker.AddChoice(ObstacleType.Wall,       GetPickerWeight(1f, +1.25f));
        obsPicker.AddChoice(ObstacleType.Window,     GetPickerWeight(2f, -1.00f));
        obsPicker.AddChoice(ObstacleType.Fan,        GetPickerWeight(1f, +0.25f));
        obsPicker.AddChoice(ObstacleType.SwitchDoor, GetPickerWeight(1f, +0.50f));

        for (int i = 0; i < totalCount; i++)
        {
            bool spawnPoint = kindPicker.Pick();
            if (spawnPoint)
            {
                var choice = tokenPicker.Pick();
                payload.Entries.Add(new SectorSpawnPayload.Entry {
                    pointTokenType = choice,
                    obstacleType = ObstacleType.None,
                    count = 1
                });
            }
            else
            {
                var choice = obsPicker.Pick();
                payload.Entries.Add(new SectorSpawnPayload.Entry {
                    pointTokenType = PointTokenType.None,
                    obstacleType = choice,
                    count = 1
                });
            }
        }

        return payload;
    }
}
