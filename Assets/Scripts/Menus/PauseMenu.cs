using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public Button quitButton;
    public Button backButton;
    
    public void OnEnable()
    {
        quitButton.onClick.AddListener(QuitGame);
        backButton.onClick.AddListener(ReturnToGame);
    }

    public void OnDisable()
    {
        quitButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveListener(ReturnToGame);
    }

    public void QuitGame()
    {
        EventBus.Publish(new GameExitedEvent());
    }

    public void ReturnToGame()
    {
        //Resume controls to game
    }
}
