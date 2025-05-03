using UnityEngine;
using System.Collections.Generic;

public class StickyModule : IBallAbilityModule
{
    public AbilityAxis Axis => AbilityAxis.Collision;

    private Ball ball;
    private IContextProvider context;
    private Rigidbody body;

    private Transform defaultParent;
    private Collider ballCollider;
    private LayerMask stickySurfaces;
    
    public void Initialize(Ball _ownerBall, IContextProvider _context)
    {
        ball = _ownerBall;
        context = _context;
        body = ball.GetComponent<Rigidbody>();
        ballCollider = ball.GetComponentInChildren<Collider>();
        defaultParent = context.GetBallParent();
        stickySurfaces = LayerMask.GetMask("CollideWithBall");
    }

    public void OnBallShot(BallShotEvent e) { }
    public void OnProjectilesSpawned(ProjectilesSpawnedEvent e) { }

    public void OnNextShotCued(NextShotCuedEvent e)
    {
        ball.transform.parent = defaultParent;
        ballCollider.enabled = true;
    }
    public void Cleanup() { }

    public void OnHitSomething(BallHitSomethingEvent e)
    {
        int stickyMask = Global.stickySurfaces.value;
        int hitLayer = e.Collision.gameObject.layer;
        if ((stickyMask & (1 << hitLayer)) == 0)
            return;
        
        Vector3 impactVel = e.ImpactVelocity;
        Vector3 dir = impactVel.normalized;
        
        float travelThisFrame = impactVel.magnitude * Time.fixedDeltaTime;
        Vector3 origin = ball.transform.position - dir * travelThisFrame;
        float maxDistance = travelThisFrame * 2f; 
        
        var sphere = ballCollider as SphereCollider;
        float radius = (sphere != null) ? sphere.radius * Mathf.Max(
                ball.transform.lossyScale.x,
                ball.transform.lossyScale.y,
                ball.transform.lossyScale.z)
            : 0.25f;
        
        Vector3 to = origin + (dir * maxDistance);
        float dist = Vector3.Distance(origin, to);
        Debug.DrawRay(origin, dir * dist, Color.red, 2f, false);
        
        RaycastHit hitInfo;
        bool didHit = Physics.SphereCast(origin, radius, dir, out hitInfo, maxDistance, stickyMask, QueryTriggerInteraction.Ignore);
        
        Vector3 contactPoint;
        Transform hitTransform;

        if (didHit)
        {
            Debug.Log(">>> Detected hit");
            contactPoint = hitInfo.point;
            hitTransform = hitInfo.collider.transform;
        }
        else
        {
            Debug.Log(">>> Unity's hit");
            contactPoint = e.Collision.contacts[0].point;
            hitTransform = e.Collision.transform;
        }
        
        ball.shouldResumePhysics = false;
        body.useGravity = false;
        body.isKinematic = true;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        ballCollider.enabled = false;
        ball.transform.position = contactPoint;
        
        var hitRb = e.Collision.rigidbody;
        if (hitRb != null)
        {
            hitRb.AddForceAtPosition( e.ImpactVelocity * body.mass * 10, contactPoint, ForceMode.Impulse);
        }

        ball.transform.SetParent(e.Collision.transform, true);
    }
}