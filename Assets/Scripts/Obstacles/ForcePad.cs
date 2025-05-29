using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class ForcePad : Obstacle
{
    public float pushForce = 10f;

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        StartCoroutine(ApplyForce(other.gameObject));
    }

    private IEnumerator ApplyForce(GameObject ballObject)
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

        rBody.linearVelocity = pushDir * pushForce;
    }
}