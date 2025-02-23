using UnityEngine;
using System;
using System.Collections.Generic;

//Note to future self: This class is only for common functionality. There should be no mention of GameManger or any state-specific stuff like preview menu or whatever
public abstract class BaseContext : IContextProvider
{
    private Dictionary<GameEvent, List<Action>> eventActions = new Dictionary<GameEvent, List<Action>>();
    
    public void RegisterListener(GameEvent eventType, Action onAbilityTriggered)
    {
        if (!eventActions.ContainsKey(eventType))
        {
            eventActions[eventType] = new List<Action>();
        }

        if (!eventActions[eventType].Contains(onAbilityTriggered))
        {
            eventActions[eventType].Add(onAbilityTriggered);
        }
    }

    public void UnregisterListener(GameEvent eventType, Action onAbilityTriggered)
    {
        if (eventActions.ContainsKey(eventType))
        {
            eventActions[eventType].Remove(onAbilityTriggered);
            if (eventActions[eventType].Count == 0)
            {
                eventActions.Remove(eventType);
            }
        }
    }

    public void TriggerEvent(GameEvent eventType)
    {
        if (eventActions.TryGetValue(eventType, out List<Action> listeners))
        {
            foreach (var listener in listeners)
            {
                listener?.Invoke();
            }
        }
    }
    
    public void BallShot()
    {
        TriggerEvent(GameEvent.BallShot);
    }

    public void NextShotCued()
    {
        TriggerEvent(GameEvent.NextShotCued);
    }
    
    public abstract void RegisterToContextEvents();
    public abstract Transform GetAimTransform();
    public abstract List<Vector3> GetTrajectory();
    public abstract void SetBallState(BallState newState);
}
