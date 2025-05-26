using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GoingBackEvent { }

public class BackButton : MonoBehaviour
{
    public Button backButton;

    private MenuManager menuManager;
    
    [Inject]
    public void Construct(MenuManager _menuManager)
    {
        menuManager = _menuManager;
    }

    private void OnEnable()
    {
        backButton.onClick.AddListener(BackButtonPressed);
    }

    private void OnDisable()
    {
        backButton.onClick.RemoveListener(BackButtonPressed);
    }
    
    public void BackButtonPressed()
    {
        EventBus.Publish(new GoingBackEvent());
        menuManager.CloseCurrentMenu();
    }
}