using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonClickedEvent
{
    public int Index;
    public ButtonGroup ButtonGroup;
    public ButtonClickedEvent(int _index, ButtonGroup _buttonGroup)
    {
        Index = _index;
        ButtonGroup = _buttonGroup;
    }
}

public class ButtonClickBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
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
    [HideInInspector]
    public bool isSelected = false;

    private SoundLibrary soundLibrary => AudioManager.Instance?.soundLibrary;

    private TextMeshProUGUI textComponent;
    private TextMeshProUGUI TextComponent
    {
        get
        {
            if (textComponent == null)
            {
                textComponent = GetComponentInChildren<TextMeshProUGUI>();
            }
            return textComponent;
        }
    }

    Color highlightedTextColor = new Color(1f, 0.5f, 0.7f); 
    
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
        if (isEnabled && !isSelected)
        {
            SetToHover();
        }

        PlaySound(playsHoverSound, overrideHoverSound, hoverClipOverride, 
            soundLibrary?.buttonHoverSFX);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isEnabled && (!staysSelected || !isSelected))
        {
            SetToDefault();
        }
    }

    public void SetToDefault()
    {
        SetTextComponentColor(Color.white);
        SetMaterial(isEnabled 
            ? GlobalAssets.Instance.GetDefaultMaterial(buttonLocation) 
            : GlobalAssets.Instance.GetLockedMaterial(buttonLocation));
    }

    public void SetToHighlighted()
    {
        SetTextComponentColor(Color.yellow);
        SetMaterial(GlobalAssets.Instance.GetSelectedMaterial(buttonLocation));
    }
    
    public void SetToHover()
    {
        SetTextComponentColor(highlightedTextColor);
        SetMaterial(GlobalAssets.Instance.GetHoverMaterial(buttonLocation));
    }

    public void SetSelected()
    {
        SetTextComponentColor(Color.yellow);
        SetMaterial(GlobalAssets.Instance.GetSelectedMaterial(buttonLocation));
        isSelected = staysSelected;
    }
    
    public void SetUnselected()
    {
        isSelected = false;
        SetToDefault();
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

    public void SetMaterial(Material _mat)
    {
        foreach (Image img in boundaries)
        {
            img.material = _mat;
        }
    }

    public void SetTextComponentColor(Color col)
    {
        if(TextComponent) TextComponent.color = col;
    }
}