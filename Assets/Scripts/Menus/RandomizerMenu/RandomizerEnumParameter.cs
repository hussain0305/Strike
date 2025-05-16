using UnityEngine;

[System.Serializable]
public struct RandomizerEnum
{
    public PinBehaviourPerTurn pinBehaviour;
    public string name;
}

public class RandomizerEnumParameter : RandomizerRangedParameter, IRandomizerParameter
{
    public RandomizerEnum[] pinBehaviours;
    
    private void Awake()
    {
        min = 0;
        max = pinBehaviours.Length - 1;
        value = 0;
        UpdateText();
    }
    
    public override void Increment()
    {
        value = (value + 1) % pinBehaviours.Length;
        UpdateText();
    }

    public void UpdateText()
    {
        text.text = pinBehaviours[value].name;
    }

}
