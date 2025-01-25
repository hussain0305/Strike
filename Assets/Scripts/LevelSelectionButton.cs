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

    public void SetBorderMaterial(Material _mat)
    {
        foreach (Image outline in outlines)
        {
            outline.material = _mat;
        }
    }
}
