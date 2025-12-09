using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Handles player immunity/shield system
/// Compatible with modular PlayerController structure
/// </summary>
public class PlayerImmunity : MonoBehaviour
{
    [Header("Immunity Settings")]
    [SerializeField] private float immunityDuration = 5f;
    [SerializeField] private float flashInterval = 0.15f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip immunityStartSound;
    [SerializeField] private AudioClip immunityEndSound;
    
    [Header("Visual Effects")]
    [FormerlySerializedAs("glowEffectPrefab")]
    [SerializeField] private GameObject glowEffectGo; // Particle system or glow sprite
    [SerializeField] private bool usePlayerCombatShield = true; // Use PlayerCombat's shield instead
    [SerializeField] private bool enableFlashing = true; // Enable sprite flashing effect
    
    private bool isImmune = false;
    private float immunityTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Material originalMaterial;
    private Coroutine immunityCoroutine;
    private Coroutine flashCoroutine;
    private GameObject activeGlowEffect;
    
    // Component references
    private PlayerCombat playerCombat;
    
    // Public properties
    public bool IsImmune => isImmune;
    public float RemainingImmunityTime => isImmune ? immunityTimer : 0f;
    public float ImmunityProgress => isImmune ? (immunityTimer / immunityDuration) : 0f;
    
    // Events
    public System.Action<float> OnImmunityStarted;
    public System.Action OnImmunityEnded;
    public System.Action<float> OnImmunityTimeChanged;
    
    private void Awake()
    {
        InitializeComponents();
        CacheOriginalMaterials();
    }
    
    private void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCombat = GetComponent<PlayerCombat>();
        
        if (spriteRenderer == null)
        {
            Debug.LogError("PlayerImmunity: SpriteRenderer component not found!");
        }
        
        if (playerCombat == null && usePlayerCombatShield)
        {
            Debug.LogWarning("PlayerImmunity: PlayerCombat component not found but usePlayerCombatShield is enabled");
        }
    }
    
    private void CacheOriginalMaterials()
    {
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            originalMaterial = spriteRenderer.material;
        }
    }
    
    private void Start()
    {
        ValidateConfiguration();
    }
    
    private void ValidateConfiguration()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("PlayerImmunity: AudioSource not assigned, immunity sounds will be silent");
        }
        
        if (!usePlayerCombatShield && glowEffectGo == null)
        {
            Debug.LogWarning("PlayerImmunity: No visual effect configured (neither combat shield nor glow effect)");
        }
        
        if (immunityDuration <= 0f)
        {
            Debug.LogWarning("PlayerImmunity: Immunity duration should be greater than 0");
        }
    }
    
    public void ActivateImmunity(float duration = -1f)
    {
        // Use default duration if none provided
        if (duration < 0)
            duration = immunityDuration;
        
        Debug.Log($"PlayerImmunity: Activating immunity for {duration} seconds");
        
        // Reset immunity duration (don't extend, just reset)
        immunityTimer = duration;
        
        // If already immune, just reset the timer and continue
        if (isImmune)
        {
            Debug.Log($"PlayerImmunity: Already immune, resetting timer to {duration}");
            OnImmunityTimeChanged?.Invoke(immunityTimer);
            return;
        }
        
        // Start immunity
        StartImmunity(duration);
    }
    
    private void StartImmunity(float duration)
    {
        isImmune = true;
        Debug.Log($"PlayerImmunity: Starting immunity for {duration} seconds");
        
        // Play start sound
        PlayImmunityStartSound();
        
        // Start visual effects
        StartVisualEffects();
        
        // Start countdown
        StartImmunityCountdown();
        
        // Trigger event
        OnImmunityStarted?.Invoke(duration);
    }
    
    private void StartVisualEffects()
    {
        // Start glow effect based on configuration
        if (usePlayerCombatShield)
        {
            Debug.Log("PlayerImmunity: Using PlayerCombat shield system for visual effects");
            // PlayerCombat will handle the shield visual through event subscription
        }
        else
        {
            StartGlowEffect();
        }
        
        // Start flashing effect if enabled
        if (enableFlashing && spriteRenderer != null)
        {
            StartFlashingEffect();
        }
    }
    
    private void StartGlowEffect()
    {
        if (glowEffectGo != null)
        {
            Debug.Log("PlayerImmunity: Activating glow effect");
            glowEffectGo.SetActive(true);
        }
        else
        {
            // Try to find glow effect as child
            Transform glowTransform = transform.Find("GlowEffect");
            if (glowTransform != null)
            {
                glowEffectGo = glowTransform.gameObject;
                glowEffectGo.SetActive(true);
                Debug.Log("PlayerImmunity: Found and activated glow effect from child");
            }
            else
            {
                Debug.LogWarning("PlayerImmunity: No glow effect GameObject found");
            }
        }
    }
    
    private void StartFlashingEffect()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashingCoroutine());
    }
    
    private void StartImmunityCountdown()
    {
        if (immunityCoroutine != null)
        {
            StopCoroutine(immunityCoroutine);
        }
        immunityCoroutine = StartCoroutine(ImmunityCountdownCoroutine());
    }
    
    private IEnumerator ImmunityCountdownCoroutine()
    {
        while (immunityTimer > 0f)
        {
            immunityTimer -= Time.deltaTime;
            OnImmunityTimeChanged?.Invoke(immunityTimer);
            yield return null;
        }
        
        // End immunity when timer reaches zero
        EndImmunity();
    }
    
    private IEnumerator FlashingCoroutine()
    {
        if (spriteRenderer == null) yield break;
        
        Color flashColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f); // Semi-transparent
        
        while (isImmune)
        {
            // Flash to semi-transparent
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashInterval);
            
            // Flash back to original (but keep immunity going)
            if (isImmune) // Check again in case immunity ended during wait
            {
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashInterval);
            }
        }
        
        // Ensure original color is restored
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
    
    private void EndImmunity()
    {
        Debug.Log("PlayerImmunity: Ending immunity");
        
        isImmune = false;
        immunityTimer = 0f;
        
        // Stop all visual effects
        StopVisualEffects();
        
        // Play end sound
        PlayImmunityEndSound();
        
        // Clean up coroutine references
        immunityCoroutine = null;
        
        // Trigger event
        OnImmunityEnded?.Invoke();
    }
    
    private void StopVisualEffects()
    {
        // Stop glow effect
        StopGlowEffect();
        
        // Stop flashing effect
        StopFlashingEffect();
    }
    
    private void StopGlowEffect()
    {
        if (usePlayerCombatShield)
        {
            Debug.Log("PlayerImmunity: PlayerCombat will handle shield deactivation");
        }
        else if (glowEffectGo != null)
        {
            Debug.Log("PlayerImmunity: Deactivating glow effect");
            glowEffectGo.SetActive(false);
        }
    }
    
    private void StopFlashingEffect()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        
        // Restore original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            Debug.Log("PlayerImmunity: Original sprite color restored");
        }
    }
    
    private void PlayImmunityStartSound()
    {
        if (audioSource != null && immunityStartSound != null)
        {
            audioSource.PlayOneShot(immunityStartSound);
            Debug.Log("PlayerImmunity: Playing immunity start sound");
        }
    }
    
    private void PlayImmunityEndSound()
    {
        if (audioSource != null && immunityEndSound != null)
        {
            audioSource.PlayOneShot(immunityEndSound);
            Debug.Log("PlayerImmunity: Playing immunity end sound");
        }
    }
    
    // Public methods for external control
    public void ForceEndImmunity()
    {
        if (isImmune)
        {
            Debug.Log("PlayerImmunity: Force ending immunity");
            
            if (immunityCoroutine != null)
            {
                StopCoroutine(immunityCoroutine);
            }
            
            EndImmunity();
        }
    }
    
    public void ExtendImmunityTime(float additionalTime)
    {
        if (isImmune)
        {
            immunityTimer += additionalTime;
            immunityTimer = Mathf.Min(immunityTimer, immunityDuration * 2f); // Cap at 2x duration
            OnImmunityTimeChanged?.Invoke(immunityTimer);
            Debug.Log($"PlayerImmunity: Immunity time extended by {additionalTime} seconds");
        }
    }
    
    public void SetImmunityDuration(float newDuration)
    {
        immunityDuration = Mathf.Max(1f, newDuration); // Minimum 1 second
        Debug.Log($"PlayerImmunity: Immunity duration set to {immunityDuration} seconds");
    }
    
    public void SetFlashInterval(float newInterval)
    {
        flashInterval = Mathf.Max(0.05f, newInterval); // Minimum 50ms
        Debug.Log($"PlayerImmunity: Flash interval set to {flashInterval} seconds");
    }
    
    public void SetFlashingEnabled(bool enabled)
    {
        enableFlashing = enabled;
        
        if (!enabled && flashCoroutine != null)
        {
            StopFlashingEffect();
        }
        else if (enabled && isImmune && flashCoroutine == null)
        {
            StartFlashingEffect();
        }
        
        Debug.Log($"PlayerImmunity: Flashing effect {(enabled ? "enabled" : "disabled")}");
    }
    
    // Public status methods
    public bool IsFlashing()
    {
        return flashCoroutine != null;
    }
    
    public float GetImmunityDuration()
    {
        return immunityDuration;
    }
    
    private void OnDestroy()
    {
        // Clean up all coroutines
        if (immunityCoroutine != null)
        {
            StopCoroutine(immunityCoroutine);
        }
        
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        
        // Clean up glow effect
        if (activeGlowEffect != null)
        {
            Destroy(activeGlowEffect);
        }
        
        Debug.Log("PlayerImmunity: Component destroyed and cleaned up");
    }
    
    private void OnValidate()
    {
        // Editor validation
        if (immunityDuration <= 0f)
        {
            Debug.LogWarning("PlayerImmunity: Immunity duration should be greater than 0");
        }
        
        if (flashInterval <= 0f)
        {
            Debug.LogWarning("PlayerImmunity: Flash interval should be greater than 0");
        }
        
        if (flashInterval > immunityDuration / 4f)
        {
            Debug.LogWarning("PlayerImmunity: Flash interval seems too long compared to immunity duration");
        }
    }
}