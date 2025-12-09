using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Shield Settings")]
    [SerializeField] private GameObject shieldGameObject;

    private PlayerImmunity playerImmunity;
    private PlayerHelicopterSystem helicopterSystem;
    private PlayerAnimation animationComponent;
    private Rigidbody2D rb;
    private bool isPlayingDamageAnimation = false;
    private Coroutine damageAnimationCoroutine;

    private void Awake()
    {
        playerImmunity = GetComponent<PlayerImmunity>();
        helicopterSystem = GetComponent<PlayerHelicopterSystem>();
        animationComponent = GetComponent<PlayerAnimation>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (playerImmunity != null)
        {
            playerImmunity.OnImmunityStarted += OnImmunityStarted;
            playerImmunity.OnImmunityEnded += OnImmunityEnded;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        
        if (other.CompareTag("Enemy"))
        {
            HandleEnemyCollision(other);
        }
    }

    private void HandleEnemyCollision(Collider2D enemy)
    {
        // Check if player is in helicopter mode - invulnerable while flying
        if (IsPlayerFlying())
        {
            Debug.Log("PlayerCombat: Player is flying - invulnerable to enemy collision");
            // Player flies through enemies without taking damage or destroying them
            return;
        }
        
        // Check if player is jumping on enemy (has downward velocity)
        bool isJumpingOnEnemy = IsJumpingOnEnemy();
        
        if (IsPlayerImmune() || isJumpingOnEnemy)
        {
            Debug.Log($"PlayerCombat: {(isJumpingOnEnemy ? "Jumping on enemy" : "Player immune")} - destroying enemy");
            DestroyEnemy(enemy);
            return;
        }

        Debug.Log("PlayerCombat: Player taking damage from enemy");
        
        TriggerDamageAnimation();
        TriggerDamageEvent();
        DestroyEnemy(enemy);
    }
    
    
    private bool IsJumpingOnEnemy()
    {
        if (rb == null) return false;
        return rb.linearVelocity.y < 0f;
    }
    
    private void DestroyEnemy(Collider2D enemy)
    {
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.Dead();
        }
        else
        {
            EnemyBoundaryCleanup enemyBoundary = enemy.GetComponent<EnemyBoundaryCleanup>();
            if (enemyBoundary != null)
            {
                enemyBoundary.MarkAsKilled();
            }
            
            Destroy(enemy.gameObject);
        }
    }
    
    private void TriggerDamageAnimation()
    {
        if (!isPlayingDamageAnimation && animationComponent != null)
        {
            if (damageAnimationCoroutine != null)
            {
                StopCoroutine(damageAnimationCoroutine);
            }
            damageAnimationCoroutine = StartCoroutine(PlayDamageAnimationRoutine());
        }
    }
    
    private IEnumerator PlayDamageAnimationRoutine()
    {
        if (isPlayingDamageAnimation) yield break;
        
        isPlayingDamageAnimation = true;
        yield return animationComponent.PlayDamageAnimation();
        isPlayingDamageAnimation = false;
        damageAnimationCoroutine = null;
    }
    
    private void TriggerDamageEvent()
    {
        EventManager.TriggerEvent<EventName>(EventName.OnEnemyCollision);
    }

    private void OnImmunityStarted(float duration)
    {
        Debug.Log($"PlayerCombat: Immunity started for {duration} seconds");
        
        if (shieldGameObject == null)
        {
            Transform shieldTransform = transform.Find("Shield");
            if (shieldTransform != null)
            {
                shieldGameObject = shieldTransform.gameObject;
            }
        }
        
        if (shieldGameObject != null)
        {
            shieldGameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("PlayerCombat: Shield GameObject not found");
        }
    }
    
    private void OnImmunityEnded()
    {
        Debug.Log("PlayerCombat: Immunity ended");
        
        if (shieldGameObject != null)
        {
            shieldGameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (playerImmunity != null)
        {
            playerImmunity.OnImmunityStarted -= OnImmunityStarted;
            playerImmunity.OnImmunityEnded -= OnImmunityEnded;
        }
        
        if (damageAnimationCoroutine != null)
        {
            StopCoroutine(damageAnimationCoroutine);
        }
    }

    // Public API
    public bool IsPlayerImmune() => playerImmunity != null && playerImmunity.IsImmune;
    public bool IsPlayerFlying() => helicopterSystem != null && helicopterSystem.IsFlying;
    public float GetRemainingImmunityTime() => playerImmunity?.RemainingImmunityTime ?? 0f;
    public bool IsPlayingDamageAnimation() => isPlayingDamageAnimation;
}