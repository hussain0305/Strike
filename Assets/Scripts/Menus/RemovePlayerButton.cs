using System;
using UnityEngine;
using UnityEngine.UI;

public class RemovePlayerButton : MonoBehaviour
{
    private Button button;
    private Button Button
    {
        get
        {
            if (button == null)
                button = GetComponentInParent<Button>();
            
            return button;
        }
    }
    
    private void OnEnable()
    {
        Button.onClick.AddListener(() =>
        {
            ModeSelector.Instance.RemovePlayer();
        });
    }

    private void OnDisable()
    {
        Button.onClick.RemoveAllListeners();
    }
}
