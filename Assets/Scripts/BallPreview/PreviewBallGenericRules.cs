using System.Collections;
using UnityEngine;

public class PreviewBallGenericRules : BallPreview, IBallPreview
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
        float previewSpin = Mathf.Min(properties.spin / 5, 1.5f);
        
        SpoofedTrajectoryParameters parameters = new SpoofedTrajectoryParameters();
        parameters.CreateFromBallProperties(properties);
        
        while (true)
        {
            EventBus.Publish(new NextShotCuedEvent());
            Context.SpoofNewTrajectory(parameters);
            
            Context.DrawTrajectory(ball.CalculateTrajectory().ToArray());

            yield return new WaitForSeconds(1);
            
            ball.Shoot();
            EventBus.Publish(new BallShotEvent());

            yield return new WaitForSeconds(3);
        }
    }
}
