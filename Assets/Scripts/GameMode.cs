using UnityEngine;

public abstract class GameMode : MonoBehaviour
{
    protected WinCondition winCondition = WinCondition.PointsRequired;
    
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
    
    private int projectileViewDuration = 10;
    public int ProjectileViewDuration => projectileViewDuration;
    
    private static GameMode instance;
    public static GameMode Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance.gameObject);
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
        int numPlayers = GameManager.Instance.NumPlayersInGame;
        bool isSolo = (numPlayers == 1);
        int numEliminatedPlayers = RoundDataManager.Instance.EliminationOrder.Count;
        
        if (isSolo && GetWinCondition() == WinCondition.PointsRequired)
        {
            int pts = RoundDataManager.Instance.GetPointsForPlayer(0);
            if (pts >= PointsRequired) 
                return true;
        }

        if (GameManager.Instance.VolleyNumber > NumVolleys)
            return true;

        if (!isSolo && numEliminatedPlayers >= numPlayers - 1)
            return true;

        if (isSolo && numEliminatedPlayers == 1)
            return true;

        return false;
    }
    
    public virtual void OnShotComplete(bool hitSomething)
    {
        EventBus.Publish(new CueNextShotEvent());
    }
}
