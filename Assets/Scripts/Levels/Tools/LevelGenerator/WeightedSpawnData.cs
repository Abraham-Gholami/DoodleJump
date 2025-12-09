using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class WeightedPlatform
{
    [Tooltip("Platform prefab to spawn")]
    public GameObject platformPrefab;
    
    [Range(0f, 10f)]
    [Tooltip("Higher weight = more likely to be selected")]
    public float weight = 1f;
    
    [Tooltip("Optional: Override color for this platform in editor")]
    public Color debugColor = Color.white;
}

[System.Serializable]
public class WeightedEnemy
{
    [Tooltip("Enemy prefab to spawn")]
    public GameObject enemyPrefab;
    
    [Range(0f, 10f)]
    [Tooltip("Higher weight = more likely to be selected")]
    public float weight = 1f;
    
    [Tooltip("Is this a moving enemy? (affects placement logic)")]
    public bool isMovingEnemy = false;
    
    [Tooltip("Can this enemy be used in chains?")]
    public bool canBeUsedInChains = true;
    
    [Tooltip("Can this enemy be used in screen blocks?")]
    public bool canBeUsedInScreenBlocks = true;
}

[System.Serializable]
public class WeightedPowerUp
{
    [Tooltip("Power-up prefab to spawn")]
    public GameObject powerUpPrefab;
    
    [Range(0f, 10f)]
    [Tooltip("Higher weight = more likely to be selected")]
    public float weight = 1f;
    
    [Tooltip("Type of power-up for categorization")]
    public PowerUpType powerUpType = PowerUpType.Boost;
    
    [Tooltip("Should this spawn on platforms or in air?")]
    public bool spawnOnPlatforms = true;
    
    [Tooltip("Offset from platform/spawn position")]
    public Vector3 spawnOffset = new Vector3(0, 0.5f, 0);
}

public enum PowerUpType
{
    Boost,          // Speed/jump boosts
    Health,         // Blood transfusion, oxygen
    Special,        // Unique power-ups
    Weapon          // Lasers, etc.
}

// Helper class for weighted selection
public static class WeightedSelector
{
    public static T SelectWeighted<T>(List<T> items, System.Func<T, float> getWeight) where T : class
    {
        if (items == null || items.Count == 0) return null;
        
        // Filter out items with zero weight and null objects
        var validItems = items.Where(item => item != null && getWeight(item) > 0).ToList();
        if (validItems.Count == 0) return null;
        
        float totalWeight = validItems.Sum(getWeight);
        if (totalWeight <= 0) return validItems[Random.Range(0, validItems.Count)];
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var item in validItems)
        {
            currentWeight += getWeight(item);
            if (randomValue <= currentWeight)
            {
                return item;
            }
        }
        
        // Fallback
        return validItems[validItems.Count - 1];
    }
    
    public static GameObject SelectWeightedGameObject(List<WeightedPlatform> platforms)
    {
        var selected = SelectWeighted(platforms, p => p.weight);
        return selected?.platformPrefab;
    }
    
    public static GameObject SelectWeightedGameObject(List<WeightedEnemy> enemies)
    {
        var selected = SelectWeighted(enemies, e => e.weight);
        return selected?.enemyPrefab;
    }
    
    public static GameObject SelectWeightedGameObject(List<WeightedPowerUp> powerUps)
    {
        var selected = SelectWeighted(powerUps, p => p.weight);
        return selected?.powerUpPrefab;
    }
    
    public static WeightedPowerUp SelectWeightedPowerUp(List<WeightedPowerUp> powerUps)
    {
        return SelectWeighted(powerUps, p => p.weight);
    }
    
    public static WeightedEnemy SelectWeightedEnemy(List<WeightedEnemy> enemies)
    {
        return SelectWeighted(enemies, e => e.weight);
    }
    
    // Specialized selections
    public static GameObject SelectChainEnemy(List<WeightedEnemy> enemies)
    {
        var chainEnemies = enemies.Where(e => e.canBeUsedInChains).ToList();
        var selected = SelectWeighted(chainEnemies, e => e.weight);
        return selected?.enemyPrefab;
    }
    
    public static GameObject SelectScreenBlockEnemy(List<WeightedEnemy> enemies)
    {
        var blockEnemies = enemies.Where(e => e.canBeUsedInScreenBlocks).ToList();
        var selected = SelectWeighted(blockEnemies, e => e.weight);
        return selected?.enemyPrefab;
    }
    
    public static List<WeightedPowerUp> SelectPowerUpsByType(List<WeightedPowerUp> powerUps, PowerUpType type)
    {
        return powerUps.Where(p => p.powerUpType == type).ToList();
    }
}