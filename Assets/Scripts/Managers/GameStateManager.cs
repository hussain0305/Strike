using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    private static GameStateManager instance;
    public static GameStateManager Instance => instance;

    public enum GameState { Menu, InGame, OnResultScreen }
    
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

    private void OnEnable()
    {
        GameManager.OnGameEnded += GameEnded;
        GameManager.OnGameExitedPrematurely += GameExitedPrematurely;
    }

    private void OnDisable()
    {
        GameManager.OnGameEnded -= GameEnded;
        GameManager.OnGameExitedPrematurely -= GameExitedPrematurely;
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

    public void ReturnEverythingToPool()
    {
        Transform worldCollectibles = LevelManager.Instance.collectiblesWorldParent;
        Transform worldCollectiblesCanvas = LevelManager.Instance.collectiblesWorldCanvasParent;
        Transform stars = LevelManager.Instance.starsParent;

        List<Transform> collectibles = new List<Transform>(worldCollectibles.childCount);
        foreach (Transform child in worldCollectibles)
        {
            collectibles.Add(child);
        }
        foreach (Transform child in collectibles)
        {
            ReturnCollectible(child);
        }

        List<Transform> canvasCollectibles = new List<Transform>(worldCollectiblesCanvas.childCount);
        foreach (Transform child in worldCollectiblesCanvas)
        {
            canvasCollectibles.Add(child);
        }
        foreach (Transform child in canvasCollectibles)
        {
            ReturnCollectible(child);
        }

        List<Transform> starList = new List<Transform>(stars.childCount);
        foreach (Transform star in stars)
        {
            starList.Add(star);
        }
        foreach (Transform star in starList)
        {
            PoolingManager.Instance.ReturnStar(star.gameObject);
        }
    }

    private void ReturnCollectible(Transform obj)
    {
        MultiplierToken multiplierCollectible = obj.GetComponent<MultiplierToken>();
        PointToken pointCollectible = obj.GetComponent<PointToken>();

        if (multiplierCollectible)
        {
            PoolingManager.Instance.ReturnObject(multiplierCollectible.multiplierTokenType, multiplierCollectible.gameObject);
        }
        else if (pointCollectible)
        {
            PoolingManager.Instance.ReturnObject(pointCollectible.pointTokenType, pointCollectible.gameObject);
        }
    }
    public void GameEnded()
    {
        ReturnEverythingToPool();
    }

    public void GameExitedPrematurely()
    {
        ReturnEverythingToPool();
        ReturnToMainMenu();
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void RetryLevel()
    {
        //TODO: Reload current scene cannot be hardcoded
        SceneManager.LoadScene(1);
    }
    
    public void LoadNextLevel()
    {
        ModeSelector.Instance.SetNextLevelSelected();
        SceneManager.LoadScene(1);
    }
}