using UnityEngine;

public class MainMenuPlayerDebug : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== MAIN MENU PLAYER DEBUG START ===");
        LogComponents();
        InvokeRepeating("LogState", 0f, 1f);
    }
    
    private void LogComponents()
    {
        var rb = GetComponent<Rigidbody2D>();
        var col = GetComponent<BoxCollider2D>();
        var anim = GetComponent<PlayerAnimation>();
        var sr = GetComponent<SpriteRenderer>();
        
        Debug.Log($"Rigidbody2D: {rb != null} | Simulated: {rb?.simulated} | Gravity: {rb?.gravityScale}");
        Debug.Log($"BoxCollider2D: {col != null} | Enabled: {col?.enabled}");
        Debug.Log($"PlayerAnimation: {anim != null}");
        Debug.Log($"SpriteRenderer: {sr != null} | Sprite: {sr?.sprite?.name}");
        Debug.Log($"SettingsDataHolder.Instance: {SettingsDataHolder.Instance != null}");
    }
    
    private void LogState()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Debug.Log($"Position: {transform.position} | Velocity: {rb.linearVelocity}");
        }
    }
    
    private void OnDestroy()
    {
        CancelInvoke();
    }
}