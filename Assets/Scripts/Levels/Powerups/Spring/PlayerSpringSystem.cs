using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player's spring power system for super jumping
/// Compatible with modular PlayerController structure
/// </summary>
public class PlayerSpringSystem : MonoBehaviour
{
    [Header("Spring Settings")]
    [SerializeField] private float springDuration = 8f;
    [SerializeField] private float springJumpMultiplier = 1.5f; // 1.5x normal jump force
    [SerializeField] private GameObject springIndicator; // Visual indicator
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem springParticles;
    [SerializeField] private ParticleSystem jumpParticles; // Particles when jumping with spring power
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip springStartSound;
    [SerializeField] private AudioClip springEndSound;
    [SerializeField] private AudioClip springJumpSound; // Special sound when jumping with spring power
    
    private bool hasSpringPower = false;
    private float springTimer = 0f;
    private Coroutine springCoroutine;
    
    // Public properties
    public bool HasSpringPower => hasSpringPower;
    public float RemainingSpringTime => hasSpringPower ? springTimer : 0f;
    public float SpringProgress => hasSpringPower ? (springTimer / springDuration) : 0f;
    public float SpringJumpMultiplier => springJumpMultiplier;
    
    // Events
    public System.Action<float> OnSpringStarted;
    public System.Action OnSpringEnded;
    public System.Action<float> OnSpringTimeChanged;
    public System.Action OnSpringJumpUsed; // When player jumps with spring power
    
    private void Awake()
    {
        ValidateComponents();
    }
    
    private void Start()
    {
        InitializeSpringIndicator();
        InitializeParticleEffects();
        ValidateConfiguration();
    }
    
    private void ValidateComponents()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogWarning("PlayerSpringSystem: No AudioSource found, sounds will be silent");
            }
        }
    }
    
    private void InitializeSpringIndicator()
    {
        if (springIndicator == null)
        {
            Transform indicatorTransform = transform.Find("SpringIndicator");
            if (indicatorTransform != null)
            {
                springIndicator = indicatorTransform.gameObject;
                Debug.Log("PlayerSpringSystem: Auto-found spring indicator");
            }
        }
        
        if (springIndicator != null)
        {
            springIndicator.SetActive(false);
        }
    }
    
    private void InitializeParticleEffects()
    {
        // Auto-find particle systems if not assigned
        if (springParticles == null)
        {
            Transform particleTransform = transform.Find("SpringParticles");
            if (particleTransform != null)
            {
                springParticles = particleTransform.GetComponent<ParticleSystem>();
            }
        }
        
        if (jumpParticles == null)
        {
            Transform jumpParticleTransform = transform.Find("SpringJumpParticles");
            if (jumpParticleTransform != null)
            {
                jumpParticles = jumpParticleTransform.GetComponent<ParticleSystem>();
            }
        }
        
        // Stop particles initially
        if (springParticles != null && springParticles.isPlaying)
        {
            springParticles.Stop();
        }
    }
    
    private void ValidateConfiguration()
    {
        if (springJumpMultiplier <= 1f)
        {
            Debug.LogWarning("PlayerSpringSystem: Spring jump multiplier should be greater than 1 for super jumping effect");
        }
        
        if (springDuration <= 0f)
        {
            Debug.LogWarning("PlayerSpringSystem: Spring duration should be greater than 0");
        }
        
        if (springIndicator == null)
        {
            Debug.LogWarning("PlayerSpringSystem: No spring indicator assigned");
        }
        
        if (springParticles == null)
        {
            Debug.LogWarning("PlayerSpringSystem: No spring particles assigned");
        }
    }
    
    public void ActivateSpringPower(float duration = -1f)
    {
        if (duration < 0) duration = springDuration;
        
        Debug.Log($"PlayerSpringSystem: Activating spring power for {duration} seconds");
        
        springTimer = duration;
        
        if (hasSpringPower)
        {
            Debug.Log($"PlayerSpringSystem: Already has spring power, resetting timer to {duration}");
            OnSpringTimeChanged?.Invoke(springTimer);
            return;
        }
        
        StartSpringPower(duration);
    }
    
    private void StartSpringPower(float duration)
    {
        hasSpringPower = true;
        
        ActivateSpringIndicator();
        StartSpringParticles();
        PlaySpringStartSound();
        StartSpringCountdown();
        
        OnSpringStarted?.Invoke(duration);
        Debug.Log("PlayerSpringSystem: Spring power activated!");
    }
    
    private void ActivateSpringIndicator()
    {
        if (springIndicator != null)
        {
            springIndicator.SetActive(true);
            Debug.Log("PlayerSpringSystem: Spring indicator activated");
        }
    }
    
    private void StartSpringParticles()
    {
        if (springParticles != null)
        {
            springParticles.Play();
            Debug.Log("PlayerSpringSystem: Spring particles started");
        }
    }
    
    private void StartSpringCountdown()
    {
        if (springCoroutine != null)
        {
            StopCoroutine(springCoroutine);
        }
        springCoroutine = StartCoroutine(SpringCountdownCoroutine());
    }
    
    private IEnumerator SpringCountdownCoroutine()
    {
        while (springTimer > 0f)
        {
            springTimer -= Time.deltaTime;
            OnSpringTimeChanged?.Invoke(springTimer);
            yield return null;
        }
        
        EndSpringPower();
    }
    
    private void EndSpringPower()
    {
        Debug.Log("PlayerSpringSystem: Spring power ended");
        
        hasSpringPower = false;
        springTimer = 0f;
        
        DeactivateSpringIndicator();
        StopSpringParticles();
        PlaySpringEndSound();
        
        springCoroutine = null;
        OnSpringEnded?.Invoke();
    }
    
    private void DeactivateSpringIndicator()
    {
        if (springIndicator != null)
        {
            springIndicator.SetActive(false);
            Debug.Log("PlayerSpringSystem: Spring indicator deactivated");
        }
    }
    
    private void StopSpringParticles()
    {
        if (springParticles != null)
        {
            springParticles.Stop();
            Debug.Log("PlayerSpringSystem: Spring particles stopped");
        }
    }
    
    // Called by Platform when player jumps with spring power
    public void OnPlayerUsedSpringJump()
    {
        if (!hasSpringPower) return;
        
        PlaySpringJumpParticles();
        PlaySpringJumpSound();
        OnSpringJumpUsed?.Invoke();
        
        Debug.Log("PlayerSpringSystem: Player used spring jump!");
    }
    
    private void PlaySpringJumpParticles()
    {
        if (jumpParticles != null)
        {
            jumpParticles.Play();
        }
    }
    
    private void PlaySpringStartSound()
    {
        if (audioSource != null && springStartSound != null)
        {
            audioSource.PlayOneShot(springStartSound);
        }
    }
    
    private void PlaySpringEndSound()
    {
        if (audioSource != null && springEndSound != null)
        {
            audioSource.PlayOneShot(springEndSound);
        }
    }
    
    private void PlaySpringJumpSound()
    {
        if (audioSource != null && springJumpSound != null)
        {
            audioSource.PlayOneShot(springJumpSound);
        }
    }
    
    // Public methods for external control
    public void ForceEndSpringPower()
    {
        if (hasSpringPower)
        {
            if (springCoroutine != null)
            {
                StopCoroutine(springCoroutine);
            }
            EndSpringPower();
        }
    }
    
    public void ExtendSpringTime(float additionalTime)
    {
        if (hasSpringPower)
        {
            springTimer += additionalTime;
            springTimer = Mathf.Min(springTimer, springDuration * 2f); // Cap at 2x duration
            OnSpringTimeChanged?.Invoke(springTimer);
            Debug.Log($"PlayerSpringSystem: Spring time extended by {additionalTime} seconds");
        }
    }
    
    public void SetSpringDuration(float newDuration)
    {
        springDuration = Mathf.Max(1f, newDuration);
        Debug.Log($"PlayerSpringSystem: Spring duration set to {springDuration} seconds");
    }
    
    public void SetSpringJumpMultiplier(float newMultiplier)
    {
        springJumpMultiplier = Mathf.Max(1f, newMultiplier);
        Debug.Log($"PlayerSpringSystem: Spring jump multiplier set to {springJumpMultiplier}");
    }
    
    // Public compatibility methods
    public bool CanSuperJump() => hasSpringPower;
    
    public float GetSpringDuration() => springDuration;
    
    private void OnDestroy()
    {
        if (springCoroutine != null)
        {
            StopCoroutine(springCoroutine);
        }
        
        Debug.Log("PlayerSpringSystem: Component destroyed and cleaned up");
    }
    
    private void OnValidate()
    {
        if (springDuration <= 0f)
        {
            Debug.LogWarning("PlayerSpringSystem: Spring duration should be greater than 0");
        }
        
        if (springJumpMultiplier <= 1f)
        {
            Debug.LogWarning("PlayerSpringSystem: Spring jump multiplier should be greater than 1");
        }
    }
}