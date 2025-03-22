using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPage : MonoBehaviour
{
    [Header("Audio Settings")]
    public Slider sfxSlider;
    public Slider musicSlider;
    public Button toggleSFX;
    public Button toggleMusic;
    public GameObject sfxOnImage;
    public GameObject sfxOffImage;
    public GameObject musicOnImage;
    public GameObject musicOffImage;
    public TextMeshProUGUI sfxOnText;
    public TextMeshProUGUI musicOnText;
    
    private int savedMusicVolume;
    private int savedSFXVolume;

    private void OnEnable()
    {
        EventBus.Subscribe<SaveLoadedEvent>(StartSetup);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        toggleMusic.onClick.AddListener(ToggleMusic);
        toggleSFX.onClick.AddListener(ToggleSFX);
        if (SaveManager.IsSaveLoaded)
        {
            StartSetup(null);
        }
    }

    private void OnDisable()
    {
        SaveManager.SetMusicVolume((int)musicSlider.value);
        SaveManager.SetSFXVolume((int)sfxSlider.value);
        EventBus.Unsubscribe<SaveLoadedEvent>(StartSetup);
        
        musicSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();
        toggleMusic.onClick.RemoveAllListeners();
        toggleSFX.onClick.RemoveAllListeners();
    }

    private void StartSetup(SaveLoadedEvent e)
    {
        savedMusicVolume = SaveManager.GetMusicVolume();
        savedSFXVolume = SaveManager.GetSFXVolume();

        musicSlider.value = savedMusicVolume;
        sfxSlider.value = savedSFXVolume;

        UpdateMusicUI(savedMusicVolume);
        UpdateSFXUI(savedSFXVolume);
    }

    private void SetMusicVolume(float value)
    {
        AudioManager.Instance.SetVolume(AudioManager.AudioChannel.Music, value);
        UpdateMusicUI((int)value);
    }

    private void SetSFXVolume(float value)
    {
        AudioManager.Instance.SetVolume(AudioManager.AudioChannel.SFX, value);
        UpdateSFXUI((int)value);
    }

    private void ToggleMusic()
    {
        if (musicSlider.value > 0)
        {
            savedMusicVolume = (int)musicSlider.value;
            musicSlider.value = 0;
        }
        else
        {
            musicSlider.value = savedMusicVolume;
        }
    }

    private void ToggleSFX()
    {
        if (sfxSlider.value > 0)
        {
            savedSFXVolume = (int)sfxSlider.value;
            sfxSlider.value = 0;
        }
        else
        {
            sfxSlider.value = savedSFXVolume;
        }
    }

    private void UpdateMusicUI(int val)
    {
        bool isOn = val > 0;
        musicOnText.text = $"{val}";
        musicOnImage.SetActive(isOn);
        musicOffImage.SetActive(!isOn);
    }

    private void UpdateSFXUI(int val)
    {
        bool isOn = val > 0;
        sfxOnText.text = $"{val}";
        sfxOnImage.SetActive(isOn);
        sfxOffImage.SetActive(!isOn);
    }
}
