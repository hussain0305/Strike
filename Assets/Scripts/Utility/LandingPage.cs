using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LandingPage : MonoBehaviour
{
    public TextMeshProUGUI loadingScreenText;
    
    private bool isKeyPressed = false;
    
    void Update()
    {
        if(SaveManager.IsGameReady && !isKeyPressed && (Input.anyKey || Input.GetMouseButtonDown(0)))
        {
            ProceedToMainMenu();
            isKeyPressed = true;
        }
    }

    public void SetText(string _text)
    {
        loadingScreenText.text = _text;
    }

    public void ProceedToMainMenu(float delay = 0f)
    {
        IEnumerator MainMenuOpen()
        {
            yield return new WaitForSeconds(delay);
            GameStateManager.Instance.SetGameState(GameState.Menu);
            MenuManager.Instance.OpenMenu(MenuBase.MenuType.MainMenu);
            gameObject.SetActive(false);
        }
        isKeyPressed = true;
        StartCoroutine(MainMenuOpen());
    }
}