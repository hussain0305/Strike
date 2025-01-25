using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerInput : MonoBehaviour
{
    public Slider powerSlider;
    public float powerMultiple = 1f;
    public TextMeshProUGUI powerText;
    
    public float Power { get; private set; }

    private void Update()
    {
        Power = powerSlider.value * powerMultiple;
        powerText.text = powerSlider.value.ToString();
    }
}