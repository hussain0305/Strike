using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class ForcePad : Obstacle
{
    public float pushForceMagnitude = 30f;
    public float delay = 0.15f;

    void OnTriggerEnter(Collider other)
    {
        StartCoroutine(ApplyForceAbsolute(other.gameObject));
    }

    private IEnumerator ApplyForceAbsolute(GameObject ballObject)
    {
        Rigidbody rBody = ballObject.GetComponent<Rigidbody>();
        Ball ball = ballObject.GetComponent<Ball>();
        
        if (!rBody || !ball) 
            yield break;
        
        ball.collidedWithSomething = true;
        
        yield return new WaitForSeconds(delay);
        
        rBody.linearVelocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;

        Vector3 pushDir = transform.forward.normalized;

        rBody.linearVelocity = pushDir * pushForceMagnitude;
    }
    
    private IEnumerator ApplyForceAdditive(GameObject ballObject)
    {
        Rigidbody rBody = ballObject.GetComponent<Rigidbody>();
        Ball ball = ballObject.GetComponent<Ball>();
        
        if (!rBody || !ball) 
            yield break;
        
        Vector3 currentVelocity = rBody.isKinematic ? ball.LastKnownVelocity : rBody.linearVelocity;
        
        ball.collidedWithSomething = true;
        
        yield return null;
        
        Vector3 push = transform.forward.normalized * pushForceMagnitude;

        rBody.linearVelocity = push + currentVelocity;
    }
    
    public override void CheckForContinuousRotation(Vector3 rotationAxis, float rotationSpeed)
    {
        bool objRotates = rotationAxis != Vector3.zero && rotationSpeed != 0;
        var crScript = axisOfRotation.GetComponent<ContinuousRotation>();

        if (objRotates)
        {
            if (!crScript)
                crScript = axisOfRotation.AddComponent<ContinuousRotation>();
            
            crScript.enabled = true;
            crScript.rotationAxis = rotationAxis;
            crScript.rotationSpeed = rotationSpeed;
            crScript.RotateAroundWorld();
            
            RBody.isKinematic = true;
        }
        else if (crScript)
        {
            Destroy(crScript);
        }
    } 
}