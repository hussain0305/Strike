using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    public AudioSource musicSourceA;
    public AudioSource musicSourceB;
    public AudioSource sfxSource;
    public AudioSource ambientSource;
    
    public SoundLibrary soundLibrary;

    private AudioSource activeMusicSource;
    private HashSet<string> uniquePlayingSounds = new HashSet<string>();

    [Header("SFX Settings")]
    [SerializeField] private int sfxPoolSize = 10;
    private List<AudioSource> sfxSources;
    private int sfxIndex = 0;

    public enum AudioChannel { Music, SFX, Ambient }

    private void Awake()
    {
        activeMusicSource = musicSourceA;
        InitializeSFXPool();
    }

    private void InitializeSFXPool()
    {
        sfxSources = new List<AudioSource>(sfxPoolSize);
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
            src.playOnAwake = false;
            sfxSources.Add(src);
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        EventBus.Subscribe<SaveLoadedEvent>(OnSaveLoaded);
        
        EventBus.Subscribe<BallShotEvent>(BallShot);
        EventBus.Subscribe<CollectibleHitEvent>(CollectibleHit);
        EventBus.Subscribe<StarCollectedEvent>(StarCollected);
        EventBus.Subscribe<PromptToSelectLevelEvent>(PromptToSelectLevel);
        EventBus.Subscribe<ResultDeterminedEvent>(ResultDetermined);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        EventBus.Unsubscribe<SaveLoadedEvent>(OnSaveLoaded);
        
        EventBus.Unsubscribe<BallShotEvent>(BallShot);
        EventBus.Unsubscribe<CollectibleHitEvent>(CollectibleHit);
        EventBus.Unsubscribe<StarCollectedEvent>(StarCollected);
        EventBus.Unsubscribe<PromptToSelectLevelEvent>(PromptToSelectLevel);
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

    public void PlaySFX(Sound sound, bool allowOverlap = true)
    {
        if (!allowOverlap)
        {
            if (uniquePlayingSounds.Contains(sound.clip.name))
                return;
            
            uniquePlayingSounds.Add(sound.clip.name);
        }

        AudioSource source = sfxSources[sfxIndex];
        sfxIndex = (sfxIndex + 1) % sfxSources.Count;

        source.pitch = sound.clip == soundLibrary?.buttonHoverSFX.clip ? Random.Range(0.95f, 1.05f) : 1;
        source.PlayOneShot(sound.clip, sound.volume);

        if (!allowOverlap)
        {
            StartCoroutine(RemoveFromUniqueList(sound.clip));
        }
    }

    private IEnumerator RemoveFromUniqueList(AudioClip clip)
    {
        yield return new WaitForSeconds(clip.length);
        uniquePlayingSounds.Remove(clip.name);
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

    public AudioSource GetAvailableSFXSource()
    {
        AudioSource source = sfxSources[sfxIndex];
        sfxIndex = (sfxIndex + 1) % sfxSources.Count;
        return source;
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

    public void BallShot(BallShotEvent e)
    {
        PlaySFX(soundLibrary.ballShotSFX);
    }
        
    public void CollectibleHit(CollectibleHitEvent e)
    {
        switch (e.Type)
        {
            case CollectibleType.Points:
                if (e.Value > 0)
                    PlaySFX(soundLibrary.positiveHitSFX[Random.Range(0, soundLibrary.positiveHitSFX.Length)]);
                else
                    PlaySFX(soundLibrary.negativeHitSFX[Random.Range(0, soundLibrary.negativeHitSFX.Length)]);
                break;
            case CollectibleType.Multiple:
                PlaySFX(soundLibrary.positiveHitSFX[Random.Range(0, soundLibrary.positiveHitSFX.Length)]);
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

    public void PromptToSelectLevel(PromptToSelectLevelEvent e)
    {
        PlaySFX(soundLibrary.negativeHitSFX[0]);
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