using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ChunkBasedLevelGenerator : MonoBehaviour
{
    /*[Header("Chunk Configuration")]
    [Tooltip("Number of parts to generate in each chunk")]
    public int partsPerChunk = 5;
    [Tooltip("How far ahead to generate chunks (in world units)")]
    public float chunkGenerationDistance = 40f;
    [Tooltip("Space between individual parts")]
    public float partSpacing = 2f;
    
    [Header("Level Parts")]
    [Tooltip("Available parts to generate from")]
    public LevelPartData[] availableParts;
    
    [Header("Generation Settings")]
    [Tooltip("Number of initial chunks to generate")]
    public int initialChunkCount = 2;
    [Tooltip("How often to check for chunk generation needs")]
    public float generationCheckInterval = 1f;
    
    [Header("Spawner References")]
    public PlatformSpawner platformSpawner;
    public EnemySpawner enemySpawner;
    public PowerUpSpawner powerUpSpawner;
    public PlatformGapFiller gapFiller;
    
    [Header("Debug")]
    public bool enableDebugLogs = true;
    public bool showChunkBoundaries = false;
    
    // Internal state
    private Camera mainCamera;
    private List<LevelChunk> generatedChunks = new List<LevelChunk>();
    private Queue<LevelPartData> recentParts = new Queue<LevelPartData>();
    private float highestGeneratedY = 0f;
    private int nextChunkId = 0;
    
    private const int MAX_RECENT_PARTS = 5;
    
    private void Start()
    {
        InitializeGenerator();
    }
    
    private void InitializeGenerator()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("ChunkBasedLevelGenerator: No main camera found!");
            enabled = false;
            return;
        }
        
        if (!ValidateConfiguration())
        {
            enabled = false;
            return;
        }
        
        InitializeSpawners();
        GenerateInitialChunks();
        StartGenerationLoop();
    }
    
    private bool ValidateConfiguration()
    {
        if (availableParts == null || availableParts.Length == 0)
        {
            Debug.LogError("ChunkBasedLevelGenerator: No parts assigned!");
            return false;
        }
        
        int validParts = availableParts.Count(part => part != null && part.IsValidPart());
        if (validParts == 0)
        {
            Debug.LogError("ChunkBasedLevelGenerator: No valid parts found!");
            return false;
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"ChunkBasedLevelGenerator: Loaded {validParts} valid parts, {partsPerChunk} parts per chunk");
        }
        
        return true;
    }
    
    private void InitializeSpawners()
    {
        if (platformSpawner == null) platformSpawner = GetComponent<PlatformSpawner>();
        if (enemySpawner == null) enemySpawner = GetComponent<EnemySpawner>();
        if (powerUpSpawner == null) powerUpSpawner = GetComponent<PowerUpSpawner>();
        if (gapFiller == null) gapFiller = GetComponent<PlatformGapFiller>();
    }
    
    private void GenerateInitialChunks()
    {
        for (int i = 0; i < initialChunkCount; i++)
        {
            GenerateNewChunk();
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"ChunkBasedLevelGenerator: Generated {initialChunkCount} initial chunks up to height {highestGeneratedY:F1}");
        }
    }
    
    private void StartGenerationLoop()
    {
        InvokeRepeating(nameof(CheckChunkGenerationNeeds), generationCheckInterval, generationCheckInterval);
    }
    
    private void CheckChunkGenerationNeeds()
    {
        if (mainCamera == null) return;
        
        float cameraY = mainCamera.transform.position.y;
        float requiredHeight = cameraY + chunkGenerationDistance;
        
        while (highestGeneratedY < requiredHeight)
        {
            GenerateNewChunk();
        }
        
        // Optional: Clean up old chunks that are far behind
        CleanupOldChunks(cameraY);
    }
    
    private void GenerateNewChunk()
    {
        var newChunk = new LevelChunk
        {
            chunkId = nextChunkId++,
            startY = highestGeneratedY,
            parts = new List<LevelPartData>(),
            platformPositions = new List<Vector3>()
        };
        
        if (enableDebugLogs)
        {
            Debug.Log($"ChunkBasedLevelGenerator: Generating chunk {newChunk.chunkId} starting at height {newChunk.startY:F1}");
        }
        
        // Select parts for this chunk
        SelectPartsForChunk(newChunk);
        
        // Generate all parts in this chunk
        float currentY = newChunk.startY;
        foreach (var part in newChunk.parts)
        {
            currentY += partSpacing;
            var partPlatforms = GeneratePartInChunk(part, ref currentY, newChunk);
            newChunk.platformPositions.AddRange(partPlatforms);
        }
        
        // Apply gap filling to entire chunk
        if (gapFiller != null && newChunk.platformPositions.Count > 0)
        {
            newChunk.platformPositions = gapFiller.FillGapsInPart(newChunk.platformPositions, newChunk.parts[0]);
        }
        
        newChunk.endY = currentY;
        highestGeneratedY = currentY;
        generatedChunks.Add(newChunk);
        
        if (enableDebugLogs)
        {
            Debug.Log($"ChunkBasedLevelGenerator: Chunk {newChunk.chunkId} completed - {newChunk.parts.Count} parts, height {newChunk.startY:F1} to {newChunk.endY:F1}");
        }
    }
    
    private void SelectPartsForChunk(LevelChunk chunk)
    {
        for (int i = 0; i < partsPerChunk; i++)
        {
            var selectedPart = SelectNextPart();
            chunk.parts.Add(selectedPart);
            
            // Update recent parts tracking
            recentParts.Enqueue(selectedPart);
            if (recentParts.Count > MAX_RECENT_PARTS)
            {
                recentParts.Dequeue();
            }
        }
    }
    
    private LevelPartData SelectNextPart()
    {
        var validParts = GetValidPartsForCurrentHeight();
        
        if (validParts.Count == 0)
        {
            return GetFallbackPart();
        }
        
        return SelectPartUsingWeights(validParts);
    }
    
    private List<LevelPartData> GetValidPartsForCurrentHeight()
    {
        var validParts = new List<LevelPartData>();
        
        foreach (var part in availableParts)
        {
            if (part != null && 
                part.IsValidPart() && 
                highestGeneratedY >= part.minHeightRequired && 
                !IsPartUsedRecently(part))
            {
                validParts.Add(part);
            }
        }
        
        // Fallback: Allow recently used parts if no others available
        if (validParts.Count == 0)
        {
            foreach (var part in availableParts)
            {
                if (part != null && 
                    part.IsValidPart() && 
                    highestGeneratedY >= part.minHeightRequired)
                {
                    validParts.Add(part);
                }
            }
        }
        
        return validParts;
    }
    
    private bool IsPartUsedRecently(LevelPartData part)
    {
        return recentParts.Contains(part);
    }
    
    private LevelPartData SelectPartUsingWeights(List<LevelPartData> validParts)
    {
        if (validParts.Count == 1) return validParts[0];
        
        float totalWeight = validParts.Sum(part => part.selectionWeight);
        if (totalWeight <= 0) return validParts[Random.Range(0, validParts.Count)];
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var part in validParts)
        {
            currentWeight += part.selectionWeight;
            if (randomValue <= currentWeight)
            {
                return part;
            }
        }
        
        return validParts[validParts.Count - 1];
    }
    
    private LevelPartData GetFallbackPart()
    {
        return availableParts.FirstOrDefault(part => part != null && part.IsValidPart());
    }
    
    private List<Vector3> GeneratePartInChunk(LevelPartData part, ref float currentY, LevelChunk chunk)
    {
        float partStartY = currentY;
        var platformPositions = new List<Vector3>();
        
        // Generate platforms
        platformPositions = GeneratePlatformsForPart(part, ref currentY);
        
        // Generate pre-part power-ups
        if (part.spawnBloodTransfusion)
        {
            powerUpSpawner.SpawnBloodTransfusion(partStartY - 1f, part);
        }
        
        // Generate enemies
        if (part.RequiresEnemies())
        {
            enemySpawner.SpawnEnemiesForPart(part, platformPositions, partStartY, currentY);
        }
        
        // Generate in-part power-ups
        if (part.spawnOxygenPickup)
        {
            powerUpSpawner.SpawnOxygenPickup(partStartY, currentY, part);
        }
        
        if (part.spawnBoosts)
        {
            powerUpSpawner.SpawnBoostsForPart(platformPositions, part.boostSpawnChance, part);
        }
        
        return platformPositions;
    }
    
    private List<Vector3> GeneratePlatformsForPart(LevelPartData part, ref float currentY)
    {
        var platformPositions = new List<Vector3>();
        
        if (part.sizeMode == PartSizeMode.FixedLength)
        {
            float targetEndY = currentY + part.fixedPartLength;
            int maxPlatforms = part.CalculatePlatformCount();
            int generated = 0;
            
            while (currentY < targetEndY && generated < maxPlatforms)
            {
                float spacing = part.GetRandomSpacing();
                currentY += spacing;
                
                if (currentY > targetEndY) currentY = targetEndY;
                
                Vector3 position = platformSpawner.SpawnPlatformAt(currentY, part);
                platformPositions.Add(position);
                generated++;
            }
            
            currentY = Mathf.Max(currentY, targetEndY);
        }
        else
        {
            int platformCount = part.CalculatePlatformCount();
            
            for (int i = 0; i < platformCount; i++)
            {
                float spacing = part.GetRandomSpacing();
                currentY += spacing;
                
                Vector3 position = platformSpawner.SpawnPlatformAt(currentY, part);
                platformPositions.Add(position);
            }
        }
        
        return platformPositions;
    }
    
    private void CleanupOldChunks(float cameraY)
    {
        float cleanupThreshold = cameraY - (chunkGenerationDistance * 2f);
        
        for (int i = generatedChunks.Count - 1; i >= 0; i--)
        {
            if (generatedChunks[i].endY < cleanupThreshold)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"ChunkBasedLevelGenerator: Cleaning up old chunk {generatedChunks[i].chunkId}");
                }
                generatedChunks.RemoveAt(i);
            }
        }
    }
    
    // Public interface
    public float GetHighestGeneratedY() => highestGeneratedY;
    public int GetGeneratedChunkCount() => generatedChunks.Count;
    public LevelChunk GetChunkAtHeight(float height)
    {
        return generatedChunks.FirstOrDefault(chunk => height >= chunk.startY && height <= chunk.endY);
    }
    
    [ContextMenu("Generate Test Chunk")]
    public void GenerateTestChunk()
    {
        GenerateNewChunk();
        Debug.Log($"ChunkBasedLevelGenerator: Test chunk generated - Total chunks: {generatedChunks.Count}");
    }
    
    [ContextMenu("Show Chunk Info")]
    public void ShowChunkInfo()
    {
        Debug.Log($"ChunkBasedLevelGenerator: {generatedChunks.Count} chunks generated");
        foreach (var chunk in generatedChunks)
        {
            Debug.Log($"Chunk {chunk.chunkId}: {chunk.parts.Count} parts, Y {chunk.startY:F1} - {chunk.endY:F1}");
        }
    }
}

[System.Serializable]
public class LevelChunk
{
    public int chunkId;
    public float startY;
    public float endY;
    public List<LevelPartData> parts;
    public List<Vector3> platformPositions;
    
    public float GetHeight() => endY - startY;
    public bool ContainsHeight(float y) => y >= startY && y <= endY;*/
}