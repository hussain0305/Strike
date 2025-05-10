using System.Collections.Generic;
using UnityEngine;

public class RicochetModule : IBallAbilityModule
{
    Ball ball;
    IContextProvider context;
    public AbilityAxis Axis => AbilityAxis.Collision;
    
    private readonly float sweepAngle = 60f;
    private readonly float angleStep = 15f;
    private readonly float ricochetLength = 40f;
    private LayerMask collideWithBallLayer;
    private const int NUM_RICOCHETS = 2;
    private int numTimesRicocheted = 0;
    
    public void Initialize(Ball ownerBall, IContextProvider _context)
    {
        ball = ownerBall;
        context  = _context;
        collideWithBallLayer = LayerMask.GetMask("CollideWithBall");
    }

    public void OnBallShot(BallShotEvent e)
    {
        numTimesRicocheted = 0;
    }

    public void OnProjectilesSpawned(ProjectilesSpawnedEvent e)
    {
        
    }

    public void OnNextShotCued(NextShotCuedEvent e)
    {
        numTimesRicocheted = 0;
    }

    public void OnHitSomething(BallHitSomethingEvent e)
    {
        if (numTimesRicocheted >= NUM_RICOCHETS)
        {
            return;
        }
        
        ContactPoint contact = e.Collision.contacts[0];
        Vector3 hitPoint = contact.point;
        Vector3 normal = contact.normal;

        float speed = e.ImpactVelocity.magnitude;

        Vector3 flatNormal = new Vector3(normal.x, 0f, normal.z).normalized;
        if (flatNormal == Vector3.zero)
        {
            flatNormal = Vector3.forward;
        }

        List<float> angleOffsets = new List<float> { 0f };
        int maxSteps = Mathf.CeilToInt(sweepAngle / angleStep);
        for (int i = 1; i <= maxSteps; i++)
        {
            float a = i * angleStep;
            if (a <= sweepAngle)
            {
                angleOffsets.Add( a);
                angleOffsets.Add(-a);
            }
        }

        foreach (float offset in angleOffsets)
        {
            Vector3 dir = Quaternion.AngleAxis(offset, Vector3.up) * flatNormal;
            Debug.DrawRay(hitPoint, dir * ricochetLength, Color.yellow, 5f);

            if (Physics.Raycast(hitPoint, dir, out RaycastHit hitInfo, ricochetLength, collideWithBallLayer, QueryTriggerInteraction.Collide))
            {
                var collectible = hitInfo.collider.GetComponent<Collectible>();
                if (collectible != null && collectible.CanBeCollected())
                {
                    numTimesRicocheted++;
                    Debug.Log(">>> R number " + numTimesRicocheted);
                    Debug.DrawLine(hitPoint, hitPoint + dir * ricochetLength, Color.green, 5f);
                    ball.rb.linearVelocity = dir * speed;
                    break;
                }
            }
        }

    }
    public void Cleanup()
    {
        
    }
}
