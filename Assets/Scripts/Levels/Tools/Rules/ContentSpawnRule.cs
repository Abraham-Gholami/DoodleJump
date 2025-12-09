using UnityEngine;

[System.Serializable]
public class ContentSpawnRule
{
    [Header("Basic Settings")]
    [Tooltip("What type of content to spawn")]
    public ContentType contentType = ContentType.Enemy;
    [Tooltip("Prefab to spawn")]
    public GameObject prefab;
    
    [Header("Spawn Count")]
    [Tooltip("Minimum number to spawn")]
    public int minCount = 1;
    [Tooltip("Maximum number to spawn")]
    public int maxCount = 1;
    
    [Header("Positioning")]
    [Tooltip("Where to spawn this content")]
    public SimplePositioning positioning = SimplePositioning.OverPlatforms;
    
    [Header("Flipping")]
    [Tooltip("How to flip spawned objects horizontally")]
    public FlipMode flipMode = FlipMode.Random;
    [Tooltip("Fixed flip direction (when using Fixed mode)")]
    public bool flipFixed = false; // false = right, true = left
    
    [Header("Special Options")]
    [Tooltip("Spawn before part starts (like blood transfusion)")]
    public bool spawnBeforePart = false;
    [Tooltip("Create a chain formation")]
    public bool createChain = false;
    
    [Header("Chain Settings")]
    [Tooltip("Minimum chain length")]
    public int minChainLength = 3;
    [Tooltip("Maximum chain length")]
    public int maxChainLength = 5;
    
    [Header("Chain Spacing")]
    public ChainSpacingMode chainSpacingMode = ChainSpacingMode.EvenlyDistributed;
    [Tooltip("Custom spacing between chain elements (when using Custom mode)")]
    public float customChainSpacing = 2f;
    [Tooltip("Minimum spacing for random mode")]
    public float minChainSpacing = 1.5f;
    [Tooltip("Maximum spacing for random mode")]
    public float maxChainSpacing = 3f;
    
    public int GetRandomCount()
    {
        return Random.Range(minCount, maxCount + 1);
    }
    
    public int GetRandomChainLength()
    {
        return Random.Range(minChainLength, maxChainLength + 1);
    }
    
    public float GetChainSpacing(float screenWidth, int actualChainLength)
    {
        switch (chainSpacingMode)
        {
            case ChainSpacingMode.EvenlyDistributed:
                return screenWidth / (actualChainLength + 1);
                
            case ChainSpacingMode.Custom:
                return customChainSpacing;
                
            case ChainSpacingMode.Random:
                return Random.Range(minChainSpacing, maxChainSpacing);
                
            default:
                return screenWidth / (actualChainLength + 1);
        }
    }
    
    public bool GetFlip()
    {
        switch (flipMode)
        {
            case FlipMode.Fixed:
                return flipFixed;
                
            case FlipMode.Random:
                return Random.Range(0, 2) == 1;
                
            case FlipMode.None:
                return false;
                
            default:
                return Random.Range(0, 2) == 1;
        }
    }
}

public enum ChainSpacingMode
{
    EvenlyDistributed,  // Spread evenly across screen width
    Custom,             // Fixed custom spacing
    Random              // Random spacing between min/max
}

public enum FlipMode
{
    None,       // No flipping (always face right)
    Fixed,      // Fixed flip direction
    Random      // Random flip (50/50 left/right)
}

public enum SimplePositioning
{
    OverPlatforms,      // Spawn above platforms
    RandomAcrossScreen  // Spawn randomly across screen width
}

public enum ContentType
{
    Enemy,
    PowerUp,
    Obstacle,
    Decoration
}