using UnityEngine;

public class Step_Fire : TutorialStep
{
    private enum Phase { WaitForClick, WaitForConfirmation }
    private Phase phase = Phase.WaitForClick;
    
    public override void Begin(TutorialController tutorialController)
    {
        name = "SHOOT";
        elementsToActivate = new[]
        {
            TutorialHUD.TutorialScreenElements.AngleButton,
            TutorialHUD.TutorialScreenElements.SpinButton,
            TutorialHUD.TutorialScreenElements.PowerButton,
            TutorialHUD.TutorialScreenElements.FireButton,
        };
        successfulText = "Well Done. You are now ready to venture into the game.";

        base.Begin(tutorialController);

        phase = Phase.WaitForClick;

        controller.tutorialHUD.SetInstructionText("Make any final tweaks and hit Shoot");

        targetButton = controller.tutorialHUD.fireButton;
        targetButton.onClick.AddListener(TargetButtonClicked);
    }

    public override void Reset()
    {
        targetButton?.onClick.RemoveListener(TargetButtonClicked);
    }

    public override void TargetButtonClicked()
    {
        base.TargetButtonClicked();

        if (phase != Phase.WaitForClick)
            return;
        
        phase = Phase.WaitForConfirmation;

        controller.tutorialHUD.SetInstructionText(successfulText, "Tap anywhere to continue.");
        controller.StartCoroutine(WaitForScreenTap());
    }
    
    private System.Collections.IEnumerator WaitForScreenTap()
    {
        while (!Input.GetMouseButtonDown(0) && Input.touchCount == 0)
        {
            yield return null;
        }

        controller.tutorialHUD.SetInstructionText("");
        EventBus.Publish(new TutorialStepCompletedEvent());
    }
}
