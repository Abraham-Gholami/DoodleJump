using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

/// <summary>
/// Simple factory for creating bullet type buttons
/// </summary>
public class BulletButtonFactory
{
    private readonly GameObject buttonPrefab;
    private readonly GameObject highlightPrefab;
    private readonly bool useHighlightEffect;

    public BulletButtonFactory(GameObject buttonPrefab, GameObject highlightPrefab, bool useHighlightEffect)
    {
        this.buttonPrefab = buttonPrefab;
        this.highlightPrefab = highlightPrefab;
        this.useHighlightEffect = useHighlightEffect;
    }

    public BulletButton CreateButton(BulletTypeData bulletType, int index, Transform parent, Action<int> onClickCallback)
    {
        if (buttonPrefab == null) return null;

        // Create button object
        GameObject buttonObj = GameObject.Instantiate(buttonPrefab, parent);
        
        // Setup button data
        BulletButton bulletButton = new BulletButton
        {
            button = buttonObj.GetComponent<Button>(),
            icon = buttonObj.transform.Find("Icon")?.GetComponent<Image>(),
            nameText = buttonObj.transform.Find("NameText")?.GetComponent<TMP_Text>(),
            backgroundImage = buttonObj.GetComponent<Image>(),
            bulletTypeData = bulletType,
            index = index,
            rectTransform = buttonObj.GetComponent<RectTransform>(),
            targetScale = Vector3.one
        };

        // Setup highlight
        if (useHighlightEffect && highlightPrefab != null)
        {
            bulletButton.highlight = GameObject.Instantiate(highlightPrefab, buttonObj.transform);
            bulletButton.highlight.SetActive(false);
        }

        // Set content
        SetButtonContent(bulletButton, bulletType);
        
        // Setup click handler
        if (bulletButton.button != null)
        {
            int buttonIndex = index; // Capture for closure
            bulletButton.button.onClick.AddListener(() => onClickCallback(buttonIndex));
        }

        return bulletButton;
    }

    private void SetButtonContent(BulletButton bulletButton, BulletTypeData bulletType)
    {
        // Set icon
        if (bulletButton.icon != null && bulletType.buttonIcon != null)
        {
            bulletButton.icon.sprite = bulletType.buttonIcon;
        }

        // Set name text
        if (bulletButton.nameText != null)
        {
            bulletButton.nameText.text = bulletType.typeName;
        }
    }
}