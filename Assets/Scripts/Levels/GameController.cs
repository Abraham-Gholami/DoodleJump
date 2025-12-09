using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UGS;

public class GameController : MonoBehaviour
{
    [Header("Basic Configuration")]
    public float destroyOffset = 0;
    public GameObject guidePanel;

    [Header("Level Type Configuration")]
    [SerializeField] private LevelType levelType = LevelType.EnemyKills;
    [Tooltip("Choose whether this level requires killing enemies or reaching a height target")]

    [Header("Enemy Kill Targets (Enemy Kills Mode Only)")]
    [SerializeField] private EnemyKillTarget[] enemyKillTargets;

    [Header("Height Target Configuration (Height Target Mode Only)")]
    [SerializeField] private float targetHeight = 100f;
    [Tooltip("Height that must be reached to complete the level")]
    [SerializeField] private Text heightTargetText;
    [SerializeField] private GameObject heightTargetPanel;
    [SerializeField] private Image heightProgressFill;
    [Tooltip("UI elements to display height progress - Fill image should have Image Type set to 'Filled'")]

    [Header("Level Configuration")]
    [SerializeField] private bool isLastLevel = false;
    [Tooltip("Check this box if this is the final level where height should be submitted to leaderboard")]

    [Header("Height Display (Last Level Only)")]
    [SerializeField] private Text heightDisplayText;
    [SerializeField] private GameObject heightDisplayPanel;
    [Tooltip("UI Text component to display current height - only shown on last level")]

    // Private variables
    private GameObject Player;
    private float Max_Height = 0;
    private Vector3 Top_Left;
    private Vector3 Camera_Pos;
    private bool Game_Over = false;
    private bool heightSubmitted = false;
    private bool heightTargetCompleted = false;
    private WhiteCellManager whiteCellManager;

    public static bool CanShoot { get; private set; }

    // Enum to define level types
    public enum LevelType
    {
        EnemyKills,
        HeightTarget
    }

    void Awake()
    {
        Player = GameObject.Find("Doodler");

        whiteCellManager = FindObjectOfType<WhiteCellManager>();
        if (whiteCellManager == null)
        {
            Debug.LogError("GameController: WhiteCellManager not found in scene!");
        }

        Camera_Pos = Camera.main.transform.position;
        Top_Left = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));

        RegisterEvents();
        InitializeLevelTargets();
        InitializeHeightDisplay();

        if (guidePanel != null)
        {
            guidePanel.SetActive(true);
            Time.timeScale = 0;
        }
        
        CanShoot = true;
        
        if (SettingsDataHolder.Instance == null)
        {
            Clear();
            SceneManager.LoadScene("MainMenu");
        }

        LogLevelConfiguration();
    }
    
    private IEnumerator ReturnToMainMenuSafely()
    {
        // Wait for end of frame to ensure clean transition
        yield return new WaitForEndOfFrame();
    }

    private void LogLevelConfiguration()
    {
        Debug.Log($"GameController: Level Type = {levelType}");
        if (levelType == LevelType.HeightTarget)
        {
            Debug.Log($"GameController: Target Height = {targetHeight} units");
        }
        if (isLastLevel)
        {
            Debug.Log("GameController: This is marked as the LAST LEVEL - height will be submitted to leaderboard");
        }
    }

    private void InitializeLevelTargets()
    {
        switch (levelType)
        {
            case LevelType.EnemyKills:
                InitializeEnemyTargets();
                HideHeightTargetUI();
                break;
            
            case LevelType.HeightTarget:
                InitializeHeightTarget();
                HideEnemyTargetUI();
                break;
        }
    }

    private void InitializeHeightTarget()
    {
        Debug.Log($"GameController: Initializing height target level - Target: {targetHeight} units");
        
        // Show height target UI
        if (heightTargetPanel != null)
        {
            heightTargetPanel.SetActive(true);
        }
        
        // Initialize progress fill image
        if (heightProgressFill != null)
        {
            heightProgressFill.fillAmount = 0f;
            
            // Ensure the image is set to Filled type for proper progress display
            if (heightProgressFill.type != Image.Type.Filled)
            {
                Debug.LogWarning("GameController: Height progress fill image should have Image Type set to 'Filled' for best results!");
            }
            
            Debug.Log("GameController: Height progress fill image initialized (fillAmount = 0)");
        }
        else
        {
            Debug.LogWarning("GameController: Height progress fill image not assigned for height target level!");
        }
        
        UpdateHeightTargetDisplay();
        heightTargetCompleted = false;
    }

    private void InitializeEnemyTargets()
    {
        if (enemyKillTargets == null || enemyKillTargets.Length == 0)
        {
            Debug.LogWarning("GameController: No enemy kill targets set up!");
            return;
        }

        foreach (var target in enemyKillTargets)
        {
            target.ResetProgress();
            Debug.Log($"GameController: Target set - {target.GetDisplayName()}: 0/{target.requiredKills}");
        }
        
        UpdateAllTargetUI();
    }

    private void HideHeightTargetUI()
    {
        if (heightTargetPanel != null)
        {
            heightTargetPanel.SetActive(false);
        }
    }

    private void HideEnemyTargetUI()
    {
        Debug.Log("GameController: Hiding enemy target UI for height target level");
        // Add code here to hide enemy target UI panels if they exist
    }

    private void UpdateHeightTargetDisplay()
    {
        if (levelType == LevelType.HeightTarget)
        {
            float progress = Mathf.Clamp01(Max_Height / targetHeight);
            
            // Update text display
            if (heightTargetText != null)
            {
                heightTargetText.text = $"Height: {Max_Height:F1}/{targetHeight:F1}m ({progress:P0})";
            }
            
            // Update progress fill image
            if (heightProgressFill != null)
            {
                heightProgressFill.fillAmount = progress;
                
                // Optional: Change fill color as it progresses
                // Uncomment this if you want color changes
                /*
                if (progress < 0.5f)
                    heightProgressFill.color = Color.Lerp(Color.red, Color.yellow, progress * 2f);
                else
                    heightProgressFill.color = Color.Lerp(Color.yellow, Color.green, (progress - 0.5f) * 2f);
                */
            }
        }
    }

    private void InitializeHeightDisplay()
    {
        if (isLastLevel)
        {
            if (heightDisplayPanel != null)
            {
                heightDisplayPanel.SetActive(true);
            }
            
            if (heightDisplayText != null)
            {
                UpdateHeightDisplay();
            }
            else
            {
                Debug.LogWarning("GameController: Height display text not assigned but this is the last level!");
            }
        }
        else
        {
            if (heightDisplayPanel != null && levelType != LevelType.HeightTarget)
            {
                heightDisplayPanel.SetActive(false);
            }
        }
    }

    private void UpdateHeightDisplay()
    {
        if (heightDisplayText != null && isLastLevel)
        {
            heightDisplayText.text = $"{Max_Height:F1}m";
        }
    }

    private void CheckHeightTarget()
    {
        if (levelType == LevelType.HeightTarget && !heightTargetCompleted && Max_Height >= targetHeight)
        {
            heightTargetCompleted = true;
            Debug.Log($"GameController: Height target reached! Current height: {Max_Height:F1}, Target: {targetHeight:F1}");
            OnLevelCompleted();
        }
    }
        
    static void Clear()
    {
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }
        
    public void OnGotItClicked()
    {
        Time.timeScale = 1;
    }

    private void OnDestroy()
    {
        UnRegisterEvents();
    }

    private void RegisterEvents()
    {
        EventManager.StartListening<EventName, GameObject>(EventName.OnEnemyKilled, OnEnemyKilled);
        EventManager.StartListening<EventName>(EventName.OnGameFailed, OnGameFailed);
    }

    private void UnRegisterEvents()
    {
        EventManager.StopListening<EventName, GameObject>(EventName.OnEnemyKilled, OnEnemyKilled);
        EventManager.StopListening<EventName>(EventName.OnGameFailed, OnGameFailed);
    }

    public void OnEnemyKilled(GameObject enemyObject)
    {
        if (levelType != LevelType.EnemyKills) return;
        if (enemyObject == null) return;

        Enemy enemyComponent = enemyObject.GetComponent<Enemy>();
        if (enemyComponent == null)
        {
            Debug.LogWarning("GameController: Killed object has no Enemy component!");
            return;
        }

        EnemyType killedEnemyType = enemyComponent.EnemyType;
        
        var matchingTarget = enemyKillTargets.FirstOrDefault(target => target.enemyType == killedEnemyType);
        if (matchingTarget != null)
        {
            matchingTarget.AddKill();
            Debug.Log($"GameController: {killedEnemyType} killed. Progress: {matchingTarget.CurrentKills}/{matchingTarget.requiredKills}");
        }
        else
        {
            Debug.LogWarning($"GameController: No target found for enemy type: {killedEnemyType}");
        }

        UpdateAllTargetUI();

        if (AreAllTargetsCompleted())
        {
            Debug.Log("GameController: All enemy targets completed! Player wins!");
            OnLevelCompleted();
        }
    }

    private bool AreAllTargetsCompleted()
    {
        switch (levelType)
        {
            case LevelType.EnemyKills:
                if (enemyKillTargets == null || enemyKillTargets.Length == 0)
                {
                    Debug.LogWarning("GameController: No enemy targets to check!");
                    return false;
                }

                foreach (var target in enemyKillTargets)
                {
                    if (!target.IsCompleted)
                    {
                        return false;
                    }
                }
                return true;

            case LevelType.HeightTarget:
                return heightTargetCompleted;

            default:
                return false;
        }
    }

    private void UpdateAllTargetUI()
    {
        if (levelType == LevelType.EnemyKills && enemyKillTargets != null)
        {
            foreach (var target in enemyKillTargets)
            {
                target.UpdateUI();
            }
        }
        else if (levelType == LevelType.HeightTarget)
        {
            UpdateHeightTargetDisplay();
        }
    }

    private void OnLevelCompleted()
    {
        Debug.Log($"GameController: Level completed! Level Type: {levelType}");
        
        if (isLastLevel)
        {
            SubmitHeightToLeaderboard("Level Completed");
        }
        
        int whiteCellsEarned = 0;
        if (whiteCellManager != null)
        {
            whiteCellsEarned = whiteCellManager.CurrentWhiteCells;
            Debug.Log($"GameController: Player has {whiteCellsEarned} white cells remaining");
        }
        
        int currentLevel = GetCurrentLevelNumber();
        Debug.Log($"GameController: Current level detected as: {currentLevel}");
        
        if (currentLevel > 0)
        {
            LevelProgressionManager.Instance.CompleteLevel(currentLevel, whiteCellsEarned);
            Debug.Log($"GameController: Level {currentLevel} completed with {whiteCellsEarned} white cells remaining");
        }
        
        DestroyAllObjects();
        StartCoroutine(LoadNextSceneWithDelay(1f));
    }

    private int GetCurrentLevelNumber()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"GameController: Extracting level number from scene name: '{sceneName}'");
        
        if (sceneName.StartsWith("Level "))
        {
            string levelNumberStr = sceneName.Substring(6);
            if (int.TryParse(levelNumberStr, out int levelNumber))
            {
                Debug.Log($"GameController: Extracted level number {levelNumber} from format 'Level X'");
                return levelNumber;
            }
        }
        
        if (sceneName.StartsWith("Level"))
        {
            string levelNumberStr = sceneName.Substring(5);
            if (int.TryParse(levelNumberStr, out int levelNumber))
            {
                Debug.Log($"GameController: Extracted level number {levelNumber} from format 'LevelX'");
                return levelNumber;
            }
        }
        
        if (int.TryParse(sceneName, out int directNumber))
        {
            Debug.Log($"GameController: Extracted level number {directNumber} from direct number format");
            return directNumber;
        }
        
        Debug.LogError($"GameController: Could not extract level number from scene name: '{sceneName}'");
        return 0;
    }

    private void OnGameFailed()
    {
        Debug.Log("GameController: Game failed due to no white cells remaining!");
        
        if (isLastLevel)
        {
            SubmitHeightToLeaderboard("Game Failed");
        }
        
        GetComponent<AudioSource>().Play();
        Set_GameOver();
        Game_Over = true;
    }

    private void SubmitHeightToLeaderboard(string reason)
    {
        if (heightSubmitted)
        {
            Debug.Log("GameController: Height already submitted, skipping duplicate submission");
            return;
        }

        if (GamingServices.Instance == null)
        {
            Debug.LogWarning("GameController: GamingServices not available, cannot submit height to leaderboard");
            return;
        }

        if (GamingServices.LeaderboardManager == null)
        {
            Debug.LogWarning("GameController: LeaderboardManager not available, cannot submit height to leaderboard");
            return;
        }

        int heightScore = Mathf.RoundToInt(Max_Height);
        
        Debug.Log($"GameController: Submitting height to leaderboard - Reason: {reason}, Height: {Max_Height:F2}u, Score: {heightScore}");
        
        try
        {
            LeaderboardManager.UpdateScore(heightScore);
            heightSubmitted = true;
            Debug.Log($"GameController: Height successfully submitted to leaderboard! Final height: {Max_Height:F2} units");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"GameController: Failed to submit height to leaderboard: {e.Message}");
        }
    }

    public float GetCurrentMaxHeight()
    {
        return Max_Height;
    }

    public bool IsLastLevel()
    {
        return isLastLevel;
    }

    public LevelType GetLevelType()
    {
        return levelType;
    }

    public float GetTargetHeight()
    {
        return targetHeight;
    }

    public float GetHeightProgress()
    {
        if (levelType != LevelType.HeightTarget) return 0f;
        return Mathf.Clamp01(Max_Height / targetHeight);
    }

    private void DestroyAllObjects()
    {
        var platforms = FindObjectsByType<Platform>(FindObjectsSortMode.None);
        foreach (var platform in platforms)
        {
            Destroy(platform.gameObject);
        }

        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }

        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            Destroy(player.gameObject);
        }
    }

    private IEnumerator LoadNextSceneWithDelay(float waitTime)
    {
        Debug.Log($"GameController: Loading WinScene in {waitTime} seconds...");
        yield return new WaitForSeconds(waitTime);
        
        Debug.Log("GameController: Loading WinScene after level completion");
        SceneManager.LoadScene("WinScene");
    }

    void FixedUpdate()
    {
        if (!Game_Over)
        {
            if (Player == null) return;
            
            if (Player.transform.position.y > Max_Height)
            {
                Max_Height = Player.transform.position.y;
                
                if (isLastLevel)
                {
                    UpdateHeightDisplay();
                }
                
                if (levelType == LevelType.HeightTarget)
                {
                    UpdateHeightTargetDisplay();
                    CheckHeightTarget();
                }
                
                if (Mathf.RoundToInt(Max_Height) % 10 == 0 && Max_Height > 0)
                {
                    if (isLastLevel || levelType == LevelType.HeightTarget)
                    {
                        Debug.Log($"GameController: Height milestone reached: {Max_Height:F1}u");
                    }
                }
            }

            if (Player.transform.position.y - Camera.main.transform.position.y < Get_DestroyDistance())
            {
                GetComponent<AudioSource>().Play();
                Set_GameOver();
                Game_Over = true;
            }
        }
    }

    public bool Get_GameOver()
    {
        return Game_Over;
    }

    public float Get_DestroyDistance()
    {
        return Camera_Pos.y + Top_Left.y + destroyOffset;
    }

    public void Set_GameOver()
    {
        CanShoot = false;

        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }

        var platforms = FindObjectsByType<Platform>(FindObjectsSortMode.None);
        for (int i = platforms.Length - 1; i >= 0; i--)
        {
            Destroy(platforms[i].gameObject);
        }
        
        DestroyAllPickupsAndPowerups();
        DestroyAllProjectiles();
        DestroyObjectsWithTags();

        int totalScore = 0;
        switch (levelType)
        {
            case LevelType.EnemyKills:
                totalScore = enemyKillTargets.Sum(target => target.CurrentKills);
                break;
            case LevelType.HeightTarget:
                totalScore = Mathf.RoundToInt(Max_Height);
                break;
        }

        if (Data_Manager.Get_HighScore() < totalScore)
            Data_Manager.Set_HighScore(totalScore);

        var backgroundCanvas = GameObject.FindObjectsByType<GameOverCanvas>(FindObjectsSortMode.None)[0];
        Button_OnClick.Set_GameOverMenu(true);
        backgroundCanvas.EnableAnimator();
        File_Manager.Save_Info();
        
        Debug.Log("GameController: Game over - player can now fall and be visible");
        SubmitHeightToLeaderboard("Game Over");
    }
    
    private void DestroyAllPickupsAndPowerups()
    {
        var bloodPickups = FindObjectsByType<BloodTransfusionPickup>(FindObjectsSortMode.None);
        foreach (var pickup in bloodPickups) Destroy(pickup.gameObject);
        
        var fluidPickups = FindObjectsByType<FluidPickup>(FindObjectsSortMode.None);
        foreach (var pickup in fluidPickups) Destroy(pickup.gameObject);
    }
    
    private void DestroyAllProjectiles()
    {
        var bullets = FindObjectsByType<Bullet>(FindObjectsSortMode.None);
        foreach (var bullet in bullets) Destroy(bullet.gameObject);
        
        var laserBullets = FindObjectsByType<LaserBullet>(FindObjectsSortMode.None);
        foreach (var laser in laserBullets) Destroy(laser.gameObject);
    }
    
    private void DestroyObjectsWithTags()
    {
        string[] tagsToDestroy = { 
            "Pickup", "PowerUp", "Boost", "Collectible", 
            "Bullet", "Laser", "Projectile", "Obstacle",
            "Temporary", "Spawned"
        };
        
        foreach (string tag in tagsToDestroy)
        {
            try
            {
                GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);
                foreach (GameObject obj in objectsWithTag) Destroy(obj);
            }
            catch (UnityEngine.UnityException)
            {
                continue;
            }
        }
    }

    // Debug Methods
    [ContextMenu("Debug - Show All Targets")]
    public void DebugShowAllTargets()
    {
        switch (levelType)
        {
            case LevelType.EnemyKills:
                Debug.Log("=== Enemy Kill Targets ===");
                for (int i = 0; i < enemyKillTargets.Length; i++)
                {
                    var target = enemyKillTargets[i];
                    Debug.Log($"{i}: {target.GetDisplayName()} - {target.CurrentKills}/{target.requiredKills} ({target.Progress:P1})");
                }
                break;
                
            case LevelType.HeightTarget:
                Debug.Log("=== Height Target ===");
                Debug.Log($"Current Height: {Max_Height:F1} | Target Height: {targetHeight:F1} | Progress: {GetHeightProgress():P1} | Completed: {heightTargetCompleted}");
                break;
        }
    }

    [ContextMenu("Debug - Force Complete Level")]
    public void DebugForceCompleteLevel()
    {
        switch (levelType)
        {
            case LevelType.EnemyKills:
                Debug.Log("Debug: Force completing enemy kill level");
                foreach (var target in enemyKillTargets)
                {
                    while (!target.IsCompleted)
                    {
                        target.AddKill();
                    }
                }
                OnLevelCompleted();
                break;
                
            case LevelType.HeightTarget:
                Debug.Log("Debug: Force completing height target level");
                Max_Height = targetHeight;
                CheckHeightTarget();
                break;
        }
    }

    [ContextMenu("Debug - Force Submit Height")]
    public void DebugForceSubmitHeight()
    {
        if (isLastLevel)
        {
            heightSubmitted = false;
            SubmitHeightToLeaderboard("Debug Force Submit");
        }
        else
        {
            Debug.Log("Debug: Cannot submit height - this level is not marked as the last level");
        }
    }

    [ContextMenu("Debug - Show Current Status")]
    public void DebugShowCurrentStatus()
    {
        Debug.Log($"Level Type: {levelType} | Max Height: {Max_Height:F2} | Is Last Level: {isLastLevel} | Height Submitted: {heightSubmitted}");
        if (levelType == LevelType.HeightTarget)
        {
            float progress = GetHeightProgress();
            Debug.Log($"Target Height: {targetHeight:F1} | Progress: {progress:P1} | Fill Amount: {(heightProgressFill != null ? heightProgressFill.fillAmount.ToString("F3") : "N/A")} | Completed: {heightTargetCompleted}");
        }
    }
}