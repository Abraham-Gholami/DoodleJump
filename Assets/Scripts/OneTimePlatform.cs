using System.Collections;
using UnityEngine;

public class OneTimePlatform : Platform 
{
    [Header("Conversion Settings")]
    [SerializeField] private GameObject glowEffect; // Visual indicator when converted
    [SerializeField] private Animator animator;
    [SerializeField] private float destroyDelay = 0.2f;
    [SerializeField] private AudioClip oneTimeJumpClip;
    [SerializeField] private AudioClip convertedPlatformJumpSound;
    
    private bool IsConverted = false;
    private Coroutine ConversionRoutine;
    
    private void Start()
    {
        // Initialize glow effect
        if (glowEffect != null)
        {
            glowEffect.SetActive(false);
        }
        else
        {
            // Try to find as child
            Transform glowTransform = transform.Find("GlowEffect");
            if (glowTransform != null)
            {
                glowEffect = glowTransform.gameObject;
                glowEffect.SetActive(false);
            }
        }
    }
    
    public void Deactive()
    {
        // Don't deactivate if converted to normal platform
        if (IsConverted)
        {
            Debug.Log("OneTimePlatform: Converted to normal platform, not deactivating");
            return;
        }

        StartCoroutine(DestroyAfterDelay());
        /*
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<PlatformEffector2D>().enabled = false;
        */
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);

    }
    
    public void ConvertToNormalPlatform(float duration)
    {
        Debug.Log($"OneTimePlatform: Converting to normal platform for {duration} seconds");
        
        if (ConversionRoutine != null)
        {
            StopCoroutine(ConversionRoutine);
        }
        
        ConversionRoutine = StartCoroutine(ConversionCoroutine(duration));
    }
    
    private IEnumerator ConversionCoroutine(float duration)
    {
        IsConverted = true;

        audioSource.clip = convertedPlatformJumpSound;
        
        animator.SetBool("IsConverted", true);
        
        // Activate glow effect
        if (glowEffect != null)
        {
            glowEffect.SetActive(true);
            Debug.Log("OneTimePlatform: Glow effect activated");
        }
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        animator.SetBool("IsConverted", false);
        
        // Revert to one-time platform
        IsConverted = false;
        
        audioSource.clip = oneTimeJumpClip;
        
        // Deactivate glow effect
        if (glowEffect != null)
        {
            glowEffect.SetActive(false);
            Debug.Log("OneTimePlatform: Glow effect deactivated");
        }
        
        Debug.Log("OneTimePlatform: Reverted to one-time platform");
        ConversionRoutine = null;
    }
}