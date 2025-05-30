using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class ForcePad : Obstacle
{
    private float pushForceMagnitude = 30f;

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
        
        yield return null;
        
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
}