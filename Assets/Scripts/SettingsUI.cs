using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsUi : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer _audioMixer;
    
    [Header("UI Toggles")]
    [SerializeField] private Toggle _musicToggle;
    [SerializeField] private Toggle _sfxToggle;

    void Start()
    {
        // Muat pengaturan yang tersimpan (1 untuk nyala, 0 untuk mati)
        bool isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        bool isSfxOn = PlayerPrefs.GetInt("SFXOn", 1) == 1;

        // Set state awal toggle dan tambahkan event listener
        if (_musicToggle != null)
        {
            // Set isOn tanpa trigger event (untuk inisialisasi)
            _musicToggle.SetIsOnWithoutNotify(isMusicOn);
            _musicToggle.onValueChanged.AddListener(SetMusic);
        }

        if (_sfxToggle != null)
        {
            _sfxToggle.SetIsOnWithoutNotify(isSfxOn);
            _sfxToggle.onValueChanged.AddListener(SetSFX);
        }

        // Terapkan pengaturan audio awal
        ApplyMusicSettings(isMusicOn);
        ApplySFXSettings(isSfxOn);
    }

    public void SetMusic(bool isOn)
    {
        PlayerPrefs.SetInt("MusicOn", isOn ? 1 : 0);
        ApplyMusicSettings(isOn);
    }

    public void SetSFX(bool isOn)
    {
        PlayerPrefs.SetInt("SFXOn", isOn ? 1 : 0);
        ApplySFXSettings(isOn);
    }

    private void ApplyMusicSettings(bool isOn)
    {
        if (_audioMixer != null)
        {
            // Nilai volume mixer biasanya 0 (normal) hingga -80 (mute)
            _audioMixer.SetFloat("MusicVolume", isOn ? 0f : -80f);
        }
    }

    private void ApplySFXSettings(bool isOn)
    {
        if (_audioMixer != null)
        {
            _audioMixer.SetFloat("SFXVolume", isOn ? 0f : -80f);
        }
    }
}
