using System.Collections;
using UnityEngine;

public class PreviewShotgunBall : BallPreview, IBallPreview
{
    private Ball ball;
    
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
        parameters.spinX = new MinMaxFloat(0, 0);
        parameters.spinY = new MinMaxFloat(0, 0);
        parameters.angleX = new MinMaxFloat(-35, -20);
        parameters.angleY = new MinMaxFloat(-10, 15);
        parameters.launchForce = new MinMaxFloat(18, 20);

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
