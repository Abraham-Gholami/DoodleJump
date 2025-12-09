using UnityEngine;
using System.Collections.Generic;

public class HeightChunkLevelGenerator : MonoBehaviour
{
    [Header("Height Chunk Configuration")]
    [Tooltip("Height of each generation chunk in world units")]
    public float chunkHeight = 8f;
    [Tooltip("How far ahead to generate chunks (in world units)")]
    public float generationDistance = 30f;
    [Tooltip("How often to check for new chunk generation")]
    public float generationCheckInterval = 1f;
    [Tooltip("Maximum chunks to generate per frame")]
    public int maxChunksPerFrame = 1;
    
    [Header("Level Parts")]
    [Tooltip("Available parts to generate from")]
    public LevelPartData[] availableParts;
    
    [Header("Spawner References")]
    public PlatformSpawner platformSpawner;
    
    [Header("Debug")]
    public bool enableDebugLogs = true;
    
    // Components
    private PartManager partManager;
    private ChunkContentGenerator chunkContentGenerator;
    private ChunkCleanupManager chunkCleanupManager;
    
    // Internal state
    private Camera mainCamera;
    private List<HeightChunk> generatedChunks = new List<HeightChunk>();
    private float highestGeneratedY = 0f;
    private int nextChunkId = 0;
    private bool isGenerating = false;
    
    private void Start()
    {
        InitializeGenerator();
    }
    
    private void InitializeGenerator()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("HeightChunkLevelGenerator: No main camera found!");
            enabled = false;
            return;
        }
        
        if (!ValidateConfiguration())
        {
            enabled = false;
            return;
        }
        
        InitializeComponents();
        StartGenerationLoop();
        GenerateInitialChunks();
    }
    
    private bool ValidateConfiguration()
    {
        if (availableParts == null || availableParts.Length == 0)
        {
            Debug.LogError("HeightChunkLevelGenerator: No parts assigned!");
            return false;
        }
        
        int validParts = 0;
        foreach (var part in availableParts)
        {
            if (part != null && part.IsValidPart())
                validParts++;
        }
        
        if (validParts == 0)
        {
            Debug.LogError("HeightChunkLevelGenerator: No valid parts found!");
            return false;
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"HeightChunkLevelGenerator: Loaded {validParts} valid parts, {chunkHeight}u per chunk");
        }
        
        return true;
    }
    
    private void InitializeComponents()
    {
        // Initialize spawner
        if (platformSpawner == null) 
            platformSpawner = GetComponent<PlatformSpawner>();
        
        // Create component instances
        partManager = new PartManager(availableParts, enableDebugLogs);
        chunkContentGenerator = new ChunkContentGenerator(platformSpawner, partManager, enableDebugLogs);
        chunkCleanupManager = new ChunkCleanupManager(generationDistance, enableDebugLogs);
        
        // Initialize components
        ContentSpawner.Initialize(platformSpawner);
        partManager.StartFirstPart();
        
        if (enableDebugLogs)
            Debug.Log("HeightChunkLevelGenerator: Components initialized");
    }
    
    private void StartGenerationLoop()
    {
        InvokeRepeating(nameof(CheckGenerationNeeds), generationCheckInterval, generationCheckInterval);
    }
    
    private void GenerateInitialChunks()
    {
        for (int i = 0; i < 3; i++)
        {
            GenerateNextChunk();
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"HeightChunkLevelGenerator: Generated 3 initial chunks up to height {highestGeneratedY:F1}");
        }
    }
    
    private void CheckGenerationNeeds()
    {
        if (isGenerating || mainCamera == null) return;
        
        float cameraY = mainCamera.transform.position.y;
        float requiredHeight = cameraY + generationDistance;
        
        if (highestGeneratedY < requiredHeight)
        {
            GenerateChunksToHeight(requiredHeight);
        }
        
        chunkCleanupManager.CleanupOldChunks(generatedChunks, cameraY);
    }
    
    private void GenerateChunksToHeight(float requiredHeight)
    {
        isGenerating = true;
        
        int chunksGenerated = 0;
        while (highestGeneratedY < requiredHeight && chunksGenerated < maxChunksPerFrame)
        {
            GenerateNextChunk();
            chunksGenerated++;
        }
        
        isGenerating = false;
        
        if (enableDebugLogs && chunksGenerated > 0)
        {
            Debug.Log($"HeightChunkLevelGenerator: Generated {chunksGenerated} chunks this frame");
        }
    }
    
    private void GenerateNextChunk()
    {
        var newChunk = CreateChunk();
        chunkContentGenerator.PopulateChunk(newChunk);
        
        highestGeneratedY = newChunk.endY;
        generatedChunks.Add(newChunk);
        
        if (enableDebugLogs)
        {
            Debug.Log($"HeightChunkLevelGenerator: Generated chunk {newChunk.chunkId} from {newChunk.startY:F1} to {newChunk.endY:F1}");
        }
    }
    
    private HeightChunk CreateChunk()
    {
        return new HeightChunk
        {
            chunkId = nextChunkId++,
            startY = highestGeneratedY,
            endY = highestGeneratedY + chunkHeight,
            platformPositions = new List<Vector3>(),
            generatedContent = new List<string>()
        };
    }
    
    // Public interface
    public float GetHighestGeneratedY() => highestGeneratedY;
    public int GetGeneratedChunkCount() => generatedChunks.Count;
    public string GetCurrentPartName() => partManager?.GetCurrentPartName() ?? "None";
    
    [ContextMenu("Force Generate One Chunk")]
    public void ForceGenerateOneChunk()
    {
        GenerateNextChunk();
        Debug.Log($"HeightChunkLevelGenerator: Force generated chunk - Total: {generatedChunks.Count}");
    }
}