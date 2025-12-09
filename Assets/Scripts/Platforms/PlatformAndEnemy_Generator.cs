using UnityEngine;

public class PlatformAndEnemyGenerator : MonoBehaviour
{
    [Header("Platform Settings")]
    public PlatformSpawnRange[] platformTiers;

    [Header("Enemy Settings")] 
    public bool useNewEnemySpawner = false;
    public EnemySpawnSettings[] enemyTiers;

    [Header("Boost Settings")]
    public BoostSpawnSettings boostSettings;
    
    [Header("Oxygen Settings")]
    public GameObject oxygenPickupPrefab;
    public float oxygenSpawnOffsetY = 2f;

    [Header("Spawn Settings")]
    public float verticalSpacingMin = 1f;
    public float verticalSpacingMax = 5f;
    public float initialOffset = 1.2f;
    public float startHeight = 0f;
    public int initialPlatformCount = 10;
    public float spawnAheadDistance = 25f; // Increased from 15f - spawn much further ahead
    public float checkInterval = 0.5f; // How often to check for new spawning

    [Header("Wall Settings")]
    public float wallOffset = 3f;
    public bool autoDetectWalls = true;
    public string wallTag = "Wall";

    private float screenLeft, screenRight;
    private float wallLeft, wallRight;
    private float highestSpawnedY; // Track the highest point we've spawned content
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("PlatformGenerator: No main camera found!");
            return;
        }

        CalculateSpawnBoundaries();
        
        // Initialize with starting height
        highestSpawnedY = startHeight;
        
        // Generate initial platforms
        GenerateInitialPlatforms();
        
        // Start continuous checking for new content
        InvokeRepeating(nameof(CheckAndSpawnNewContent), checkInterval, checkInterval);
    }

    private void CalculateSpawnBoundaries()
    {
        Vector3 bottomLeft = mainCamera.ScreenToWorldPoint(Vector3.zero);
        Vector3 bottomRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
        
        screenLeft = bottomLeft.x + initialOffset;
        screenRight = bottomRight.x - initialOffset;

        if (autoDetectWalls)
        {
            FindWallBoundaries();
        }
        else
        {
            wallLeft = screenLeft;
            wallRight = screenRight;
        }

        screenLeft = Mathf.Max(screenLeft, wallLeft + wallOffset);
        screenRight = Mathf.Min(screenRight, wallRight - wallOffset);

        Debug.Log($"PlatformGenerator: Spawn boundaries - Left: {screenLeft}, Right: {screenRight}");
    }

    private void FindWallBoundaries()
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag(wallTag);
        
        if (walls.Length >= 2)
        {
            float leftmost = float.MaxValue;
            float rightmost = float.MinValue;

            foreach (GameObject wall in walls)
            {
                Collider2D wallCollider = wall.GetComponent<Collider2D>();
                if (wallCollider != null)
                {
                    Bounds bounds = wallCollider.bounds;
                    
                    if (wall.transform.position.x < 0)
                    {
                        leftmost = Mathf.Min(leftmost, bounds.max.x);
                    }
                    else
                    {
                        rightmost = Mathf.Max(rightmost, bounds.min.x);
                    }
                }
            }

            wallLeft = leftmost != float.MaxValue ? leftmost : screenLeft;
            wallRight = rightmost != float.MinValue ? rightmost : screenRight;
        }
        else
        {
            Debug.LogWarning($"PlatformGenerator: Could not find 2 walls with tag '{wallTag}'. Using screen boundaries.");
            wallLeft = screenLeft;
            wallRight = screenRight;
        }
    }

    private void GenerateInitialPlatforms()
    {
        for (int i = 0; i < initialPlatformCount; i++)
        {
            float spacing = Random.Range(verticalSpacingMin, verticalSpacingMax);
            highestSpawnedY += spacing;
            
            SpawnPlatformAt(highestSpawnedY, true);
        }
        
        Debug.Log($"PlatformGenerator: Generated {initialPlatformCount} initial platforms up to height {highestSpawnedY}");
    }

    private void CheckAndSpawnNewContent()
    {
        if (mainCamera == null) return;

        // Calculate how high we need content
        float cameraY = mainCamera.transform.position.y;
        float cameraHeight = GetCameraHeight();
        float cameraTopY = cameraY + (cameraHeight / 2f);
        float neededSpawnHeight = cameraTopY + spawnAheadDistance;

        // Check if we need to spawn more content
        if (highestSpawnedY < neededSpawnHeight)
        {
            // Calculate how many platforms we need
            float heightToFill = neededSpawnHeight - highestSpawnedY;
            float averageSpacing = (verticalSpacingMin + verticalSpacingMax) / 2f;
            int platformsNeeded = Mathf.CeilToInt(heightToFill / averageSpacing);
            
            // Spawn at least 3 platforms at a time for efficiency
            platformsNeeded = Mathf.Max(platformsNeeded, 3);
            
            SpawnAdditionalPlatforms(platformsNeeded);
            
            Debug.Log($"PlatformGenerator: Camera at {cameraY:F1}, spawned {platformsNeeded} platforms up to {highestSpawnedY:F1}");
        }
    }

    private void SpawnAdditionalPlatforms(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float spacing = Random.Range(verticalSpacingMin, verticalSpacingMax);
            highestSpawnedY += spacing;
            
            SpawnPlatformAt(highestSpawnedY, false);
        }
    }

    private void SpawnPlatformAt(float yPosition, bool isInitial)
    {
        float x = Random.Range(screenLeft, screenRight);
        Vector3 platformPosition = new Vector3(x, yPosition, 0);

        // Spawn enemies (oxygen will be spawned automatically if a chain is detected)
        if (useNewEnemySpawner)
        {
            EventManager.TriggerEvent<EventName, float, float>(EventName.SpawnEnemy, yPosition, verticalSpacingMax);
        }
        else
        {
            SpawnEnemiesIfEligible(yPosition, isInitial);
        }
        
        // Spawn platform
        GameObject platform = SpawnPlatform(platformPosition);
        
        // Attach boost
        TryAttachBoost(platform, platformPosition);
    }



    private void SpawnEnemiesIfEligible(float height, bool isInitial)
    {
        EnemySpawnSettings activeSettings = GetActiveEnemySettings(height);
        if (activeSettings == null) return;

        // Convert 1-in-N chance to percentage for clearer understanding
        float spawnPercentage = (100f / activeSettings.spawnChance);
        float randomRoll = Random.Range(0f, 100f);
        
        Debug.Log($"Enemy spawn check at height {height:F1}: Need < {spawnPercentage:F1}%, rolled {randomRoll:F1}%");
        
        // Check spawn chance - if random roll is less than spawn percentage, spawn enemies
        if (randomRoll >= spawnPercentage) return;

        // Determine spawn type
        EnemySpawnType spawnType = DetermineSpawnType(activeSettings);
        
        Debug.Log($"Spawning {spawnType} enemies at height {height:F1}");
        
        // Check if we're spawning a chain and maybe spawn oxygen before it
        if ((spawnType == EnemySpawnType.Chain || spawnType == EnemySpawnType.ScreenBlock) && !isInitial)
        {
            CheckForOxygenSpawnBeforeChain(height, spawnType);
        }
        
        switch (spawnType)
        {
            case EnemySpawnType.Single:
                SpawnSingleEnemy(height, activeSettings);
                break;
            case EnemySpawnType.Chain:
                SpawnEnemyChain(height, activeSettings);
                break;
            case EnemySpawnType.ScreenBlock:
                SpawnScreenBlockingChain(height, activeSettings);
                break;
        }
    }
    
    private void CheckForOxygenSpawnBeforeChain(float chainHeight, EnemySpawnType chainType)
    {
        if (oxygenPickupPrefab == null) 
        {
            Debug.LogWarning("PlatformGenerator: oxygenPickupPrefab is null - cannot spawn oxygen");
            return;
        }
        
        // Simple 50% probability to spawn oxygen before chain
        float oxygenChance = Random.Range(0f, 1f);
        bool shouldSpawnOxygen = oxygenChance <= 0.5f;
        
        Debug.Log($"PlatformGenerator: Chain detected at height {chainHeight:F1}, oxygen roll: {oxygenChance:F2} (need ≤ 0.5)");
        
        if (shouldSpawnOxygen)
        {
            // Spawn oxygen pickup before the chain
            float oxygenHeight = chainHeight - oxygenSpawnOffsetY;
            float x = Random.Range(screenLeft, screenRight);
            Vector3 oxygenPosition = new Vector3(x, oxygenHeight, 0);
            
            GameObject oxygen = Instantiate(oxygenPickupPrefab, oxygenPosition, Quaternion.identity);
            if (oxygen != null)
            {
                Debug.Log($"PlatformGenerator: ✅ OXYGEN SPAWNED before {chainType} at position {oxygenPosition}");
            }
            else
            {
                Debug.LogError("PlatformGenerator: Failed to instantiate oxygen pickup!");
            }
        }
        else
        {
            Debug.Log($"PlatformGenerator: No oxygen spawned before {chainType} (failed 50% chance)");
        }
    }

    private EnemySpawnSettings GetActiveEnemySettings(float height)
    {
        for (int i = enemyTiers.Length - 1; i >= 0; i--)
        {
            if (height >= enemyTiers[i].minYToSpawn)
            {
                return enemyTiers[i];
            }
        }
        return null;
    }

    private EnemySpawnType DetermineSpawnType(EnemySpawnSettings settings)
    {
        float rand = Random.Range(0f, 100f);
        
        if (rand < settings.screenBlockChance)
            return EnemySpawnType.ScreenBlock;
        else if (rand < settings.screenBlockChance + settings.chainSpawnChance)
            return EnemySpawnType.Chain;
        else
            return EnemySpawnType.Single;
    }

    private void SpawnSingleEnemy(float height, EnemySpawnSettings settings)
    {
        float x = Random.Range(screenLeft, screenRight);
        float y = height + Random.Range(0.5f, 1.5f);
        
        GameObject enemyPrefab = Random.Range(0f, 1f) < settings.movingEnemyRatio ? 
            settings.movingEnemyPrefab : settings.staticEnemyPrefab;
        
        if (enemyPrefab != null)
        {
            Instantiate(enemyPrefab, new Vector3(x, y, 0), Quaternion.identity);
        }
    }

    private void SpawnEnemyChain(float height, EnemySpawnSettings settings)
    {
        int chainLength = Random.Range(settings.minChainLength, settings.maxChainLength + 1);
        float y = height + Random.Range(0.5f, 1.5f);
        
        float totalWidth = screenRight - screenLeft;
        float enemySpacing = totalWidth / (chainLength + 1);
        
        for (int i = 0; i < chainLength; i++)
        {
            float x = screenLeft + enemySpacing * (i + 1);
            x = Mathf.Clamp(x, screenLeft, screenRight);
            
            if (settings.staticEnemyPrefab != null)
            {
                Instantiate(settings.staticEnemyPrefab, new Vector3(x, y, 0), Quaternion.identity);
            }
        }
    }

    private void SpawnScreenBlockingChain(float height, EnemySpawnSettings settings)
    {
        float y = height + Random.Range(0.5f, 1.5f);
        
        float enemyWidth = GetEnemyWidth(settings.staticEnemyPrefab);
        float gapSize = settings.screenBlockGapSize;
        float totalWidth = screenRight - screenLeft;
        
        int enemyCount = Mathf.FloorToInt(totalWidth / (enemyWidth + gapSize));
        float actualSpacing = totalWidth / enemyCount;
        
        // Leave one gap for player to pass through
        int gapIndex = Random.Range(0, enemyCount);
        
        for (int i = 0; i < enemyCount; i++)
        {
            if (i == gapIndex) continue; // Skip one enemy to create gap
            
            float x = screenLeft + actualSpacing * i + actualSpacing * 0.5f;
            x = Mathf.Clamp(x, screenLeft, screenRight);
            
            if (settings.staticEnemyPrefab != null)
            {
                Instantiate(settings.staticEnemyPrefab, new Vector3(x, y, 0), Quaternion.identity);
            }
        }
    }

    private float GetEnemyWidth(GameObject enemyPrefab)
    {
        if (enemyPrefab == null) return 1f;
        
        Collider2D collider = enemyPrefab.GetComponent<Collider2D>();
        if (collider != null)
        {
            return collider.bounds.size.x;
        }
        return 1f;
    }

    private GameObject SpawnPlatform(Vector3 position)
    {
        GameObject platformToSpawn = GetPlatformForHeight(position.y);
        
        EventManager.TriggerEvent<EventName, Vector3>(EventName.OnSpawnPlatform, position);
        return Instantiate(platformToSpawn, position, Quaternion.identity);
    }

    private GameObject GetPlatformForHeight(float height)
    {
        foreach (var tier in platformTiers)
        {
            if (height >= tier.minY && height < tier.maxY)
            {
                return tier.possiblePlatforms[Random.Range(0, tier.possiblePlatforms.Length)];
            }
        }

        // Default to last tier
        var lastTier = platformTiers[platformTiers.Length - 1];
        return lastTier.possiblePlatforms[0];
    }

    private void TryAttachBoost(GameObject platform, Vector3 platformPos)
    {
        if (platform == null || boostSettings.boostPrefabs.Length == 0)
            return;

        if (Random.Range(1, boostSettings.spawnChance + 1) == 1)
        {
            GameObject boost = boostSettings.boostPrefabs[Random.Range(0, boostSettings.boostPrefabs.Length)];
            
            Vector3 boostPosition = platformPos + boostSettings.localOffset;
            boostPosition.x = Mathf.Clamp(boostPosition.x, screenLeft, screenRight);
            
            Instantiate(boost, boostPosition, Quaternion.identity, platform.transform);
        }
    }

    private float GetCameraHeight()
    {
        if (mainCamera.orthographic)
        {
            return mainCamera.orthographicSize * 2f;
        }
        else
        {
            float distance = Mathf.Abs(mainCamera.transform.position.z);
            return 2f * distance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        }
    }

    // Public methods
    public void RecalculateBoundaries()
    {
        CalculateSpawnBoundaries();
    }

    public Vector2 GetSpawnBoundaries()
    {
        return new Vector2(screenLeft, screenRight);
    }

    public float GetHighestSpawnedY()
    {
        return highestSpawnedY;
    }

    // Compatibility method for old Platform script
    public void GeneratePlatforms(int count, bool isInitial)
    {
        Debug.Log($"PlatformGenerator: Legacy GeneratePlatforms called - spawning {count} platforms");
        SpawnAdditionalPlatforms(count);
    }

    // Method to force spawn more content (useful for testing)
    [ContextMenu("Force Spawn 5 Platforms")]
    public void ForceSpawnPlatforms()
    {
        SpawnAdditionalPlatforms(5);
    }
}

public enum EnemySpawnType
{
    Single,
    Chain,
    ScreenBlock
}

[System.Serializable]
public class PlatformSpawnRange
{
    public float minY;
    public float maxY;
    public GameObject[] possiblePlatforms;
}

[System.Serializable]
public class EnemySpawnSettings
{
    [Header("Basic Settings")]
    public float minYToSpawn = 20f;
    public int spawnChance = 12;
    
    [Header("Enemy Prefabs")]
    public GameObject staticEnemyPrefab;
    public GameObject movingEnemyPrefab;
    
    [Header("Single Enemy Settings")]
    [Range(0f, 1f)]
    public float movingEnemyRatio = 0.3f;
    
    [Header("Chain Settings")]
    [Range(0f, 100f)]
    public float chainSpawnChance = 20f;
    public int minChainLength = 2;
    public int maxChainLength = 4;
    
    [Header("Screen Blocking Settings")]
    [Range(0f, 100f)]
    public float screenBlockChance = 5f;
    public float screenBlockGapSize = 0.5f;
}

[System.Serializable]
public class BoostSpawnSettings
{
    public int spawnChance = 40;
    public GameObject[] boostPrefabs;
    public Vector3 localOffset = new Vector3(0.5f, 0.27f, 0);
}