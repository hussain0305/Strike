using System.Collections;
using System.Threading;
using UnityEngine;

public class BallLandingIndicator : MonoBehaviour
{
    public Transform sphere;
    public BallLandingIndicatorPulse pulse;

    private Coroutine animationCoroutine;

    private WaitForSeconds waitHandler;
    private WaitForSeconds WaitHandler
    {
        get
        {
            if (waitHandler == null)
            {
                waitHandler = new WaitForSeconds(1);
            }
            return waitHandler;
        }
    }
    
    public void Animate()
    {
        if (animationCoroutine != null)
        {
            return;
        }

        animationCoroutine = StartCoroutine(AnimationRoutine());
    }

    public void ResetAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    private IEnumerator AnimationRoutine()
    {
        float sphereScaleTime = 0.75f;
        float timePassed = 0f;
        sphere.localScale = Vector3.one;
        
        pulse.Activate();
        while (timePassed <= sphereScaleTime)
        {
            sphere.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, Easings.EaseInQuart(Mathf.Clamp01(timePassed / sphereScaleTime)));
            
            timePassed += Time.deltaTime;
            yield return null;
        }

        yield return WaitHandler;
        animationCoroutine = null;
    }
}
