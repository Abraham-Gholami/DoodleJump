using System;
using UnityEngine;

[ExecuteAlways]
public class CameraHelper : MonoBehaviour
{
    public float targetWidth = 10f; // how many world units wide the view should always be

    private void Start()
    {
        AdjustCameraSize();
    }

    private void Update()
    {
        #if UNITY_EDITOR
        AdjustCameraSize();
        #endif
    }

    private void OnGUI()
    {
        AdjustCameraSize();
    }

    private void AdjustCameraSize()
    {
        float aspect = (float)Screen.width / Screen.height;
        if (Camera.main != null) Camera.main.orthographicSize = (targetWidth / aspect) / 2f;
    }
}
