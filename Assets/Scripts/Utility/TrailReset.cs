using System;
using UnityEngine;
using UnityEngine.Serialization;

public class TrailReset : MonoBehaviour
{
    private TrailRenderer trail;
    private TrailRenderer Trail => trail ??= GetComponent<TrailRenderer>();

    private void OnEnable()
    {
        EventBus.Subscribe<BallShotEvent>(ResumeTrail);
        EventBus.Subscribe<NextShotCuedEvent>(ResetTrail);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BallShotEvent>(ResumeTrail);
        EventBus.Unsubscribe<NextShotCuedEvent>(ResetTrail);
    }
    
    public void ResumeTrail(BallShotEvent e)
    {
        Trail.Clear();
        Trail.emitting = true;
    }

    public void ResetTrail(NextShotCuedEvent e)
    {
        Trail.emitting = false;
        Trail.Clear();
    }
}
