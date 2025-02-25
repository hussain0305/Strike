using System.Collections;
using UnityEngine;

public class PreviewSoccerBall : BallPreview, IBallPreview
{
    private Ball ball;
    
    public void PlayPreview(GameObject previewBall)
    {
        ball = previewBall.GetComponent<Ball>();
        Debug.Log(">>> In soccer ball play preview");
        CoroutineDispatcher.Instance.RunCoroutine(PreviewRoutine(), CoroutineType.BallPreview);
    }

    IEnumerator PreviewRoutine()
    {
        while (true)
        {
            MainMenu.Context.DrawTrajectory(MainMenu.Context.GetTrajectory().ToArray());

            yield return new WaitForSeconds(1);
            
            ball.Initialize(MainMenu.Context);
            ball.Shoot();
            
            yield return new WaitForSeconds(2);
            TriggerResetPreview();
            ball.ResetBall();
        }
    }
}
