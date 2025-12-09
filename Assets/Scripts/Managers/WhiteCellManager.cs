using UnityEngine;

public class WhiteCellManager : MonoBehaviour
{
    [Header("White Cell Settings")]
    [SerializeField] private int maxWhiteCells = 3;
    
    private int currentWhiteCells;
    
    public int CurrentWhiteCells => currentWhiteCells;
    public int MaxWhiteCells => maxWhiteCells;

    private void Awake()
    {
        InitializeWhiteCells();
        StartListeningToEvents();
    }

    private void OnDestroy()
    {
        StopListeningToEvents();
    }

    private void InitializeWhiteCells()
    {
        currentWhiteCells = maxWhiteCells;
        EventManager.TriggerEvent<EventName, int>(EventName.OnWhiteCellCountChanged, currentWhiteCells);
    }

    private void StartListeningToEvents()
    {
        EventManager.StartListening<EventName>(EventName.OnEnemyCollision, OnEnemyCollision);
        EventManager.StartListening<EventName>(EventName.OnEnemyMissed, OnEnemyMissed);
    }

    private void StopListeningToEvents()
    {
        EventManager.StopListening<EventName>(EventName.OnEnemyCollision, OnEnemyCollision);
        EventManager.StopListening<EventName>(EventName.OnEnemyMissed, OnEnemyMissed);
    }

    private void OnEnemyCollision()
    {
        LoseWhiteCell("Enemy Collision");
    }

    private void OnEnemyMissed()
    {
        LoseWhiteCell("Enemy Missed");
    }

    private void LoseWhiteCell(string reason)
    {
        if (currentWhiteCells <= 0)
            return;

        currentWhiteCells--;
        
        Debug.Log($"White cell lost due to: {reason}. Remaining: {currentWhiteCells}");
        
        // Trigger events
        EventManager.TriggerEvent<EventName, int, string>(EventName.OnWhiteCellLost, currentWhiteCells, reason);
        EventManager.TriggerEvent<EventName, int>(EventName.OnWhiteCellCountChanged, currentWhiteCells);
        
        // Check for game failure
        if (currentWhiteCells <= 0)
        {
            TriggerGameFailure();
        }
    }

    private void TriggerGameFailure()
    {
        Debug.Log("Game Failed: No white cells remaining!");
        EventManager.TriggerEvent<EventName>(EventName.OnGameFailed);
    }

    public void ResetWhiteCells()
    {
        InitializeWhiteCells();
    }

    // Method to manually set white cells (for testing or special cases)
    public void SetWhiteCells(int amount)
    {
        currentWhiteCells = Mathf.Clamp(amount, 0, maxWhiteCells);
        EventManager.TriggerEvent<EventName, int>(EventName.OnWhiteCellCountChanged, currentWhiteCells);
    }
}