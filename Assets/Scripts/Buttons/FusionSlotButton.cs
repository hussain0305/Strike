using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FusionSlotButton : MonoBehaviour
{
    public bool isPrimary;
    public Button button;
    public GameObject selectedSection;
    public GameObject unselectedSection;
    public TextMeshProUGUI buttonTitle;
    public BallStatRow WeightStatRow;
    public BallStatRow spinStatRow;
    public BallStatRow bounceStatRow;

    private string ballID;
    
    public void SetSelected(string id)
    {
        ballID = id;
        selectedSection.SetActive(true);
        unselectedSection.SetActive(false);
        
        BallProperties properties = Balls.Instance.GetBall(ballID);
        
        WeightStatRow.SetWeight(properties.weight);
        spinStatRow.SetSpin(properties.spin);
        bounceStatRow.SetBounce(properties.physicsMaterial.bounciness);
        buttonTitle.text = properties.name.ToUpper();
    }

    public void SetUnselected()
    {
        selectedSection.SetActive(false);
        unselectedSection.SetActive(true);
        ballID = null;
        buttonTitle.text = isPrimary ? "Primary" : "Secondary";
    }

    public void SetInteractable(bool interactable)
    {
        if (!interactable)
        {
            //A non-interactable button will always be in the "Empty" state
            SetUnselected();
        }
        button.interactable = interactable;
    }
}
