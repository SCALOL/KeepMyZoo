using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TutorialScript : MonoBehaviour
{
    [Header("Swipe Config")]
    [Tooltip("Minimum drag distance in pixels to register a swipe.")]
    public float minSwipeDistance = 60f;

    public UnityEvent OnProceedAction;

    // ── Private State ────────────────────────────────────────
    private Vector2 _touchStart;
    private bool _tracking = false;

    // Update is called once per frame
    void Update()
    {
        //SwipeActionCheck();
        TapCheck();
    }

    private void TapCheck()
    {
        if (Touchscreen.current != null)
        {
            var primary = Touchscreen.current.primaryTouch;

            if (primary.press.wasPressedThisFrame)
            {
                OnProceedAction?.Invoke();
            }
        }
        else if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                OnProceedAction?.Invoke();
            }
        }
    }

    private void SwipeActionCheck()
    {
        // ── Touchscreen (device) ─────────────────────────────
        if (Touchscreen.current != null)
        {
            var primary = Touchscreen.current.primaryTouch;

            if (primary.press.wasPressedThisFrame)
            {
                _touchStart = primary.position.ReadValue();
                _tracking = true;
            }
            else if (primary.press.wasReleasedThisFrame && _tracking)
            {
                ProcessSwipe(_touchStart, primary.position.ReadValue());
                _tracking = false;
            }
        }
        // ── Mouse fallback (Unity Editor testing) ────────────
        else if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _touchStart = Mouse.current.position.ReadValue();
                _tracking = true;
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame && _tracking)
            {
                ProcessSwipe(_touchStart, Mouse.current.position.ReadValue());
                _tracking = false;
            }
        }
    }

    // ── Conditional Right-Swipe Helper ───────────────────────

    /// <summary>
    /// Fires <see cref="OnProceedAction"/> when all three
    /// conditions are true simultaneously:
    ///   1. The resolved swipe direction is Right (ID 3).
    ///   2. <see cref="rightSwipeTarget"/> has been assigned in
    ///      the Inspector (not null).
    ///   3. The target is currently active in the hierarchy
    ///      (i.e. it exists and is visible/enabled in the scene).
    ///
    /// Normal preview / confirm logic is unaffected — this is
    /// purely additive.
    /// </summary>
    private void TryFireRightSwipeEvent(int direction)
    {
        const int RIGHT = 3;
        if (direction != RIGHT) return;

        OnProceedAction?.Invoke();
    }

    private void ProcessSwipe(Vector2 startPos, Vector2 endPos)
    {
        Vector2 delta = endPos - startPos;

        // Reject tiny drags / accidental taps
        if (delta.magnitude < minSwipeDistance) return;

        int direction = ResolveDirection(delta);

        // ── Conditional Right-Swipe check (Preview) ───────
        TryFireRightSwipeEvent(direction);
    }



    /// <summary>
    /// Returns the dominant axis direction as an int:
    ///   0 = Up | 1 = Down | 2 = Left | 3 = Right
    /// </summary>
    private int ResolveDirection(Vector2 delta)
    {
        // Compare absolute horizontal vs vertical movement
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x > 0 ? 3 : 2;  // Right : Left
        else
            return delta.y > 0 ? 0 : 1;  // Up    : Down
    }
}
