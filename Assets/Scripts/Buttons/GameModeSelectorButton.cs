using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameModeSelectorButton : MonoBehaviour
{
    public Button selectorButton;
    public GameModeType type;
    public int sceneToLoad;

    private void Awake()
    {
        selectorButton.onClick.AddListener(Selected);
    }

    public void Selected()
    {
        // ModeSelector.Instance.GameModeSelected(this);
    }
}
