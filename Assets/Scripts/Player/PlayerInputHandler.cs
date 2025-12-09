using UnityEngine;

/// <summary>
/// Handles all player input and delegates to appropriate components
/// Compatible with modular PlayerController structure
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [Header("Tap Settings")]
    [SerializeField] private float maxTapDuration = 0.2f;
    [SerializeField] private float longPressDuration = 0.5f;
    [SerializeField] private float laserFireRate = 0.3f;
    
    [Header("Input Sensitivity")]
    [SerializeField] private float tiltDeadZone = 0.1f;
    
    [Header("References")]
    [SerializeField] private Joystick joystick;
    
    // Input tracking variables
    private bool isTouchDown;
    private float touchStartTime;
    private Vector2 touchStartPosition;
    private float lastLaserFireTime;
    private bool isInLaserMode = false;
    private bool hasFiredNormalBullet = false;
    
    // Component references for validation
    private PlayerShooting playerShooting;
    private PlayerOxygenSystem oxygenSystem;
    
    // Events for component communication
    public System.Action<Vector2> OnShortTap;
    public System.Action<Vector2> OnLongPress;
    public System.Action<Vector2> OnLaserContinuous;
    public System.Action<float> OnMovementInput;
    
    private void Awake()
    {
        // Get component references for validation
        playerShooting = GetComponent<PlayerShooting>();
        oxygenSystem = GetComponent<PlayerOxygenSystem>();
        
        if (playerShooting == null)
        {
            Debug.LogError("PlayerInputHandler: PlayerShooting component not found!");
        }
    }
    
    private void Start()
    {
        ValidateConfiguration();
    }
    
    private void Update()
    {
        HandleMovementInput();
        HandleTapInput();
    }
    
    private void ValidateConfiguration()
    {
        // Check if SettingsDataHolder is ready
        if (SettingsDataHolder.Instance == null)
        {
            Debug.LogWarning("PlayerInputHandler: SettingsDataHolder not ready yet");
        }
        else
        {
            Debug.Log($"PlayerInputHandler: Using control type {SettingsDataHolder.ControlType}");
        }

        if (SettingsDataHolder.ControlType == 1 && joystick == null)
        {
            Debug.LogWarning("PlayerInputHandler: Control type is set to joystick but no joystick reference assigned");
        }
        
        if (oxygenSystem == null)
        {
            Debug.LogWarning("PlayerInputHandler: PlayerOxygenSystem not found - laser mode won't work");
        }
    }
    
    private void HandleMovementInput()
    {
        float movementInput = GetMovementInputForCurrentControlType();
        OnMovementInput?.Invoke(movementInput);
    }
    
    private float GetMovementInputForCurrentControlType()
    {
        // Ensure SettingsDataHolder is properly initialized
        if (SettingsDataHolder.Instance == null)
        {
            return GetJoystickInput();
        }

        int controlType = SettingsDataHolder.ControlType;
        
        switch (controlType)
        {
            case 0: // Tilt controls
                return GetTiltInput();
                
            case 1: // Joystick/keyboard controls
                return GetJoystickInput();
                
            default:
                Debug.LogWarning($"PlayerInputHandler: Unknown control type {controlType}, using tilt");
                return GetTiltInput();
        }
    }
    
    private float GetTiltInput()
    {
        float rawTilt = Mathf.Clamp(Input.acceleration.x, -1f, 1f);
        
        // Apply dead zone to prevent jittering
        if (Mathf.Abs(rawTilt) < tiltDeadZone)
            rawTilt = 0f;
            
        return rawTilt;
    }
    
    private float GetJoystickInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        
        // Use joystick if keyboard input is zero and joystick is available
        if (horizontal == 0f && joystick != null)
        {
            horizontal = joystick.Horizontal;
        }
        
        return horizontal;
    }
    
    private void HandleTapInput()
    {
        HandleTouchStart();
        HandleTouchHold();
        HandleTouchEnd();
        HandleTouchTimeout();
    }
    
    private void HandleTouchStart()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isTouchDown = true;
            touchStartTime = Time.time;
            touchStartPosition = Input.mousePosition;
            
            // Reset state for new touch
            ResetTouchState();
        }
    }
    
    private void ResetTouchState()
    {
        isInLaserMode = false;
        hasFiredNormalBullet = false;
        lastLaserFireTime = 0f;
    }
    
    private void HandleTouchHold()
    {
        if (!isTouchDown) return;
        
        Vector2 currentTouchPosition = Input.mousePosition;
        
        // Skip processing if touch is on joystick
        if (IsInputOnJoystick(currentTouchPosition))
            return;
        
        float touchDuration = Time.time - touchStartTime;
        
        // Try to enter laser mode after long press duration
        TryEnterLaserMode(touchDuration, currentTouchPosition);
        
        // Handle continuous laser firing
        HandleContinuousLaserFiring(currentTouchPosition);
    }
    
    private void TryEnterLaserMode(float touchDuration, Vector2 touchPosition)
    {
        if (touchDuration >= longPressDuration && !isInLaserMode && !hasFiredNormalBullet)
        {
            // Check if oxygen power is available before attempting
            if (CanEnterLaserMode())
            {
                OnLongPress?.Invoke(touchPosition);
                // isInLaserMode will be set by PlayerShooting if successful
            }
            else
            {
                hasFiredNormalBullet = true; // Prevent further attempts this touch
            }
        }
    }
    
    private void HandleContinuousLaserFiring(Vector2 touchPosition)
    {
        if (isInLaserMode && (Time.time - lastLaserFireTime) >= laserFireRate)
        {
            OnLaserContinuous?.Invoke(touchPosition);
            lastLaserFireTime = Time.time;
        }
    }
    
    private void HandleTouchEnd()
    {
        if (Input.GetMouseButtonUp(0) && isTouchDown)
        {
            Vector2 touchEndPosition = Input.mousePosition;
            
            // Skip processing if touch was on joystick
            if (IsInputOnJoystick(touchStartPosition))
            {
                CleanupTouch();
                return;
            }
            
            float touchDuration = Time.time - touchStartTime;
            
            // Fire normal bullet only for short taps that didn't enter laser mode
            TryFireNormalBullet(touchDuration, touchStartPosition);
            
            CleanupTouch();
        }
    }
    
    private void TryFireNormalBullet(float touchDuration, Vector2 touchPosition)
    {
        bool isShortTap = touchDuration <= maxTapDuration;
        bool didntEnterLaserMode = !isInLaserMode;
        bool didntFireBulletYet = !hasFiredNormalBullet;
        
        if (isShortTap && didntEnterLaserMode && didntFireBulletYet)
        {
            OnShortTap?.Invoke(touchPosition);
            hasFiredNormalBullet = true;
        }
    }
    
    private void CleanupTouch()
    {
        isTouchDown = false;
        isInLaserMode = false;
        hasFiredNormalBullet = false;
    }
    
    private void HandleTouchTimeout()
    {
        // Reset if touch is held too long without being useful (10 seconds timeout)
        if (isTouchDown && (Time.time - touchStartTime) > longPressDuration + 10f)
        {
            CleanupTouch();
        }
    }
    
    private bool IsInputOnJoystick(Vector2 tapPosition)
    {
        // Check if we should use joystick detection
        if (joystick == null) return false;
        
        // Ensure SettingsDataHolder is ready
        if (SettingsDataHolder.Instance == null) return false;
        
        // Only check joystick area if control type is joystick
        if (SettingsDataHolder.ControlType != 1) return false;
        
        RectTransform joyRect = joystick.GetComponent<RectTransform>();
        if (joyRect == null) return false;
        
        Vector2 screenPos;
        bool isInRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joyRect.parent as RectTransform, tapPosition, null, out screenPos);
        
        return isInRect && joyRect.rect.Contains(screenPos);
    }
    
    private bool CanEnterLaserMode()
    {
        // Check if oxygen system is available and has power
        return oxygenSystem != null && oxygenSystem.HasOxygenPower;
    }
    
    // Public API methods for external control and configuration
    public void SetMaxTapDuration(float duration)
    {
        maxTapDuration = Mathf.Max(0.05f, duration); // Minimum 50ms
        Debug.Log($"PlayerInputHandler: Max tap duration set to {maxTapDuration}");
    }
    
    public void SetLongPressDuration(float duration)
    {
        longPressDuration = Mathf.Max(0.1f, duration); // Minimum 100ms
        Debug.Log($"PlayerInputHandler: Long press duration set to {longPressDuration}");
    }
    
    public void SetLaserFireRate(float rate)
    {
        laserFireRate = Mathf.Max(0.05f, rate); // Minimum 50ms between shots
        Debug.Log($"PlayerInputHandler: Laser fire rate set to {laserFireRate}");
    }
    
    public void SetTiltDeadZone(float deadZone)
    {
        tiltDeadZone = Mathf.Clamp01(deadZone);
        Debug.Log($"PlayerInputHandler: Tilt dead zone set to {tiltDeadZone}");
    }
    
    // Public status methods
    public float GetCurrentTouchDuration()
    {
        return isTouchDown ? (Time.time - touchStartTime) : 0f;
    }
    
    public bool IsTouchDown()
    {
        return isTouchDown;
    }
    
    public bool IsInLaserMode()
    {
        return isInLaserMode;
    }
    
    public Vector2 GetTouchStartPosition()
    {
        return touchStartPosition;
    }
    
    public int GetCurrentControlType()
    {
        return SettingsDataHolder.ControlType;
    }
    
    // Method for PlayerShooting to set laser mode state
    public void SetLaserModeActive(bool active)
    {
        bool wasInLaserMode = isInLaserMode;
        isInLaserMode = active;
        
        if (wasInLaserMode != isInLaserMode)
        {
            Debug.Log($"PlayerInputHandler: Laser mode {(active ? "activated" : "deactivated")}");
        }
    }
    
    // Validation in editor
    private void OnValidate()
    {
        if (maxTapDuration <= 0f)
        {
            Debug.LogWarning("PlayerInputHandler: Max tap duration should be greater than 0");
        }
        
        if (longPressDuration <= maxTapDuration)
        {
            Debug.LogWarning("PlayerInputHandler: Long press duration should be greater than max tap duration");
        }
        
        if (laserFireRate <= 0f)
        {
            Debug.LogWarning("PlayerInputHandler: Laser fire rate should be greater than 0");
        }
        
        if (tiltDeadZone < 0f || tiltDeadZone > 1f)
        {
            Debug.LogWarning("PlayerInputHandler: Tilt dead zone should be between 0 and 1");
        }
    }
}