using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class SaveLoadedEvent { }
public class GameReadyEvent { }

public static class SaveManager
{
    private static SaveData currentSaveData;
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

        currentSaveData.SyncListToDictionary();
        Debug.Log("IsSaveLoaded set to true");
        IsSaveLoaded = true;
    }
    
    public static IEnumerator LoadSaveProcess()
    {
        yield return new WaitForSeconds(0.25f);//Wait for everyone to register

        LoadData();

        yield return new WaitUntil(() => IsSaveLoaded);
        yield return new WaitForSeconds(0.25f);

        EventBus.Publish(new SaveLoadedEvent());
    }
    
    private static void SaveData()
    {
        currentSaveData.SyncDictionaryToList();
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
            EventBus.Publish(new GameReadyEvent());
        }
    }

    #endregion

    #region Get From and Save To Save File

    public static int GetStars()
    {
        EnsureDataLoaded();
        return currentSaveData.stars;
    }

    public static void AddStars(int numStars, bool writeSaveImmediately = true)
    {
        EnsureDataLoaded();
        currentSaveData.stars = currentSaveData.stars + numStars;
        if (writeSaveImmediately)
        {
            SaveData();
        }
        EventBus.Publish(new StarsEarnedEvent(numStars));
    }

    public static void SpendStars(int numStars)
    {
        EnsureDataLoaded();
        currentSaveData.stars = currentSaveData.stars - numStars;
        SaveData();
        EventBus.Publish(new StarsSpentEvent(numStars));
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

    public static void SetEquippedBall(string ballID)
    {
        EnsureDataLoaded();
        currentSaveData.isFusionEquipped = false;
        currentSaveData.selectedBall = ballID;
        SaveData();
    }

    public static string GetEquippedBall()
    {
        EnsureDataLoaded();
        return currentSaveData.selectedBall;
    }

    public static void SetLevelCompleted(GameModeType gameMode, int levelIndex, bool writeSaveImmediately = true)
    {
        SetLevelCompleted((int)gameMode, levelIndex, writeSaveImmediately);
    }
    
    public static void SetLevelCompleted(int gameModeIndex, int levelIndex, bool writeSaveImmediately = true)
    {
        EnsureDataLoaded();
        foreach (var progress in currentSaveData.levelProgress)
        {
            if (progress.gameMode == gameModeIndex)
            {
                if (levelIndex > progress.maxUnlockedLevel)
                {
                    progress.maxUnlockedLevel = levelIndex;
                    if (writeSaveImmediately)
                    {
                        SaveData();
                    }
                }
                return;
            }
        }
        currentSaveData.levelProgress.Add(new LevelProgress(gameModeIndex, levelIndex));
        if (writeSaveImmediately)
        {
            SaveData();
        }
    }

    public static int GetHighestClearedLevel(GameModeType gameModeIndex)
    {
        return GetHighestClearedLevel((int)gameModeIndex);
    }
    
    public static int GetHighestClearedLevel(int gameModeIndex)
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

    public static void AddUnlockedBall(string ballID)
    {
        EnsureDataLoaded();
        currentSaveData.unlockedBalls.Add(ballID);
        SaveData();
    }

    public static bool IsBallUnlocked(string ballID)
    {
        EnsureDataLoaded();
        return currentSaveData.unlockedBalls.Contains(ballID);
    }
    
    public static void SetStarCollected(int gameMode, int levelIndex, int starIndex, bool writeSaveImmediately = true)
    {
        string key = $"{gameMode}-{levelIndex}";

        if (!currentSaveData.collectedStars.ContainsKey(key))
        {
            currentSaveData.collectedStars[key] = 0;
        }
        currentSaveData.collectedStars[key] |= (1 << (starIndex));
        if (writeSaveImmediately)
        {
            SaveData();
        }
    }

    public static bool IsStarCollected(int gameMode, int levelIndex, int starIndex)
    {
        string key = $"{gameMode}-{levelIndex}";

        if (!currentSaveData.collectedStars.ContainsKey(key))
        {
            return false;
        }

        bool collected = (currentSaveData.collectedStars[key] & (1 << (starIndex))) != 0;
        return collected;
    }

    public static void GetStarsCollectedStatus(GameModeType gameMode, int levelIndex, out bool[] starStatus)
    {
        GetStarsCollectedStatus((int)gameMode, levelIndex, out starStatus);
    }
    
    public static void GetStarsCollectedStatus(int gameMode, int levelIndex, out bool[] starStatus)
    {
        starStatus = new bool[3];
        string key = $"{gameMode}-{levelIndex}";

        if (currentSaveData.collectedStars.ContainsKey(key))
        {
            int bitmask = currentSaveData.collectedStars[key];
            for (int i = 0; i < 3; i++)
            {
                starStatus[i] = (bitmask & (1 << i)) != 0;
            }
        }
    }
    
    public static int GetCollectedStarsCount(int gameMode, int levelIndex)
    {
        string key = $"{gameMode}-{levelIndex}";

        return currentSaveData.collectedStars.ContainsKey(key) ? CountBits(currentSaveData.collectedStars[key]) : 0;
    }

    private static int CountBits(int value)
    {
        int count = 0;
        while (value > 0)
        {
            count += value & 1;
            value >>= 1;
        }
        return count;
    }

    public static void SetMusicVolume(int newMusicVolume)
    {
        currentSaveData.musicVolume = newMusicVolume;
        SaveData();
    }
    
    public static int GetMusicVolume()
    {
        return currentSaveData.musicVolume;
    }
    
    public static int GetAmbientVolume()
    {
        return currentSaveData.musicVolume == 0 ? 100 : 0;
    }

    public static void SetSFXVolume(int newSFXVolume)
    {
        currentSaveData.sfxVolume = newSFXVolume;
        SaveData();
    }
    
    public static int GetSFXVolume()
    {
        return currentSaveData.sfxVolume;
    }
    
    private static string MakeFusionKey(string primaryID, string secondaryID)
    { 
        return $"{primaryID}+{secondaryID}";
    }
        
    public static void AddUnlockedFusion(string primaryID, string secondaryID)
    {
        EnsureDataLoaded();
        string key = MakeFusionKey(primaryID, secondaryID);
        if (!currentSaveData.unlockedFusions.Contains(key))
        {
            currentSaveData.unlockedFusions.Add(key);
            SaveData();
        }
    }
    
    public static bool IsFusionUnlocked(string primaryID, string secondaryID)
    {
        EnsureDataLoaded();
        string key = MakeFusionKey(primaryID, secondaryID);
        return currentSaveData.unlockedFusions.Contains(key);
    }
    
    public static void AddFusionToFavorites(string primaryID, string secondaryID)
    {
        EnsureDataLoaded();
        string key = MakeFusionKey(primaryID, secondaryID);

        if (currentSaveData.favoriteFusions.Contains(key)) return;

        currentSaveData.favoriteFusions[2] = currentSaveData.favoriteFusions[1];
        currentSaveData.favoriteFusions[1] = currentSaveData.favoriteFusions[0];
        currentSaveData.favoriteFusions[0] = key;

        SaveData();
    }
    
    public static bool IsFusionFavorited(string primaryID, string secondaryID)
    {
        EnsureDataLoaded();
        string key = MakeFusionKey(primaryID, secondaryID);
        return currentSaveData.favoriteFusions.Contains(key);
    }
    
    public static void SetSelectedFusion(string primaryID, string secondaryID)
    {
        EnsureDataLoaded();
        currentSaveData.selectedFusion = MakeFusionKey(primaryID, secondaryID);
        SaveData();
    }
    
    public static string GetSelectedFusion()
    {
        EnsureDataLoaded();
        return currentSaveData.selectedFusion;
    }
    
    public static void SetFusionEquipped(bool equipFusion)
    {
        EnsureDataLoaded();
        currentSaveData.isFusionEquipped = equipFusion;
        SaveData();
    }
    
    public static bool IsFusionEquipped()
    {
        EnsureDataLoaded();
        return currentSaveData.isFusionEquipped;
    }
    
    public static bool IsFusionUnlockedKey(string key)
    {
        return currentSaveData.unlockedFusions.Contains(key);
    }

    public static string GetFavoriteFusionAt(int index)
    {
        return currentSaveData.favoriteFusions[index];
    }

    public static void SetLastPlayedGauntletMode(int mode)
    {
        EnsureDataLoaded();
        currentSaveData.lastPlayedGauntletMode = mode;
        SaveData();
    }
    
    public static int GetLastPlayedGauntletMode()
    {
        EnsureDataLoaded();
        return currentSaveData.lastPlayedGauntletMode;
    }
    
    public static void RecordEndlessPlayed(int difficulty)
    {
        EnsureDataLoaded();
        var rec = GetEndlessRecord(difficulty);
        rec.numPlayed++;
        SaveData();
    }

    public static void RecordEndlessWin(int difficulty)
    {
        EnsureDataLoaded();
        var rec = GetEndlessRecord(difficulty);
        rec.numWon++;
        SaveData();
    }

    public static void RecordEndlessLoss(int difficulty)
    {
        EnsureDataLoaded();
        var rec = GetEndlessRecord(difficulty);
        rec.numLost++;
        SaveData();
    }

    public static EndlessModeRecord GetEndlessRecord(int difficulty)
    {
        EnsureDataLoaded();
        if (!currentSaveData.endlessRecords.TryGetValue(difficulty, out var rec))
        {
            rec = new EndlessModeRecord(difficulty);
            currentSaveData.endlessRecords[difficulty] = rec;
            SaveData();
        }
        return rec;
    }

    public static bool SpendSurplusPoints(int numPoints, bool writeSaveImmediately = true)
    {
        EnsureDataLoaded();
        if (currentSaveData.surplusPoints < numPoints)
            return false;

        currentSaveData.surplusPoints -= numPoints;
        if (writeSaveImmediately)
        {
            SaveData();
        }
        EventBus.Publish(new SurplusPointsSpentEvent(numPoints));
        return true;
    }

    public static void AddSurplusPoints(int numPoints, bool writeSaveImmediately = true)
    {
        EnsureDataLoaded();
        currentSaveData.surplusPoints += numPoints;
        if (writeSaveImmediately)
        {
            SaveData();
        }
        EventBus.Publish(new SurplusPointsEarnedEvent(numPoints));
    }

    public static int GetSurplusPoints()
    {
        EnsureDataLoaded();
        return currentSaveData.surplusPoints;
    }

    #endregion
}