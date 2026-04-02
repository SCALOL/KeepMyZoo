// ============================================================
//  SRFocusListener.cs
//  Namespace: Unity.Samples.ScreenReader
//
//  Attach to: Any GameObject that also has an AccessibleElement
//             subcomponent (AccessibleButton, AccessibleText,
//             AccessibleSoundButton, etc.)
//
//  What it does:
//    Fires OnScreenReaderFocused whenever TalkBack (Android) or
//    VoiceOver (iOS) navigates onto this specific element.
//    Wire the UnityEvent in the Inspector — no code changes needed
//    to the target scripts.
//
//  focusChanged in AccessibleElement fires ONLY from the screen
//  reader — it never fires from normal touch input, so no
//  isScreenReaderEnabled guard is needed inside the handler.
// ============================================================
using UnityEngine;
using UnityEngine.Events;

namespace Unity.Samples.ScreenReader
{
    [DisallowMultipleComponent]
    public sealed class SRFocusListener : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────
        [Header("Target Element")]
        [Tooltip("The AccessibleElement on this (or another) GameObject " +
                 "to listen to. Leave empty to auto-detect on this GO.")]
        public AccessibleElement targetElement;

        [Header("Event")]
        [Tooltip("Fired when TalkBack / VoiceOver focuses this element.\n" +
                 "Wire any method here — it is SR-only by definition.")]
        public UnityEvent OnScreenReaderFocused;

        // ── Unity Lifecycle ───────────────────────────────────────
        void Awake()
        {
            // Auto-detect if not assigned in Inspector
            if (targetElement == null)
                targetElement = GetComponent<AccessibleElement>();

            if (targetElement == null)
                Debug.LogWarning($"[SRFocusListener] No AccessibleElement found on {gameObject.name}.");
        }

        void OnEnable()
        {
            if (targetElement != null)
                targetElement.focusChanged += HandleFocus;
        }

        void OnDisable()
        {
            if (targetElement != null)
                targetElement.focusChanged -= HandleFocus;
        }

        // ── Handler ───────────────────────────────────────────────

        /// <summary>
        /// Called by AccessibleElement.focusChanged when TalkBack /
        /// VoiceOver navigates onto this element.
        /// This event is screen-reader-only — no guard needed.
        /// </summary>
        void HandleFocus()
        {
            OnScreenReaderFocused?.Invoke();
        }
    }
}
