using UnityEngine;

public class EnemyBoundaryCleanup : MonoBehaviour
{
    [Header("Enemy Boundary Settings")]
    [SerializeField] private float destroyBuffer = 5f;
    [SerializeField] private bool triggerMissedEventOnCleanup = true;
    [SerializeField] private bool onlyTriggerIfAlive = true;
    
    private bool wasKilled = false;
    private bool hasTriggeredMissedEvent = false;
    private bool isBeingDestroyed = false;
    private Enemy enemyComponent;
    
    private void Start()
    {
        // Get enemy component reference
        enemyComponent = GetComponent<Enemy>();
        
        // Listen for when this specific enemy gets killed
        StartListeningToEvents();
        
        // Register this enemy as spawned
        EventManager.TriggerEvent<EventName, GameObject>(EventName.SpawnEnemy, gameObject);
    }
    
    private void OnDestroy()
    {
        StopListeningToEvents();
    }
    
    private void FixedUpdate()
    {
        if (!isBeingDestroyed && !IsEnemyDead())
        {
            CheckBoundaryCleanup();
        }
    }
    
    private bool IsEnemyDead()
    {
        // Check if enemy is already dead (either marked by us or by the Enemy component)
        return wasKilled || (enemyComponent != null && enemyComponent.IsAlreadyDead);
    }
    
    private void StartListeningToEvents()
    {
        EventManager.StartListening<EventName, GameObject>(EventName.OnEnemyKilled, OnEnemyKilled);
    }
    
    private void StopListeningToEvents()
    {
        EventManager.StopListening<EventName, GameObject>(EventName.OnEnemyKilled, OnEnemyKilled);
    }
    
    private void OnEnemyKilled(GameObject killedEnemy)
    {
        // Check if this component and its GameObject still exist
        if (this == null || gameObject == null)
            return;
            
        // Check if this enemy was the one killed
        if (killedEnemy == gameObject)
        {
            wasKilled = true;
        }
    }
    
    private void CheckBoundaryCleanup()
    {
        // Only cleanup when below screen (most common for Doodle Jump)
        float bottomScreenY = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        bool shouldCleanup = transform.position.y < bottomScreenY - destroyBuffer;
        
        if (shouldCleanup)
        {
            CleanupEnemy();
        }
    }
    
    private void CleanupEnemy()
    {
        isBeingDestroyed = true;
        
        // Only trigger missed event if enemy is alive and we haven't triggered it before
        if (triggerMissedEventOnCleanup && !IsEnemyDead() && onlyTriggerIfAlive && !hasTriggeredMissedEvent)
        {
            Debug.Log($"Enemy {gameObject.name} was missed - triggering missed event");
            EventManager.TriggerEvent<EventName>(EventName.OnEnemyMissed);
            hasTriggeredMissedEvent = true;
        }
        
        // Destroy the enemy if it's not already being destroyed by the Enemy component
        if (!IsEnemyDead())
        {
            Debug.Log($"Enemy {gameObject.name} cleaned up by boundary system");
            Destroy(gameObject);
        }
    }
    
    // Public method to mark enemy as killed (can be called from enemy death script)
    public void MarkAsKilled()
    {
        wasKilled = true;
    }
    
    // Public method to check if enemy was killed
    public bool WasKilled()
    {
        return wasKilled;
    }
    
    // Public method to force missed event (for testing)
    public void TriggerMissedEvent()
    {
        if (!hasTriggeredMissedEvent && !IsEnemyDead())
        {
            EventManager.TriggerEvent<EventName>(EventName.OnEnemyMissed);
            hasTriggeredMissedEvent = true;
        }
    }
    
    // Public method to force cleanup (useful for testing)
    public void ForceCleanup()
    {
        if (!isBeingDestroyed)
        {
            CleanupEnemy();
        }
    }
    
    // Method to set custom destroy buffer
    public void SetDestroyBuffer(float buffer)
    {
        destroyBuffer = buffer;
    }
}