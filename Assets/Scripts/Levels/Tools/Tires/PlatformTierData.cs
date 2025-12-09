using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Platform Tier", menuName = "Level Generation/Platform Tier")]
public class PlatformTierData : ScriptableObject
{
    [Header("Tier Info")]
    [Tooltip("Name for this platform tier")]
    public string tierName = "New Platform Tier";
    [TextArea(2, 3)]
    public string description = "Describe what platforms this tier contains...";
    
    [Header("Height Range")]
    [Tooltip("Minimum height where this tier can be used")]
    public float minHeight = 0f;
    [Tooltip("Maximum height where this tier can be used (use very high number for no limit)")]
    public float maxHeight = 1000f;
    
    [Header("Available Platforms")]
    [Tooltip("All platforms available in this tier with their weights")]
    public List<WeightedPlatform> availablePlatforms = new List<WeightedPlatform>();
    
    [Header("Visual")]
    [Tooltip("Color for editor visualization")]
    public Color tierColor = Color.white;
    
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(tierName) || tierName == "New Platform Tier")
        {
            tierName = $"Platform Tier {minHeight}-{maxHeight}";
        }
        
        if (minHeight > maxHeight)
        {
            minHeight = maxHeight;
        }
        
        // Remove null platforms
        availablePlatforms.RemoveAll(p => p.platformPrefab == null);
    }
    
    /// <summary>
    /// Check if this tier is valid for the given height
    /// </summary>
    public bool IsValidForHeight(float height)
    {
        return height >= minHeight && height < maxHeight && availablePlatforms.Count > 0;
    }
    
    /// <summary>
    /// Get a random platform from this tier
    /// </summary>
    public GameObject GetRandomPlatform()
    {
        return WeightedSelector.SelectWeightedGameObject(availablePlatforms);
    }
    
    /// <summary>
    /// Get total weight of all platforms (for debugging)
    /// </summary>
    public float GetTotalWeight()
    {
        float total = 0f;
        foreach (var platform in availablePlatforms)
        {
            if (platform.platformPrefab != null)
                total += platform.weight;
        }
        return total;
    }
}