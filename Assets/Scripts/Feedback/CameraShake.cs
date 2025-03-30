using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPos;
    private float shakeTimeRemaining;
    private float shakePower;
    private float shakeFadeTime;

    private void Awake()
    {
        originalPos = transform.localPosition;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<CollectibleHitEvent>(Shake);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<CollectibleHitEvent>(Shake);
    }

    public void Shake(CollectibleHitEvent e)
    {
        GetPowerAndDuration(e.Type, e.Value, out float duration, out float power);
        if (duration <= 0 || power <= 0) return;

        if (e.Value < 0)
        {
            duration = 0.1f;
            power = 0.15f;
        }
        
        originalPos = transform.localPosition;
        shakeTimeRemaining = duration;
        shakePower = power;
        shakeFadeTime = power / duration;
    }

    public void GetPowerAndDuration(CollectibleType type, int value, out float duration, out float power)
    {
        if (type == CollectibleType.Multiple)
        {
            duration = 0;
            power = 0;
            return;
        }

        int clampedValue = Mathf.Clamp(Mathf.Abs(value), 5, 100);

        float minPower = 0.1f;
        float maxPower = 1f;
        float minDuration = 0.1f;
        float maxDuration = 0.3f;

        float t = (clampedValue - 5) / 95f;
        duration = Mathf.Lerp(minDuration, maxDuration, t);
        power = Mathf.Lerp(minPower, maxPower, t);
    }
    
    private void LateUpdate()
    {
        if (shakeTimeRemaining > 0)
        {
            shakeTimeRemaining -= Time.unscaledDeltaTime;

            float x = Random.Range(-1f, 1f) * shakePower;
            float y = Random.Range(-1f, 1f) * shakePower;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            shakePower = Mathf.MoveTowards(shakePower, 0f, shakeFadeTime * Time.unscaledDeltaTime);

            if (shakeTimeRemaining <= 0f)
                transform.localPosition = originalPos;
        }
    }
}