using UnityEngine;
using System;
using System.Collections.Generic;

public class InGameContext : BaseContext
{
    public override Transform GetAimTransform()
    {
        return GameManager.Instance.angleInput.cylinderPivot;
    }

    public override List<Vector3> GetTrajectory()
    {
        return GameManager.Instance.CalculateTrajectoryPoints();
    }

    public override void RegisterToContextEvents()
    {
        GameManager.OnBallShot += BallShot;
        GameManager.OnNextShotCued += NextShotCued;
    }

    public override void SetBallState(BallState newState)
    {
        GameManager.BallState = newState;
    }
}