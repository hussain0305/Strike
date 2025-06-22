using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EarnStarsPage : MonoBehaviour
{
    public GameObject exchangeAvailableSection;
    public GameObject exchangeUnavailableSection;

    public TextMeshProUGUI exchangePointsText;
    public TextMeshProUGUI exchangeStarsText;

    public TextMeshProUGUI exchangeRateMessage;

    public Button exchangeButton;
    
    private const int EXCHANGE_RATE = 50;
    
    private void OnEnable()
    {
        EventBus.Subscribe<SurplusPointsSpentEvent>(PointsSpent);
        SetupExchangeSection();
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<SurplusPointsSpentEvent>(PointsSpent);
        exchangeButton.onClick.RemoveAllListeners();
    }

    public void PointsSpent(SurplusPointsSpentEvent e)
    {
        SetupExchangeSection();
    }

    public void SetupExchangeSection()
    {
        if (!SaveManager.IsSaveLoaded)
            return;
        
        int currentSurplusPoints = SaveManager.GetSurplusPoints();
        bool exchangeAvailable = currentSurplusPoints > EXCHANGE_RATE;
        exchangeAvailableSection.SetActive(exchangeAvailable);
        exchangeUnavailableSection.SetActive(!exchangeAvailable);

        if (!exchangeAvailable)
        {
            exchangeRateMessage.text = $"Collect {EXCHANGE_RATE} points to exchange for a Star";
            return;
        }

        int pointsToExchange = currentSurplusPoints - (currentSurplusPoints % EXCHANGE_RATE);
        int starsToExchange = pointsToExchange / EXCHANGE_RATE;

        exchangePointsText.text = pointsToExchange.ToString();
        exchangeStarsText.text = starsToExchange.ToString();
        
        exchangeButton.onClick.RemoveAllListeners();
        exchangeButton.onClick.AddListener(Exchange);
    }

    public void Exchange()
    {
        int currentSurplusPoints = SaveManager.GetSurplusPoints();
        bool exchangeAvailable = currentSurplusPoints > EXCHANGE_RATE;

        if (!exchangeAvailable)
        {
            return;
        }

        int pointsToExchange = currentSurplusPoints - (currentSurplusPoints % EXCHANGE_RATE);
        int starsToExchange = pointsToExchange / EXCHANGE_RATE;

        if (SaveManager.SpendSurplusPoints(pointsToExchange, false))
        {
            SaveManager.AddStars(starsToExchange);
        }
    }
}
