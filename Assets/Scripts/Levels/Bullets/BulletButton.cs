using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Data class representing a bullet type button
/// </summary>
[System.Serializable]
public class BulletButton
{
    public Button button;
    public Image icon;
    public Image backgroundImage;
    public TMP_Text nameText;
    public GameObject highlight;
    public BulletTypeData bulletTypeData;
    public int index;
    public Vector3 targetScale;
    public RectTransform rectTransform;

    public void UpdateVisuals(bool isSelected, float selectedScale, float unselectedScale)
    {
        if (bulletTypeData == null) return;

        // Update scale target
        targetScale = Vector3.one * (isSelected ? selectedScale : unselectedScale);

        // Update colors
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? bulletTypeData.highlightColor : bulletTypeData.normalColor;
        }

        // Update text color
        if (nameText != null)
        {
            nameText.color = isSelected ? bulletTypeData.highlightColor : bulletTypeData.normalColor;
        }

        // Update highlight
        if (highlight != null)
        {
            highlight.SetActive(isSelected);
        }

        // Update button colors
        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = isSelected ? bulletTypeData.highlightColor : bulletTypeData.normalColor;
            colors.selectedColor = isSelected ? bulletTypeData.highlightColor : bulletTypeData.normalColor;
            button.colors = colors;
        }
    }

    public void AnimateScale(float animationSpeed)
    {
        if (button != null)
        {
            button.transform.localScale = Vector3.Lerp(
                button.transform.localScale,
                targetScale,
                Time.deltaTime * animationSpeed
            );
        }
    }

    public void SetInitialScale(float scale)
    {
        targetScale = Vector3.one * scale;
        if (button != null)
        {
            button.transform.localScale = targetScale;
        }
    }
}