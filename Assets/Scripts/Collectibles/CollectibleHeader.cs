using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CollectibleHeader : MonoBehaviour
{
    public Image background;
    public TextMeshProUGUI text;
    
    private Coroutine animationCoroutine;

    [HideInInspector]
    public bool locationSet = false;

    private Vector3 lowerPosition;
    private Vector3 upperPosition;
    
    private void OnEnable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        if (locationSet)
        {
            animationCoroutine = StartCoroutine(HoverAnimation());
        }
    }

    private void Start()
    {
        QOLFeaturesManager.Instance.RegisterCollectibleHeader(this);
    }

    public void StartAnimation()
    {
        lowerPosition = transform.position - new Vector3(0, -0.5f, 0);
        upperPosition = transform.position - new Vector3(0, 0.5f, 0);

        locationSet = true;
        animationCoroutine = StartCoroutine(HoverAnimation());
    }

    IEnumerator HoverAnimation()
    {
        float duration = 2f;
        float timePassed = 0;

        Vector3 positionA = lowerPosition;
        Vector3 positionB = upperPosition;
        while (true)
        {
            while (timePassed < duration)
            {
                timePassed += Time.deltaTime;
                float t = timePassed / duration;
                float easedT = Easings.EaseInOutSine(t);

                transform.position = Vector3.Lerp(positionA, positionB, easedT);

                yield return null;
            }

            (positionA, positionB) = (positionB, positionA);

            timePassed = 0;
            yield return null;
        }
    }

    public void SetText(string _text)
    {
        text.text = _text;
    }

    public void SetText(int points)
    {
        text.text = points.ToString();
    }

    public void SetBackground()
    {
        
    }
}
