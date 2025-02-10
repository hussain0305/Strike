using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public enum GameState { Menu, InGame, OnResultScreen }

    private static GameStateManager instance;
    public static GameStateManager Instance => instance;

    public event Action<GameState> OnGameStateChanged;
    
    private List<GameStateToggleListener> stateToggleListeners = new List<GameStateToggleListener>();

    private void Awake()
    {
        if (Instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterStateListener(GameStateToggleListener listener)
    {
        if (!stateToggleListeners.Contains(listener))
        {
            stateToggleListeners.Add(listener);
        }
    }

    public void UnregisterStateListener(GameStateToggleListener listener)
    {
        stateToggleListeners.Remove(listener);
    }

    public void SetGameState(GameState newState)
    {
        IEnumerator DelayedBroadcast()
        {
            yield return null;
            OnGameStateChanged?.Invoke(newState);

            stateToggleListeners.RemoveAll(listener => listener == null || listener.gameObject == null);

            List<GameStateToggleListener> listenersCopy = new List<GameStateToggleListener>(stateToggleListeners);

            foreach (var listener in listenersCopy)
            {
                listener.gameObject.SetActive(true);
                listener.CheckState(newState);
            }
        }

        StartCoroutine(DelayedBroadcast());
    }
}