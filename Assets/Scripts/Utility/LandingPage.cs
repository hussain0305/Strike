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
        loadingScreenText.text = "LOADING SAVE FILE";
        CoroutineDispatcher.Instance.RunCoroutine(SaveManager.LoadSaveProcess());
    }

    public void SaveLoaded()
    {
        loadingScreenText.text = "SETTING THINGS UP";
        ModeSelector.Instance.Init();
    }
    
    public void GameReady()
    {
        loadingScreenText.text = "PRESS ANY KEY TO START";
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