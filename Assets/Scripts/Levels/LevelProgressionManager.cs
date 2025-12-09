using UnityEngine;

public class LevelProgressionManager : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int totalLevels = 4;
    
    private static LevelProgressionManager instance;
    public static LevelProgressionManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("LevelProgressionManager");
                instance = go.AddComponent<LevelProgressionManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeProgression();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeProgression()
    {
        // Ensure level 1 is always unlocked
        if (!IsLevelUnlocked(1))
        {
            UnlockLevel(1);
        }
    }

    /// <summary>
    /// Records level completion and unlocks next level if applicable
    /// </summary>
    /// <param name="levelNumber">The level that was completed</param>
    /// <param name="whiteCellsEarned">Number of white cells remaining (0-3)</param>
    public void CompleteLevel(int levelNumber, int whiteCellsEarned)
    {
        // Clamp white cells to valid range
        whiteCellsEarned = Mathf.Clamp(whiteCellsEarned, 0, 3);
        
        // Only save if this is better than previous attempt
        int currentBest = GetLevelWhiteCells(levelNumber);
        if (whiteCellsEarned > currentBest)
        {
            SaveLevelWhiteCells(levelNumber, whiteCellsEarned);
        }
        
        // Mark level as completed
        SetLevelCompleted(levelNumber, true);
        
        // Unlock next level if player earned at least 1 white cell
        if (whiteCellsEarned >= 1 && levelNumber < totalLevels)
        {
            UnlockLevel(levelNumber + 1);
        }
        
        Debug.Log($"Level {levelNumber} completed with {whiteCellsEarned} white cells. Next level unlocked: {whiteCellsEarned >= 1}");
    }

    /// <summary>
    /// Check if a level is unlocked
    /// </summary>
    public bool IsLevelUnlocked(int levelNumber)
    {
        if (levelNumber == 1) return true; // Level 1 always unlocked
        return PlayerPrefs.GetInt($"Level_{levelNumber}_Unlocked", 0) == 1;
    }

    /// <summary>
    /// Get the best white cells earned for a level
    /// </summary>
    public int GetLevelWhiteCells(int levelNumber)
    {
        return PlayerPrefs.GetInt($"Level_{levelNumber}_WhiteCells", 0);
    }

    /// <summary>
    /// Check if a level has been completed
    /// </summary>
    public bool IsLevelCompleted(int levelNumber)
    {
        return PlayerPrefs.GetInt($"Level_{levelNumber}_Completed", 0) == 1;
    }

    /// <summary>
    /// Get total number of levels
    /// </summary>
    public int GetTotalLevels()
    {
        return totalLevels;
    }

    /// <summary>
    /// Reset all progress (for testing)
    /// </summary>
    public void ResetAllProgress()
    {
        for (int i = 1; i <= totalLevels; i++)
        {
            PlayerPrefs.DeleteKey($"Level_{i}_Unlocked");
            PlayerPrefs.DeleteKey($"Level_{i}_WhiteCells");
            PlayerPrefs.DeleteKey($"Level_{i}_Completed");
        }
        
        // Ensure level 1 is unlocked
        UnlockLevel(1);
        PlayerPrefs.Save();
        
        Debug.Log("All level progress reset!");
    }

    // Private helper methods
    private void UnlockLevel(int levelNumber)
    {
        PlayerPrefs.SetInt($"Level_{levelNumber}_Unlocked", 1);
        PlayerPrefs.Save();
    }

    private void SaveLevelWhiteCells(int levelNumber, int whiteCells)
    {
        PlayerPrefs.SetInt($"Level_{levelNumber}_WhiteCells", whiteCells);
        PlayerPrefs.Save();
    }

    private void SetLevelCompleted(int levelNumber, bool completed)
    {
        PlayerPrefs.SetInt($"Level_{levelNumber}_Completed", completed ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Debug method - remove in production
    [ContextMenu("Reset Progress")]
    private void DebugResetProgress()
    {
        ResetAllProgress();
    }
}