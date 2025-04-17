using System;
using TMPro;
using UnityEngine;

public class EliminationFeedbackText : MonoBehaviour
{
    public GameObject elimination;
    public TextMeshProUGUI eliminationText;
    
    private void OnEnable()
    {
        EventBus.Subscribe<GameEndedEvent>(DisableText);
        EventBus.Subscribe<GameExitedEvent>(DisableText);
        EventBus.Subscribe<NextShotCuedEvent>(DisableText);
        EventBus.Subscribe<PlayerEliminatedEvent>(ShowEliminationText);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameEndedEvent>(DisableText);
        EventBus.Unsubscribe<GameExitedEvent>(DisableText);
        EventBus.Unsubscribe<NextShotCuedEvent>(DisableText);
        EventBus.Unsubscribe<PlayerEliminatedEvent>(ShowEliminationText);
    }

    public void ShowEliminationText(PlayerEliminatedEvent e)
    {
        elimination.SetActive(true);
        if(GameManager.Instance.NumPlayersInGame == 1)
            eliminationText.text = $"!! ELIMINATED !!";
        else
            eliminationText.text = $"PLAYER {e.PlayerIndex + 1} ELIMINATED!";
    }

    public void DisableText<T>(T e)
    {
        elimination.SetActive(false);
    }
}
