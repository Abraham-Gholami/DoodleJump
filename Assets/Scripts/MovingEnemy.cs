using UnityEngine;

public class MovingEnemy : Enemy
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
    
    void FixedUpdate()
    {
        // Don't move if enemy is dead
        if (IsAlreadyDead) return;
        
        MoveEnemy();
    }
    
    void MoveEnemy()
    {
        if (movingRight)
        {
            transform.position += Vector3.right * speed * Time.fixedDeltaTime;
            
            if (transform.position.x >= rightBoundary)
            {
                movingRight = false;
                FlipSprite();
            }
        }
        else
        {
            transform.position += Vector3.left * speed * Time.fixedDeltaTime;
            
            if (transform.position.x <= leftBoundary)
            {
                movingRight = true;
                FlipSprite();
            }
        }
    }
    
    void FlipSprite()
    {
        // Flip the enemy sprite when changing direction
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}