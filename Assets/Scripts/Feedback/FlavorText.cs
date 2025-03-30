using System.Collections;
using UnityEngine;

public class FlavorText : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float lifetime = 1.2f;
    [SerializeField] private float popInTime = 0.2f;
    [SerializeField] private float scaleDownTime = 0.2f;

    private Vector3 originalScale;
    private FlavorTextSpawner spawner;

    private void Awake()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    public void Init(Sprite sprite, FlavorTextSpawner _spawner)
    {
        spriteRenderer.sprite = sprite;
        spawner = _spawner;
        StopAllCoroutines();
        StartCoroutine(PlayPopupRoutine());
    }

    private IEnumerator PlayPopupRoutine()
    {
        float t = 0f;
        while (t < popInTime)
        {
            t += Time.deltaTime;
            float eased = Easings.EaseOutBack(t / popInTime);
            transform.localScale = originalScale * eased;
            yield return null;
        }

        transform.localScale = originalScale;

        yield return new WaitForSeconds(lifetime - popInTime - scaleDownTime);

        t = 0f;
        while (t < scaleDownTime)
        {
            t += Time.deltaTime;
            float eased = 1f - Easings.EaseInSine(t / scaleDownTime);
            transform.localScale = originalScale * eased;
            yield return null;
        }

        transform.localScale = Vector3.zero;
        spawner.ReturnToPool(gameObject);
    }
}