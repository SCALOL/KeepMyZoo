// ============================================================
//  AppInit.cs
//  Attach to: The AudioManager GameObject (it persists across
//             scenes via DontDestroyOnLoad, so it only needs
//             to run once).
//  Purpose:   Mobile performance bootstrap — runs before any
//             other Start() via [DefaultExecutionOrder].
// ============================================================
using UnityEngine;

[DefaultExecutionOrder(-100)] // Run before all other scripts
public class AppInit : MonoBehaviour
{
    private void Awake()
    {
        // Cap frame rate (spec §8)
        Application.targetFrameRate = 60;

        // Prevent screen from dimming during gameplay
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Lock to portrait for mobile swipe accuracy
        Screen.orientation = ScreenOrientation.Portrait;

        Debug.Log("[AppInit] App initialised. TargetFPS=60, Portrait lock.");
    }
}
