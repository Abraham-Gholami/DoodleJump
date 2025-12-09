using UnityEngine;

public class PartManager
{
    private readonly LevelPartData[] availableParts;
    private readonly bool enableDebugLogs;
    
    private PartGenerationState currentPartState;
    private int currentPartIndex = 0;
    
    public PartGenerationState CurrentPartState => currentPartState;
    
    public PartManager(LevelPartData[] parts, bool debugLogs)
    {
        availableParts = parts;
        enableDebugLogs = debugLogs;
    }
    
    public void StartFirstPart()
    {
        var firstPart = SelectNextPartConsecutively();
        if (firstPart == null)
        {
            Debug.LogError("PartManager: Could not select first part!");
            return;
        }
        
        currentPartState = new PartGenerationState
        {
            currentPart = firstPart,
            partStartY = 0f,
            platformsGenerated = 0,
            platformsNeeded = firstPart.CalculatePlatformCount(),
            partCurrentY = 0f,
            isPartComplete = false,
            contentSpawnQueue = ContentScheduler.CreateContentSpawnQueue(firstPart)
        };
        
        if (enableDebugLogs)
        {
            Debug.Log($"PartManager: Starting with part 0: '{currentPartState.currentPart.partName}' - needs {currentPartState.platformsNeeded} platforms, {currentPartState.contentSpawnQueue.Count} content items");
        }
    }
    
    public void StartNextPart(float startY)
    {
        var newPart = SelectNextPartConsecutively();
        Debug.Log($"**** PART {newPart.partName} STARTED. *******");
        
        if (newPart == null)
        {
            Debug.LogError("PartManager: Could not select next part!");
            return;
        }
        
        currentPartState = new PartGenerationState
        {
            currentPart = newPart,
            partStartY = startY,
            platformsGenerated = 0,
            platformsNeeded = newPart.CalculatePlatformCount(),
            partCurrentY = startY,
            isPartComplete = false,
            contentSpawnQueue = ContentScheduler.CreateContentSpawnQueue(newPart)
        };
        
        if (enableDebugLogs)
        {
            Debug.Log($"PartManager: Starting part {currentPartIndex}: '{newPart.partName}' at {startY:F1} - needs {currentPartState.platformsNeeded} platforms, {currentPartState.contentSpawnQueue.Count} content items");
        }
    }
    
    public void OnPlatformGenerated(Vector3 platformPosition)
    {
        currentPartState.platformsGenerated++;
        currentPartState.partCurrentY = platformPosition.y;
        
        // Check if part is complete
        if (currentPartState.platformsGenerated >= currentPartState.platformsNeeded)
        {
            currentPartState.isPartComplete = true;
            
            if (enableDebugLogs)
            {
                Debug.Log($"PartManager: Part '{currentPartState.currentPart.partName}' completed at height {currentPartState.partCurrentY:F1}");
            }
        }
    }
    
    public bool NeedsNewPart()
    {
        return currentPartState != null && currentPartState.isPartComplete;
    }
    
    public bool NeedsMorePlatforms()
    {
        return currentPartState != null && 
               currentPartState.platformsGenerated < currentPartState.platformsNeeded;
    }
    
    public float GetPartProgress()
    {
        if (currentPartState == null || currentPartState.platformsNeeded == 0)
            return 0f;
            
        return (float)currentPartState.platformsGenerated / currentPartState.platformsNeeded;
    }
    
    public string GetCurrentPartName()
    {
        return currentPartState?.currentPart?.partName ?? "None";
    }
    
    private LevelPartData SelectNextPartConsecutively()
    {
        if (availableParts == null || availableParts.Length == 0)
        {
            Debug.LogError("PartManager: No parts available!");
            return null;
        }
        
        LevelPartData selectedPart = availableParts[currentPartIndex % availableParts.Length];
        currentPartIndex++;
        
        if (selectedPart == null || !selectedPart.IsValidPart())
        {
            Debug.LogError($"PartManager: Part at index {currentPartIndex - 1} is invalid!");
            return null;
        }
        
        return selectedPart;
    }
}