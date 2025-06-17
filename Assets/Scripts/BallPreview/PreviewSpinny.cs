using System.Collections;
using UnityEngine;

public class PreviewSpinny : BallPreview, IBallPreview
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
        parameters.CreateFromBallProperties(properties);
        parameters.angleX = new MinMaxFloat(-45, -60);
        parameters.angleY = new MinMaxFloat(0, 0);

        while (true)
        {
            bool higherEnd = Random.Range(0, 10) < 5;
            parameters.spinX = new MinMaxFloat(higherEnd ? 0.1f : -0.15f, higherEnd ? 0.15f : -0.1f);
            parameters.spinY = new MinMaxFloat(-0.06f, -0.02f);
            Context.SpoofNewTrajectory(parameters);
            Context.DrawTrajectory(ball.CalculateTrajectory().ToArray());
            EventBus.Publish(new NextShotCuedEvent());

            yield return new WaitForSeconds(1);
            
            ball.Shoot();
            EventBus.Publish(new BallShotEvent());

            yield return new WaitForSeconds(3);
        }
    }
}
