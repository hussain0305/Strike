using UnityEngine;
using UnityEngine.UI;

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
                maxPlayers = ModeSelector.Instance.MaxPlayers;

            return maxPlayers;
        }    
    }
    
    private void OnEnable()
    {
        Button.onClick.AddListener(() =>
        {
            ModeSelector.Instance.AddPlayer();
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
