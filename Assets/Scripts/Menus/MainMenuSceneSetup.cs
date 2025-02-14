using UnityEngine;

public class MainMenuSceneSetup : MonoBehaviour
{
    public delegate void InMenu();
    public static event InMenu OnMenu;

    public LandingPage landingPage;
    
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
        InputManager.Instance.SetContext(GameContext.InMenu);
        landingPage.SetText("Loading Save file");
        CoroutineDispatcher.Instance.RunCoroutine(SaveManager.LoadSaveProcess());
    }

    public void SaveLoaded()
    {
        landingPage.SetText("Setting things up");
        ModeSelector.Instance.Init();
        OnMenu?.Invoke();
    }
    
    public void GameReady()
    {
        landingPage.SetText("Press any key to start");
    }
}
