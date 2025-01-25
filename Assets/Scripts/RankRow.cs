using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankRow : MonoBehaviour
{
    public TextMeshProUGUI rank;
    public new TextMeshProUGUI name;
    public TextMeshProUGUI points;

    public void SetInfo(int _rank, string _name, int _points)
    {
        rank.text = _rank.ToString();
        name.text = _name;
        points.text = _points.ToString();
    }
}
