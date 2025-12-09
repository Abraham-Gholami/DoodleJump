using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("Spawn Boundaries")]
    [Tooltip("Screen edge offset for platform spawning")]
    public float screenEdgeOffset = 1.2f;
    [Tooltip("Wall offset to prevent spawning too close to walls")]
    public float wallOffset = 3f;
    [Tooltip("Automatically detect wall boundaries")]
    public bool autoDetectWalls = true;
    [Tooltip("Tag used to identify wall objects")]
    public string wallTag = "Wall";
    
    private float spawnLeft, spawnRight;
    private Camera mainCamera;
    
    private void Start()
    {
        InitializeSpawner();
    }
    
    private void InitializeSpawner()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("PlatformSpawner: No main camera found!");
            return;
        }
        
        CalculateSpawnBoundaries();
    }
    
    public Vector3 SpawnPlatformAt(float yPosition, LevelPartData partData)
    {
        if (partData == null)
        {
            Debug.LogError("PlatformSpawner: No part data provided!");
            return Vector3.zero;
        }
        
        if (partData.platformTypes.Count == 0)
        {
            Debug.LogError($"PlatformSpawner: Part '{partData.partName}' has no platform types!");
            return Vector3.zero;
        }
        
        // Generate random X position within boundaries
        float xPosition = Random.Range(spawnLeft, spawnRight);
        Vector3 spawnPosition = new Vector3(xPosition, yPosition, 0);
        
        // Select platform using weighted system
        GameObject platformPrefab = WeightedSelector.SelectWeightedGameObject(partData.platformTypes);
        
        if (platformPrefab == null)
        {
            Debug.LogError($"PlatformSpawner: Failed to select platform for part '{partData.partName}'");
            return Vector3.zero;
        }
        
        // Spawn platform
        GameObject spawnedPlatform = Instantiate(platformPrefab, spawnPosition, Quaternion.identity);
        
        // Trigger event for other systems
        EventManager.TriggerEvent<EventName, Vector3>(EventName.OnSpawnPlatform, spawnPosition);
        
        return spawnPosition;
    }
    
    private void CalculateSpawnBoundaries()
    {
        // Get screen boundaries in world space
        Vector3 bottomLeft = mainCamera.ScreenToWorldPoint(Vector3.zero);
        Vector3 bottomRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
        
        spawnLeft = bottomLeft.x + screenEdgeOffset;
        spawnRight = bottomRight.x - screenEdgeOffset;
        
        // Adjust for walls if auto-detection is enabled
        if (autoDetectWalls)
        {
            AdjustBoundariesForWalls();
        }
        
        Debug.Log($"PlatformSpawner: Spawn boundaries set to [{spawnLeft:F2}, {spawnRight:F2}]");
    }
    
    private void AdjustBoundariesForWalls()
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag(wallTag);
        
        if (walls.Length < 2)
        {
            Debug.LogWarning($"PlatformSpawner: Expected 2 walls with tag '{wallTag}', found {walls.Length}");
            return;
        }
        
        float leftWallBound = float.MinValue;
        float rightWallBound = float.MaxValue;
        
        foreach (GameObject wall in walls)
        {
            Collider2D wallCollider = wall.GetComponent<Collider2D>();
            if (wallCollider == null) continue;
            
            Bounds wallBounds = wallCollider.bounds;
            
            // Left wall (negative X)
            if (wall.transform.position.x < 0)
            {
                leftWallBound = Mathf.Max(leftWallBound, wallBounds.max.x);
            }
            // Right wall (positive X)
            else
            {
                rightWallBound = Mathf.Min(rightWallBound, wallBounds.min.x);
            }
        }
        
        // Apply wall constraints with offset
        if (leftWallBound != float.MinValue)
        {
            spawnLeft = Mathf.Max(spawnLeft, leftWallBound + wallOffset);
        }
        
        if (rightWallBound != float.MaxValue)
        {
            spawnRight = Mathf.Min(spawnRight, rightWallBound - wallOffset);
        }
        
        // Validate final boundaries
        if (spawnLeft >= spawnRight)
        {
            Debug.LogError("PlatformSpawner: Invalid spawn boundaries after wall adjustment!");
        }
    }
    
    public Vector2 GetSpawnBoundaries()
    {
        return new Vector2(spawnLeft, spawnRight);
    }
    
    public bool IsValidSpawnPosition(Vector3 position)
    {
        return position.x >= spawnLeft && position.x <= spawnRight;
    }
    
    [ContextMenu("Recalculate Boundaries")]
    public void RecalculateBoundaries()
    {
        CalculateSpawnBoundaries();
    }
}