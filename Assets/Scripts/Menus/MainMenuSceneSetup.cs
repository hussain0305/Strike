using UnityEngine;
using Zenject;

public class InMenuEvent { }

public class MainMenuSceneSetup : MonoBehaviour
{
    public LandingPage landingPage;
    
    private ModeSelector modeSelector;
    private InputManager inputManager;
    
    [Inject]
    public void Construct(ModeSelector _modeSelector, InputManager _inputManager)
    {
        modeSelector = _modeSelector;
        inputManager = _inputManager;
    }

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
        inputManager.SetContext(GameContext.InMenu);
        landingPage.SetText("Loading Save file");
        CoroutineDispatcher.Instance.RunCoroutine(SaveManager.LoadSaveProcess());
    }

    public void SaveLoaded(SaveLoadedEvent e)
    {
        landingPage.SetText("Setting things up");
        modeSelector.Init();
        EventBus.Publish(new InMenuEvent());
    }
    
    public void GameReady(GameReadyEvent e)
    {
        landingPage.SetText("Press any key to start");
    }
}
