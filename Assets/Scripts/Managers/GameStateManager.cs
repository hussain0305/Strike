using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class GameStateManager : MonoBehaviour
{
    private List<GameStateToggleListener> stateToggleListeners = new List<GameStateToggleListener>();

    private GameState currentGameState;
    public GameState CurrentGameState => currentGameState;

    private PoolingManager poolingManager;
    [InjectOptional]
    private ModeSelector modeSelector;
    
    [Inject]
    public void Construct(PoolingManager _poolingManager)
    {
        poolingManager = _poolingManager;
    }
    
    private void OnEnable()
    {
        EventBus.Subscribe<GameEndedEvent>(GameEnded);
        EventBus.Subscribe<GameRestartedEvent>(GameRestarted);
        EventBus.Subscribe<GameExitedEvent>(GameExitedPrematurely);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameEndedEvent>(GameEnded);
        EventBus.Unsubscribe<GameRestartedEvent>(GameRestarted);
        EventBus.Unsubscribe<GameExitedEvent>(GameExitedPrematurely);
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
            EventBus.Publish(new GameStateChangedEvent(newState));

            stateToggleListeners.RemoveAll(listener => listener == null || listener.gameObject == null);

            List<GameStateToggleListener> listenersCopy = new List<GameStateToggleListener>(stateToggleListeners);

            foreach (var listener in listenersCopy)
            {
                listener.gameObject.SetActive(true);
                listener.CheckState(newState);
            }
        }

        currentGameState = newState;
        StartCoroutine(DelayedBroadcast());
    }

    public void ReturnEverythingToPool()
    {
        Transform collectiblesParent = LevelManager.Instance.collectiblesParent;
        Transform stars = LevelManager.Instance.starsParent;
        Transform worldObstacles = LevelManager.Instance.worldObstaclesParent;
        Transform platformObstacles = LevelManager.Instance.platformObstaclesParent;
        
        List<Transform> collectibles = new List<Transform>(collectiblesParent.childCount);
        foreach (Transform child in collectiblesParent)
        {
            collectibles.Add(child);
        }
        foreach (Transform child in collectibles)
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
            poolingManager.ReturnStar(star.gameObject);
        }

        if (worldObstacles.childCount > 0)
        {
            List<Transform> worldObstaclesList = new List<Transform>(worldObstacles.childCount);
            foreach (Transform child in worldObstacles)
            {
                worldObstaclesList.Add(child);
            }
            foreach (Transform child in worldObstaclesList)
            {
                Obstacle obstacle = child.GetComponent<Obstacle>();
                poolingManager.ReturnObject(obstacle.type, obstacle.gameObject);
            }
        }

        if (platformObstacles.childCount > 0)
        {
            List<Transform> platformObstaclesList = new List<Transform>(platformObstacles.childCount);
            foreach (Transform child in platformObstacles)
            {
                platformObstaclesList.Add(child);
            }
            foreach (Transform child in platformObstaclesList)
            {
                Obstacle obstacle = child.GetComponent<Obstacle>();
                poolingManager.ReturnObject(obstacle.type, obstacle.gameObject);
            }
        }
    }

    public void ReturnCollectible(Transform obj)
    {
        MultiplierToken multiplierCollectible = obj.GetComponent<MultiplierToken>();
        PointToken pointCollectible = obj.GetComponent<PointToken>();
        DangerToken dangerCollectible = obj.GetComponent<DangerToken>();

        if (multiplierCollectible)
        {
            poolingManager.ReturnObject(multiplierCollectible.multiplierTokenType, multiplierCollectible.gameObject);
        }
        else if (pointCollectible)
        {
            poolingManager.ReturnObject(pointCollectible.pointTokenType, pointCollectible.gameObject);
        }
        else if (dangerCollectible)
        {
            poolingManager.ReturnObject(dangerCollectible.dangerTokenType, dangerCollectible.gameObject);
        }
    }
    public void GameEnded(GameEndedEvent e)
    {
        ReturnEverythingToPool();
    }

    public void GameExitedPrematurely(GameExitedEvent e)
    {
        ReturnEverythingToPool();
        ReturnToMainMenu();
    }

    public void GameRestarted(GameRestartedEvent e)
    {
        ReturnEverythingToPool();
        RetryLevel();
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene(modeSelector.CurrentSelectedModeInfo.scene);
    }
    
    public void LoadNextLevel()
    {
        modeSelector.SetNextLevelSelected();
        SceneManager.LoadScene(modeSelector.CurrentSelectedModeInfo.scene);
    }
}