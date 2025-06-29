using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public static class KeyManager
{
    private static readonly string KeyPath = Application.persistentDataPath + "/straesKey.dat";
    private static readonly string IvPath = Application.persistentDataPath + "/straesIv.dat";
    private static readonly string KeyStorageKey = "StrAESKey";
    private static readonly string IvStorageKey = "StrAESIv";
    
    public static void GenerateAndStoreKeys()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string key = WebGLStorageBridge.Load(KeyStorageKey);
        string iv = WebGLStorageBridge.Load(IvStorageKey);
        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(iv))
            return;

        using (Aes aes = Aes.Create())
        {
            aes.GenerateKey();
            aes.GenerateIV();

            string encodedKey = Convert.ToBase64String(aes.Key);
            string encodedIv = Convert.ToBase64String(aes.IV);

            WebGLStorageBridge.Save(KeyStorageKey, encodedKey);
            WebGLStorageBridge.Save(IvStorageKey, encodedIv);
        }
#else
        if (!File.Exists(KeyPath) || !File.Exists(IvPath))
        {
            using (Aes aes = Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();
                
                string key = Convert.ToBase64String(aes.Key);
                string iv = Convert.ToBase64String(aes.IV);

                File.WriteAllText(KeyPath, key);
                File.WriteAllText(IvPath, iv);
            }
        }
#endif
    }

    public static (string key, string iv) LoadKeys()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string key = WebGLStorageBridge.Load(KeyStorageKey);
        string iv = WebGLStorageBridge.Load(IvStorageKey);
        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(iv))
            return (key, iv);

        Debug.LogError("[WebGL Save] Keys not found!");
        return (null, null);
#else
        if (File.Exists(KeyPath) && File.Exists(IvPath))
        {
            string key = File.ReadAllText(KeyPath);
            string iv = File.ReadAllText(IvPath);
            return (key, iv);
        }
        Debug.LogError("Keys not found! Ensure GenerateAndStoreKeys() is called on game start.");
        return (null, null);
#endif
    }
}