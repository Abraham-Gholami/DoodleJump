// ============= HEART UI MANAGER =============
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartUI : MonoBehaviour
{
    [Header("Heart Sprites")]
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;
    
    [Header("UI Settings")]
    [SerializeField] private GameObject heartPrefab; // Prefab with Image component
    [SerializeField] private Transform heartsContainer; // Parent object for hearts
    [SerializeField] private float heartSpacing = 60f; // Space between hearts
    
    [Header("Animation Settings")]
    [SerializeField] private float damageAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve damageShakeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);
    [SerializeField] private float shakeIntensity = 10f;
    
    private List<HeartSprite> heartSprites = new List<HeartSprite>();
    private PlayerHealth playerHealth;
    
    private void Awake()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        
        if (playerHealth == null)
        {
            Debug.LogError("HeartUI: No PlayerHealth component found!");
            return;
        }
        
        // Subscribe to health events
        playerHealth.OnHealthChanged += UpdateHeartDisplay;
        playerHealth.OnPlayerDeath += OnPlayerDeath;
    }
    
    private void Start()
    {
        InitializeHearts();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHeartDisplay;
            playerHealth.OnPlayerDeath -= OnPlayerDeath;
        }
    }
    
    private void InitializeHearts()
    {
        // Clear existing hearts
        ClearHearts();
        
        // Create heart sprites based on max health
        for (int i = 0; i < playerHealth.MaxHearts; i++)
        {
            CreateHeart(i);
        }
        
        // Update display to current health
        UpdateHeartDisplay(playerHealth.CurrentHearts);
    }
    
    private void CreateHeart(int index)
    {
        if (heartPrefab == null || heartsContainer == null)
        {
            Debug.LogError("HeartUI: Missing heart prefab or hearts container!");
            return;
        }
        
        // Instantiate heart
        GameObject heartObject = Instantiate(heartPrefab, heartsContainer);
        
        // Position heart
        RectTransform rectTransform = heartObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(index * heartSpacing, 0);
        }
        
        // Setup heart sprite component
        HeartSprite heartSprite = heartObject.GetComponent<HeartSprite>();
        if (heartSprite == null)
        {
            heartSprite = heartObject.AddComponent<HeartSprite>();
        }
        
        heartSprite.Initialize(fullHeartSprite, emptyHeartSprite);
        heartSprites.Add(heartSprite);
    }
    
    private void UpdateHeartDisplay(int currentHearts)
    {
        for (int i = 0; i < heartSprites.Count; i++)
        {
            if (i < currentHearts)
            {
                heartSprites[i].SetFull();
            }
            else
            {
                heartSprites[i].SetEmpty();
            }
        }
        
        // Play damage animation if health decreased
        if (currentHearts < playerHealth.MaxHearts && currentHearts >= 0)
        {
            StartCoroutine(PlayDamageAnimation());
        }
    }
    
    private IEnumerator PlayDamageAnimation()
    {
        float elapsed = 0f;
        Vector3 originalPosition = heartsContainer.localPosition;
        
        while (elapsed < damageAnimationDuration)
        {
            float normalizedTime = elapsed / damageAnimationDuration;
            float shakeValue = damageShakeCurve.Evaluate(normalizedTime) * shakeIntensity;
            
            // Apply random shake
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeValue, shakeValue),
                Random.Range(-shakeValue, shakeValue),
                0
            );
            
            heartsContainer.localPosition = originalPosition + shakeOffset;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Reset position
        heartsContainer.localPosition = originalPosition;
    }
    
    private void OnPlayerDeath()
    {
        // Optional: Add death animation here
        StartCoroutine(PlayDeathAnimation());
    }
    
    private IEnumerator PlayDeathAnimation()
    {
        // Simple fade out animation
        CanvasGroup canvasGroup = heartsContainer.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = heartsContainer.gameObject.AddComponent<CanvasGroup>();
        }
        
        float elapsed = 0f;
        float duration = 1f;
        
        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(1f, 0.3f, elapsed / duration);
            canvasGroup.alpha = alpha;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    private void ClearHearts()
    {
        foreach (HeartSprite heart in heartSprites)
        {
            if (heart != null && heart.gameObject != null)
            {
                DestroyImmediate(heart.gameObject);
            }
        }
        heartSprites.Clear();
    }
    
    // Public method to refresh hearts (useful when max hearts change)
    public void RefreshHearts()
    {
        InitializeHearts();
    }
}