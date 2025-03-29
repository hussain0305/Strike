using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioMixer audioMixer;

    public AudioSource musicSourceA;
    public AudioSource musicSourceB;
    public AudioSource sfxSource;

    public SoundLibrary soundLibrary;

    private AudioSource activeMusicSource;
    private HashSet<string> uniquePlayingSounds = new HashSet<string>();

    [Header("SFX Settings")]
    [SerializeField] private int sfxPoolSize = 10;
    private List<AudioSource> sfxSources;
    private int sfxIndex = 0;

    public enum AudioChannel { Music, SFX }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        activeMusicSource = musicSourceA;

        InitializeSFXPool();
    }

    private void InitializeSFXPool()
    {
        sfxSources = new List<AudioSource>();
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
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        EventBus.Unsubscribe<SaveLoadedEvent>(OnSaveLoaded);
    }

    public void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 1.0f)
    {
        if (activeMusicSource.clip == clip) return;

        AudioSource newSource = activeMusicSource == musicSourceA ? musicSourceB : musicSourceA;
        newSource.clip = clip;
        newSource.loop = loop;
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

    public void PlaySFX(AudioClip clip, bool allowOverlap = true)
    {
        if (!allowOverlap)
        {
            if (uniquePlayingSounds.Contains(clip.name)) return;
            uniquePlayingSounds.Add(clip.name);
        }

        AudioSource source = sfxSources[sfxIndex];
        sfxIndex = (sfxIndex + 1) % sfxSources.Count;
        source.PlayOneShot(clip);

        if (!allowOverlap)
        {
            StartCoroutine(RemoveFromUniqueList(clip));
        }
    }

    private IEnumerator RemoveFromUniqueList(AudioClip clip)
    {
        yield return new WaitForSeconds(clip.length);
        uniquePlayingSounds.Remove(clip.name);
    }

    public void SetVolume(AudioChannel channel, float volume)
    {
        string param = channel == AudioChannel.Music ? "MusicVolume" : "SFXVolume";

        float dB = (volume > 0) ? Mathf.Lerp(-30, 0, volume / 100) : -80;
        audioMixer.SetFloat(param, dB);
    }

    public void OnGameStateChanged(GameStateChangedEvent e)
    {
        AudioClip clip = null;

        switch (e.gameState)
        {
            case GameState.Menu:
                clip = soundLibrary.menuMusic;
                break;
            case GameState.InGame:
                clip = soundLibrary.levelMusic;
                break;
        }

        if (clip != null)
        {
            PlayMusic(clip);
        }
    }

    public void OnSaveLoaded(SaveLoadedEvent e)
    {
        SetVolume(AudioChannel.Music, SaveManager.GetMusicVolume());
        SetVolume(AudioChannel.SFX, SaveManager.GetSFXVolume());
    }

    public void PlaySuccessfulActionSFX()
    {
        PlaySFX(soundLibrary.successfulActionSFX, false);
    }

    public void PlayUnsuccessfulActionSFX()
    {
        PlaySFX(soundLibrary.unsuccessfulActionSFX, false);
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
}