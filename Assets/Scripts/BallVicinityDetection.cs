using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallVicinityDetection : MonoBehaviour
{
    private Ball ball;
    private Ball Ball => ball ??= GetComponentInParent<Ball>();
    
    private PortalTraveler traveler;
    private PortalTraveler PortalTraveler => traveler ??= GetComponentInParent<PortalTraveler>();
    
    private Vector3 lastPosition;
    private int raycastLayerMask;
    private int portalLayerMask;
    private const float portalRayDistance = 3f;
    
    private GameManager gameManager;
    private GameManager GameManager => gameManager ??= Ball.GameManager;
    
    private void Start()
    {
        lastPosition = transform.position;
        raycastLayerMask = ~LayerMask.GetMask("CollideWithBallUnaffected", "OtherCollectingObject", "Portal");
        portalLayerMask = LayerMask.GetMask("Portal");
        
    }

    private void Update()
    {
        if (!GameManager || !GameManager.IsGameplayActive)
            return;

        Vector3 currentPosition = transform.position;
        Vector3 motionVector = currentPosition - lastPosition;

        float rayLength = GameManager.PowerInput.Power / 4f;
        
        if (motionVector.sqrMagnitude > 0 && !Ball.collidedWithSomething)
        {
            bool hitPortal = Physics.SphereCast(lastPosition, Global.BALL_RADIUS, motionVector.normalized, out RaycastHit portalHit,
                rayLength, portalLayerMask );
            
            if (Physics.SphereCast(lastPosition, Global.BALL_RADIUS, motionVector.normalized, out var hit, rayLength, raycastLayerMask))
            {
                if (hitPortal && portalHit.distance < hit.distance)
                {
                    
                }
                else if (hit.collider.gameObject != Ball.gameObject && !PortalTraveler.isPassingThroughPortal)
                {
                    Ball.collidedWithSomething = true;
                }
            }
        }

        lastPosition = currentPosition;
    }
}
