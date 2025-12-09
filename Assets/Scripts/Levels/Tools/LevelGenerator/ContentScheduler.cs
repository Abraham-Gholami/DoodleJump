using System.Collections.Generic;
using System.Linq;

public static class ContentScheduler
{
    public static List<ContentSpawnItem> CreateContentSpawnQueue(LevelPartData part)
    {
        var contentQueue = new List<ContentSpawnItem>();
        
        if (part.contentRules == null || part.contentRules.Length == 0)
            return contentQueue;
        
        // Create spawn items for each content rule
        foreach (var rule in part.contentRules)
        {
            if (rule.spawnBeforePart) continue; // Handle these separately
            
            if (rule.createChain)
            {
                int chainCount = rule.GetRandomCount();
                for (int i = 0; i < chainCount; i++)
                {
                    contentQueue.Add(new ContentSpawnItem
                    {
                        rule = rule,
                        isChain = true,
                        hasSpawned = false
                    });
                }
            }
            else
            {
                int itemCount = rule.GetRandomCount();
                for (int i = 0; i < itemCount; i++)
                {
                    contentQueue.Add(new ContentSpawnItem
                    {
                        rule = rule,
                        isChain = false,
                        hasSpawned = false
                    });
                }
            }
        }
        
        return contentQueue;
    }
    
    public static void SpawnContentBasedOnProgress(PartGenerationState partState, float currentHeight)
    {
        if (partState.contentSpawnQueue.Count == 0) return;
        
        // Calculate progress through the part (0 to 1)
        float partProgress = (float)partState.platformsGenerated / partState.platformsNeeded;
        
        // Determine how many content items should be spawned by now
        int totalContentItems = partState.contentSpawnQueue.Count;
        int expectedSpawnedCount = UnityEngine.Mathf.FloorToInt(partProgress * totalContentItems);
        
        // Count already spawned items
        int alreadySpawned = partState.contentSpawnQueue.Count(item => item.hasSpawned);
        
        // Spawn items that should be spawned by now
        int itemsToSpawn = expectedSpawnedCount - alreadySpawned;
        
        for (int i = 0; i < itemsToSpawn && i < totalContentItems; i++)
        {
            var nextItem = partState.contentSpawnQueue.FirstOrDefault(item => !item.hasSpawned);
            if (nextItem != null)
            {
                ContentSpawner.SpawnContentItem(nextItem, currentHeight);
                nextItem.hasSpawned = true;
            }
        }
    }
    
    public static void SpawnRemainingContent(PartGenerationState partState, float currentHeight)
    {
        // Spawn any content that hasn't been spawned yet
        foreach (var item in partState.contentSpawnQueue)
        {
            if (!item.hasSpawned)
            {
                ContentSpawner.SpawnContentItem(item, currentHeight);
                item.hasSpawned = true;
            }
        }
    }
}