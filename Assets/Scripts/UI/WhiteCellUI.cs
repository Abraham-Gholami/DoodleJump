using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WhiteCellUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform whiteCellContainer;
    [SerializeField] private GameObject whiteCellPrefab;
    
    [Header("Visual Settings")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.gray;
    
    [Header("Components")]
    [SerializeField] private WhiteCellAnimator cellAnimator;
    [SerializeField] private WhiteCellAudio cellAudio;
    [SerializeField] private ScreenShakeController screenShake;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private List<GameObject> whiteCellUIElements = new List<GameObject>();
    private int maxWhiteCells = 3;
    private int previousCellCount = 3;
    private bool isInitialized = false;
    
    private void Start()
    {
        if (enableDebugLogs)
            Debug.Log("WhiteCellUI: Starting initialization");
        
        ValidateReferences();
        InitializeComponents();
        InitializeUI();
        StartListeningToEvents();
    }
    
    private void ValidateReferences()
    {
        if (whiteCellContainer == null)
            Debug.LogError("WhiteCellUI: whiteCellContainer is NULL!");
        
        if (whiteCellPrefab == null)
            Debug.LogError("WhiteCellUI: whiteCellPrefab is NULL!");
        
        if (enableDebugLogs)
            Debug.Log($"WhiteCellUI: References validated");
    }
    
    private void InitializeComponents()
    {
        if (cellAnimator == null) cellAnimator = GetComponent<WhiteCellAnimator>();
        if (cellAudio == null) cellAudio = GetComponent<WhiteCellAudio>();
        if (screenShake == null) screenShake = GetComponent<ScreenShakeController>();
        
        // Initialize components AFTER UI is created
        cellAudio?.Initialize();
        screenShake?.Initialize();
        
        // Note: cellAnimator.Initialize() will be called after CreateAllCells()
    }
    
    private void InitializeUI()
    {
        if (whiteCellContainer == null || whiteCellPrefab == null) return;
        
        CreateAllCells();
        SetAllCellsActive();
        
        // Initialize animator AFTER cells are created
        cellAnimator?.Initialize(this);
        
        isInitialized = true;
        
        if (enableDebugLogs)
            Debug.Log($"WhiteCellUI: Initialization complete - {whiteCellUIElements.Count} cells created");
    }
    
    private void CreateAllCells()
    {
        ClearAllCells();
        
        for (int i = 0; i < maxWhiteCells; i++)
        {
            GameObject newCell = Instantiate(whiteCellPrefab, whiteCellContainer);
            whiteCellUIElements.Add(newCell);
            
            if (enableDebugLogs)
                Debug.Log($"WhiteCellUI: Created cell {i}");
        }
    }
    
    private void ClearAllCells()
    {
        foreach (GameObject cell in whiteCellUIElements)
        {
            if (cell != null) DestroyImmediate(cell);
        }
        whiteCellUIElements.Clear();
    }
    
    private void SetAllCellsActive()
    {
        for (int i = 0; i < whiteCellUIElements.Count; i++)
        {
            SetCellActive(i, true);
        }
    }
    
    public void SetCellActive(int cellIndex, bool isActive)
    {
        if (cellIndex < 0 || cellIndex >= whiteCellUIElements.Count) return;
        
        GameObject cell = whiteCellUIElements[cellIndex];
        if (cell == null) return;
        
        Image cellImage = cell.GetComponent<Image>();
        if (cellImage != null)
        {
            cellImage.color = isActive ? activeColor : inactiveColor;
            cell.transform.localScale = isActive ? Vector3.one : Vector3.one * 0.9f;
        }
    }
    
    private void StartListeningToEvents()
    {
        EventManager.StartListening<EventName, int>(EventName.OnWhiteCellCountChanged, OnWhiteCellCountChanged);
        EventManager.StartListening<EventName, int, string>(EventName.OnWhiteCellLost, OnWhiteCellLost);
    }
    
    private void StopListeningToEvents()
    {
        EventManager.StopListening<EventName, int>(EventName.OnWhiteCellCountChanged, OnWhiteCellCountChanged);
        EventManager.StopListening<EventName, int, string>(EventName.OnWhiteCellLost, OnWhiteCellLost);
    }
    
    private void OnWhiteCellCountChanged(int newCount)
    {
        if (!isInitialized) return;
        
        if (enableDebugLogs)
            Debug.Log($"WhiteCellUI: White cell count changed from {previousCellCount} to {newCount}");
        
        // Handle animations BEFORE updating display
        if (newCount < previousCellCount)
        {
            HandleCellsLost(previousCellCount, newCount);
        }
        else if (newCount > previousCellCount)
        {
            HandleCellsRestored(previousCellCount, newCount);
        }
        else
        {
            // No change in count, just update display
            UpdateCellDisplay(newCount);
        }
        
        previousCellCount = newCount;
    }
    
    private void OnWhiteCellLost(int remainingCount, string reason)
    {
        if (enableDebugLogs)
            Debug.Log($"WhiteCellUI: White cell lost - {remainingCount} remaining, reason: {reason}");
    }
    
    private void HandleCellsLost(int oldCount, int newCount)
    {
        // DON'T update display immediately - let animation handle it
        
        // Trigger lose animations
        for (int i = newCount; i < oldCount; i++)
        {
            cellAnimator?.PlayLoseAnimation(i);
        }
        
        // Play audio and screen shake
        cellAudio?.PlayLoseSound();
        screenShake?.TriggerShake();
    }
    
    private void HandleCellsRestored(int oldCount, int newCount)
    {
        // DON'T update display immediately - let animation handle it
        
        // Trigger restore animations  
        for (int i = oldCount; i < newCount; i++)
        {
            cellAnimator?.PlayRestoreAnimation(i);
        }
        
        // Play audio
        cellAudio?.PlayRestoreSound();
    }
    
    private void UpdateCellDisplay(int activeCount)
    {
        for (int i = 0; i < whiteCellUIElements.Count; i++)
        {
            bool shouldBeActive = i < activeCount;
            SetCellActive(i, shouldBeActive);
        }
    }
    
    // Public interface
    public GameObject GetCell(int index)
    {
        if (index >= 0 && index < whiteCellUIElements.Count)
            return whiteCellUIElements[index];
        return null;
    }
    
    public int GetCellCount() => whiteCellUIElements.Count;
    
    private void OnDestroy()
    {
        StopListeningToEvents();
    }
}