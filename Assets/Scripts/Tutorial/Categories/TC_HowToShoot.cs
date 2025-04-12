using System;
using UnityEngine;

public class TC_HowToShoot : TutorialCategory
{
    private void Awake()
    {
        categorySteps = new TutorialStep[]
        {
            new Step_AdjustAngle(),
            new Step_ToggleTrajectory(),
            new Step_AdjustPower(),
            new Step_AdjustSpin(),
            new Step_Fire(),
        };
    }
}
