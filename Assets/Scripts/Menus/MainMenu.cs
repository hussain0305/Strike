using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

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

    private MenuManager menuManager;
    private ModeSelector modeSelector;
    
    [Inject]
    public void Construct(MenuManager _menuManager, ModeSelector _modeSelector)
    {
        menuManager = _menuManager;
        modeSelector = _modeSelector;
    }

    private void OnEnable()
    {
        ballSelectionButton.onClick.RemoveAllListeners();
        ballSelectionButton.onClick.AddListener(() =>
        {
            menuManager.OpenMenu(MenuBase.MenuType.BallSelectionPage);
        });
        
        gauntletModeButton.onClick.RemoveAllListeners();
        gauntletModeButton.onClick.AddListener(() =>
        {
            menuManager.OpenMenu(MenuBase.MenuType.GameModeScreen);
        });
        
        randomizerModeButton.onClick.RemoveAllListeners();
        randomizerModeButton.onClick.AddListener(() =>
        {
            menuManager.OpenMenu(MenuBase.MenuType.RandomizerPage);
        });
        
        settingsPageButton.onClick.RemoveAllListeners();
        settingsPageButton.onClick.AddListener(() =>
        {
            menuManager.OpenMenu(MenuBase.MenuType.SettingsPage);
        });
        
        tuorialButton.onClick.RemoveAllListeners();
        tuorialButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(modeSelector.GetTutorialLevel());
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