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
        EventBus.Subscribe<NumPlayersChangedEvent>(NumPlayersChanged);
    }

    private void OnDisable()
    {
        Button.onClick.RemoveAllListeners();
        EventBus.Unsubscribe<NumPlayersChangedEvent>(NumPlayersChanged);
    }
    
    public void NumPlayersChanged(NumPlayersChangedEvent e)
    {
        Button.interactable = e.numPlayers > 1;
        Button.enabled = e.numPlayers > 1;
    }
}
