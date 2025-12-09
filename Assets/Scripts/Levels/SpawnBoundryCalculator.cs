using UnityEngine;

public class SpawnBoundaryCalculator
{
    private float initialOffset;
    private float wallOffset = 3f;
    
    public SpawnBoundaryCalculator(float offset)
    {
        initialOffset = offset;
    }
    
    public SpawnBoundaries GetBoundaries()
    {
        Vector3 bottomLeft = Camera.main.ScreenToWorldPoint(Vector3.zero);
        Vector3 bottomRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
        
        float screenLeft = bottomLeft.x + initialOffset;
        float screenRight = bottomRight.x - initialOffset;
        
        // Find walls if they exist
        var (wallLeft, wallRight) = FindWallBoundaries(screenLeft, screenRight);
        
        float finalLeft = Mathf.Max(screenLeft, wallLeft + wallOffset);
        float finalRight = Mathf.Min(screenRight, wallRight - wallOffset);
        
        return new SpawnBoundaries(finalLeft, finalRight);
    }
    
    private (float left, float right) FindWallBoundaries(float defaultLeft, float defaultRight)
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        
        if (walls.Length < 2)
            return (defaultLeft, defaultRight);
        
        float leftmost = float.MaxValue;
        float rightmost = float.MinValue;
        
        foreach (GameObject wall in walls)
        {
            Collider2D wallCollider = wall.GetComponent<Collider2D>();
            if (wallCollider != null)
            {
                Bounds bounds = wallCollider.bounds;
                
                if (wall.transform.position.x < 0)
                    leftmost = Mathf.Min(leftmost, bounds.max.x);
                else
                    rightmost = Mathf.Max(rightmost, bounds.min.x);
            }
        }
        
        float finalLeft = leftmost != float.MaxValue ? leftmost : defaultLeft;
        float finalRight = rightmost != float.MinValue ? rightmost : defaultRight;
        
        return (finalLeft, finalRight);
    }
}

public struct SpawnBoundaries
{
    public float left;
    public float right;
    
    public SpawnBoundaries(float left, float right)
    {
        this.left = left;
        this.right = right;
    }
}