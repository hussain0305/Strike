using System;
using UnityEngine;

public class CollisionForce : MonoBehaviour
{
    private Ball ball;
    private float forceByMass;
    
    public void Initialize(Ball _ball, bool impartForce)
    {
        ball = _ball;
        forceByMass = ball.rb.mass;
        if (!impartForce)
            Destroy(this);
    }

    public void AddForceOnCollectibleHit(Rigidbody other, Vector3 point, Vector3 velocity)
    {
        if (other)
            other.AddForceAtPosition(velocity * forceByMass, point, ForceMode.Impulse);
    }
}
