using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class LevelProgress
{
    public int gameMode;
    public int maxUnlockedLevel;

    public LevelProgress(int gameMode, int maxUnlockedLevel)
    {
        this.gameMode = gameMode;
        this.maxUnlockedLevel = maxUnlockedLevel;
    }
}

[System.Serializable]
public class CollectedStarEntry
{
    public string key;
    public int value;

    public CollectedStarEntry(string key, int value)
    {
        this.key = key;
        this.value = value;
    }
}

[System.Serializable]
public class SaveData
{
    public int stars = 0;
    public int sfxVolume = 0;
    public int musicVolume = 0;
    public int[] unlockedGameModes = {0};
    public string selectedBall = "soccBall";
    public List<string> unlockedBalls = new() { "soccBall" };
    public Dictionary<string, int> collectedStars = new Dictionary<string, int>();
    public List<LevelProgress> levelProgress = new List<LevelProgress> { new LevelProgress(0, 0) };

    public List<CollectedStarEntry> collectedStarsList = new List<CollectedStarEntry>();
    public void SyncDictionaryToList()
    {
        collectedStarsList = collectedStars.Select(kvp => new CollectedStarEntry(kvp.Key, kvp.Value)).ToList();
    }
    public void SyncListToDictionary()
    {
        collectedStars = collectedStarsList.ToDictionary(entry => entry.key, entry => entry.value);
    }
}

public static class SaveSystem
{
    public static bool IsSaveLoaded { get; private set; }
    
    public static string GetSavePath()
    {
        return Application.persistentDataPath + "/strsavefile.json";
    }

    public static void SaveGame(SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string encryptedJson = EncryptionUtils.Encrypt(json);
        
        File.WriteAllText(GetSavePath(), encryptedJson);
    }

    public static SaveData LoadGame()
    {
        string path = GetSavePath();
        if (File.Exists(path))
        {
            string encryptedJson = File.ReadAllText(path);
            string json = EncryptionUtils.Decrypt(encryptedJson);
            return JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            Debug.LogWarning("Save file not found, returning default data.");
            return new SaveData();
        }
    }
    
    public static async Task<SaveData> LoadGameAsync()
    {
        string path = GetSavePath();
        if (File.Exists(path))
        {
            return await Task.Run(() =>
            {
                string encryptedJson = File.ReadAllText(path);
                string json = EncryptionUtils.Decrypt(encryptedJson);
                return JsonUtility.FromJson<SaveData>(json);
            });
        }
        else
        {
            Debug.LogWarning("Save file not found, returning default data.");
            return new SaveData();
        }
    }
}