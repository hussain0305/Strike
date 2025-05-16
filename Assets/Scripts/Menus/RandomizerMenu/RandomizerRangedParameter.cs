using System;
using TMPro;
using UnityEngine;

public class RandomizerRangedParameter : MonoBehaviour, IRandomizerParameter
{
    public TextMeshProUGUI text;
    public RandomizerParameterType id;
    public RandomizerParameterType Id => id;
    protected int min;
    protected int max;

    protected int value;
    public int ValueInt => value;

    object IRandomizerParameter.Value => value;

    public event Action<object> OnValueChanged;

    private void Awake()
    {
        min = 1;
        max = 10;
    }

    public virtual void Increment()
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
