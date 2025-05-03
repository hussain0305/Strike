using UnityEngine;
using System;
using System.Collections.Generic;

public class InGameContext : IContextProvider
{
    public Transform GetAimTransform()
    {
        return GameManager.Instance.AngleInput.cylinderPivot;
    }

    public Transform GetBallParent()
    {
        return GameManager.Instance.tee.transform;
    }

    public float GetLaunchForce()
    {
        return GameManager.Instance.LaunchForce;
    }

    public Quaternion GetLaunchAngle()
    {
        return GameManager.Instance.LaunchAngle;
    }

    public Vector2 GetSpinVector()
    {
        return GameManager.Instance.SpinVector;
    }

    public void SetBallState(BallState newState)
    {
        GameManager.BallState = newState;
    }

    public PinBehaviourPerTurn GetPinResetBehaviour()
    {
        return GameMode.Instance.PinBehaviour;
    }

    public Tee GetTee()
    {
        return GameManager.Instance.tee;
    }

    public int GetTrajectoryDefinition()
    {
        return GameManager.TRAJECTORY_DEFINITION;
    }

    public float GetGravity()
    {
        return GameManager.Instance.Gravity;
    }
}