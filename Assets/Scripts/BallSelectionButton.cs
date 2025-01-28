using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallSelectionButton : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI ballNameText;
    public Image[] outlines;
    
    private int ballIndex;
    public int BallIndex => ballIndex;
    
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
        SetSelected();
        BallSelectionPage.Instance.SetSelectedBall(BallIndex);
    }

    public void SetBorderMaterial(Material _mat)
    {
        foreach (Image outline in outlines)
        {
            outline.material = _mat;
        }
    }

    public void SetSelected()
    {
        SetBorderMaterial(GlobalAssets.Instance.GetSelectedMaterial(ButtonLocation.MainMenu));
    }

    public void SetUnselected()
    {
        SetBorderMaterial(GlobalAssets.Instance.GetDefaultMaterial(ButtonLocation.MainMenu));
    }
}
