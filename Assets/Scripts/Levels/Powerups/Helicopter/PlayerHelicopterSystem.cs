using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player's helicopter power system for temporary flight
/// Compatible with modular PlayerController structure
/// </summary>
public class PlayerHelicopterSystem : MonoBehaviour
{
    [Header("Helicopter Settings")]
    [SerializeField] private float helicopterDuration = 6f;
    [SerializeField] private float flySpeed = 8f;
    [SerializeField] private float horizontalControlStrength = 0.8f; // How much horizontal control during flight
    [SerializeField] private GameObject helicopterIndicator; // Visual helicopter/propeller
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem helicopterParticles;
    [SerializeField] private ParticleSystem thrustParticles; // Downward thrust particles
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip helicopterStartSound;
    [SerializeField] private AudioClip helicopterEndSound;
    [SerializeField] private AudioClip helicopterLoopSound; // Continuous propeller sound
    
    private bool hasHelicopterPower = false;
    private float helicopterTimer = 0f;
    private Coroutine helicopterCoroutine;
    private AudioSource helicopterLoopAudioSource;
    private Vector2 originalGravityScale;
    
    // Component references
    private Rigidbody2D rb;
    private PlayerInputHandler inputHandler;
    
    // Original physics state
    private float originalDrag;
    private bool wasKinematic;
    
    // Public properties
    public bool HasHelicopterPower => hasHelicopterPower;
    public float RemainingHelicopterTime => hasHelicopterPower ? helicopterTimer : 0f;
    public float HelicopterProgress => hasHelicopterPower ? (helicopterTimer / helicopterDuration) : 0f;
    public bool IsFlying => hasHelicopterPower;
    
    // Events
    public System.Action<float> OnHelicopterStarted;
    public System.Action OnHelicopterEnded;
    public System.Action<float> OnHelicopterTimeChanged;
    
    private void Awake()
    {
        ValidateComponents();
        InitializeComponents();
    }
    
    private void Start()
    {
        InitializeHelicopterIndicator();
        InitializeParticleEffects();
        InitializeAudio();
        ValidateConfiguration();
        CacheOriginalPhysics();
    }
    
    private void ValidateComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        inputHandler = GetComponent<PlayerInputHandler>();
        
        if (rb == null)
        {
            Debug.LogError("PlayerHelicopterSystem: Rigidbody2D component is required!");
        }
        
        if (inputHandler == null)
        {
            Debug.LogWarning("PlayerHelicopterSystem: PlayerInputHandler not found, horizontal control may not work");
        }
    }
    
    private void InitializeComponents()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("PlayerHelicopterSystem: No AudioSource found, sounds will be silent");
            }
        }
    }
    
    private void InitializeHelicopterIndicator()
    {
        if (helicopterIndicator == null)
        {
            Transform indicatorTransform = transform.Find("HelicopterIndicator") ?? transform.Find("Propeller");
            if (indicatorTransform != null)
            {
                helicopterIndicator = indicatorTransform.gameObject;
                Debug.Log("PlayerHelicopterSystem: Auto-found helicopter indicator");
            }
        }
        
        if (helicopterIndicator != null)
        {
            helicopterIndicator.SetActive(false);
        }
    }
    
    private void InitializeParticleEffects()
    {
        // Auto-find particle systems if not assigned
        if (helicopterParticles == null)
        {
            Transform particleTransform = transform.Find("HelicopterParticles");
            if (particleTransform != null)
            {
                helicopterParticles = particleTransform.GetComponent<ParticleSystem>();
            }
        }
        
        if (thrustParticles == null)
        {
            Transform thrustTransform = transform.Find("ThrustParticles");
            if (thrustTransform != null)
            {
                thrustParticles = thrustTransform.GetComponent<ParticleSystem>();
            }
        }
        
        // Stop particles initially
        if (helicopterParticles != null && helicopterParticles.isPlaying)
        {
            helicopterParticles.Stop();
        }
        
        if (thrustParticles != null && thrustParticles.isPlaying)
        {
            thrustParticles.Stop();
        }
    }
    
    private void InitializeAudio()
    {
        // Create separate audio source for looping helicopter sound
        if (helicopterLoopSound != null)
        {
            GameObject loopAudioGO = new GameObject("HelicopterLoopAudio");
            loopAudioGO.transform.SetParent(transform);
            helicopterLoopAudioSource = loopAudioGO.AddComponent<AudioSource>();
            helicopterLoopAudioSource.clip = helicopterLoopSound;
            helicopterLoopAudioSource.loop = true;
            helicopterLoopAudioSource.playOnAwake = false;
        }
    }
    
    private void ValidateConfiguration()
    {
        if (helicopterDuration <= 0f)
        {
            Debug.LogWarning("PlayerHelicopterSystem: Helicopter duration should be greater than 0");
        }
        
        if (flySpeed <= 0f)
        {
            Debug.LogWarning("PlayerHelicopterSystem: Fly speed should be greater than 0");
        }
        
        if (helicopterIndicator == null)
        {
            Debug.LogWarning("PlayerHelicopterSystem: No helicopter indicator assigned");
        }
    }
    
    private void CacheOriginalPhysics()
    {
        if (rb != null)
        {
            originalGravityScale = new Vector2(rb.gravityScale, rb.gravityScale);
            originalDrag = rb.linearDamping;
            wasKinematic = rb.isKinematic;
        }
    }
    
    public void ActivateHelicopterPower(float duration = -1f)
    {
        if (duration < 0) duration = helicopterDuration;
        
        Debug.Log($"PlayerHelicopterSystem: Activating helicopter power for {duration} seconds");
        
        helicopterTimer = duration;
        
        if (hasHelicopterPower)
        {
            Debug.Log($"PlayerHelicopterSystem: Already has helicopter power, resetting timer to {duration}");
            OnHelicopterTimeChanged?.Invoke(helicopterTimer);
            return;
        }
        
        StartHelicopterPower(duration);
    }
    
    private void StartHelicopterPower(float duration)
    {
        hasHelicopterPower = true;
        
        ModifyPhysicsForFlight();
        ActivateHelicopterIndicator();
        StartHelicopterParticles();
        PlayHelicopterStartSound();
        StartHelicopterCountdown();
        
        OnHelicopterStarted?.Invoke(duration);
        Debug.Log("PlayerHelicopterSystem: Helicopter power activated - Player is now flying!");
    }
    
    private void ModifyPhysicsForFlight()
    {
        if (rb == null) return;
        
        // Override gravity and apply upward force
        rb.gravityScale = 0f;
        rb.linearDamping = 2f; // Add some air resistance
        
        Debug.Log("PlayerHelicopterSystem: Physics modified for flight");
    }
    
    private void ActivateHelicopterIndicator()
    {
        if (helicopterIndicator != null)
        {
            helicopterIndicator.SetActive(true);
            Debug.Log("PlayerHelicopterSystem: Helicopter indicator activated");
        }
    }
    
    private void StartHelicopterParticles()
    {
        if (helicopterParticles != null)
        {
            helicopterParticles.Play();
        }
        
        if (thrustParticles != null)
        {
            thrustParticles.Play();
        }
        
        Debug.Log("PlayerHelicopterSystem: Helicopter particles started");
    }
    
    private void StartHelicopterCountdown()
    {
        if (helicopterCoroutine != null)
        {
            StopCoroutine(helicopterCoroutine);
        }
        helicopterCoroutine = StartCoroutine(HelicopterCountdownCoroutine());
    }
    
    private IEnumerator HelicopterCountdownCoroutine()
    {
        while (helicopterTimer > 0f)
        {
            helicopterTimer -= Time.deltaTime;
            OnHelicopterTimeChanged?.Invoke(helicopterTimer);
            yield return null;
        }
        
        EndHelicopterPower();
    }
    
    private void FixedUpdate()
    {
        if (hasHelicopterPower)
        {
            HandleHelicopterMovement();
        }
    }
    
    private void HandleHelicopterMovement()
    {
        if (rb == null) return;
        
        // Apply upward flight force
        Vector2 velocity = rb.linearVelocity;
        velocity.y = flySpeed;
        
        // Apply horizontal control if input handler available
        if (inputHandler != null)
        {
            float horizontalInput = GetHorizontalInput();
            velocity.x = horizontalInput * flySpeed * horizontalControlStrength;
        }
        
        rb.linearVelocity = velocity;
    }
    
    private float GetHorizontalInput()
    {
        // Use the same input logic as PlayerInputHandler for consistency
        if (SettingsDataHolder.Instance == null) 
        {
            // Fallback to keyboard input
            return Input.GetAxis("Horizontal");
        }
        
        int controlType = SettingsDataHolder.ControlType;
        
        switch (controlType)
        {
            case 0: // Tilt controls
                float rawTilt = Mathf.Clamp(Input.acceleration.x, -1f, 1f);
                // Apply dead zone like PlayerInputHandler does
                if (Mathf.Abs(rawTilt) < 0.1f) // Using same dead zone as PlayerInputHandler
                    rawTilt = 0f;
                return rawTilt;
                
            case 1: // Joystick/keyboard controls
                float horizontal = Input.GetAxis("Horizontal");
                
                // Also check for joystick input if available
                if (horizontal == 0f && inputHandler != null)
                {
                    // Try to get joystick reference from input handler
                    var joystickField = inputHandler.GetType().GetField("joystick", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (joystickField != null)
                    {
                        var joystick = joystickField.GetValue(inputHandler) as Joystick;
                        if (joystick != null)
                        {
                            horizontal = joystick.Horizontal;
                        }
                    }
                }
                
                return horizontal;
                
            default:
                return Input.GetAxis("Horizontal");
        }
    }
    
    private void EndHelicopterPower()
    {
        Debug.Log("PlayerHelicopterSystem: Helicopter power ended");
        
        hasHelicopterPower = false;
        helicopterTimer = 0f;
        
        RestoreOriginalPhysics();
        DeactivateHelicopterIndicator();
        StopHelicopterParticles();
        PlayHelicopterEndSound();
        
        helicopterCoroutine = null;
        OnHelicopterEnded?.Invoke();
    }
    
    private void RestoreOriginalPhysics()
    {
        if (rb == null) return;
        
        // Restore original physics settings
        rb.gravityScale = originalGravityScale.x;
        rb.linearDamping = originalDrag;
        rb.isKinematic = wasKinematic;
        
        Debug.Log("PlayerHelicopterSystem: Original physics restored");
    }
    
    private void DeactivateHelicopterIndicator()
    {
        if (helicopterIndicator != null)
        {
            helicopterIndicator.SetActive(false);
            Debug.Log("PlayerHelicopterSystem: Helicopter indicator deactivated");
        }
    }
    
    private void StopHelicopterParticles()
    {
        if (helicopterParticles != null)
        {
            helicopterParticles.Stop();
        }
        
        if (thrustParticles != null)
        {
            thrustParticles.Stop();
        }
        
        Debug.Log("PlayerHelicopterSystem: Helicopter particles stopped");
    }
    
    private void PlayHelicopterStartSound()
    {
        if (audioSource != null && helicopterStartSound != null)
        {
            audioSource.PlayOneShot(helicopterStartSound);
        }
        
        if (helicopterLoopAudioSource != null)
        {
            helicopterLoopAudioSource.Play();
        }
    }
    
    private void PlayHelicopterEndSound()
    {
        if (audioSource != null && helicopterEndSound != null)
        {
            audioSource.PlayOneShot(helicopterEndSound);
        }
        
        if (helicopterLoopAudioSource != null)
        {
            helicopterLoopAudioSource.Stop();
        }
    }
    
    // Public methods for external control
    public void ForceEndHelicopterPower()
    {
        if (hasHelicopterPower)
        {
            if (helicopterCoroutine != null)
            {
                StopCoroutine(helicopterCoroutine);
            }
            EndHelicopterPower();
        }
    }
    
    public void ExtendHelicopterTime(float additionalTime)
    {
        if (hasHelicopterPower)
        {
            helicopterTimer += additionalTime;
            helicopterTimer = Mathf.Min(helicopterTimer, helicopterDuration * 2f); // Cap at 2x duration
            OnHelicopterTimeChanged?.Invoke(helicopterTimer);
            Debug.Log($"PlayerHelicopterSystem: Helicopter time extended by {additionalTime} seconds");
        }
    }
    
    public void SetHelicopterDuration(float newDuration)
    {
        helicopterDuration = Mathf.Max(1f, newDuration);
        Debug.Log($"PlayerHelicopterSystem: Helicopter duration set to {helicopterDuration} seconds");
    }
    
    public void SetFlySpeed(float newSpeed)
    {
        flySpeed = Mathf.Max(1f, newSpeed);
        Debug.Log($"PlayerHelicopterSystem: Fly speed set to {flySpeed}");
    }
    
    // Public compatibility methods
    public bool CanFly() => hasHelicopterPower;
    public float GetHelicopterDuration() => helicopterDuration;
    
    private void OnDestroy()
    {
        if (helicopterCoroutine != null)
        {
            StopCoroutine(helicopterCoroutine);
        }
        
        if (helicopterLoopAudioSource != null)
        {
            Destroy(helicopterLoopAudioSource.gameObject);
        }
        
        Debug.Log("PlayerHelicopterSystem: Component destroyed and cleaned up");
    }
    
    private void OnValidate()
    {
        if (helicopterDuration <= 0f)
        {
            Debug.LogWarning("PlayerHelicopterSystem: Helicopter duration should be greater than 0");
        }
        
        if (flySpeed <= 0f)
        {
            Debug.LogWarning("PlayerHelicopterSystem: Fly speed should be greater than 0");
        }
        
        if (horizontalControlStrength < 0f || horizontalControlStrength > 1f)
        {
            Debug.LogWarning("PlayerHelicopterSystem: Horizontal control strength should be between 0 and 1");
        }
    }
}