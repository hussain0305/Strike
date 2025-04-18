using UnityEngine;

public class DeathMatch : GameMode
{
    public override void OnShotComplete(bool hitNormalPin)
    {
        if (!hitNormalPin)
        {
            int player = GameManager.Instance.CurrentPlayerTurn;
            RoundDataManager.Instance.EliminatePlayer(player);
        }
    }
}