using UnityEngine;
using UnityEngine.UI;

public class Step_AdjustSpin : TutorialStep
{
    private enum Phase { WaitForClick, WaitForSwipe, WaitForConfirmation }
    private Phase phase = Phase.WaitForClick;

    private Vector2 startingSpin;
    
    public override void Begin(TutorialController tutorialController)
    {
        name = "SPIN";
        elementsToActivate = new[]
        {
            TutorialHUD.TutorialScreenElements.SpinButton
        };
        successfulText = "Almost there!";

        base.Begin(tutorialController);

        phase = Phase.WaitForClick;

        controller.tutorialHUD.SetInstructionText("Tap the Spin button to begin adding Spin");

        targetButton = controller.tutorialHUD.spinButton;
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
        controller.tutorialHUD.SetInstructionText("Now swipe across the screen to set your spin.");
        startingSpin = controller.tutorialHUD.BallParameterController.spinInput.SpinVector;
        controller.StartCoroutine(CheckSwipeCoroutine());
    }

    private System.Collections.IEnumerator CheckSwipeCoroutine()
    {
        bool spinChanged = false;
        bool fingerLifted = false;

        while (true)
        {
            var currentSpin = controller.tutorialHUD.BallParameterController.spinInput.SpinVector;

            if (!spinChanged && Vector2.SqrMagnitude(currentSpin - startingSpin) > 0.2f)
            {
                spinChanged = true;
            }

            if (spinChanged)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                fingerLifted = Input.GetMouseButtonUp(0);
#elif UNITY_IOS || UNITY_ANDROID
            if (Input.touchCount == 0) 
                fingerLifted = true;
#endif            
            }

            if (spinChanged && fingerLifted)
            {
                phase = Phase.WaitForConfirmation;

                controller.tutorialHUD.SetInstructionText(successfulText, "Tap anywhere to continue.");
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
