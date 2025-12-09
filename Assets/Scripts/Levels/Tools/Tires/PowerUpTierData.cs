using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New PowerUp Tier", menuName = "Level Generation/PowerUp Tier")]
public class PowerUpTierData : ScriptableObject
{
    [Header("Tier Info")]
    [Tooltip("Name for this power-up tier")]
    public string tierName = "New PowerUp Tier";
    [TextArea(2, 3)]
    public string description = "Describe what power-ups this tier contains...";
    
    [Header("Height Range")]
    [Tooltip("Minimum height where this tier can be used")]
    public float minHeight = 0f;
    [Tooltip("Maximum height where this tier can be used (use very high number for no limit)")]
    public float maxHeight = 1000f;
    
    [Header("Available Power-ups")]
    [Tooltip("All power-ups available in this tier with their weights and properties")]
    public List<WeightedPowerUp> availablePowerUps = new List<WeightedPowerUp>();
    
    [Header("Special Power-ups")]
    [Tooltip("Specific power-up to use for blood transfusions (leave empty to use first Health type)")]
    public GameObject bloodTransfusionOverride;
    [Tooltip("Specific power-up to use for oxygen pickups (leave empty to use first Health type)")]
    public GameObject oxygenPickupOverride;
    
    [Header("Visual")]
    [Tooltip("Color for editor visualization")]
    public Color tierColor = Color.green;
    
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(tierName) || tierName == "New PowerUp Tier")
        {
            tierName = $"PowerUp Tier {minHeight}-{maxHeight}";
        }
        
        if (minHeight > maxHeight)
        {
            minHeight = maxHeight;
        }
        
        // Remove null power-ups
        availablePowerUps.RemoveAll(p => p.powerUpPrefab == null);
    }
    
    /// <summary>
    /// Check if this tier is valid for the given height
    /// </summary>
    public bool IsValidForHeight(float height)
    {
        return height >= minHeight && height < maxHeight && availablePowerUps.Count > 0;
    }
    
    /// <summary>
    /// Get a random power-up of a specific type
    /// </summary>
    public GameObject GetRandomPowerUpByType(PowerUpType type)
    {
        var powerUpsOfType = WeightedSelector.SelectPowerUpsByType(availablePowerUps, type);
        return WeightedSelector.SelectWeightedGameObject(powerUpsOfType);
    }
    
    /// <summary>
    /// Get a WeightedPowerUp of a specific type (includes offset info)
    /// </summary>
    public WeightedPowerUp GetRandomWeightedPowerUpByType(PowerUpType type)
    {
        var powerUpsOfType = WeightedSelector.SelectPowerUpsByType(availablePowerUps, type);
        return WeightedSelector.SelectWeightedPowerUp(powerUpsOfType);
    }
    
    /// <summary>
    /// Get platform-spawnable boosts
    /// </summary>
    public List<WeightedPowerUp> GetPlatformBoosts()
    {
        return availablePowerUps.Where(p => p.powerUpType == PowerUpType.Boost && p.spawnOnPlatforms).ToList();
    }
    
    /// <summary>
    /// Get blood transfusion power-up
    /// </summary>
    public GameObject GetBloodTransfusion()
    {
        if (bloodTransfusionOverride != null)
            return bloodTransfusionOverride;
            
        return GetRandomPowerUpByType(PowerUpType.Health);
    }
    
    /// <summary>
    /// Get oxygen pickup power-up
    /// </summary>
    public GameObject GetOxygenPickup()
    {
        if (oxygenPickupOverride != null)
            return oxygenPickupOverride;
            
        return GetRandomPowerUpByType(PowerUpType.Health);
    }
    
    /// <summary>
    /// Get power-ups by category
    /// </summary>
    public List<WeightedPowerUp> GetBoosts()
    {
        return WeightedSelector.SelectPowerUpsByType(availablePowerUps, PowerUpType.Boost);
    }
    
    public List<WeightedPowerUp> GetHealthPowerUps()
    {
        return WeightedSelector.SelectPowerUpsByType(availablePowerUps, PowerUpType.Health);
    }
    
    public List<WeightedPowerUp> GetWeapons()
    {
        return WeightedSelector.SelectPowerUpsByType(availablePowerUps, PowerUpType.Weapon);
    }
    
    public List<WeightedPowerUp> GetSpecialPowerUps()
    {
        return WeightedSelector.SelectPowerUpsByType(availablePowerUps, PowerUpType.Special);
    }
}