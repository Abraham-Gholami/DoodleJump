using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class HeartPickupSpawner : MonoBehaviour
{
    [FormerlySerializedAs("bloodPickupPrefab")]
    [Header("Spawn Settings")]
    [SerializeField] private GameObject heartPickupPrefab;
    [SerializeField] private int spawnChance = 100; // 1 in N chance (higher = more rare)
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0.5f, 0);
    
    [Header("Spawn Conditions")]
    [SerializeField] private float minHeightToSpawn = 10f; // Don't spawn hearts too early
    
    private PlatformAndEnemyGenerator platformGenerator;
    
    private void Start()
    {
        platformGenerator = FindObjectOfType<PlatformAndEnemyGenerator>();
    }

    private void OnEnable()
    {
        EventManager.StartListening<EventName, Vector3>(EventName.OnSpawnPlatform, OnHeartSpawnCheck);
    }

    private void OnDisable()
    {
        EventManager.StopListening<EventName, Vector3>(EventName.OnSpawnPlatform, OnHeartSpawnCheck);

    }

    public void TrySpawnHeartPickup(Vector3 platformPosition)
    {
        // Check height requirement
        if (platformPosition.y < minHeightToSpawn)
            return;
        
        // Check spawn chance
        if (Random.Range(1, spawnChance + 1) != 1)
            return;
        
        // Get spawn boundaries to ensure heart spawns within screen
        if (platformGenerator != null)
        {
            var boundaries = platformGenerator.GetSpawnBoundaries();
            Vector3 spawnPosition = platformPosition + spawnOffset;
            spawnPosition.x = Mathf.Clamp(spawnPosition.x, boundaries.x, boundaries.y);
            
            Instantiate(heartPickupPrefab, spawnPosition, Quaternion.identity);
        }
    }

    public void OnHeartSpawnCheck(Vector3 platformPosition)
    {
        TrySpawnHeartPickup(platformPosition);
    }
}