using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallVicinityDetection : MonoBehaviour
{
    public Ball ball;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other && other.gameObject.layer == LayerMask.NameToLayer("CollideWithBallUnaffected"))
        {
            return;
        }
        ball.collidedWithSomething = true;
    }
}
