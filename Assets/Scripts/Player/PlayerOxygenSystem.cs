using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player's oxygen power system for laser shooting
/// Compatible with modular PlayerController structure
/// </summary>
public class PlayerOxygenSystem : MonoBehaviour
{
    [Header("Oxygen Settings")]
    [SerializeField] private float oxygenDuration = 10f;
    [SerializeField] private GameObject oxygenIndicator; // Visual indicator
    
    [Header("Laser Settings")]
    [SerializeField] private GameObject laserBulletPrefab;
    [SerializeField] private float laserSpeed = 15f;
    [SerializeField] private Transform bulletOrigin;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip oxygenStartSound;
    [SerializeField] private AudioClip oxygenEndSound;
    [SerializeField] private AudioClip laserShootSound;
    
    private bool hasOxygenPower = false;
    private float oxygenTimer = 0f;
    private Coroutine oxygenCoroutine;
    
    // Component references
    private PlayerShooting playerShooting;
    
    // Public properties
    public bool HasOxygenPower => hasOxygenPower;
    public float RemainingOxygenTime => hasOxygenPower ? oxygenTimer : 0f;
    public float OxygenProgress => hasOxygenPower ? (oxygenTimer / oxygenDuration) : 0f;
    
    // Events
    public System.Action<float> OnOxygenStarted;
    public System.Action OnOxygenEnded;
    public System.Action<float> OnOxygenTimeChanged;
    
    private void Awake()
    {
        // Get component references
        playerShooting = GetComponent<PlayerShooting>();
        
        // Validate critical components
        if (playerShooting == null)
        {
            Debug.LogError("PlayerOxygenSystem: PlayerShooting component not found! Laser functionality won't work properly.");
        }
    }
    
    private void Start()
    {
        InitializeOxygenIndicator();
        InitializeBulletOrigin();
        ValidateConfiguration();
    }
    
    private void InitializeOxygenIndicator()
    {
        if (oxygenIndicator != null)
        {
            oxygenIndicator.SetActive(false);
            Debug.Log("PlayerOxygenSystem: Oxygen indicator initialized and deactivated");
        }
        else
        {
            // Try to find oxygen indicator as child
            Transform oxygenTransform = transform.Find("OxygenIndicator");
            if (oxygenTransform != null)
            {
                oxygenIndicator = oxygenTransform.gameObject;
                oxygenIndicator.SetActive(false);
                Debug.Log("PlayerOxygenSystem: Found and assigned oxygen indicator from child");
            }
            else
            {
                Debug.LogWarning("PlayerOxygenSystem: No oxygen indicator assigned or found as child 'OxygenIndicator'");
            }
        }
    }
    
    private void InitializeBulletOrigin()
    {
        if (bulletOrigin == null)
        {
            // Try to get from PlayerShooting component first
            if (playerShooting != null)
            {
                // Use reflection to access bulletOrigin from PlayerShooting
                var field = playerShooting.GetType().GetField("bulletOrigin", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                if (field != null)
                {
                    bulletOrigin = field.GetValue(playerShooting) as Transform;
                    Debug.Log("PlayerOxygenSystem: Bullet origin retrieved from PlayerShooting component");
                }
            }
            
            // Fallback: use player transform
            if (bulletOrigin == null)
            {
                bulletOrigin = transform;
                Debug.LogWarning("PlayerOxygenSystem: Using player transform as bullet origin fallback");
            }
        }
    }
    
    private void ValidateConfiguration()
    {
        if (laserBulletPrefab == null)
        {
            Debug.LogError("PlayerOxygenSystem: laserBulletPrefab not assigned! Laser shooting won't work.");
            laserBulletPrefab = BulletTypeManager.GetCurrentBulletPrefab();
        }
        
        if (bulletOrigin == null)
        {
            Debug.LogError("PlayerOxygenSystem: bulletOrigin not found! Laser shooting won't work.");
        }
        
        if (audioSource == null)
        {
            Debug.LogWarning("PlayerOxygenSystem: AudioSource not assigned, oxygen sounds will be silent");
        }
    }
    
    public void ActivateOxygenPower(float duration = -1f)
    {
        // Use default duration if none provided
        if (duration < 0)
            duration = oxygenDuration;
        
        Debug.Log($"PlayerOxygenSystem: Activating oxygen power for {duration} seconds");
        
        // Reset oxygen duration (don't extend, just reset)
        oxygenTimer = duration;
        
        // If already has oxygen, just reset the timer
        if (hasOxygenPower)
        {
            Debug.Log($"PlayerOxygenSystem: Already has oxygen, resetting timer to {duration}");
            OnOxygenTimeChanged?.Invoke(oxygenTimer);
            return;
        }
        
        // Start oxygen power
        hasOxygenPower = true;
        
        // Show visual indicator
        ActivateOxygenIndicator();
        
        // Play start sound
        PlayOxygenStartSound();
        
        // Start countdown
        StartOxygenCountdown();
        
        // Trigger event
        OnOxygenStarted?.Invoke(duration);
    }
    
    private void ActivateOxygenIndicator()
    {
        if (oxygenIndicator != null)
        {
            oxygenIndicator.SetActive(true);
            Debug.Log("PlayerOxygenSystem: Oxygen indicator activated");
        }
        else
        {
            Debug.LogWarning("PlayerOxygenSystem: No oxygen indicator to activate");
        }
    }
    
    private void StartOxygenCountdown()
    {
        if (oxygenCoroutine != null)
        {
            StopCoroutine(oxygenCoroutine);
        }
        oxygenCoroutine = StartCoroutine(OxygenCountdown());
    }
    
    public bool TryShootLaser(Vector3 direction)
    {
        Debug.Log($"PlayerOxygenSystem: TryShootLaser called - HasOxygenPower: {hasOxygenPower}, Direction: {direction}");
        
        if (!ValidateLaserShot())
        {
            return false;
        }
        
        return CreateAndConfigureLaser(direction);
    }
    
    private bool ValidateLaserShot()
    {
        if (!hasOxygenPower)
        {
            Debug.LogWarning("PlayerOxygenSystem: No oxygen power available for laser!");
            return false;
        }
        
        if (laserBulletPrefab == null)
        {
            Debug.LogError("PlayerOxygenSystem: laserBulletPrefab is NULL! Please assign it in the inspector.");
            laserBulletPrefab = BulletTypeManager.GetCurrentBulletPrefab();
            
            if(laserBulletPrefab == null)
                return false;
        }
        
        if (bulletOrigin == null)
        {
            Debug.LogError("PlayerOxygenSystem: bulletOrigin is NULL! Please assign it in the inspector.");
            return false;
        }
        
        return true;
    }
    
    private bool CreateAndConfigureLaser(Vector3 direction)
    {
        Debug.Log($"PlayerOxygenSystem: Creating laser at {bulletOrigin.position}");

        if (laserBulletPrefab == null)
        {
            laserBulletPrefab = BulletTypeManager.GetCurrentBulletPrefab();
        }
        
        // Create laser bullet
        GameObject laser = Instantiate(laserBulletPrefab, bulletOrigin.position, Quaternion.identity);
        
        if (laser == null)
        {
            Debug.LogError("PlayerOxygenSystem: Failed to instantiate laser!");
            return false;
        }
        
        Debug.Log($"PlayerOxygenSystem: Laser instantiated: {laser.name}");
        
        // Configure laser movement
        if (!ConfigureLaserMovement(laser, direction))
        {
            Debug.LogError("PlayerOxygenSystem: Failed to configure laser movement!");
            Destroy(laser);
            return false;
        }
        
        // Play laser sound
        PlayLaserShootSound();
        
        Debug.Log("PlayerOxygenSystem: Laser shot successfully!");
        return true;
    }
    
    private bool ConfigureLaserMovement(GameObject laser, Vector3 direction)
    {
        // Try LaserBullet component first
        LaserBullet laserComponent = laser.GetComponent<LaserBullet>();
        if (laserComponent != null)
        {
            laserComponent.SetDirection(direction);
            laserComponent.SetSpeed(laserSpeed);
            Debug.Log($"PlayerOxygenSystem: Laser configured with LaserBullet component - Direction: {direction}, Speed: {laserSpeed}");
            return true;
        }
        
        // Fallback: use Rigidbody2D
        Rigidbody2D laserRb = laser.GetComponent<Rigidbody2D>();
        if (laserRb != null)
        {
            laserRb.linearVelocity = direction * laserSpeed;
            Debug.Log($"PlayerOxygenSystem: Laser configured with Rigidbody2D - Velocity: {direction * laserSpeed}");
            return true;
        }
        
        Debug.LogError("PlayerOxygenSystem: Laser has neither LaserBullet component nor Rigidbody2D!");
        return false;
    }
    
    private IEnumerator OxygenCountdown()
    {
        while (oxygenTimer > 0f)
        {
            oxygenTimer -= Time.deltaTime;
            OnOxygenTimeChanged?.Invoke(oxygenTimer);
            yield return null;
        }
        
        // End oxygen power
        EndOxygenPower();
    }
    
    private void EndOxygenPower()
    {
        Debug.Log("PlayerOxygenSystem: Oxygen power ended");
        
        hasOxygenPower = false;
        oxygenTimer = 0f;
        
        // Hide visual indicator
        DeactivateOxygenIndicator();
        
        // Play end sound
        PlayOxygenEndSound();
        
        // Clean up coroutine reference
        oxygenCoroutine = null;
        
        // Trigger event
        OnOxygenEnded?.Invoke();
    }
    
    private void DeactivateOxygenIndicator()
    {
        if (oxygenIndicator != null)
        {
            oxygenIndicator.SetActive(false);
            Debug.Log("PlayerOxygenSystem: Oxygen indicator deactivated");
        }
    }
    
    private void PlayOxygenStartSound()
    {
        if (audioSource != null && oxygenStartSound != null)
        {
            audioSource.PlayOneShot(oxygenStartSound);
        }
    }
    
    private void PlayOxygenEndSound()
    {
        if (audioSource != null && oxygenEndSound != null)
        {
            audioSource.PlayOneShot(oxygenEndSound);
        }
    }
    
    private void PlayLaserShootSound()
    {
        if (audioSource != null && laserShootSound != null)
        {
            audioSource.PlayOneShot(laserShootSound);
        }
    }
    
    // Public methods for external control
    public void ForceEndOxygenPower()
    {
        if (hasOxygenPower)
        {
            if (oxygenCoroutine != null)
            {
                StopCoroutine(oxygenCoroutine);
            }
            EndOxygenPower();
        }
    }
    
    public void ExtendOxygenTime(float additionalTime)
    {
        if (hasOxygenPower)
        {
            oxygenTimer += additionalTime;
            oxygenTimer = Mathf.Min(oxygenTimer, oxygenDuration * 2f); // Cap at 2x duration
            OnOxygenTimeChanged?.Invoke(oxygenTimer);
            Debug.Log($"PlayerOxygenSystem: Oxygen time extended by {additionalTime} seconds");
        }
    }
    
    public void SetOxygenDuration(float newDuration)
    {
        oxygenDuration = Mathf.Max(1f, newDuration); // Minimum 1 second
        Debug.Log($"PlayerOxygenSystem: Oxygen duration set to {oxygenDuration} seconds");
    }
    
    // Compatibility method for modular PlayerShooting
    public bool CanShootLaser()
    {
        return hasOxygenPower && laserBulletPrefab != null && bulletOrigin != null;
    }
    
    private void OnDestroy()
    {
        if (oxygenCoroutine != null)
        {
            StopCoroutine(oxygenCoroutine);
        }
        
        Debug.Log("PlayerOxygenSystem: Component destroyed and cleaned up");
    }
    
    private void OnValidate()
    {
        // Editor validation
        if (oxygenDuration <= 0f)
        {
            Debug.LogWarning("PlayerOxygenSystem: Oxygen duration should be greater than 0");
        }
        
        if (laserSpeed <= 0f)
        {
            Debug.LogWarning("PlayerOxygenSystem: Laser speed should be greater than 0");
        }
        
        if (laserBulletPrefab == null)
        {
            Debug.LogWarning("PlayerOxygenSystem: Laser bullet prefab not assigned");
        }
    }
}