using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PartContentContext
{
    public float partStartY;
    public float partEndY;
    public float currentHeight;
    public List<Vector3> availablePlatforms;
    public bool isPrePartSpawn;
    public bool isPartComplete;
    
    public float GetPartProgress()
    {
        if (partEndY <= partStartY) return 0f;
        return Mathf.Clamp01((currentHeight - partStartY) / (partEndY - partStartY));
    }
}