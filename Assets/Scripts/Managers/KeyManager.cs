using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public static class KeyManager
{
    private static readonly string KeyPath = Application.persistentDataPath + "/straesKey.dat";
    private static readonly string IvPath = Application.persistentDataPath + "/straesIv.dat";

    public static void GenerateAndStoreKeys()
    {
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
    }

    public static (string key, string iv) LoadKeys()
    {
        if (File.Exists(KeyPath) && File.Exists(IvPath))
        {
            string key = File.ReadAllText(KeyPath);
            string iv = File.ReadAllText(IvPath);
            return (key, iv);
        }
        else
        {
            Debug.LogError("Keys not found! Ensure GenerateAndStoreKeys() is called on game start.");
            return (null, null);
        }
    }
}