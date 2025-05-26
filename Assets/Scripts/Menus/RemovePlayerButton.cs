using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

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
    
    private ModeSelector modeSelector;
    
    [Inject]
    public void Construct(ModeSelector _modeSelector)
    {
        modeSelector = _modeSelector;
    }

    private void OnEnable()
    {
        Button.onClick.AddListener(() =>
        {
            modeSelector.RemovePlayer();
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
