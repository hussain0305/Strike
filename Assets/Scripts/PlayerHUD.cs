using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class HUDAction_Fire { }
public class HUDAction_Next { }
public class HUDAction_CheckTrajectory { }
public class HUDAction_CheckTrajectoryHistory { }
public class HUDAction_ViewAdForTrajectory { }

public class PlayerHUD : MonoBehaviour, IHUD
{
    [Header("Controls Panel")]
    public ShotInput shotInput;
    public TrajectoryButton trajectoryButton;
    public GameObject trajectoryButtonSection;
    public GameObject trajectoryAdButtonSection;
    public TrajectoryHistoryButton trajectoryHistoryButton;
    
    [Header("Game Screen")]
    public Button fireButton;
    public Button nextButton;
    public Image nextButtonFill;
    public TextMeshProUGUI volleyText;
    public TextMeshProUGUI objectiveText;

    private PowerInput PowerInput => shotInput.powerInput;
    private SpinInput SpinInput => shotInput.spinInput;
    private AngleInput AngleInput => shotInput.angleInput;
    private float LaunchForce => PowerInput.Power;
    private Vector2 SpinVector => SpinInput.SpinVector;
    private Quaternion LaunchAngle => AngleInput.cylinderPivot.rotation;

    private const int MIN_POWER_TO_OFFER_TRAJECTORY_VIEW = 10;

    private bool shotOngoing;
    
    [Inject]
    private GameMode gameMode;
    [Inject]
    private RoundDataManager roundDataManager;
    [Inject]
    private TrajectoryHistoryViewer trajectoryHistoryViewer;
    
    private void OnEnable()
    {
        fireButton.onClick.AddListener(() =>
        {
            EventBus.Publish(new HUDAction_Fire());
        });
        
        nextButton.onClick.AddListener(() =>
        {
            EventBus.Publish(new HUDAction_Next());
        });
        
        EventBus.Subscribe<BallShotEvent>(BallShot);
        EventBus.Subscribe<CueNextShotEvent>(CueNextShot);
        EventBus.Subscribe<NextShotCuedEvent>(NextShotCued);
        EventBus.Subscribe<ShotCompleteEvent>(ShotComplete);
        EventBus.Subscribe<StoppedShotInput>(StoppedInputtingShot);
        EventBus.Subscribe<NewRoundStartedEvent>(NewRoundStarted);
    }

    private void OnDisable()
    {
        fireButton.onClick.RemoveAllListeners();
        nextButton.onClick.RemoveAllListeners();
        
        EventBus.Unsubscribe<BallShotEvent>(BallShot);
        EventBus.Unsubscribe<CueNextShotEvent>(CueNextShot);
        EventBus.Unsubscribe<NextShotCuedEvent>(NextShotCued);
        EventBus.Unsubscribe<ShotCompleteEvent>(ShotComplete);
        EventBus.Unsubscribe<StoppedShotInput>(StoppedInputtingShot);
        EventBus.Unsubscribe<NewRoundStartedEvent>(NewRoundStarted);
    }
    
    public void ShowLevelInfo()
    {
        volleyText.text = "Volley 1";

        switch (gameMode.GetWinCondition())
        {
            case WinCondition.PointsRequired:
                objectiveText.text = $"TARGET : {gameMode.PointsRequired}";
                break;
            case WinCondition.PointsRanking:
                objectiveText.text = $"Maximum total points wins";
                break;
            case WinCondition.Survival:
                objectiveText.text = $"Survive All Volleys";
                break;
        }
    }

    public void StoppedInputtingShot(StoppedShotInput e)
    {
        if (!trajectoryButtonSection.gameObject.activeSelf && LaunchForce > MIN_POWER_TO_OFFER_TRAJECTORY_VIEW && 
            LaunchAngle.eulerAngles.sqrMagnitude > 1)
        {
            CheckToShowTrajectoryButton();
        }
    }
    
    public void CheckToShowTrajectoryButton()
    {
        if (shotOngoing)
            return;
        
        int trajectoryViewsRemaining = roundDataManager.GetTrajectoryViewsRemaining();
        if (trajectoryViewsRemaining > 0)
        {
            trajectoryButtonSection.gameObject.SetActive(true);
            trajectoryAdButtonSection.gameObject.SetActive(false);
            trajectoryButton.ShowButton(trajectoryViewsRemaining);
        }
        else
        {
            trajectoryButtonSection.gameObject.SetActive(false);
#if ADS_DISABLED
#else
            trajectoryAdButtonSection.gameObject.SetActive(true);
#endif
        }
    }
    
    private void BallShot(BallShotEvent e)
    {
        shotOngoing = true;
        fireButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        trajectoryAdButtonSection.gameObject.SetActive(false);
        trajectoryHistoryButton.gameObject.SetActive(false);
        trajectoryButtonSection.gameObject.SetActive(false);
    }
    
    private void ShotComplete(ShotCompleteEvent e)
    {
        nextButton.gameObject.SetActive(false);
    }
    
    private void CueNextShot(CueNextShotEvent e)
    {
        nextButton.gameObject.SetActive(false);
    }

    private void NextShotCued(NextShotCuedEvent e)
    {
        shotOngoing = false;
        fireButton.gameObject.SetActive(true);
        CheckToShowTrajectoryHistoryButton();
    }
    
    private void NewRoundStarted(NewRoundStartedEvent e)
    {
        volleyText.text = $"Volley {e.RoundNumber}";
    }

    public void SetNextButtonActive(bool active)
    {
        nextButton.gameObject.SetActive(active);
    }

    public void SetTrajectoryButtonSectionActive(bool active)
    {
        trajectoryButtonSection.gameObject.SetActive(active);
    }

    public void SetTrajectoryAdButtonSectionActive(bool active)
    {
#if ADS_DISABLED
        active = false;
#endif
        trajectoryAdButtonSection.gameObject.SetActive(active);
    }

    public void UpdateOptionalShotPhaseProgress(float progress)
    {
        nextButtonFill.fillAmount = progress;
    }

    public void UpdateTrajectoryViewTimeRemaining(int secondsRemaining)
    {
        trajectoryButton.SetCountdownText(secondsRemaining);
    }
    
    public void CheckToShowTrajectoryHistoryButton()
    {
        if (shotOngoing)
            return;
        
        trajectoryHistoryButton.gameObject.SetActive(trajectoryHistoryViewer.GetIsTrajectoryHistoryAvailable());
    }
}
