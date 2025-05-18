using UnityEngine;

public class EndlessAreaPopulator
{
    private RandomizedHexStack randomizedHexStack;

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
        randomizedHexStack = endlessModeLoader.RandomizedHexStack;
    }

    public void PopulateAreas(SectorCoord[][] areasToFill)
    {
        foreach (SectorCoord[] areaToFill in areasToFill)
        {
            PopulateArea(areaToFill);
        }
    }

    public void PopulateArea(SectorCoord[] sectors)
    {
        //Re-instate check if needed, commenting it out right now because the info coming through current channels is pre-vetted
        // if (!IsAreaContainedOnGrid(area))
        // {
        //     return;
        // }

        AreaBoundingCoord areaBoundingCoord = new AreaBoundingCoord(sectors);
        if (areaBoundingCoord.IsSquare())
        {
            bool spawnHexStack = true; //and other conditions
            if (spawnHexStack)
            {
                //TODO: REMOVE THE areaBoundingCoord parameter when debug messages are removed. Not needed further down the pipeline
                randomizedHexStack.PrepareHexStack(sectors, areaBoundingCoord);
                return;
            }
        }
        Debug.Log("!!! Didn't spawn Hex stack so doing regular rectangular area spawn");
        FillRectangularArea(sectors);
    }
    
    public void FillRectangularArea(SectorCoord[] sectors)
    {
        sectorGridHelper.GetAreaBounds(sectors, out AreaWorldBound worldBound);
        SectorSpawnPayload spawnPayload = SpawnPayloadEngine.AreaSpawnPayloadPicker(sectors, worldBound, difficulty, sectorGridSize.x, sectorGridSize.z);
        var instructions = SectorLayoutEngine.LayoutSector(spawnPayload.Entries, worldBound);

        foreach (var inst in instructions)
        {
            Debug.Log("--->> Spawned stuff for rectangular area");
            if (inst.Entry.obstacleType != ObstacleType.None)
                endlessModeLoader.SpawnObstacle(inst.Entry.obstacleType, inst.Position, inst.Rotation);
            else
                endlessModeLoader.SpawnPointToken(inst.Entry.pointTokenType, inst.Position, inst.Rotation);
        }

    }

}
