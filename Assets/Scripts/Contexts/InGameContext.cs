using UnityEngine;
using System;
using System.Collections.Generic;

public class InGameContext : IContextProvider
{
    public Transform GetAimTransform()
    {
        return GameManager.Instance.angleInput.cylinderPivot;
    }

    public List<Vector3> GetTrajectory()
    {
        return GameManager.Instance.CalculateTrajectoryPoints();
    }

    public void SetBallState(BallState newState)
    {
        GameManager.BallState = newState;
    }

    public PinBehaviourPerTurn GetPinResetBehaviour()
    {
        return GameManager.Instance.pinBehaviour;
    }

    public Transform GetBallTeePosition()
    {
        return GameManager.Instance.tee.ballPosition;
    }
}