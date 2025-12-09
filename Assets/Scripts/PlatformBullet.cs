using System.Collections;
using UnityEngine;

public class PlatformBullet : MonoBehaviour
{
    [SerializeField] private float duration = 1f;
    [SerializeField] private float convertDuration = 10f; // How long platforms stay converted
    
    private bool hasHit = false;
    
    private void Awake()
    {
        StartCoroutine(DelayedDestroyBullet());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("BULLET HIT ONE TIME PLATFORM....");
        
        //if (hasHit) return;
        
        // Only collide with OneTimePlatforms
        var oneTimePlatform = other.gameObject.GetComponent<OneTimePlatform>();
        if (oneTimePlatform != null)
        {
            hasHit = true;
            oneTimePlatform.ConvertToNormalPlatform(convertDuration);
            StopAllCoroutines();
            //Destroy(gameObject);
        }
    }

    private IEnumerator DelayedDestroyBullet()
    {
        yield return new WaitForSeconds(duration);
        if(gameObject != null)
            Destroy(gameObject);
    }
}