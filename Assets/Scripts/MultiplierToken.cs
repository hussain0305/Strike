using TMPro;
using UnityEngine;

public class MultiplierToken : Collectible
{
    [Header("Multiplier Section")]
    public MultiplierTokenType multiplierTokenType;
    public TextMeshPro multipleText;

    public void Start()
    {
        base.Start();
        multipleText.text = $"{value}x";
    }
}
