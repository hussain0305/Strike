using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallVicinityDetection : MonoBehaviour
{
    public Ball ball;
    private Vector3 lastPosition;
    private int raycastLayerMask;
    
    private void Start()
    {
        lastPosition = transform.position;
        raycastLayerMask = ~LayerMask.GetMask("CollideWithBallUnaffected", "OtherCollectingObject");
    }

    private void Update()
    {
        if (!GameManager.IsGameplayActive)
            return;
        
        if (ball.collidedWithSomething)
            return;

        Vector3 currentPosition = transform.position;
        Vector3 motionVector = currentPosition - lastPosition;

        float rayLength = GameManager.Instance.PowerInput.Power / 4f;
        
        if (motionVector.sqrMagnitude > 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(lastPosition, motionVector.normalized, out hit, rayLength, raycastLayerMask))
            {
                if (hit.collider.gameObject != ball.gameObject)
                {
                    ball.collidedWithSomething = true;
                }
            }
            Debug.DrawLine(lastPosition, lastPosition + motionVector.normalized * rayLength, Color.red, 2f);
        }

        lastPosition = currentPosition;
    }
}
