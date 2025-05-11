using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public Transform starsParent;
    public Transform portalsParent;
    public Transform collectiblesParent;
    public Transform obstaclesParentPlatform;
    public Transform obstaclesParentWorld;

    protected int targetPoints;
    
    public virtual void LoadLevel() { }
    
    public virtual int GetTargetPoints()
    {
        return targetPoints;
    }
}
