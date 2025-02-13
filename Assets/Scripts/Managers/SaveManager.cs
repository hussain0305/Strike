using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public static class SaveManager
{
    private static SaveData currentSaveData;
    public static event Action OnSaveFileLoaded;
    public static event Action OnGameReady;
    public static bool IsSaveLoaded { get; private set; } = false;
    public static bool IsGameReady { get; private set; } = false;
    
    private static readonly HashSet<MonoBehaviour> pendingListeners = new HashSet<MonoBehaviour>();
    
    static SaveManager()
    {
        KeyManager.GenerateAndStoreKeys();
        // LoadData();
    }
    
    public static async void LoadData()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            currentSaveData = WebGLSaveSystem.LoadGame();
        }
        else
        {
            currentSaveData = await SaveSystem.LoadGameAsync();
        }

        Debug.Log("IsSaveLoaded set to true");
        IsSaveLoaded = true;
    }
    
    public static IEnumerator LoadSaveProcess()
    {
        yield return new WaitForSeconds(0.25f);//Wait for everyone to register

        LoadData();

        yield return new WaitUntil(() => IsSaveLoaded);
        yield return new WaitForSeconds(0.25f);

        OnSaveFileLoaded?.Invoke();
    }
    
    private static void SaveData()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            WebGLSaveSystem.SaveGame(currentSaveData);
        }
        else
        {
            SaveSystem.SaveGame(currentSaveData);
        }
    }
    
    private static void EnsureDataLoaded()
    {
        if (!IsSaveLoaded)
        {
            Debug.LogError("Save data has not been loaded yet!");
            throw new InvalidOperationException("Save data is not loaded. Ensure LoadData is called before accessing save-related methods.");
        }
    }

    #region Listeners

    public static void RegisterListener(MonoBehaviour listener)
    {
        pendingListeners.Add(listener);
    }

    public static void MarkListenerComplete(MonoBehaviour listener)
    {
        if (pendingListeners.Contains(listener))
        {
            pendingListeners.Remove(listener);
        }

        if (pendingListeners.Count == 0)
        {
            Debug.Log("All listeners completed. Broadcasting OnGameReady.");
            IsGameReady = true;
            OnGameReady?.Invoke();
        }
    }

    #endregion

    #region Get From and Save To Save File

    public static int GetStars()
    {
        EnsureDataLoaded();
        return currentSaveData.stars;
    }

    public static void AddStars(int numStars)
    {
        EnsureDataLoaded();
        currentSaveData.stars = currentSaveData.stars + numStars;
        SaveData();
    }

    public static void SpendStars(int numStars)
    {
        EnsureDataLoaded();
        currentSaveData.stars = currentSaveData.stars - numStars;
        SaveData();
    }

    public static bool GetIsGameModeUnlocked(int gameModeIndex)
    {
        EnsureDataLoaded();
        foreach (int gameMode in currentSaveData.unlockedGameModes)
        {
            if (gameMode == gameModeIndex)
            {
                return true;
            }
        }
        return false;
    }

    public static void SetGameModeUnlocked(int gameModeIndex)
    {
        EnsureDataLoaded();
        List<int> currentlyUnlockedGameModes = currentSaveData.unlockedGameModes.ToList();
        if (!currentlyUnlockedGameModes.Contains(gameModeIndex))
        {
            currentlyUnlockedGameModes.Add(gameModeIndex);
        }
        currentSaveData.unlockedGameModes = currentlyUnlockedGameModes.ToArray();
        SaveData();
    }

    public static void SetSelectedBall(int ballIndex)
    {
        EnsureDataLoaded();
        currentSaveData.selectedBall = ballIndex;
        SaveData();
    }

    public static int GetSelectedBall()
    {
        EnsureDataLoaded();
        return currentSaveData.selectedBall;
    }

    public static void SetLevelCompleted(GameModeType gameMode, int levelIndex)
    {
        SetLevelCompleted((int)gameMode, levelIndex);
    }
    public static void SetLevelCompleted(int gameModeIndex, int levelIndex)
    {
        EnsureDataLoaded();
        foreach (var progress in currentSaveData.levelProgress)
        {
            if (progress.gameMode == gameModeIndex)
            {
                if (levelIndex > progress.maxUnlockedLevel)
                {
                    progress.maxUnlockedLevel = levelIndex;
                    SaveData();
                }
                return;
            }
        }
        currentSaveData.levelProgress.Add(new LevelProgress(gameModeIndex, levelIndex));
        SaveData();
    }

    public static int GetMaxUnlockedLevel(GameModeType gameModeIndex)
    {
        return GetMaxUnlockedLevel((int)gameModeIndex);
    }
    
    public static int GetMaxUnlockedLevel(int gameModeIndex)
    {
        EnsureDataLoaded();
        foreach (var progress in currentSaveData.levelProgress)
        {
            if (progress.gameMode == gameModeIndex)
            {
                return progress.maxUnlockedLevel;
            }
        }
        return 0;
    }

    #endregion
}

/*
private static void LoadData()
{
    if (Application.platform == RuntimePlatform.WebGLPlayer)
    {
        currentSaveData = WebGLSaveSystem.LoadGame();
    }
    else
    {
        currentSaveData = SaveSystem.LoadGame();
    }
    IsSaveLoaded = true;
    OnSaveFileLoaded?.Invoke();
}
*/