// ============================================================
//  InputManager.cs
//  Attach to: A GameObject named "InputManager" in Scene 1.
//  Requires:  New Input System package installed.
//             In Project Settings > Player > Active Input
//             Handling, set to "New Input System Package" or
//             "Both".
// ============================================================
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Accessibility;

public class InputManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────
    public static InputManager Instance { get; private set; }

    // ── Inspector Config ─────────────────────────────────────
    [Header("Swipe Config")]
    [Tooltip("Minimum drag distance in pixels to register a swipe.")]
    public float minSwipeDistance = 60f;

    [Tooltip("Seconds within which a 2nd same-direction swipe counts as Confirm.")]
    public float doubleSwipeWindow = 0.3f;

    [Tooltip("Fired when: direction == Right AND rightSwipeTarget is assigned " +
             "AND rightSwipeTarget.activeInHierarchy == true.\n" +
             "Normal preview / confirm logic still runs alongside this event.")]
    public UnityEvent OnRightSwipeWithTarget;

    // ── Events (optional: wire in Inspector or subscribe in code) ──
    [Header("Standard Swipe Events")]
    public UnityEvent<int> OnPreview;   // arg: soundId (single swipe)
    public UnityEvent<int> OnConfirm;   // arg: soundId (double swipe)

    // ── Direction → Sound ID Map ─────────────────────────────
    // 0 = Up    → Sound 0
    // 1 = Down  → Sound 1
    // 2 = Left  → Sound 2
    // 3 = Right → Sound 3

    // ── Private State ────────────────────────────────────────
    private int CurrentButton = -1;

    // ── Unity Lifecycle ──────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ProcessSelection(int buttonid)
    {
        //Condition : if pressed just once selected that button preview sound, 
        //if pressed the second time without pressed any other button,handleconfirm that button
        //if pressed once but pressed any other button before the second time, preview that new button and reset the previous button's state
        if (!AssistiveSupport.isScreenReaderEnabled)
        {
            if (buttonid != CurrentButton)
            {
                CurrentButton = buttonid;
                AudioManager.Instance.PlayGameSound(buttonid);
            }
            else
            {
                GameManager.Instance.HandleConfirm(buttonid);

            }


        }
        else { GameManager.Instance.HandleConfirm(buttonid); }
        


    }
    
}