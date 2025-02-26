using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class NotableEventsManager : MonoBehaviour
{
    [System.Serializable]
    public struct HitTextInfo
    {
        public int numHits;
        public GameObject textGO;
    }

    [System.Serializable]
    public struct MultiplierUITextInfo
    {
        public GameObject textGO;
        public TextMeshProUGUI textComponent;
    }

    public HitTextInfo[] streakTexts;
    public MultiplierUITextInfo multiplierText;
    
    private int numberHitsInThisShot = 0;
    private GameObject currentActiveText;
    private Coroutine scalingCoroutine;

    private Coroutine multiplierAnimationRoutine;
    
    private void OnEnable()
    {
        EventBus.Subscribe<CollectibleHitEvent>(CollectibleHit);
        EventBus.Subscribe<NextShotCuedEvent>(NextShotCued);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<CollectibleHitEvent>(CollectibleHit);
        EventBus.Unsubscribe<NextShotCuedEvent>(NextShotCued);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<CollectibleHitEvent>(CollectibleHit);
        EventBus.Unsubscribe<NextShotCuedEvent>(NextShotCued);
    }

    public void CollectibleHit(CollectibleHitEvent e)
    {
        switch (e.Type)
        {
            case CollectibleType.Multiple:
                ShowMultiplierText(e.Value);
                break;
            case CollectibleType.Points:
                numberHitsInThisShot++;
                PlayHitMessage(numberHitsInThisShot);
                break;
        }
    }

    public void NextShotCued(NextShotCuedEvent e)
    {
        if (numberHitsInThisShot == 0)
        {
            DisableAllTexts();
        }
        else
        {
            PlayScaleDownAnimation(currentActiveText);
        }
        HideMultiplierText();
        numberHitsInThisShot = 0;
    }

    public void PlayHitMessage(int numHits)
    {
        foreach (HitTextInfo currentText in streakTexts)
        {
            if (currentText.numHits == numHits)
            {
                currentActiveText = currentText.textGO;
                currentActiveText.SetActive(true);
                PlayScaleUpAnimation(currentActiveText);
            }
            else
            {
                currentText.textGO.SetActive(false);
            }
        }
    }

    public void PlayScaleUpAnimation(GameObject textGO)
    {
        if (scalingCoroutine != null)
        {
            StopCoroutine(scalingCoroutine);
        }
        scalingCoroutine = StartCoroutine(ScaleUpAnimation(textGO));
    }

    IEnumerator ScaleUpAnimation(GameObject textGO)
    {
        float timePassed = 0;
        float animationTime = 0.25f;

        while (timePassed <= animationTime)
        {
            timePassed += Time.deltaTime;
            textGO.transform.localScale =
                Vector3.Lerp(Vector3.zero, Vector3.one, Easings.EaseInCubic(timePassed / animationTime));

            yield return null;
        }

        scalingCoroutine = null;
    }
    
    public void PlayScaleDownAnimation(GameObject textGO)
    {
        if (scalingCoroutine != null)
        {
            StopCoroutine(scalingCoroutine);
        }
        scalingCoroutine = StartCoroutine(ScaleDownAnimation(textGO));
    }

    IEnumerator ScaleDownAnimation(GameObject textGO)
    {
        float timePassed = 0;
        float animationTime = 0.25f;

        while (timePassed <= animationTime)
        {
            timePassed += Time.deltaTime;
            textGO.transform.localScale =
                Vector3.Lerp(Vector3.one, Vector3.zero, Easings.EaseInCubic(timePassed / animationTime));

            yield return null;
        }

        DisableAllTexts();
        scalingCoroutine = null;
    }

    public void PlayMissedMessage()
    {
    }
    
    public void DisableAllTexts()
    {
        foreach (HitTextInfo currentText in streakTexts)
        {
            currentText.textGO.SetActive(false);
        }
    }

    public void ShowMultiplierText(int value)
    {
        if (multiplierAnimationRoutine != null)
        {
            StopCoroutine(multiplierAnimationRoutine);
        }

        multiplierAnimationRoutine = StartCoroutine(AnimateMultiplierText(value));
    }
    
    IEnumerator AnimateMultiplierText(int value)
    {
        multiplierText.textGO.SetActive(true);
        multiplierText.textComponent.text = $"{value}x";
        
        float speed = 15.0f;
        float maxAngle = 1.0f;
        float scaleAmount = 1.01f;

        Vector3 originalScale = Vector3.one;
        GameObject textGO = multiplierText.textGO;
        Transform textComp = multiplierText.textComponent.transform;
        
        while (textGO.activeSelf)
        {
            float angle = Mathf.Sin(Time.time * speed) * maxAngle;
            float scale = 1.0f + Mathf.Sin(Time.time * speed) * 0.1f;

            textComp.rotation = Quaternion.Euler(0, 0, angle);
            textComp.localScale = originalScale * (scaleAmount * scale);

            yield return null;
        }

        textComp.rotation = Quaternion.identity;
        textComp.localScale = originalScale;
    }

    public void HideMultiplierText()
    {
        if (multiplierAnimationRoutine != null)
        {
            StopCoroutine(multiplierAnimationRoutine);
        }
        multiplierText.textGO.SetActive(false);
    }
}
