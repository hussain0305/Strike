using System;
using System.Collections;
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

    public HitTextInfo[] allTexts;
    
    private string[] streakTexts =
    {
        "Missed",       //No hits
        "Hit",          //Single Hit
        "Double Hit",   //2x
        "Triple Hit",   //3x
        "Quad Hit",     //4x
        "Penta Hit",    //5x
        "Hexa Hit",     //6x
        "Hepta Hit",    //7x
        "Octa Hit",     //8x
        "Nona Hit",     //9x
        "Deca Hit"      //10x
    };
    
    private int numberHitsInThisShot = 0;
    private GameObject currentActiveText;
    private Coroutine scalingCoroutine;

    private void OnEnable()
    {
        Collectible.OnCollectibleHit += CollectibleHit;
        GameManager.OnNextShotCued += NextShotCued;
    }

    private void OnDisable()
    {
        Collectible.OnCollectibleHit -= CollectibleHit;
        GameManager.OnNextShotCued -= NextShotCued;
    }

    private void OnDestroy()
    {
        Collectible.OnCollectibleHit -= CollectibleHit;
        GameManager.OnNextShotCued -= NextShotCued;
    }

    public void CollectibleHit(CollectibleType type, int value)
    {
        numberHitsInThisShot++;
        PlayHitMessage(numberHitsInThisShot);
    }

    public void NextShotCued()
    {
        if (numberHitsInThisShot == 0)
        {
            DisableAllTexts();
        }
        else
        {
            PlayScaleDownAnimation(currentActiveText);
        }
        numberHitsInThisShot = 0;
    }

    public void PlayHitMessage(int numHits)
    {
        foreach (HitTextInfo currentText in allTexts)
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
        foreach (HitTextInfo currentText in allTexts)
        {
            currentText.textGO.SetActive(false);
        }
    }
}
