using System;
using System.Collections;
using UnityEngine;

public class SettingsDataHolder : MonoBehaviour
{
    public static SettingsDataHolder Instance { get; private set; }
    
    // Static settings that persist and are loaded immediately
    public static int SelectedCharacterIndex { get; private set; } = 0;
    public static int ControlType { get; private set; } = 0;

    // Safe property access with full validation
    public static Sprite[] SelectedCharacterJumpFrames
    {
        get
        {
            if (!ValidateInstance()) return null;
            
            var frames = Instance.CharactersJumpFrames?.charactersFrames;
            if (!ValidateFramesArray(frames, "Jump")) return null;
            
            if (!ValidateCharacterIndex(frames.Length)) return null;
            
            return frames[SelectedCharacterIndex].frames;
        }
    }

    public static Sprite[] SelectedCharacterShootFrames
    {
        get
        {
            if (!ValidateInstance()) return null;
            
            var frames = Instance.CharactersShootFrames?.charactersFrames;
            if (!ValidateFramesArray(frames, "Shoot")) return null;
            
            if (!ValidateCharacterIndex(frames.Length)) return null;
            
            return frames[SelectedCharacterIndex].frames;
        }
    }

    [Header("Character Data")]
    [SerializeField] private CharactersFramesData CharactersJumpFrames;
    [SerializeField] private CharactersFramesData CharactersShootFrames;
    
    private void Awake()
    {
        Time.timeScale = 1f;
        
        Debug.Log($"SettingsDataHolder: Awake called - Current ControlType: {ControlType}");
        
        if (!InitializeSingleton()) return;
        
        
        LoadSettingsStatic();
        RegisterEvents();
        
        // Reload settings to ensure we have the latest values
        Debug.Log("SettingsDataHolder: Reloading settings in Awake");
        
        Debug.Log($"SettingsDataHolder initialized - Character: {SelectedCharacterIndex}, Control: {ControlType}");
    }
    
    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        ValidateData();
    }
    
    private bool InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("SettingsDataHolder: Duplicate instance found, destroying new one");
            Destroy(gameObject);
            return false;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        return true;
    }
    
    private static void LoadSettingsStatic()
    {
        int savedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
        int savedControlType = PlayerPrefs.GetInt("ControlType", 0);
        
        // Basic validation - will be refined when Instance is available
        SelectedCharacterIndex = Mathf.Max(0, savedCharacterIndex);
        ControlType = Mathf.Clamp(savedControlType, 0, 1); // Assuming 0-1 range for control types
        
        Debug.Log($"SettingsDataHolder: LoadSettingsStatic - Character: {SelectedCharacterIndex}, Control: {ControlType}");
        Debug.Log($"SettingsDataHolder: Raw PlayerPrefs - Character: {savedCharacterIndex}, Control: {savedControlType}");
    }
    
    private void ValidateData()
    {
        // Validate character index against actual data
        if (CharactersJumpFrames?.charactersFrames != null)
        {
            int maxIndex = CharactersJumpFrames.charactersFrames.Length - 1;
            if (SelectedCharacterIndex > maxIndex)
            {
                Debug.LogWarning($"Character index {SelectedCharacterIndex} is out of range, clamping to {maxIndex}");
                SetCharacterIndex(maxIndex);
            }
        }
        
        // Validate that jump and shoot frames match
        if (CharactersJumpFrames?.charactersFrames?.Length != CharactersShootFrames?.charactersFrames?.Length)
        {
            Debug.LogError("SettingsDataHolder: Jump and Shoot frames arrays have different lengths!");
        }
    }

    private void RegisterEvents()
    {
        EventManager.StartListening<EventName, int>(EventName.OnCharacterChanged, OnCharacterChanged);
        EventManager.StartListening<EventName, int>(EventName.OnControlTypeChanged, OnControlTypeChanged);
    }

    private void UnregisterEvents()
    {
        EventManager.StopListening<EventName, int>(EventName.OnCharacterChanged, OnCharacterChanged);
        EventManager.StopListening<EventName, int>(EventName.OnControlTypeChanged, OnControlTypeChanged);
    }

    private void OnCharacterChanged(int newIndex)
    {
        Debug.Log($"SettingsDataHolder: OnCharacterChanged called with index {newIndex}");
        SetCharacterIndex(newIndex);
    }

    private void OnControlTypeChanged(int newControlType)
    {
        Debug.Log($"SettingsDataHolder: OnControlTypeChanged called with type {newControlType}");
        SetControlType(newControlType);
    }
    
    public static void SetCharacterIndex(int newIndex)
    {
        // Validate against available characters if Instance exists
        if (Instance != null && Instance.CharactersJumpFrames?.charactersFrames != null)
        {
            newIndex = Mathf.Clamp(newIndex, 0, Instance.CharactersJumpFrames.charactersFrames.Length - 1);
        }
        else
        {
            newIndex = Mathf.Max(0, newIndex); // Basic validation
        }
        
        if (SelectedCharacterIndex != newIndex)
        {
            SelectedCharacterIndex = newIndex;
            PlayerPrefs.SetInt("SelectedCharacterIndex", newIndex);
            PlayerPrefs.Save();
            
            Debug.Log($"Character changed to index {newIndex}");
        }
    }
    
    public static void SetControlType(int newControlType)
    {
        Debug.Log($"SettingsDataHolder: SetControlType called - Current: {ControlType}, New: {newControlType}");
        
        newControlType = Mathf.Clamp(newControlType, 0, 1); // Assuming 0-1 range
        
        if (ControlType != newControlType)
        {
            ControlType = newControlType;
            PlayerPrefs.SetInt("ControlType", newControlType);
            PlayerPrefs.Save();
            
            Debug.Log($"SettingsDataHolder: Control type successfully changed to {newControlType}");
        }
        else
        {
            Debug.Log($"SettingsDataHolder: Control type unchanged - already {newControlType}");
        }
    }

    // Validation helper methods
    private static bool ValidateInstance()
    {
        if (Instance == null)
        {
            Debug.LogError("SettingsDataHolder: Instance is null!");
            return false;
        }
        return true;
    }
    
    private static bool ValidateFramesArray(InnerList[] frames, string frameType)
    {
        if (frames == null || frames.Length == 0)
        {
            Debug.LogError($"SettingsDataHolder: {frameType} frames array is null or empty!");
            return false;
        }
        return true;
    }
    
    private static bool ValidateCharacterIndex(int arrayLength)
    {
        if (SelectedCharacterIndex < 0 || SelectedCharacterIndex >= arrayLength)
        {
            Debug.LogError($"SettingsDataHolder: Character index {SelectedCharacterIndex} is out of range [0-{arrayLength-1}]!");
            return false;
        }
        return true;
    }
    
    // Public utility methods
    public static bool IsValidCharacterIndex(int index)
    {
        if (Instance?.CharactersJumpFrames?.charactersFrames == null) return false;
        return index >= 0 && index < Instance.CharactersJumpFrames.charactersFrames.Length;
    }
    
    public static int GetMaxCharacterIndex()
    {
        if (Instance?.CharactersJumpFrames?.charactersFrames == null) return 0;
        return Instance.CharactersJumpFrames.charactersFrames.Length - 1;
    }
    
    public static string GetCurrentCharacterName()
    {
        var jumpFrames = Instance?.CharactersJumpFrames?.charactersFrames;
        if (jumpFrames != null && IsValidCharacterIndex(SelectedCharacterIndex))
        {
            return jumpFrames[SelectedCharacterIndex].characterName;
        }
        return "Unknown";
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            UnregisterEvents();
            Instance = null;
        }
    }
    
    private void OnValidate()
    {
        // Editor validation - only runs in editor
        if (CharactersJumpFrames?.charactersFrames?.Length != CharactersShootFrames?.charactersFrames?.Length)
        {
            Debug.LogWarning("SettingsDataHolder: Jump and Shoot frames arrays should have the same length!");
        }
        
        // Check for null frames in arrays
        if (CharactersJumpFrames?.charactersFrames != null)
        {
            for (int i = 0; i < CharactersJumpFrames.charactersFrames.Length; i++)
            {
                var character = CharactersJumpFrames.charactersFrames[i];
                if (character.frames == null || character.frames.Length == 0)
                {
                    Debug.LogWarning($"Character {i} ({character.characterName}) has no jump frames!");
                }
            }
        }
        
        if (CharactersShootFrames?.charactersFrames != null)
        {
            for (int i = 0; i < CharactersShootFrames.charactersFrames.Length; i++)
            {
                var character = CharactersShootFrames.charactersFrames[i];
                if (character.frames == null || character.frames.Length == 0)
                {
                    Debug.LogWarning($"Character {i} ({character.characterName}) has no shoot frames!");
                }
            }
        }
    }
    
    [System.Serializable]
    public class CharactersFramesData
    {
        public InnerList[] charactersFrames;
    }
    
    [System.Serializable]
    public class InnerList
    {
        public string characterName;
        public Sprite[] frames;
    }
}