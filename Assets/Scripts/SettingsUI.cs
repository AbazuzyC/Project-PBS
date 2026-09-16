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

    private void Awake()
    {
        AutoFindToggles();
        SetupPanelNotifier();
    }

    private void OnEnable()
    {
        AutoFindToggles();
        SyncTogglesWithSettings();
    }

    private void Start()
    {
        AutoFindToggles();
        SetupPanelNotifier();
        RegisterToggleListeners();
        SyncTogglesWithSettings();
    }

    private void AutoFindToggles()
    {
        if (_musicToggle != null && _sfxToggle != null) return;

        Toggle[] toggles = GetComponentsInChildren<Toggle>(true);
        foreach (var t in toggles)
        {
            string lowerName = t.gameObject.name.ToLower();
            if (_musicToggle == null && (lowerName.Contains("musik") || lowerName.Contains("music") || lowerName.Contains("bgm")))
            {
                _musicToggle = t;
            }
            else if (_sfxToggle == null && (lowerName.Contains("suara") || lowerName.Contains("sfx") || lowerName.Contains("sound")))
            {
                _sfxToggle = t;
            }
        }

        // Fallback jika nama tidak spesifik
        if (toggles.Length >= 2)
        {
            if (_sfxToggle == null && _musicToggle != toggles[0]) _sfxToggle = toggles[0];
            if (_musicToggle == null && _sfxToggle != toggles[1]) _musicToggle = toggles[1];
        }
    }

    private void SetupPanelNotifier()
    {
        Transform panelTransform = null;
        if (_musicToggle != null)
        {
            panelTransform = _musicToggle.transform.parent;
        }
        else
        {
            Transform t = transform.Find("Setting");
            if (t != null) panelTransform = t;
        }

        if (panelTransform != null)
        {
            var notifier = panelTransform.GetComponent<SettingsPanelNotifier>();
            if (notifier == null)
            {
                notifier = panelTransform.gameObject.AddComponent<SettingsPanelNotifier>();
            }
            notifier.OnPanelEnabled = () =>
            {
                SyncTogglesWithSettings();
            };
        }
    }

    private void RegisterToggleListeners()
    {
        if (_musicToggle != null)
        {
            _musicToggle.onValueChanged.RemoveListener(SetMusic);
            _musicToggle.onValueChanged.AddListener(SetMusic);
        }

        if (_sfxToggle != null)
        {
            _sfxToggle.onValueChanged.RemoveListener(SetSFX);
            _sfxToggle.onValueChanged.AddListener(SetSFX);
        }
    }

    public void SyncTogglesWithSettings()
    {
        bool isMusicOn = AudioManager.Instance != null 
            ? AudioManager.Instance.IsMusicOn 
            : (PlayerPrefs.GetInt("MusicOn", 1) == 1);

        bool isSfxOn = AudioManager.Instance != null 
            ? AudioManager.Instance.IsSFXOn 
            : (PlayerPrefs.GetInt("SFXOn", 1) == 1);

        if (_musicToggle != null)
        {
            _musicToggle.SetIsOnWithoutNotify(isMusicOn);
            if (_musicToggle.graphic != null)
            {
                _musicToggle.graphic.canvasRenderer.SetAlpha(isMusicOn ? 1f : 0f);
            }
        }

        if (_sfxToggle != null)
        {
            _sfxToggle.SetIsOnWithoutNotify(isSfxOn);
            if (_sfxToggle.graphic != null)
            {
                _sfxToggle.graphic.canvasRenderer.SetAlpha(isSfxOn ? 1f : 0f);
            }
        }

        ApplyMusicSettings(isMusicOn);
        ApplySFXSettings(isSfxOn);
    }

    public void SetMusic(bool isOn)
    {
        PlayerPrefs.SetInt("MusicOn", isOn ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMusicSettings(isOn);

        if (isOn && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
        }
    }

    public void SetSFX(bool isOn)
    {
        PlayerPrefs.SetInt("SFXOn", isOn ? 1 : 0);
        PlayerPrefs.Save();
        ApplySFXSettings(isOn);

        // Feedback suara klik saat menyalakan kembali SFX
        if (isOn && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayClick();
        }
    }

    private void ApplyMusicSettings(bool isOn)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicEnabled(isOn);
        }

        if (_audioMixer != null)
        {
            _audioMixer.SetFloat("MusicVolume", isOn ? 0f : -80f);
        }
    }

    private void ApplySFXSettings(bool isOn)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXEnabled(isOn);
        }

        if (_audioMixer != null)
        {
            _audioMixer.SetFloat("SFXVolume", isOn ? 0f : -80f);
        }
    }
}

public class SettingsPanelNotifier : MonoBehaviour
{
    public Action OnPanelEnabled;

    private void OnEnable()
    {
        OnPanelEnabled?.Invoke();
    }
}
