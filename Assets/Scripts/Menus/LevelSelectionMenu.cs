using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionMenu : MonoBehaviour
{
    [Header("Select A Level Prompt")]
    public GameObject PromptGameObject;
    public Image[] promptImages;
    public TextMeshProUGUI promptMessage;
    private Color promptLineColor;
    private Color promptTextColor;
    private Color transparentColor;

    [Header("Info Section")]
    public GameObject soloModeSection;
    public GameObject passplaySection;
    
    private static LevelSelectionMenu instance;
    public static LevelSelectionMenu Instance => instance;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance.gameObject);
        }
        instance = this;
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
        ModeSelector.Instance.ResetSelectedLevel();
        EventBus.Subscribe<NumPlayersChangedEvent>(NumPlayersChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<NumPlayersChangedEvent>(NumPlayersChanged);
    }

    private Coroutine promptRoutine;
    public void PromptToSelectLevel()
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
        soloModeSection.SetActive(e.numPlayers == 1);
        passplaySection.SetActive(e.numPlayers != 1);
    }
}
