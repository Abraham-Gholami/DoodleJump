using UnityEngine;

[ExecuteAlways] // makes it work in editor too
public class ResponsiveBackground : MonoBehaviour
{
    public Camera mainCamera;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!mainCamera) mainCamera = Camera.main;
        if (!sr || !mainCamera) return;

        float screenAspect = (float)Screen.width / Screen.height;
        float cameraHeight = mainCamera.orthographicSize * 2;

        Vector2 newScale = new Vector2(
            cameraHeight * screenAspect / sr.sprite.bounds.size.x,
            cameraHeight / sr.sprite.bounds.size.y
        );

        transform.localScale = newScale;
    }
}