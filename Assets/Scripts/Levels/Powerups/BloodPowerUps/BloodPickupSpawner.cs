using UnityEngine;
using Random = UnityEngine.Random;

public class BloodPickupSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject bloodPickupPrefab;
    [SerializeField] private int spawnChance = 100; // 1 in N chance (higher = more rare)
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0.5f, 0);
    
    [Header("Spawn Conditions")]
    [SerializeField] private float minHeightToSpawn = 10f; // Don't spawn blood too early
    
    private PlatformAndEnemyGenerator platformGenerator;
    
    private void Start()
    {
        platformGenerator = FindObjectOfType<PlatformAndEnemyGenerator>();
    }

    private void OnEnable()
    {
        EventManager.StartListening<EventName, Vector3>(EventName.OnSpawnPlatform, OnBloodSpawnCheck);
    }

    private void OnDisable()
    {
        EventManager.StopListening<EventName, Vector3>(EventName.OnSpawnPlatform, OnBloodSpawnCheck);
    }

    public void TrySpawnBloodPickup(Vector3 platformPosition)
    {
        // Check height requirement
        if (platformPosition.y < minHeightToSpawn)
            return;
        
        // Check spawn chance
        if (Random.Range(1, spawnChance + 1) != 1)
            return;
        
        // Get spawn boundaries to ensure blood spawns within screen
        if (platformGenerator != null)
        {
            var boundaries = platformGenerator.GetSpawnBoundaries();
            Vector3 spawnPosition = platformPosition + spawnOffset;
            spawnPosition.x = Mathf.Clamp(spawnPosition.x, boundaries.x, boundaries.y);
            
            Instantiate(bloodPickupPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"Blood pickup spawned at {spawnPosition}");
        }
    }

    public void OnBloodSpawnCheck(Vector3 platformPosition)
    {
        TrySpawnBloodPickup(platformPosition);
    }
}