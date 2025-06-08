using UnityEngine;

public interface IHUD
{
    void ShowLevelInfo();
    void SetNextButtonActive(bool active);
    void SetTrajectoryButtonSectionActive(bool active);
    void SetTrajectoryAdButtonSectionActive(bool active);
    void UpdateOptionalShotPhaseProgress(float time);
    void UpdateTrajectoryViewTimeRemaining(int secondsRemaining);
    void CheckToShowTrajectoryButton();
}
