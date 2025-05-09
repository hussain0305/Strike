using System;
using System.Collections;
using UnityEngine;

public class MultiplierTokenHitReaction : MonoBehaviour, ICollectibleHitReaction
{
    public ParticleSystem hitPFX;
    private Collectible collectible;
    private Collectible Collectible => collectible ??= GetComponent<Collectible>();

    private Vector3 defaultRotation;
    
    private void Start()
    {
        defaultRotation = Collectible.transform.localEulerAngles;
    }

    private void OnEnable()
    {
        SetDefaultVisuals(null);
        EventBus.Subscribe<NextShotCuedEvent>(SetDefaultVisuals);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<NextShotCuedEvent>(SetDefaultVisuals);
    }

    public void UpdatePoints(int points)
    {
        
    }

    public void CheckIfHitsExhasuted(int numTimesCollected, int numTimesCanBeCollected)
    {
        if (numTimesCollected > numTimesCanBeCollected)
            return;

        hitPFX.Play();
        StartCoroutine(ScaleAndDisappear());
    }

    public void SetDefaultVisuals(NextShotCuedEvent e)
    {
        transform.localScale = Vector3.one;
    }

    private IEnumerator ScaleAndDisappear()
    {
        float disableAfter = hitPFX.main.duration;
        float timePassed = 0f;
        float effectDuration = disableAfter;
        while (timePassed < effectDuration)
        {
            float scale = Easings.EaseScaleUpAndVanish(Mathf.Clamp01(timePassed / effectDuration));
            transform.localScale = Vector3.one * scale;

            timePassed += Time.deltaTime;
            yield return null;
        }
    }
}
