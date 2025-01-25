using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraToggleButton : MonoBehaviour
{
    public delegate void CameraSwitched(CameraHoistLocation newCameraPos);
    public static event CameraSwitched OnCameraSwitched;

    public CameraHoistLocation hoistLocation;
    public Image[] outlines;
    
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(ButtonPressed);
        CameraController.OnCameraSwitchProcessed += CameraSwitchProcessed;
    }

    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
        CameraController.OnCameraSwitchProcessed -= CameraSwitchProcessed;
    }

    public void ButtonPressed()
    {
        if (!GameManager.BallShootable)
        {
            return;
        }
        OnCameraSwitched?.Invoke(hoistLocation);
    }

    public void CameraSwitchProcessed(CameraHoistLocation newCameraHoistLocation)
    {
        foreach (Image outline in outlines)
        {
            outline.material = newCameraHoistLocation == hoistLocation
                ? GlobalAssets.Instance.GetSelectedMaterial(ButtonLocation.GameHUD)
                : GlobalAssets.Instance.GetDefaultMaterial(ButtonLocation.GameHUD);
        }
    }
}
