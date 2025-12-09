using UnityEngine;

public class LevelUIManager : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color unlockedButtonColor = Color.white;
    [SerializeField] private Color lockedButtonColor = Color.gray;
    [SerializeField] private Color filledWhiteCellColor = Color.white;
    [SerializeField] private Color emptyWhiteCellColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    
    // Public properties for elements to access colors
    public Color UnlockedButtonColor => unlockedButtonColor;
    public Color LockedButtonColor => lockedButtonColor;
    public Color FilledWhiteCellColor => filledWhiteCellColor;
    public Color EmptyWhiteCellColor => emptyWhiteCellColor;

    private LevelUIElement[] levelUIElements;

    private void Awake()
    {
        // Find all LevelUIElement components in the scene
        levelUIElements = FindObjectsOfType<LevelUIElement>();
        
        // Sort by level number to ensure correct order
        System.Array.Sort(levelUIElements, (a, b) => a.LevelNumber.CompareTo(b.LevelNumber));
        
        Debug.Log($"Found {levelUIElements.Length} level UI elements");
    }

    private void Start()
    {
        UpdateAllLevelUI();
    }

    /// <summary>
    /// Updates the UI for all levels based on current progression
    /// </summary>
    public void UpdateAllLevelUI()
    {
        foreach (LevelUIElement levelElement in levelUIElements)
        {
            if (levelElement != null)
            {
                levelElement.UpdateUI();
            }
        }
        
        Debug.Log("Updated UI for all levels");
    }

    /// <summary>
    /// Updates UI for a specific level
    /// </summary>
    public void UpdateLevelUI(int levelNumber)
    {
        LevelUIElement targetElement = GetLevelUIElement(levelNumber);
        if (targetElement != null)
        {
            targetElement.UpdateUI();
        }
        else
        {
            Debug.LogWarning($"Level UI element for level {levelNumber} not found!");
        }
    }

    /// <summary>
    /// Get a specific level UI element
    /// </summary>
    public LevelUIElement GetLevelUIElement(int levelNumber)
    {
        foreach (LevelUIElement element in levelUIElements)
        {
            if (element.LevelNumber == levelNumber)
            {
                return element;
            }
        }
        return null;
    }

    /// <summary>
    /// Refresh the level UI (call this when returning from a level)
    /// </summary>
    public void RefreshUI()
    {
        UpdateAllLevelUI();
    }

    /// <summary>
    /// Get total number of level UI elements found
    /// </summary>
    public int GetTotalLevelUIElements()
    {
        return levelUIElements != null ? levelUIElements.Length : 0;
    }

    // Debug methods for testing
    [ContextMenu("Refresh All UI")]
    private void DebugRefreshUI()
    {
        UpdateAllLevelUI();
    }

    [ContextMenu("Log Level Elements")]
    private void DebugLogLevelElements()
    {
        if (levelUIElements == null)
        {
            Debug.Log("No level UI elements found");
            return;
        }

        foreach (LevelUIElement element in levelUIElements)
        {
            if (element != null)
            {
                Debug.Log($"Level {element.LevelNumber}: {element.gameObject.name}");
            }
        }
    }
}