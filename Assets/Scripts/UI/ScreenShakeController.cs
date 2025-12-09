using System.Collections;
using UnityEngine;

public class ScreenShakeController : MonoBehaviour
{
    [Header("Screen Effects")]
    [SerializeField] private bool enableScreenEffects = true;
    [SerializeField] private float screenShakeDuration = 0.3f;
    [SerializeField] private float screenShakeIntensity = 2f;
    
    private Camera mainCamera;
    private Coroutine currentShakeCoroutine;
    
    public void Initialize()
    {
        mainCamera = Camera.main;
    }
    
    public void TriggerShake()
    {
        if (!enableScreenEffects) return;
        
        // Stop any existing shake
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
        }
        
        // Start new shake
        currentShakeCoroutine = StartCoroutine(ScreenShakeCoroutine());
    }
    
    private IEnumerator ScreenShakeCoroutine()
    {
        if (mainCamera == null) 
        {
            currentShakeCoroutine = null;
            yield break;
        }
        
        Vector3 originalPosition = mainCamera.transform.localPosition;
        float elapsedTime = 0f;
        
        while (elapsedTime < screenShakeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / screenShakeDuration;
            float intensity = screenShakeIntensity * (1f - progress);
            
            Vector3 shake = new Vector3(
                Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity),
                0
            );
            
            mainCamera.transform.localPosition = originalPosition + shake;
            yield return null;
        }
        
        mainCamera.transform.localPosition = originalPosition;
        currentShakeCoroutine = null;
    }
}