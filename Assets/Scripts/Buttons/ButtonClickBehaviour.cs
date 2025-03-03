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
    public bool playsHoverSound = true;
    public bool playsClickSound = true;
    public bool overrideHoverSound = false;
    public AudioClip hoverClipOverride;
    public bool overrideClickSound = false;
    public AudioClip clickClipOverride;
    [HideInInspector]
    public bool isEnabled = true;

    private SoundLibrary soundLibrary => AudioManager.Instance?.soundLibrary;

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
        
        PlaySound(playsClickSound, overrideClickSound, clickClipOverride, 
            soundLibrary?.buttonClickSFX);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isEnabled && !ButtonStateManager.Instance.IsButtonSelected(this))
        {
            SetToHover();
        }

        PlaySound(playsHoverSound, overrideHoverSound, hoverClipOverride, 
            soundLibrary?.buttonHoverSFX);
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
    
    private void PlaySound(bool shouldPlay, bool overrideClip, AudioClip clip, AudioClip defaultSound)
    {
        if (!shouldPlay) return;
    
        if (overrideClip)
        {
            AudioManager.Instance.PlaySFX(clip, false);
        }
        else if(defaultSound != null)
        {
            AudioManager.Instance.PlaySFX(defaultSound, false);
        }
    }
}