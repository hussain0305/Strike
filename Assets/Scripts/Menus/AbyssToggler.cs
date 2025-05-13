using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AbyssToggler : MonoBehaviour
{
    private AbyssTogglerVisuals visuals;
    private AbyssTogglerVisuals Visuals
    {
        get
        {
            if (visuals == null)
            {
                visuals = GetComponent<AbyssTogglerVisuals>();
            }
            return visuals;
        }
    }
    
    private Button button;
    private Button Button
    {
        get
        {
            if (button == null)
                button = GetComponent<Button>();
            
            return button;
        }    
    }

    private void OnEnable()
    {
        Button.onClick.AddListener(Visuals.Toggle);
    }

    private void OnDisable()
    {
        Button.onClick.RemoveAllListeners();
    }
}
