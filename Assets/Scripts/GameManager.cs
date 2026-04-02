// ============================================================
//  GameManager.cs
//  Attach to: A persistent GameObject named "GameManager"
//             in Scene 1 (Gameplay).
// ============================================================
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── State ────────────────────────────────────────────────
    public enum GameState { Idle, Playback, Input, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Idle;

    // ── Inspector Config ─────────────────────────────────────
    [Header("Game Config")]
    [Tooltip("Hearts the player starts with each game.")]
    public int startingHearts = 3;

    [Tooltip("Gap in seconds between each sound during playback.")]
    public float soundDelayBetween = 0.85f;

    [Tooltip("Pause before playback begins each level.")]
    public float prePlaybackDelay = 0.6f;

    [Tooltip("Pause before advancing to next level on win.")]
    public float levelCompleteDelay = 1.2f;
    //[Tooltip("Assign Animal audioClips pool for random sequence in game")]

    //[Tooltip("Assign Animal Images same sequence as AudioClips pool")]

    // ── Runtime State ────────────────────────────────────────
    public int CurrentLevel  { get; private set; } = 1;
    public int CurrentHearts { get; private set; }

    private List<int> _sequence  = new List<int>();
    private int        _inputIdx = 0;
    private int _seq_lenght = 0;

    // ── Events (wire these to UIManager in the Inspector) ────
    [Header("Events")]
    public UnityEvent<int, int> OnLevelStart;      // arg: level number
    public UnityEvent<int> OnHeartsChanged;   // arg: remaining hearts
    public UnityEvent      OnPlaybackStart;
    public UnityEvent      OnInputPhaseStart;
    public UnityEvent<int, int> OnCorrectInput;    // arg: new inputIndex
    public UnityEvent      OnWrongInput;
    public UnityEvent      OnLevelComplete;
    public UnityEvent<int> OnGameOver;        // arg: level reached

    // ── Unity Lifecycle ──────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        BeginGame();
    }

    // ── Public API ───────────────────────────────────────────

    /// <summary>Start the whole game from Level 1.</summary>
    public void BeginGame()
    {
        StartLevel(1);
    }

    /// <summary>Restart from Level 1 (called by Restart button).</summary>
    public void RestartGame()
    {
        StopAllCoroutines();
        StartLevel(1);
    }

    /// <summary>
    /// Called by InputManager each time the player double-swipes to confirm a sound.
    /// </summary>
    public void HandleConfirm(int confirmedSoundId)
    {
        if (CurrentState != GameState.Input) return;

        int expected = _sequence[_inputIdx];

        if (confirmedSoundId == expected)
        {
            // ── Correct ──────────────────────────────────────
            _inputIdx++;
            OnCorrectInput?.Invoke(_inputIdx,_seq_lenght);

            AudioManager.Instance.PlayCorrectSound();

            if (_inputIdx >= _sequence.Count)
            {
                AudioManager.Instance.PlayWinSound();
                StartCoroutine(LevelCompleteRoutine());
            }
                
            
            
            // else: stay in Input state, wait for next confirm
        }
        else
        {
            // ── Wrong: lose heart, STAY on same slot ─────────
            CurrentHearts--;
            OnHeartsChanged?.Invoke(CurrentHearts);
            OnWrongInput?.Invoke();
            Handheld.Vibrate();
            AudioManager.Instance.PlayErrorSound();

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif

            if (CurrentHearts <= 0)
                StartCoroutine(GameOverRoutine());
            // else: remain in Input state at same _inputIdx so player can retry
        }
    }

    // ── Private Logic ────────────────────────────────────────

    private void StartLevel(int level)
    {
        CurrentLevel  = level;
        CurrentHearts = startingHearts;
        _inputIdx     = 0;
        _seq_lenght = 4 + level / 4;
        _sequence     = GenerateSequence(_seq_lenght); // add sound every 3 levels for variety, but keep it manageable

        OnLevelStart?.Invoke(CurrentLevel,_seq_lenght);
        OnHeartsChanged?.Invoke(CurrentHearts);

        StartCoroutine(PlaybackRoutine());
    }

    /// <summary>
    /// Full random sequence from 4-sound pool; repeats allowed.
    /// </summary>
    private List<int> GenerateSequence(int length)
    {
        var seq = new List<int>(length);
        for (int i = 0; i < length; i++)
            seq.Add(Random.Range(0, 4)); // IDs 0-3
        return seq;
    }

    private IEnumerator PlaybackRoutine()
    {
        SetState(GameState.Playback);
        OnPlaybackStart?.Invoke();

        yield return new WaitForSeconds(prePlaybackDelay);

        foreach (int soundId in _sequence)
        {
            AudioManager.Instance.PlayGameSound(soundId);
            yield return new WaitForSeconds(soundDelayBetween);
        }

        yield return new WaitForSeconds(0.3f);

        SetState(GameState.Input);
        OnInputPhaseStart?.Invoke();
    }

    private IEnumerator LevelCompleteRoutine()
    {
        SetState(GameState.Idle);
        OnLevelComplete?.Invoke();
        SaveSystem.SaveHighScore(CurrentLevel);

        yield return new WaitForSeconds(levelCompleteDelay);

        StartLevel(CurrentLevel + 1);
    }

    private IEnumerator GameOverRoutine()
    {
        SetState(GameState.GameOver);
        SaveSystem.SaveHighScore(CurrentLevel);
        AudioManager.Instance.PlayGameOverSound();
        yield return new WaitForSeconds(0.5f);

        OnGameOver?.Invoke(CurrentLevel);
    }

    private void SetState(GameState newState)
    {
        CurrentState = newState;
    }

    public void PreviewSound(int soundid)
    {
        AudioManager.Instance.PlayGameSound(soundid);
    }
}
