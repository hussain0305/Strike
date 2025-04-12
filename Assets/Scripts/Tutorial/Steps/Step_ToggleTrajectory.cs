using UnityEngine;

public class Step_ToggleTrajectory : TutorialStep
{
    private enum Phase { WaitForClick, WaitForConfirmation }
    private Phase phase = Phase.WaitForClick;
    
    public override void Begin(TutorialController tutorialController)
    {
        name = "TRAJECTORY";
        elementsToActivate = new[]
        {
            TutorialHUD.TutorialScreenElements.TrajectoryButton,
            TutorialHUD.TutorialScreenElements.TrajectoryButtonContainer,
        };
        successfulText = "Excellent. Now let's try adding some power and see the trajectory of your shot.";

        base.Begin(tutorialController);

        phase = Phase.WaitForClick;

        controller.tutorialHUD.SetInstructionText("Tap the Trajectory button on the bottom right of your screen to see the trajectory of your shot");

        targetButton = controller.tutorialHUD.trajectoryButton.GetComponentInChildren<UnityEngine.UI.Button>(true);
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

        controller.tutorialHUD.BallParameterController.powerInput.OverridePower(20);
        controller.tutorialHUD.trajectoryButtonContainer.SetActive(false);
        controller.tutorialHUD.SetInstructionText(successfulText,"Tap anywhere to continue.");
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
