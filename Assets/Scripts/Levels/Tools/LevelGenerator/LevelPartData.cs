using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New Level Part", menuName = "Level Generation/Level Part")]
public class LevelPartData : ScriptableObject
{
    [Header("Part Identity")]
    [Tooltip("Name displayed in inspector and debug logs")]
    public string partName = "New Part";
    public LevelPartType partType = LevelPartType.Safe;
    [TextArea(2, 3)]
    [Tooltip("Describe what this part does for documentation")]
    public string description = "";
    
    [Header("Part Size")]
    public PartSizeMode sizeMode = PartSizeMode.PlatformCount;
    
    [Header("Platform Configuration")]
    [Tooltip("Number of platforms when using Platform Count mode")]
    public int platformCount = 4;
    [Tooltip("Fixed height when using Fixed Length mode")]
    public float fixedPartLength = 10f;
    [Tooltip("Minimum platforms to ensure playability in Fixed Length mode")]
    public int minPlatformsInFixedLength = 2;
    [Tooltip("Spacing between platforms")]
    public float platformSpacingMin = 1.5f;
    public float platformSpacingMax = 3.5f;
    [Tooltip("Platforms available for this part")]
    public List<WeightedPlatform> platformTypes = new List<WeightedPlatform>();
    
    [Header("Content Rules")]
    [Tooltip("What content this part spawns and when")]
    public ContentSpawnRule[] contentRules = new ContentSpawnRule[0];
    
    [Header("Part Rules")]
    [Range(1f, 5f)]
    [Tooltip("Difficulty scale: 1=Very Easy, 5=Very Hard")]
    public float difficultyRating = 1f;
    [Tooltip("Minimum game height before this part can appear")]
    public float minHeightRequired = 0f;
    
    [Header("Visual")]
    [Tooltip("Color for editor visualization")]
    public Color editorColor = Color.white;
    
    // Validation and helper methods
    private void OnValidate()
    {
        ValidateSettings();
        UpdatePartName();
    }
    
    private void ValidateSettings()
    {
        platformCount = Mathf.Max(1, platformCount);
        fixedPartLength = Mathf.Max(1f, fixedPartLength);
        minPlatformsInFixedLength = Mathf.Max(1, minPlatformsInFixedLength);
        
        if (platformSpacingMin > platformSpacingMax)
            platformSpacingMin = platformSpacingMax;
    }
    
    private void UpdatePartName()
    {
        if (string.IsNullOrEmpty(partName) || partName == "New Part")
        {
            partName = $"{partType} {difficultyRating:F1}";
        }
    }
    
    public bool IsValidPart()
    {
        if (platformTypes.Count == 0)
        {
            Debug.LogError($"Part '{partName}': No platform types specified!");
            return false;
        }
        
        return true;
    }
    
    public bool RequiresEnemies()
    {
        return contentRules.Any(rule => rule.contentType == ContentType.Enemy);
    }
    
    public bool RequiresPowerUps()
    {
        return contentRules.Any(rule => rule.contentType == ContentType.PowerUp);
    }
    
    public int CalculatePlatformCount()
    {
        if (sizeMode == PartSizeMode.PlatformCount)
        {
            int variation = Random.Range(-1, 2);
            return Mathf.Max(1, platformCount + variation);
        }
        else
        {
            float averageSpacing = (platformSpacingMin + platformSpacingMax) / 2f;
            int calculated = Mathf.FloorToInt(fixedPartLength / averageSpacing);
            return Mathf.Max(minPlatformsInFixedLength, calculated);
        }
    }
    
    public float CalculateTargetLength()
    {
        if (sizeMode == PartSizeMode.FixedLength)
        {
            return fixedPartLength;
        }
        else
        {
            float averageSpacing = (platformSpacingMin + platformSpacingMax) / 2f;
            return CalculatePlatformCount() * averageSpacing;
        }
    }
    
    public float GetRandomSpacing()
    {
        return Random.Range(platformSpacingMin, platformSpacingMax);
    }
}

public enum PartSizeMode
{
    PlatformCount,
    FixedLength
}

public enum LevelPartType
{
    Safe,
    EnemyChallenge,
    PowerUp,
    Mixed,
    Rest
}

public enum EnemyPlacementType
{
    RandomAbovePlatforms,
    Chain,
    ScreenBlock,
    Mixed
}