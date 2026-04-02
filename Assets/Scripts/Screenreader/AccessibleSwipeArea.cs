// ============================================================
//  AccessibleSwipeArea.cs
//  Namespace: Unity.Samples.ScreenReader  (matches uploaded scripts)
//
//  Attach to: The SwipeArea GameObject in Scene 1 (Gameplay).
//
//  How it works in TalkBack/VoiceOver:
//    • TalkBack swipe UP   → incremented → cycle sound forward
//    • TalkBack swipe DOWN → decremented → cycle sound backward
//    • TalkBack double-tap → selected    → confirm sound to GameManager
//
//  Note: allowsDirectInteraction is NOT supported on Android
//  (see AccessibleElement.SetNodeProperties line 219).
//  Raw swipes never reach Unity while TalkBack is active on Android.
//  TalkBack users play entirely through increment/decrement/select.
// ============================================================
using UnityEngine;
using UnityEngine.Accessibility;

namespace Unity.Samples.ScreenReader
{
    [AddComponentMenu("Accessibility/Accessible Swipe Area")]
    [DisallowMultipleComponent]
    public sealed class AccessibleSwipeArea : AccessibleElement
    {
        // ── Sound name table (update to match your actual clips) ──
        static readonly string[] k_SoundNames = { "Up", "Down", "Left", "Right" };

        // ── Currently selected sound (0-3) ───────────────────────
        int m_SelectedSoundId = 0;

        // ── Unity Lifecycle ───────────────────────────────────────
        void Start()
        {
            role  = AccessibilityRole.Button;
            hint  = "Swipe up or down to cycle sounds, double-tap to confirm";
            // Node starts inactive — GameplayAnnouncer enables it
            // when GameManager enters the Input phase.
            isActive = false;

            RefreshLabel();
        }

        // ── AccessibleElement overrides ───────────────────────────

        protected override void BindToControl()
        {
            incremented += OnIncremented;
            decremented += OnDecremented;
            selected    += OnSelected;
        }

        protected override void UnbindFromControl()
        {
            incremented -= OnIncremented;
            decremented -= OnDecremented;
            selected    -= OnSelected;
        }

        // ── TalkBack gesture handlers ─────────────────────────────

        void OnIncremented()
        {
            m_SelectedSoundId = (m_SelectedSoundId + 1) % 4;
            RefreshLabel();
            // Play preview so the user hears what they have selected
            AudioManager.Instance?.PlayGameSound(m_SelectedSoundId);
            // Tell the SR the label changed
            this.DelayRefreshNodeFrames();
        }

        void OnDecremented()
        {
            // +3 mod 4 is equivalent to -1 mod 4 (avoids negative modulo)
            m_SelectedSoundId = (m_SelectedSoundId + 3) % 4;
            RefreshLabel();
            AudioManager.Instance?.PlayGameSound(m_SelectedSoundId);
            this.DelayRefreshNodeFrames();
        }

        bool OnSelected()
        {
            if (GameManager.Instance == null ||
                GameManager.Instance.CurrentState != GameManager.GameState.Input)
                return false;

            GameManager.Instance.HandleConfirm(m_SelectedSoundId);
            return true;
        }

        // ── Public API (called by GameplayAnnouncer) ──────────────

        /// <summary>
        /// Reset the selected sound back to 0 and activate/deactivate
        /// this node based on whether the game is in Input phase.
        /// Call this whenever the game phase changes.
        /// </summary>
        public void SetInputPhaseActive(bool inputPhase)
        {
            m_SelectedSoundId = 0;
            isActive          = inputPhase;
            RefreshLabel();
            SetNodeProperties();
        }

        // ── Helpers ───────────────────────────────────────────────

        void RefreshLabel()
        {
            label = $"Sound input. Selected: {k_SoundNames[m_SelectedSoundId]}";
        }
    }
}
