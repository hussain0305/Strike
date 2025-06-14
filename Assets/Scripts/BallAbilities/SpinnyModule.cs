using UnityEngine;

public class SpinnyModule : IBallAbilityModule
{
    public AbilityAxis Axis => AbilityAxis.SpinModifier;
    private IContextProvider context;
    private Ball ball;

    private float spin;
    
    public SpinnyModule(float spinVal)
    {
        spin = spinVal;
    }
    
    public void Initialize(Ball _ownerBall, IContextProvider _context)
    {
        context = _context;
        ball = _ownerBall;
        
        ball.SetSpin(spin);
    }

    public void OnBallShot(BallShotEvent e) { }

    public void OnProjectilesSpawned(ProjectilesSpawnedEvent e) { }

    public void OnNextShotCued(NextShotCuedEvent e) { }

    public void OnHitSomething(BallHitSomethingEvent e) { }

    public void Cleanup() { }
}
