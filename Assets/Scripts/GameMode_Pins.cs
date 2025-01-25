public class GameMode_Pins : GameMode
{
    public override WinCondition GetWinCondition()
    {
        if (ModeSelector.Instance)
        {
            if (ModeSelector.Instance.GetNumPlayers() == 1)
            {
                winCondition = WinCondition.PointsRequired;
                pointsRequired = LevelLoader.Instance.GetTargetPoints();
            }
            else
            {
                winCondition = WinCondition.PointsRanking;
            }
        }
        else
        {
            winCondition = WinCondition.PointsRequired;
            pointsRequired = LevelLoader.Instance.GetTargetPoints();
        }
        return winCondition;
    }
}
