using UnityEngine;

public class MirrorModule : IBallAbilityModule, IBallAbilityUpdateableModule
{
    Ball ball;
    IContextProvider context;
    public AbilityAxis Axis => AbilityAxis.Spawn;
    
    private GameObject mirroredBall;
    private IBallAbilityUpdateableModule ballAbilityUpdateableModuleImplementation;

    public void Initialize(Ball ownerBall, IContextProvider _context)
    {
        ball = ownerBall;
        context  = _context;
    }

    public void SetShadowBall(GameObject go)
    {
        mirroredBall = go;
    }

    public void OnBallShot(BallShotEvent e)
    {
        mirroredBall.SetActive(true);
    }

    public void OnProjectilesSpawned(ProjectilesSpawnedEvent e)
    {
        
    }

    public void OnNextShotCued(NextShotCuedEvent e)
    {
        mirroredBall.SetActive(false);
    }

    public void OnHitSomething(BallHitSomethingEvent e)
    {
        
    }

    public void Cleanup()
    {
        
    }

    public void OnUpdate()
    {
        mirroredBall.transform.position = new Vector3(-ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);
    }
}
