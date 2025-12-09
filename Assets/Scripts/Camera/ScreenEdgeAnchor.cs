using UnityEngine;

[ExecuteAlways]
public class ScreenEdgeAnchor : MonoBehaviour
{
    public enum HorizontalSide { Left, Right }
    public HorizontalSide side;

    public float xPadding = 0f; // small padding from edge

    private Camera cam;

    void Update()
    {
        cam = Camera.main;
        if (!cam) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        Vector3 newPos = transform.position;
        //newPos.y = cam.transform.position.y + yOffset;

        if (side == HorizontalSide.Left)
            newPos.x = cam.transform.position.x - halfWidth + xPadding;
        else
            newPos.x = cam.transform.position.x + halfWidth - xPadding;

        transform.position = newPos;
    }
}