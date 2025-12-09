using System.Collections;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private Transform bulletOrigin;
    
    [Header("Shooting Settings")]
    [SerializeField] private float shootingCooldown = 0.5f;
    [SerializeField] private float maxHorizontalFactor = 0.75f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("Platform Shooting Settings")]
    [SerializeField] private GameObject platformBulletPrefab; // New dedicated platform bullet
    [SerializeField] private float platformBulletSpeed = 10f;

    [Header("Shooting Control")]
    [SerializeField] private bool allowNormalShooting = true; // Original shooting system
    [SerializeField] private bool requiresPlatformShootPowerup = false; // Need powerup to shoot platforms

    [Header("Platform Shoot Power-up")]
    [SerializeField] private float platformShootDuration = 10f;
    private bool hasPlatformShootPower = false;
    private float platformShootTimer = 0f;
    private Coroutine platformShootCoroutine;
    
    private PlayerInputHandler inputHandler;
    private PlayerOxygenSystem oxygenSystem;
    private Camera mainCamera;
    
    private bool isShooting;
    private float lastShootTime;
    private bool canShoot = true;
    
// Events
    public System.Action<float> OnPlatformShootStarted;
    public System.Action OnPlatformShootEnded;
    public System.Action<float> OnPlatformShootTimeChanged;

// Public properties
    public bool HasPlatformShootPower => hasPlatformShootPower;
    public float RemainingPlatformShootTime => hasPlatformShootPower ? platformShootTimer : 0f;

    public bool IsShooting => isShooting;
    public bool CanShoot => GameController.CanShoot && canShoot && !isShooting && 
                            mainCamera != null && bulletOrigin != null && 
                            (allowNormalShooting || (requiresPlatformShootPowerup && hasPlatformShootPower));

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        oxygenSystem = GetComponent<PlayerOxygenSystem>();
        mainCamera = Camera.main;
        lastShootTime = -shootingCooldown;
        
        if (bulletOrigin == null)
            bulletOrigin = transform;
    }

    private void Start()
    {
        if (inputHandler != null)
        {
            inputHandler.OnShortTap += HandleShortTap;
            inputHandler.OnLongPress += HandleLongPress;
            inputHandler.OnLaserContinuous += HandleLaserContinuous;
        }
    }

    private void Update()
    {
        UpdateShootingCooldown();
    }

    private void HandleShortTap(Vector2 tapPosition)
    {
        TryShootBullet(tapPosition);
    }

    private void HandleLongPress(Vector2 tapPosition)
    {
        if (CanShootLaser() && CanShoot)
        {
            lastShootTime = Time.time;
            StartCoroutine(ShootWithAnimation(tapPosition, true));
            SetLaserModeActive(true);
            Debug.Log("PlayerShooting: Entered laser mode");
        }
        else
        {
            Debug.Log("PlayerShooting: Cannot enter laser mode - no oxygen power or on cooldown");
        }
    }
    
    private void HandleLaserContinuous(Vector2 tapPosition)
    {
        if (CanShootLaser() && CanShoot)
        {
            lastShootTime = Time.time;
            StartCoroutine(ShootWithAnimation(tapPosition, true));
        }
        else
        {
            SetLaserModeActive(false);
            Debug.Log("PlayerShooting: Exiting laser mode - lost oxygen power or on cooldown");
        }
    }

    private void TryShootBullet(Vector2 tapPosition)
    {
        if (!CanShoot) return;

        lastShootTime = Time.time;
        StartCoroutine(ShootWithAnimation(tapPosition, false));
    }

    private IEnumerator ShootWithAnimation(Vector3 tapPos, bool isLaser)
    {
        if (isShooting) yield break;
        
        isShooting = true;
        
        // Get animation component to handle sprite changes
        var animationComponent = GetComponent<PlayerAnimation>();
        if (animationComponent != null)
        {
            yield return animationComponent.PlayShootAnimation(() => {
                if (isLaser)
                    ShootLaser(tapPos);
                else
                    ShootBullet(tapPos);
            });
        }
        else
        {
            // Fallback - shoot immediately without animation
            if (isLaser)
                ShootLaser(tapPos);
            else
                ShootBullet(tapPos);
        }
        
        isShooting = false;
    }

    private void ShootLaser(Vector3 tapPos)
    {
        if (!CanShootLaser()) return;
        
        Vector3 shootDirection = CalculateShootDirection(tapPos);
        
        if (oxygenSystem.TryShootLaser(shootDirection))
        {
            PlayShootAudio();
            Debug.Log("PlayerShooting: Laser fired successfully");
        }
    }

    private void ShootBullet(Vector3 tapPos)
    {
        Vector3 shootDirection = CalculateShootDirection(tapPos);
    
        // Shoot platform bullet if power is active
        if (hasPlatformShootPower && platformBulletPrefab != null)
        {
            ShootPlatformBullet(shootDirection);
            return;
        }
    
        // Otherwise shoot normal bullet (if allowed)
        if (!allowNormalShooting && !hasPlatformShootPower)
        {
            Debug.Log("PlayerShooting: Normal shooting disabled, need platform shoot power");
            return;
        }
    
        GameObject bulletPrefab = BulletTypeManager.GetCurrentBulletPrefab();
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletOrigin.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        
            if (bulletRb != null)
            {
                bulletRb.linearVelocity = shootDirection * bulletSpeed;
            }
        
            PlayShootAudio();
        }
    }

    private void ShootPlatformBullet(Vector3 shootDirection)
    {
        GameObject platformBullet = Instantiate(platformBulletPrefab, bulletOrigin.position, Quaternion.identity);
        Rigidbody2D bulletRb = platformBullet.GetComponent<Rigidbody2D>();
    
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = shootDirection * platformBulletSpeed;
        }
    
        PlayShootAudio();
        Debug.Log("PlayerShooting: Platform bullet fired");
    }
    
    private Vector3 CalculateShootDirection(Vector3 tapPos)
    {
        if (mainCamera == null || bulletOrigin == null) return Vector3.up;
        
        Vector3 worldTap = mainCamera.ScreenToWorldPoint(tapPos);
        worldTap.z = 0f;

        Vector3 shootDir = (worldTap - bulletOrigin.position).normalized;
        shootDir.y = Mathf.Max(0.4f, shootDir.y);
        shootDir.x = Mathf.Clamp(shootDir.x, -maxHorizontalFactor, maxHorizontalFactor);
        
        return shootDir.normalized;
    }

    private void UpdateShootingCooldown()
    {
        canShoot = (Time.time - lastShootTime) >= shootingCooldown;
    }

    private void PlayShootAudio()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }

    private bool CanShootLaser()
    {
        return oxygenSystem != null && oxygenSystem.CanShootLaser();
    }

    private void SetLaserModeActive(bool active)
    {
        if (inputHandler == null) return;
        
        // Use the public method instead of reflection
        inputHandler.SetLaserModeActive(active);
    }

    private void OnDestroy()
    {
        if (inputHandler != null)
        {
            inputHandler.OnShortTap -= HandleShortTap;
            inputHandler.OnLongPress -= HandleLongPress;
            inputHandler.OnLaserContinuous -= HandleLaserContinuous;
        }
    }
    
    // Platform Shoot Power-up System
    public void ActivatePlatformShootPower(float duration = -1f)
    {
        if (duration < 0)
            duration = platformShootDuration;
    
        Debug.Log($"PlayerShooting: Activating platform shoot power for {duration} seconds");
    
        platformShootTimer = duration;
    
        if (hasPlatformShootPower)
        {
            Debug.Log($"PlayerShooting: Already has platform shoot power, resetting timer to {duration}");
            OnPlatformShootTimeChanged?.Invoke(platformShootTimer);
            return;
        }
    
        hasPlatformShootPower = true;
    
        if (platformShootCoroutine != null)
        {
            StopCoroutine(platformShootCoroutine);
        }
        platformShootCoroutine = StartCoroutine(PlatformShootCountdown());
    
        OnPlatformShootStarted?.Invoke(duration);
    }

    private IEnumerator PlatformShootCountdown()
    {
        while (platformShootTimer > 0f)
        {
            platformShootTimer -= Time.deltaTime;
            OnPlatformShootTimeChanged?.Invoke(platformShootTimer);
            yield return null;
        }
    
        EndPlatformShootPower();
    }

    private void EndPlatformShootPower()
    {
        Debug.Log("PlayerShooting: Platform shoot power ended");
    
        hasPlatformShootPower = false;
        platformShootTimer = 0f;
        platformShootCoroutine = null;
    
        OnPlatformShootEnded?.Invoke();
    }

    public void ForceEndPlatformShootPower()
    {
        if (hasPlatformShootPower)
        {
            if (platformShootCoroutine != null)
            {
                StopCoroutine(platformShootCoroutine);
            }
            EndPlatformShootPower();
        }
    }

    public void SetAllowNormalShooting(bool allow)
    {
        allowNormalShooting = allow;
        Debug.Log($"PlayerShooting: Normal shooting {(allow ? "enabled" : "disabled")}");
    }

    public void SetRequiresPlatformShootPowerup(bool requires)
    {
        requiresPlatformShootPowerup = requires;
        Debug.Log($"PlayerShooting: Platform shoot powerup requirement {(requires ? "enabled" : "disabled")}");
    }

    // Public API
    public float GetRemainingCooldown() => canShoot ? 0f : shootingCooldown - (Time.time - lastShootTime);
    public bool HasOxygenPower() => oxygenSystem?.HasOxygenPower ?? false;
    public float GetRemainingOxygenTime() => oxygenSystem?.RemainingOxygenTime ?? 0f;
}