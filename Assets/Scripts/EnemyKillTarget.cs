using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EnemyKillTarget
{
    [Header("Target Configuration")]
    public EnemyType enemyType;
    public int requiredKills;
    
    [Header("UI References (Optional - for GameController)")]
    public Image scoreFiller;
    public Text scoreText;
    
    [Header("Runtime Data")]
    [SerializeField] private int currentKills = 0;
    
    // Properties
    public int CurrentKills => currentKills;
    public bool IsCompleted => currentKills >= requiredKills;
    public float Progress => requiredKills > 0 ? (float)currentKills / requiredKills : 0f;
    
    // Methods
    public void AddKill()
    {
        if (currentKills < requiredKills)
        {
            currentKills++;
            UpdateUI();
        }
    }
    
    public void ResetProgress()
    {
        currentKills = 0;
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{currentKills} / {requiredKills}";
        }
        
        if (scoreFiller != null)
        {
            scoreFiller.fillAmount = Progress;
        }
    }
    
    // Get display name for UI
    public string GetDisplayName()
    {
        switch (enemyType)
        {
            case EnemyType.SickleCell:
                return "Sickle Cells";
            case EnemyType.Fungi:
                return "Fungi";
            case EnemyType.Bacteria:
                return "Bacteria";
            case EnemyType.Virus:
                return "Viruses";
            default:
                return enemyType.ToString();
        }
    }
}