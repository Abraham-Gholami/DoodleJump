/*using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlannedSpawn
{
    public float height;
    public GameObject prefab;
    public Vector3 position;
    public ContentType contentType;
    public bool hasSpawned;
    public string sourcePart;
    
    public PlannedSpawn(float height, GameObject prefab, Vector3 position, ContentType type, string partName)
    {
        this.height = height;
        this.prefab = prefab;
        this.position = position;
        this.contentType = type;
        this.hasSpawned = false;
        this.sourcePart = partName;
    }
}

public static class PlannedSpawnCalculator
{
    private static PlatformSpawner platformSpawner;
    
    public static void Initialize(PlatformSpawner spawner)
    {
        platformSpawner = spawner;
    }
    
    public static List<PlannedSpawn> CalculatePartContent(LevelPartData part, float partStartY, float partEndY, List<Vector3> platformPositions)
    {
        var plannedSpawns = new List<PlannedSpawn>();
        
        if (part.contentRules == null || part.contentRules.Length == 0)
        {
            Debug.Log($"PlannedSpawnCalculator: Part '{part.partName}' has no content rules");
            return plannedSpawns;
        }
        
        Debug.Log($"PlannedSpawnCalculator: Calculating content for part '{part.partName}' from {partStartY:F1} to {partEndY:F1}");
        
        // First, collect all spawns that need to be created
        var allSpawnsToCreate = new List<SpawnRequest>();
        
        foreach (var rule in part.contentRules)
        {
            if (rule.prefab == null)
            {
                Debug.LogWarning($"PlannedSpawnCalculator: Rule has no prefab assigned for {rule.contentType}");
                continue;
            }
            
            if (rule.spawnBeforePart)
            {
                // Handle before-part spawns separately
                var beforePartSpawns = CalculateBeforePartContent(rule, partStartY, part.partName);
                plannedSpawns.AddRange(beforePartSpawns);
            }
            else
            {
                // Add to main part spawns
                if (rule.createChain)
                {
                    // Chain counts as 1 spawn request
                    allSpawnsToCreate.Add(new SpawnRequest
                    {
                        rule = rule,
                        isChain = true,
                        partName = part.partName
                    });
                }
                else
                {
                    // Individual spawns
                    int spawnCount = rule.GetRandomCount();
                    for (int i = 0; i < spawnCount; i++)
                    {
                        allSpawnsToCreate.Add(new SpawnRequest
                        {
                            rule = rule,
                            isChain = false,
                            partName = part.partName
                        });
                    }
                }
            }
        }
        
        // Now distribute all spawns evenly across the part height
        if (allSpawnsToCreate.Count > 0)
        {
            var distributedSpawns = DistributeSpawnsAcrossHeight(allSpawnsToCreate, partStartY, partEndY, platformPositions);
            plannedSpawns.AddRange(distributedSpawns);
        }
        
        Debug.Log($"PlannedSpawnCalculator: Calculated {plannedSpawns.Count} planned spawns for part '{part.partName}'");
        return plannedSpawns;
    }
    
    private static List<PlannedSpawn> DistributeSpawnsAcrossHeight(List<SpawnRequest> spawnRequests, float partStartY, float partEndY, List<Vector3> platformPositions)
    {
        var plannedSpawns = new List<PlannedSpawn>();
        
        if (spawnRequests.Count == 0) return plannedSpawns;
        
        float partHeight = partEndY - partStartY;
        float sectionHeight = partHeight / spawnRequests.Count;
        
        Debug.Log($"PlannedSpawnCalculator: Distributing {spawnRequests.Count} spawns across {partHeight:F1} units (sections of {sectionHeight:F1} each)");
        
        for (int i = 0; i < spawnRequests.Count; i++)
        {
            var request = spawnRequests[i];
            float sectionStartY = partStartY + (i * sectionHeight);
            float sectionEndY = partStartY + ((i + 1) * sectionHeight);
            
            if (request.isChain)
            {
                var chainSpawns = CreateChainInSection(request.rule, sectionStartY, sectionEndY, request.partName);
                plannedSpawns.AddRange(chainSpawns);
            }
            else
            {
                var singleSpawn = CreateSingleSpawnInSection(request.rule, sectionStartY, sectionEndY, platformPositions, request.partName);
                if (singleSpawn != null)
                {
                    plannedSpawns.Add(singleSpawn);
                }
            }
        }
        
        return plannedSpawns;
    }
    
    private static List<PlannedSpawn> CreateChainInSection(ContentSpawnRule rule, float sectionStartY, float sectionEndY, string partName)
    {
        var spawns = new List<PlannedSpawn>();
        
        if (platformSpawner == null) return spawns;
        
        var boundaries = platformSpawner.GetSpawnBoundaries();
        float totalWidth = boundaries.y - boundaries.x;
        float spacing = totalWidth / (rule.chainLength + 1);
        
        // Place chain in middle of section
        float chainY = (sectionStartY + sectionEndY) / 2f;
        
        // Create random gap
        int gapIndex = Random.Range(0, rule.chainLength);
        
        for (int i = 0; i < rule.chainLength; i++)
        {
            if (i == gapIndex) continue; // Skip gap
            
            float x = boundaries.x + spacing * (i + 1);
            Vector3 position = new Vector3(x, chainY, 0);
            
            var spawn = new PlannedSpawn(chainY, rule.prefab, position, rule.contentType, partName);
            spawns.Add(spawn);
        }
        
        Debug.Log($"PlannedSpawnCalculator: Created chain with {spawns.Count} items in section {sectionStartY:F1}-{sectionEndY:F1}");
        return spawns;
    }
    
    private static PlannedSpawn CreateSingleSpawnInSection(ContentSpawnRule rule, float sectionStartY, float sectionEndY, List<Vector3> platformPositions, string partName)
    {
        Vector3 spawnPosition = CalculatePositionInSection(rule, sectionStartY, sectionEndY, platformPositions);
        
        if (spawnPosition != Vector3.zero)
        {
            return new PlannedSpawn(spawnPosition.y, rule.prefab, spawnPosition, rule.contentType, partName);
        }
        
        return null;
    }
    
    private static Vector3 CalculatePositionInSection(ContentSpawnRule rule, float sectionStartY, float sectionEndY, List<Vector3> platformPositions)
    {
        if (platformSpawner == null) return Vector3.zero;
        
        Vector3 basePosition = Vector3.zero;
        var boundaries = platformSpawner.GetSpawnBoundaries();
        
        switch (rule.positioning)
        {
            case SimplePositioning.OverPlatforms:
                basePosition = GetPlatformInSection(platformPositions, sectionStartY, sectionEndY);
                if (basePosition == Vector3.zero)
                {
                    // Fallback to random position in section
                    basePosition = GetRandomPositionInSection(sectionStartY, sectionEndY, boundaries);
                }
                break;
                
            case SimplePositioning.RandomAcrossScreen:
                basePosition = GetRandomPositionInSection(sectionStartY, sectionEndY, boundaries);
                break;
        }
        
        // Add height offset so enemies don't spawn inside platforms
        return basePosition + new Vector3(0, 1f, 0);
    }
    
    private static Vector3 GetPlatformInSection(List<Vector3> platformPositions, float sectionStartY, float sectionEndY)
    {
        // Find platforms within this section
        var validPlatforms = new List<Vector3>();
        foreach (var platform in platformPositions)
        {
            if (platform.y >= sectionStartY && platform.y <= sectionEndY)
            {
                validPlatforms.Add(platform);
            }
        }
        
        if (validPlatforms.Count > 0)
        {
            return validPlatforms[Random.Range(0, validPlatforms.Count)];
        }
        
        return Vector3.zero; // No platforms in section
    }
    
    private static Vector3 GetRandomPositionInSection(float sectionStartY, float sectionEndY, Vector2 boundaries)
    {
        float randomX = Random.Range(boundaries.x, boundaries.y);
        float randomY = Random.Range(sectionStartY, sectionEndY);
        
        return new Vector3(randomX, randomY, 0);
    }
    
    private static List<PlannedSpawn> CalculateBeforePartContent(ContentSpawnRule rule, float partStartY, string partName)
    {
        var spawns = new List<PlannedSpawn>();
        int spawnCount = rule.GetRandomCount();
        
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 position = CalculateBeforePartPosition(rule, partStartY);
            var spawn = new PlannedSpawn(position.y, rule.prefab, position, rule.contentType, partName);
            spawns.Add(spawn);
        }
        
        Debug.Log($"PlannedSpawnCalculator: Planned {spawnCount} before-part {rule.contentType} spawns");
        return spawns;
    }
    
    
    private static Vector3 CalculateBeforePartPosition(ContentSpawnRule rule, float partStartY)
    {
        if (platformSpawner == null) return Vector3.zero;
        
        var boundaries = platformSpawner.GetSpawnBoundaries();
        float randomX = Random.Range(boundaries.x, boundaries.y);
        float y = partStartY - 1f;
        
        return new Vector3(randomX, y, 0);
    }
}

[System.Serializable]
public class SpawnRequest
{
    public ContentSpawnRule rule;
    public bool isChain;
    public string partName;
}

public static class PlannedSpawnManager
{
    public static void SpawnContentAtHeight(List<PlannedSpawn> plannedSpawns, float currentHeight, float spawnDistance = 2f)
    {
        foreach (var spawn in plannedSpawns)
        {
            if (!spawn.hasSpawned && currentHeight >= (spawn.height - spawnDistance))
            {
                if (spawn.prefab == null)
                {
                    Debug.LogError($"PlannedSpawnManager: Spawn prefab is NULL for {spawn.contentType} at {spawn.position}!");
                    spawn.hasSpawned = true; // Mark as spawned to avoid repeated errors
                    continue;
                }
                
                Debug.Log($"PlannedSpawnManager: About to instantiate {spawn.prefab.name} at {spawn.position}");
                
                GameObject spawnedObject = Object.Instantiate(spawn.prefab, spawn.position, Quaternion.identity);
                
                if (spawnedObject == null)
                {
                    Debug.LogError($"PlannedSpawnManager: Instantiate returned NULL for {spawn.prefab.name}!");
                }
                else
                {
                    Debug.Log($"PlannedSpawnManager: ✅ Successfully instantiated {spawnedObject.name} with ID {spawnedObject.GetInstanceID()}");
                    Debug.Log($"PlannedSpawnManager: Object position: {spawnedObject.transform.position}, active: {spawnedObject.activeInHierarchy}");
                }
                
                spawn.hasSpawned = true;
            }
        }
    }
    
    public static int GetUnspawnedCount(List<PlannedSpawn> plannedSpawns)
    {
        int count = 0;
        foreach (var spawn in plannedSpawns)
        {
            if (!spawn.hasSpawned) count++;
        }
        return count;
    }
}*/