using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class LandingPage : MonoBehaviour
{
    public TextMeshProUGUI loadingScreenText;
    
    private bool isKeyPressed = false;
    
    private MenuManager menuManager;
    private GameStateManager gameStateManager;
    
    [Inject]
    public void Construct(MenuManager _menuManager, GameStateManager _gameStateManager)
    {
        menuManager = _menuManager;
        gameStateManager = _gameStateManager;
    }

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
            gameStateManager.SetGameState(GameState.Menu);
            menuManager.OpenMenu(MenuBase.MenuType.MainMenu);
            gameObject.SetActive(false);
        }
        isKeyPressed = true;
        StartCoroutine(MainMenuOpen());
    }
}