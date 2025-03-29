using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallSelectedEvent
{
    public int Index;
    public string ID;
    public BallSelectedEvent(int index, string id)
    {
        Index = index; 
        ID = id;
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

    private string ballID;
    public string BallID => ballID;
    
    private ButtonFeedback buttonBehaviour;
    public ButtonFeedback ButtonBehaviour
    {
        get
        {
            if (buttonBehaviour == null)
            {
                buttonBehaviour = GetComponentInChildren<ButtonFeedback>();
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
        EventBus.Publish(new BallSelectedEvent(BallIndex, BallID));
    }
    
    public void SetSelected()
    {
        ButtonBehaviour.SetSelected();
        ballNameText.color = selectedTextColor;
    }

    public void SetUnselected()
    {
        ButtonBehaviour.SetUnselected();
        ballNameText.color = unselectedTextColor;
    }

    public void SetBallID(string id)
    {
        ballID = id;
    }
}
