using System.Collections.Generic;
using UnityEngine;

public class TutorialRoutineManager
{
    // private TutorialManager tutorialManager;
    // private List<TutorialCategory> categories;
    // private int currentCategoryIndex = 0;
    // private int currentStepIndex = 0;
    // private bool sequentialMode;
    //
    // public void Initialize(TutorialManager manager, bool runSequentially)
    // {
    //     tutorialManager = manager;
    //     sequentialMode = runSequentially;
    //     categories = LoadAllCategories();
    //     StartCategory(0);
    // }
    //
    // public void StartCategory(int index)
    // {
    //     currentCategoryIndex = index;
    //     currentStepIndex = 0;
    //     categories[currentCategoryIndex].ResetSteps();
    //     categories[currentCategoryIndex].StartStep(currentStepIndex, tutorialManager, OnStepComplete);
    // }
    //
    // private void OnStepComplete()
    // {
    //     currentStepIndex++;
    //     if (currentStepIndex < categories[currentCategoryIndex].Steps.Count)
    //     {
    //         categories[currentCategoryIndex].StartStep(currentStepIndex, tutorialManager, OnStepComplete);
    //     }
    //     else if (sequentialMode)
    //     {
    //         currentCategoryIndex++;
    //         if (currentCategoryIndex < categories.Count)
    //         {
    //             StartCategory(currentCategoryIndex);
    //         }
    //         else
    //         {
    //             EndTutorial();
    //         }
    //     }
    //     else
    //     {
    //         ShowCategoryMenu();
    //     }
    // }
    //
    // private void EndTutorial()
    // {
    //     Debug.Log("Tutorial complete!");
    // }
    //
    // private List<TutorialCategory> LoadAllCategories()
    // {
    //     return new List<TutorialCategory>
    //     {
    //         new TutorialCategory("Basics", new List<TutorialStep>
    //         {
    //             // new Step_AdjustAngle(),
    //             // new Step_AdjustSpin(),
    //             // new Step_AdjustPower(),
    //             // new Step_FireBall()
    //         }),
    //         new TutorialCategory("Scoring", new List<TutorialStep> { /* ... */ }),
    //         new TutorialCategory("Trajectory View", new List<TutorialStep> { /* ... */ }),
    //     };
    // }
    //
    // private void ShowCategoryMenu()
    // {
    //     Debug.Log("Tutorial menu: select a category to try");
    // }
}
