using System;
using TMPro;
using UnityEngine;

public class EndlessModeLoadingProgress
{
    public string Message;
    public int StepNumber;
    public bool Completed;
    
    public EndlessModeLoadingProgress(string message, int stepNumber)
    {
        Message = message;
        StepNumber = stepNumber;
        Completed = false;
    }
    public EndlessModeLoadingProgress(bool completed)
    {
        Message = "";
        StepNumber = -1;
        Completed = completed;
    }
}

public class EndlessModeLoadingScreen : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    public GameManager gameManager;
    
    //TODO: Hard-coded value here, think of a better way if there's time later on
    private const int TOTAL_STEPS = 5;

    private void OnEnable()
    {
        EventBus.Subscribe<EndlessModeLoadingProgress>(OnLoadingProgress);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<EndlessModeLoadingProgress>(OnLoadingProgress);
    }

    private void Start()
    {
        gameManager.enabled = true;
    }

    private void OnLoadingProgress(EndlessModeLoadingProgress e)
    {
        if (e.Completed)
        {
            Destroy(this.gameObject);
        }
        else
        {
            loadingText.text = $"({e.StepNumber}/{TOTAL_STEPS}) {e.Message}";
        }
    }
}
