using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallVicinityDetection : MonoBehaviour
{
    private Ball ball;
    private Ball Ball => ball ??= GetComponentInParent<Ball>();
    private Vector3 lastPosition;
    private int raycastLayerMask;
    private int portalLayerMask;
    private const float portalRayDistance = 1f;
    
    private void Start()
    {
        lastPosition = transform.position;
        raycastLayerMask = ~LayerMask.GetMask("CollideWithBallUnaffected", "OtherCollectingObject", "Portal");
        portalLayerMask = LayerMask.GetMask("Portal"); }

    private void Update()
    {
        if (!GameManager.IsGameplayActive)
            return;

        Vector3 currentPosition = transform.position;
        Vector3 motionVector = currentPosition - lastPosition;

        float rayLength = GameManager.Instance.PowerInput.Power / 4f;
        
        if (motionVector.sqrMagnitude > 0)
        {
            if (!Ball.collidedWithSomething)
            {
                RaycastHit hit;
                if (Physics.Raycast(lastPosition, motionVector.normalized, out hit, rayLength, raycastLayerMask))
                {
                    if (hit.collider.gameObject != Ball.gameObject)
                    {
                        Ball.collidedWithSomething = true;
                    }
                }
                // Debug.DrawLine(lastPosition, lastPosition + motionVector.normalized * rayLength, Color.red, 2f);
            }
            
            RaycastHit portalHit;
            Debug.DrawLine(lastPosition, lastPosition + motionVector.normalized * portalRayDistance, Color.red, 2f);
            if (Physics.Raycast(lastPosition, motionVector.normalized, out portalHit, portalRayDistance, portalLayerMask))
            {
                var portal = portalHit.collider.GetComponentInParent<Portal>();
                if (portal != null && portal.linkedPortal != null)
                {
                    bool enteredFromFront = Vector3.Dot(motionVector, portal.transform.forward) > 0f;
                    Vector3 localPos = portal.transform.InverseTransformPoint(portalHit.point);
                    Quaternion localRot = Quaternion.Inverse(portal.transform.rotation) * Ball.transform.rotation;

                    if (!enteredFromFront)
                        localRot = Quaternion.Euler(0f, 180f, 0f) * localRot;
                    
                    Transform exitT = portal.linkedPortal.transform;
                    Vector3 exitPos = exitT.TransformPoint(localPos) + exitT.forward * 0.1f;
                    Quaternion exitRot = exitT.rotation * localRot;

                    var traveler = Ball.GetComponent<PortalTraveler>();
                    if (traveler != null)
                    {
                        traveler.Teleport(exitPos, exitRot);
                    }
                    else
                    {
                        Debug.LogWarning("Ball needs a PortalTraveler component!");
                    }

                    lastPosition = exitPos;
                    return;
                }
            }
        }

        lastPosition = currentPosition;
    }
}
