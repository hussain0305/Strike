using System;
using UnityEngine;
using UnityEngine.UI;

public class GoingBackEvent { }

public class BackButton : MonoBehaviour
{
    public Button backButton;

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
        MenuManager.Instance.CloseCurrentMenu();
    }
}