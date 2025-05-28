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
        if (mirroredBall)
        {
            mirroredBall.transform.parent = ball.transform.parent;
            mirroredBall.transform.localScale = ball.transform.localScale;
        }
    }

    public void SetShadowBall(GameObject go)
    {
        if (go)
        {
            mirroredBall = go;
        }

        if (ball)
        {
            mirroredBall.transform.parent = ball.transform.parent;
            mirroredBall.transform.localScale = ball.transform.localScale;
        }
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
        DiscardMirroredBall();
    }

    public void DiscardMirroredBall()
    {
        if (mirroredBall)
        {
            GameObject.Destroy(mirroredBall);
        }
    }

    public void OnUpdate()
    {
        mirroredBall.transform.localPosition = new Vector3(-ball.transform.localPosition.x, ball.transform.localPosition.y, ball.transform.localPosition.z);
    }
}
