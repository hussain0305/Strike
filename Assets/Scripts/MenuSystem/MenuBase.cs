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

    public void Start()
    {
        RegisterAndDisable();
    }
    
    public void RegisterAndDisable()
    {
        MenuManager.Instance.RegisterMenu(this);
        gameObject.SetActive(false);
    }
}