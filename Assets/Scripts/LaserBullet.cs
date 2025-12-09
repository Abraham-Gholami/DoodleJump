// ============= LASER BULLET =============
using UnityEngine;

public class LaserBullet : MonoBehaviour
{
    [Header("Laser Settings")]
    [SerializeField] private float speed = 20f; // Faster speed for laser
    [SerializeField] private bool reflectOffWalls = true;
    [SerializeField] private float screenEdgeOffset = 0.5f;
    
    [Header("Visual Effects")]
    [SerializeField] private TrailRenderer laserTrail; // For laser tail effect
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private LineRenderer laserBeam; // Optional beam effect
    [SerializeField] private bool autoSetupTrail = true; // Auto-setup trail if missing
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip reflectSound;
    [SerializeField] private AudioClip enemyHitSound;
    
    [Header("Destruction")]
    [SerializeField] private float maxLifetime = 15f; // Auto-destroy after time
    
    private Vector2 moveDirection = Vector2.up;
    private Rigidbody2D rb;
    private float leftBoundary;
    private float rightBoundary;
    private float topBoundary;
    private Camera mainCamera;
    private int enemiesKilled = 0;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        
        CalculateBoundaries();
        SetupVisualEffects();
        
        // Set initial velocity
        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }
        
        // Auto-destroy after max lifetime
        Destroy(gameObject, maxLifetime);
        
        Debug.Log("LaserBullet: Created and moving");
    }
    
    private void CalculateBoundaries()
    {
        if (mainCamera != null)
        {
            Vector3 leftScreen = mainCamera.ScreenToWorldPoint(Vector3.zero);
            Vector3 rightScreen = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
            Vector3 topScreen = mainCamera.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
            
            leftBoundary = leftScreen.x + screenEdgeOffset;
            rightBoundary = rightScreen.x - screenEdgeOffset;
            topBoundary = topScreen.y + 5f; // Extra buffer for top
        }
    }
    
    private void SetupVisualEffects()
    {
        // Auto-setup trail renderer if missing
        if (laserTrail == null && autoSetupTrail)
        {
            laserTrail = GetComponent<TrailRenderer>();
            if (laserTrail == null)
            {
                laserTrail = gameObject.AddComponent<TrailRenderer>();
                Debug.Log("LaserBullet: Auto-added TrailRenderer component");
            }
        }
        
        // Setup trail renderer for laser tail
        if (laserTrail != null)
        {
            laserTrail.startColor = Color.cyan;
            laserTrail.endColor = new Color(0, 1, 1, 0); // Transparent cyan
            laserTrail.startWidth = 0.3f; // Wider for better visibility
            laserTrail.endWidth = 0.05f;
            laserTrail.time = 0.8f; // Longer trail
            laserTrail.material = new Material(Shader.Find("Sprites/Default"));
            laserTrail.material.color = Color.cyan;
            Debug.Log("LaserBullet: Trail renderer configured");
        }
        else
        {
            Debug.LogWarning("LaserBullet: No TrailRenderer found - laser won't have tail effect");
        }
        
        // Setup line renderer for beam effect
        if (laserBeam != null)
        {
            laserBeam.startColor = Color.cyan;
            laserBeam.endColor = Color.white;
            laserBeam.startWidth = 0.1f;
            laserBeam.endWidth = 0.1f;
            Debug.Log("LaserBullet: Line renderer configured");
        }
    }
    
    private void Update()
    {
        CheckBoundaryReflection();
        CheckIfOffScreen();
    }
    
    private void CheckBoundaryReflection()
    {
        if (!reflectOffWalls) return;
        
        Vector3 pos = transform.position;
        bool reflected = false;
        
        // Check left and right boundaries
        if ((pos.x <= leftBoundary && moveDirection.x < 0) || 
            (pos.x >= rightBoundary && moveDirection.x > 0))
        {
            // Reflect horizontally
            moveDirection.x = -moveDirection.x;
            reflected = true;
            
            // Clamp position to boundary
            pos.x = Mathf.Clamp(pos.x, leftBoundary, rightBoundary);
            transform.position = pos;
        }
        
        if (reflected)
        {
            // Update velocity
            if (rb != null)
            {
                rb.linearVelocity = moveDirection * speed;
            }
            
            PlayReflectEffect();
            Debug.Log($"LaserBullet: Reflected, new direction: {moveDirection}");
        }
    }
    
    private void CheckIfOffScreen()
    {
        // Destroy if laser goes too far off screen (especially upward)
        if (transform.position.y > topBoundary)
        {
            Debug.Log($"LaserBullet: Exited screen, killed {enemiesKilled} enemies");
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Kill enemy without destroying laser
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Dead();
                enemiesKilled++;
                Debug.Log($"LaserBullet: Killed enemy {other.name}, total kills: {enemiesKilled}");
            }
            else
            {
                Destroy(other.gameObject);
                enemiesKilled++;
            }
            
            PlayEnemyHitEffect(other.transform.position);
        }
        // Note: Laser does NOT get destroyed by enemies - it penetrates through
    }
    
    private void PlayReflectEffect()
    {
        // Play reflect sound
        if (audioSource != null && reflectSound != null)
        {
            audioSource.PlayOneShot(reflectSound);
        }
        
        // Play particle effect
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
    }
    
    private void PlayEnemyHitEffect(Vector3 hitPosition)
    {
        // Play enemy hit sound
        if (audioSource != null && enemyHitSound != null)
        {
            audioSource.PlayOneShot(enemyHitSound);
        }
        
        // Play hit particle effect
        if (hitEffect != null)
        {
            Instantiate(hitEffect, hitPosition, Quaternion.identity);
        }
    }
    
    // Public methods for external control
    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }
    }
    
    public int GetEnemiesKilled()
    {
        return enemiesKilled;
    }
}