using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FusionBallButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button button;

    private string ballID;
    private Action<string> onChosen;
    [HideInInspector]
    public BallProperties ballProperties;

    public void Initialize(BallProperties props, Action<string> onChosenCallback)
    {
        ballID = props.id;
        ballProperties = props;
        nameText.text = props.name;
        onChosen = onChosenCallback;

        button.onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        onChosen?.Invoke(ballID);
    }

    public void Cleanup()
    {
        button.onClick.RemoveListener(HandleClick);
    }
}