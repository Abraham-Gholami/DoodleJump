using UnityEngine;

public class FluidPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int whiteCellsToAdd = 1;
    [SerializeField] private AudioClip pickupSound;
    
    [Header("Visual Effects")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private Vector3 startPosition;
    private AudioSource audioSource;
    private bool hasBeenPickedUp = false;
    
    private void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        
        ValidateSetup();
    }
    
    private void ValidateSetup()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"FluidPowerup: {gameObject.name} is missing Collider2D component!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogError($"FluidPowerup: {gameObject.name} Collider2D must be set to IsTrigger = true!");
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"FluidPowerup: {gameObject.name} setup complete at position {transform.position}");
        }
    }
    
    private void Update()
    {
        if (!hasBeenPickedUp)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenPickedUp) return;
        
        if (enableDebugLogs)
            Debug.Log($"FluidPowerup: Trigger entered by {other.gameObject.name} with tag '{other.tag}'");
        
        if (other.CompareTag("Player"))
        {
            if (enableDebugLogs)
                Debug.Log("FluidPowerup: Player detected, attempting pickup...");
            
            WhiteCellManager whiteCellManager = FindObjectOfType<WhiteCellManager>();
            if (whiteCellManager != null)
            {
                hasBeenPickedUp = true;
                
                int currentCells = whiteCellManager.CurrentWhiteCells;
                int maxCells = whiteCellManager.MaxWhiteCells;
                
                if (currentCells < maxCells)
                {
                    int newAmount = Mathf.Min(currentCells + whiteCellsToAdd, maxCells);
                    whiteCellManager.SetWhiteCells(newAmount);
                    
                    if (enableDebugLogs)
                        Debug.Log($"FluidPowerup: Added {newAmount - currentCells} white cells. Total: {newAmount}/{maxCells}");
                    
                    PlayPickupSound();
                }
                else
                {
                    if (enableDebugLogs)
                        Debug.Log($"FluidPowerup: White cells already at maximum ({maxCells})");
                }
                
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("FluidPowerup: WhiteCellManager not found in scene!");
            }
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log($"FluidPowerup: {other.gameObject.name} is not tagged as Player (tag: '{other.tag}')");
        }
    }
    
    private void PlayPickupSound()
    {
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
            if (enableDebugLogs)
                Debug.Log("FluidPowerup: Pickup sound played");
        }
        else if (enableDebugLogs)
        {
            Debug.Log("FluidPowerup: No pickup sound configured");
        }
    }
    
    public void SetWhiteCellsToAdd(int amount)
    {
        whiteCellsToAdd = amount;
        if (enableDebugLogs)
            Debug.Log($"FluidPowerup: White cells to add set to {amount}");
    }
    
    [ContextMenu("Test Pickup")]
    public void TestPickup()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            OnTriggerEnter2D(playerObject.GetComponent<Collider2D>());
        }
        else
        {
            Debug.LogError("FluidPowerup: No GameObject with 'Player' tag found for testing!");
        }
    }
}