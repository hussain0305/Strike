using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DangerBorderFlicker : MonoBehaviour
{
    [SerializeField] private Image borderImage;
    [SerializeField] private float flickerDuration = 0.2f;
    [SerializeField] private float maxAlpha = 0.7f;

    private Coroutine flickerRoutine;

    private void OnEnable()
    {
        EventBus.Subscribe<CollectibleHitEvent>(OnCollectibleHit);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<CollectibleHitEvent>(OnCollectibleHit);
    }

    private void OnCollectibleHit(CollectibleHitEvent e)
    {
        if (e.Value < 0)
        {
            TriggerFlicker();
        }
    }

    public void TriggerFlicker()
    {
        if (flickerRoutine != null)
            StopCoroutine(flickerRoutine);
        flickerRoutine = StartCoroutine(Flicker());
    }

    private IEnumerator Flicker()
    {
        Color col = borderImage.color;
        col.a = maxAlpha;
        borderImage.color = col;

        float elapsed = 0f;
        while (elapsed < flickerDuration)
        {
            float t = elapsed / flickerDuration;
            col.a = Mathf.Lerp(maxAlpha, 0f, t);
            borderImage.color = col;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        col.a = 0f;
        borderImage.color = col;
    }
}