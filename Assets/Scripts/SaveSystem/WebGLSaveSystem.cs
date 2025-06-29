using UnityEngine;

public static class WebGLSaveSystem
{
    public static void SaveGame(SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string encryptedJson = EncryptionUtils.Encrypt(json);

        string safeString = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(encryptedJson));
        WebGLStorageBridge.Save("StrSaveData", safeString);
    }

    public static SaveData LoadGame()
    {
        string safeString = WebGLStorageBridge.Load("StrSaveData");

        if (string.IsNullOrEmpty(safeString))
        {
            return new SaveData();
        }
        
        try
        {
            string encryptedJson = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(safeString));
            string json = EncryptionUtils.Decrypt(encryptedJson);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (System.Exception ex)
        {
            return new SaveData();
        }
    }
}