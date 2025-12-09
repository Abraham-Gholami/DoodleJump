using System.Collections;
using UnityEngine;

/// <summary>
/// Helicopter power pickup that gives player temporary flight ability with invulnerability
/// Place this on pickup GameObjects in the world
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HelicopterPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float helicopterDuration = 6f;
    [SerializeField] private bool destroyOnPickup = true;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem pickupParticles;
    [SerializeField] private GameObject pickupIndicator; // Glow or outline effect
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.3f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickupSound;
    
    [Header("Animation")]
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float bobSpeed = 1f;
    
    private Vector3 startPosition;
    private Color originalColor;
    private bool isPickedUp = false;
    private float timeOffset;
    
    private void Awake()
    {
        ValidateComponents();
        InitializePickup();
    }
    
    private void Start()
    {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, Mathf.PI * 2f); // Random offset for animation
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Make sure trigger is enabled
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    private void Update()
    {
        if (isPickedUp) return;
        
        AnimatePickup();
    }
    
    private void ValidateComponents()
    {
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogError("HelicopterPickup: Collider2D component is required!");
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("HelicopterPickup: No AudioSource found, pickup will be silent");
            }
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning("HelicopterPickup: No SpriteRenderer found, visual effects may not work");
            }
        }
    }
    
    private void InitializePickup()
    {
        // Auto-find particle system if not assigned
        if (pickupParticles == null)
        {
            pickupParticles = GetComponentInChildren<ParticleSystem>();
        }
        
        // Auto-find pickup indicator if not assigned
        if (pickupIndicator == null)
        {
            Transform indicator = transform.Find("PickupIndicator") ?? transform.Find("Glow");
            if (indicator != null)
            {
                pickupIndicator = indicator.gameObject;
            }
        }
        
        // Stop particles initially if they're playing
        if (pickupParticles != null && pickupParticles.isPlaying)
        {
            pickupParticles.Stop();
        }
    }
    
    private void AnimatePickup()
    {
        float time = Time.time + timeOffset;
        
        // Bobbing animation only
        if (bobHeight > 0f)
        {
            Vector3 bobPosition = startPosition;
            bobPosition.y += Mathf.Sin(time * bobSpeed) * bobHeight;
            transform.position = bobPosition;
        }
        
        // Pulsing color effect
        if (spriteRenderer != null && pulseIntensity > 0f)
        {
            float pulse = 1f + Mathf.Sin(time * pulseSpeed) * pulseIntensity;
            Color pulseColor = originalColor * pulse;
            pulseColor.a = originalColor.a; // Keep original alpha
            spriteRenderer.color = pulseColor;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPickedUp || !other.CompareTag("Player")) return;
        
        // Check if player is in helicopter mode - can't collect while flying
        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("HelicopterPickup: Player doesn't have PlayerController component!");
            return;
        }
        
        PlayerHelicopterSystem helicopterSystem = playerController.HelicopterSystem;
        if (helicopterSystem == null)
        {
            Debug.LogWarning("HelicopterPickup: Player doesn't have PlayerHelicopterSystem component!");
            return;
        }
        
        // Don't allow pickup while already flying
        if (helicopterSystem.IsFlying)
        {
            Debug.Log("HelicopterPickup: Can't collect helicopter pickup while already flying!");
            return;
        }
        
        // Successfully picked up
        PickupHelicopterPower(helicopterSystem);
    }
    
    private void PickupHelicopterPower(PlayerHelicopterSystem helicopterSystem)
    {
        isPickedUp = true;
        
        Debug.Log($"HelicopterPickup: Player collected helicopter power for {helicopterDuration} seconds!");
        
        // Activate helicopter power on player
        helicopterSystem.ActivateHelicopterPower(helicopterDuration);
        
        // Play pickup effects
        PlayPickupEffects();
        
        // Handle pickup cleanup
        if (destroyOnPickup)
        {
            StartCoroutine(DestroyAfterEffects());
        }
        else
        {
            // Disable pickup temporarily
            GetComponent<Collider2D>().enabled = false;
            StartCoroutine(ReactivatePickup());
        }
    }
    
    private void PlayPickupEffects()
    {
        // Play pickup sound
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
        
        // Play pickup particles
        if (pickupParticles != null)
        {
            pickupParticles.Play();
        }
        
        // Hide visual elements
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        if (pickupIndicator != null)
        {
            pickupIndicator.SetActive(false);
        }
    }
    
    private IEnumerator DestroyAfterEffects()
    {
        // Wait for audio to finish
        if (audioSource != null && audioSource.isPlaying)
        {
            yield return new WaitWhile(() => audioSource.isPlaying);
        }
        
        // Wait for particles to finish
        if (pickupParticles != null && pickupParticles.isPlaying)
        {
            yield return new WaitWhile(() => pickupParticles.isPlaying);
        }
        
        // Small additional delay to ensure everything finishes
        yield return new WaitForSeconds(0.1f);
        
        Destroy(gameObject);
    }
    
    private IEnumerator ReactivatePickup()
    {
        // Wait for helicopter duration + buffer time before reactivating
        yield return new WaitForSeconds(helicopterDuration + 3f);
        
        // Reactivate pickup
        isPickedUp = false;
        GetComponent<Collider2D>().enabled = true;
        
        // Show visual elements again
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        if (pickupIndicator != null)
        {
            pickupIndicator.SetActive(true);
        }
        
        Debug.Log("HelicopterPickup: Pickup reactivated");
    }
    
    // Public methods for external control
    public void SetHelicopterDuration(float duration)
    {
        helicopterDuration = Mathf.Max(1f, duration);
        Debug.Log($"HelicopterPickup: Helicopter duration set to {helicopterDuration} seconds");
    }
    
    public void SetDestroyOnPickup(bool destroy)
    {
        destroyOnPickup = destroy;
        Debug.Log($"HelicopterPickup: Destroy on pickup set to {destroy}");
    }
    
    public void ForcePickup()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            OnTriggerEnter2D(player.GetComponent<Collider2D>());
        }
    }
    
    public bool IsPickedUp() => isPickedUp;
    
    public float GetHelicopterDuration() => helicopterDuration;
    
    // Editor validation
    private void OnValidate()
    {
        if (helicopterDuration <= 0f)
        {
            Debug.LogWarning("HelicopterPickup: Helicopter duration should be greater than 0");
        }
        
        if (bobHeight < 0f)
        {
            Debug.LogWarning("HelicopterPickup: Negative bob height may look weird");
        }
        
        if (pulseIntensity < 0f || pulseIntensity > 1f)
        {
            Debug.LogWarning("HelicopterPickup: Pulse intensity should be between 0 and 1");
        }
    }
    
    // Gizmo for editor visualization
    private void OnDrawGizmos()
    {
        // Draw pickup range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        // Draw bob range if bobbing is enabled
        if (bobHeight > 0f)
        {
            Gizmos.color = Color.yellow;
            Vector3 topPos = transform.position + Vector3.up * bobHeight;
            Vector3 bottomPos = transform.position - Vector3.up * bobHeight;
            Gizmos.DrawLine(topPos, bottomPos);
        }
    }
}