using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionButton : MonoBehaviour
{
    [System.Serializable]
    public struct StarUIRepresentation
    {
        public Image collected;
        public Image uncollected;
    }
    
    public Button button;
    public TextMeshProUGUI levelText;
    public StarUIRepresentation[] stars;
    
    private GameModeType gameMode;
    private ButtonFeedback buttonBehaviour;
    private ButtonFeedback ButtonBehaviour
    {
        get
        {
            if (!buttonBehaviour)
            {
                buttonBehaviour = GetComponentInChildren<ButtonFeedback>();
            }

            return buttonBehaviour;
        }
    }
    private int levelNumber;
    public int LevelNumber => levelNumber;
    
    private void OnEnable()
    {
        EventBus.Subscribe<ButtonClickedEvent>(ButtonPressedEvent);
        button.onClick.AddListener(ButtonPressed);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ButtonClickedEvent>(ButtonPressedEvent);
        button.onClick.RemoveAllListeners();
    }

    public void SetText(string _text)
    {
        levelText.text = _text;
    }
    
    public void SetText(int _levelNumber)
    {
        levelText.text = $"LEVEL {_levelNumber}";
    }

    public void SetMappedLevel(GameModeType _gameMode, int _level)
    {
        gameMode = _gameMode;
        levelNumber = _level;
        SetText(_level);
        SetStars();
    }

    public void SetStars()
    {
        int numStarsCollected = SaveManager.GetCollectedStarsCount((int)gameMode, levelNumber);

        int i = 0;
        for (; i < numStarsCollected; i++)
        {
            stars[i].collected.gameObject.SetActive(true);
            stars[i].uncollected.gameObject.SetActive(false);
        }

        for (; i < stars.Length; i++)
        {
            stars[i].collected.gameObject.SetActive(false);
            stars[i].uncollected.gameObject.SetActive(true);
        }
    }
    
    public void ButtonPressed()
    {
        EventBus.Publish(new ButtonClickedEvent(LevelNumber, ButtonGroup.LevelSelection));
    }

    public void ButtonPressedEvent(ButtonClickedEvent e)
    {
        if (e.ButtonGroup != ButtonGroup.LevelSelection)
        {
            return;
        }
        UpdateAppearance(e.Index);
    }
    
    public void UpdateAppearance(int selectedLevel)
    {
        if (LevelNumber == selectedLevel)
        {
            ButtonBehaviour.SetSelected();
        }
        else
        {
            ButtonBehaviour.SetUnselected();
        }
    }
    
    public void SetUnlocked()
    {
        button.enabled = true;
        ButtonBehaviour.isEnabled = true;
        ButtonBehaviour.SetToDefault();
    }
    
    public void SetLocked()
    {
        button.enabled = false;
        ButtonBehaviour.isEnabled = false;
        ButtonBehaviour.SetToDefault();
    }
}
