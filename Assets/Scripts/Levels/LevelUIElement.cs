using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUIElement : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int levelNumber;
    
    [Header("Auto-Find Settings")]
    [SerializeField] private bool autoFindElements = true;
    
    [Header("UI Element Names (for auto-find)")]
    [SerializeField] private string levelTextName = "LevelText";
    [SerializeField] private string whiteCellsParentName = "WhiteCells";
    [SerializeField] private string lockIconName = "LockIcon";
    
    [Header("Manual Assignment (if auto-find fails)")]
    [SerializeField] private Button levelButton;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image[] whiteCellImages = new Image[3];
    [SerializeField] private GameObject lockIcon;
    
    // Properties
    public int LevelNumber => levelNumber;
    public Button LevelButton => levelButton;
    public TMP_Text LevelText => levelText;
    public Image[] WhiteCellImages => whiteCellImages;
    public GameObject LockIcon => lockIcon;

    private void Awake()
    {
        if (autoFindElements)
        {
            FindUIElements();
        }
        
        ValidateElements();
    }

    private void Start()
    {
        UpdateUI();
    }

    /// <summary>
    /// Automatically finds UI elements based on names
    /// </summary>
    private void FindUIElements()
    {
        // Find button component on this GameObject
        if (levelButton == null)
        {
            levelButton = GetComponent<Button>();
        }
        
        // Find level text
        if (levelText == null)
        {
            Transform textTransform = transform.Find(levelTextName);
            if (textTransform != null)
            {
                levelText = textTransform.GetComponent<TMP_Text>();
            }
        }
        
        // Find white cells container and get Image components
        Transform whiteCellsParent = transform.Find(whiteCellsParentName);
        if (whiteCellsParent != null)
        {
            Image[] foundWhiteCells = whiteCellsParent.GetComponentsInChildren<Image>();
            if (foundWhiteCells.Length >= 3)
            {
                for (int i = 0; i < 3 && i < foundWhiteCells.Length; i++)
                {
                    whiteCellImages[i] = foundWhiteCells[i];
                }
            }
            else
            {
                Debug.LogWarning($"Level {levelNumber}: Expected 3 white cell images, found {foundWhiteCells.Length}");
            }
        }
        
        // Find lock icon
        if (lockIcon == null)
        {
            Transform lockTransform = transform.Find(lockIconName);
            if (lockTransform != null)
            {
                lockIcon = lockTransform.gameObject;
            }
        }
    }

    /// <summary>
    /// Validates that all required elements are assigned
    /// </summary>
    private void ValidateElements()
    {
        if (levelButton == null)
            Debug.LogError($"Level {levelNumber}: Button component not found!");
        
        if (levelText == null)
            Debug.LogWarning($"Level {levelNumber}: Level text not found. Looking for child named '{levelTextName}'");
        
        for (int i = 0; i < whiteCellImages.Length; i++)
        {
            if (whiteCellImages[i] == null)
                Debug.LogWarning($"Level {levelNumber}: White cell image {i + 1} not assigned!");
        }
    }

    /// <summary>
    /// Updates this level's UI based on current progression
    /// </summary>
    public void UpdateUI()
    {
        if (levelNumber <= 0)
        {
            Debug.LogError($"Level number not set for {gameObject.name}!");
            return;
        }

        // Get the UI manager for colors
        LevelUIManager uiManager = FindObjectOfType<LevelUIManager>();
        if (uiManager == null)
        {
            Debug.LogError("LevelUIManager not found!");
            return;
        }

        // Get the levels scene manager to check unlock status (respects debug unlock flag)
        LevelsSceneManager levelsManager = FindObjectOfType<LevelsSceneManager>();
        
        // Check if level is accessible (either through progression or debug unlock)
        bool isUnlocked;
        if (levelsManager != null)
        {
            isUnlocked = levelsManager.IsLevelAccessible(levelNumber);
        }
        else
        {
            // Fallback to progression manager if levels manager not found
            isUnlocked = LevelProgressionManager.Instance.IsLevelUnlocked(levelNumber);
            Debug.LogWarning($"Level {levelNumber}: LevelsSceneManager not found, using fallback progression check");
        }
        
        int whiteCellsEarned = LevelProgressionManager.Instance.GetLevelWhiteCells(levelNumber);
        
        // Update button appearance
        UpdateButtonAppearance(isUnlocked, uiManager);
        
        // Update white cells display
        UpdateWhiteCellsDisplay(whiteCellsEarned, uiManager);
        
        // Update lock icon
        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }
        
        // Update level text
        if (levelText != null)
        {
            levelText.text = levelNumber.ToString();
        }
    }

    /// <summary>
    /// Updates the visual appearance of the level button
    /// </summary>
    private void UpdateButtonAppearance(bool isUnlocked, LevelUIManager uiManager)
    {
        if (levelButton != null)
        {
            Color targetColor = isUnlocked ? uiManager.UnlockedButtonColor : uiManager.LockedButtonColor;
            
            // Change button color
            ColorBlock colors = levelButton.colors;
            colors.normalColor = targetColor;
            colors.highlightedColor = targetColor;
            colors.selectedColor = targetColor;
            levelButton.colors = colors;
            
            // Change button's main image color
            if (levelButton.targetGraphic != null)
            {
                levelButton.targetGraphic.color = targetColor;
            }
        }
    }

    /// <summary>
    /// Updates the white cell icons based on earned amount
    /// </summary>
    private void UpdateWhiteCellsDisplay(int whiteCellsEarned, LevelUIManager uiManager)
    {
        for (int i = 0; i < whiteCellImages.Length; i++)
        {
            if (whiteCellImages[i] != null)
            {
                // Fill white cells up to the earned amount
                if (i < whiteCellsEarned)
                {
                    whiteCellImages[i].color = uiManager.FilledWhiteCellColor;
                }
                else
                {
                    whiteCellImages[i].color = uiManager.EmptyWhiteCellColor;
                }
            }
        }
    }

    /// <summary>
    /// Called when this level button is clicked
    /// </summary>
    public void OnLevelButtonClicked()
    {
        // Find the LevelsSceneManager and call its method
        LevelsSceneManager sceneManager = FindObjectOfType<LevelsSceneManager>();
        if (sceneManager != null)
        {
            sceneManager.OnLevelClick(levelNumber);
        }
        else
        {
            Debug.LogError("LevelsSceneManager not found!");
        }
    }

    // Editor helper methods
    [ContextMenu("Auto-Find Elements")]
    private void EditorAutoFind()
    {
        FindUIElements();
        ValidateElements();
    }

    [ContextMenu("Update UI")]
    private void EditorUpdateUI()
    {
        if (Application.isPlaying)
        {
            UpdateUI();
        }
        else
        {
            Debug.Log("Update UI only works in play mode");
        }
    }

    // Set level number from inspector or code
    public void SetLevelNumber(int level)
    {
        levelNumber = level;
    }
}