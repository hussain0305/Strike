using UnityEngine;

public class TutorialContext : IContextProvider
{
    private Ball currentBall;
    private Transform aimTransform;
    private Tee tee;

    private Vector2 spoofedSpinVector;
    private Quaternion spoofedLaunchAngle;
    private float spoofedLaunchForce;
    private ShotInput shotInput;
    private float gravity;
    
    public Transform GetAimTransform()
    {
        return aimTransform;
    }

    public Transform GetBallParent()
    {
        return tee?.transform;
    }

    public Vector2 GetSpinVector()
    {
        return shotInput.spinInput.SpinVector;
    }
    
    public Quaternion GetLaunchAngle()
    {
        return shotInput.angleInput.cylinderPivot.rotation;
    }

    public float GetLaunchForce()
    {
        return shotInput.powerInput.Power;
    }

    public int GetTrajectoryDefinition()
    {
        return 100;
    }

    public void SetBallState(BallState newState)
    {
        
    }

    public BallState GetBallState()
    {
        return BallState.OnTee;
    }

    public float GetGravity()
    {
        return gravity;
    }
    
    public Tee GetTee()
    {
        return tee;
    }

    public PinBehaviourPerTurn GetPinResetBehaviour()
    {
        return PinBehaviourPerTurn.Reset;
    }
    
    public void InitTutorial(Ball _ball, ShotInput shotInput, Tee _tee)
    {
        this.shotInput = shotInput;
        aimTransform = this.shotInput.angleInput.cylinderPivot;
        currentBall = _ball;
        tee = _tee;
        gravity = -Physics.gravity.y;
    }
}
