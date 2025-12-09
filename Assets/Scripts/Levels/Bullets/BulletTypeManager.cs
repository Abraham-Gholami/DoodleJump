// ============= GENERIC BULLET TYPE MANAGER =============
using System.Collections.Generic;
using UnityEngine;

public class BulletTypeManager : MonoBehaviour
{
    [Header("Bullet Types Configuration")]
    [SerializeField] private List<BulletTypeData> availableBulletTypes = new List<BulletTypeData>();
    [SerializeField] private int defaultBulletTypeIndex = 0;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip switchSoundClip;
    
    private int currentBulletTypeIndex = 0;
    private BulletTypeData currentBulletType;
    
    // Events for UI and other systems
    public System.Action<BulletTypeData, int> OnBulletTypeChanged;
    
    // Singleton pattern for easy access
    public static BulletTypeManager Instance { get; private set; }
    
    // Public properties
    public BulletTypeData CurrentBulletType => currentBulletType;
    public GameObject CurrentBulletPrefab => currentBulletType?.bulletPrefab;
    public List<BulletTypeData> AvailableBulletTypes => availableBulletTypes;
    public int CurrentBulletTypeIndex => currentBulletTypeIndex;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Validate setup
        ValidateConfiguration();
        
        // Set default bullet type
        SetBulletType(defaultBulletTypeIndex);
    }
    
    private void Start()
    {
        // Notify UI of initial bullet type
        OnBulletTypeChanged?.Invoke(currentBulletType, currentBulletTypeIndex);
    }
    
    private void ValidateConfiguration()
    {
        // Remove null entries
        availableBulletTypes.RemoveAll(bulletType => bulletType == null);
        
        // Validate default index
        if (defaultBulletTypeIndex >= availableBulletTypes.Count)
        {
            defaultBulletTypeIndex = 0;
            Debug.LogWarning("BulletTypeManager: Default bullet type index out of range. Reset to 0.");
        }
        
        // Check for missing data
        for (int i = 0; i < availableBulletTypes.Count; i++)
        {
            BulletTypeData bulletType = availableBulletTypes[i];
            if (bulletType.bulletPrefab == null)
            {
                Debug.LogError($"BulletTypeManager: Bullet type '{bulletType.typeName}' is missing bullet prefab!");
            }
            if (bulletType.buttonIcon == null)
            {
                Debug.LogWarning($"BulletTypeManager: Bullet type '{bulletType.typeName}' is missing button icon!");
            }
        }
    }
    
    public void SetBulletType(int index)
    {
        if (index < 0 || index >= availableBulletTypes.Count)
        {
            Debug.LogError($"BulletTypeManager: Invalid bullet type index {index}");
            return;
        }
        
        // Don't switch if already selected
        if (currentBulletTypeIndex == index && currentBulletType != null)
            return;
        
        currentBulletTypeIndex = index;
        currentBulletType = availableBulletTypes[index];
        
        // Play switch sound
        PlaySwitchSound();
        
        // Notify systems
        OnBulletTypeChanged?.Invoke(currentBulletType, currentBulletTypeIndex);
    }
    
    public void SetBulletType(BulletTypeData bulletTypeData)
    {
        int index = availableBulletTypes.IndexOf(bulletTypeData);
        if (index >= 0)
        {
            SetBulletType(index);
        }
        else
        {
            Debug.LogError($"BulletTypeManager: Bullet type '{bulletTypeData.typeName}' not found in available types!");
        }
    }
    
    public void NextBulletType()
    {
        if (availableBulletTypes.Count <= 1) return;
        
        int nextIndex = (currentBulletTypeIndex + 1) % availableBulletTypes.Count;
        SetBulletType(nextIndex);
    }
    
    public void PreviousBulletType()
    {
        if (availableBulletTypes.Count <= 1) return;
        
        int prevIndex = currentBulletTypeIndex - 1;
        if (prevIndex < 0) prevIndex = availableBulletTypes.Count - 1;
        SetBulletType(prevIndex);
    }
    
    private void PlaySwitchSound()
    {
        if (audioSource != null && switchSoundClip != null)
        {
            audioSource.PlayOneShot(switchSoundClip);
        }
    }
    
    // Static methods for easy access
    public static GameObject GetCurrentBulletPrefab()
    {
        return Instance?.CurrentBulletPrefab;
    }
    
    public static BulletTypeData GetCurrentBulletType()
    {
        return Instance?.CurrentBulletType;
    }
    
    // Method to check if specific bullet type is available
    public bool HasBulletType(BulletTypeData bulletTypeData)
    {
        return availableBulletTypes.Contains(bulletTypeData);
    }
    
    // Method to get bullet type by name
    public BulletTypeData GetBulletTypeByName(string typeName)
    {
        return availableBulletTypes.Find(bt => bt.typeName.Equals(typeName, System.StringComparison.OrdinalIgnoreCase));
    }
    
    // Method to add bullet type at runtime (useful for power-ups)
    public void AddBulletType(BulletTypeData bulletTypeData)
    {
        if (!availableBulletTypes.Contains(bulletTypeData))
        {
            availableBulletTypes.Add(bulletTypeData);
            OnBulletTypeChanged?.Invoke(currentBulletType, currentBulletTypeIndex);
        }
    }
    
    // Method to remove bullet type at runtime
    public void RemoveBulletType(BulletTypeData bulletTypeData)
    {
        int index = availableBulletTypes.IndexOf(bulletTypeData);
        if (index >= 0)
        {
            availableBulletTypes.RemoveAt(index);
            
            // If current bullet type was removed, switch to default
            if (currentBulletTypeIndex == index)
            {
                SetBulletType(0);
            }
            else if (currentBulletTypeIndex > index)
            {
                currentBulletTypeIndex--; // Adjust index after removal
            }
        }
    }
}