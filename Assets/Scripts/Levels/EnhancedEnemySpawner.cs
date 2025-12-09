using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnhancedEnemySpawner : MonoBehaviour
{
    [Header("Enemy Tier Settings")]
    public EnemyTierSettings[] enemyTiers;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    private SpawnBoundaryCalculator boundaryCalculator;
    
    private void Start()
    {
        boundaryCalculator = new SpawnBoundaryCalculator(1.2f);
        ValidateTiers();
    }

    private void OnEnable()
    {
        EventManager.StartListening<EventName, float, float>(EventName.SpawnEnemy, TrySpawnEnemies);
    }

    private void OnDisable()
    {
        EventManager.StopListening<EventName, float, float>(EventName.SpawnEnemy, TrySpawnEnemies);

    }
    

    private void ValidateTiers()
    {
        if (enemyTiers == null || enemyTiers.Length == 0)
        {
            Debug.LogError("EnhancedEnemySpawner: No enemy tiers configured!");
            return;
        }
        
        for (int i = 0; i < enemyTiers.Length; i++)
        {
            if (!enemyTiers[i].HasValidEnemies())
            {
                Debug.LogWarning($"EnhancedEnemySpawner: Tier {i} has no valid enemy prefabs!");
            }
        }
    }
    
    public void TrySpawnEnemies(float height, float spacing)
    {
        EnemyTierSettings activeSettings = GetActiveTierSettings(height);
        if (activeSettings == null || !activeSettings.HasValidEnemies()) 
        {
            if (showDebugLogs)
                Debug.Log($"No valid enemy tier found for height {height}");
            return;
        }
        
        if (Random.Range(0, activeSettings.spawnChance) != 0) 
        {
            if (showDebugLogs)
                Debug.Log($"Enemy spawn chance failed at height {height}");
            return;
        }
        
        EnemySpawnType spawnType = DetermineSpawnType(activeSettings);
        var boundaries = boundaryCalculator.GetBoundaries();
        
        if (showDebugLogs)
            Debug.Log($"Spawning {spawnType} enemies at height {height}");
        
        switch (spawnType)
        {
            case EnemySpawnType.Single:
                SpawnSingleEnemy(height, spacing, activeSettings, boundaries);
                break;
            case EnemySpawnType.Chain:
                SpawnEnemyChain(height, spacing, activeSettings, boundaries);
                break;
            case EnemySpawnType.ScreenBlock:
                SpawnScreenBlockingChain(height, spacing, activeSettings, boundaries);
                break;
        }
    }
    
    private EnemyTierSettings GetActiveTierSettings(float height)
    {
        // Find the highest tier that matches the height requirement
        EnemyTierSettings bestTier = null;
        
        for (int i = 0; i < enemyTiers.Length; i++)
        {
            if (height >= enemyTiers[i].minYToSpawn)
            {
                if (bestTier == null || enemyTiers[i].minYToSpawn > bestTier.minYToSpawn)
                {
                    bestTier = enemyTiers[i];
                }
            }
        }
        
        return bestTier;
    }
    
    private EnemySpawnType DetermineSpawnType(EnemyTierSettings settings)
    {
        float rand = Random.Range(0f, 100f);
        
        if (rand < settings.screenBlockChance)
            return EnemySpawnType.ScreenBlock;
        else if (rand < settings.screenBlockChance + settings.chainSpawnChance)
            return EnemySpawnType.Chain;
        else
            return EnemySpawnType.Single;
    }
    
    private void SpawnSingleEnemy(float height, float spacing, EnemyTierSettings settings, SpawnBoundaries boundaries)
    {
        float x = Random.Range(boundaries.left, boundaries.right);
        float y = Random.Range(height + 0.5f, height + spacing - 0.5f);
        
        GameObject enemyPrefab = settings.GetRandomEnemy();
        
        if (enemyPrefab != null)
        {
            GameObject spawnedEnemy = Instantiate(enemyPrefab, new Vector3(x, y, 0), Quaternion.identity);
            
            if (showDebugLogs)
                Debug.Log($"Spawned single enemy: {enemyPrefab.name} at ({x}, {y})");
        }
    }
    
    private void SpawnEnemyChain(float height, float spacing, EnemyTierSettings settings, SpawnBoundaries boundaries)
    {
        int chainLength = Random.Range(settings.minChainLength, settings.maxChainLength + 1);
        float y = Random.Range(height + 0.5f, height + spacing - 0.5f);
        
        GameObject enemyToUse = null;
        
        // If uniform chain, select one enemy type for the entire chain
        if (settings.uniformChainEnemies)
        {
            enemyToUse = settings.GetRandomEnemy();
        }
        
        float totalWidth = boundaries.right - boundaries.left;
        float enemySpacing = totalWidth / (chainLength + 1);
        
        for (int i = 0; i < chainLength; i++)
        {
            float x = boundaries.left + enemySpacing * (i + 1);
            x = Mathf.Clamp(x, boundaries.left, boundaries.right);
            
            // If not uniform chain, get a new random enemy for each position
            GameObject currentEnemyPrefab = settings.uniformChainEnemies ? enemyToUse : settings.GetRandomEnemy();
            
            if (currentEnemyPrefab != null)
            {
                GameObject spawnedEnemy = Instantiate(currentEnemyPrefab, new Vector3(x, y, 0), Quaternion.identity);
                
                if (showDebugLogs)
                    Debug.Log($"Spawned chain enemy {i+1}/{chainLength}: {currentEnemyPrefab.name} at ({x}, {y})");
            }
        }
    }
    
    private void SpawnScreenBlockingChain(float height, float spacing, EnemyTierSettings settings, SpawnBoundaries boundaries)
    {
        float y = Random.Range(height + 0.5f, height + spacing - 0.5f);
        
        GameObject enemyPrefab = settings.GetScreenBlockEnemy();
        if (enemyPrefab == null) return;
        
        float enemyWidth = GetEnemyWidth(enemyPrefab);
        float gapSize = settings.screenBlockGapSize;
        float totalWidth = boundaries.right - boundaries.left;
        
        int enemyCount = Mathf.FloorToInt(totalWidth / (enemyWidth + gapSize));
        if (enemyCount <= 1) return; // Not enough space for screen block
        
        float actualSpacing = totalWidth / enemyCount;
        
        // Leave one gap for player to pass through
        int gapIndex = Random.Range(0, enemyCount);
        
        for (int i = 0; i < enemyCount; i++)
        {
            if (i == gapIndex) continue; // Skip one enemy to create gap
            
            float x = boundaries.left + actualSpacing * i + actualSpacing * 0.5f;
            x = Mathf.Clamp(x, boundaries.left, boundaries.right);
            
            GameObject spawnedEnemy = Instantiate(enemyPrefab, new Vector3(x, y, 0), Quaternion.identity);
            
            if (showDebugLogs)
                Debug.Log($"Spawned screen block enemy {i+1}/{enemyCount}: {enemyPrefab.name} at ({x}, {y})");
        }
    }
    
    private float GetEnemyWidth(GameObject enemyPrefab)
    {
        Collider2D collider = enemyPrefab.GetComponent<Collider2D>();
        return collider != null ? collider.bounds.size.x : 1f;
    }
    
    // Public method to get tier info (useful for debugging or UI)
    public EnemyTierSettings GetTierForHeight(float height)
    {
        return GetActiveTierSettings(height);
    }
    
    // Public method to force spawn enemies (useful for testing)
    public void ForceSpawnEnemies(float height, float spacing, EnemySpawnType spawnType)
    {
        EnemyTierSettings activeSettings = GetActiveTierSettings(height);
        if (activeSettings == null || !activeSettings.HasValidEnemies()) return;
        
        var boundaries = boundaryCalculator.GetBoundaries();
        
        switch (spawnType)
        {
            case EnemySpawnType.Single:
                SpawnSingleEnemy(height, spacing, activeSettings, boundaries);
                break;
            case EnemySpawnType.Chain:
                SpawnEnemyChain(height, spacing, activeSettings, boundaries);
                break;
            case EnemySpawnType.ScreenBlock:
                SpawnScreenBlockingChain(height, spacing, activeSettings, boundaries);
                break;
        }
    }
}