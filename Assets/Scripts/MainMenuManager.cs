// ============================================================
//  MainMenuManager.cs
//  Attach to: A GameObject named "MainMenuManager"
//             in Scene 0 (MainMenu).
// ============================================================
using JetBrains.Annotations;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // ── Inspector References ─────────────────────────────────
    [Header("Canvases")]
    public GameObject mainMenuCanvas;
    public GameObject settingsCanvas;
    public GameObject tutorial1Canvas;
    public GameObject tutorial2Canvas;

    // ── Unity Lifecycle ──────────────────────────────────────
    private void Start()
    {
        DefaultMainMenuSet();
  
    }

    public void DefaultMainMenuSet()
    {
        mainMenuCanvas.SetActive(true);
        settingsCanvas.SetActive(false);
        tutorial1Canvas.SetActive(false);
        tutorial2Canvas.SetActive(false);
    }

    // ── Button Callbacks (wire to Button.OnClick in Inspector) ──

    public void OnStartGameButton()
    {
        bool next = !tutorial1Canvas.activeSelf;
        tutorial1Canvas.SetActive(next);
        mainMenuCanvas.SetActive(!next);
        if (settingsCanvas.activeSelf == next)
        {
            settingsCanvas.SetActive(!next);
        }
        
    }

    public void OnTutorialFinished()
    {
        SceneManager.LoadScene(1); // Gameplay
    }

    public void OnSettingsButton()
    {
        bool next = !settingsCanvas.activeSelf;
        settingsCanvas.SetActive(next);
        mainMenuCanvas.SetActive(!next);
    }

    public void OnNextTutorial()
    { 
        tutorial1Canvas.SetActive(false);
        tutorial2Canvas.SetActive(true);   
    }
    public void OnExitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
