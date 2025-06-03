using UnityEngine;

public class EndlessGameMode : GameMode
{
    private bool eliminationPinHit = false;
    
    public void SetAsResetMode()
    {
        pinBehaviour = PinBehaviourPerTurn.Reset;
    }
    
    public void SetAsDisappearingPinMode()
    {
        pinBehaviour = PinBehaviourPerTurn.DisappearUponCollection;
    }
    
    public override void OnShotComplete(bool hitDangerPin, bool hitNormalPin)
    {
        eliminationPinHit = eliminationPinHit || hitDangerPin;
        EventBus.Publish(new CueNextShotEvent());
    }
    
    public override bool LevelCompletedSuccessfully()
    {
        return !eliminationPinHit && roundDataManager.GetPointsForPlayer(0) >= PointsRequired;
    }

}
