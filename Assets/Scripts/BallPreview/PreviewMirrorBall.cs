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
        SpoofedTrajectoryParameters parameters = new SpoofedTrajectoryParameters();
        parameters.CreateFromBallProperties(properties);
        parameters.spinX = new MinMaxFloat(0.05f, 0.1f);
        parameters.spinY = new MinMaxFloat(0, 0);
        parameters.angleX = new MinMaxFloat(-30, -50);
        parameters.angleY = new MinMaxFloat(5, 10);
        
        while (true)
        {
            MainMenu.Context.SpoofNewTrajectory(parameters);
            MainMenu.Context.DrawTrajectory(ball.CalculateTrajectory().ToArray());
            EventBus.Publish(new NextShotCuedEvent());

            yield return new WaitForSeconds(1);
            
            ball.Shoot();
            EventBus.Publish(new BallShotEvent());

            yield return new WaitForSeconds(3);
        }
    }
}
