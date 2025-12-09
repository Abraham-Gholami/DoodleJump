using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float duration = 1f;
    private bool hasHit = false; // Prevent multiple hits
    
    private void Awake()
    {
        StartCoroutine(DelayedDestroyBullet());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        
        // Check for enemy
        var enemy = other.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            hasHit = true;
            enemy.Dead();
            StopAllCoroutines();
            Destroy(gameObject);
            return;
        }
    }

    private IEnumerator DelayedDestroyBullet()
    {
        yield return new WaitForSeconds(duration);
        if(gameObject != null)
            Destroy(gameObject);
    }
}