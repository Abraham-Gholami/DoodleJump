// ============= IMMUNITY UI (OPTIONAL) =============
using UnityEngine;
using UnityEngine.UI;

public class ImmunityUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject immunityPanel;
    [SerializeField] private Image immunityFillBar;
    [SerializeField] private Text immunityTimeText;
    [SerializeField] private Image immunityIcon;
    
    [Header("Visual Settings")]
    [SerializeField] private Color immunityBarColor = Color.cyan;
    [SerializeField] private Color lowTimeColor = Color.red;
    [SerializeField] private float lowTimeThreshold = 2f;
    [SerializeField] private bool showTimeText = true;
    [SerializeField] private bool animateIcon = true;
    
    private PlayerImmunity playerImmunity;
    private float originalIconScale;
    
    private void Awake()
    {
        playerImmunity = FindObjectOfType<PlayerImmunity>();
        
        if (playerImmunity == null)
        {
            Debug.LogWarning("ImmunityUI: No PlayerImmunity component found!");
            return;
        }
        
        // Subscribe to immunity events
        playerImmunity.OnImmunityStarted += OnImmunityStarted;
        playerImmunity.OnImmunityEnded += OnImmunityEnded;
        playerImmunity.OnImmunityTimeChanged += OnImmunityTimeChanged;
        
        // Store original icon scale for animation
        if (immunityIcon != null)
        {
            originalIconScale = immunityIcon.transform.localScale.x;
        }
        
        // Initially hide the immunity panel
        if (immunityPanel != null)
        {
            immunityPanel.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (playerImmunity != null)
        {
            playerImmunity.OnImmunityStarted -= OnImmunityStarted;
            playerImmunity.OnImmunityEnded -= OnImmunityEnded;
            playerImmunity.OnImmunityTimeChanged -= OnImmunityTimeChanged;
        }
    }
    
    private void OnImmunityStarted(float duration)
    {
        if (immunityPanel != null)
        {
            immunityPanel.SetActive(true);
        }
        
        // Reset colors
        if (immunityFillBar != null)
        {
            immunityFillBar.color = immunityBarColor;
        }
    }
    
    private void OnImmunityEnded()
    {
        if (immunityPanel != null)
        {
            immunityPanel.SetActive(false);
        }
    }
    
    private void OnImmunityTimeChanged(float remainingTime)
    {
        // Update fill bar
        if (immunityFillBar != null && playerImmunity != null)
        {
            float progress = playerImmunity.ImmunityProgress;
            immunityFillBar.fillAmount = progress;
            
            // Change color when time is low
            if (remainingTime <= lowTimeThreshold)
            {
                immunityFillBar.color = Color.Lerp(lowTimeColor, immunityBarColor, remainingTime / lowTimeThreshold);
            }
        }
        
        // Update time text
        if (immunityTimeText != null && showTimeText)
        {
            immunityTimeText.text = $"{remainingTime:F1}s";
        }
        
        // Animate icon
        if (immunityIcon != null && animateIcon && remainingTime <= lowTimeThreshold)
        {
            float pulse = Mathf.Sin(Time.time * 10f) * 0.1f + 1f;
            immunityIcon.transform.localScale = Vector3.one * (originalIconScale * pulse);
        }
        else if (immunityIcon != null)
        {
            immunityIcon.transform.localScale = Vector3.one * originalIconScale;
        }
    }
}