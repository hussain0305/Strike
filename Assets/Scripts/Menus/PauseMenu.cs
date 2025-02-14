using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public delegate void GameExitedPrematurely();
    public static event GameExitedPrematurely OnGameExitedPrematurely;

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
        OnGameExitedPrematurely?.Invoke();
        SceneManager.LoadScene(0);
    }

    public void ReturnToGame()
    {
        //Resume controls to game
    }
}
