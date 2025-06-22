using System;
using UnityEngine;
using Zenject;

public class MenuBase : MonoBehaviour
{
    [System.Serializable]
    public enum MenuType
    {
        None = -1,
        MainMenu,
        GameModeScreen,
        BallSelectionPage,
        QuitGameScreen,
        PauseMenu,
        SettingsPage,
        RandomizerPage,
        AdditionalVolleyOfferPopup,
        EarnStarsPage
    };
    
    [System.Serializable]
    public enum PopupType
    {
        Settings
    };

    public MenuType menuType;
    public bool disableAfterRegister = true;
    public bool waitForSaveFileLoaded = false;

    private MenuManager menuManager;
    
    [Inject]
    public void Construct(MenuManager _menuManager)
    {
        menuManager = _menuManager;
    }

    public void Start()
    {
        Register();
    }
    
    public void Register()
    {
        menuManager.RegisterMenu(this);
        if (waitForSaveFileLoaded)
        {
            EventBus.Subscribe<SaveLoadedEvent>(OnSaveFileLoaded);
        }
        else if (disableAfterRegister)
        {
            gameObject.SetActive(false);
        }
    }
    
    private void OnSaveFileLoaded(SaveLoadedEvent e)
    {
        EventBus.Unsubscribe<SaveLoadedEvent>(OnSaveFileLoaded);
        if (waitForSaveFileLoaded)
        {
            gameObject.SetActive(false);
        }
    }
}