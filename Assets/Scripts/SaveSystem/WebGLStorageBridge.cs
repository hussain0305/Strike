using System.Runtime.InteropServices;
using UnityEngine;

public static class WebGLStorageBridge
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SaveToLocalStorage(string key, string value);

    [DllImport("__Internal")]
    private static extern System.IntPtr LoadFromLocalStorage(string key);
#endif

    public static void Save(string key, string value)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SaveToLocalStorage(key, value);
#else
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
#endif
    }

    public static string Load(string key)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        var ptr = LoadFromLocalStorage(key);
        if (ptr == System.IntPtr.Zero) return null;
        return Marshal.PtrToStringAuto(ptr);
#else
        return PlayerPrefs.GetString(key, null);
#endif
    }
}