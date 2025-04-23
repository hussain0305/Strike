using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallStatRow : MonoBehaviour
{
    [System.Serializable]
    public enum BallProperty
    {
        name,
        weight,
        spin,
        bounce
    };

    public BallProperty propertyType;

    public TextMeshProUGUI propertyTypeText;
    public Slider propertySlider;
    public TextMeshProUGUI propertyValueText;

    public void SetWeight(float value)
    {
        propertyTypeText.text = "WEIGHT";
        propertySlider.value = Mathf.Clamp01(value / Balls.Instance.maxWeight);
        propertyValueText.text = value.ToString("F1");
    }
    
    public void SetWeight(float value, float maxValue)
    {
        propertyTypeText.text = "WEIGHT";
        propertySlider.value = Mathf.Clamp01(value / maxValue);
        propertyValueText.text = value.ToString("F1");
    }
    
    public void SetSpin(float value)
    {
        propertyTypeText.text = "SPIN";
        propertySlider.value = Mathf.Clamp01(value / Balls.Instance.maxSpin);
        propertyValueText.text = value.ToString("F1");
    }
    
    public void SetSpin(float value, float maxValue)
    {
        propertyTypeText.text = "SPIN";
        propertySlider.value = Mathf.Clamp01(value / maxValue);
        propertyValueText.text = value.ToString("F1");
    }
    
    public void SetBounce(float value)
    {
        propertyTypeText.text = "BOUNCE";
        propertySlider.value = Mathf.Clamp01(value / Balls.Instance.maxBounce);
        propertyValueText.text = value.ToString("F2");
    }
    
    public void SetBounce(float value, float maxValue)
    {
        propertyTypeText.text = "BOUNCE";
        propertySlider.value = Mathf.Clamp01(value / maxValue);
        propertyValueText.text = value.ToString("F2");
    }
    
}