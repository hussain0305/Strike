using UnityEngine;
using Zenject;

public class GameStateToggleListener : MonoBehaviour
{
    public GameState[] statesToBeActiveIn;

    private GameStateManager gameStateManager;
    
    [Inject]
    public void Construct(GameStateManager _gameStateManager)
    {
        gameStateManager = _gameStateManager;
    }

    private void OnEnable()
    {
        gameStateManager?.RegisterStateListener(this);
    }

    private void OnDisable()
    {
        gameStateManager?.UnregisterStateListener(this);
    }

    public void CheckState(GameState currentState)
    {
        bool shouldBeActive = System.Array.Exists(statesToBeActiveIn, state => state == currentState);
        gameObject.SetActive(shouldBeActive);
    }
}
