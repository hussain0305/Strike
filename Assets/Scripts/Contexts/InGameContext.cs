using UnityEngine;

public class InGameContext : IContextProvider
{
    private GameManager gameManager;
    private GameMode gameMode;

    public InGameContext(GameManager _gameManager, GameMode _gameMode)
    {
        gameManager = _gameManager;
        gameMode = _gameMode;
    }
    
    public Transform GetAimTransform()
    {
        return gameManager.AngleInput.cylinderPivot;
    }

    public Transform GetBallParent()
    {
        return gameManager.tee.transform;
    }

    public float GetLaunchForce()
    {
        return gameManager.LaunchForce;
    }

    public Quaternion GetLaunchAngle()
    {
        return gameManager.LaunchAngle;
    }

    public Vector2 GetSpinVector()
    {
        return gameManager.SpinVector;
    }

    public void SetBallState(BallState newState)
    {
        GameManager.BallState = newState;
    }

    public PinBehaviourPerTurn GetPinResetBehaviour()
    {
        return gameMode.PinBehaviour;
    }

    public Tee GetTee()
    {
        return gameManager.tee;
    }

    public int GetTrajectoryDefinition()
    {
        return GameManager.TRAJECTORY_DEFINITION;
    }

    public float GetGravity()
    {
        return gameManager.Gravity;
    }
}