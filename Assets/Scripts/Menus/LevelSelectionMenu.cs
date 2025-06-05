using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PromptToSelectLevelEvent { }

public class LevelSelectionMenu : MonoBehaviour
{
    [Header("Select A Level Prompt")]
    public GameObject PromptGameObject;
    public Image[] promptImages;
    public TextMeshProUGUI promptMessage;
    public TextMeshProUGUI numPlayersText;
    private Color promptLineColor;
    private Color promptTextColor;
    private Color transparentColor;
    
    private ModeSelector modeSelector;
    
    [Inject]
    public void Construct(ModeSelector _modeSelector)
    {
        modeSelector = _modeSelector;
    }
    
    private void Start()
    {
        promptLineColor = promptImages[0].color;
        promptTextColor = promptMessage.color;
        transparentColor = new Color(1, 0, 0, 0);
    }

    private void OnEnable()
    {
        PromptGameObject.SetActive(false);
        modeSelector.LevelSelectionMenuOpened();
        numPlayersText.text = modeSelector.GetNumPlayers().ToString();
        EventBus.Subscribe<PromptToSelectLevelEvent>(PromptToSelectLevel);
        EventBus.Subscribe<NumPlayersChangedEvent>(NumPlayersChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PromptToSelectLevelEvent>(PromptToSelectLevel);
        EventBus.Unsubscribe<NumPlayersChangedEvent>(NumPlayersChanged);
    }

    private Coroutine promptRoutine;
    public void PromptToSelectLevel(PromptToSelectLevelEvent e)
    {
        IEnumerator FadePrompt()
        {
            float timePassed = 0;
            float timeToFadeIn = 0.25f;
            while (timePassed <= timeToFadeIn)
            {
                float lerpVal = timePassed / timeToFadeIn;
                foreach (Image img in promptImages)
                {
                    img.color = Color.Lerp(transparentColor, promptLineColor, lerpVal);
                }
                promptMessage.color = Color.Lerp(transparentColor, promptTextColor, lerpVal);
                timePassed += Time.deltaTime;
                yield return null;
            }
            
            timePassed = 0;
            float timeToFadeOut = 1;
            while (timePassed <= timeToFadeOut)
            {
                float lerpVal = timePassed / timeToFadeOut;
                foreach (Image img in promptImages)
                {
                    img.color = Color.Lerp(promptLineColor, transparentColor, lerpVal);
                }
                promptMessage.color = Color.Lerp(promptTextColor, transparentColor, lerpVal);
                timePassed += Time.deltaTime;
                yield return null;
            }
            PromptGameObject.SetActive(false);
            promptRoutine = null;
        }

        PromptGameObject.SetActive(true);
        if (promptRoutine != null)
        {
            StopCoroutine(promptRoutine);
        }
        promptRoutine = StartCoroutine(FadePrompt());
    }

    public void NumPlayersChanged(NumPlayersChangedEvent e)
    {
        numPlayersText.text = e.numPlayers.ToString();
    }
}
