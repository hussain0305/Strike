using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public ObstacleType type;
    
    private Positioning positioning;
    private IContextProvider context;
    
    public void InitializeAndSetup(IContextProvider _context, LevelExporter.ObstacleData _obstacleData)
    {
        context = _context;
        type = _obstacleData.type;
        positioning = _obstacleData.positioning;
        
        bool obstacleMoves = _obstacleData.path != null && _obstacleData.path.Length > 1;
        ContinuousMovement cmScript = GetComponent<ContinuousMovement>();

        if (obstacleMoves)
        {
            if (!cmScript)
                cmScript = gameObject.AddComponent<ContinuousMovement>();

            cmScript.pointA = _obstacleData.path[0];
            cmScript.pointB = _obstacleData.path[1];
            cmScript.speed  = _obstacleData.movementSpeed;
                
            Rigidbody rBody = GetComponent<Rigidbody>();
            rBody.isKinematic = true;
        }
        else if (cmScript)
        {
            Destroy(cmScript);
        }

        bool obstacleRotates = _obstacleData.rotationAxis != Vector3.zero && _obstacleData.rotationSpeed != 0;
        ContinuousRotation crScript = GetComponent<ContinuousRotation>();

        if (obstacleRotates)
        {
            if (!crScript)
                crScript = gameObject.AddComponent<ContinuousRotation>();

            crScript.rotationAxis = _obstacleData.rotationAxis;
            crScript.rotationSpeed = _obstacleData.rotationSpeed;
            
            Rigidbody rBody = GetComponent<Rigidbody>();
            rBody.isKinematic = true;
        }
        else if (crScript)
        {
            Destroy(crScript);
        }
    }
}
