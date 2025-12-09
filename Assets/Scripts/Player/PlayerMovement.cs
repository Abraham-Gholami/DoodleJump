using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float draggingMovementSpeed = 10f;
    [SerializeField] private float tiltSmoothing = 5f;
    
    [Header("Boundary Settings")]
    [SerializeField] private float boundaryOffset = 0.5f;

    private Rigidbody2D rb;
    private Camera mainCamera;
    private PlayerInputHandler inputHandler;
    
    private float movementInput;
    private float smoothedTilt;
    private Vector3 originalScale;
    private float leftBoundary;
    private float rightBoundary;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        inputHandler = GetComponent<PlayerInputHandler>();
        originalScale = transform.localScale;
        
        CalculateBoundaries();
    }

    private void Start()
    {
        if (inputHandler != null)
        {
            inputHandler.OnMovementInput += HandleMovementInput;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
        ClampPlayerPosition();
        UpdateDirection();
    }

    private void HandleMovementInput(float input)
    {
        if (SettingsDataHolder.ControlType == 0)
        {
            // Tilt controls
            smoothedTilt = Mathf.Lerp(smoothedTilt, input, Time.deltaTime * tiltSmoothing);
            movementInput = smoothedTilt * movementSpeed;
        }
        else
        {
            // Direct controls
            movementInput = input * draggingMovementSpeed;
        }
    }

    private void MovePlayer()
    {
        if (rb == null) return;
        
        Vector2 velocity = rb.linearVelocity;
        velocity.x = movementInput;
        rb.linearVelocity = velocity;
    }

    private void UpdateDirection()
    {
        if (Mathf.Abs(movementInput) > 0.1f)
        {
            if (movementInput > 0)
                transform.localScale = originalScale;
            else if (movementInput < 0)
                transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
        }
    }

    private void ClampPlayerPosition()
    {
        if (mainCamera == null) return;

        CalculateBoundaries();
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, leftBoundary, rightBoundary);
        transform.position = pos;
    }

    private void CalculateBoundaries()
    {
        if (mainCamera != null)
        {
            Vector3 leftPoint = mainCamera.ScreenToWorldPoint(Vector3.zero);
            Vector3 rightPoint = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
            
            leftBoundary = leftPoint.x + boundaryOffset;
            rightBoundary = rightPoint.x - boundaryOffset;
        }
    }

    private void OnDestroy()
    {
        if (inputHandler != null)
        {
            inputHandler.OnMovementInput -= HandleMovementInput;
        }
    }

    // Public API
    public float GetCurrentMovementInput() => movementInput;
    public bool IsMoving() => Mathf.Abs(movementInput) > 0.1f;
}