using System;
using UnityEngine;

public abstract class GameMode : MonoBehaviour
{
    public WinCondition winCondition;
    public int pointsRequired;
    
    public int numVolleys = 5;
    public float minTimePerShot = 5;
    public float maxTimePerShot = 10;
    public int numProjectileViews = 3;
    public int projectileViewDuration = 10;
    
    private static GameMode instance;
    public static GameMode Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<CollectibleHitEvent>(CollectibleHit);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<CollectibleHitEvent>(CollectibleHit);
    }

    public void CollectibleHit(CollectibleHitEvent e)
    {
        if (e.Type == CollectibleType.Danger)
        {
            int player = GameManager.Instance.CurrentPlayerTurn;
            RoundDataManager.Instance.EliminatePlayer(player);
        }
    }

    public virtual WinCondition GetWinCondition()
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

    public float GetMinTimePerShot()
    {
        return minTimePerShot;
    }
    
    public float GetMaxTimePerShot()
    {
        return maxTimePerShot;
    }

    public float GetOptionalTimePerShot()
    {
        return maxTimePerShot - minTimePerShot;
    }
    
    public virtual bool ShouldEndGame()
    {
        int numPlayers = GameManager.Instance.NumPlayersInGame;
        bool isSolo = (numPlayers == 1);
        int numEliminatedPlayers = RoundDataManager.Instance.EliminationOrder.Count;
        
        if (isSolo && GetWinCondition() == WinCondition.PointsRequired)
        {
            int pts = RoundDataManager.Instance.GetPointsForPlayer(0);
            if (pts >= pointsRequired) 
                return true;
        }

        if (GameManager.Instance.VolleyNumber > numVolleys)
            return true;

        if (!isSolo && numEliminatedPlayers >= numPlayers - 1)
            return true;

        if (isSolo && numEliminatedPlayers == 1)
            return true;

        return false;
    }
    
    public virtual void OnShotComplete(bool hitNormalPin)
    {
        
    }
}
