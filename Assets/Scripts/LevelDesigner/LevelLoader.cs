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
    
    public void CheckForContinuousMovement(GameObject obj, Vector3[] path, float movementSpeed)
    {
        bool objMoves = path != null && path.Length > 1;
        var cmScript = obj.GetComponent<ContinuousMovement>();

        if (objMoves)
        {
            if (!cmScript)
                cmScript = obj.AddComponent<ContinuousMovement>();

            cmScript.enabled = true;
            cmScript.canMove = true;
            cmScript.CreateMarkers(path);
            cmScript.speed  = movementSpeed;
        }
        else if (cmScript)
        {
            Destroy(cmScript);
        }
    } 
    
    public void CheckForContinuousRotation(GameObject obj, Vector3 rotationAxis, float rotationSpeed)
    {
        bool objRotates = rotationSpeed != 0;
        var crScript = obj.GetComponent<ContinuousRotation>();

        if (objRotates)
        {
            if (!crScript)
                crScript = obj.AddComponent<ContinuousRotation>();

            crScript.enabled = true;
            crScript.rotationAxis = rotationAxis;
            crScript.rotationSpeed = rotationSpeed;
        }
        else if (crScript && !crScript.gameObject.CompareTag(Global.ResistComponentDeletionTag))
        {
            Destroy(crScript);
        }
    }
}
