using System;
using System.Collections.Generic;
using UnityEngine;

public class ButtonStateManager : MonoBehaviour
{
    private static ButtonStateManager _instance;
    public static ButtonStateManager Instance
    {
        get 
        { 
            if (_instance == null)
            {
                GameObject obj = new GameObject("ButtonStateManager");
                _instance = obj.AddComponent<ButtonStateManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    private Dictionary<ButtonClickBehaviour.ButtonGroup, ButtonClickBehaviour> selectedButtonsByGroup = new Dictionary<ButtonClickBehaviour.ButtonGroup, ButtonClickBehaviour>();

    private void OnEnable()
    {
        EventBus.Subscribe<GameModeChangedEvent>(GameModeChanged);
        EventBus.Subscribe<GameEndedEvent>(RefreshDictionary);
        EventBus.Subscribe<GameExitedEvent>(RefreshDictionary);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameModeChangedEvent>(GameModeChanged);
        EventBus.Unsubscribe<GameEndedEvent>(RefreshDictionary);
        EventBus.Subscribe<GameExitedEvent>(RefreshDictionary);
    }

    public void RegisterButton(ButtonClickBehaviour button)
    {
        if (!selectedButtonsByGroup.ContainsKey(button.groupId))
            selectedButtonsByGroup[button.groupId] = null;
    }

    public void SelectButton(ButtonClickBehaviour button)
    {
        if (selectedButtonsByGroup.TryGetValue(button.groupId, out var selectedButton) && selectedButton != button)
        {
            selectedButton?.SetToDefault();
        }

        selectedButtonsByGroup[button.groupId] = button;
        button.SetToHighlighted();
    }

    public bool IsButtonSelected(ButtonClickBehaviour button)
    {
        return selectedButtonsByGroup.TryGetValue(button.groupId, out var selectedButton) && selectedButton == button;
    }

    public void GameModeChanged(GameModeChangedEvent e)
    {
        ResetLevelSelectionButtons();
    }

    public void RefreshDictionary()
    {
        selectedButtonsByGroup?.Clear();
    }

    public void RefreshDictionary(GameEndedEvent e)
    {
        RefreshDictionary();
    }

    public void RefreshDictionary(GameExitedEvent e)
    {
        RefreshDictionary();
    }

    public void ResetLevelSelectionButtons()
    {
        selectedButtonsByGroup[ButtonClickBehaviour.ButtonGroup.LevelSelection] = null;
    }
}