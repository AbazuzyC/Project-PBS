using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public GameObject settingsPanel;
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private Slider _musicSlider; 
    [SerializeField] private Slider _sfxSlider;

    void Awake()
    {
        SetMusicVolume();
        SetSFXVolume();
    }
    public void SetMusicVolume()
    {
        float volume = _musicSlider.value;
        float normalized = Mathf.Max(volume, 0.0001f);
        _audioMixer.SetFloat("MusicVolume", Mathf.Log10(normalized) * 20);
        PlayerPrefs.SetFloat("Music", volume);
    }
    public void SetSFXVolume()
    {
        float volume = _sfxSlider.value;
        float normalized = Mathf.Max(volume, 0.0001f);
        _audioMixer.SetFloat("SFXVolume", Mathf.Log10(normalized) * 20);
        PlayerPrefs.SetFloat("SFX", volume);
    }
}
