using UnityEngine;

public class ChunkContentGenerator
{
    private readonly PlatformSpawner platformSpawner;
    private readonly PartManager partManager;
    private readonly bool enableDebugLogs;
    
    // Safety limits
    private const int MAX_PLATFORMS_PER_CHUNK = 10;
    private const int MAX_GENERATION_LOOPS = 5;
    
    public ChunkContentGenerator(PlatformSpawner spawner, PartManager manager, bool debugLogs)
    {
        platformSpawner = spawner;
        partManager = manager;
        enableDebugLogs = debugLogs;
    }
    
    public void PopulateChunk(HeightChunk chunk)
    {
        float currentY = chunk.startY;
        int generationLoops = 0;
        int platformsInChunk = 0;
        
        while (ShouldContinueGeneration(currentY, chunk.endY, generationLoops, platformsInChunk))
        {
            generationLoops++;
            
            // Check if we need a new part
            if (partManager.NeedsNewPart())
            {
                partManager.StartNextPart(currentY);
            }
            
            // Generate platform if needed
            if (partManager.NeedsMorePlatforms())
            {
                var platformResult = TryGeneratePlatform(chunk, currentY);
                if (!platformResult.success)
                {
                    // Platform doesn't fit, move to next chunk
                    break;
                }
                
                currentY = platformResult.nextY;
                platformsInChunk++;
                
                // Update part manager and spawn content progressively
                partManager.OnPlatformGenerated(platformResult.position);
                ContentScheduler.SpawnContentBasedOnProgress(partManager.CurrentPartState, platformResult.position.y);
                
                // If part completed, spawn remaining content
                if (partManager.CurrentPartState.isPartComplete)
                {
                    ContentScheduler.SpawnRemainingContent(partManager.CurrentPartState, platformResult.position.y);
                }
            }
            
            // Safety check for infinite loops
            if (currentY >= chunk.endY)
            {
                break;
            }
        }
        
        if (generationLoops >= MAX_GENERATION_LOOPS)
        {
            Debug.LogWarning($"ChunkContentGenerator: Hit max generation loops for chunk {chunk.chunkId}");
        }
    }
    
    private bool ShouldContinueGeneration(float currentY, float chunkEndY, int loops, int platformCount)
    {
        return currentY < chunkEndY && 
               loops < MAX_GENERATION_LOOPS && 
               platformCount < MAX_PLATFORMS_PER_CHUNK;
    }
    
    private PlatformGenerationResult TryGeneratePlatform(HeightChunk chunk, float currentY)
    {
        var partState = partManager.CurrentPartState;
        if (partState == null)
        {
            return new PlatformGenerationResult { success = false };
        }
        
        float spacing = partState.currentPart.GetRandomSpacing();
        float nextPlatformY = Mathf.Max(currentY, partState.partCurrentY) + spacing;
        
        // Check if platform fits in this chunk
        if (nextPlatformY > chunk.endY)
        {
            return new PlatformGenerationResult { success = false };
        }
        
        Vector3 platformPos = platformSpawner.SpawnPlatformAt(nextPlatformY, partState.currentPart);
        if (platformPos == Vector3.zero)
        {
            return new PlatformGenerationResult { success = false };
        }
        
        // Record platform in chunk
        chunk.platformPositions.Add(platformPos);
        chunk.generatedContent.Add($"Platform from {partState.currentPart.partName}");
        
        return new PlatformGenerationResult 
        { 
            success = true, 
            position = platformPos, 
            nextY = nextPlatformY 
        };
    }
    
    private struct PlatformGenerationResult
    {
        public bool success;
        public Vector3 position;
        public float nextY;
    }
}