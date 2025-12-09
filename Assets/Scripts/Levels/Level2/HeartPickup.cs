using UnityEngine;
using UnityEngine.Serialization;

public class HeartPickup : MonoBehaviour
{
    [FormerlySerializedAs("heartValue")]
    [Header("Pickup Settings")]
    [SerializeField] private int duration = 1;
    [SerializeField] private AudioClip pickupSound;
    
    [Header("Visual Effects")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    
    private Vector3 startPosition;
    private AudioSource audioSource;
    
    private void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
    }
    
    private void Update()
    {
        // Simple bobbing animation
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger entered Heatlh " + other.gameObject.name);
        
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Only pick up if player isn't at max health
                if (playerHealth.CurrentHearts < playerHealth.MaxHearts)
                {
                    playerHealth.AddHeart(duration);
                }
                
                PlayPickupSound();
                Destroy(gameObject);
            }
        }
    }
    
    private void PlayPickupSound()
    {
        if (audioSource != null && pickupSound != null)
        {
            // Play sound and destroy after sound finishes
            audioSource.PlayOneShot(pickupSound);
            // Note: In a more complex system, you might want to use an audio manager
            // to handle cleanup of audio sources after they finish playing
        }
    }
}