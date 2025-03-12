using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button ballSelectionButton;
    public Button gameModeSelectionButton;
    public Button settingsPageButton;

    private static MenuContext context;
    public static MenuContext Context
    {
        get
        {
            if (context == null)
            {
                context = new MenuContext();
            }
            return context;
        }
    }

    private static ITrajectoryModifier trajectoryModifier;
    public static ITrajectoryModifier TrajectoryModifier
    {
        get
        {
            if (trajectoryModifier == null)
            {
                trajectoryModifier = new DefaultTrajectoryModifier();
            }
            return trajectoryModifier;
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
        context = null;
    }
    
    private void OnDestroy()
    {
        ClearContext();
    }
}