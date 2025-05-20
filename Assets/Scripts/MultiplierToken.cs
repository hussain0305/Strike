using TMPro;
using UnityEngine;

public class MultiplierToken : Collectible
{
    [Header("Multiplier Section")]
    public MultiplierTokenType multiplierTokenType;
    public TextMeshPro multipleText;

    protected override void SetupPointBoard()
    {
        multipleText.text = $"{value}x";
    }
}
