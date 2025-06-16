using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public enum SFXType{ Unclassified, PelletBounce, DiscoPelletSpawn, MenuActionPositive, MenuActionNegative}

public enum SFXPriority
{
    None = 0,
    VeryLow = 1,
    Low = 2,
    Medium = 3,
    High = 4,
    VeryHigh = 5
}

public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    public AudioSource musicSourceA;
    public AudioSource musicSourceB;
    public AudioSource sfxSource;
    public AudioSource ambientSource;
    
    public SoundLibrary soundLibrary;

    private AudioSource activeMusicSource;

    [Header("SFX Settings")]
    [SerializeField] private int sfxPoolSize = 20;
    private Queue<AudioSource> inactiveSources;
    private Dictionary<SFXPriority, List<AudioSource>> activeSources;

    public enum AudioChannel { Music, SFX, Ambient }

    private int numPositiveHitsThisTurn = 0;
    private int numNegativeHitsThisTurn = 0;
    
    private void Awake()
    {
        activeMusicSource = musicSourceA;
        InitializeSFXPool();
    }

    private void InitializeSFXPool()
    {
        activeSources = new Dictionary<SFXPriority, List<AudioSource>>();
        foreach (SFXPriority sfxPriority in Enum.GetValues(typeof(SFXPriority)))
        {
            activeSources.Add(sfxPriority, new List<AudioSource>());
        }
        
        inactiveSources = new Queue<AudioSource>(sfxPoolSize);
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
            src.playOnAwake = false;
            inactiveSources.Enqueue(src);
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        EventBus.Subscribe<SaveLoadedEvent>(OnSaveLoaded);
        
        EventBus.Subscribe<PlaySoundEvent>(PlaySoundEventReceived);
        EventBus.Subscribe<NextShotCuedEvent>(NextShotCued);
        EventBus.Subscribe<BallShotEvent>(BallShot);
        EventBus.Subscribe<CollectibleHitEvent>(CollectibleHit);
        EventBus.Subscribe<StarCollectedEvent>(StarCollected);
        EventBus.Subscribe<ResultDeterminedEvent>(ResultDetermined);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        EventBus.Unsubscribe<SaveLoadedEvent>(OnSaveLoaded);
        
        EventBus.Unsubscribe<PlaySoundEvent>(PlaySoundEventReceived);
        EventBus.Unsubscribe<NextShotCuedEvent>(NextShotCued);
        EventBus.Unsubscribe<BallShotEvent>(BallShot);
        EventBus.Unsubscribe<CollectibleHitEvent>(CollectibleHit);
        EventBus.Unsubscribe<StarCollectedEvent>(StarCollected);
        EventBus.Unsubscribe<ResultDeterminedEvent>(ResultDetermined);
    }
    
    public void PlayMusic(Sound music, bool loop = true, float fadeDuration = 1.0f)
    {
        if (activeMusicSource.clip == music.clip)
            return;

        AudioSource newSource = activeMusicSource == musicSourceA ? musicSourceB : musicSourceA;
        newSource.clip = music.clip;
        newSource.loop = loop;
        newSource.volume = music.volume;
        newSource.Play();

        StartCoroutine(CrossfadeMusic(activeMusicSource, newSource, fadeDuration));
        activeMusicSource = newSource;
    }

    private IEnumerator CrossfadeMusic(AudioSource from, AudioSource to, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            float t = time / duration;
            from.volume = Mathf.Lerp(1, 0, t);
            to.volume = Mathf.Lerp(0, 1, t);
            time += Time.deltaTime;
            yield return null;
        }
        from.Stop();
        from.volume = 1;
    }

    public void PlaySFX(Sound sound, float pitch = 1, float volume = 1)
    {
        if (!GetAvailableSFXSource(sound.priority, out AudioSource source))
            return;

        if (sound.clip == soundLibrary?.buttonHoverSFX.clip)
        {
            pitch = Random.Range(0.95f, 1.05f);
        }
        source.pitch = pitch;
        source.PlayOneShot(sound.clip, sound.volume * volume);
    }
    
    public void SetVolume(AudioChannel channel, float volume)
    {
        string param = "";
        switch (channel)
        {
            case AudioChannel.Music:
                param = "MusicVolume";
                break;
            
            case AudioChannel.SFX:
                param = "SFXVolume";
                break;
            
            case AudioChannel.Ambient:
                param = "AmbientVolume";
                break;
            
            default:
                return;
        }
        float dB = (volume > 0) ? Mathf.Lerp(-30, 0, volume / 100) : -80;
        audioMixer.SetFloat(param, dB);
    }
    
    public void OnSaveLoaded(SaveLoadedEvent e)
    {
        SetVolume(AudioChannel.Music, SaveManager.GetMusicVolume());
        SetVolume(AudioChannel.SFX, SaveManager.GetSFXVolume());
        SetVolume(AudioChannel.Ambient, SaveManager.GetAmbientVolume());
    }

    public bool GetAvailableSFXSource(SFXPriority priority, out AudioSource source)
    {
        if (inactiveSources.Count > 0)
        {
            source = inactiveSources.Dequeue();
            activeSources[priority].Add(source); 
            return true;
        }
        
        CleanupSFXDictionaryForInactiveSources();

        if (inactiveSources.Count > 0)
        {
            source = inactiveSources.Dequeue();
            activeSources[priority].Add(source); 
            return true;
        }

        foreach (var sourcePriority in activeSources.Keys)
        {
            if (sourcePriority < priority && activeSources[sourcePriority] != null && activeSources[sourcePriority].Count > 0)
            {
                for (int i = 0; i < activeSources[sourcePriority].Count; i++)
                {
                    if (!activeSources[sourcePriority][i].isPlaying)
                    {
                        source = activeSources[sourcePriority][i];
                        activeSources[sourcePriority].Remove(source); 
                        activeSources[priority].Add(source);
                        return true;
                    }
                }
            }
        }

        source = null;
        return false;
    }

    public void CleanupSFXDictionaryForInactiveSources()
    {
        foreach (var sources in activeSources.Values)
        {
            AudioSource[] sourcesArray = sources.ToArray();
            for (int i = 0; i < sourcesArray.Length; i++)
            {
                if (!sourcesArray[i].isPlaying)
                {
                    inactiveSources.Enqueue(sourcesArray[i]);
                    sources.Remove(sourcesArray[i]);
                }
            }
        }
    }

    public void CleanupSFXDictionaryCompletely()
    {
        foreach (var sources in activeSources.Values)
        {
            AudioSource[] sourcesArray = sources.ToArray();
            for (int i = 0; i < sourcesArray.Length; i++)
            {
                inactiveSources.Enqueue(sourcesArray[i]);
                sources.Remove(sourcesArray[i]);
            }
        }
    }

    public IEnumerator ResetPitchAfter(float delay, AudioSource source)
    {
        yield return new WaitForSeconds(delay);
        source.pitch = 1f;
    }

    public void PlayAmbientNoise()
    {
        ambientSource.clip = soundLibrary.ambientNoise.clip;
        ambientSource.volume = soundLibrary.ambientNoise.volume;
        ambientSource.loop = true;
        ambientSource.Play();

        SetVolume(AudioChannel.Ambient, 100f);
    }
    
    public void StopAmbientNoise()
    {
        SetVolume(AudioChannel.Ambient, 0f);

        if (ambientSource.isPlaying)
            ambientSource.Pause();
    }
    
#region Playing Specific Event Sounds

    public void PlaySoundEventReceived(PlaySoundEvent e)
    {
        switch (e.Type)
        {
            case SFXType.PelletBounce:
                PlaySFX(soundLibrary.pelletBounceSFX, e.Pitch, e.Volume);
                break;
            case SFXType.DiscoPelletSpawn:
                PlaySFX(soundLibrary.discoPelletsSpawnSFX, e.Pitch, e.Volume);
                break;
            
            case SFXType.MenuActionPositive:
                PlaySFX(soundLibrary.menuActionPositive);
                break;
            case SFXType.MenuActionNegative:
                PlaySFX(soundLibrary.menuActionNegative);
                break;
        }
    }

    public void NextShotCued(NextShotCuedEvent e)
    {
        CleanupSFXDictionaryCompletely();
        PlaySFX(soundLibrary.nextShotCuedSFX);
    }

    public void BallShot(BallShotEvent e)
    {
        numNegativeHitsThisTurn = 0;
        numPositiveHitsThisTurn = 0;
        PlaySFX(soundLibrary.ballShotSFX);
    }
        
    public void CollectibleHit(CollectibleHitEvent e)
    {
        switch (e.Type)
        {
            case CollectibleType.Points:
                if (e.Value > 0)
                {
                    numPositiveHitsThisTurn++;
                    PlaySFX(soundLibrary.positiveHitSFX, 
                        0.8f + (numPositiveHitsThisTurn * 0.04f),
                        0.75f + (numPositiveHitsThisTurn * 0.05f));
                }
                else
                {
                    numNegativeHitsThisTurn++;
                    PlaySFX(soundLibrary.negativeHitSFX, 
                        0.8f + (numNegativeHitsThisTurn * 0.04f),
                        0.75f + (numNegativeHitsThisTurn * 0.05f));
                        ;
                }
                break;
            case CollectibleType.Multiple:
                PlaySFX(soundLibrary.multiplierHitSFX);
                break;
            case CollectibleType.Danger:
                PlaySFX(soundLibrary.eliminationSFX);
                break;
        }
    }
        
    public void StarCollected(StarCollectedEvent e)
    {
        PlaySFX(soundLibrary.starPickupSFX);
    }

    public void OnGameStateChanged(GameStateChangedEvent e)
    {
        StopAmbientNoise();

        Sound? music = null;

        switch (e.gameState)
        {
            case GameState.Menu:
                music = soundLibrary.menuMusic;
                break;
            case GameState.InGame:
                music = soundLibrary.levelMusic[Random.Range(0, soundLibrary.levelMusic.Length)];
                // if (SaveManager.GetMusicVolume() == 0 && SaveManager.GetSFXVolume() != 0)
                //     PlayAmbientNoise();
                // else
                //     StopAmbientNoise();
                break;
        }

        if (music != null)
        {
            PlayMusic((Sound)music);
        }
    }

    public void ResultDetermined(ResultDeterminedEvent e)
    {
        StopAmbientNoise();
        if (!e.IsPlayingSolo || (e.IsPlayingSolo && e.Won))
        {
            PlaySFX(soundLibrary.wonGameSFX);
        }
        else
        {
            PlaySFX(soundLibrary.lostGameSFX);
        }
    }
    
#endregion
}