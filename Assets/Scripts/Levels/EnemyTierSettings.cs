using UnityEngine;

[System.Serializable]
public class EnemyTierSettings
{
    [Header("Basic Settings")]
    public float minYToSpawn = 20f;
    public int spawnChance = 12; // 1 in N chance to spawn enemies at this tier
    
    [Header("Static Enemy Pool")]
    public GameObject[] staticEnemyPrefabs;
    
    [Header("Moving Enemy Pool")]
    public GameObject[] movingEnemyPrefabs;
    
    [Header("Single Enemy Settings")]
    [Range(0f, 1f)]
    public float movingEnemyRatio = 0.3f; // 30% chance for moving, 70% for static
    
    [Header("Chain Settings")]
    [Range(0f, 100f)]
    public float chainSpawnChance = 20f; // % chance to spawn chain instead of single
    public int minChainLength = 2;
    public int maxChainLength = 4;
    [Tooltip("Should chain enemies be all the same type?")]
    public bool uniformChainEnemies = true;
    
    [Header("Screen Blocking Settings")]
    [Range(0f, 100f)]
    public float screenBlockChance = 5f; // % chance to spawn screen blocking chain
    public float screenBlockGapSize = 0.5f; // Gap between enemies in screen block
    [Tooltip("Which enemy type to use for screen blocking (uses static by default)")]
    public bool useMovingEnemiesForScreenBlock = false;
    
    // Validation
    private void OnValidate()
    {
        if (staticEnemyPrefabs == null || staticEnemyPrefabs.Length == 0)
        {
            Debug.LogWarning($"EnemyTierSettings: No static enemy prefabs assigned for tier starting at Y={minYToSpawn}");
        }
        
        if (movingEnemyPrefabs == null || movingEnemyPrefabs.Length == 0)
        {
            Debug.LogWarning($"EnemyTierSettings: No moving enemy prefabs assigned for tier starting at Y={minYToSpawn}. Moving enemies will not spawn.");
        }
    }
    
    /// <summary>
    /// Gets a random static enemy prefab from the pool
    /// </summary>
    public GameObject GetRandomStaticEnemy()
    {
        if (staticEnemyPrefabs == null || staticEnemyPrefabs.Length == 0)
            return null;
            
        return staticEnemyPrefabs[Random.Range(0, staticEnemyPrefabs.Length)];
    }
    
    /// <summary>
    /// Gets a random moving enemy prefab from the pool
    /// </summary>
    public GameObject GetRandomMovingEnemy()
    {
        if (movingEnemyPrefabs == null || movingEnemyPrefabs.Length == 0)
            return null;
            
        return movingEnemyPrefabs[Random.Range(0, movingEnemyPrefabs.Length)];
    }
    
    /// <summary>
    /// Gets a random enemy based on the moving/static ratio
    /// </summary>
    public GameObject GetRandomEnemy()
    {
        bool shouldUseMoving = Random.Range(0f, 1f) < movingEnemyRatio;
        
        if (shouldUseMoving)
        {
            GameObject movingEnemy = GetRandomMovingEnemy();
            // Fallback to static if no moving enemies available
            return movingEnemy != null ? movingEnemy : GetRandomStaticEnemy();
        }
        else
        {
            GameObject staticEnemy = GetRandomStaticEnemy();
            // Fallback to moving if no static enemies available
            return staticEnemy != null ? staticEnemy : GetRandomMovingEnemy();
        }
    }
    
    /// <summary>
    /// Gets the appropriate enemy for screen blocking
    /// </summary>
    public GameObject GetScreenBlockEnemy()
    {
        if (useMovingEnemiesForScreenBlock)
        {
            GameObject movingEnemy = GetRandomMovingEnemy();
            return movingEnemy != null ? movingEnemy : GetRandomStaticEnemy();
        }
        else
        {
            GameObject staticEnemy = GetRandomStaticEnemy();
            return staticEnemy != null ? staticEnemy : GetRandomMovingEnemy();
        }
    }
    
    /// <summary>
    /// Checks if this tier has any valid enemies to spawn
    /// </summary>
    public bool HasValidEnemies()
    {
        return (staticEnemyPrefabs != null && staticEnemyPrefabs.Length > 0) ||
               (movingEnemyPrefabs != null && movingEnemyPrefabs.Length > 0);
    }
}