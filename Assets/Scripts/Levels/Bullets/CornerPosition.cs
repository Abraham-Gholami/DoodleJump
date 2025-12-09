using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles positioning buttons at screen corners with safe area support
/// </summary>
public class CornerPositioner
{
    private readonly float cornerMarginX;
    private readonly float cornerMarginY;
    private readonly bool useSafeArea;
    private readonly Canvas parentCanvas;

    public CornerPositioner(float marginX, float marginY, bool useSafeArea, Canvas parentCanvas)
    {
        this.cornerMarginX = marginX;
        this.cornerMarginY = marginY;
        this.useSafeArea = useSafeArea;
        this.parentCanvas = parentCanvas;
    }

    public void SetupCornerButtons(List<BulletButton> buttons)
    {
        if (buttons.Count != 2) return;

        // Setup anchors for corner positioning
        SetupButtonAnchor(buttons[0], true);  // Bottom-left
        SetupButtonAnchor(buttons[1], false); // Bottom-right
        
        // Set initial positions
        UpdatePositions(buttons);
    }

    public void UpdatePositions(List<BulletButton> buttons)
    {
        if (buttons.Count != 2) return;

        Vector2 safeAreaOffset = GetSafeAreaOffset();

        // Position bottom-left
        if (buttons[0].rectTransform != null)
        {
            buttons[0].rectTransform.anchoredPosition = new Vector2(
                cornerMarginX + safeAreaOffset.x,
                cornerMarginY + safeAreaOffset.y
            );
        }

        // Position bottom-right
        if (buttons[1].rectTransform != null)
        {
            buttons[1].rectTransform.anchoredPosition = new Vector2(
                -(cornerMarginX + safeAreaOffset.x),
                cornerMarginY + safeAreaOffset.y
            );
        }
    }

    private void SetupButtonAnchor(BulletButton button, bool isLeft)
    {
        if (button.rectTransform == null) return;

        if (isLeft) // Bottom-left
        {
            button.rectTransform.anchorMin = new Vector2(0, 0);
            button.rectTransform.anchorMax = new Vector2(0, 0);
            button.rectTransform.pivot = new Vector2(0, 0);
        }
        else // Bottom-right
        {
            button.rectTransform.anchorMin = new Vector2(1, 0);
            button.rectTransform.anchorMax = new Vector2(1, 0);
            button.rectTransform.pivot = new Vector2(1, 0);
        }
    }

    private Vector2 GetSafeAreaOffset()
    {
        if (!useSafeArea) return Vector2.zero;

        Rect safeArea = Screen.safeArea;
        float leftMargin = safeArea.x;
        float bottomMargin = safeArea.y;

        // Convert to Canvas coordinates
        if (parentCanvas != null)
        {
            float scaleFactor = parentCanvas.scaleFactor;
            leftMargin /= scaleFactor;
            bottomMargin /= scaleFactor;
        }

        return new Vector2(Mathf.Max(0, leftMargin), Mathf.Max(0, bottomMargin));
    }
}