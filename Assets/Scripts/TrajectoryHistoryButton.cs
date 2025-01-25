using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrajectoryHistoryButton : MonoBehaviour
{
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
        button.onClick.AddListener(() =>
        {
            trajectoryHistoryBeingDisplayed = !trajectoryHistoryBeingDisplayed;
            if (trajectoryHistoryBeingDisplayed)
            {
                TrajectoryHistoryViewer.Instance.ShowTrajectoryHistory();
            }
            else
            {
                TrajectoryHistoryViewer.Instance.HideTrajectoryHistory();
            }
        });
    }
}
