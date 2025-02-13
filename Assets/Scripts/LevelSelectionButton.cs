using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionButton : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI levelText;
    public Image[] outlines;
    
    private GameModeType gameMode;
    private ButtonClickBehaviour clickBehaviour;
    private ButtonClickBehaviour ClickBehaviour
    {
        get
        {
            if (!clickBehaviour)
            {
                clickBehaviour = GetComponentInChildren<ButtonClickBehaviour>();
            }

            return clickBehaviour;
        }
    }
    private int levelNumber;
    public int LevelNumber => levelNumber;
    
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
    }

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
        ModeSelector.Instance.SetSelectedLevel(levelNumber);
    }

    public void SetBorderMaterial(int selectedLevel)
    {
        if (LevelNumber == selectedLevel)
        {
            SetBorderMaterial(GlobalAssets.Instance.GetSelectedMaterial(ButtonLocation.MainMenu));
        }
        else
        {
            SetBorderMaterial(ClickBehaviour.isEnabled
                ? GlobalAssets.Instance.GetDefaultMaterial(ButtonLocation.MainMenu)
                : GlobalAssets.Instance.GetLockedMaterial(ButtonLocation.MainMenu));
        }
    }
    
    public void SetBorderMaterial(Material _mat)
    {
        foreach (Image outline in outlines)
        {
            outline.material = _mat;
        }
    }

    public void SetUnlocked()
    {
        button.enabled = true;
        button.GetComponent<Image>().enabled = true;
        ClickBehaviour.isEnabled = true;
    }
    
    public void SetLocked()
    {
        button.enabled = false;
        ClickBehaviour.isEnabled = false;
        button.GetComponent<Image>().enabled = false;
    }
}
