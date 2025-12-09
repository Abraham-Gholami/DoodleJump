// ============= BULLET TYPE SCRIPTABLE OBJECT =============
using UnityEngine;

[CreateAssetMenu(fileName = "New Bullet Type", menuName = "Game/Bullet Type", order = 1)]
public class BulletTypeData : ScriptableObject
{
    [Header("Bullet Information")]
    public string typeName;
    [TextArea(2, 4)]
    public string description;
    
    [Header("Bullet Assets")]
    public GameObject bulletPrefab;
    public Sprite buttonIcon;
    
    [Header("Visual Settings")]
    public Color highlightColor = Color.white;
    public Color normalColor = Color.gray;
    
    private void OnValidate()
    {
        // Auto-generate name from asset name if empty
        if (string.IsNullOrEmpty(typeName))
        {
            typeName = name;
        }
    }
}