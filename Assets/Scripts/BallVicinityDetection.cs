using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallVicinityDetection : MonoBehaviour
{
    public Ball ball;
    private Vector3 lastPosition;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (!GameManager.IsGameplayActive)
            return;
        
        if (ball.collidedWithSomething)
            return;

        Vector3 currentPosition = transform.position;
        Vector3 motionVector = currentPosition - lastPosition;

        float rayLength = GameManager.Instance.powerInput.Power / 4f;
        
        if (motionVector.sqrMagnitude > 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(lastPosition, motionVector.normalized, out hit, rayLength))
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("CollideWithBallUnaffected") &&
                    hit.collider.gameObject != ball.gameObject)
                {
                    ball.collidedWithSomething = true;
                }
            }
            Debug.DrawLine(lastPosition, lastPosition + motionVector.normalized * rayLength, Color.red, 2f);
        }

        lastPosition = currentPosition;
    }
}
