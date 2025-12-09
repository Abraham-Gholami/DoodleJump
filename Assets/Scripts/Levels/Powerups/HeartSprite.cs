using UnityEngine;
using UnityEngine.UI;

public class HeartSprite : MonoBehaviour
{
    private Image heartImage;
    private Sprite fullSprite;
    private Sprite emptySprite;
    private bool isFull = true;
    
    public void Initialize(Sprite fullHeartSprite, Sprite emptyHeartSprite)
    {
        heartImage = GetComponent<Image>();
        if (heartImage == null)
        {
            Debug.LogError("HeartSprite: No Image component found!");
            return;
        }
        
        fullSprite = fullHeartSprite;
        emptySprite = emptyHeartSprite;
        
        // Set initial state to full
        SetFull();
    }
    
    public void SetFull()
    {
        if (heartImage != null && fullSprite != null)
        {
            heartImage.sprite = fullSprite;
            isFull = true;
        }
    }
    
    public void SetEmpty()
    {
        if (heartImage != null && emptySprite != null)
        {
            heartImage.sprite = emptySprite;
            isFull = false;
        }
    }
    
    public bool IsFull()
    {
        return isFull;
    }
}