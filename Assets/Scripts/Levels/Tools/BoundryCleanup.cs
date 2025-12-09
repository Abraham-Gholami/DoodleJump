// ============= REUSABLE BOUNDARY CLEANUP COMPONENT =============
using UnityEngine;

public class BoundaryCleanup : MonoBehaviour
{
    [Header("Cleanup Settings")]
    [SerializeField]
    protected float destroyBuffer = 5f; // Distance below screen to destroy
    [SerializeField] protected bool onlyCleanupBelowScreen = true; // If false, also cleanup above/sides
    
    [Header("Special Conditions")]
    [SerializeField]
    protected bool waitForAudioToFinish = false; // Wait for audio before destroying
    
    [Header("Events & Audio")]
    [SerializeField]
    protected bool triggerEventOnCleanup = false;
    [SerializeField] protected bool playCleanupSound = false;
    [SerializeField] protected AudioClip cleanupSound;

    protected AudioSource audioSource;
    private Enemy enemyComponent; // For checking if enemy is dead
    private bool isBeingDestroyed = false;
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    private void FixedUpdate()
    {
        if (!isBeingDestroyed)
        {
            CheckBoundaryCleanup();
        }
    }
    
    private void CheckBoundaryCleanup()
    {
        bool shouldCleanup = false;
        
        if (onlyCleanupBelowScreen)
        {
            // Only cleanup when below screen (most common for Doodle Jump)
            float bottomScreenY = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
            shouldCleanup = transform.position.y < bottomScreenY - destroyBuffer;
        }
        else
        {
            // Cleanup when outside any screen boundary
            Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
            shouldCleanup = screenPos.x < -0.1f || screenPos.x > 1.1f || 
                           screenPos.y < -0.1f || screenPos.y > 1.1f;
        }
        
        if (shouldCleanup)
        {
            CleanupObject();
        }
    }
    
    private void CleanupObject()
    {
        isBeingDestroyed = true;
        
        // Trigger event if needed (for score tracking, analytics, etc.)
        if (triggerEventOnCleanup)
        {
            // Add your custom events here if needed
            // EventManager.TriggerEvent(EventName.OnObjectCleanedUp);
            Debug.Log($"Object {gameObject.name} cleaned up by boundary system");
        }
        
        // Play cleanup sound if needed
        if (playCleanupSound && audioSource != null && cleanupSound != null)
        {
            audioSource.PlayOneShot(cleanupSound);
            
            if (waitForAudioToFinish)
            {
                // Wait for sound to finish before destroying
                Destroy(gameObject, cleanupSound.length);
            }
            else
            {
                // Destroy immediately, sound will play briefly
                Destroy(gameObject);
            }
        }
        else
        {
            // No sound, destroy immediately
            Destroy(gameObject);
        }
    }
    
    // Public method to force cleanup (useful for testing or special cases)
    public void ForceCleanup()
    {
        if (!isBeingDestroyed)
        {
            CleanupObject();
        }
    }
    
    // Public method to temporarily disable cleanup (useful for special states)
    public void SetCleanupEnabled(bool enabled)
    {
        this.enabled = enabled;
    }
}