using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button ballSelectionButton;
    public Button gameModeSelectionButton;
    public Button settingsPageButton;

    private static MenuContext _context;
    public static MenuContext Context
    {
        get
        {
            if (_context == null)
            {
                _context = new MenuContext();
            }
            return _context;
        }
    }

    private void OnEnable()
    {
        ballSelectionButton.onClick.AddListener(() =>
        {
            MenuManager.Instance.OpenMenu(MenuBase.MenuType.BallSelectionPage);
        });
        
        gameModeSelectionButton.onClick.AddListener(() =>
        {
            MenuManager.Instance.OpenMenu(MenuBase.MenuType.GameModeScreen);
        });
        settingsPageButton.onClick.AddListener(() =>
        {
            MenuManager.Instance.OpenMenu(MenuBase.MenuType.SettingsPage);
        });
    }

    private void OnDisable()
    {
        settingsPageButton.onClick.RemoveAllListeners();
        ballSelectionButton.onClick.RemoveAllListeners();
        gameModeSelectionButton.onClick.RemoveAllListeners();
    }

    public static void ClearContext()
    {
        _context = null;
    }
    
    private void OnDestroy()
    {
        ClearContext();
    }
}
