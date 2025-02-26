using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button ballSelectionButton;
    public Button gameModeSelectionButton;

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
    }

    private void OnDisable()
    {
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
