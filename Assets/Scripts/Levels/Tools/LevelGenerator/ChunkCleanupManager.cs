using System.Collections.Generic;
using UnityEngine;

public class ChunkCleanupManager
{
    private readonly float generationDistance;
    private readonly bool enableDebugLogs;
    
    public ChunkCleanupManager(float distance, bool debugLogs)
    {
        generationDistance = distance;
        enableDebugLogs = debugLogs;
    }
    
    public void CleanupOldChunks(List<HeightChunk> chunks, float cameraY)
    {
        float cleanupThreshold = cameraY - (generationDistance * 1.5f);
        
        for (int i = chunks.Count - 1; i >= 0; i--)
        {
            if (chunks[i].endY < cleanupThreshold)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"ChunkCleanupManager: Cleaning up chunk {chunks[i].chunkId}");
                }
                chunks.RemoveAt(i);
            }
        }
    }
}