// ============= IMPROVED BLOOD TRANSFUSION PICKUP =============
using UnityEngine;
using UnityEngine.Serialization;

public class BloodTransfusionPickup : MonoBehaviour
{
    [FormerlySerializedAs("heartValue")]
    [Header("Pickup Settings")]
    [SerializeField] private float immunityDuration = 5f; // Default value, can be overridden
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private bool resetDuration = true; // Reset to full duration instead of stacking
    
    [Header("Visual Effects")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private Vector3 startPosition;
    private AudioSource audioSource;
    private bool hasBeenPickedUp = false; // Prevent multiple pickups
    
    private void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        
        // Validate setup
        ValidateSetup();
        
        // Get shield duration from PlayerImmunity default if available
        PlayerImmunity playerImmunity = FindObjectOfType<PlayerImmunity>();
        if (playerImmunity != null)
        {
            // Use the default immunity duration from PlayerImmunity
            // Since we can't access GetInitialShieldDuration anymore
            if (enableDebugLogs)
                Debug.Log($"BloodTransfusion: Using default immunity duration {immunityDuration}");
        }
        else
        {
            if (enableDebugLogs)
                Debug.LogWarning("BloodTransfusion: No PlayerImmunity found, using default duration");
        }
    }
    
    private void ValidateSetup()
    {
        // Check for required components
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"BloodTransfusion: {gameObject.name} is missing Collider2D component!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogError($"BloodTransfusion: {gameObject.name} Collider2D must be set to IsTrigger = true!");
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"BloodTransfusion: {gameObject.name} setup complete at position {transform.position}");
        }
    }
    
    private void Update()
    {
        // Simple bobbing animation
        if (!hasBeenPickedUp)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenPickedUp) return; // Prevent double pickup
        
        if (enableDebugLogs)
            Debug.Log($"BloodTransfusion: Trigger entered by {other.gameObject.name} with tag '{other.tag}'");
        
        // Check for Player tag (your actual player tag)
        if (other.CompareTag("Player"))
        {
            if (enableDebugLogs)
                Debug.Log("BloodTransfusion: Player detected, attempting pickup...");
            
            // Get PlayerImmunity component directly (since PlayerController is now modular)
            PlayerImmunity playerImmunity = other.GetComponent<PlayerImmunity>();
            if (playerImmunity != null)
            {
                hasBeenPickedUp = true; // Mark as picked up immediately
                
                if (enableDebugLogs)
                {
                    Debug.Log($"BloodTransfusion: PlayerImmunity found. Current immunity: {playerImmunity.IsImmune}, Remaining time: {playerImmunity.RemainingImmunityTime}");
                }
                
                // Activate immunity directly through PlayerImmunity
                if (resetDuration)
                {
                    playerImmunity.ActivateImmunity(immunityDuration);
                    if (enableDebugLogs)
                        Debug.Log($"BloodTransfusion: Shield RESET to {immunityDuration} seconds via PlayerImmunity");
                }
                else
                {
                    // Extend current immunity
                    float currentTime = playerImmunity.RemainingImmunityTime;
                    float newDuration = currentTime + immunityDuration;
                    playerImmunity.ActivateImmunity(newDuration);
                    if (enableDebugLogs)
                        Debug.Log($"BloodTransfusion: Shield EXTENDED from {currentTime} to {newDuration} seconds via PlayerImmunity");
                }
                
                PlayPickupSound();
                
                // Destroy immediately to prevent further collisions
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError($"BloodTransfusion: Player {other.gameObject.name} is missing PlayerImmunity component!");
            }
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log($"BloodTransfusion: {other.gameObject.name} is not tagged as Player (tag: '{other.tag}')");
        }
    }
    
    private void PlayPickupSound()
    {
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
            if (enableDebugLogs)
                Debug.Log("BloodTransfusion: Pickup sound played");
        }
        else if (enableDebugLogs)
        {
            Debug.Log("BloodTransfusion: No pickup sound configured");
        }
    }
    
    // Method to set custom immunity duration (useful for different pickup types)
    public void SetImmunityDuration(float duration)
    {
        immunityDuration = duration;
        if (enableDebugLogs)
            Debug.Log($"BloodTransfusion: Immunity duration set to {duration}");
    }
    
    // Method to test pickup manually (for debugging)
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
            Debug.LogError("BloodTransfusion: No GameObject with 'Player' tag found for testing!");
        }
    }
}