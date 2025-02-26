using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    private Stack<GameObject> menuStack = new Stack<GameObject>();
    private Stack<GameObject> popupStack = new Stack<GameObject>();

    private Dictionary<MenuBase.MenuType, GameObject> menuDictionary = new Dictionary<MenuBase.MenuType, GameObject>();

    private GameContext currentGameContext = GameContext.InMenu;
    private MenuBase.MenuType? requiredMenuType = MenuBase.MenuType.MainMenu;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        EventBus.Subscribe<InGameEvent>(InGame);
        EventBus.Subscribe<InMenuEvent>(InMenus);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<InGameEvent>(InGame);
        EventBus.Unsubscribe<InMenuEvent>(InMenus);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        List<MenuBase.MenuType> toRemove = new List<MenuBase.MenuType>();
        
        foreach (var pair in menuDictionary)
        {
            if (pair.Value == null)
                toRemove.Add(pair.Key);
        }

        foreach (var key in toRemove)
        {
            menuDictionary.Remove(key);
        }

        menuStack.Clear();
        popupStack.Clear();
    }

    public void OpenMenu(MenuBase.MenuType menuType)
    {
        if (menuDictionary.ContainsKey(menuType))
        {
            OpenMenu(menuDictionary[menuType]);
        }
    }

    public void OpenMenu(GameObject menu)
    {
        CloseAllPopups();

        if (menuStack.Count > 0)
            menuStack.Peek().SetActive(false);
        
        menu.SetActive(true);
        menuStack.Push(menu);
    }

    public void CloseCurrentMenu()
    {
        if (menuStack.Count > 0)
        {
            GameObject topMenu = menuStack.Pop();
            topMenu.SetActive(false);

            if (menuStack.Count > 0)
            {
                menuStack.Peek().SetActive(true);
            }
            else if (requiredMenuType.HasValue && menuDictionary.ContainsKey(requiredMenuType.Value))
            {
                OpenMenu(requiredMenuType.Value);
            }
        }
    }

    public void OpenPopup(GameObject popup)
    {
        popup.SetActive(true);
        popupStack.Push(popup);
    }

    public void CloseCurrentPopup()
    {
        if (popupStack.Count > 0)
        {
            GameObject topPopup = popupStack.Pop();
            topPopup.SetActive(false);
        }
    }

    public void CloseAllPopups()
    {
        while (popupStack.Count > 0)
        {
            GameObject topPopup = popupStack.Pop();
            topPopup.SetActive(false);
        }
    }

    public void RegisterMenu(MenuBase menuBase)
    {
        menuDictionary.Add(menuBase.menuType, menuBase.gameObject);
    }

    public bool IsAnyMenuOpen()
    {
        return menuStack.Count != 0;
    }
    
    public void InGame(InGameEvent e)
    {
        currentGameContext = GameContext.InGame;
        requiredMenuType = null;
    }

    public void InMenus(InMenuEvent e)
    {
        currentGameContext = GameContext.InMenu;
        requiredMenuType = MenuBase.MenuType.MainMenu;
    }

}