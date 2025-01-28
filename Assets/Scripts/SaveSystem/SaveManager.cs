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
    public static bool IsSaveLoaded { get; private set; } = false;
    
    static SaveManager()
    {
        KeyManager.GenerateAndStoreKeys();
        // LoadData();
    }

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

    public static async void LoadData()
    {
        string savePath = SaveSystem.GetSavePath(); // Access Unity API on the main thread

        if (File.Exists(savePath))
        {
            await Task.Run(() =>
            {
                string encryptedJson = File.ReadAllText(savePath);
                string json = EncryptionUtils.Decrypt(encryptedJson);
                currentSaveData = JsonUtility.FromJson<SaveData>(json);
            });
        }
        else
        {
            Debug.LogWarning("Save file not found, returning default data.");
            currentSaveData = new SaveData();
        }

        Debug.Log("IsSaveLoaded set to true");
        IsSaveLoaded = true;
        CoroutineDispatcher.Instance.RunCoroutine(DelayedSaveFileLoadedBroadcast());
    }

    public static IEnumerator DelayedSaveFileLoadedBroadcast()
    {
        yield return new WaitForSeconds(0.5f);
        OnSaveFileLoaded?.Invoke();
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
}