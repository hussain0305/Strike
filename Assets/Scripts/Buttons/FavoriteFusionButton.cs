using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FavoriteFusionButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI label;

    private string fusionKey;
    private Action<string> onSelected;

    public void Initialize(string key, Action<string> onSelectedCallback)
    {
        Cleanup();
        fusionKey  = key;
        onSelected = onSelectedCallback;
        label.text = FormatLabel(key);
        button.onClick.AddListener(HandleClick);
    }

    private void HandleClick() => onSelected?.Invoke(fusionKey);

    private string FormatLabel(string key)
    {
        var parts = key.Split('+');
        if (parts.Length == 2)
            return $"{Balls.Instance.GetBall(parts[0]).name.ToUpper()} - {Balls.Instance.GetBall(parts[1]).name.ToUpper()}";
        return key.ToUpper();
    }

    public void Cleanup()
    {
        button.onClick.RemoveAllListeners();
    }
}