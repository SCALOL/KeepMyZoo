// ============================================================
//  SoundVisualizer.cs
//  Attach to: A GameObject named "SoundVisualizer" in Scene 1.
//
//  What it does:
//    • Shows the matching animal sprite when a sound plays
//      (both during Playback and during player Input phase).
//    • Hides the sprite after a configurable display duration.
//    • Supports an optional idle/default sprite shown when
//      no sound is active.
// ============================================================
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SoundVisualizer : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────
    public static SoundVisualizer Instance { get; private set; }

    // ── Inspector: Sprite Mapping ─────────────────────────────
    [Header("Animal Sprites  (index = Sound ID)")]
    [Tooltip("Exactly 4 sprites. Index 0=Up, 1=Down, 2=Left, 3=Right.\n" +
             "Drag one animal sprite into each slot.")]
    public Sprite[] animalSprites = new Sprite[4];

    [Tooltip("Optional: shown when no sound is playing. " +
             "Leave empty to hide the image instead.")]
    public Sprite idleSprite;

    // ── Inspector: Display Target ─────────────────────────────
    [Header("Display Target")]
    [Tooltip("The UI Image component that will show the sprite. " +
             "Add a UI Image to any GameObject in your GameCanvas " +
             "and drag it here.")]
    public Image displayImage;

    // ── Inspector: Timing ─────────────────────────────────────
    [Header("Timing")]
    [Tooltip("How long (seconds) the animal sprite stays visible " +
             "after a sound plays.")]
    public float displayDuration = 0.7f;

    [Tooltip("Fade out the sprite over this many seconds after " +
             "displayDuration. Set to 0 for instant hide.")]
    public float fadeDuration = 0.15f;

    // ── Private State ─────────────────────────────────────────
    private Coroutine _hideRoutine;

    // ── Unity Lifecycle ───────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        ValidateSetup();

        // Subscribe to AudioManager-level sound events via
        // InputManager (player input phase) and GameManager
        // (playback phase).
        //
        // AudioManager doesn't broadcast events by itself, so
        // we hook into the two sources that call PlayGameSound():
        //   • InputManager.OnPreview  — player single-swipe
        //   • InputManager.OnConfirm  — player double-swipe
        //   • GameManager  plays sounds directly in coroutine,
        //     so we expose a public method AudioManager can call.

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPreview.AddListener(ShowSprite);
            InputManager.Instance.OnConfirm.AddListener(ShowSprite);
        }
        else
        {
            Debug.LogWarning("[SoundVisualizer] InputManager not found. " +
                             "Player-input sprites won't show.");
        }

        ShowIdle();
    }

    private void OnDestroy()
    {
        // Clean up listeners to avoid memory leaks
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPreview.RemoveListener(ShowSprite);
            InputManager.Instance.OnConfirm.RemoveListener(ShowSprite);
        }
    }

    // ── Public API ────────────────────────────────────────────

    /// <summary>
    /// Show the animal sprite for the given sound ID (0-3).
    /// Called automatically by Input events.
    /// Also call this from AudioManager when playing sounds
    /// during Playback phase — see AudioManager integration below.
    /// </summary>
    public void ShowSprite(int soundId)
    {
        if (soundId < 0 || soundId >= animalSprites.Length)
        {
            Debug.LogWarning($"[SoundVisualizer] Sound ID {soundId} has no sprite slot.");
            return;
        }

        Sprite target = animalSprites[soundId];
        if (target == null)
        {
            Debug.LogWarning($"[SoundVisualizer] Sprite slot {soundId} is empty.");
            return;
        }

        // Cancel any pending hide from a previous sound
        if (_hideRoutine != null) StopCoroutine(_hideRoutine);

        // Show immediately at full opacity
        displayImage.sprite  = target;
        displayImage.enabled = true;
        SetAlpha(1f);

        // Schedule hide
        _hideRoutine = StartCoroutine(HideAfterDelay());
    }

    /// <summary>
    /// Immediately return to the idle state (called on level
    /// start, game over, etc.)
    /// </summary>
    public void ShowIdle()
    {
        if (_hideRoutine != null) { StopCoroutine(_hideRoutine); _hideRoutine = null; }

        if (idleSprite != null)
        {
            displayImage.sprite  = idleSprite;
            displayImage.enabled = true;
            SetAlpha(1f);
        }
        else
        {
            displayImage.enabled = false;
        }
    }

    // ── Private Helpers ───────────────────────────────────────

    private IEnumerator HideAfterDelay()
    {
        // Keep sprite fully visible for displayDuration
        yield return new WaitForSeconds(displayDuration);

        if (fadeDuration > 0f)
        {
            // Fade out
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                SetAlpha(1f - Mathf.Clamp01(elapsed / fadeDuration));
                yield return null;
            }
        }

        ShowIdle();
        _hideRoutine = null;
    }

    private void SetAlpha(float alpha)
    {
        Color c = displayImage.color;
        c.a = alpha;
        displayImage.color = c;
    }

    private void ValidateSetup()
    {
        if (displayImage == null)
            Debug.LogError("[SoundVisualizer] displayImage is not assigned!");

        if (animalSprites.Length != 4)
            Debug.LogError("[SoundVisualizer] animalSprites must have exactly 4 slots.");

        for (int i = 0; i < animalSprites.Length; i++)
            if (animalSprites[i] == null)
                Debug.LogWarning($"[SoundVisualizer] animalSprites[{i}] is empty.");
    }
}
