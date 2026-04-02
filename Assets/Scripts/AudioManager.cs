// ============================================================
//  AudioManager.cs
//  Attach to: A persistent GameObject named "AudioManager"
//             in Scene 0 (MainMenu). Mark DontDestroyOnLoad.
// ============================================================
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────
    public static AudioManager Instance { get; private set; }

    // ── Inspector References ─────────────────────────────────
    [Header("Audio Mixer")]
    [Tooltip("Drag your MasterMixer asset here.")]
    public AudioMixer masterMixer;

    [Header("Audio Sources")]
    [Tooltip("AudioSource component for SFX (child GameObject).")]
    public AudioSource sfxSource;

    [Tooltip("AudioSource component for Music (child GameObject).")]
    public AudioSource musicSource;

    [Header("Game Sound Clips  (index = Sound ID)")]
    [Tooltip("Exactly 4 clips. Index 0=Up, 1=Down, 2=Left, 3=Right.")]
    public AudioClip[] gameSoundClips = new AudioClip[4];

    [Header("Special Clips")]
    public AudioClip errorClip;    // Played on wrong input
    public AudioClip tickClip;     // Blind Mode menu hover
    public AudioClip winClip;      // Level complete sting
    public AudioClip musicClip;    // Background music loop
    public AudioClip correctClip;
    public AudioClip gameOverClip;

    // ── PlayerPrefs Keys (shared with SettingsManager) ───────
    public const string MusicVolKey = "MusicVol";
    public const string SFXVolKey   = "SFXVol";

    // ── Mixer Parameter Names ────────────────────────────────
    // Make sure your AudioMixer has two exposed float params
    // named exactly: "MusicVolume" and "SFXVolume"
    private const string MixerMusicParam = "MusicVolume";
    private const string MixerSFXParam   = "SFXVolume";

    // ── Unity Lifecycle ──────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Restore saved volumes
        float savedMusic = PlayerPrefs.GetFloat(MusicVolKey, 1f);
        float savedSFX   = PlayerPrefs.GetFloat(SFXVolKey,   1f);
        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSFX);

        // Start background music
        if (musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // ── Public Playback API ──────────────────────────────────

    /// <summary>Play one of the 4 game sounds by ID (0-3).</summary>
    public void PlayGameSound(int soundId)
    {
        if (soundId < 0 || soundId >= gameSoundClips.Length)
        {
            Debug.LogWarning($"[AudioManager] Sound ID {soundId} out of range.");
            return;
        }
        if (gameSoundClips[soundId] == null)
        {
            Debug.LogWarning($"[AudioManager] No clip assigned for Sound ID {soundId}.");
            return;
        }
        sfxSource.PlayOneShot(gameSoundClips[soundId]);
        // Notify SoundVisualizer so the animal sprite shows during
        // Playback phase (AudioManager-driven). During Input phase,
        // SoundVisualizer subscribes to InputManager events directly.
        SoundVisualizer.Instance?.ShowSprite(soundId);
    }

    /// <summary>Play the error/wrong-answer sound.</summary>
    public void PlayErrorSound()
    {
        if (errorClip != null) sfxSource.PlayOneShot(errorClip);
    }

    /// <summary>Play the Blind Mode menu hover tick.</summary>
    public void PlayTickSound()
    {
        if (tickClip != null) sfxSource.PlayOneShot(tickClip);
    }

    /// <summary>Play level-complete win sting.</summary>
    public void PlayWinSound()
    {
        if (winClip != null) sfxSource.PlayOneShot(winClip);
    }
    public void PlayCorrectSound()
    {
        if (correctClip != null) sfxSource.PlayOneShot(correctClip);
    }

    public void PlayGameOverSound()
    {
        if (gameOverClip != null) sfxSource.PlayOneShot(gameOverClip);
    }
    // ── Volume Control ───────────────────────────────────────

    /// <summary>
    /// Set music volume from a 0–1 slider value.
    /// Converts to logarithmic dB for the AudioMixer.
    /// </summary>
    public void SetMusicVolume(float sliderValue)
    {
        float dB = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
        masterMixer.SetFloat(MixerMusicParam, dB);
    }

    /// <summary>
    /// Set SFX volume from a 0–1 slider value.
    /// </summary>
    public void SetSFXVolume(float sliderValue)
    {
        float dB = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
        masterMixer.SetFloat(MixerSFXParam, dB);
        PlayTickSound();
    }
}
