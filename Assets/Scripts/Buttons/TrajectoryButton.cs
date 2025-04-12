using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrajectoryButton : MonoBehaviour
{
    public Image trajectoryIcon;
    public GameObject remainingInfoBubble;
    public TextMeshProUGUI remainingText;
    public TextMeshProUGUI countdownText;
    public Button button;
    
    private void OnEnable()
    {
        button.onClick.AddListener(ButtonPressed);
    }

    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }

    public void ButtonPressed()
    {
        button.enabled = false;
        remainingInfoBubble.SetActive(false);
        countdownText.gameObject.SetActive(true);
        trajectoryIcon.gameObject.SetActive(false);
        EventBus.Publish(new TrajectoryEnabledEvent());
    }

    public void SetCountdownText(int secondsRemaining)
    {
        countdownText.text = secondsRemaining.ToString();
    }

    public void ShowButton(int triesRemaining)
    {
        button.enabled = true;
        remainingInfoBubble.SetActive(true);
        remainingText.text = triesRemaining.ToString();
        countdownText.gameObject.SetActive(false);
        trajectoryIcon.gameObject.SetActive(true);
    }
}
