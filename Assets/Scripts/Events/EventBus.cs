using System;
using System.Collections.Generic;

public static class EventBus
{
    private static Dictionary<Type, Delegate> eventTable = new Dictionary<Type, Delegate>();

    public static void Subscribe<T>(Action<T> listener)
    {
        if (!eventTable.ContainsKey(typeof(T)))
            eventTable[typeof(T)] = null;

        eventTable[typeof(T)] = (Action<T>)eventTable[typeof(T)] + listener;
    }

    public static void Unsubscribe<T>(Action<T> listener)
    {
        if (eventTable.ContainsKey(typeof(T)))
        {
            eventTable[typeof(T)] = (Action<T>)eventTable[typeof(T)] - listener;
            if (eventTable[typeof(T)] == null)
                eventTable.Remove(typeof(T));
        }
    }

    public static void Publish<T>(T eventData)
    {
        if (eventTable.ContainsKey(typeof(T)) && eventTable[typeof(T)] is Action<T> action)
            action.Invoke(eventData);
    }

    public static void Clear()
    {
        eventTable.Clear();
    }
}