using UnityEngine;

public class Platform : MonoBehaviour
{
    public const float JumpForce = 20f;

    [SerializeField] private float destroyBuffer = 1f;
    [SerializeField] protected AudioSource audioSource;

    protected virtual void FixedUpdate()
    {
        float bottomScreenY = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;

        if (transform.position.y < bottomScreenY - destroyBuffer)
        {
            // Note: Platform spawning is now handled automatically by PlatformAndEnemyGenerator
            // based on camera position, so we don't need to manually trigger it here
            
            DisablePlatformVisuals();

            if (transform.childCount > 0)
            {
                var child = transform.GetChild(0);

                if (child.GetComponent<Platform>())
                {
                    child.GetComponent<EdgeCollider2D>().enabled = false;
                    child.GetComponent<PlatformEffector2D>().enabled = false;
                    child.GetComponent<SpriteRenderer>().enabled = false;
                }

                
                /*
                if (!GetComponent<AudioSource>().isPlaying &&
                    !child.GetComponent<AudioSource>().isPlaying)
                {
                    Destroy(gameObject);
                }*/
            }
            else
            {/*
                if (!GetComponent<AudioSource>().isPlaying)
                    Destroy(gameObject);*/
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        var playerRb = other.collider.GetComponent<Rigidbody2D>();
        if (playerRb == null || playerRb.linearVelocity.y > 0f) return;

        // Check if player is in helicopter mode - no platform interactions while flying
        PlayerHelicopterSystem helicopterSystem = other.gameObject.GetComponent<PlayerHelicopterSystem>();
        if (helicopterSystem != null && helicopterSystem.IsFlying)
        {
            Debug.Log("Platform: Player is flying - no platform interaction");
            return; // Player flies through platforms
        }

        // Calculate jump force (normal or spring-boosted)
        float jumpForce = CalculateJumpForce(other.gameObject);
        
        var force = playerRb.linearVelocity;
        force.y = jumpForce;
        playerRb.linearVelocity = force;

        if (audioSource != null)
        {
            audioSource.Play();
        }

        if (GetComponent<Animator>())
            GetComponent<Animator>().SetBool("Active", true);

        CheckPlatformType();
    }

    private float CalculateJumpForce(GameObject player)
    {
        // Check if player has spring power
        PlayerSpringSystem springSystem = player.GetComponent<PlayerSpringSystem>();
        
        if (springSystem != null && springSystem.HasSpringPower)
        {
            // Apply spring boost
            float boostedJumpForce = JumpForce * springSystem.SpringJumpMultiplier;
            
            // Notify spring system that player used spring jump
            springSystem.OnPlayerUsedSpringJump();
            
            Debug.Log($"Platform: Spring jump! Force: {boostedJumpForce} (multiplier: {springSystem.SpringJumpMultiplier})");
            return boostedJumpForce;
        }
        
        // Normal jump
        return JumpForce;
    }

    private void CheckPlatformType()
    {
        if (TryGetComponent(out OneTimePlatform white))
            white.Deactive();
        else if (TryGetComponent(out Platform_Brown brown))
            brown.Deactive();
    }

    private void DisablePlatformVisuals()
    {
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<PlatformEffector2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
    }
}