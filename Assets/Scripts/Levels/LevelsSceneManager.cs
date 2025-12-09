using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelsSceneManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private LevelUIManager levelUIManager;
    
    [Header("Debug Settings")]
    [SerializeField] private bool unlockAllLevels = false;

    // Track the previous state to detect changes
    private bool previousUnlockState;

    private void Start()
    {
        // Find LevelUIManager if not assigned
        if (levelUIManager == null)
        {
            levelUIManager = FindObjectOfType<LevelUIManager>();
        }
        
        // Initialize previous state
        previousUnlockState = unlockAllLevels;
        
        // Refresh UI when scene loads
        if (levelUIManager != null)
        {
            levelUIManager.UpdateAllLevelUI();
        }
    }

    private void Update()
    {
        // Check if unlock state changed in inspector
        if (unlockAllLevels != previousUnlockState)
        {
            previousUnlockState = unlockAllLevels;
            Debug.Log($"Unlock All Levels changed to: {unlockAllLevels}");
            RefreshLevelUI();
        }
    }

    /// <summary>
    /// Called when a level button is clicked
    /// </summary>
    /// <param name="level">Level number (1-based)</param>
    public void OnLevelClick(int level)
    {
        // Check if level is unlocked (or if unlock all is enabled)
        if (unlockAllLevels || LevelProgressionManager.Instance.IsLevelUnlocked(level))
        {
            Debug.Log($"Loading Level {level}");
            SceneManager.LoadScene("Level " + level);
        }
        else
        {
            Debug.Log($"Level {level} is locked!");
            
            // Optional: Show a message to the player
            ShowLevelLockedMessage(level);
        }
    }

    /// <summary>
    /// Navigate back to main menu
    /// </summary>
    public void OnBackBtnClick()
    {
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Optional method to show locked level message
    /// </summary>
    private void ShowLevelLockedMessage(int level)
    {
        // You can implement a popup or UI message here
        // For now, just log to console
        Debug.Log($"Complete the previous level with at least 1 white cell to unlock Level {level}!");
        
        // Optional: Add UI feedback like screen shake, sound effect, or popup
    }

    /// <summary>
    /// Refresh the level UI (call this when returning from a level)
    /// </summary>
    public void RefreshLevelUI()
    {
        if (levelUIManager != null)
        {
            levelUIManager.RefreshUI();
        }
    }

    /// <summary>
    /// Get reference to level UI manager
    /// </summary>
    public LevelUIManager GetLevelUIManager()
    {
        return levelUIManager;
    }

    /// <summary>
    /// Check if all levels are unlocked (either through progression or debug flag)
    /// </summary>
    public bool IsLevelAccessible(int level)
    {
        return unlockAllLevels || LevelProgressionManager.Instance.IsLevelUnlocked(level);
    }

    // Debug methods for testing
    [ContextMenu("Unlock All Levels")]
    private void DebugUnlockAllLevels()
    {
        for (int i = 1; i <= 4; i++)
        {
            LevelProgressionManager.Instance.CompleteLevel(i, 3);
        }
        RefreshLevelUI();
    }

    [ContextMenu("Reset All Progress")]
    private void DebugResetProgress()
    {
        LevelProgressionManager.Instance.ResetAllProgress();
        RefreshLevelUI();
    }

    [ContextMenu("Toggle Unlock All")]
    private void DebugToggleUnlockAll()
    {
        unlockAllLevels = !unlockAllLevels;
        Debug.Log($"Unlock All Levels: {unlockAllLevels}");
        RefreshLevelUI();
    }
}