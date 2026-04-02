// ============================================================
//  SettingsManager.cs
//  Attach to: The SettingsCanvas GameObject in BOTH scenes.
//  Each scene gets its own copy of this script on its own
//  SettingsCanvas — no DontDestroyOnLoad needed.
// ============================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    // ── Inspector References ─────────────────────────────────
    [Header("Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Tooltip("UI Text elements to HIDE when Blind Mode is ON. " +
             "Drag in all TextMeshPro objects that would clutter " +
             "a screen reader (e.g. decorative labels).")]
    public GameObject[] blindModeHideTargets;

    // ── PlayerPrefs Keys ─────────────────────────────────────
    private const string BlindModeKey = "BlindMode";

    // ── Unity Lifecycle ──────────────────────────────────────
    private void OnEnable()
    {
        // Restore sliders and toggle to saved values every time
        // the panel opens.
        musicSlider.value    = PlayerPrefs.GetFloat(AudioManager.MusicVolKey, 1f);
        sfxSlider.value      = PlayerPrefs.GetFloat(AudioManager.SFXVolKey,   1f);

        // Apply immediately (AudioManager may not have applied
        // these yet if the scene just loaded)
        ApplyMusicVolume(musicSlider.value);
        ApplySFXVolume(sfxSlider.value);

        // Register listeners
        musicSlider.onValueChanged.AddListener(ApplyMusicVolume);
        sfxSlider.onValueChanged.AddListener(ApplySFXVolume);
    }

    private void OnDisable()
    {
        // Unregister to avoid double-callbacks if panel is
        // re-enabled later in the same scene.
        musicSlider.onValueChanged.RemoveListener(ApplyMusicVolume);
        sfxSlider.onValueChanged.RemoveListener(ApplySFXVolume);
    }

    // ── Callbacks ────────────────────────────────────────────

    private void ApplyMusicVolume(float value)
    {
        AudioManager.Instance?.SetMusicVolume(value);
        PlayerPrefs.SetFloat(AudioManager.MusicVolKey, value);
    }

    private void ApplySFXVolume(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
        PlayerPrefs.SetFloat(AudioManager.SFXVolKey, value);
    }
}
