using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class HeightChunk
{
    public int chunkId;
    public float startY;
    public float endY;
    public List<Vector3> platformPositions;
    public List<string> generatedContent;
    
    public float GetHeight() => endY - startY;
    public bool ContainsHeight(float y) => y >= startY && y <= endY;
}

[System.Serializable]
public class PartGenerationState
{
    public LevelPartData currentPart;
    public float partStartY;
    public float partCurrentY;
    public int platformsGenerated;
    public int platformsNeeded;
    public bool isPartComplete;
    public List<ContentSpawnItem> contentSpawnQueue;
}

[System.Serializable]
public class ContentSpawnItem
{
    public ContentSpawnRule rule;
    public bool isChain;
    public bool hasSpawned;
    public int priority;
    public float spawnedHeight;
}