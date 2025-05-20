using UnityEngine;

public class EndlessSectorPopulator
{
    private EndlessModeLoader endlessModeLoader;
    private SectorGridHelper sectorGridHelper;
    private int difficulty;
    private SectorCoord sectorGridSize;

    public void Initialize(EndlessModeLoader _endlessModeLoader, SectorGridHelper _sectorGridHelper)
    {
        endlessModeLoader = _endlessModeLoader;
        sectorGridHelper = _sectorGridHelper;
        difficulty = endlessModeLoader.Difficulty;
        sectorGridSize = sectorGridHelper.sectorGridSize;
    }
    
    public void PopulateSectors(SectorCoord[] loneSectors)
    {
        foreach (var loneSector in loneSectors)
        {
            PopulateSector(loneSector);
        }
    }

    public void PopulateSector(SectorCoord sector)
    {
        //Re-instate check if needed, commenting it out right now because the info coming through current channels is pre-vetted
        // if (!IsSectorContainedOnGrid(sector))
        // {
        //     return;
        // }
        sectorGridHelper.GetSectorBounds(sector, out AreaWorldBound worldBound);
        SectorSpawnPayload spawnPayload = SpawnPayloadEngine.SectorSpawnPayloadPicker(sector, difficulty, sectorGridSize.x, sectorGridSize.z);
        var instructions = SectorLayoutEngine.LayoutSector(spawnPayload.Entries, worldBound, sector, sectorGridHelper.sectorGridSize);

        SectorInfo sectorInfo = new SectorInfo(sector, spawnPayload);
        foreach (var inst in instructions)
        {
            if (inst.Entry.obstacleType != ObstacleType.None)
                endlessModeLoader.SpawnObstacle(inst.Entry.obstacleType, inst.Position, inst.Rotation);
            else
                endlessModeLoader.SpawnPointToken(inst.Entry.pointTokenType, inst.Position, inst.Rotation, sectorInfo);
        }
    }

}
