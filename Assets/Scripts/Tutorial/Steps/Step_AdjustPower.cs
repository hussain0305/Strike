using UnityEngine;
using UnityEngine.UI;

public class Step_AdjustPower : TutorialStep
{
    private enum Phase { WaitForClick, WaitForSwipe, WaitForConfirmation }
    private Phase phase = Phase.WaitForClick;

    private int startingPower;

    public override void Begin(TutorialController tutorialController)
    {
        name = "POWER";
        elementsToActivate = new[]
        {
            TutorialHUD.TutorialScreenElements.PowerButton
        };
        successfulText = "Brilliant. Let's try adding some Spin now.";

        base.Begin(tutorialController);

        phase = Phase.WaitForClick;

        controller.tutorialHUD.SetInstructionText("Tap the Power button to begin adding Power");

        targetButton = controller.tutorialHUD.powerButton;
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
        controller.tutorialHUD.SetInstructionText("Now swipe across the screen to set your Power.");
        startingPower = (int)controller.tutorialHUD.BallParameterController.powerInput.Power;
        controller.StartCoroutine(CheckSwipeCoroutine());
    }

    private System.Collections.IEnumerator CheckSwipeCoroutine()
    {
        bool powerChanged = false;
        bool fingerLifted = false;

        while (true)
        {
            var currentPower = (int)controller.tutorialHUD.BallParameterController.powerInput.Power;

            if (!powerChanged && Mathf.Abs(currentPower - startingPower) > 5f)
            {
                powerChanged = true;
            }

            if (powerChanged)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                fingerLifted = Input.GetMouseButtonUp(0);
#elif UNITY_IOS || UNITY_ANDROID
            if (Input.touchCount == 0) 
                fingerLifted = true;
#endif            
            }

            if (powerChanged && fingerLifted)
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
