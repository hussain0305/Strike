using System.Collections.Generic;
using UnityEngine;

public class RicochetConfig
{
    public float sweepAngle;
    public float angleStep;
    public float ricochetLength;
    public int maxRicochets;

    public RicochetConfig(float _sweepAngle, float _angleStep, float _ricochetLength, int _maxRicochets)
    {
        sweepAngle = _sweepAngle;
        angleStep = _angleStep;
        ricochetLength = _ricochetLength;
        maxRicochets = _maxRicochets;
    }
}

public class RicochetModule : IBallAbilityModule
{
    Ball ball;
    IContextProvider context;
    public AbilityAxis Axis => AbilityAxis.Collision;
    
    private LayerMask collideWithBallLayer;
    private int numTimesRicocheted = 0;
    private RicochetConfig ricochetConfig;
    
    public void Initialize(Ball ownerBall, IContextProvider _context)
    {
        ball = ownerBall;
        context  = _context;
        collideWithBallLayer = LayerMask.GetMask("CollideWithBall");

        if (context is MenuContext)
        {
            ricochetConfig = new RicochetConfig(180, 15, 60, 12);
        }
        else if (context is InGameContext)
        {
            ricochetConfig = new RicochetConfig(60, 15, 40, 3);
        }
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
        if (numTimesRicocheted >= ricochetConfig.maxRicochets)
            return;
        
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
        int maxSteps = Mathf.CeilToInt(ricochetConfig.sweepAngle / ricochetConfig.angleStep);
        for (int i = 1; i <= maxSteps; i++)
        {
            float a = i * ricochetConfig.angleStep;
            if (a <= ricochetConfig.sweepAngle)
            {
                angleOffsets.Add( a);
                angleOffsets.Add(-a);
            }
        }

        foreach (float offset in angleOffsets)
        {
            Vector3 dir = Quaternion.AngleAxis(offset, Vector3.up) * flatNormal;
            Debug.DrawRay(hitPoint, dir * ricochetConfig.ricochetLength, Color.yellow, 5f);

            if (Physics.Raycast(hitPoint, dir, out RaycastHit hitInfo, ricochetConfig.ricochetLength, collideWithBallLayer, QueryTriggerInteraction.Collide))
            {
                var collectible = hitInfo.collider.GetComponent<ICollectible>();
                if (collectible != null && collectible.CanBeCollected())
                {
                    numTimesRicocheted++;
                    Debug.DrawLine(hitPoint, hitPoint + dir * ricochetConfig.ricochetLength, Color.green, 5f);
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
