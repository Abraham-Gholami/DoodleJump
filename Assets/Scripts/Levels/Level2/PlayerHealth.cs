// ============= UPDATED PLAYER HEALTH COMPONENT =============
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHearts = 3;
    [SerializeField] private float invincibilityDuration = 2f;
    [SerializeField] private float flashInterval = 0.1f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSoundClip;
    
    private int currentHearts;
    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;
    private GameController gameController;
    
    // Public properties
    public int CurrentHearts => currentHearts;
    public int MaxHearts => maxHearts;
    public bool IsInvincible => isInvincible;
    
    // Events
    public System.Action<int> OnHealthChanged;
    public System.Action OnPlayerDeath;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameController = FindObjectOfType<GameController>();
        
        // Initialize health
        currentHearts = maxHearts;
    }
    
    public bool TakeDamage(int damage = 1)
    {
        if (isInvincible) return false;
        
        currentHearts -= damage;
        currentHearts = Mathf.Max(0, currentHearts);
        
        // Play hit sound
        PlayHitSound();
        
        // Trigger events (HeartUI will listen to this)
        OnHealthChanged?.Invoke(currentHearts);
        
        if (currentHearts <= 0)
        {
            Die();
            return true; // Player died
        }
        else
        {
            StartInvincibility();
            return false; // Player survived
        }
    }
    
    public void AddHeart(int amount = 1)
    {
        currentHearts += amount;
        currentHearts = Mathf.Min(maxHearts, currentHearts);
        
        OnHealthChanged?.Invoke(currentHearts);
    }
    
    private void PlayHitSound()
    {
        if (audioSource != null && hitSoundClip != null)
        {
            audioSource.PlayOneShot(hitSoundClip);
        }
    }
    
    private void StartInvincibility()
    {
        if (!isInvincible)
        {
            StartCoroutine(InvincibilityCoroutine());
        }
    }
    
    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        
        // Flash effect
        StartCoroutine(FlashEffect());
        
        // Wait for invincibility duration
        yield return new WaitForSeconds(invincibilityDuration);
        
        isInvincible = false;
        
        // Ensure sprite is visible when invincibility ends
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }
    
    private IEnumerator FlashEffect()
    {
        if (spriteRenderer == null) yield break;
        
        Color originalColor = spriteRenderer.color;
        Color flashColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
        
        float elapsed = 0f;
        while (elapsed < invincibilityDuration && isInvincible)
        {
            // Toggle between normal and flash color
            spriteRenderer.color = (Time.time % (flashInterval * 2) < flashInterval) ? flashColor : originalColor;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Restore original color
        spriteRenderer.color = originalColor;
    }
    
    private void Die()
    {
        OnPlayerDeath?.Invoke();
        
        if (gameController != null)
        {
            gameController.Set_GameOver();
        }
    }
    
    // Public method to reset health (useful for game restart)
    public void ResetHealth()
    {
        currentHearts = maxHearts;
        isInvincible = false;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        
        // Notify UI to update
        OnHealthChanged?.Invoke(currentHearts);
    }
    
    // Public method to set max hearts (useful for power-ups or level progression)
    public void SetMaxHearts(int newMaxHearts)
    {
        maxHearts = newMaxHearts;
        currentHearts = Mathf.Min(currentHearts, maxHearts);
        
        // Refresh the heart UI to show new max hearts
        HeartUI heartUI = FindObjectOfType<HeartUI>();
        if (heartUI != null)
        {
            heartUI.RefreshHearts();
        }
        
        OnHealthChanged?.Invoke(currentHearts);
    }
}