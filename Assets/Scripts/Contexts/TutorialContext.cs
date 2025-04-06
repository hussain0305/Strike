using UnityEngine;

public class TutorialContext : IContextProvider
{
    private Ball currentBall;
    private Transform aimTransform;
    private Tee tee;

    private Vector2 spoofedSpinVector;
    private Quaternion spoofedLaunchAngle;
    private float spoofedLaunchForce;
    private BallParameterController ballParameterController;
    private float gravity;
    
    public Transform GetAimTransform()
    {
        return aimTransform;
    }
    
    public Vector2 GetSpinVector()
    {
        return ballParameterController.spinInput.SpinVector;
    }
    
    public Quaternion GetLaunchAngle()
    {
        return ballParameterController.angleInput.cylinderPivot.rotation;
    }

    public float GetLaunchForce()
    {
        return ballParameterController.powerInput.Power;
    }

    public int GetTrajectoryDefinition()
    {
        return 100;
    }

    public void SetBallState(BallState newState)
    {
        
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
    
    public void InitTutorial(Ball _ball, BallParameterController _ballParameterController, Tee _tee)
    {
        ballParameterController = _ballParameterController;
        aimTransform = ballParameterController.angleInput.cylinderPivot;
        currentBall = _ball;
        tee = _tee;
        gravity = -Physics.gravity.y;
    }
}
