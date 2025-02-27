using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallSelectedEvent
{
    public int Index;
    public BallSelectedEvent(int index)
    {
        Index = index;
    }
}

public class BallSelectionButton : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI ballNameText;
    public Image[] outlines;
    public Color selectedTextColor;
    public Color unselectedTextColor;
    
    private int ballIndex;
    public int BallIndex => ballIndex;

    private ButtonClickBehaviour buttonBehaviour;
    public ButtonClickBehaviour ButtonBehaviour
    {
        get
        {
            if (buttonBehaviour == null)
            {
                buttonBehaviour = GetComponentInChildren<ButtonClickBehaviour>();
            }
            return buttonBehaviour;
        }
    }
    
    public void SetBallName(string _text)
    {
        ballNameText.text = _text;
    }

    public void SetBallIndex(int _index)
    {
        ballIndex = _index;
    }
    
    private void OnEnable()
    {
        button.onClick.AddListener(PreviewBall);
    }

    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }

    public void PreviewBall()
    {
        EventBus.Publish(new BallSelectedEvent(BallIndex));
        BallSelectionPage.Instance.SetSelectedBall(BallIndex);
    }
    
    public void SetSelected()
    {
        ButtonBehaviour.SetSelected();
        ballNameText.color = selectedTextColor;
    }

    public void SetUnselected()
    {
        ballNameText.color = unselectedTextColor;
    }
}
