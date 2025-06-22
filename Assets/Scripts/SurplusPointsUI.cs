using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SurplusPointsEarnedEvent
{
    public int SurplusPointsEarned;
    public SurplusPointsEarnedEvent(int surplusPointsEarned)
    {
        SurplusPointsEarned = surplusPointsEarned;
    }
}

public class SurplusPointsSpentEvent
{
    public int SurplusPointsSpent;
    public SurplusPointsSpentEvent(int surplusPointsSpent)
    {
        SurplusPointsSpent = surplusPointsSpent;
    }
}

public class SurplusPointsUI : MonoBehaviour
{
    public TextMeshProUGUI spCount;
    public TextMeshProUGUI spAddedText;
    public TextMeshProUGUI spSpentText;
    public Image spCountBG;

    private TextMeshProUGUI currentChangeText;
    private Coroutine spChangedAnimation;

    private float xCurrentScale = 1;
    
    private void OnEnable()
    {
        EventBus.Subscribe<SurplusPointsEarnedEvent>(SurplusPointsEarned);
        EventBus.Subscribe<SurplusPointsSpentEvent>(SurplusPointsSpent);
        EventBus.Subscribe<SaveLoadedEvent>(SaveLoaded);
        SetSurplusPointsCount();
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<SurplusPointsEarnedEvent>(SurplusPointsEarned);
        EventBus.Unsubscribe<SurplusPointsSpentEvent>(SurplusPointsSpent);
        EventBus.Unsubscribe<SaveLoadedEvent>(SaveLoaded);

        spAddedText.gameObject.SetActive(false);
        spSpentText.gameObject.SetActive(false);
        spChangedAnimation = null;
    }

    public void SurplusPointsEarned(SurplusPointsEarnedEvent e)
    {
        if (spChangedAnimation != null)
        {
            StopCoroutine(spChangedAnimation);
        }

        spChangedAnimation = StartCoroutine(SurplusPointsChangedAnimation(e.SurplusPointsEarned));
    }

    public void SurplusPointsSpent(SurplusPointsSpentEvent e)
    {
        if (spChangedAnimation != null)
        {
            StopCoroutine(spChangedAnimation);
        }

        spChangedAnimation = StartCoroutine(SurplusPointsChangedAnimation(-e.SurplusPointsSpent));
    }

    private IEnumerator SurplusPointsChangedAnimation(int change)
    {
        if (change == 0)
        {
            spAddedText.gameObject.SetActive(false);
            spSpentText.gameObject.SetActive(false);
            yield break;
        }
        
        bool surplusPointsAdded = change > 0;
        spAddedText.gameObject.SetActive(surplusPointsAdded);
        spSpentText.gameObject.SetActive(!surplusPointsAdded);

        currentChangeText = surplusPointsAdded ? spAddedText : spSpentText;
        
        int startCount = SaveManager.GetSurplusPoints() - change;
        int endCount = SaveManager.GetSurplusPoints();
        
        spCount.text = startCount.ToString();
        string sign = surplusPointsAdded ? "+" : "-";
        currentChangeText.text = $"{sign}{Math.Abs(change)}";
        
        float timePassed = 0f;
        float animationDuration = 1f;
        
        while (timePassed < animationDuration)
        {
            timePassed += Time.deltaTime;
            float t = timePassed / animationDuration;
            int currentSurplusPointsCount = Mathf.RoundToInt(Mathf.Lerp(startCount, endCount, t));
            int currentChangeValue = Mathf.RoundToInt(Mathf.Lerp(change, 0, t));
            
            spCount.text = currentSurplusPointsCount.ToString();
            currentChangeText.text = $"{sign}{Math.Abs(currentChangeValue)}";
            Vector3 scale = spCountBG.transform.localScale;
            xCurrentScale = GetFillAmount(Math.Abs(currentSurplusPointsCount));
            spCountBG.transform.localScale = new Vector3(xCurrentScale, scale.y, scale.z) ;
            Vector3 changeTextScale = currentChangeText.transform.localScale;
            currentChangeText.transform.localScale = new Vector3(1f / xCurrentScale, changeTextScale.y, changeTextScale.z);

            yield return null;
        }
        
        spCount.text = endCount.ToString();
        currentChangeText.text = "";
        
        spAddedText.gameObject.SetActive(false);
        spSpentText.gameObject.SetActive(false);
        spChangedAnimation = null;
    }

    public void SetSurplusPointsCount()
    {
        if (!SaveManager.IsSaveLoaded)
        {
            return;
        }

        int surplusPoints = SaveManager.GetSurplusPoints();
        spCount.text = surplusPoints.ToString();
        Vector3 scale = spCountBG.transform.localScale;
        xCurrentScale = GetFillAmount(surplusPoints);
        spCountBG.transform.localScale = new Vector3(xCurrentScale, scale.y, scale.z) ;

    }
    
    private float GetFillAmount(int surplusPoints)
    {
        int digitCount = Mathf.Clamp(surplusPoints.ToString().Length, 2, 6);
        return Mathf.Lerp(0.35f, 1f, (digitCount - 2) / 4f);
    }

    public void SaveLoaded(SaveLoadedEvent e)
    {
        SetSurplusPointsCount();
    }
}
