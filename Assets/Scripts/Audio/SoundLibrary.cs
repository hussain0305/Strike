using UnityEngine;

[System.Serializable]
public struct Sound
{
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
}

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    [Header("Music")]
    public Sound menuMusic;
    public Sound[] levelMusic;

    [Header("Sound Effects - UI")]
    public Sound buttonHoverSFX;
    public Sound buttonClickSFX;
    public Sound backButtonClickSFX;
    
    [Header("Sound Effects - Game")]
    public Sound ballShotSFX;
    public Sound starPickupSFX;
    public Sound[] positiveHitSFX;
    public Sound[] negativeHitSFX;
    public Sound eliminationSFX;
    public Sound forcePadSFX;
    public Sound nextShotCuedSFX;
    public Sound ambientNoise;
    
    [Header("Sound Effects - Results")]
    public Sound wonGameSFX;
    public Sound lostGameSFX;
}