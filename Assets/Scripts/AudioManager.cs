using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

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

    private void Awake()
    {
        // Singleton pattern: Pastikan hanya ada 1 AudioManager dan tidak hancur saat ganti scene
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Auto-setup AudioSource jika lupa di-assign di Inspector
        SetupAudioSources();
    }

    private void Start()
    {
        if (playBgmOnStart && defaultBGM != null)
        {
            PlayBGM(defaultBGM);
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

    #region BGM Controls
    /// <summary>
    /// Memutar BGM. Jika tidak ada parameter clip, memutar defaultBGM.
    /// </summary>
    public void PlayBGM(AudioClip musicClip = null)
    {
        AudioClip clipToPlay = musicClip != null ? musicClip : defaultBGM;
        if (clipToPlay == null) return;

        // Jangan restart BGM jika lagu yang sama sedang diputar
        if (bgmSource.clip == clipToPlay && bgmSource.isPlaying) return;

        bgmSource.clip = clipToPlay;
        bgmSource.loop = true;
        bgmSource.Play();
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
        if (bgmSource != null && !bgmSource.isPlaying)
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
