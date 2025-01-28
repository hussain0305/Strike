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
}