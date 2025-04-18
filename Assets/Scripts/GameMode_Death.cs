using UnityEngine;

public class GameMode_Death : GameMode
{
    public override WinCondition GetWinCondition() => winCondition;

    public override void OnShotComplete(bool hitNormalPin)
    {
        if (!hitNormalPin)
        {
            int player = GameManager.Instance.CurrentPlayerTurn;
            RoundDataManager.Instance.EliminatePlayer(player);
        }
    }
}