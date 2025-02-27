using System.Collections;
using UnityEngine;

public class PreviewShotgunBall : BallPreview, IBallPreview
{
    private Ball ball;
    
    public void PlayPreview(GameObject previewBall)
    {
        ball = previewBall.GetComponent<Ball>();
        CoroutineDispatcher.Instance.RunCoroutine(PreviewRoutine(), CoroutineType.BallPreview);
    }

    public IEnumerator PreviewRoutine()
    {
        ball.Initialize(MainMenu.Context);
        while (true)
        {
            // MainMenu.Context.DrawTrajectory(MainMenu.Context.GetTrajectory().ToArray());
            EventBus.Publish(new NextShotCuedEvent());

            yield return new WaitForSeconds(1);
            
            ball.Shoot();
            EventBus.Publish(new BallShotEvent());

            yield return new WaitForSeconds(3);
        }
    }
}
