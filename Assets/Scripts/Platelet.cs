using UnityEngine;

public class Platelet : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float shootDuration = 10f;
    [SerializeField] private AudioClip pickupSound;
    
    [Header("Visual Effects")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private Vector3 startPosition;
    private AudioSource audioSource;
    private bool hasBeenPickedUp = false;
    
    private void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        ValidateSetup();
    }
    
    private void ValidateSetup()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"PlatformShootPowerup: {gameObject.name} is missing Collider2D component!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogError($"PlatformShootPowerup: {gameObject.name} Collider2D must be set to IsTrigger = true!");
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"PlatformShootPowerup: {gameObject.name} setup complete at position {transform.position}");
        }
    }
    
    private void Update()
    {
        if (!hasBeenPickedUp)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenPickedUp) return;
        
        if (enableDebugLogs)
            Debug.Log($"PlatformShootPowerup: Trigger entered by {other.gameObject.name} with tag '{other.tag}'");
        
        if (other.CompareTag("Player"))
        {
            if (enableDebugLogs)
                Debug.Log("PlatformShootPowerup: Player detected, attempting pickup...");
            
            PlayerShooting playerShooting = other.GetComponent<PlayerShooting>();
            if (playerShooting != null)
            {
                hasBeenPickedUp = true;
                
                if (enableDebugLogs)
                {
                    Debug.Log($"PlatformShootPowerup: PlayerShooting found. Current power: {playerShooting.HasPlatformShootPower}, Remaining time: {playerShooting.RemainingPlatformShootTime}");
                }
                
                playerShooting.ActivatePlatformShootPower(shootDuration);
                
                if (enableDebugLogs)
                    Debug.Log($"PlatformShootPowerup: Platform shoot power activated for {shootDuration} seconds");
                
                PlayPickupSound();
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError($"PlatformShootPowerup: Player {other.gameObject.name} is missing PlayerShooting component!");
            }
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log($"PlatformShootPowerup: {other.gameObject.name} is not tagged as Player (tag: '{other.tag}')");
        }
    }
    
    private void PlayPickupSound()
    {
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
            if (enableDebugLogs)
                Debug.Log("PlatformShootPowerup: Pickup sound played");
        }
    }
    
    public void SetShootDuration(float duration)
    {
        shootDuration = duration;
        if (enableDebugLogs)
            Debug.Log($"PlatformShootPowerup: Shoot duration set to {duration}");
    }
    
    [ContextMenu("Test Pickup")]
    public void TestPickup()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            OnTriggerEnter2D(playerObject.GetComponent<Collider2D>());
        }
        else
        {
            Debug.LogError("PlatformShootPowerup: No GameObject with 'Player' tag found for testing!");
        }
    }
}