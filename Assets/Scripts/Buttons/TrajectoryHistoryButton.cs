using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrajectoryHistoryButton : MonoBehaviour
{
    public TrajectoryHistoryViewer trajectoryHistoryViewer;
    public Button button;
    
    private bool trajectoryHistoryBeingDisplayed = false;

    private void OnEnable()
    {
        trajectoryHistoryBeingDisplayed = false;
    }
    
    private void OnDisable()
    {
        trajectoryHistoryBeingDisplayed = false;
    }

    private void Start()
    {
        button.onClick.AddListener(ButtonPressed);
    }

    public void ButtonPressed()
    {
        EventBus.Publish(new HUDAction_CheckTrajectoryHistory());
        trajectoryHistoryBeingDisplayed = !trajectoryHistoryBeingDisplayed;
        if (trajectoryHistoryBeingDisplayed)
        {
            trajectoryHistoryViewer.ShowTrajectoryHistory();
        }
        else
        {
            trajectoryHistoryViewer.HideTrajectoryHistory();
        }
    }
}
