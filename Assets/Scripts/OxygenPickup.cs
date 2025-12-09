// ============= OXYGEN PICKUP =============
using UnityEngine;

public class OxygenPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float oxygenDuration = 10f;
    [SerializeField] private AudioClip pickupSound;
    
    [Header("Visual Effects")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private ParticleSystem pickupEffect;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private Vector3 startPosition;
    private AudioSource audioSource;
    private bool hasBeenPickedUp = false;
    
    private void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        
        if (enableDebugLogs)
            Debug.Log($"OxygenPickup: Spawned at {transform.position} with duration {oxygenDuration}s");
    }
    
    private void Update()
    {
        // Bobbing animation
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
            Debug.Log($"OxygenPickup: Trigger entered by {other.gameObject.name} with tag '{other.tag}'");
        
        if (other.CompareTag("Player"))
        {
            if (enableDebugLogs)
                Debug.Log("OxygenPickup: Player detected, attempting pickup...");
            
            PlayerOxygenSystem oxygenSystem = other.GetComponent<PlayerOxygenSystem>();
            if (oxygenSystem != null)
            {
                hasBeenPickedUp = true;
                
                // Activate oxygen power
                oxygenSystem.ActivateOxygenPower(oxygenDuration);
                
                if (enableDebugLogs)
                    Debug.Log($"OxygenPickup: Oxygen power activated for {oxygenDuration} seconds");
                
                // Play effects
                PlayPickupEffects();
                
                // Destroy pickup immediately to prevent further collisions
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("OxygenPickup: Player missing PlayerOxygenSystem component!");
            }
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log($"OxygenPickup: {other.gameObject.name} is not tagged as Player (tag: '{other.tag}')");
        }
    }
    
    private void PlayPickupEffects()
    {
        // Play sound
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
        
        // Play particle effect
        if (pickupEffect != null)
        {
            pickupEffect.Play();
        }
    }
    
    // Method to set custom duration
    public void SetOxygenDuration(float duration)
    {
        oxygenDuration = duration;
    }
}