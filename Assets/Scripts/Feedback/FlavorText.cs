using System.Collections;
using TMPro;
using UnityEngine;

public class FlavorText : MonoBehaviour
{
    [SerializeField] private TextMeshPro message;
    private float lifetime = 1.2f;
    private float popInTime = 0.2f;
    private float scaleDownTime = 0.2f;
    private float verticalMoveAmount = 10f;
    private float jitterTime = 1f;
    private float jitterStrength = 2f;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private FlavorTextSpawner spawner;

    private void Awake()
    {
        transform.localScale = Vector3.zero;
    }

    public void Init(string messageText, Material material, float jitter, FlavorTextSpawner _spawner)
    {
        message.text = messageText;
        message.fontMaterial = material;
        jitterStrength = jitter;
        spawner = _spawner;

        transform.localScale = Vector3.zero;
        originalScale = Vector3.one;
        originalPosition = transform.position;

        StopAllCoroutines();
        StartCoroutine(PlayPopupRoutine());
    }

    private IEnumerator PlayPopupRoutine()
    {
        float totalDuration = popInTime + jitterTime + (lifetime - popInTime - scaleDownTime - jitterTime) + scaleDownTime;
        float elapsed = 0f;

        float t = 0f;
        // POP IN
        while (t < popInTime)
        {
            t += Time.deltaTime;
            elapsed += Time.deltaTime;

            float normalizedTime = t / popInTime;
            float scaleEase = Easings.EaseOutBack(normalizedTime);
            float verticalProgress = Mathf.Clamp01(elapsed / totalDuration);
            float moveEase = Easings.EaseOutQuint(verticalProgress);

            transform.localScale = originalScale * scaleEase;
            transform.position = originalPosition + Vector3.up * (verticalMoveAmount * moveEase);

            yield return null;
        }

        transform.localScale = originalScale;

        // JITTER
        t = 0f;
        while (t < jitterTime)
        {
            t += Time.deltaTime;
            elapsed += Time.deltaTime;

            float verticalProgress = Mathf.Clamp01(elapsed / totalDuration);
            float moveEase = Easings.EaseOutQuint(verticalProgress);

            float noiseX = (Mathf.PerlinNoise(Time.time * 60f, 0f) - 0.5f) * jitterStrength;
            float noiseY = (Mathf.PerlinNoise(0f, Time.time * 60f) - 0.5f) * jitterStrength;

            transform.position = originalPosition + Vector3.up * (verticalMoveAmount * moveEase) + new Vector3(noiseX, noiseY, 0f);

            yield return null;
        }

        // STILL phase (no jitter, just float)
        float stillTime = lifetime - popInTime - scaleDownTime - jitterTime;
        if (stillTime > 0f)
        {
            float stillT = 0f;
            while (stillT < stillTime)
            {
                stillT += Time.deltaTime;
                elapsed += Time.deltaTime;

                float verticalProgress = Mathf.Clamp01(elapsed / totalDuration);
                float moveEase = Easings.EaseOutQuint(verticalProgress);

                transform.position = originalPosition + Vector3.up * (verticalMoveAmount * moveEase);

                yield return null;
            }
        }

        // SCALE DOWN (while continuing upward float)
        t = 0f;
        while (t < scaleDownTime)
        {
            t += Time.deltaTime;
            elapsed += Time.deltaTime;

            float normalizedTime = t / scaleDownTime;
            float scaleEase = 1f - Easings.EaseInSine(normalizedTime);
            float verticalProgress = Mathf.Clamp01(elapsed / totalDuration);
            float moveEase = Easings.EaseOutQuint(verticalProgress);

            transform.localScale = originalScale * scaleEase;
            transform.position = originalPosition + Vector3.up * (verticalMoveAmount * moveEase);

            yield return null;
        }

        transform.localScale = Vector3.zero;
        transform.position = originalPosition;
        spawner.ReturnToPool(gameObject);
    }
}
