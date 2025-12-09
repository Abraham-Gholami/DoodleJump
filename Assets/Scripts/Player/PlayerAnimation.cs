using System;
using System.Collections;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;
    private Sprite fallbackSprite;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    
        CacheFallbackSprite();
    
        // Enable collider immediately so player can land on platforms
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
    }
    
    private void Start()
    {
        // Start in falling state so player can collide with platforms immediately
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
    
        // Set initial falling sprite
        UpdateMovementSprite(true);
    }

    private void FixedUpdate()
    {
        // Wait for SettingsDataHolder to be ready
        if (SettingsDataHolder.Instance == null)
        {
            return;
        }
    
        UpdateSpriteAndCollider();
    }

    private void CacheFallbackSprite()
    {
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            fallbackSprite = spriteRenderer.sprite;
        }
    }

    private void UpdateSpriteAndCollider()
    {
        if (spriteRenderer == null || rb == null) return;
    
        // Don't update sprite if shooting animation is playing
        var shootingComponent = GetComponent<PlayerShooting>();
        if (shootingComponent != null && shootingComponent.IsShooting) return;
    
        bool isFalling = rb.linearVelocity.y <= 0; // Changed < to <= so standing counts as falling
        UpdateMovementSprite(isFalling);
    
        if (boxCollider != null)
        {
            boxCollider.enabled = isFalling; // Enabled when falling or standing still
        }
    }

    
    private void UpdateMovementSprite(bool isFalling)
    {
        var jumpFrames = SettingsDataHolder.SelectedCharacterJumpFrames;
        if (jumpFrames != null && jumpFrames.Length >= 2)
        {
            spriteRenderer.sprite = jumpFrames[isFalling ? 0 : 1];
        }
        else if (fallbackSprite != null)
        {
            spriteRenderer.sprite = fallbackSprite;
        }
        else
        {
            // No frames available yet - wait for next frame
            return;
        }
    }

    public IEnumerator PlayShootAnimation(System.Action onFireFrame)
    {
        var shootingSprites = SettingsDataHolder.SelectedCharacterShootFrames;

        if (shootingSprites == null || shootingSprites.Length == 0)
        {
            onFireFrame?.Invoke();
            yield break;
        }

        for (int i = 0; i < shootingSprites.Length; i++)
        {
            if (i == Mathf.Min(2, shootingSprites.Length - 1))
            {
                onFireFrame?.Invoke();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = shootingSprites[i];
            }
            
            yield return new WaitForSeconds(0.02f);
        }
    }

    public IEnumerator PlayDamageAnimation()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        Vector3 originalPosition = transform.position;

        // Flash red
        spriteRenderer.color = Color.red;

        // Shake effect
        const float shakeDuration = 0.2f;
        const float shakeIntensity = 0.1f;
        const int shakeSteps = 10;
        
        for (int i = 0; i < shakeSteps; i++)
        {
            Vector3 shakeOffset = new Vector3(
                UnityEngine.Random.Range(-shakeIntensity, shakeIntensity),
                UnityEngine.Random.Range(-shakeIntensity, shakeIntensity),
                0f
            );
            
            transform.position = originalPosition + shakeOffset;
            yield return new WaitForSeconds(shakeDuration / shakeSteps);
        }

        // Restore original state
        transform.position = originalPosition;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}