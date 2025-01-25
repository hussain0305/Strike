using UnityEngine;

public class GameMode : MonoBehaviour
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

    public virtual WinCondition GetWinCondition()
    {
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
}
