using System;
using UnityEngine;

public class MenuBase : MonoBehaviour
{
    [System.Serializable]
    public enum MenuType
    {
        MainMenu,
        GameModeScreen,
        BallSelectionPage
    };
    
    [System.Serializable]
    public enum PopupType
    {
        Settings
    };

    public MenuType menuType;
    public bool disableAfterRegister = true;
    public bool waitForSaveFileLoaded = false;

    public void Start()
    {
        Register();
    }
    
    public void Register()
    {
        MenuManager.Instance.RegisterMenu(this);
        if (waitForSaveFileLoaded)
        {
            SaveManager.OnSaveFileLoaded += OnSaveFileLoaded;
        }
        else if (disableAfterRegister)
        {
            gameObject.SetActive(false);
        }
    }
    
    private void OnSaveFileLoaded()
    {
        SaveManager.OnSaveFileLoaded -= OnSaveFileLoaded;
        if (disableAfterRegister)
        {
            gameObject.SetActive(false);
        }
    }
}