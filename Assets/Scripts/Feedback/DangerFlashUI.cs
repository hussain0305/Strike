using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DangerFlashUI : MonoBehaviour
{
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private float maxAlpha = 0.4f;

    private Coroutine flashRoutine;

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
        if (e.Value <= -50)
        {
            TriggerFlash();
        }
    }

    public void TriggerFlash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        Color col = flashImage.color;
        col.a = maxAlpha;
        flashImage.color = col;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            float t = elapsed / flashDuration;
            col.a = Mathf.Lerp(maxAlpha, 0f, t);
            flashImage.color = col;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        col.a = 0f;
        flashImage.color = col;
    }
}