using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
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

public class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ButtonGroup groupId = ButtonGroup.Default;
    public ButtonLocation buttonLocation;
    public Image outline;
    public bool backToDefaultOnEnable = true;
    public bool staysSelected = false;
    public bool playsHoverSound = true;
    public bool playsClickSound = true;
    public bool overrideHoverSound = false;
    public AudioClip hoverClipOverride;
    public bool overrideClickSound = false;
    public AudioClip clickClipOverride;
    
    [Header("Hover Pop Animation")]
    public Transform popTarget;
    public float popScale = 1.1f;
    public float popDuration = 0.15f;
    private Coroutine popRoutine;

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
    
    private void Awake()
    {
        if (popTarget == null)
        {
            popTarget = transform.parent;
        }
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
        if (isEnabled && !isSelected)
        {
            SetToHover();
        }

        PlaySound(playsHoverSound, overrideHoverSound, hoverClipOverride, 
            soundLibrary?.buttonHoverSFX);
        
        if (popTarget != null)
        {
            if (popRoutine != null)
                StopCoroutine(popRoutine);
            popRoutine = StartCoroutine(PlayHoverPop());
        }
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
        
        popTarget.localScale = Vector3.one;
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
        if (!shouldPlay || AudioManager.Instance == null) return;

        AudioClip chosenClip = overrideClip ? clip : defaultSound;
        if (chosenClip == null) return;

        // Slight pitch variation for hover (not for click)
        float pitch = 1f;
        if (chosenClip == soundLibrary?.buttonHoverSFX)
        {
            pitch = UnityEngine.Random.Range(0.95f, 1.05f);
        }

        AudioSource source = AudioManager.Instance.GetAvailableSFXSource();
        source.pitch = pitch;
        source.PlayOneShot(chosenClip);
        StartCoroutine(AudioManager.Instance.ResetPitchAfter(chosenClip.length, source));
    }

    public void SetMaterial(Material _mat)
    {
        outline.material = _mat;
    }

    public void SetTextComponentColor(Color col)
    {
        if(TextComponent) TextComponent.color = col;
    }
    
    private IEnumerator PlayHoverPop()
    {
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = originalScale * popScale;
        float time = 0f;

        while (time < popDuration / 2f)
        {
            popTarget.localScale = Vector3.Lerp(originalScale, targetScale, time / (popDuration / 2f));
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        popTarget.localScale = targetScale;
        time = 0f;

        while (time < popDuration / 2f)
        {
            popTarget.localScale = Vector3.Lerp(targetScale, originalScale, time / (popDuration / 2f));
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        popTarget.localScale = originalScale;
    }
}