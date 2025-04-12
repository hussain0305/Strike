using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialHUD : MonoBehaviour
{
    public enum TutorialScreenElements
    {
        GameHUD, FireButton, NextButton, TrajectoryButton, TrajectoryButtonContainer, TrajectoryHistoryButton,
        TrajectoryHistoryButtonContainer, CategoryInfoPanel, StepInfoPanel, AngleButton, SpinButton, PowerButton
    }

    [Header("Master")]
    public TutorialController tutorialController;
    public GameObject gameHUD;
    public GameObject tutorialMenu;
    
    [Header("Action Section")]
    public Button fireButton;
    public Button nextButton;
    
    [Header("Trajectory Section")]
    public GameObject trajectoryButton;
    public GameObject trajectoryButtonContainer;
    public GameObject trajectoryHistoryButton;
    public GameObject trajectoryHistoryButtonContainer;
    
    [Header("Info Section")]
    public GameObject categoryInfoPanel;
    public TextMeshProUGUI categoryNameText;
    public GameObject stepInfoPanel;
    public TextMeshProUGUI stepNameText;

    [Header("Ball Parameters Section")]
    public BallParameterController BallParameterController;
    public Button angleButton;
    public Button spinButton;
    public Button powerButton;
    
    [Header("Tutorial Specific")] 
    public TextMeshProUGUI stepInstructionText;
    public TextMeshProUGUI stepContinueText;
    public Button categorySelectionButton;
    public TutorialCategoryButton[] categoryButtons;
    
    public Dictionary<TutorialScreenElements, GameObject> tutorialScreenElements;
    [HideInInspector]
    public TutorialScreenElements[] toggleableElements;
    
    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        tutorialScreenElements =
            new Dictionary<TutorialScreenElements, GameObject>()
            {
                {TutorialScreenElements.GameHUD, gameHUD},
                {TutorialScreenElements.FireButton, fireButton.gameObject},
                {TutorialScreenElements.NextButton, nextButton.gameObject},
                {TutorialScreenElements.TrajectoryButton, trajectoryButton},
                {TutorialScreenElements.TrajectoryButtonContainer, trajectoryButtonContainer},
                {TutorialScreenElements.TrajectoryHistoryButton, trajectoryHistoryButton},
                {TutorialScreenElements.TrajectoryHistoryButtonContainer, trajectoryHistoryButtonContainer},
                {TutorialScreenElements.CategoryInfoPanel, categoryInfoPanel},
                {TutorialScreenElements.StepInfoPanel, stepInfoPanel},
                {TutorialScreenElements.AngleButton, angleButton.gameObject},
                {TutorialScreenElements.SpinButton, spinButton.gameObject},
                {TutorialScreenElements.PowerButton, powerButton.gameObject},
            };

        toggleableElements = new TutorialScreenElements[]
        {
            TutorialScreenElements.FireButton,
            TutorialScreenElements.NextButton,
            TutorialScreenElements.TrajectoryButton,
            TutorialScreenElements.TrajectoryButtonContainer,
            TutorialScreenElements.TrajectoryHistoryButton,
            TutorialScreenElements.TrajectoryHistoryButtonContainer,
            TutorialScreenElements.AngleButton,
            TutorialScreenElements.SpinButton,
            TutorialScreenElements.PowerButton
        };
    }

    private void OnEnable()
    {
        EventBus.Subscribe<TutorialCategorySelectedEvent>(CategorySelected);
        
        categorySelectionButton.onClick.RemoveAllListeners();
        categorySelectionButton.onClick.AddListener(OpenCategorySelectionMenu);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<TutorialCategorySelectedEvent>(CategorySelected);
        
        categorySelectionButton.onClick.RemoveAllListeners();
    }
    
    public void ResetCategorySelectionMenu()
    {
        foreach (var categoryButton in categoryButtons)
        {
            categoryButton.Reset();
        }
        
        gameHUD.SetActive(false);
        tutorialMenu.SetActive(true);
    }
    
    public void OpenCategorySelectionMenu()
    {
        tutorialController.Reset();
        EventBus.Publish(new NextShotCuedEvent());

        foreach (var categoryButton in categoryButtons)
        {
            categoryButton.SetAppearance(tutorialController.CurrentCategory);
        }
        
        gameHUD.SetActive(false);
        tutorialMenu.SetActive(true);
    }
    
    public void CategorySelected(TutorialCategorySelectedEvent e)
    {
        gameHUD.SetActive(true);
        tutorialMenu.SetActive(false);

        tutorialScreenElements[TutorialScreenElements.CategoryInfoPanel].SetActive(true);
        categoryNameText.text = e.Category.categoryName;
    }

    public void SetStepText(string stepName)
    {
        tutorialScreenElements[TutorialScreenElements.StepInfoPanel].SetActive(true);
        stepNameText.text = stepName;
    }
    
    public void SetUIElementsActive(List<TutorialScreenElements> activeElements)
    {
        foreach (var toggleableElement in toggleableElements)
        {
            tutorialScreenElements[toggleableElement].SetActive(activeElements.Contains(toggleableElement));
        }
    }
    
    public void SetAllUIElementsInactive()
    {
        foreach (var toggleableElement in toggleableElements)
        {
            tutorialScreenElements[toggleableElement].SetActive(false);
        }
    }
    
    public void SetInstructionText(string instructionText, string secondaryText = "")
    {
        stepInstructionText.text = instructionText;
        stepContinueText.text = secondaryText;
    }
}
