using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clean BulletTypeUI using composition pattern
/// Delegates specific responsibilities to focused components
/// </summary>
public class BulletTypeUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonContainer; // For 3+ bullets
    
    [Header("Visual Settings")]
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float unselectedScale = 0.8f;
    [SerializeField] private float scaleAnimationSpeed = 5f;
    
    [Header("Corner Settings (2 bullets only)")]
    [SerializeField] private float cornerMarginX = 50f;
    [SerializeField] private float cornerMarginY = 50f;
    [SerializeField] private bool useSafeArea = true;
    
    [Header("Highlight Settings")]
    [SerializeField] private bool useHighlightEffect = true;
    [SerializeField] private GameObject highlightPrefab;
    
    // Components
    private BulletTypeManager bulletTypeManager;
    private BulletButtonFactory buttonFactory;
    private CornerPositioner cornerPositioner;
    
    // Data
    private List<BulletButton> bulletButtons = new List<BulletButton>();
    private bool isUsingCornerLayout = false;

    private void Awake()
    {
        InitializeComponents();
        SetupEventSubscription();
    }

    private void Start()
    {
        CreateUI();
    }

    private void Update()
    {
        AnimateButtons();
        UpdateCornerLayoutIfNeeded();
    }

    private void InitializeComponents()
    {
        bulletTypeManager = FindObjectOfType<BulletTypeManager>();
        if (bulletTypeManager == null)
        {
            Debug.LogError("BulletTypeUI: BulletTypeManager not found!");
            return;
        }

        // Create component instances
        buttonFactory = new BulletButtonFactory(buttonPrefab, highlightPrefab, useHighlightEffect);
        cornerPositioner = new CornerPositioner(cornerMarginX, cornerMarginY, useSafeArea, GetComponentInParent<Canvas>());
    }

    private void SetupEventSubscription()
    {
        if (bulletTypeManager != null)
        {
            bulletTypeManager.OnBulletTypeChanged += OnBulletTypeChanged;
        }
    }

    private void CreateUI()
    {
        ClearExistingButtons();
        
        int bulletCount = bulletTypeManager.AvailableBulletTypes.Count;
        
        switch (bulletCount)
        {
            case 0:
                HandleNoBullets();
                break;
            case 1:
                HandleSingleBullet();
                break;
            case 2:
                HandleTwoBullets();
                break;
            default:
                HandleMultipleBullets();
                break;
        }

        gameObject.SetActive(bulletButtons.Count > 0);
        UpdateVisuals();
    }

    private void HandleNoBullets()
    {
        Debug.LogWarning("BulletTypeUI: No bullet types available!");
    }

    private void HandleSingleBullet()
    {
        Debug.Log("BulletTypeUI: Single bullet type - hiding UI");
    }

    private void HandleTwoBullets()
    {
        Debug.Log("BulletTypeUI: Two bullet types - using corner layout");
        isUsingCornerLayout = true;
        CreateButtonsForCornerLayout();
    }

    private void HandleMultipleBullets()
    {
        Debug.Log($"BulletTypeUI: {bulletTypeManager.AvailableBulletTypes.Count} bullet types - using container layout");
        isUsingCornerLayout = false;
        CreateButtonsForContainerLayout();
    }

    private void CreateButtonsForCornerLayout()
    {
        for (int i = 0; i < 2; i++)
        {
            BulletButton button = CreateButton(i, transform);
            button.SetInitialScale(unselectedScale);
            bulletButtons.Add(button);
        }
        
        cornerPositioner.SetupCornerButtons(bulletButtons);
    }

    private void CreateButtonsForContainerLayout()
    {
        if (buttonContainer == null)
        {
            Debug.LogError("BulletTypeUI: Button container missing for container layout!");
            return;
        }

        for (int i = 0; i < bulletTypeManager.AvailableBulletTypes.Count; i++)
        {
            BulletButton button = CreateButton(i, buttonContainer);
            button.SetInitialScale(unselectedScale);
            bulletButtons.Add(button);
        }
    }

    private BulletButton CreateButton(int index, Transform parent)
    {
        BulletTypeData bulletType = bulletTypeManager.AvailableBulletTypes[index];
        return buttonFactory.CreateButton(bulletType, index, parent, OnButtonClicked);
    }

    private void AnimateButtons()
    {
        foreach (var button in bulletButtons)
        {
            button.AnimateScale(scaleAnimationSpeed);
        }
    }

    private void UpdateCornerLayoutIfNeeded()
    {
        if (isUsingCornerLayout)
        {
            cornerPositioner.UpdatePositions(bulletButtons);
        }
    }

    private void OnButtonClicked(int index)
    {
        bulletTypeManager.SetBulletType(index);
    }

    private void OnBulletTypeChanged(BulletTypeData newBulletType, int newIndex)
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        int currentIndex = bulletTypeManager.CurrentBulletTypeIndex;
        
        for (int i = 0; i < bulletButtons.Count; i++)
        {
            bool isSelected = (i == currentIndex);
            bulletButtons[i].UpdateVisuals(isSelected, selectedScale, unselectedScale);
        }
    }

    private void ClearExistingButtons()
    {
        foreach (var button in bulletButtons)
        {
            if (button.button != null)
            {
                DestroyImmediate(button.button.gameObject);
            }
        }
        bulletButtons.Clear();
        isUsingCornerLayout = false;
    }

    private void OnDestroy()
    {
        if (bulletTypeManager != null)
        {
            bulletTypeManager.OnBulletTypeChanged -= OnBulletTypeChanged;
        }
    }

    // Public API
    public void RefreshUI() => CreateUI();
    public bool IsUsingCornerLayout => isUsingCornerLayout;
    public int ButtonCount => bulletButtons.Count;
}