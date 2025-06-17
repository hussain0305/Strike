using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameModeInfoController : MonoBehaviour
{
    [Header("Info Section")]
    public Image progressBar;
    
    [Header("Progress Section")]
    public GameObject progressSection;
    public GameObject soloModeSection;
    public GameObject passplaySection;
    
    [Header("Game Mode Rules")]
    public GameObject gameModeRulesSection;
    public TextMeshProUGUI gameModeRules;

    private GameObject firstCarouselElement;
    private GameObject secondCarouselElement;
    private GameModeInfo currentGameModeInfo;

    private Coroutine infoCarouselCoroutine;
    
    private void OnEnable()
    {
        EventBus.Subscribe<GameModeChangedEvent>(GameModeChanged);
        EventBus.Subscribe<NumPlayersChangedEvent>(NumPlayersChanged);
        
        firstCarouselElement = progressSection;
        secondCarouselElement = gameModeRulesSection;
        
        RestartInfoCarousel();
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameModeChangedEvent>(GameModeChanged);
        EventBus.Unsubscribe<NumPlayersChangedEvent>(NumPlayersChanged);
        
        StopAllCoroutines();
        infoCarouselCoroutine = null;
    }
    
    public void NumPlayersChanged(NumPlayersChangedEvent e)
    {
        soloModeSection.SetActive(e.numPlayers == 1);
        passplaySection.SetActive(e.numPlayers != 1);
        
        firstCarouselElement = progressSection;
        secondCarouselElement = gameModeRulesSection;

        RestartInfoCarousel();
    }

    public void GameModeChanged(GameModeChangedEvent e)
    {
        currentGameModeInfo = e.GameModeInfo;
        
        string[] rulesArray = new string[e.GameModeInfo.rules.Length];
        for (int i = 0; i < rulesArray.Length; i++)
        {
            rulesArray[i] = e.GameModeInfo.rules[i] + "  [ \u2022 ]";
        }
        string rulesConsolidated = string.Join("\n", rulesArray);
        
        gameModeRules.text = rulesConsolidated;
        firstCarouselElement = gameModeRulesSection;
        secondCarouselElement = progressSection;

        RestartInfoCarousel();
    }

    public void RestartInfoCarousel()
    {
        if (infoCarouselCoroutine != null)
        {
            StopCoroutine(infoCarouselCoroutine);
        }

        if (!gameObject.activeInHierarchy)
            return;
        
        infoCarouselCoroutine = StartCoroutine(InfoCarousel());
    }
    
    private IEnumerator InfoCarousel()
    {
        while (true)
        {
            firstCarouselElement.SetActive(true);
            secondCarouselElement.SetActive(false);
            
            float timePassed = 0;
            float infoTime = 5;
            
            while (timePassed <= infoTime)
            {
                progressBar.fillAmount = timePassed / infoTime;
                
                timePassed += Time.deltaTime;
                yield return null;
            }
            
            firstCarouselElement.SetActive(false);
            secondCarouselElement.SetActive(true);

            timePassed = 0;
            while (timePassed <= infoTime)
            {
                progressBar.fillAmount = timePassed / infoTime;
                
                timePassed += Time.deltaTime;
                yield return null;
            }
        }
    }
}
