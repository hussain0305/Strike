using System;
using UnityEngine;
using UnityEngine.UI;

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
        MenuManager.Instance.CloseCurrentMenu();
    }
}