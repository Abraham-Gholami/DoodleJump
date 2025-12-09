using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PowerUpSpawner : MonoBehaviour
{/*
    [Header("Power-up Spawn Settings")]
    [Tooltip("Default offset from spawn position")]
    public Vector3 defaultSpawnOffset = new Vector3(0, 0.5f, 0);
    
    private PlatformSpawner platformSpawner;
    
    private void Start()
    {
        InitializeSpawner();
    }
    
    private void InitializeSpawner()
    {
        platformSpawner = GetComponent<PlatformSpawner>();
        if (platformSpawner == null)
        {
            Debug.LogError("PowerUpSpawner: PlatformSpawner component not found!");
        }
    }
    
    public void SpawnBloodTransfusion(float yPosition, LevelPartData partData)
    {
        if (!ValidatePartData(partData)) return;
        
        var healthPowerUps = GetPowerUpsByType(partData.powerUpTypes, PowerUpType.Health);
        if (healthPowerUps.Count == 0)
        {
            Debug.LogError($"PowerUpSpawner: No Health power-ups found in part '{partData.partName}' for blood transfusion!");
            return;
        }
        
        var selectedPowerUp = WeightedSelector.SelectWeightedPowerUp(healthPowerUps);
        Vector3 spawnPosition = GenerateRandomPosition(yPosition);
        
        SpawnPowerUp(selectedPowerUp, spawnPosition, "Blood Transfusion");
    }
    
    public void SpawnOxygenPickup(float partStartY, float partEndY, LevelPartData partData)
    {
        if (!ValidatePartData(partData)) return;
        
        var healthPowerUps = GetPowerUpsByType(partData.powerUpTypes, PowerUpType.Health);
        if (healthPowerUps.Count == 0)
        {
            Debug.LogError($"PowerUpSpawner: No Health power-ups found in part '{partData.partName}' for oxygen pickup!");
            return;
        }
        
        var selectedPowerUp = WeightedSelector.SelectWeightedPowerUp(healthPowerUps);
        float randomY = Random.Range(partStartY, partEndY);
        Vector3 spawnPosition = GenerateRandomPosition(randomY);
        
        SpawnPowerUp(selectedPowerUp, spawnPosition, "Oxygen Pickup");
    }
    
    public void SpawnBoostsForPart(List<Vector3> platformPositions, float spawnChance, LevelPartData partData)
    {
        if (!ValidatePartData(partData)) return;
        
        var boostPowerUps = GetBoostPowerUpsForPlatforms(partData.powerUpTypes);
        if (boostPowerUps.Count == 0)
        {
            Debug.LogWarning($"PowerUpSpawner: No platform-spawnable Boost power-ups found in part '{partData.partName}'");
            return;
        }
        
        int spawnedCount = 0;
        foreach (var platformPosition in platformPositions)
        {
            if (ShouldSpawnBoost(spawnChance))
            {
                var selectedPowerUp = WeightedSelector.SelectWeightedPowerUp(boostPowerUps);
                Vector3 spawnPosition = CalculatePlatformSpawnPosition(platformPosition, selectedPowerUp);
                
                if (SpawnPowerUp(selectedPowerUp, spawnPosition, "Platform Boost"))
                {
                    spawnedCount++;
                }
            }
        }
        
        if (spawnedCount > 0)
        {
            Debug.Log($"PowerUpSpawner: Spawned {spawnedCount} boosts on platforms");
        }
    }
    
    public void SpawnSpecialPowerUp(PowerUpType powerUpType, Vector3 position, LevelPartData partData)
    {
        if (!ValidatePartData(partData)) return;
        
        var specialPowerUps = GetPowerUpsByType(partData.powerUpTypes, powerUpType);
        if (specialPowerUps.Count == 0)
        {
            Debug.LogWarning($"PowerUpSpawner: No {powerUpType} power-ups found in part '{partData.partName}'");
            return;
        }
        
        var selectedPowerUp = WeightedSelector.SelectWeightedPowerUp(specialPowerUps);
        SpawnPowerUp(selectedPowerUp, position, $"Special {powerUpType}");
    }
    
    private bool ValidatePartData(LevelPartData partData)
    {
        if (partData == null)
        {
            Debug.LogError("PowerUpSpawner: No part data provided!");
            return false;
        }
        
        if (partData.powerUpTypes.Count == 0)
        {
            Debug.LogError($"PowerUpSpawner: Part '{partData.partName}' has no power-up types!");
            return false;
        }
        
        return true;
    }
    
    private bool SpawnPowerUp(WeightedPowerUp powerUp, Vector3 position, string logContext)
    {
        if (powerUp?.powerUpPrefab == null)
        {
            Debug.LogError($"PowerUpSpawner: Selected power-up is null for {logContext}!");
            return false;
        }
        
        Vector3 finalPosition = position + powerUp.spawnOffset;
        GameObject spawnedPowerUp = Instantiate(powerUp.powerUpPrefab, finalPosition, Quaternion.identity);
        
        if (spawnedPowerUp != null)
        {
            Debug.Log($"PowerUpSpawner: {logContext} '{powerUp.powerUpPrefab.name}' spawned at {finalPosition}");
            return true;
        }
        
        return false;
    }
    
    private Vector3 GenerateRandomPosition(float yPosition)
    {
        var spawnBoundaries = platformSpawner.GetSpawnBoundaries();
        float randomX = Random.Range(spawnBoundaries.x, spawnBoundaries.y);
        return new Vector3(randomX, yPosition, 0);
    }
    
    private Vector3 CalculatePlatformSpawnPosition(Vector3 platformPosition, WeightedPowerUp powerUp)
    {
        Vector3 offset = powerUp?.spawnOffset ?? defaultSpawnOffset;
        return platformPosition + offset;
    }
    
    private bool ShouldSpawnBoost(float spawnChance)
    {
        return Random.Range(0f, 1f) < spawnChance;
    }
    
    private List<WeightedPowerUp> GetPowerUpsByType(List<WeightedPowerUp> powerUps, PowerUpType targetType)
    {
        return powerUps.Where(powerUp => 
            powerUp.powerUpType == targetType && 
            powerUp.powerUpPrefab != null).ToList();
    }
    
    private List<WeightedPowerUp> GetBoostPowerUpsForPlatforms(List<WeightedPowerUp> powerUps)
    {
        return powerUps.Where(powerUp => 
            powerUp.powerUpType == PowerUpType.Boost && 
            powerUp.spawnOnPlatforms && 
            powerUp.powerUpPrefab != null).ToList();
    }
    
    public bool HasValidPowerUpsForType(LevelPartData partData, PowerUpType powerUpType)
    {
        if (partData?.powerUpTypes == null) return false;
        
        return GetPowerUpsByType(partData.powerUpTypes, powerUpType).Count > 0;
    }
    
    public int GetAvailablePowerUpCount(LevelPartData partData, PowerUpType powerUpType)
    {
        if (partData?.powerUpTypes == null) return 0;
        
        return GetPowerUpsByType(partData.powerUpTypes, powerUpType).Count;
    }
    
    [ContextMenu("Test Spawn Random Power-up")]
    public void TestSpawnRandomPowerUp()
    {
        if (platformSpawner != null)
        {
            Vector3 testPosition = GenerateRandomPosition(10f);
            Debug.Log($"PowerUpSpawner: Test spawn position generated at {testPosition}");
        }
    }*/
}