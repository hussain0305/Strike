using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public struct TabEntry
{
    public Image buttonOutline;
    public Button button;
    public GameObject[] pages;
    public Material outlineSelectedMaterial;
}

public class TabToggleManager : MonoBehaviour
{
    [Header("Tab Setup")]
    public TabEntry[] tabs;
    public Image underline;
    
    [Header("Visual Styles")]
    public float selectedSize = 1.1f;
    public float unSelectedSize = 0.95f;
    public Material outlineUnselectedMaterial;
    public Material fontSelectedMaterial;
    public Material fontUnselectedMaterial;

    private int _selectedIndex = -1;

    private void Start()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i;
            tabs[i].button.onClick.AddListener(() => SelectTab(index));
        }

        if (tabs.Length > 0)
            SelectTab(0);
    }

    private void SelectTab(int index)
    {
        if (index < 0 || index >= tabs.Length || index == _selectedIndex)
            return;

        _selectedIndex = index;
        UpdateTabs();
    }

    private void UpdateTabs()
    {
        underline.material = tabs[_selectedIndex].outlineSelectedMaterial;
        for (int i = 0; i < tabs.Length; i++)
        {
            bool isSelected = (i == _selectedIndex);

            foreach (var page in tabs[i].pages)
            {
                page.SetActive(isSelected);
            }
            tabs[i].button.transform.parent.localScale = (isSelected ? selectedSize : unSelectedSize) * Vector3.one;

            
            if (tabs[i].buttonOutline != null)
            {
                tabs[i].buttonOutline.material = isSelected
                    ? tabs[i].outlineSelectedMaterial
                    : outlineUnselectedMaterial;
            }

            var label = tabs[i].button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.fontSharedMaterial = isSelected
                    ? fontSelectedMaterial
                    : fontUnselectedMaterial;
            }
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].button.onClick.RemoveAllListeners();
        }
    }
}
