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

    [Header("Materials")]
    public ButtonMaterials[] defaultMaterials;
    public ButtonMaterials[] selectedMaterials;
    public Material flatHitEffectMaterial;
    
    private Dictionary<ButtonLocation, Material> defaultMaterialsDictionary;
    private Dictionary<ButtonLocation, Material> selectedMaterialsDictionary;
    
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
    }
}