using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoppedShotInput { }

public class ShotInputSwitchedEvent
{
    public ShotInput.ShotParameter ShotParameter;
    public ShotInputSwitchedEvent(ShotInput.ShotParameter shotParameter)
    {
        ShotParameter = shotParameter;
    }
}

public class ShotInput : MonoBehaviour
{
    public enum ShotParameter
    {
        None,
        Angle,
        Spin,
        Power
    }

    public Button spinButton;
    public Button angleButton;
    public Button powerButton;

    public TextMeshProUGUI spinTextNormal;
    public TextMeshProUGUI spinTextGlowy;
    public TextMeshProUGUI spinTextTutorial;
    
    public TextMeshProUGUI angleTextNormal;
    public TextMeshProUGUI angleTextGlowy;
    public TextMeshProUGUI angleTextTutorial;
    
    public TextMeshProUGUI powerTextNormal;
    public TextMeshProUGUI powerTextGlowy;
    public TextMeshProUGUI powerTextTutorial;

    public SpinInput spinInput;
    public AngleInput angleInput;
    public PowerInput powerInput;

    private bool isCurrentlyAtAimCamPosition = false;
    private Coroutine movingButtonsCoroutine;
    private const float SPIN_BUTTON_DEFAULT_POSITION = -175f;
    private const float ANGLE_BUTTON_DEFAULT_POSITION = -225f;
    private const float POWER_BUTTON_DEFAULT_POSITION = 200f;
    private const float SPIN_BUTTON_AIM_CAM_POSITION = -325f;
    private const float ANGLE_BUTTON_AIM_CAM_POSITION = -420f;
    private const float POWER_BUTTON_AIM_CAM_POSITION = 380f;
    private RectTransform spinButtonRect;
    private RectTransform SpinButtonRect => spinButtonRect ??= spinButton.GetComponent<RectTransform>();
    private RectTransform angleButtonRect;
    private RectTransform AngleButtonRect => angleButtonRect ??= angleButton.GetComponent<RectTransform>();
    private RectTransform powerButtonRect;
    private RectTransform PowerButtonRect => powerButtonRect ??= powerButton.GetComponent<RectTransform>();

    private void OnEnable()
    {
        EventBus.Subscribe<NextShotCuedEvent>(NextShotCued);
        EventBus.Subscribe<ShotInputSwitchedEvent>(ShotParameterInputSwitched);
        EventBus.Subscribe<CameraSwitchProcessedEvent>(CameraSwitchProcessed);
        EventBus.Subscribe<CameraSwitchCompletedEvent>(CameraSwitchCompleted);
        
        spinButton.onClick.AddListener(() =>
        {
            SetShotParameterSelected(ShotParameter.Spin);
        });
        
        angleButton.onClick.AddListener(() =>
        {
            SetShotParameterSelected(ShotParameter.Angle);
        });

        powerButton.onClick.AddListener(() =>
        {
            SetShotParameterSelected(ShotParameter.Power);
        });
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ShotInputSwitchedEvent>(ShotParameterInputSwitched);
        EventBus.Unsubscribe<NextShotCuedEvent>(NextShotCued);
        EventBus.Unsubscribe<CameraSwitchProcessedEvent>(CameraSwitchProcessed);
        EventBus.Unsubscribe<CameraSwitchCompletedEvent>(CameraSwitchCompleted);

        spinButton.onClick.RemoveAllListeners();
        angleButton.onClick.RemoveAllListeners();
        powerButton.onClick.RemoveAllListeners();
    }

    private ShotParameter currentParameterInput = ShotParameter.None;

    public bool IsInputtingSpin()
    {
        return currentParameterInput == ShotParameter.Spin;
    }
    public bool IsInputtingAngle()
    {
        return currentParameterInput == ShotParameter.Angle;
    }
    public bool IsInputtingPower()
    {
        return currentParameterInput == ShotParameter.Power;
    }

    private void ShotParameterInputSwitched(ShotInputSwitchedEvent e)
    {
        currentParameterInput = e.ShotParameter;
    }

    public void NextShotCued(NextShotCuedEvent e)
    {
        currentParameterInput = ShotParameter.None;
        spinTextNormal.text = "Spin";
        spinTextGlowy.text = "Spin";
        spinTextTutorial.text = "Click to set";
        
        angleTextNormal.text = "Angle";
        angleTextGlowy.text = "Angle";
        angleTextTutorial.text = "Click to set";

        powerTextNormal.text = "Power";
        powerTextGlowy.text = "Power";
        powerTextTutorial.text = "Click to set";

        SetShotParameterSelected();
    }

    public void SetShotParameterSelected()
    {
        spinTextNormal.gameObject.SetActive(true);
        spinTextGlowy.gameObject.SetActive(false);
        
        angleTextNormal.gameObject.SetActive(true);
        angleTextGlowy.gameObject.SetActive(false);
        
        powerTextNormal.gameObject.SetActive(true);
        powerTextGlowy.gameObject.SetActive(false);
    }
    
    public void SetShotParameterSelected(ShotParameter shotParameter)
    {
        SetShotParameterSelected();
        EventBus.Publish(new ShotInputSwitchedEvent(shotParameter));
        switch (shotParameter)
        {
            case ShotParameter.Spin:
                spinTextNormal.gameObject.SetActive(false);
                spinTextGlowy.gameObject.SetActive(true);
                spinTextTutorial.text = "SPIN";
                break;
            case ShotParameter.Angle:
                angleTextNormal.gameObject.SetActive(false);
                angleTextGlowy.gameObject.SetActive(true);
                angleTextTutorial.text = "ANGLE";
                break;
            case ShotParameter.Power:
                powerTextNormal.gameObject.SetActive(false);
                powerTextGlowy.gameObject.SetActive(true);
                powerTextTutorial.text = "POWER";
                break;
        }
    }
    
    public void CameraSwitchProcessed(CameraSwitchProcessedEvent e)
    {
        if (movingButtonsCoroutine != null)
        {
            StopCoroutine(movingButtonsCoroutine);
        }
        bool newAtAimPositionVal = e.NewCameraPos.cameraNumber == 1;
        if (newAtAimPositionVal == isCurrentlyAtAimCamPosition)
        {
            return;
        }
        isCurrentlyAtAimCamPosition = newAtAimPositionVal;
        movingButtonsCoroutine = StartCoroutine(MoveButtonsCoroutine(e.SwitchTime - 0.05f, isCurrentlyAtAimCamPosition));
    }
    
    public void CameraSwitchCompleted(CameraSwitchCompletedEvent e)
    {
        if (movingButtonsCoroutine != null)
        {
            StopCoroutine(movingButtonsCoroutine);
        }
        movingButtonsCoroutine = null;

        bool aimPositions = e.NewCameraPos.cameraNumber == 1;
        isCurrentlyAtAimCamPosition = aimPositions;

        SpinButtonRect.anchoredPosition3D = new Vector3(
            aimPositions ? SPIN_BUTTON_AIM_CAM_POSITION : SPIN_BUTTON_DEFAULT_POSITION,
            SpinButtonRect.anchoredPosition3D.y, SpinButtonRect.anchoredPosition3D.z);
        
        AngleButtonRect.anchoredPosition3D = new Vector3(
            aimPositions ? ANGLE_BUTTON_AIM_CAM_POSITION : ANGLE_BUTTON_DEFAULT_POSITION,
            AngleButtonRect.anchoredPosition3D.y, AngleButtonRect.anchoredPosition3D.z);
        
        PowerButtonRect.anchoredPosition3D = new Vector3(
            aimPositions ? POWER_BUTTON_AIM_CAM_POSITION : POWER_BUTTON_DEFAULT_POSITION,
            PowerButtonRect.anchoredPosition3D.y, PowerButtonRect.anchoredPosition3D.z);
    }

    private IEnumerator MoveButtonsCoroutine(float time, bool targetIsAimCam)
    {
        float timePassed = 0;
        float spinStartX = targetIsAimCam ? SPIN_BUTTON_DEFAULT_POSITION : SPIN_BUTTON_AIM_CAM_POSITION;
        float spinEndX = targetIsAimCam ? SPIN_BUTTON_AIM_CAM_POSITION : SPIN_BUTTON_DEFAULT_POSITION;
        float angleStartX = targetIsAimCam ? ANGLE_BUTTON_DEFAULT_POSITION : ANGLE_BUTTON_AIM_CAM_POSITION;
        float angleEndX = targetIsAimCam ? ANGLE_BUTTON_AIM_CAM_POSITION : ANGLE_BUTTON_DEFAULT_POSITION;
        float powerStartX = targetIsAimCam ? POWER_BUTTON_DEFAULT_POSITION : POWER_BUTTON_AIM_CAM_POSITION;
        float powerEndX = targetIsAimCam ? POWER_BUTTON_AIM_CAM_POSITION : POWER_BUTTON_DEFAULT_POSITION;
        isCurrentlyAtAimCamPosition = targetIsAimCam;

        while (timePassed <= time)
        {
            float lerpVal = timePassed / time;
            SpinButtonRect.anchoredPosition3D = new Vector3(Mathf.Lerp(spinStartX, spinEndX, lerpVal), 
                SpinButtonRect.anchoredPosition3D.y, SpinButtonRect.anchoredPosition3D.z);
            AngleButtonRect.anchoredPosition3D = new Vector3(Mathf.Lerp(angleStartX, angleEndX, lerpVal), 
                AngleButtonRect.anchoredPosition3D.y, AngleButtonRect.anchoredPosition3D.z);
            PowerButtonRect.anchoredPosition3D = new Vector3(Mathf.Lerp(powerStartX, powerEndX, lerpVal), 
                PowerButtonRect.anchoredPosition3D.y, PowerButtonRect.anchoredPosition3D.z);
            
            timePassed += Time.deltaTime;
            yield return null;
        }

        movingButtonsCoroutine = null;
    }
}
