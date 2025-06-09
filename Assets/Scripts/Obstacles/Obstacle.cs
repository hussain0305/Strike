using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public ObstacleType type;
    public GameObject axisOfRotation;
    public bool isKinematic = true;
    
    private Positioning positioning;
    protected IContextProvider context;
    protected int numTotalPlayers;
    
    private Rigidbody rBody;
    public Rigidbody RBody => rBody ??= GetComponent<Rigidbody>();

    public void InitializeAndSetup(IContextProvider _context, LevelExporter.ObstacleData _obstacleData, int _numTotalPlayers = 1)
    {
        context = _context;
        numTotalPlayers = _numTotalPlayers;
        type = _obstacleData.type;
        positioning = _obstacleData.positioning;
        
        RBody.isKinematic = isKinematic;
        CheckForContinuousMovement(_obstacleData.path, _obstacleData.movementSpeed);
        CheckForContinuousRotation(_obstacleData.rotationAxis, _obstacleData.rotationSpeed);
    }
    
    public void CheckForContinuousMovement(Vector3[] path, float movementSpeed)
    {
        bool objMoves = path != null && path.Length > 1;
        var cmScript = gameObject.GetComponent<ContinuousMovement>();

        if (objMoves)
        {
            if (!cmScript)
                cmScript = gameObject.AddComponent<ContinuousMovement>();
            
            cmScript.CreateMarkers(path[0], path[1]);
            cmScript.speed  = movementSpeed;
            
            RBody.isKinematic = true;
        }
        else if (cmScript)
        {
            Destroy(cmScript);
        }
    } 
    
    public virtual void CheckForContinuousRotation(Vector3 rotationAxis, float rotationSpeed)
    {
        bool objRotates = rotationAxis != Vector3.zero && rotationSpeed != 0;
        var crScript = axisOfRotation.GetComponent<ContinuousRotation>();

        if (objRotates)
        {
            if (!crScript)
                crScript = axisOfRotation.AddComponent<ContinuousRotation>();
            crScript.rotationAxis = rotationAxis;
            crScript.rotationSpeed = rotationSpeed;
            
            RBody.isKinematic = true;
        }
        else if (crScript)
        {
            Destroy(crScript);
        }
    } 
}
