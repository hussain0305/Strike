using System;
using UnityEngine;
using UnityEngine.UI;

public class TrajectoryAdAttemptedEvent { }

public class TrajectoryAdButton : MonoBehaviour
{
    public Button button;

    private void OnEnable()
    {
        button.onClick.AddListener(AdButtonPressed);
    }

    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }

    public void AdButtonPressed()
    {
        EventBus.Publish(new TrajectoryAdAttemptedEvent());
    }
}
