using UnityEngine;
using System;
using System.Collections.Generic;
using NUnit.Framework;

public interface IContextProvider
{
    void RegisterToContextEvents();
    Transform GetAimTransform();
    List<Vector3> GetTrajectory();
    void SetBallState(BallState newState);
    
    void RegisterListener(GameEvent eventType, Action onAbilityTriggered);
    void UnregisterListener(GameEvent eventType, Action onAbilityTriggered);
    void TriggerEvent(GameEvent eventType);
}
