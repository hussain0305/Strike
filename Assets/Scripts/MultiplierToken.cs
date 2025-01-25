using TMPro;

public class MultiplierToken : Collectible
{
    public MultiplierTokenType multiplierTokenType;
    public TextMeshProUGUI multipleText;

    public new void Start()
    {
        base.Start();
        multipleText.text = $"{value}x";
    }
}
