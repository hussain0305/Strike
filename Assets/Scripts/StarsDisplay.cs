using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarsDisplay : MonoBehaviour
{
    public TextMeshProUGUI starCount;
    public TextMeshProUGUI starsAddedText;
    public TextMeshProUGUI starsSpentText;
    public Image starCountBG;
    
    private TextMeshProUGUI currentChangeText;
    private Coroutine starsChangedAnimation;

    private void OnEnable()
    {
        EventBus.Subscribe<StarsEarnedEvent>(StarsEarned);
        EventBus.Subscribe<StarsSpentEvent>(StarsSpent);
        EventBus.Subscribe<SaveLoadedEvent>(SaveLoaded);
        SetStarCount();
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<StarsEarnedEvent>(StarsEarned);
        EventBus.Unsubscribe<StarsSpentEvent>(StarsSpent);
        EventBus.Unsubscribe<SaveLoadedEvent>(SaveLoaded);

        starsAddedText.gameObject.SetActive(false);
        starsSpentText.gameObject.SetActive(false);
        starsChangedAnimation = null;
    }

    public void StarsEarned(StarsEarnedEvent e)
    {
        if (starsChangedAnimation != null)
        {
            StopCoroutine(starsChangedAnimation);
        }

        starsChangedAnimation = StartCoroutine(StarsChangedAnimation(e.NumStarsEarned));
    }

    public void StarsSpent(StarsSpentEvent e)
    {
        if (starsChangedAnimation != null)
        {
            StopCoroutine(starsChangedAnimation);
        }

        starsChangedAnimation = StartCoroutine(StarsChangedAnimation(-e.NumStarsSpent));
    }

    private IEnumerator StarsChangedAnimation(int change)
    {
        bool starsAdded = change > 0;
        starsAddedText.gameObject.SetActive(starsAdded);
        starsSpentText.gameObject.SetActive(!starsAdded);

        currentChangeText = starsAdded ? starsAddedText : starsSpentText;
        
        int startCount = SaveManager.GetStars() - change;
        int endCount = SaveManager.GetStars();
        
        starCount.text = startCount.ToString();
        string sign = starsAdded ? "+" : "-";
        currentChangeText.text = $"{sign}{Math.Abs(change)}";
        
        float timePassed = 0f;
        float animationDuration = 1f;
        
        while (timePassed < animationDuration)
        {
            timePassed += Time.deltaTime;
            float t = timePassed / animationDuration;
            int currentStarCount = Mathf.RoundToInt(Mathf.Lerp(startCount, endCount, t));
            int currentChangeValue = Mathf.RoundToInt(Mathf.Lerp(change, 0, t));
            
            starCount.text = currentStarCount.ToString();
            currentChangeText.text = $"{sign}{Math.Abs(currentChangeValue)}";
            starCountBG.fillAmount = GetFillAmount(Math.Abs(currentStarCount));

            yield return null;
        }
        
        starCount.text = endCount.ToString();
        currentChangeText.text = "";
        
        starsAddedText.gameObject.SetActive(false);
        starsSpentText.gameObject.SetActive(false);
        starsChangedAnimation = null;
    }

    public void SetStarCount()
    {
        if (!SaveManager.IsSaveLoaded)
        {
            return;
        }

        int numStars = SaveManager.GetStars();
        starCount.text = numStars.ToString();
        starCountBG.fillAmount = GetFillAmount(numStars);
    }
    
    private float GetFillAmount(int stars)
    {
        int digitCount = Mathf.Clamp(stars.ToString().Length, 2, 6);
        return Mathf.Lerp(0.35f, 1f, (digitCount - 2) / 4f);
    }

    public void SaveLoaded(SaveLoadedEvent e)
    {
        SetStarCount();
    }
}
