// ============= GLOW EFFECT COMPONENT =============
using UnityEngine;

public class GlowEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private Color glowColor = Color.cyan;
    [SerializeField] private bool rotateEffect = true;
    [SerializeField] private float rotationSpeed = 45f;
    
    private SpriteRenderer spriteRenderer;
    private ParticleSystem particles;
    private Vector3 originalScale;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        particles = GetComponent<ParticleSystem>();
        originalScale = transform.localScale;
        
        // Set up sprite renderer if present
        if (spriteRenderer != null)
        {
            spriteRenderer.color = glowColor;
        }
        
        // Set up particle system if present
        if (particles != null)
        {
            var main = particles.main;
            main.startColor = glowColor;
        }
    }
    
    private void Update()
    {
        // Pulsing scale effect
        float pulse = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        transform.localScale = originalScale * pulse;
        
        // Rotation effect
        if (rotateEffect)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
        
        // Optional: Fade alpha based on pulse
        if (spriteRenderer != null)
        {
            Color color = glowColor;
            color.a = pulse * 0.5f; // Semi-transparent pulsing
            spriteRenderer.color = color;
        }
    }
    
    private void OnEnable()
    {
        // Reset scale when enabled
        transform.localScale = originalScale;
        
        // Start particle system if present
        if (particles != null)
        {
            particles.Play();
        }
    }
    
    private void OnDisable()
    {
        // Stop particle system if present
        if (particles != null)
        {
            particles.Stop();
        }
    }
}