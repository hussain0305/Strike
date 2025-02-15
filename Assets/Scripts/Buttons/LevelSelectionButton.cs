using System;
using System.Collections;
using System.Collections.Generic;
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
    private ButtonClickBehaviour buttonBehaviour;
    private ButtonClickBehaviour ButtonBehaviour
    {
        get
        {
            if (!buttonBehaviour)
            {
                buttonBehaviour = GetComponentInChildren<ButtonClickBehaviour>();
            }

            return buttonBehaviour;
        }
    }
    private int levelNumber;
    public int LevelNumber => levelNumber;
    
    private void OnEnable()
    {
        button.onClick.AddListener(ButtonPressed);
    }

    private void OnDisable()
    {
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
        SaveManager.GetStarsCollectedStatus((int)gameMode, levelNumber, out bool[] starsStatus);
        int i = 0;
        foreach (StarUIRepresentation star in stars)
        {
            star.collected.gameObject.SetActive(starsStatus[i]);
            star.uncollected.gameObject.SetActive(!starsStatus[i]);
            i++;
        }
    }
    
    public void ButtonPressed()
    {
        ModeSelector.Instance.SetSelectedLevel(levelNumber);
    }

    public void SetBorderMaterial(int selectedLevel)
    {
        if (LevelNumber == selectedLevel)
        {
            ButtonBehaviour.SetSelected();
        }
    }
    
    public void SetUnlocked()
    {
        button.enabled = true;
        button.GetComponent<Image>().enabled = true;
        ButtonBehaviour.isEnabled = true;
    }
    
    public void SetLocked()
    {
        button.enabled = false;
        ButtonBehaviour.isEnabled = false;
        button.GetComponent<Image>().enabled = false;
    }
}
