using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }

    [Header("Audio Sources (Hanya butuh 2)")]
    [Tooltip("AudioSource untuk Background Music (Looping)")]
    [SerializeField] private AudioSource bgmSource;
    [Tooltip("AudioSource untuk Sound Effects (Memakai PlayOneShot)")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Mixer (Opsional)")]
    [Tooltip("Tarik group Music & SFX dari MainMixer ke sini jika menggunakan AudioMixer")]
    [SerializeField] private UnityEngine.Audio.AudioMixerGroup musicMixerGroup;
    [SerializeField] private UnityEngine.Audio.AudioMixerGroup sfxMixerGroup;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip defaultBGM;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip correctClip;
    [SerializeField] private AudioClip doneClip;
    [SerializeField] private AudioClip popClip;
    [SerializeField] private AudioClip wrongClip;

    [Header("Settings")]
    [SerializeField] private bool playBgmOnStart = true;

    public bool IsMusicOn { get; private set; } = true;
    public bool IsSFXOn { get; private set; } = true;

    private void Awake()
    {
        // Singleton pattern: Pastikan hanya ada 1 AudioManager dan tidak hancur saat ganti scene
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Auto-setup AudioSource jika lupa di-assign di Inspector
        SetupAudioSources();

        // Muat pengaturan awal dari PlayerPrefs
        LoadAudioSettings();
    }

    private void Start()
    {
        LoadAudioSettings();

        if (playBgmOnStart && defaultBGM != null)
        {
            if (IsMusicOn)
            {
                PlayBGM(defaultBGM);
            }
            else
            {
                bgmSource.clip = defaultBGM;
                bgmSource.mute = true;
            }
        }
    }

    private void SetupAudioSources()
    {
        // Jika bgmSource belum dipasang, buat otomatis
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        // Jika sfxSource belum dipasang, buat otomatis
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        if (musicMixerGroup != null)
        {
            bgmSource.outputAudioMixerGroup = musicMixerGroup;
        }

        if (sfxMixerGroup != null)
        {
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        }
    }

    #region Audio Settings & PlayerPrefs
    public void LoadAudioSettings()
    {
        IsMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        IsSFXOn = PlayerPrefs.GetInt("SFXOn", 1) == 1;

        ApplyAudioSettings();
    }

    public void ApplyAudioSettings()
    {
        if (bgmSource != null)
        {
            bgmSource.mute = !IsMusicOn;
        }

        if (sfxSource != null)
        {
            sfxSource.mute = !IsSFXOn;
        }

        if (musicMixerGroup != null && musicMixerGroup.audioMixer != null)
        {
            musicMixerGroup.audioMixer.SetFloat("MusicVolume", IsMusicOn ? 0f : -80f);
        }

        if (sfxMixerGroup != null && sfxMixerGroup.audioMixer != null)
        {
            sfxMixerGroup.audioMixer.SetFloat("SFXVolume", IsSFXOn ? 0f : -80f);
        }
    }

    public void SetMusicEnabled(bool isEnabled)
    {
        IsMusicOn = isEnabled;
        PlayerPrefs.SetInt("MusicOn", isEnabled ? 1 : 0);
        PlayerPrefs.Save();

        if (bgmSource != null)
        {
            bgmSource.mute = !isEnabled;
            if (isEnabled)
            {
                if (!bgmSource.isPlaying)
                {
                    if (bgmSource.clip != null)
                    {
                        bgmSource.UnPause();
                        if (!bgmSource.isPlaying) bgmSource.Play();
                    }
                    else if (defaultBGM != null)
                    {
                        PlayBGM(defaultBGM);
                    }
                }
            }
            else
            {
                bgmSource.Pause();
            }
        }

        if (musicMixerGroup != null && musicMixerGroup.audioMixer != null)
        {
            musicMixerGroup.audioMixer.SetFloat("MusicVolume", isEnabled ? 0f : -80f);
        }
    }

    public void SetSFXEnabled(bool isEnabled)
    {
        IsSFXOn = isEnabled;
        PlayerPrefs.SetInt("SFXOn", isEnabled ? 1 : 0);
        PlayerPrefs.Save();

        if (sfxSource != null)
        {
            sfxSource.mute = !isEnabled;
        }

        if (sfxMixerGroup != null && sfxMixerGroup.audioMixer != null)
        {
            sfxMixerGroup.audioMixer.SetFloat("SFXVolume", isEnabled ? 0f : -80f);
        }
    }

    public void ToggleMusic()
    {
        SetMusicEnabled(!IsMusicOn);
    }

    public void ToggleSFX()
    {
        SetSFXEnabled(!IsSFXOn);
    }
    #endregion

    #region BGM Controls
    /// <summary>
    /// Memutar BGM. Jika tidak ada parameter clip, memutar defaultBGM.
    /// </summary>
    public void PlayBGM(AudioClip musicClip = null)
    {
        AudioClip clipToPlay = musicClip != null ? musicClip : defaultBGM;
        if (clipToPlay == null) return;
        if (bgmSource == null) return;

        bgmSource.mute = !IsMusicOn;

        // Jangan restart BGM jika lagu yang sama sedang diputar
        if (bgmSource.clip == clipToPlay && bgmSource.isPlaying) return;

        bgmSource.clip = clipToPlay;
        bgmSource.loop = true;
        bgmSource.Play();

        if (!IsMusicOn)
        {
            bgmSource.Pause();
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    public void PauseBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (bgmSource != null && !bgmSource.isPlaying && IsMusicOn)
        {
            bgmSource.UnPause();
        }
    }
    #endregion

    #region SFX Controls (Mudah dipanggil dari Inspector OnClick & Script)
    /// <summary>
    /// Memutar SFX acuan bebas dengan volume tertentu.
    /// Menggunakan PlayOneShot agar suara tidak saling memotong meski hanya pakai 1 AudioSource SFX!
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (!IsSFXOn) return;

        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    // --- Fungsi tanpa parameter: Sangat mudah dimasukkan ke OnClick() UI Button di Inspector ---

    public void PlayClick()
    {
        PlaySFX(clickClip);
    }

    public void PlayCorrect()
    {
        PlaySFX(correctClip);
    }

    public void PlayDone()
    {
        PlaySFX(doneClip);
    }

    public void PlayPop()
    {
        PlaySFX(popClip);
    }

    public void PlayWrong()
    {
        PlaySFX(wrongClip);
    }
    #endregion

    #region Enum SFX Helper (Pilihan pemanggilan lewat kode alternatif)
    public enum SoundType
    {
        Click,
        Correct,
        Done,
        Pop,
        Wrong
    }

    public void PlaySound(SoundType soundType)
    {
        switch (soundType)
        {
            case SoundType.Click: PlayClick(); break;
            case SoundType.Correct: PlayCorrect(); break;
            case SoundType.Done: PlayDone(); break;
            case SoundType.Pop: PlayPop(); break;
            case SoundType.Wrong: PlayWrong(); break;
        }
    }
    #endregion
}
