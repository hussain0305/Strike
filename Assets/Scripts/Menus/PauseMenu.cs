using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public Button quitButton;
    public Button backButton;
    public Button restartButton;
    
    private bool restarting = false;

    public void OnEnable()
    {
        quitButton.onClick.AddListener(QuitGame);
        backButton.onClick.AddListener(ReturnToGame);
        restartButton.onClick.AddListener(RestartLevel);
    }

    public void OnDisable()
    {
        quitButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveListener(ReturnToGame);
        restartButton.onClick.RemoveAllListeners();
    }

    public void QuitGame()
    {
        if (restarting)
            return;
        
        EventBus.Publish(new GameExitedEvent());
    }

    public void RestartLevel()
    {
        EventBus.Publish(new GameRestartedEvent());
    }

    public void ReturnToGame()
    {
        //Resume controls to game
    }
}
