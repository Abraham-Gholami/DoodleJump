using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelPartGenerator : MonoBehaviour
{
    /*[Header("Level Configuration")]
    [Tooltip("Available parts to generate from")]
    public LevelPartData[] availableParts;
    [Tooltip("Space between parts")]
    public float partTransitionSpacing = 2f;
    
    [Header("Generation Settings")]
    [Tooltip("Number of parts to generate at start")]
    public int initialPartsCount = 3;
    [Tooltip("How far ahead of camera to generate content")]
    public float spawnAheadDistance = 25f;
    [Tooltip("How often to check for new generation needs")]
    public float generationCheckInterval = 0.5f;
    
    [Header("Spawner References")]
    public PlatformSpawner platformSpawner;
    public EnemySpawner enemySpawner;
    public PowerUpSpawner powerUpSpawner;
    
    [Header("Debug")]
    [Tooltip("Enable detailed console logging")]
    public bool enableDebugLogs = true;
    [Tooltip("Show part generation in scene view")]
    public bool showGenerationDebug = false;
    
    // Internal state
    private Camera mainCamera;
    private float highestGeneratedY;
    private Queue<LevelPartData> recentParts = new Queue<LevelPartData>();
    private const int MAX_RECENT_PARTS_HISTORY = 3;
    
    private void Start()
    {
        InitializeGenerator();
    }
    
    private void InitializeGenerator()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("LevelPartGenerator: No main camera found!");
            enabled = false;
            return;
        }
        
        if (!ValidateConfiguration())
        {
            enabled = false;
            return;
        }
        
        InitializeSpawners();
        GenerateInitialLevel();
        StartGenerationLoop();
    }
    
    private bool ValidateConfiguration()
    {
        if (availableParts == null || availableParts.Length == 0)
        {
            Debug.LogError("LevelPartGenerator: No parts assigned!");
            return false;
        }
        
        int validParts = 0;
        foreach (var part in availableParts)
        {
            if (part != null && part.IsValidPart())
            {
                validParts++;
            }
        }
        
        if (validParts == 0)
        {
            Debug.LogError("LevelPartGenerator: No valid parts found!");
            return false;
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"LevelPartGenerator: Loaded {validParts} valid parts");
        }
        
        return true;
    }
    
    private void InitializeSpawners()
    {
        if (platformSpawner == null) platformSpawner = GetComponent<PlatformSpawner>();
        if (enemySpawner == null) enemySpawner = GetComponent<EnemySpawner>();
        if (powerUpSpawner == null) powerUpSpawner = GetComponent<PowerUpSpawner>();
        
        if (platformSpawner == null || enemySpawner == null || powerUpSpawner == null)
        {
            Debug.LogError("LevelPartGenerator: Missing required spawner components!");
        }
    }
    
    private void GenerateInitialLevel()
    {
        highestGeneratedY = 0f;
        
        for (int i = 0; i < initialPartsCount; i++)
        {
            LevelPartData selectedPart = SelectNextPart();
            GeneratePart(selectedPart);
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"LevelPartGenerator: Generated {initialPartsCount} initial parts up to height {highestGeneratedY:F1}");
        }
    }
    
    private void StartGenerationLoop()
    {
        InvokeRepeating(nameof(CheckGenerationNeeds), generationCheckInterval, generationCheckInterval);
    }
    
    private void CheckGenerationNeeds()
    {
        if (mainCamera == null) return;
        
        float cameraY = mainCamera.transform.position.y;
        float cameraHeight = CalculateCameraHeight();
        float requiredHeight = cameraY + (cameraHeight / 2f) + spawnAheadDistance;
        
        while (highestGeneratedY < requiredHeight)
        {
            LevelPartData selectedPart = SelectNextPart();
            GeneratePart(selectedPart);
        }
    }
    
    private LevelPartData SelectNextPart()
    {
        var validParts = GetValidPartsForCurrentHeight();
        
        if (validParts.Count == 0)
        {
            Debug.LogWarning("LevelPartGenerator: No valid parts available, using fallback");
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
        
        // If no parts available due to recent use, allow any valid part by height
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
        foreach (var part in availableParts)
        {
            if (part != null && part.IsValidPart())
            {
                return part;
            }
        }
        return null;
    }
    
    private void GeneratePart(LevelPartData part)
    {
        if (part == null) return;
        
        float partStartY = highestGeneratedY + partTransitionSpacing;
        float partCurrentY = partStartY;
        
        if (enableDebugLogs)
        {
            Debug.Log($"LevelPartGenerator: Generating '{part.partName}' at height {partStartY:F1}");
        }
        
        // Generate platforms
        var platformPositions = GeneratePlatforms(part, ref partCurrentY);
        
        // Generate pre-part power-ups (like blood transfusion)
        GeneratePrePartPowerUps(part, partStartY);
        
        // Generate enemies
        GenerateEnemies(part, platformPositions, partStartY, partCurrentY);
        
        // Generate in-part power-ups
        GenerateInPartPowerUps(part, platformPositions, partStartY, partCurrentY);
        
        // Update state
        UpdateGenerationState(part, partCurrentY);
    }
    
    private List<Vector3> GeneratePlatforms(LevelPartData part, ref float currentY)
    {
        var platformPositions = new List<Vector3>();
        
        if (part.sizeMode == PartSizeMode.FixedLength)
        {
            platformPositions = GeneratePlatformsForFixedLength(part, ref currentY);
        }
        else
        {
            platformPositions = GeneratePlatformsForCount(part, ref currentY);
        }
        
        if (enableDebugLogs)
        {
            string modeInfo = part.sizeMode == PartSizeMode.FixedLength ? 
                $"fixed length {part.fixedPartLength}u" : 
                "platform count";
            Debug.Log($"  → {platformPositions.Count} platforms spawned ({modeInfo})");
        }
        
        return platformPositions;
    }
    
    private List<Vector3> GeneratePlatformsForFixedLength(LevelPartData part, ref float currentY)
    {
        var platforms = new List<Vector3>();
        float targetEndY = currentY + part.fixedPartLength;
        int maxPlatforms = part.CalculatePlatformCount();
        int generated = 0;
        
        while (currentY < targetEndY && generated < maxPlatforms)
        {
            float spacing = part.GetRandomSpacing();
            currentY += spacing;
            
            if (currentY > targetEndY)
                currentY = targetEndY;
            
            Vector3 position = platformSpawner.SpawnPlatformAt(currentY, part);
            platforms.Add(position);
            generated++;
        }
        
        currentY = Mathf.Max(currentY, targetEndY);
        return platforms;
    }
    
    private List<Vector3> GeneratePlatformsForCount(LevelPartData part, ref float currentY)
    {
        var platforms = new List<Vector3>();
        int platformCount = part.CalculatePlatformCount();
        
        for (int i = 0; i < platformCount; i++)
        {
            float spacing = part.GetRandomSpacing();
            currentY += spacing;
            
            Vector3 position = platformSpawner.SpawnPlatformAt(currentY, part);
            platforms.Add(position);
        }
        
        return platforms;
    }
    
    private void GeneratePrePartPowerUps(LevelPartData part, float partStartY)
    {
        if (part.spawnBloodTransfusion)
        {
            powerUpSpawner.SpawnBloodTransfusion(partStartY - 1f, part);
            if (enableDebugLogs)
                Debug.Log("  → Blood transfusion spawned before part");
        }
    }
    
    private void GenerateEnemies(LevelPartData part, List<Vector3> platformPositions, float partStartY, float partEndY)
    {
        if (part.RequiresEnemies())
        {
            enemySpawner.SpawnEnemiesForPart(part, platformPositions, partStartY, partEndY);
            if (enableDebugLogs)
                Debug.Log($"  → Enemies spawned ({part.enemyPlacement})");
        }
    }
    
    private void GenerateInPartPowerUps(LevelPartData part, List<Vector3> platformPositions, float partStartY, float partEndY)
    {
        if (part.spawnOxygenPickup)
        {
            powerUpSpawner.SpawnOxygenPickup(partStartY, partEndY, part);
            if (enableDebugLogs)
                Debug.Log("  → Oxygen pickup spawned");
        }
        
        if (part.spawnBoosts)
        {
            powerUpSpawner.SpawnBoostsForPart(platformPositions, part.boostSpawnChance, part);
            if (enableDebugLogs)
                Debug.Log($"  → Boosts spawned (chance: {part.boostSpawnChance:P0})");
        }
    }
    
    private void UpdateGenerationState(LevelPartData part, float newHeight)
    {
        highestGeneratedY = newHeight;
        
        // Update recent parts history
        recentParts.Enqueue(part);
        if (recentParts.Count > MAX_RECENT_PARTS_HISTORY)
        {
            recentParts.Dequeue();
        }
    }
    
    private float CalculateCameraHeight()
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
    
    // Public interface
    public float GetHighestGeneratedY() => highestGeneratedY;
    
    [ContextMenu("Generate Test Part")]
    public void GenerateTestPart()
    {
        if (availableParts != null && availableParts.Length > 0)
        {
            var part = availableParts[Random.Range(0, availableParts.Length)];
            if (part != null && part.IsValidPart())
            {
                GeneratePart(part);
                Debug.Log($"Test generated: {part.partName} at height {highestGeneratedY}");
            }
        }
    }*/
}