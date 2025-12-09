using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class PlatformGapFiller : MonoBehaviour
{
    [Header("Gap Detection Settings")]
    [Tooltip("Maximum distance player can jump")]
    public float maxJumpDistance = 4f;
    [Tooltip("Safety margin for jump calculations")]
    public float jumpSafetyMargin = 0.5f;
    [Tooltip("Minimum gap size that triggers filling")]
    public float minimumGapToFill = 2f;
    
    [Header("Fill Platform Settings")]
    [Tooltip("Height variation for gap-filling platforms")]
    public float fillPlatformHeightVariation = 0.3f;
    [Tooltip("X position randomization for gap platforms")]
    public float fillPlatformXVariation = 1f;
    
    [Header("Debug")]
    [Tooltip("Show gap detection in console")]
    public bool enableGapDetectionLogs = false;
    [Tooltip("Show filled gap positions")]
    public bool enableFillLogs = true;
    
    [SerializeField] private PlatformSpawner platformSpawner;
    
    private void Start()
    {
        InitializeGapFiller();
    }
    
    private void InitializeGapFiller()
    {
        if (platformSpawner == null)
        {
            StartCoroutine(TryToGetPlatformSpawner());
            Debug.Log("PlatformGapFiller: PlatformSpawner component not found!");
            enabled = false;
        }
    }

    private IEnumerator TryToGetPlatformSpawner()
    {
        while (platformSpawner == null)
        {
            yield return new WaitForSeconds(0.2f);
            Debug.Log("PlatformGapFiller: Trying to get platform spawner");
            platformSpawner = GetComponent<PlatformSpawner>();
        }
    }
    
    public List<Vector3> FillGapsInPart(List<Vector3> originalPlatforms, LevelPartData partData)
    {
        if (!ValidateInput(originalPlatforms, partData))
        {
            return originalPlatforms;
        }
        
        var processedPlatforms = new List<Vector3>(originalPlatforms);
        int gapsFilled = 0;
        
        // Sort platforms by height to ensure proper order
        processedPlatforms.Sort((a, b) => a.y.CompareTo(b.y));
        
        // Check gaps between consecutive platforms
        for (int i = 0; i < processedPlatforms.Count - 1; i++)
        {
            Vector3 currentPlatform = processedPlatforms[i];
            Vector3 nextPlatform = processedPlatforms[i + 1];
            
            if (ShouldFillGap(currentPlatform, nextPlatform))
            {
                var fillPlatforms = CreateFillPlatforms(currentPlatform, nextPlatform, partData);
                
                // Insert fill platforms into list at correct positions
                for (int j = 0; j < fillPlatforms.Count; j++)
                {
                    processedPlatforms.Insert(i + 1 + j, fillPlatforms[j]);
                    gapsFilled++;
                }
                
                // Skip ahead to avoid processing newly added platforms
                i += fillPlatforms.Count;
            }
        }
        
        if (gapsFilled > 0 && enableFillLogs)
        {
            Debug.Log($"PlatformGapFiller: Filled {gapsFilled} gaps in part '{partData.partName}'");
        }
        
        return processedPlatforms;
    }
    
    public bool CheckForGapsInPart(List<Vector3> platforms)
    {
        if (platforms.Count < 2) return false;
        
        var sortedPlatforms = new List<Vector3>(platforms);
        sortedPlatforms.Sort((a, b) => a.y.CompareTo(b.y));
        
        for (int i = 0; i < sortedPlatforms.Count - 1; i++)
        {
            if (ShouldFillGap(sortedPlatforms[i], sortedPlatforms[i + 1]))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private bool ValidateInput(List<Vector3> platforms, LevelPartData partData)
    {
        if (platforms == null || platforms.Count < 2)
        {
            if (enableGapDetectionLogs)
            {
                Debug.Log("PlatformGapFiller: Not enough platforms to check for gaps");
            }
            return false;
        }
        
        if (partData == null)
        {
            Debug.LogError("PlatformGapFiller: No part data provided!");
            return false;
        }
        
        if (partData.platformTypes.Count == 0)
        {
            Debug.LogError($"PlatformGapFiller: Part '{partData.partName}' has no platform types for gap filling!");
            return false;
        }
        
        return true;
    }
    
    private bool ShouldFillGap(Vector3 platformA, Vector3 platformB)
    {
        float distance = Vector3.Distance(platformA, platformB);
        float effectiveMaxJump = maxJumpDistance - jumpSafetyMargin;
        
        bool gapTooLarge = distance > effectiveMaxJump;
        bool gapSignificant = distance > minimumGapToFill;
        
        if (enableGapDetectionLogs && gapTooLarge)
        {
            Debug.Log($"PlatformGapFiller: Gap detected - Distance: {distance:F2}, Max Jump: {effectiveMaxJump:F2}");
        }
        
        return gapTooLarge && gapSignificant;
    }
    
    private List<Vector3> CreateFillPlatforms(Vector3 platformA, Vector3 platformB, LevelPartData partData)
    {
        var fillPlatforms = new List<Vector3>();
        
        float gapDistance = Vector3.Distance(platformA, platformB);
        float effectiveMaxJump = maxJumpDistance - jumpSafetyMargin;
        
        // Calculate how many platforms needed to fill gap
        int platformsNeeded = Mathf.CeilToInt(gapDistance / effectiveMaxJump);
        
        // Create evenly spaced fill platforms
        for (int i = 1; i <= platformsNeeded; i++)
        {
            float t = (float)i / (platformsNeeded + 1);
            Vector3 basePosition = Vector3.Lerp(platformA, platformB, t);
            
            // Add variation to make it feel natural
            Vector3 fillPosition = AddPositionVariation(basePosition);
            
            // Ensure position is within spawn boundaries
            fillPosition = ClampToSpawnBoundaries(fillPosition);
            
            // Spawn the actual platform
            Vector3 spawnedPosition = platformSpawner.SpawnPlatformAt(fillPosition.y, partData);
            
            if (spawnedPosition != Vector3.zero)
            {
                fillPlatforms.Add(spawnedPosition);
                
                if (enableFillLogs)
                {
                    Debug.Log($"PlatformGapFiller: Fill platform spawned at {spawnedPosition}");
                }
            }
        }
        
        return fillPlatforms;
    }
    
    private Vector3 AddPositionVariation(Vector3 basePosition)
    {
        float xVariation = Random.Range(-fillPlatformXVariation, fillPlatformXVariation);
        float yVariation = Random.Range(-fillPlatformHeightVariation, fillPlatformHeightVariation);
        
        return new Vector3(
            basePosition.x + xVariation,
            basePosition.y + yVariation,
            basePosition.z
        );
    }
    
    private Vector3 ClampToSpawnBoundaries(Vector3 position)
    {
        var boundaries = platformSpawner.GetSpawnBoundaries();
        
        return new Vector3(
            Mathf.Clamp(position.x, boundaries.x, boundaries.y),
            position.y,
            position.z
        );
    }
    
    public float CalculateJumpDifficulty(List<Vector3> platforms)
    {
        if (platforms.Count < 2) return 0f;
        
        var sortedPlatforms = new List<Vector3>(platforms);
        sortedPlatforms.Sort((a, b) => a.y.CompareTo(b.y));
        
        float totalDifficulty = 0f;
        int gapCount = 0;
        
        for (int i = 0; i < sortedPlatforms.Count - 1; i++)
        {
            float distance = Vector3.Distance(sortedPlatforms[i], sortedPlatforms[i + 1]);
            float difficulty = Mathf.Clamp01(distance / maxJumpDistance);
            
            totalDifficulty += difficulty;
            gapCount++;
        }
        
        return gapCount > 0 ? totalDifficulty / gapCount : 0f;
    }
    
    [ContextMenu("Test Gap Detection")]
    public void TestGapDetection()
    {
        var testPlatforms = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 6, 0), // Large gap
            new Vector3(0, 8, 0)  // Small gap
        };
        
        Debug.Log($"PlatformGapFiller: Test gap detection - Found gaps: {CheckForGapsInPart(testPlatforms)}");
        
        for (int i = 0; i < testPlatforms.Count - 1; i++)
        {
            bool shouldFill = ShouldFillGap(testPlatforms[i], testPlatforms[i + 1]);
            float distance = Vector3.Distance(testPlatforms[i], testPlatforms[i + 1]);
            Debug.Log($"Gap {i}: Distance {distance:F2}, Should Fill: {shouldFill}");
        }
    }
}