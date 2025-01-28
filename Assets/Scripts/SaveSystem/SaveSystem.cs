using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int stars = 0;
    public int[] unlockedGameModes = {0};
    public int selectedBall = 0;
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
}