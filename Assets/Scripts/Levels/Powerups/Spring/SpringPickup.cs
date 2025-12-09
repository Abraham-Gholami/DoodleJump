using System.Collections;
using UnityEngine;

/// <summary>
/// Spring power pickup that gives player temporary super jump ability
/// Place this on pickup GameObjects in the world
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SpringPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float springDuration = 8f;
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private float rotationSpeed = 45f; // Visual rotation effect
    
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
            Debug.LogError("SpringPickup: Collider2D component is required!");
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("SpringPickup: No AudioSource found, pickup will be silent");
            }
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning("SpringPickup: No SpriteRenderer found, visual effects may not work");
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
        
        // Bobbing animation
        if (bobHeight > 0f)
        {
            Vector3 bobPosition = startPosition;
            bobPosition.y += Mathf.Sin(time * bobSpeed) * bobHeight;
            transform.position = bobPosition;
        }
        
        // Rotation animation
        if (rotationSpeed != 0f)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
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
        
        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("SpringPickup: Player doesn't have PlayerController component!");
            return;
        }
        
        // Try to give spring power to player
        PlayerSpringSystem springSystem = playerController.SpringSystem;
        if (springSystem == null)
        {
            Debug.LogWarning("SpringPickup: Player doesn't have PlayerSpringSystem component!");
            return;
        }
        
        // Successfully picked up
        PickupSpringPower(springSystem);
    }
    
    private void PickupSpringPower(PlayerSpringSystem springSystem)
    {
        isPickedUp = true;
        
        Debug.Log($"SpringPickup: Player collected spring power for {springDuration} seconds!");
        
        // Activate spring power on player
        springSystem.ActivateSpringPower(springDuration);
        
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
        // Wait for spring duration + buffer time before reactivating
        yield return new WaitForSeconds(springDuration + 2f);
        
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
        
        Debug.Log("SpringPickup: Pickup reactivated");
    }
    
    // Public methods for external control
    public void SetSpringDuration(float duration)
    {
        springDuration = Mathf.Max(1f, duration);
        Debug.Log($"SpringPickup: Spring duration set to {springDuration} seconds");
    }
    
    public void SetDestroyOnPickup(bool destroy)
    {
        destroyOnPickup = destroy;
        Debug.Log($"SpringPickup: Destroy on pickup set to {destroy}");
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
    
    public float GetSpringDuration() => springDuration;
    
    // Editor validation
    private void OnValidate()
    {
        if (springDuration <= 0f)
        {
            Debug.LogWarning("SpringPickup: Spring duration should be greater than 0");
        }
        
        if (bobHeight < 0f)
        {
            Debug.LogWarning("SpringPickup: Negative bob height may look weird");
        }
        
        if (pulseIntensity < 0f || pulseIntensity > 1f)
        {
            Debug.LogWarning("SpringPickup: Pulse intensity should be between 0 and 1");
        }
    }
    
    // Gizmo for editor visualization
    private void OnDrawGizmos()
    {
        // Draw pickup range
        Gizmos.color = Color.green;
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