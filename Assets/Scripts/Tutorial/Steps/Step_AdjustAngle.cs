using UnityEngine;
using UnityEngine.UI;

public class Step_AdjustAngle : TutorialStep
{
    private enum Phase { WaitForClick, WaitForSwipe, WaitForConfirmation }
    private Phase phase = Phase.WaitForClick;

    private Quaternion startingAngle;

    public override void Begin(TutorialController tutorialController)
    {
        name = "ANGLE";
        elementsToActivate = new[]
        {
            TutorialHUD.TutorialScreenElements.AngleButton
        };
        successfulText = "Now let’s turn on trajectory view to preview your shot.";

        base.Begin(tutorialController);

        phase = Phase.WaitForClick;

        controller.tutorialHUD.SetInstructionText("Tap the Angle button to start aiming");

        targetButton = controller.tutorialHUD.angleButton;
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

        phase = Phase.WaitForSwipe;
        controller.tutorialHUD.SetInstructionText("Now swipe across the screen to set your Angle.");
        startingAngle = controller.tutorialHUD.BallParameterController.angleInput.cylinderPivot.rotation;
        controller.StartCoroutine(CheckSwipeCoroutine());
    }

    private System.Collections.IEnumerator CheckSwipeCoroutine()
    {
        bool angleChanged = false;
        bool fingerLifted = false;

        while (true)
        {
            var currentRotation = controller.tutorialHUD.BallParameterController.angleInput.cylinderPivot.rotation;

            if (!angleChanged && Quaternion.Angle(startingAngle, currentRotation) > 5f)
            {
                angleChanged = true;
            }

            if (angleChanged)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                fingerLifted = Input.GetMouseButtonUp(0);
#elif UNITY_IOS || UNITY_ANDROID
            if (Input.touchCount == 0) 
                fingerLifted = true;
#endif            
            }

            if (angleChanged && fingerLifted)
            {
                phase = Phase.WaitForConfirmation;

                controller.tutorialHUD.SetInstructionText(successfulText, "Tap anywhere to continue...");
                controller.StartCoroutine(WaitForScreenTap());
                yield break;
            }

            yield return null;
        }
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
