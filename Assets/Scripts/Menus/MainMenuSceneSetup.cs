using UnityEngine;

public class InMenuEvent { }

public class MainMenuSceneSetup : MonoBehaviour
{
    public LandingPage landingPage;
    
    private void OnEnable()
    {
        EventBus.Subscribe<SaveLoadedEvent>(SaveLoaded);
        EventBus.Subscribe<GameReadyEvent>(GameReady);
        SaveManager.LoadData();
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<SaveLoadedEvent>(SaveLoaded);
        EventBus.Unsubscribe<GameReadyEvent>(GameReady);
    }

    private void Start()
    {
        InputManager.Instance.SetContext(GameContext.InMenu);
        landingPage.SetText("Loading Save file");
        CoroutineDispatcher.Instance.RunCoroutine(SaveManager.LoadSaveProcess());
    }

    public void SaveLoaded(SaveLoadedEvent e)
    {
        landingPage.SetText("Setting things up");
        ModeSelector.Instance.Init();
        EventBus.Publish(new InMenuEvent());
    }
    
    public void GameReady(GameReadyEvent e)
    {
        landingPage.SetText("Press any key to start");
    }
}
