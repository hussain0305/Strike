using UnityEngine;
using System.Collections.Generic;

public interface IContextProvider
{
    Transform GetAimTransform();
    List<Vector3> GetTrajectory();
    void SetBallState(BallState newState);
    PinBehaviourPerTurn GetPinResetBehaviour();
}
