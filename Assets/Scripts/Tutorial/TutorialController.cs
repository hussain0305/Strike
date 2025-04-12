using UnityEngine;

public class TutorialStepCompletedEvent { }
public class TutorialResetEvent { }

public class TutorialController : MonoBehaviour
{
    public TutorialHUD tutorialHUD;

    private TutorialCategory currentCategory;
    private TutorialStep currentStep;
    private int currentStepIndex;

    public TutorialCategory CurrentCategory => currentCategory;
    public TutorialStep CurrentStep => currentStep;
    public int CurrentStepIndex => currentStepIndex;

    private void OnEnable()
    {
        EventBus.Subscribe<TutorialCategorySelectedEvent>(CategorySelected);
        EventBus.Subscribe<TutorialStepCompletedEvent>(TutorialStepCompleted);
        Reset();
        tutorialHUD.ResetCategorySelectionMenu();
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<TutorialCategorySelectedEvent>(CategorySelected);
        EventBus.Unsubscribe<TutorialStepCompletedEvent>(TutorialStepCompleted);
    }
    
    public void Reset()
    {
        EventBus.Publish(new TutorialResetEvent());
        currentCategory = null;
        currentStep = null;
        currentStepIndex = -1;
    }

    public void CategorySelected(TutorialCategorySelectedEvent e)
    {
        SetCurrentCategory(e.Category);
    }
    
    public void SetCurrentCategory(TutorialCategory _category)
    {
        currentCategory = _category;
        currentStepIndex = 0;
        
        SetCurrentStep(currentStepIndex);
    }

    public void SetCurrentStep(int index)
    {
        if (index >= currentCategory.categorySteps.Length)
        {
            Debug.LogError("Step number exceeds steps in this category");
            return;
        }
        currentStep = currentCategory.categorySteps[index];
        currentCategory.StartStep(currentStepIndex, this);
        tutorialHUD.SetStepText($"({currentStepIndex + 1}/{currentCategory.categorySteps.Length}) {currentStep.name}");
    }

    public void MoveToNextStep()
    {
        currentStepIndex++;
        if (currentStepIndex >= currentCategory.categorySteps.Length)
        {
            Reset();
            //TODO: Process move to next category or moving to end of list of all categories
        }
        else
        {
            SetCurrentStep(currentStepIndex);
        }
    }

    public void TutorialStepCompleted(TutorialStepCompletedEvent e)
    {
        MoveToNextStep();
    }
}
