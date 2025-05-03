using UnityEngine;
using System.Collections.Generic;

public interface IContextProvider
{
    int GetTrajectoryDefinition();
    void SetBallState(BallState newState);
    float GetGravity();
    float GetLaunchForce();
    Tee GetTee();
    Vector2 GetSpinVector();
    Transform GetAimTransform();
    Transform GetBallParent();
    Quaternion GetLaunchAngle();
    PinBehaviourPerTurn GetPinResetBehaviour();
}
