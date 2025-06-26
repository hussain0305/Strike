using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDTutorials : MonoBehaviour
{
    public TextMeshProUGUI cameraTutorial;
    public TextMeshProUGUI trajectoryTutorial;
    public TextMeshProUGUI trajectoryHistoryTutorial;
    public TextMeshProUGUI spinButtonTutorial;
    public TextMeshProUGUI angleButtonTutorial;
    public TextMeshProUGUI powerButtonTutorial;
    public TextMeshProUGUI shootButtonTutorial;
    public TextMeshProUGUI swipeTutorial;

    private bool cameraTutorialShown;
    private bool trajectoryTutorialShown;
    private bool trajectoryHistoryTutorialShown;
    private bool spinTutorialShown;
    private bool angleTutorialShown;
    private bool powerTutorialShown;
    private bool shootTutorialShown;
    private bool swipeTutorialShown;

    private const string CAM_KEY = "cameraTutorial";
    private const string TRAJECTORY_KEY = "trajectoryTutorial";
    private const string TRAJECTORY_HISTORY_KEY = "trajectoryHistoryTutorial";
    private const string SPIN_KEY = "spinTutorial";
    private const string ANGLE_KEY = "angleTutorial";
    private const string POWER_KEY = "powerTutorial";
    private const string SHOOT_KEY = "shootTutorial";
    private const string SWIPE_KEY = "swipeTutorial";

    private float flickerSpeed = 2f;
    private List<TextMeshProUGUI> flickeringTexts = new List<TextMeshProUGUI>();
    
    private void Start()
    {
        cameraTutorialShown = PlayerPrefs.GetInt(CAM_KEY, 0) == 1;
        trajectoryTutorialShown = PlayerPrefs.GetInt(TRAJECTORY_KEY, 0) == 1;
        trajectoryHistoryTutorialShown = PlayerPrefs.GetInt(TRAJECTORY_HISTORY_KEY, 0) == 1;
        spinTutorialShown = PlayerPrefs.GetInt(SPIN_KEY, 0) == 1;
        angleTutorialShown = PlayerPrefs.GetInt(ANGLE_KEY, 0) == 1;
        powerTutorialShown = PlayerPrefs.GetInt(POWER_KEY, 0) == 1;
        shootTutorialShown = PlayerPrefs.GetInt(SHOOT_KEY, 0) == 1;
        swipeTutorialShown = PlayerPrefs.GetInt(SWIPE_KEY, 0) == 1;

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
        if (!spinTutorialShown)
        {
            spinButtonTutorial.gameObject.SetActive(true);
            flickeringTexts.Add(spinButtonTutorial);
        }        
        if (!angleTutorialShown)
        {
            angleButtonTutorial.gameObject.SetActive(true);
            flickeringTexts.Add(angleButtonTutorial);
        }        
        if (!powerTutorialShown)
        {
            powerButtonTutorial.gameObject.SetActive(true);
            flickeringTexts.Add(powerButtonTutorial);
        }        
        if (!shootTutorialShown)
        {
            shootButtonTutorial.gameObject.SetActive(true);
            flickeringTexts.Add(shootButtonTutorial);
        }
        if (!swipeTutorialShown)
        {
            //This is correct, swipeTutorial will be inactive at the beginning
            swipeTutorial.gameObject.SetActive(false);
            flickeringTexts.Add(swipeTutorial);
        }
        CheckIfItShouldStillbeActive();
    }
    
    private void OnEnable()
    {
        EventBus.Subscribe<HUDAction_CheckTrajectory>(CheckedTrajectory);
        EventBus.Subscribe<HUDAction_CheckTrajectoryHistory>(CheckedTrajectoryHistory);
        EventBus.Subscribe<HUDAction_CheckedCameraOptions>(CheckedCameraOptions);
        EventBus.Subscribe<ShotInputSwitchedEvent>(ShotInputSwitched);
        EventBus.Subscribe<StoppedShotInput>(StoppedShotInput);
        EventBus.Subscribe<BallShotEvent>(BallShot);
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
        EventBus.Unsubscribe<ShotInputSwitchedEvent>(ShotInputSwitched);
        EventBus.Unsubscribe<StoppedShotInput>(StoppedShotInput);
        EventBus.Unsubscribe<BallShotEvent>(BallShot);
    }

    private void DisableAllTexts()
    {
        cameraTutorial.gameObject.SetActive(false);
        trajectoryTutorial.gameObject.SetActive(false);
        trajectoryHistoryTutorial.gameObject.SetActive(false);
        spinButtonTutorial.gameObject.SetActive(false);
        angleButtonTutorial.gameObject.SetActive(false);
        powerButtonTutorial.gameObject.SetActive(false);
        shootButtonTutorial.gameObject.SetActive(false);
        swipeTutorial.gameObject.SetActive(false);
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

    private void ShotInputSwitched(ShotInputSwitchedEvent e)
    {
        if (!swipeTutorialShown)
        {
            swipeTutorial.gameObject.SetActive(true);
        }

        switch (e.ShotParameter)
        {
            case ShotInput.ShotParameter.Angle:
                AngleInput();
                break;
            case ShotInput.ShotParameter.Spin:
                SpinInput();
                break;
            case ShotInput.ShotParameter.Power:
                PowerInput();
                break;
        }
    }

    private void SpinInput()
    {
        PlayerPrefs.SetInt(SPIN_KEY, 1);
        
        if (flickeringTexts.Contains(spinButtonTutorial))
            flickeringTexts.Remove(spinButtonTutorial);

        spinButtonTutorial.gameObject.SetActive(false);
        
        CheckIfItShouldStillbeActive();
    }

    private void AngleInput()
    {
        PlayerPrefs.SetInt(ANGLE_KEY, 1);
        
        if (flickeringTexts.Contains(angleButtonTutorial))
            flickeringTexts.Remove(angleButtonTutorial);

        angleButtonTutorial.gameObject.SetActive(false);
        
        CheckIfItShouldStillbeActive();
    }

    private void PowerInput()
    {
        PlayerPrefs.SetInt(POWER_KEY, 1);
        
        if (flickeringTexts.Contains(powerButtonTutorial))
            flickeringTexts.Remove(powerButtonTutorial);

        powerButtonTutorial.gameObject.SetActive(false);
        
        CheckIfItShouldStillbeActive();
    }

    private void StoppedShotInput(StoppedShotInput e)
    {
        PlayerPrefs.SetInt(SWIPE_KEY, 1);
        
        if (flickeringTexts.Contains(swipeTutorial))
            flickeringTexts.Remove(swipeTutorial);

        swipeTutorial.gameObject.SetActive(false);
        
        CheckIfItShouldStillbeActive();
    }

    private void BallShot(BallShotEvent e)
    {
        PlayerPrefs.SetInt(SHOOT_KEY, 1);
        
        if (flickeringTexts.Contains(shootButtonTutorial))
            flickeringTexts.Remove(shootButtonTutorial);

        shootButtonTutorial.gameObject.SetActive(false);
        
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
