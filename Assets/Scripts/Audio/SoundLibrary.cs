using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip levelMusic;

    [Header("Sound Effects - UI")]
    public AudioClip buttonHoverSFX;
    public AudioClip buttonClickSFX;
    public AudioClip backButtonClickSFX;
    public AudioClip successfulActionSFX;
    public AudioClip unsuccessfulActionSFX;
    
    [Header("Sound Effects - Game")]
    public AudioClip hitEffectSFX;
    public AudioClip jumpSFX;
    public AudioClip starPickupSFX;
}