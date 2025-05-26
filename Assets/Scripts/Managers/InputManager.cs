using System;
using UnityEngine;
using Zenject;

public class InputManager : MonoBehaviour
{
    private GameContext currentContext = GameContext.InMenu; 
    
    private MenuManager menuManager;
    
    [Inject]
    public void Construct(MenuManager _menuManager)
    {
        menuManager = _menuManager;
    }
    
    private void Start()
    {
        EventBus.Subscribe<InGameEvent>(InGame);
        EventBus.Subscribe<InMenuEvent>(InMenus);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<InGameEvent>(InGame);
        EventBus.Subscribe<InMenuEvent>(InMenus);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackAction();
        }
    }

    public void InGame(InGameEvent e)
    {
        SetContext(GameContext.InGame);
    }

    public void InMenus(InMenuEvent e)
    {
        SetContext(GameContext.InMenu);
    }
    
    public void SetContext(GameContext context)
    {
        currentContext = context;
    }

    private void HandleBackAction()
    {
        switch (currentContext)
        {
            case GameContext.InMenu:
                menuManager.CloseCurrentMenu();
                break;

            case GameContext.InGame:
                if (menuManager.IsAnyMenuOpen())
                {
                    menuManager.CloseCurrentMenu();
                }
                else
                {
                    menuManager.OpenMenu(MenuBase.MenuType.PauseMenu);
                }
                break;
        }
    }
}
