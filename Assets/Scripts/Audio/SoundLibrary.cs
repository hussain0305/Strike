using UnityEngine;

[System.Serializable]
public struct Sound
{
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    
    public SFXPriority priority;
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
    public Sound menuActionPositive;
    public Sound menuActionNegative;
    
    [Header("Sound Effects - Game")]
    public Sound ballShotSFX;
    public Sound starPickupSFX;
    public Sound positiveHitSFX;
    public Sound negativeHitSFX;
    public Sound multiplierHitSFX;
    public Sound eliminationSFX;
    public Sound nextShotCuedSFX;
    public Sound ambientNoise;
    
    [Header("Ball Sound Effects - General")]
    public Sound ballBounceSFX;
    
    [Header("Ball Sound Effects - General")]
    public Sound pelletBounceSFX;

    [Header("Ball Sound Effects - General")]
    public Sound discoPelletsSpawnSFX;

    [Header("Sound Effects - Results")]
    public Sound wonGameSFX;
    public Sound lostGameSFX;
}