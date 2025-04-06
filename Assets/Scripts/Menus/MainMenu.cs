using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button ballSelectionButton;
    public Button gameModeSelectionButton;
    public Button settingsPageButton;
    public Button tuorialButton;

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
        ballSelectionButton.onClick.RemoveAllListeners();
        ballSelectionButton.onClick.AddListener(() =>
        {
            MenuManager.Instance.OpenMenu(MenuBase.MenuType.BallSelectionPage);
        });
        
        gameModeSelectionButton.onClick.RemoveAllListeners();
        gameModeSelectionButton.onClick.AddListener(() =>
        {
            MenuManager.Instance.OpenMenu(MenuBase.MenuType.GameModeScreen);
        });
        
        settingsPageButton.onClick.RemoveAllListeners();
        settingsPageButton.onClick.AddListener(() =>
        {
            MenuManager.Instance.OpenMenu(MenuBase.MenuType.SettingsPage);
        });
        
        tuorialButton.onClick.RemoveAllListeners();
        tuorialButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(ModeSelector.Instance.GetTutorialLevel());
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