using UnityEngine;

public class RandomizerBoolParameter : MonoBehaviour, IRandomizerParameter
{
    public RandomizerParameterType id;
    public RandomizerParameterType Id => id;

    private bool value;

    object IRandomizerParameter.Value => value;

    public event System.Action<object> OnValueChanged;
    
    public void Set(bool state)
    {
        if (value != state)
        {
            value = state;
            OnValueChanged?.Invoke(value);
        }
    }
}
