using UnityEngine;

public static class WebGLSaveSystem
{
    public static void SaveGame(SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string encryptedJson = EncryptionUtils.Encrypt(json);
        PlayerPrefs.SetString("StrSaveData", encryptedJson);
        PlayerPrefs.Save();
    }

    public static SaveData LoadGame()
    {
        if (PlayerPrefs.HasKey("StrSaveData"))
        {
            string encryptedJson = PlayerPrefs.GetString("StrSaveData");
            string json = EncryptionUtils.Decrypt(encryptedJson);
            return JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            Debug.LogWarning("No save data found in PlayerPrefs, returning default values.");
            return new SaveData();
        }
    }
}