using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EventBus
{
    private static Dictionary<Type, Delegate> subscribers = new Dictionary<Type, Delegate>();
    
    public static void Subscribe<T>(Action<T> listener)
    {
        var type = typeof(T);
        if (!subscribers.ContainsKey(type))
            subscribers[type] = null;

        var existingDelegate = subscribers[type] as Action<T>;

        if (existingDelegate != null && existingDelegate.GetInvocationList().Contains(listener))
            return;

        subscribers[type] = existingDelegate + listener;
    }

    public static void Unsubscribe<T>(Action<T> listener)
    {
        if (subscribers.ContainsKey(typeof(T)))
        {
            subscribers[typeof(T)] = (Action<T>)subscribers[typeof(T)] - listener;
            if (subscribers[typeof(T)] == null)
                subscribers.Remove(typeof(T));
        }
    }

    public static void Publish<T>(T eventData)
    {
        if (subscribers.ContainsKey(typeof(T)) && subscribers[typeof(T)] is Action<T> action)
            action.Invoke(eventData);
    }

    public static void Clear()
    {
        subscribers.Clear();
    }
}

/*
public static void Subscribe<T>(Action<T> listener)
{
    if (!eventTable.ContainsKey(typeof(T)))
        eventTable[typeof(T)] = null;

    eventTable[typeof(T)] = (Action<T>)eventTable[typeof(T)] + listener;
}
*/