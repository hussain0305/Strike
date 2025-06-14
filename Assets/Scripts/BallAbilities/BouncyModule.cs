using UnityEngine;

public class BouncyModule : IBallAbilityModule
{
    public AbilityAxis Axis => AbilityAxis.PhysicsMaterial;
    private IContextProvider context;
    private PhysicsMaterial bouncyMaterial;
    private Ball ball;

    public BouncyModule(PhysicsMaterial _bouncyMaterial)
    {
        bouncyMaterial = _bouncyMaterial;
    }
    
    public void Initialize(Ball _ownerBall, IContextProvider _context)
    {
        ball = _ownerBall;
        context = _context;
        ball.GetComponent<Collider>().sharedMaterial = bouncyMaterial;
    }

    public void OnBallShot(BallShotEvent e) { }

    public void OnProjectilesSpawned(ProjectilesSpawnedEvent e)
    {
        foreach (GameObject projectile in e.projectiles)
        {
            Collider collider = projectile.GetComponent<Collider>();
            if (collider)
            {
                collider.sharedMaterial = bouncyMaterial;
            }
        }
    }

    public void OnNextShotCued(NextShotCuedEvent e) { }

    public void OnHitSomething(BallHitSomethingEvent e) { }

    public void Cleanup() { }
}
