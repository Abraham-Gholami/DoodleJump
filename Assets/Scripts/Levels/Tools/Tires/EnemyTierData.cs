using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New Enemy Tier", menuName = "Level Generation/Enemy Tier")]
public class EnemyTierData : ScriptableObject
{
    [Header("Tier Info")]
    [Tooltip("Name for this enemy tier")]
    public string tierName = "New Enemy Tier";
    [TextArea(2, 3)]
    public string description = "Describe what enemies this tier contains...";
    
    [Header("Height Range")]
    [Tooltip("Minimum height where this tier can be used")]
    public float minHeight = 0f;
    [Tooltip("Maximum height where this tier can be used (use very high number for no limit)")]
    public float maxHeight = 1000f;
    
    [Header("Available Enemies")]
    [Tooltip("All enemies available in this tier with their weights and properties")]
    public List<WeightedEnemy> availableEnemies = new List<WeightedEnemy>();
    
    [Header("Chain Settings")]
    [Tooltip("Default chain length range for this tier")]
    public int minChainLength = 2;
    public int maxChainLength = 4;
    [Tooltip("Should chain enemies be all the same type?")]
    public bool uniformChainEnemies = true;
    
    [Header("Screen Block Settings")]
    [Tooltip("Gap size between enemies in screen blocks")]
    public float screenBlockGapSize = 0.5f;
    
    [Header("Visual")]
    [Tooltip("Color for editor visualization")]
    public Color tierColor = Color.red;
    
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(tierName) || tierName == "New Enemy Tier")
        {
            tierName = $"Enemy Tier {minHeight}-{maxHeight}";
        }
        
        if (minHeight > maxHeight)
        {
            minHeight = maxHeight;
        }
        
        if (minChainLength > maxChainLength)
        {
            minChainLength = maxChainLength;
        }
        
        // Remove null enemies
        availableEnemies.RemoveAll(e => e.enemyPrefab == null);
    }
    
    /// <summary>
    /// Check if this tier is valid for the given height
    /// </summary>
    public bool IsValidForHeight(float height)
    {
        return height >= minHeight && height < maxHeight && HasValidEnemies();
    }
    
    /// <summary>
    /// Check if this tier has any valid enemies
    /// </summary>
    public bool HasValidEnemies()
    {
        return availableEnemies.Any(e => e.enemyPrefab != null && e.weight > 0);
    }
    
    /// <summary>
    /// Get a random enemy from this tier
    /// </summary>
    public GameObject GetRandomEnemy()
    {
        return WeightedSelector.SelectWeightedGameObject(availableEnemies);
    }
    
    /// <summary>
    /// Get a random enemy suitable for chains
    /// </summary>
    public GameObject GetChainEnemy()
    {
        return WeightedSelector.SelectChainEnemy(availableEnemies);
    }
    
    /// <summary>
    /// Get a random enemy suitable for screen blocks
    /// </summary>
    public GameObject GetScreenBlockEnemy()
    {
        return WeightedSelector.SelectScreenBlockEnemy(availableEnemies);
    }
    
    /// <summary>
    /// Get random chain length for this tier
    /// </summary>
    public int GetRandomChainLength()
    {
        return Random.Range(minChainLength, maxChainLength + 1);
    }
    
    /// <summary>
    /// Get enemies of a specific type
    /// </summary>
    public List<WeightedEnemy> GetMovingEnemies()
    {
        return availableEnemies.Where(e => e.isMovingEnemy).ToList();
    }
    
    public List<WeightedEnemy> GetStaticEnemies()
    {
        return availableEnemies.Where(e => !e.isMovingEnemy).ToList();
    }
}