using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallParameterInputSwitchedEvent
{
    public BallParameterController.ShotParameter ShotParameter;
    public BallParameterInputSwitchedEvent(BallParameterController.ShotParameter shotParameter)
    {
        ShotParameter = shotParameter;
    }
}

public class BallParameterController : MonoBehaviour
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
    public TextMeshProUGUI angleTextNormal;
    public TextMeshProUGUI angleTextGlowy;
    public TextMeshProUGUI powerTextNormal;
    public TextMeshProUGUI powerTextGlowy;
    
    public SpinInput spinInput;
    public AngleInput angleInput;
    public PowerInput powerInput;

    private void OnEnable()
    {
        EventBus.Subscribe<NextShotCuedEvent>(NextShotCued);
        EventBus.Subscribe<BallParameterInputSwitchedEvent>(ShotParameterInputSwitched);
        
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
        EventBus.Unsubscribe<BallParameterInputSwitchedEvent>(ShotParameterInputSwitched);
        EventBus.Unsubscribe<NextShotCuedEvent>(NextShotCued);
        
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

    private void ShotParameterInputSwitched(BallParameterInputSwitchedEvent e)
    {
        currentParameterInput = e.ShotParameter;
    }

    public void NextShotCued(NextShotCuedEvent e)
    {
        currentParameterInput = ShotParameter.None;
        spinTextNormal.text = "Spin";
        spinTextGlowy.text = "Spin";
        
        angleTextNormal.text = "Angle";
        angleTextGlowy.text = "Angle";
        
        powerTextNormal.text = "Power";
        powerTextGlowy.text = "Power";
        
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
        EventBus.Publish(new BallParameterInputSwitchedEvent(shotParameter));
        switch (shotParameter)
        {
            case ShotParameter.Spin:
                spinTextNormal.gameObject.SetActive(false);
                spinTextGlowy.gameObject.SetActive(true);
                break;
            case ShotParameter.Angle:
                angleTextNormal.gameObject.SetActive(false);
                angleTextGlowy.gameObject.SetActive(true);
                break;
            case ShotParameter.Power:
                powerTextNormal.gameObject.SetActive(false);
                powerTextGlowy.gameObject.SetActive(true);
                break;
        }
    }
}
