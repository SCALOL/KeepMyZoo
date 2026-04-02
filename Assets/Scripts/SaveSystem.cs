// ============================================================
//  SaveSystem.cs
//  Static utility — no GameObject needed. Call directly from
//  any script: SaveSystem.SaveHighScore(level);
// ============================================================
using UnityEngine;

public static class SaveSystem
{
    // ── PlayerPrefs Keys ─────────────────────────────────────
    private const string KeyHighScore = "HighScore";

    // ── Public API ───────────────────────────────────────────

    /// <summary>
    /// Persist the high score. Only writes if 'level' beats
    /// the current record.
    /// </summary>
    public static void SaveHighScore(int level)
    {
        int current = PlayerPrefs.GetInt(KeyHighScore, 0);
        if (level > current)
        {
            PlayerPrefs.SetInt(KeyHighScore, level);
            PlayerPrefs.Save();
            Debug.Log($"[SaveSystem] New high score saved: {level}");
        }
    }

    /// <summary>Returns the stored high score (0 if none).</summary>
    public static int GetHighScore()
    {
        return PlayerPrefs.GetInt(KeyHighScore, 0);
    }

    /// <summary>
    /// Wipe all PlayerPrefs data. Useful for a "Reset Progress"
    /// option in Settings or QA testing.
    /// </summary>
    public static void ClearAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[SaveSystem] All data cleared.");
    }
}
