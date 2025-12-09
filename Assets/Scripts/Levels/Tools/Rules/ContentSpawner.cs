using UnityEngine;

public static class ContentSpawner
{
    private static PlatformSpawner platformSpawner;
    
    public static void Initialize(PlatformSpawner spawner)
    {
        platformSpawner = spawner;
    }
    
    public static void SpawnContentItem(ContentSpawnItem item, float height)
    {
        if (item.isChain)
        {
            SpawnChainAtHeight(item.rule, height);
        }
        else
        {
            SpawnElementAtHeight(item.rule, height);
        }
    }
    
    private static void SpawnElementAtHeight(ContentSpawnRule rule, float height)
    {
        if (rule.prefab == null) return;
        
        Vector3 spawnPosition = CalculateSpawnPosition(rule, height);
        
        if (spawnPosition != Vector3.zero)
        {
            bool shouldFlip = rule.GetFlip();
            
            GameObject spawnedObject = Object.Instantiate(rule.prefab, spawnPosition, Quaternion.identity);
            
            // Apply flip through Enemy component if available
            Enemy enemy = spawnedObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.SetFlip(shouldFlip);
            }
            else if (shouldFlip)
            {
                Vector3 scale = spawnedObject.transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                spawnedObject.transform.localScale = scale;
            }
        }
    }
    
    private static void SpawnChainAtHeight(ContentSpawnRule rule, float height)
    {
        if (rule.prefab == null || platformSpawner == null) return;
        
        var boundaries = platformSpawner.GetSpawnBoundaries();
        float totalWidth = boundaries.y - boundaries.x;
        
        int actualChainLength = rule.GetRandomChainLength();
        float spacing = rule.GetChainSpacing(totalWidth, actualChainLength);
        bool chainFlip = rule.GetFlip();
        
        float startX = CalculateChainStartPosition(rule, totalWidth, actualChainLength, spacing);
        
        for (int i = 0; i < actualChainLength; i++)
        {
            float x = CalculateChainElementX(rule, boundaries, startX, spacing, i);
            Vector3 position = new Vector3(x, height + 1f, 0);
            
            GameObject chainObject = Object.Instantiate(rule.prefab, position, Quaternion.identity);
            
            // Apply flip
            Enemy enemy = chainObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.SetFlip(chainFlip);
            }
            else if (chainFlip)
            {
                Vector3 scale = chainObject.transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                chainObject.transform.localScale = scale;
            }
        }
    }
    
    private static float CalculateChainStartPosition(ContentSpawnRule rule, float totalWidth, int chainLength, float spacing)
    {
        var boundaries = platformSpawner.GetSpawnBoundaries();
        
        if (rule.chainSpacingMode == ChainSpacingMode.EvenlyDistributed)
        {
            return boundaries.x;
        }
        else
        {
            float totalChainWidth = (chainLength - 1) * spacing;
            
            if (totalChainWidth > totalWidth)
            {
                return boundaries.x;
            }
            else
            {
                float maxStartX = boundaries.y - totalChainWidth;
                return Random.Range(boundaries.x, maxStartX);
            }
        }
    }
    
    private static float CalculateChainElementX(ContentSpawnRule rule, Vector2 boundaries, float startX, float spacing, int index)
    {
        if (rule.chainSpacingMode == ChainSpacingMode.EvenlyDistributed)
        {
            float totalWidth = boundaries.y - boundaries.x;
            int actualChainLength = rule.GetRandomChainLength();
            float evenSpacing = totalWidth / (actualChainLength + 1);
            return boundaries.x + evenSpacing * (index + 1);
        }
        else
        {
            return startX + spacing * index;
        }
    }
    
    private static Vector3 CalculateSpawnPosition(ContentSpawnRule rule, float height)
    {
        if (platformSpawner == null) return Vector3.zero;
        
        var boundaries = platformSpawner.GetSpawnBoundaries();
        float randomX = Random.Range(boundaries.x, boundaries.y);
        return new Vector3(randomX, height + 1f, 0);
    }
}