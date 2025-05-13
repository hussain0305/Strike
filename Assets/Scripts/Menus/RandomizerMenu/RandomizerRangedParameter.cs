using System;
using TMPro;
using UnityEngine;

public class RandomizerRangedParameter : MonoBehaviour, IRandomizerParameter
{
    public TextMeshProUGUI text;
    public RandomizerParameterType id;
    public RandomizerParameterType Id => id;
    public int min = 1;
    public int max = 5;
    public int step = 1;

    private int value;
    public int ValueInt => value;

    object IRandomizerParameter.Value => value;

    public event Action<object> OnValueChanged;
    
    public void Increment()
    {
        if (value + step <= max)
        {
            value += step;
            text.text = value.ToString();
            OnValueChanged?.Invoke(value);
        }
    }

    public void Decrement()
    {
        if (value - step >= min)
        {
            value -= step;
            text.text = value.ToString();
            OnValueChanged?.Invoke(value);
        }
    }
}
