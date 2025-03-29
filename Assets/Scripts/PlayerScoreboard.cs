using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerScoreboard : MonoBehaviour
{
    public GameObject currentShotTakerIndicator;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;

    [Header("Animation")]
    public TextMeshProUGUI animationTextPositive;
    public TextMeshProUGUI animationTextNegative;
    public RectTransform startPosition;
    public RectTransform targetPosition;

    private int playerIndex;
    private int trueScore;
    private int currentScoreOnDisplay;
    private Coroutine addScoreCoroutine;
    private TextMeshProUGUI animationText;

    public void SetPlayer(int index)
    {
        playerIndex = index;
        SetName("Player " + (index + 1));
    }

    public void SetName(string _name)
    {
        nameText.text = _name;
    }

    public void SetScore(int score)
    {
        currentScoreOnDisplay = score;
        scoreText.text = score.ToString();
    }

    public void TickToScore(int _trueScore, int pointsFromThisHit)
    {
        if (addScoreCoroutine != null)
        {
            StopCoroutine(addScoreCoroutine);
        }

        trueScore = _trueScore;
        addScoreCoroutine = StartCoroutine(AddScore(pointsFromThisHit));
    }

    public void SetCurrentShotTaker(bool value)
    {
        currentShotTakerIndicator.gameObject.SetActive(value);
    }

    IEnumerator AddScore(int pointsFromThisHit)
    {
        animationTextPositive.gameObject.SetActive(false);
        animationTextNegative.gameObject.SetActive(false);
        animationText = pointsFromThisHit > 0 ? animationTextPositive : animationTextNegative;
        string sign = pointsFromThisHit > 0 ? "+" : "";
        animationText.text = $"{sign}{pointsFromThisHit.ToString()}";
        animationText.gameObject.SetActive(true);
        animationText.transform.localScale = Vector3.one;
        animationText.rectTransform.anchoredPosition = startPosition.anchoredPosition;

        float growScale = 1.5f;
        float timePassed = 0;
        float timeToGrow = 0.25f;
        while (timePassed <= timeToGrow)
        {
            animationText.transform.localScale = Mathf.Lerp(1, growScale, timePassed / timeToGrow) * Vector3.one;
            
            timePassed += Time.deltaTime;
            yield return null;
        }
        
        timePassed = 0;
        float timeToTravel = 0.25f;
        while (timePassed <= timeToTravel)
        {
            float lerpVal = timePassed / timeToGrow;
            animationText.transform.localScale = Mathf.Lerp(growScale, 0, lerpVal) * Vector3.one;
            animationText.rectTransform.anchoredPosition = Vector2.Lerp(startPosition.anchoredPosition, targetPosition.anchoredPosition, lerpVal);
            
            timePassed += Time.deltaTime;
            yield return null;
        }
        
        animationText.gameObject.SetActive(false);
        
        int tick = 1;
        float totalTime = 1;
        int numTicks = 8;
        float tickDuration = totalTime / numTicks;
        while (tick <= numTicks)
        {
            int scoreRemaining = trueScore - currentScoreOnDisplay;
            int scoreToAddThisTick = scoreRemaining / (numTicks + 1 - tick);
            SetScore(currentScoreOnDisplay + scoreToAddThisTick);
        
            tick++;
            yield return new WaitForSeconds(tickDuration);
        }

        SetScore(trueScore);
        addScoreCoroutine = null;
    }
}
