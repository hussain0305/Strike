using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LandingPage : MonoBehaviour
{
    public TextMeshProUGUI loadingScreenText;
    
    private bool isKeyPressed = false;

    private void OnEnable()
    {
        SaveManager.OnSaveFileLoaded += SaveLoaded;
        SaveManager.OnGameReady += GameReady;
        SaveManager.LoadData();
    }

    private void OnDisable()
    {
        SaveManager.OnSaveFileLoaded -= SaveLoaded;
        SaveManager.OnGameReady -= GameReady;
    }

    private void Start()
    {
        loadingScreenText.text = "Loading Save file";
        CoroutineDispatcher.Instance.RunCoroutine(SaveManager.LoadSaveProcess());
    }

    public void SaveLoaded()
    {
        loadingScreenText.text = "Setting things up";
        ModeSelector.Instance.Init();
    }
    
    public void GameReady()
    {
        loadingScreenText.text = "Press any key to start";
    }

    void Update()
    {
        if(SaveManager.IsGameReady && !isKeyPressed && (Input.anyKey || Input.GetMouseButtonDown(0)))
        {
            isKeyPressed = true;
            MenuManager.Instance.OpenMenu(MenuBase.MenuType.MainMenu);
            gameObject.SetActive(false);
        }
    }
}