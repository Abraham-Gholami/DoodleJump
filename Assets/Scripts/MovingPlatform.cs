using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : Platform
{
    [Header("Movement Settings")]
    public float speed = 4f;
    public float wallOffset = 3f;
    
    private bool movingRight = true;
    private float leftBoundary;
    private float rightBoundary;
    
    void Start()
    {
        CalculateBoundaries();
    }
    
    void CalculateBoundaries()
    {
        Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        Vector3 topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.nearClipPlane));
        
        leftBoundary = bottomLeft.x + wallOffset;
        rightBoundary = topRight.x - wallOffset;
    }
    
    protected override void FixedUpdate()
    {
        base.FixedUpdate(); // Call parent's destruction logic
        MovePlatform();     // Then handle movement
    }
    
    void MovePlatform()
    {
        if (movingRight)
        {
            transform.position += Vector3.right * speed * Time.fixedDeltaTime;
            
            if (transform.position.x >= rightBoundary)
                movingRight = false;
        }
        else
        {
            transform.position += Vector3.left * speed * Time.fixedDeltaTime;
            
            if (transform.position.x <= leftBoundary)
                movingRight = true;
        }
    }
}