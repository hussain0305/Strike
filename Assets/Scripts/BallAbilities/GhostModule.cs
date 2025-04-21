using UnityEngine;

public class GhostModule : IBallAbilityModule
{
    public AbilityAxis Axis => AbilityAxis.Collision;

    private Ball ball;
    private IContextProvider context;

    public void Initialize(Ball _ownerBall, IContextProvider _context)
    {
        ball = _ownerBall;
        context = _context;
    }

    public void OnBallShot(BallShotEvent e) { }

    public void OnProjectilesSpawned(ProjectilesSpawnedEvent e)
    {
        foreach (GameObject proj in e.projectiles)
        {
            Collider collider = proj.GetComponent<Collider>();
            collider.isTrigger = true;
        }
    }

    public void OnNextShotCued(NextShotCuedEvent e) { }
    public void OnHitSomething(BallHitSomethingEvent e) { }
    public void Cleanup() { }
}
