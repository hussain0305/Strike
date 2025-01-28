using UnityEngine;
using System;
using System.Collections;

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

    public void RunCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
}