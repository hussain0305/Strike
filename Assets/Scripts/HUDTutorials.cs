using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDTutorials : MonoBehaviour
{
    public TextMeshProUGUI cameraTutorial;
    public TextMeshProUGUI trajectoryTutorial;
    public TextMeshProUGUI trajectoryHistoryTutorial;

    private bool cameraTutorialShown;
    private bool trajectoryTutorialShown;
    private bool trajectoryHistoryTutorialShown;

    private const string CAM_KEY = "cameraTutorial";
    private const string TRAJECTORY_KEY = "trajectoryTutorial";
    private const string TRAJECTORY_HISTORY_KEY = "trajectoryHistoryTutorial";

    private float flickerSpeed = 2f;
    private List<TextMeshProUGUI> flickeringTexts = new List<TextMeshProUGUI>();
    
    private void Start()
    {
        cameraTutorialShown = PlayerPrefs.GetInt(CAM_KEY, 0) == 1;
        trajectoryTutorialShown = PlayerPrefs.GetInt(TRAJECTORY_KEY, 0) == 1;
        trajectoryHistoryTutorialShown = PlayerPrefs.GetInt(TRAJECTORY_HISTORY_KEY, 0) == 1;

        DisableAllTexts();
        
        if (!cameraTutorialShown)
        {
            cameraTutorial.gameObject.SetActive(true);
            flickeringTexts.Add(cameraTutorial);
        }
        if (!trajectoryTutorialShown)
        {
            trajectoryTutorial.gameObject.SetActive(true);
            flickeringTexts.Add(trajectoryTutorial);
        }
        if (!trajectoryHistoryTutorialShown)
        {
            trajectoryHistoryTutorial.gameObject.SetActive(true);
            flickeringTexts.Add(trajectoryHistoryTutorial);
        }        
        CheckIfItShouldStillbeActive();
    }
    
    private void OnEnable()
    {
        EventBus.Subscribe<HUDAction_CheckTrajectory>(CheckedTrajectory);
        EventBus.Subscribe<HUDAction_CheckTrajectoryHistory>(CheckedTrajectoryHistory);
        EventBus.Subscribe<HUDAction_CheckedCameraOptions>(CheckedCameraOptions);
    }
    
    private void OnDisable()
    {
        UnsubscribeAll();
    }

    private void Update()
    {
        for (int i = 0; i < flickeringTexts.Count; i++)
        {
            Color color = flickeringTexts[i].color;
            color.a = Mathf.Abs(Mathf.Sin(Time.time * flickerSpeed));
            flickeringTexts[i].color = color;
        }
    }

    private void UnsubscribeAll()
    {
        EventBus.Unsubscribe<HUDAction_CheckTrajectory>(CheckedTrajectory);
        EventBus.Unsubscribe<HUDAction_CheckTrajectoryHistory>(CheckedTrajectoryHistory);
        EventBus.Unsubscribe<HUDAction_CheckedCameraOptions>(CheckedCameraOptions);
    }

    private void DisableAllTexts()
    {
        cameraTutorial.gameObject.SetActive(false);
        trajectoryTutorial.gameObject.SetActive(false);
        trajectoryHistoryTutorial.gameObject.SetActive(false);
    }

    private void CheckedCameraOptions(HUDAction_CheckedCameraOptions e)
    {
        PlayerPrefs.SetInt(CAM_KEY, 1);
        
        if (flickeringTexts.Contains(cameraTutorial))
            flickeringTexts.Remove(cameraTutorial);
        
        cameraTutorial.gameObject.SetActive(false);
            
        CheckIfItShouldStillbeActive();
    }

    private void CheckedTrajectory(HUDAction_CheckTrajectory e)
    {
        PlayerPrefs.SetInt(TRAJECTORY_KEY, 1);
            
        if (flickeringTexts.Contains(trajectoryTutorial))
            flickeringTexts.Remove(trajectoryTutorial);
        
        trajectoryTutorial.gameObject.SetActive(false);
           
        CheckIfItShouldStillbeActive();
    }

    private void CheckedTrajectoryHistory(HUDAction_CheckTrajectoryHistory e)
    {
        PlayerPrefs.SetInt(TRAJECTORY_HISTORY_KEY, 1);
            
        if (flickeringTexts.Contains(trajectoryHistoryTutorial))
            flickeringTexts.Remove(trajectoryHistoryTutorial);
        
        trajectoryHistoryTutorial.gameObject.SetActive(false);
        
        CheckIfItShouldStillbeActive();
    }

    private void CheckIfItShouldStillbeActive()
    {
        if (flickeringTexts.Count != 0)
            return;
        
        UnsubscribeAll();
        DisableAllTexts();
        Destroy(gameObject);
    }
}
