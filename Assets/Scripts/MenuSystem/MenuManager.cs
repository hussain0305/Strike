using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    private Stack<GameObject> menuStack = new Stack<GameObject>();
    private Stack<GameObject> popupStack = new Stack<GameObject>();

    private Dictionary<MenuBase.MenuType, GameObject> menuDictionary = new Dictionary<MenuBase.MenuType, GameObject>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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
                menuStack.Peek().SetActive(true);
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
}