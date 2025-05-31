using UnityEngine;
using Zenject;

public abstract class GameMode : MonoBehaviour
{
    protected WinCondition defaultWinCondition = WinCondition.PointsRequired;
    protected WinCondition multiplayerWinCondition = WinCondition.PointsRanking;
    
    protected PinBehaviourPerTurn pinBehaviour = PinBehaviourPerTurn.Reset;
    public PinBehaviourPerTurn PinBehaviour => pinBehaviour;

    private int pointsRequired;
    public int PointsRequired => pointsRequired;
    
    protected int numVolleys = 5;
    public int NumVolleys => numVolleys;
    
    private float minTimePerShot = 5;
    public float MinTimePerShot => minTimePerShot;
    
    private float maxTimePerShot = 10;
    public float MaxTimePerShot => maxTimePerShot;
    
    private int numProjectileViews = 3;
    public int NumProjectileViews => numProjectileViews;
    
    private int projectileViewDuration = 30;
    public int ProjectileViewDuration => projectileViewDuration;
    
    [InjectOptional]
    protected ModeSelector modeSelector;
    [InjectOptional]
    protected GameManager gameManager;
    [InjectOptional]
    protected RoundDataManager roundDataManager;
    
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
            int player = gameManager.CurrentPlayerTurn;
            roundDataManager.EliminatePlayer(player);
        }
    }

    public virtual WinCondition GetWinCondition()
    {
        pointsRequired = gameManager.levelLoader.GetTargetPoints();
        return modeSelector.IsPlayingSolo ? defaultWinCondition : multiplayerWinCondition;
    }

    public float GetMinTimePerShot()
    {
        return MinTimePerShot;
    }
    
    public float GetMaxTimePerShot()
    {
        return MaxTimePerShot;
    }

    public float GetOptionalTimePerShot()
    {
        return MaxTimePerShot - MinTimePerShot;
    }
    
    public virtual bool ShouldEndGame()
    {
        int numPlayers = gameManager.NumPlayersInGame;
        bool isSolo = (numPlayers == 1);
        int numEliminatedPlayers = roundDataManager.EliminationOrder.Count;
        
        if (isSolo && GetWinCondition() == WinCondition.PointsRequired)
        {
            int pts = roundDataManager.GetPointsForPlayer(0);
            if (pts >= PointsRequired) 
                return true;
        }

        if (gameManager.VolleyNumber > NumVolleys)
            return true;

        if (!isSolo && numEliminatedPlayers >= numPlayers - 1)
            return true;

        if (isSolo && numEliminatedPlayers == 1)
            return true;

        return false;
    }
    
    public virtual void OnShotComplete(bool hitDangerPin, bool hitNormalPin)
    {
        EventBus.Publish(new CueNextShotEvent());
    }

    public virtual bool LevelCompletedSuccessfully()
    {
        return roundDataManager.GetPointsForPlayer(0) >= PointsRequired;
    }
}
