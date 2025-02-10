using UnityEngine;

public class GameStateToggleListener : MonoBehaviour
{
    public GameStateManager.GameState[] statesToBeActiveIn;

    private void OnEnable()
    {
        GameStateManager.Instance.RegisterStateListener(this);
    }

    private void OnDisable()
    {
        GameStateManager.Instance.UnregisterStateListener(this);
    }

    public void CheckState(GameStateManager.GameState currentState)
    {
        bool shouldBeActive = System.Array.Exists(statesToBeActiveIn, state => state == currentState);
        gameObject.SetActive(shouldBeActive);
        Debug.Log($"|| {gameObject.name} set to {shouldBeActive}");
    }
}
