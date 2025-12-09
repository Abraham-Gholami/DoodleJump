using System.Collections;
using UnityEngine;

using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Type")]
    [SerializeField] private EnemyType enemyType = EnemyType.SickleCell;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deadClip;
    
    [Header("Proximity Audio")]
    [SerializeField] private AudioClip proximityClip;
    [SerializeField] private float proximityDistance = 5f;
    
    [Header("Collision Prevention")]
    [SerializeField] private float collisionCooldown = 1f;
    
    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    
    [Header("Death Animation Settings")]
    [SerializeField] private float deathAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve deathScaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private bool addRotationOnDeath = true;
    [SerializeField] private float deathRotationSpeed = 360f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    // Public properties
    public bool IsAlreadyDead => isAlreadyDead;
    public EnemyType EnemyType => enemyType;

    // Private state
    private bool isAlreadyDead = false;
    private bool hasCollidedWithPlayer = false;
    private bool hasPlayedProximitySound = false;
    private float lastCollisionTime = -999f;
    private Vector3 originalScale;
    private Transform playerTransform;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning($"Enemy {gameObject.name}: No player found with 'Player' tag!");
        }
    }

    private void Update()
    {
        if (isAlreadyDead || hasPlayedProximitySound || playerTransform == null || proximityClip == null) 
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer <= proximityDistance)
        {
            PlayProximitySound();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isAlreadyDead)
        {
            HandlePlayerCollision();
        }
    }

    private void HandlePlayerCollision()
    {
        // Prevent multiple collisions with same enemy
        if (hasCollidedWithPlayer)
        {
            if (enableDebugLogs)
                Debug.Log($"Enemy {gameObject.name}: Already collided with player, ignoring repeat collision");
            return;
        }

        // Check collision cooldown
        if (Time.time - lastCollisionTime < collisionCooldown)
        {
            if (enableDebugLogs)
                Debug.Log($"Enemy {gameObject.name}: Still in collision cooldown, ignoring");
            return;
        }

        // Mark collision to prevent repeats
        hasCollidedWithPlayer = true;
        lastCollisionTime = Time.time;

        if (enableDebugLogs)
            Debug.Log($"Enemy {gameObject.name}: First collision with player - triggering damage event");

        // Visual feedback
        StartCoroutine(CollisionFeedback());
    }

    private IEnumerator CollisionFeedback()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            
            yield return new WaitForSeconds(0.15f);
            
            if (spriteRenderer != null) // Check if still exists
                spriteRenderer.color = originalColor;
        }
    }

    private void PlayProximitySound()
    {
        if (hasPlayedProximitySound) return;
        
        hasPlayedProximitySound = true;
        
        if (audioSource != null && proximityClip != null)
        {
            audioSource.PlayOneShot(proximityClip);
            
            if (enableDebugLogs)
                Debug.Log($"Enemy {gameObject.name}: Played proximity sound");
        }
    }

    public void SetFlip(bool shouldFlip)
    {
        if (shouldFlip)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x); // Flip left
            transform.localScale = scale;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x); // Face right
            transform.localScale = scale;
        }
        
        originalScale = transform.localScale;
    }

    public void Dead()
    {
        if (isAlreadyDead) return;
        
        isAlreadyDead = true;
        
        // Mark boundary cleanup as killed to prevent missed events
        EnemyBoundaryCleanup boundaryCleanup = GetComponent<EnemyBoundaryCleanup>();
        if (boundaryCleanup != null)
        {
            boundaryCleanup.MarkAsKilled();
        }
        
        // Stop and play death audio
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            if (deadClip != null)
            {
                audioSource.PlayOneShot(deadClip);
            }
        }
        
        // Trigger death event
        EventManager.TriggerEvent<EventName, GameObject>(EventName.OnEnemyKilled, gameObject);
        
        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger("Dead");
        }
        else
        {
            StartCoroutine(PlayDeathAnimation());
        }
    }
    
    private IEnumerator PlayDeathAnimation()
    {
        float elapsed = 0f;
        Vector3 startScale = originalScale;
        
        while (elapsed < deathAnimationDuration)
        {
            float progress = elapsed / deathAnimationDuration;
            
            // Apply scale animation
            float scaleMultiplier = deathScaleCurve.Evaluate(progress);
            transform.localScale = startScale * scaleMultiplier;
            
            // Apply rotation animation
            if (addRotationOnDeath)
            {
                transform.Rotate(0, 0, deathRotationSpeed * Time.deltaTime);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final scale
        transform.localScale = Vector3.zero;
        
        // Complete death
        OnDeathAnimationComplete();
    }
    
    public void OnDeathAnimationComplete()
    {
        Destroy(gameObject);
    }
}

public enum EnemyType
{
    SickleCell,
    Fungi,
    Bacteria,
    Virus
}