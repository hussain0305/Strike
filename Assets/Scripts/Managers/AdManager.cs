using System;
using UnityEngine;

public static class AdManager
{
    public static bool IsPlayingRewardedAd = false;
    public static bool IsPlayingInterstitialAd = false;
    
    private static bool isInitialized = true;

    public static void Initialize()
    {
        isInitialized = true;
    }

    public static void ShowInterstitial(Action<bool> onClose)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("AdManager hasn't been initialized yet");
            onClose?.Invoke(false);
            return;
        }

        onClose?.Invoke(true);
    }

    public static void ShowRewardedAd(Action<bool> onReward)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("AdManager hasn't been initialized yet");
            onReward?.Invoke(false);
            return;
        }

        onReward?.Invoke(true);
    }
}
