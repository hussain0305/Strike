using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class AdditionalVolleyOffer : MonoBehaviour
{
    public GameObject banner;
    public TextMeshProUGUI message;
    public TextMeshProUGUI timer;
    public Button yesButton;
    public Button noButton;

    private Action<bool> onComplete;
    [HideInInspector]
    public bool offerTaken;
    [HideInInspector]
    public bool offerActive;
    
    [Inject]
    private MenuManager menuManager;

    public void InitMenu()
    {
        offerActive = true;
        offerTaken = false;
        StopAllCoroutines();
        yesButton.onClick.AddListener(YesButtonPressed);
        noButton.onClick.AddListener(NoButtonPressed);
    }

    private void ExitMenu()
    {
        offerActive = false;
        onComplete = null;
        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();
        menuManager.CloseCurrentMenu();
    }

    public void Show(string messageText, Action<bool> _onComplete)
    {
        InitMenu();
        message.text = messageText;
        onComplete = _onComplete;

        menuManager.OpenMenu(MenuBase.MenuType.AdditionalVolleyOfferPopup);

        StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        float timeLeft = 10;
        while (timeLeft > 0)
        {
            while (AdManager.IsPlayingRewardedAd)
                yield return null;

            if (!banner.activeInHierarchy)
                break;
            
            timer.text = ((int)timeLeft).ToString();
            timeLeft -= Time.deltaTime;
            yield return null;
        }
        onComplete?.Invoke(false);
        ExitMenu();
    }
    
    public void YesButtonPressed()
    {
        AdManager.ShowRewardedAd((success) =>
        {
            offerTaken = true;
            onComplete?.Invoke(success);
            ExitMenu();
        });
    }
    
    public void NoButtonPressed()
    {
        onComplete?.Invoke(false);
        ExitMenu();
    }
}
