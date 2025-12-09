using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WhiteCellAnimator : MonoBehaviour
{
    [Header("Lose Animation")]
    [SerializeField] private float loseAnimationDuration = 1f;
    [SerializeField] private AnimationCurve loseScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private Color loseFlashColor = Color.red;
    [SerializeField] private float loseShakeIntensity = 10f;
    
    [Header("Restore Animation")]
    [SerializeField] private float restoreAnimationDuration = 0.8f;
    [SerializeField] private AnimationCurve restoreScaleCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.6f, 1.2f),
        new Keyframe(1f, 1f)
    );
    [SerializeField] private Color restoreFlashColor = Color.green;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private WhiteCellUI parentUI;
    private Coroutine[] cellAnimationCoroutines;
    
    public void Initialize(WhiteCellUI parent)
    {
        parentUI = parent;
        int cellCount = parentUI.GetCellCount();
        cellAnimationCoroutines = new Coroutine[cellCount];
        
        if (enableDebugLogs)
            Debug.Log($"WhiteCellAnimator: Initialized with {cellCount} cells");
    }
    
    public void PlayLoseAnimation(int cellIndex)
    {
        if (enableDebugLogs)
            Debug.Log($"WhiteCellAnimator: PlayLoseAnimation called for cell {cellIndex}");
        StartCellAnimation(cellIndex, false);
    }
    
    public void PlayRestoreAnimation(int cellIndex)
    {
        if (enableDebugLogs)
            Debug.Log($"WhiteCellAnimator: PlayRestoreAnimation called for cell {cellIndex}");
        StartCellAnimation(cellIndex, true);
    }
    
    private void StartCellAnimation(int cellIndex, bool isRestore)
    {
        if (cellIndex < 0 || cellIndex >= cellAnimationCoroutines.Length) 
        {
            if (enableDebugLogs)
                Debug.LogWarning($"WhiteCellAnimator: Invalid cell index {cellIndex}");
            return;
        }
        
        if (enableDebugLogs)
            Debug.Log($"WhiteCellAnimator: Starting {(isRestore ? "restore" : "lose")} animation for cell {cellIndex}");
        
        // Stop existing animation
        if (cellAnimationCoroutines[cellIndex] != null)
        {
            StopCoroutine(cellAnimationCoroutines[cellIndex]);
            if (enableDebugLogs)
                Debug.Log($"WhiteCellAnimator: Stopped existing animation for cell {cellIndex}");
        }
        
        // Start new animation
        if (isRestore)
        {
            cellAnimationCoroutines[cellIndex] = StartCoroutine(RestoreAnimationCoroutine(cellIndex));
        }
        else
        {
            cellAnimationCoroutines[cellIndex] = StartCoroutine(LoseAnimationCoroutine(cellIndex));
        }
    }
    
    private IEnumerator LoseAnimationCoroutine(int cellIndex)
    {
        GameObject cell = parentUI.GetCell(cellIndex);
        if (cell == null) yield break;
        
        Image cellImage = cell.GetComponent<Image>();
        if (cellImage == null) yield break;
        
        Color originalColor = cellImage.color;
        Vector3 originalScale = cell.transform.localScale;
        Vector3 originalPosition = cell.transform.localPosition;
        
        float elapsedTime = 0f;
        while (elapsedTime < loseAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / loseAnimationDuration;
            
            // Flash effect
            if (progress < 0.5f)
            {
                Color flashColor = Color.Lerp(originalColor, loseFlashColor, 
                    Mathf.Sin(progress * Mathf.PI * 8));
                cellImage.color = flashColor;
            }
            else
            {
                cellImage.color = Color.Lerp(originalColor, Color.gray, (progress - 0.5f) * 2f);
            }
            
            // Scale animation
            float scaleMultiplier = loseScaleCurve.Evaluate(progress);
            cell.transform.localScale = originalScale * scaleMultiplier;
            
            // Shake effect
            if (progress < 0.6f)
            {
                float shakeProgress = progress / 0.6f;
                float shakeAmount = loseShakeIntensity * (1f - shakeProgress);
                Vector3 shake = new Vector3(
                    Random.Range(-shakeAmount, shakeAmount),
                    Random.Range(-shakeAmount, shakeAmount),
                    0
                );
                cell.transform.localPosition = originalPosition + shake;
            }
            else
            {
                cell.transform.localPosition = originalPosition;
            }
            
            yield return null;
        }
        
        // Reset and finalize
        cell.transform.localPosition = originalPosition;
        parentUI.SetCellActive(cellIndex, false);
        cellAnimationCoroutines[cellIndex] = null;
    }
    
    private IEnumerator RestoreAnimationCoroutine(int cellIndex)
    {
        GameObject cell = parentUI.GetCell(cellIndex);
        if (cell == null) yield break;
        
        Image cellImage = cell.GetComponent<Image>();
        if (cellImage == null) yield break;
        
        Vector3 originalScale = cell.transform.localScale;
        
        // Start from inactive state
        cellImage.color = Color.gray;
        cell.transform.localScale = Vector3.zero;
        
        float elapsedTime = 0f;
        while (elapsedTime < restoreAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / restoreAnimationDuration;
            
            // Color transition with flash
            if (progress < 0.3f)
            {
                Color flashColor = Color.Lerp(Color.gray, restoreFlashColor, 
                    Mathf.Sin(progress * Mathf.PI * 10));
                cellImage.color = flashColor;
            }
            else
            {
                cellImage.color = Color.Lerp(restoreFlashColor, Color.white, (progress - 0.3f) / 0.7f);
            }
            
            // Scale animation with bounce
            float scaleMultiplier = restoreScaleCurve.Evaluate(progress);
            
            // Add pulse effect
            if (progress > 0.5f)
            {
                float pulseProgress = (progress - 0.5f) / 0.5f;
                float pulse = 1f + (Mathf.Sin(pulseProgress * Mathf.PI * 4) * 0.1f * (1f - pulseProgress));
                scaleMultiplier *= pulse;
            }
            
            cell.transform.localScale = originalScale * scaleMultiplier;
            yield return null;
        }
        
        // Finalize
        parentUI.SetCellActive(cellIndex, true);
        cellAnimationCoroutines[cellIndex] = null;
    }
}