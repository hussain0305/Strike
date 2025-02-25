using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum CoroutineType
{
    Untracked,
    BallPreview
};

public class CoroutineDispatcher : MonoBehaviour
{
    private static CoroutineDispatcher instance;
    public static CoroutineDispatcher Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = new GameObject("CoroutineDispatcher");
                instance = obj.AddComponent<CoroutineDispatcher>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private Dictionary<CoroutineType, Coroutine> activeCoroutines = new Dictionary<CoroutineType, Coroutine>();

    public void RunCoroutine(IEnumerator coroutine, CoroutineType coroutineType = CoroutineType.Untracked)
    {
        if (coroutineType != CoroutineType.Untracked && activeCoroutines.TryGetValue(coroutineType, out Coroutine existingCoroutine))
        {
            StopCoroutine(existingCoroutine);
        }
        activeCoroutines[coroutineType] = StartCoroutine(coroutine);
    }
    
    public void StopCoroutine(CoroutineType coroutineType)
    {
        if (activeCoroutines.TryGetValue(coroutineType, out Coroutine runningCoroutine))
        {
            StopCoroutine(runningCoroutine);
            activeCoroutines.Remove(coroutineType);
        }
    }

    public void StopAllTrackedCoroutines()
    {
        foreach (Coroutine runningCoroutine in activeCoroutines.Values)
        {
            StopCoroutine(runningCoroutine);
        }
        activeCoroutines.Clear();
    }
}