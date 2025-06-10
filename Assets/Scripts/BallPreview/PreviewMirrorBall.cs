using System;
using System.Collections;
using UnityEngine;

public class PreviewMirrorBall : BallPreview, IBallPreview
{
    private Ball ball;

    private void OnEnable()
    {
        EventBus.Subscribe<BallSelectedEvent>(BallSelected);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BallSelectedEvent>(BallSelected);
    }

    public void BallSelected(BallSelectedEvent e)
    {
        if (!ball)
            return;
        
        AbilityDriver ballAbilityDriver = ball.GetComponent<AbilityDriver>();
        foreach (var module in ballAbilityDriver.modules)
        {
            MirrorModule mm = module as MirrorModule;
            if (mm != null)
            {
                mm.DiscardMirroredBall();
                return;
            }
        }
    }

    public void PlayPreview(string ballID, GameObject previewBall)
    {
        Init(ballID);
        ball = previewBall.GetComponent<Ball>();
        CoroutineDispatcher.Instance.RunCoroutine(PreviewRoutine(), CoroutineType.BallPreview);
    }

    public IEnumerator PreviewRoutine()
    {
        ball.Initialize(MainMenu.Context, MainMenu.TrajectoryModifier, properties);
        while (true)
        {
            MainMenu.Context.SpoofNewTrajectory(spinXMin:0.35f, spinXMax:0.7f, launchForceMin:7f, launchForceMax:8f);
            MainMenu.Context.DrawTrajectory(ball.CalculateTrajectory().ToArray());
            EventBus.Publish(new NextShotCuedEvent());

            yield return new WaitForSeconds(1);
            
            ball.Shoot();
            EventBus.Publish(new BallShotEvent());

            yield return new WaitForSeconds(3);
        }
    }
}
