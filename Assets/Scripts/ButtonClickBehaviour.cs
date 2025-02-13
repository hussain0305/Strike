using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonClickBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [System.Serializable]
    public enum ButtonGroup
    {
        Default,
        LevelSelection,
        BallSelection,
        CameraToggle,
        CamerBehaviour
    }
    public ButtonGroup groupId = ButtonGroup.Default;
    public ButtonLocation buttonLocation;
    public Image[] boundaries;
    public bool backToDefaultOnEnable = true;
    public bool staysSelected = false;
    [HideInInspector]
    public bool isEnabled = true;

    private void Awake()
    {
        ButtonStateManager.Instance.RegisterButton(this);
    }

    private void OnEnable()
    {
        if (backToDefaultOnEnable)
        {
            SetToDefault();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isEnabled)
        {
            return;
        }
        if (staysSelected)
        {
            SetSelected();
        }
        else
        {
            SetToHighlighted();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!staysSelected)
        {
            SetToDefault();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isEnabled && !ButtonStateManager.Instance.IsButtonSelected(this))
        {
            SetToHover();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isEnabled && (!staysSelected || !ButtonStateManager.Instance.IsButtonSelected(this)))
        {
            SetToDefault();
        }
    }
    
    public void SetMaterial(Material _mat)
    {
        foreach (Image img in boundaries)
        {
            img.material = _mat;
        }
    }

    public void SetToDefault()
    {
        SetMaterial(isEnabled 
            ? GlobalAssets.Instance.GetDefaultMaterial(buttonLocation) 
            : GlobalAssets.Instance.GetLockedMaterial(buttonLocation));
    }

    public void SetToHighlighted()
    {
        SetMaterial(GlobalAssets.Instance.GetSelectedMaterial(buttonLocation));
    }
    
    public void SetToHover()
    {
        SetMaterial(GlobalAssets.Instance.GetHoverMaterial(buttonLocation));
    }

    public void SetSelected()
    {
        ButtonStateManager.Instance.SelectButton(this);
    }
}
