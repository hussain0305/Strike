using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrajectoryLabel : MonoBehaviour
{
    public Image identifier;
    public Image background;
    public TextMeshProUGUI angleText;
    public TextMeshProUGUI spinText;
    public TextMeshProUGUI powerText;
    public TextMeshProUGUI pointsText;

    public void SetInfo(Vector2 _angle, Vector2 _spin, int _power, int _points, Material _identifier, Color backgroundColor)
    {
        background.color = backgroundColor;
        identifier.material = _identifier;
        angleText.text = $"{_angle.x * 100:F0}, {_angle.y * 100:F0}";
        spinText.text = $"{_spin.x * 100:F0}, {_spin.y * 100:F0}";
        powerText.text = _power.ToString();
        pointsText.text = _points.ToString();
    }
}
