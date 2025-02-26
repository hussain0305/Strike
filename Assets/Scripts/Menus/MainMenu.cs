using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button ballSelectionButton;
    public Button gameModeSelectionButton;

    public static MenuContext Context { get; private set; }

    private void Awake()
    {
        if (Context == null)
        {
            Context = new MenuContext();
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

    private void OnDestroy()
    {
        Context = null;
    }
}
