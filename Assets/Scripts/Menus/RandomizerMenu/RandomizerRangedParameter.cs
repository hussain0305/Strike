using System;
using TMPro;
using UnityEngine;

public class RandomizerRangedParameter : MonoBehaviour, IRandomizerParameter
{
    public TextMeshProUGUI text;
    public RandomizerParameterType id;
    public RandomizerParameterType Id => id;
    private int min = 1;
    private int max = 10;

    private int value;
    public int ValueInt => value;

    object IRandomizerParameter.Value => value;

    public event Action<object> OnValueChanged;
    
    public void Increment()
    {
        if (value + 1 <= max)
        {
            value++;
            text.text = value.ToString();
            OnValueChanged?.Invoke(value);
        }
    }

    public void Decrement()
    {
        if (value - 1 >= min)
        {
            value--;
            text.text = value.ToString();
            OnValueChanged?.Invoke(value);
        }
    }
}
