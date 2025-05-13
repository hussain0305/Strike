using System;
using UnityEngine;

public enum RandomizerParameterType
{
    Dificulty,
    HasDangerPins
}

public interface IRandomizerParameter
{
    public RandomizerParameterType Id { get; }
    public object Value { get; }
    event Action<object> OnValueChanged; 
}
