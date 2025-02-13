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

    private Dictionary<string, ButtonClickBehaviour> selectedButtonsByGroup = new Dictionary<string, ButtonClickBehaviour>();

    public void RegisterButton(ButtonClickBehaviour button)
    {
        if (string.IsNullOrEmpty(button.groupId))
            button.groupId = "Global";

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
}