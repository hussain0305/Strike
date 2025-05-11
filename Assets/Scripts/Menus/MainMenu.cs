using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button ballSelectionButton;
    public Button gauntletModeButton;
    public Button randomizerModeButton;
    public Button settingsPageButton;
    public Button tuorialButton;
    public Button exitButton;

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
        
        gauntletModeButton.onClick.RemoveAllListeners();
        gauntletModeButton.onClick.AddListener(() =>
        {
            MenuManager.Instance.OpenMenu(MenuBase.MenuType.GameModeScreen);
        });
        
        randomizerModeButton.onClick.RemoveAllListeners();
        randomizerModeButton.onClick.AddListener(() =>
        {
            MenuManager.Instance.OpenMenu(MenuBase.MenuType.RandomizerPage);
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
        
        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(Application.Quit);
    }

    private void OnDisable()
    {
        settingsPageButton.onClick.RemoveAllListeners();
        ballSelectionButton.onClick.RemoveAllListeners();
        gauntletModeButton.onClick.RemoveAllListeners();
        randomizerModeButton.onClick.RemoveAllListeners();
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