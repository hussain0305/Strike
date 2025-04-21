using UnityEngine;
using System.Collections.Generic;

public class StickyModule : IBallAbilityModule
{
    public AbilityAxis Axis => AbilityAxis.Collision;

    private Ball ball;
    private IContextProvider context;
    private Rigidbody body;

    public void Initialize(Ball _ownerBall, IContextProvider _context)
    {
        ball = _ownerBall;
        context = _context;
        body = ball.GetComponent<Rigidbody>();
    }

    public void OnBallShot(BallShotEvent e) { }
    public void OnProjectilesSpawned(ProjectilesSpawnedEvent e) { }
    public void OnNextShotCued(NextShotCuedEvent e) { }
    public void Cleanup() { }

    public void OnHitSomething(BallHitSomethingEvent e)
    {
        if (ball)
        {
            body.useGravity    = false;
            body.isKinematic   = true;
            body.linearVelocity      = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }
}