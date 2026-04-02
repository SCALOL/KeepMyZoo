// ============================================================
//  UIManager.cs
//  Attach to: A GameObject named "UIManager" in Scene 1.
//  Requires:  TextMeshPro package installed.
// ============================================================
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Unity.Samples.ScreenReader;

public class UIManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────
    public static UIManager Instance { get; private set; }

    // ── Inspector: Canvas References ─────────────────────────
    [Header("Canvases")]
    public GameObject gameCanvas;
    public GameObject gameOverCanvas;
    public GameObject settingsCanvas;   // Overlay
    public GameObject SoundButtonGroupCanvas; // Overlay for sound buttons during input phase

    // ── Inspector: Game Canvas Elements ──────────────────────
    [Header("Game Canvas — HUD")]
    [Tooltip("Displays current level number.")]
    public GameObject levelText;
    [Tooltip("Displays Successfully Caught Amount")]
    public GameObject CaughtAmounText; 
    [Tooltip("Status label: 'Listen...' or 'Your turn!'")]
    public TextMeshProUGUI statusText;

    [Tooltip("3 hearts Text")]
    public GameObject CurrentHeartText;

    // ── Inspector: Game Over Canvas Elements ─────────────────
    [Header("Game Over Canvas")]
    public GameObject scoreText;
    public GameObject bestScoreText;

    // ── Unity Lifecycle ──────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Subscribe to GameManager events
        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("[UIManager] GameManager not found in scene!");
            return;
        }

        gm.OnLevelStart.AddListener(HandleLevelStart);
        gm.OnHeartsChanged.AddListener(HandleHeartsChanged);
        gm.OnPlaybackStart.AddListener(HandlePlaybackStart);
        gm.OnInputPhaseStart.AddListener(HandleInputPhaseStart);
        gm.OnLevelComplete.AddListener(HandleLevelComplete);
        gm.OnGameOver.AddListener(HandleGameOver);
        gm.OnCorrectInput.AddListener(HandleAnimalCaughtSuccess);

        // Initial view
        ShowGameCanvas();
    }

    // ── Event Handlers ───────────────────────────────────────

    private void HandleLevelStart(int level, int soundsQuatity)
    {
        ShowGameCanvas();
        levelText.GetComponent<TextMeshProUGUI>().text = $"Level {level}";
        CaughtAmounText.GetComponent<TextMeshProUGUI>().text = $"0 / {soundsQuatity}";
        SetAccessibilityLabel(levelText, $"Level {level}");
        SetAccessibilityLabel(CaughtAmounText, $"จับได้ 0 ตัว จากทั้งหมด {soundsQuatity} ตัว");
        
        statusText.text = "Get ready...";
    }

    private void HandleAnimalCaughtSuccess(int caughtAmount, int soundsQuatity)
    {
        CaughtAmounText.GetComponent<TextMeshProUGUI>().text = $"{caughtAmount} / {soundsQuatity}";
        SetAccessibilityLabel(CaughtAmounText, $"จับได้ {caughtAmount} ตัว จากทั้งหมด {soundsQuatity} ตัว");
    }

    private void HandleHeartsChanged(int hearts)
    {
        Debug.Log($"{hearts} hearts remaining");
        CurrentHeartText.GetComponent<TextMeshProUGUI>().text = $"{hearts}";
        SetAccessibilityLabel(CurrentHeartText, $"เหลือหัวใจ {hearts} ดวง");

    }

    private void SetAccessibilityLabel(GameObject AccessibleElement, string labelInput)
    { 
        var AccessibleLabel = AccessibleElement.GetComponent<AccessibleText>();
        if (AccessibleLabel != null)
        {
            AccessibleLabel.label = labelInput;
            AccessibleLabel.SetNodeProperties();
        }
    }
    private void HandlePlaybackStart()
    {
        statusText.text = "Listen...";
        //deactivate SoundButtonGroup Canvas
        SoundButtonGroupCanvas.SetActive(false);

    }

    private void HandleInputPhaseStart()
    {
        statusText.text = "Your turn!";
        //activate SoundButtonGroup Canvas
        SoundButtonGroupCanvas.SetActive(true);
    }

    private void HandleLevelComplete()
    {
        statusText.text = "Nice!"; // ⬆ emoji as visual sting
    }

    private void HandleGameOver(int levelReached)
    {
        gameCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        gameOverCanvas.SetActive(true);
        SoundButtonGroupCanvas.SetActive(false);

        scoreText.GetComponent<TextMeshProUGUI>().text    = $"Level Reached\n{levelReached}";
        bestScoreText.GetComponent<TextMeshProUGUI>().text = $"Best Level\n{SaveSystem.GetHighScore()}";
        SetAccessibilityLabel(scoreText, $"คุณมาถึง Level {levelReached}");
        SetAccessibilityLabel(bestScoreText, $"Level สูงสุดที่ได้ {SaveSystem.GetHighScore()}");

    }

    // ── Canvas Helpers ───────────────────────────────────────

    public void ShowGameCanvas()
    {
        gameCanvas.SetActive(true);
        gameOverCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
    }

    public void ToggleSettings()
    {
        bool next = !settingsCanvas.activeSelf;
        settingsCanvas.SetActive(next);
        gameCanvas.SetActive(!next);
    }

    // ── Button Callbacks (wire to Button.OnClick in Inspector) ──

    public void OnRestartButton()
    {
        GameManager.Instance.RestartGame();
    }

    public void OnHomeButton()
    {
        SceneManager.LoadScene(0); // MainMenu
    }

    public void OnSettingsButton()
    {
        ToggleSettings();
    }
}
