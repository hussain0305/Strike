using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalAssets", menuName = "Game/GlobalAssets", order = 1)]
public class GlobalAssets : ScriptableObject
{
    private static GlobalAssets _instance;
    public static GlobalAssets Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GlobalAssets>("GlobalAssets");
                if (_instance == null)
                {
                    Debug.LogError("GlobalAssets instance not found. Please create one in the Resources folder.");
                }
            }
            return _instance;
        }
    }

    [Header("Collectibles Appearance")]
    public Material positiveCollectibleMaterial;
    public Material negativeCollectibleMaterial;
    public Material positiveCollectibleHitMaterial;
    public Material negativeCollectibleHitMaterial;
    
    [Header("Collectibles Font")]
    public Material multiplierCollectibleTextMaterial;
    public Material positiveCollectibleTextMaterial;
    public Material negativeCollectibleTextMaterial;
    public Material positiveCollectibleHitTextMaterial;
    public Material negativeCollectibleHitTextMaterial;

    [Header("Materials")]
    public ButtonMaterials[] defaultMaterials;
    public ButtonMaterials[] selectedMaterials;
    public ButtonMaterials[] lockedMaterials;
    public ButtonMaterials[] hoverMaterials;
    public Material flatHitEffectMaterial;

    [Header("Rarity")]
    public RarityAppearance[] rarityAppearance;

    private Dictionary<ButtonLocation, Material> lockedMaterialsDictionary;
    private Dictionary<ButtonLocation, Material> defaultMaterialsDictionary;
    private Dictionary<ButtonLocation, Material> selectedMaterialsDictionary;
    private Dictionary<ButtonLocation, Material> hoverMaterialsDictionary;
    
    public Material GetDefaultMaterial(ButtonLocation buttonLocation)
    {
        if (defaultMaterialsDictionary == null)
        {
            PrepareDictionaries();
        }

        return defaultMaterialsDictionary[buttonLocation];
    }

    public Material GetSelectedMaterial(ButtonLocation buttonLocation)
    {
        if (selectedMaterialsDictionary == null)
        {
            PrepareDictionaries();
        }
        return selectedMaterialsDictionary[buttonLocation];
    }

    public Material GetLockedMaterial(ButtonLocation buttonLocation)
    {
        if (lockedMaterialsDictionary == null)
        {
            PrepareDictionaries();
        }

        return lockedMaterialsDictionary[buttonLocation];
    }
    
    public Material GetHoverMaterial(ButtonLocation buttonLocation)
    {
        if (hoverMaterialsDictionary == null)
        {
            PrepareDictionaries();
        }

        return hoverMaterialsDictionary[buttonLocation];
    }

    public void PrepareDictionaries()
    {
        defaultMaterialsDictionary = new Dictionary<ButtonLocation, Material>();
        foreach (ButtonMaterials buttMat in defaultMaterials)
        {
            defaultMaterialsDictionary.Add(buttMat.buttonLocation, buttMat.material);
        }

        selectedMaterialsDictionary = new Dictionary<ButtonLocation, Material>();
        foreach (ButtonMaterials buttMat in selectedMaterials)
        {
            selectedMaterialsDictionary.Add(buttMat.buttonLocation, buttMat.material);
        }
        
        lockedMaterialsDictionary = new Dictionary<ButtonLocation, Material>();
        foreach (ButtonMaterials buttMat in lockedMaterials)
        {
            lockedMaterialsDictionary.Add(buttMat.buttonLocation, buttMat.material);
        }
    
        hoverMaterialsDictionary = new Dictionary<ButtonLocation, Material>();
        foreach (ButtonMaterials buttMat in hoverMaterials)
        {
            hoverMaterialsDictionary.Add(buttMat.buttonLocation, buttMat.material);
        }
    }

    public RarityAppearance GetRarityAppearanceSettings(Rarity rarity)
    {
        foreach (RarityAppearance appearance in rarityAppearance)
        {
            if (appearance.rarity == rarity)
            {
                return appearance;
            }
        }

        return rarityAppearance[0];
    }
}