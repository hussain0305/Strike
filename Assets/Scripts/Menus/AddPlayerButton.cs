using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class AddPlayerButton : MonoBehaviour
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

    private int maxPlayers = -1;
    private int MaxPlayers
    {
        get
        {
            if (maxPlayers == -1)
                maxPlayers = modeSelector.MaxPlayers;

            return maxPlayers;
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
            modeSelector.AddPlayer();
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
        Button.interactable = e.numPlayers < MaxPlayers;
        Button.enabled = e.numPlayers < MaxPlayers;
    }
}
