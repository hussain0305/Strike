using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

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
    public Sound hoverClipOverride;
    public bool overrideClickSound = false;
    public Sound clickClipOverride;
    public bool overrideNormalTextColor;
    public Color normalTextColor;
    
    [Header("Hover Pop Animation")]
    public Transform popTarget;
    public float popScale = 1.1f;
    public float popDuration = 0.15f;
    private Coroutine popRoutine;

    [HideInInspector]
    public bool isEnabled = true;
    [HideInInspector]
    public bool isSelected = false;

    private SoundLibrary soundLibrary => audioManager?.soundLibrary;

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

    private Button button;
    private Button Button
    {
        get
        {
            if (button == null)
                button = GetComponent<Button>();
            if (button == null)
                button = GetComponentInParent<Button>();
            
            return button;
        }
    }
    
    private Color highlightedTextColor = new Color(1f, 0.5f, 0.7f); 
    private Color defaultTextColor = Color.white;
    
    private AudioManager audioManager;
    
    [Inject]
    public void Construct(AudioManager _audioManager)
    {
        audioManager = _audioManager;
    }

    private void Awake()
    {
        if (popTarget == null)
        {
            popTarget = transform.parent;
        }
    }

    private void Start()
    {
        defaultTextColor = overrideNormalTextColor ? normalTextColor : Color.white;
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
        if (!Button.enabled)
            return;

        if (!isEnabled)
            return;

        if (staysSelected)
            SetSelected();
        else
            SetToHighlighted();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!Button.enabled)
            return;

        if (!staysSelected)
            SetToDefault();
        
        PlayClickSound();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Button.enabled)
            return;

        if (isEnabled && !isSelected)
            SetToHover();

        PlayHoverSound();
        
        if (popTarget != null)
        {
            if (popRoutine != null)
                StopCoroutine(popRoutine);
            popRoutine = StartCoroutine(PlayHoverPop());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Button.enabled)
            return;
        
        if (isEnabled && (!staysSelected || !isSelected))
            SetToDefault();
    }

    public void SetToDefault()
    {
        SetTextComponentColor(defaultTextColor);
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
    
    private void PlayClickSound()
    {
        if (!playsClickSound || audioManager == null)
            return;

        audioManager.PlaySFX(overrideClickSound ? clickClipOverride : soundLibrary.buttonClickSFX);
    }

    private void PlayHoverSound()
    {
        if (!playsHoverSound || audioManager == null)
            return;

        audioManager.PlaySFX(overrideHoverSound ? hoverClipOverride : soundLibrary.buttonHoverSFX);
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