using TMPro;

public class MultiplierToken : Collectible
{
    public MultiplierTokenType multiplierTokenType;
    public TextMeshProUGUI multipleText;

    public void Start()
    {
        multipleText.text = $"{value}x";
    }
}
