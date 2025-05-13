using UnityEngine;
using UnityEngine.UI;

public class RandomizerParameterToggle : MonoBehaviour
{
    private AbyssTogglerVisuals visuals;
    private AbyssTogglerVisuals Visuals
    {
        get
        {
            if (visuals == null)
            {
                visuals = GetComponentInParent<AbyssTogglerVisuals>();
            }
            return visuals;
        }
    }
    
    private Button button;
    private Button Button
    {
        get
        {
            if (button == null)
                button = GetComponent<Button>();
            
            return button;
        }    
    }
    
    private RandomizerBoolParameter parameter;
    private RandomizerBoolParameter Parameter
    {
        get
        {
            if(parameter == null)
                parameter = GetComponentInParent<RandomizerBoolParameter>();
            return parameter;
        }
    }

    private bool val;
    
    private void OnEnable()
    {
        val = false;
        Button.onClick.AddListener(Clicked);
    }

    private void OnDisable()
    {
        Button.onClick.RemoveAllListeners();
    }

    public void Clicked()
    {
        val = !val;
        Visuals.SetValue(val);
        Parameter?.Set(val);
    }
}
