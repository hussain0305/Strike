using System.Linq;
using UnityEngine.UI;
using UnityEngine;

[System.Serializable]
public class TutorialStep
{
    public string name;
    public string introText;
    public string successfulText = "Completed";
    public TutorialHUD.TutorialScreenElements[] elementsToActivate;

    protected TutorialController controller;
    protected Button targetButton;

    public virtual void Begin(TutorialController tutorialController)
    {
        controller = tutorialController;

        controller.tutorialHUD.SetUIElementsActive(elementsToActivate.ToList());
        controller.tutorialHUD.SetInstructionText(introText);
    }

    public virtual void Reset() { }
    
    public virtual void TargetButtonClicked()
    {
        targetButton.onClick.RemoveListener(TargetButtonClicked);
    }
}
