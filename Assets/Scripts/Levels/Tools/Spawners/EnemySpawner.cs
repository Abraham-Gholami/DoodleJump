using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    /*[Header("Enemy Spawn Settings")]
    [Tooltip("Default height offset above platforms")]
    public float platformHeightOffset = 1f;
    [Tooltip("Random height variation")]
    public float heightVariation = 0.5f;
    [Tooltip("Default chain length range")]
    public int minChainLength = 2;
    public int maxChainLength = 5;
    [Tooltip("Default gap size for screen blocks")]
    public float screenBlockGapSize = 0.5f;
    
    private PlatformSpawner platformSpawner;
    
    private void Start()
    {
        InitializeSpawner();
    }
    
    private void InitializeSpawner()
    {
        platformSpawner = GetComponent<PlatformSpawner>();
        if (platformSpawner == null)
        {
            Debug.LogError("EnemySpawner: PlatformSpawner component not found!");
        }
    }
    
    public void SpawnEnemiesForPart(LevelPartData part, List<Vector3> platformPositions, float partStartY, float partEndY)
    {
        if (!ValidatePartData(part)) return;
        
        switch (part.enemyPlacement)
        {
            case EnemyPlacementType.RandomAbovePlatforms:
                SpawnRandomEnemies(part, platformPositions);
                break;
                
            case EnemyPlacementType.Chain:
                SpawnEnemyChain(part, partStartY + platformHeightOffset);
                break;
                
            case EnemyPlacementType.ScreenBlock:
                SpawnScreenBlock(part, partStartY + platformHeightOffset);
                break;
                
            case EnemyPlacementType.Mixed:
                SpawnMixedEnemies(part, platformPositions, partStartY);
                break;
        }
        
        Debug.Log($"EnemySpawner: Spawned {part.enemyPlacement} enemies for part '{part.partName}'");
    }
    
    private bool ValidatePartData(LevelPartData part)
    {
        if (part == null)
        {
            Debug.LogError("EnemySpawner: No part data provided!");
            return false;
        }
        
        if (part.enemyTypes.Count == 0)
        {
            Debug.LogError($"EnemySpawner: Part '{part.partName}' has no enemy types!");
            return false;
        }
        
        return true;
    }
    
    private void SpawnRandomEnemies(LevelPartData part, List<Vector3> platformPositions)
    {
        if (part.enemyCount <= 0) return;
        
        var spawnBoundaries = platformSpawner.GetSpawnBoundaries();
        
        for (int i = 0; i < part.enemyCount; i++)
        {
            Vector3 spawnPosition = CalculateRandomEnemyPosition(platformPositions, spawnBoundaries);
            SpawnSingleEnemy(part.enemyTypes, spawnPosition);
        }
    }
    
    private void SpawnEnemyChain(LevelPartData part, float yPosition)
    {
        var chainEnemies = GetEnemiesForChains(part.enemyTypes);
        if (chainEnemies.Count == 0)
        {
            Debug.LogError($"EnemySpawner: No chain-compatible enemies in part '{part.partName}'!");
            return;
        }
        
        var spawnBoundaries = platformSpawner.GetSpawnBoundaries();
        int chainLength = Random.Range(minChainLength, maxChainLength + 1);
        
        // Calculate positions across screen width
        float totalWidth = spawnBoundaries.y - spawnBoundaries.x;
        float spacing = totalWidth / (chainLength + 1);
        
        // Select one enemy type for the entire chain
        GameObject enemyPrefab = WeightedSelector.SelectWeightedGameObject(chainEnemies);
        
        // Spawn chain
        for (int i = 0; i < chainLength; i++)
        {
            float xPosition = spawnBoundaries.x + spacing * (i + 1);
            Vector3 spawnPosition = new Vector3(xPosition, yPosition, 0);
            
            if (enemyPrefab != null)
            {
                Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }
    
    private void SpawnScreenBlock(LevelPartData part, float yPosition)
    {
        var blockEnemies = GetEnemiesForScreenBlocks(part.enemyTypes);
        if (blockEnemies.Count == 0)
        {
            Debug.LogError($"EnemySpawner: No screen-block compatible enemies in part '{part.partName}'!");
            return;
        }
        
        var spawnBoundaries = platformSpawner.GetSpawnBoundaries();
        GameObject enemyPrefab = WeightedSelector.SelectWeightedGameObject(blockEnemies);
        
        if (enemyPrefab == null) return;
        
        // Calculate how many enemies fit across screen
        float enemyWidth = GetEnemyWidth(enemyPrefab);
        float totalWidth = spawnBoundaries.y - spawnBoundaries.x;
        int enemyCount = Mathf.FloorToInt(totalWidth / (enemyWidth + screenBlockGapSize));
        
        if (enemyCount <= 1) return;
        
        // Create random gap for player passage
        int gapIndex = Random.Range(0, enemyCount);
        float spacing = totalWidth / enemyCount;
        
        // Spawn enemies with gap
        for (int i = 0; i < enemyCount; i++)
        {
            if (i == gapIndex) continue; // Skip gap position
            
            float xPosition = spawnBoundaries.x + spacing * i + spacing * 0.5f;
            Vector3 spawnPosition = new Vector3(xPosition, yPosition, 0);
            
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }
    
    private void SpawnMixedEnemies(LevelPartData part, List<Vector3> platformPositions, float partStartY)
    {
        // Spawn some random enemies
        if (part.enemyCount > 0)
        {
            int randomCount = Mathf.Max(1, part.enemyCount / 2);
            var tempPart = part;
            tempPart.enemyCount = randomCount;
            SpawnRandomEnemies(tempPart, platformPositions);
        }
        
        // 30% chance to add a small chain
        if (Random.Range(0f, 1f) < 0.3f)
        {
            SpawnEnemyChain(part, partStartY + platformHeightOffset + 2f);
        }
    }
    
    private Vector3 CalculateRandomEnemyPosition(List<Vector3> platformPositions, Vector2 spawnBoundaries)
    {
        // 70% chance to spawn above a platform, 30% chance random position
        if (platformPositions.Count > 0 && Random.Range(0f, 1f) < 0.7f)
        {
            Vector3 platformPos = platformPositions[Random.Range(0, platformPositions.Count)];
            float heightOffset = platformHeightOffset + Random.Range(-heightVariation, heightVariation);
            return new Vector3(platformPos.x, platformPos.y + heightOffset, 0);
        }
        else
        {
            float randomX = Random.Range(spawnBoundaries.x, spawnBoundaries.y);
            float randomY = platformPositions.Count > 0 ? 
                Random.Range(platformPositions[0].y, platformPositions[platformPositions.Count - 1].y) + platformHeightOffset :
                platformHeightOffset;
            return new Vector3(randomX, randomY, 0);
        }
    }
    
    private void SpawnSingleEnemy(List<WeightedEnemy> enemyTypes, Vector3 position)
    {
        GameObject enemyPrefab = WeightedSelector.SelectWeightedGameObject(enemyTypes);
        
        if (enemyPrefab != null)
        {
            Instantiate(enemyPrefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("EnemySpawner: Failed to select enemy from weighted list!");
        }
    }
    
    private List<WeightedEnemy> GetEnemiesForChains(List<WeightedEnemy> allEnemies)
    {
        return allEnemies.Where(enemy => 
            enemy.canBeUsedInChains && 
            enemy.enemyPrefab != null).ToList();
    }
    
    private List<WeightedEnemy> GetEnemiesForScreenBlocks(List<WeightedEnemy> allEnemies)
    {
        return allEnemies.Where(enemy => 
            enemy.canBeUsedInScreenBlocks && 
            enemy.enemyPrefab != null).ToList();
    }
    
    private float GetEnemyWidth(GameObject enemyPrefab)
    {
        if (enemyPrefab == null) return 1f;
        
        Collider2D collider = enemyPrefab.GetComponent<Collider2D>();
        if (collider != null)
        {
            return collider.bounds.size.x;
        }
        
        // Fallback to transform scale
        return enemyPrefab.transform.localScale.x;
    }
    
    public bool HasValidEnemiesForPlacement(LevelPartData part, EnemyPlacementType placementType)
    {
        switch (placementType)
        {
            case EnemyPlacementType.Chain:
                return GetEnemiesForChains(part.enemyTypes).Count > 0;
            case EnemyPlacementType.ScreenBlock:
                return GetEnemiesForScreenBlocks(part.enemyTypes).Count > 0;
            default:
                return part.enemyTypes.Count > 0;
        }
    }*/
}