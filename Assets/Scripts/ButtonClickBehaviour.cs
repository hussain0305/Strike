using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonClickBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public ButtonLocation buttonLocation;
    public Image[] boundaries;
    public bool backToDefaultOnEnable = true;
    
    private void OnEnable()
    {
        if (backToDefaultOnEnable)
        {
            SetMaterial(GlobalAssets.Instance.GetDefaultMaterial(buttonLocation));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetMaterial(GlobalAssets.Instance.GetSelectedMaterial(buttonLocation));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SetMaterial(GlobalAssets.Instance.GetDefaultMaterial(buttonLocation));
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
        SetMaterial(GlobalAssets.Instance.GetDefaultMaterial(buttonLocation));
    }

    public void SetToHighlighted()
    {
        SetMaterial(GlobalAssets.Instance.GetSelectedMaterial(buttonLocation));
    }
}
