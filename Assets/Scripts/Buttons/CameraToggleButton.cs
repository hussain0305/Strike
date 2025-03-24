using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraSwitchedEvent
{
    public CameraHoistLocation NewCameraPos { get; }

    public CameraSwitchedEvent(CameraHoistLocation newCameraPos)
    {
        NewCameraPos = newCameraPos;
    }
}

public class CameraToggleButton : MonoBehaviour
{
    public CameraHoistLocation hoistLocation;
    public Image outline;
    
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(ButtonPressed);
        EventBus.Subscribe<CameraSwitchProcessedEvent>(CameraSwitchProcessed);
    }

    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
        EventBus.Unsubscribe<CameraSwitchProcessedEvent>(CameraSwitchProcessed);
    }

    public void ButtonPressed()
    {
        if (!GameManager.BallShootable)
        {
            return;
        }
        EventBus.Publish(new CameraSwitchedEvent(hoistLocation));
    }

    public void CameraSwitchProcessed(CameraSwitchProcessedEvent e)
    {
        outline.material = e.NewCameraPos == hoistLocation
            ? GlobalAssets.Instance.GetSelectedMaterial(ButtonLocation.GameHUD)
            : GlobalAssets.Instance.GetDefaultMaterial(ButtonLocation.GameHUD);
    }
}
