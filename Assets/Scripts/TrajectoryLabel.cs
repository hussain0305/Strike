using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrajectoryLabel : MonoBehaviour
{
    public Image identifier;
    public TextMeshProUGUI angleText;
    public TextMeshProUGUI spinText;
    public TextMeshProUGUI powerText;
    public TextMeshProUGUI pointsText;

    public void SetInfo(Vector2 _angle, Vector2 _spin, int _power, int _points, Material _identifier)
    {
        identifier.material = _identifier;
        angleText.text = $"{_angle.x:F2}, {_angle.y:F2}";
        spinText.text = $"{_spin.x:F2}, {_spin.y:F2}";
        powerText.text = _power.ToString();
        pointsText.text = _points.ToString();
    }
}
