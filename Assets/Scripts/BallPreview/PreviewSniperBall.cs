using System.Collections;
using UnityEngine;

public class PreviewSniperBall : BallPreview, IBallPreview
{
    private Ball ball;
    
    public void PlayPreview(GameObject previewBall)
    {
        ball = previewBall.GetComponent<Ball>();
        CoroutineDispatcher.Instance.RunCoroutine(PreviewRoutine(), CoroutineType.BallPreview);
    }

    public IEnumerator PreviewRoutine()
    {
        ball.Initialize(MainMenu.Context, MainMenu.TrajectoryModifier);
        Vector3[] drawnTrajectory = new[] { Vector3.zero, Vector3.zero };
        while (true)
        {
            EventBus.Publish(new NextShotCuedEvent());

            Context.SetSpoofedLaunchForce(5f);
            Context.SetSpoofedSpinVector(Vector2.zero);

            float aimPreviewDuration = 2f;
            float sweepAngle = 20f;
            float speed = 1f;
            while (aimPreviewDuration > 0)
            {
                float angle = Mathf.Sin(Time.time * speed) * sweepAngle;
                Context.SetSpoofedLaunchAngle(Quaternion.Euler(-10f, angle, 0));
                Context.DrawTrajectory(drawnTrajectory);
                aimPreviewDuration -= Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(1);
            
            ball.Shoot();
            EventBus.Publish(new BallShotEvent());

            yield return new WaitForSeconds(3);
        }
    }
}
