using System;
using System.Linq;
using UnityEngine;

public class Step_ToggleTrajectory : TutorialStep
{
    private enum Phase { WaitForFirstTap, WaitForTargetButtonClick, WaitForConfirmation }
    private Phase phase = Phase.WaitForFirstTap;
    
    public override void Begin(TutorialController tutorialController)
    {
        name = "TRAJECTORY";
        successfulText = "Excellent. Now let's try adding some power and see the trajectory of your shot.";

        elementsToActivate = Array.Empty<TutorialHUD.TutorialScreenElements>();
        base.Begin(tutorialController);

        phase = Phase.WaitForFirstTap;

        controller.tutorialHUD.SetInstructionText("Every two seconds—and any time you tweak a shot parameter—an indicator shows where the ball would land on an unobstructed trajectory",
            "Tap anywhere to continue.");
        controller.StartCoroutine(WaitForScreenTap(0.2f));
    }

    public override void Reset()
    {
        targetButton?.onClick.RemoveListener(TargetButtonClicked);
    }

    public override void TargetButtonClicked()
    {
        base.TargetButtonClicked();

        if (phase != Phase.WaitForTargetButtonClick)
            return;
        
        phase = Phase.WaitForConfirmation;

        controller.tutorialHUD.BallParameterController.powerInput.OverridePower(20);
        controller.tutorialHUD.trajectoryButtonContainer.SetActive(false);
        controller.tutorialHUD.SetInstructionText(successfulText,"Tap anywhere to continue.");
        controller.StartCoroutine(WaitForScreenTap());
    }
    
    private System.Collections.IEnumerator WaitForScreenTap(float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        while (!Input.GetMouseButtonDown(0) && Input.touchCount == 0)
        {
            yield return null;
        }

        if (phase == Phase.WaitForFirstTap)
        {
            Debug.Log(">>> dsgfhgf");
            elementsToActivate = new[]
            {
                TutorialHUD.TutorialScreenElements.TrajectoryButton,
                TutorialHUD.TutorialScreenElements.TrajectoryButtonContainer,
            };
            controller.tutorialHUD.SetUIElementsActive(elementsToActivate.ToList());
            controller.tutorialHUD.SetInstructionText(introText);
            targetButton = controller.tutorialHUD.trajectoryButton.GetComponentInChildren<UnityEngine.UI.Button>(true);
            targetButton.onClick.AddListener(TargetButtonClicked);
            phase = Phase.WaitForTargetButtonClick;
            controller.tutorialHUD.SetInstructionText("But let's try something better.\nTap the bottom-right Trajectory button to view your shot’s FULL PATH—you get three checks per level.");
            yield break;
        }

        controller.tutorialHUD.SetInstructionText("");
        EventBus.Publish(new TutorialStepCompletedEvent());
    }
}
