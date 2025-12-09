using UnityEngine;

/// <summary>
/// Main PlayerController that coordinates between all player components
/// Uses explicit dependency validation instead of auto-adding components
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerShooting playerShooting;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private PlayerImmunity playerImmunity;
    
    [Header("Optional Components")]
    [SerializeField] private PlayerOxygenSystem oxygenSystem;
    [SerializeField] private PlayerSpringSystem springSystem;
    [SerializeField] private PlayerHelicopterSystem helicopterSystem;
    
    [Header("References")]
    [SerializeField] private GameController gameController;

    // Public properties for status queries
    public bool CanShoot => playerShooting?.CanShoot ?? false;
    public bool IsImmune => playerCombat?.IsPlayerImmune() ?? false;
    public bool HasOxygenPower => oxygenSystem?.HasOxygenPower ?? false;
    public bool HasSpringPower => springSystem?.HasSpringPower ?? false;
    public bool IsFlying => helicopterSystem?.IsFlying ?? false;
    public bool IsMoving => playerMovement?.IsMoving() ?? false;
    public Vector3 Position => transform.position;

    private void Awake()
    {
        Debug.Log("PlayerController: Initializing...");
        AutoFindComponents();
        //ValidateRequiredComponents();
    }
    
    private void Start()
    {
        if (IsConfigurationValid())
        {
            Debug.Log($"PlayerController: Successfully initialized with control type {SettingsDataHolder.ControlType}");
        }
    }

    private void AutoFindComponents()
    {
        // Only auto-find if not manually assigned
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (playerShooting == null) playerShooting = GetComponent<PlayerShooting>();
        if (playerAnimation == null) playerAnimation = GetComponent<PlayerAnimation>();
        if (playerCombat == null) playerCombat = GetComponent<PlayerCombat>();
        if (inputHandler == null) inputHandler = GetComponent<PlayerInputHandler>();
        if (playerImmunity == null) playerImmunity = GetComponent<PlayerImmunity>();
        if (oxygenSystem == null) oxygenSystem = GetComponent<PlayerOxygenSystem>();
        if (springSystem == null) springSystem = GetComponent<PlayerSpringSystem>();
        if (helicopterSystem == null) helicopterSystem = GetComponent<PlayerHelicopterSystem>();
    }

    private void ValidateRequiredComponents()
    {
        bool hasErrors = false;

        if (playerMovement == null)
        {
            Debug.LogError("PlayerController: PlayerMovement component is required!");
            hasErrors = true;
        }

        if (playerShooting == null)
        {
            Debug.LogError("PlayerController: PlayerShooting component is required!");
            hasErrors = true;
        }

        if (playerCombat == null)
        {
            Debug.LogError("PlayerController: PlayerCombat component is required!");
            hasErrors = true;
        }

        if (inputHandler == null)
        {
            Debug.LogError("PlayerController: PlayerInputHandler component is required!");
            hasErrors = true;
        }

        if (playerImmunity == null)
        {
            Debug.LogError("PlayerController: PlayerImmunity component is required!");
            hasErrors = true;
        }

        if (hasErrors)
        {
            Debug.LogError("PlayerController: Missing required components! Player may not function correctly.");
        }

        // Optional component warnings
        if (oxygenSystem == null)
        {
            Debug.LogWarning("PlayerController: PlayerOxygenSystem not found - laser functionality unavailable");
        }

        if (springSystem == null)
        {
            Debug.LogWarning("PlayerController: PlayerSpringSystem not found - spring boost functionality unavailable");
        }

        if (helicopterSystem == null)
        {
            Debug.LogWarning("PlayerController: PlayerHelicopterSystem not found - flight functionality unavailable");
        }
    }

    private bool IsConfigurationValid()
    {
        return playerMovement != null && playerShooting != null && 
               playerCombat != null && inputHandler != null && playerImmunity != null;
    }

    // Delegated API methods with null safety
    public float GetRemainingShootCooldown() => playerShooting?.GetRemainingCooldown() ?? 0f;
    public float GetRemainingImmunityTime() => playerCombat?.GetRemainingImmunityTime() ?? 0f;
    public float GetRemainingOxygenTime() => oxygenSystem?.RemainingOxygenTime ?? 0f;
    public float GetRemainingSpringTime() => springSystem?.RemainingSpringTime ?? 0f;
    public float GetRemainingHelicopterTime() => helicopterSystem?.RemainingHelicopterTime ?? 0f;
    public float GetCurrentMovementInput() => playerMovement?.GetCurrentMovementInput() ?? 0f;
    public bool IsPlayingDamageAnimation() => playerCombat?.IsPlayingDamageAnimation() ?? false;
    public bool IsPlayerActive() => gameObject.activeInHierarchy && IsConfigurationValid();

    // Component access for external systems that need direct access
    public PlayerMovement Movement => playerMovement;
    public PlayerShooting Shooting => playerShooting;
    public PlayerCombat Combat => playerCombat;
    public PlayerOxygenSystem OxygenSystem => oxygenSystem;
    public PlayerSpringSystem SpringSystem => springSystem;
    public PlayerHelicopterSystem HelicopterSystem => helicopterSystem;
    public PlayerImmunity Immunity => playerImmunity;

    private void OnValidate()
    {
        if (gameController == null)
        {
            gameController = FindFirstObjectByType<GameController>();
        }
        
        // Auto-find components in editor for convenience
        AutoFindComponents();
    }
}