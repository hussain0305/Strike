using UnityEngine;
using System.Collections;

public class PreviewStickyBall : BallPreview, IBallPreview
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
        while (true)
        {
            Context.SpoofNewTrajectory();
            Context.DrawTrajectory(ball.CalculateTrajectory().ToArray());
            EventBus.Publish(new NextShotCuedEvent());

            yield return new WaitForSeconds(1);
            
            ball.Shoot();
            EventBus.Publish(new BallShotEvent());

            yield return new WaitForSeconds(3);
        }
    }
}
