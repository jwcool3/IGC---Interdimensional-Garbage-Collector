using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/// <summary>
/// Represents a single compartment/module of the player's ship.
/// Each compartment provides specific bonuses and can be upgraded.
/// </summary>
public class ShipCompartment : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Basic Info")]
    [Tooltip("Unique identifier for this compartment type")]
    [SerializeField] private string compartmentId;

    [Tooltip("Display name shown in the UI")]
    [SerializeField] private string displayName;
    
    [Tooltip("Description of the compartment's function")]
    [TextArea(3, 5)]
    [SerializeField] private string description;
    
    [Tooltip("Type of compartment (determines effects)")]
    [SerializeField] private CompartmentType type;
    
    [Header("Upgrade Properties")]
    [Tooltip("Current level of this compartment")]
    [SerializeField] private int currentLevel = 1;
    
    [Tooltip("Maximum level this compartment can reach")]
    [SerializeField] private int maxLevel = 5;
    
    [Tooltip("Base Recycling Points cost (increases with level)")]
    [SerializeField] private float baseRPCost = 100f;
    
    [Tooltip("Base Dimensional Potential cost (increases with level)")]
    [SerializeField] private float baseDPCost = 10f;
    
    [Tooltip("Multiplier for cost increases with each level")]
    [SerializeField] private float costMultiplier = 1.5f;
    
    [Header("Visual Elements")]
    [Tooltip("Reference to this compartment's sprite renderer")]
    [SerializeField] private SpriteRenderer compartmentRenderer;
    
    [Tooltip("Sprites for each level (index 0 = level 1)")]
    [SerializeField] private Sprite[] levelSprites; 
    
    [Tooltip("Visual indicator shown when compartment is selected")]
    [SerializeField] private GameObject selectionIndicator;
    
    [Header("Effects")]
    [Tooltip("Level-based effect multipliers")]
    [SerializeField] private float[] effectMultipliers = new float[] { 1f, 1.25f, 1.5f, 1.75f, 2f };
    
    [Tooltip("Special unlocks at specific levels (e.g., new features)")]
    [SerializeField] private List<SpecialUnlock> specialUnlocks = new List<SpecialUnlock>();
    
    // Public properties for external access
    public string Id => compartmentId;
    public string DisplayName => displayName;
    public string Description => description;
    public CompartmentType Type => type;
    public int CurrentLevel => currentLevel;
    public int MaxLevel => maxLevel;
    
    // Event fired when compartment is clicked
    public System.Action<ShipCompartment> OnCompartmentClicked;
    
    // Property to get current effect multiplier
    public float CurrentEffectMultiplier => 
        currentLevel <= effectMultipliers.Length ? 
        effectMultipliers[currentLevel - 1] : 
        effectMultipliers[effectMultipliers.Length - 1];
    
    /// <summary>
    /// Gets the base effect value for this compartment type
    /// </summary>
    public float GetBaseEffectValue()
    {
        switch (type)
        {
            case CompartmentType.Engine:
                return 0.1f; // 10% base collection rate increase
            case CompartmentType.Lab:
                return 0.15f; // 15% base recycling efficiency
            case CompartmentType.Storage:
                return 50f; // 50 base storage capacity
            case CompartmentType.Bridge:
                return 0.2f; // 20% base command efficiency
            case CompartmentType.Recycling:
                return 0.25f; // 25% base recycling speed
            case CompartmentType.Stabilizer:
                return 0.1f; // 10% base stability increase
            case CompartmentType.Communications:
                return 0.15f; // 15% base communication range
            case CompartmentType.Scanner:
                return 0.2f; // 20% base scan efficiency
            default:
                return 0f;
        }
    }

    /// <summary>
    /// Gets the total effect value including level multipliers
    /// </summary>
    public float GetTotalEffectValue()
    {
        return GetBaseEffectValue() * CurrentEffectMultiplier;
    }
    
    /// <summary>
    /// Special unlocks that occur at specific levels
    /// </summary>
    [System.Serializable]
    public class SpecialUnlock
    {
        public int unlockLevel;
        public string unlockName;
        [TextArea(2, 3)]
        public string unlockDescription;
    }
    
    private void Start()
    {
        // Register with ShipManager if available
        if (ShipManager.Instance != null && !string.IsNullOrEmpty(compartmentId))
        {
            ShipManager.Instance.RegisterCompartment(this);
        }
        
        // Ensure visual state matches current level
        UpdateVisuals();
    }
    
    private void OnEnable()
    {
        // Refresh visuals when enabled
        UpdateVisuals();
    }
    
    /// <summary>
    /// Upgrades the compartment to the next level
    /// </summary>
    /// <returns>True if upgrade was successful</returns>
    public bool UpgradeLevel()
    {
        if (currentLevel >= maxLevel)
            return false;
            
        currentLevel++;
        UpdateVisuals();
        ApplyEffects();
        
        // Debug info
        Debug.Log($"Upgraded {displayName} to level {currentLevel}");
        
        return true;
    }
    
    /// <summary>
    /// Gets the current upgrade cost
    /// </summary>
    /// <param name="isDimensionalPotential">True to get DP cost, false for RP cost</param>
    /// <returns>The cost to upgrade to the next level</returns>
    public float GetUpgradeCost(bool isDimensionalPotential)
    {
        // Return 0 if already at max level
        if (currentLevel >= maxLevel)
            return 0f;
            
        float baseCost = isDimensionalPotential ? baseDPCost : baseRPCost;
        return Mathf.Round(baseCost * Mathf.Pow(costMultiplier, currentLevel - 1));
    }
    
    /// <summary>
    /// Sets the selection state for this compartment
    /// </summary>
    /// <param name="selected">Whether this compartment should be selected</param>
    public void SetSelected(bool selected)
    {
        if (selectionIndicator != null)
            selectionIndicator.SetActive(selected);
            
        // Optional: Add animation or effects for selection
        if (selected)
        {
            // You could add a pulse animation here
            // For example: StartCoroutine(PulseAnimation());
        }
    }
    
    /// <summary>
    /// Updates the visual representation based on current level
    /// </summary>
    private void UpdateVisuals()
    {
        // Update sprite based on level
        if (compartmentRenderer != null && levelSprites != null && levelSprites.Length > 0)
        {
            int spriteIndex = Mathf.Min(currentLevel - 1, levelSprites.Length - 1);
            if (spriteIndex >= 0 && spriteIndex < levelSprites.Length && levelSprites[spriteIndex] != null)
            {
                compartmentRenderer.sprite = levelSprites[spriteIndex];
            }
            else
            {
                Debug.LogWarning($"Missing level sprite for {displayName} at level {currentLevel}");
            }
        }
    }
    
    /// <summary>
    /// Apply the compartment's effects based on its type and level
    /// </summary>
    public void ApplyEffects()
    {
        if (ShipManager.Instance == null)
            return;
            
        // Different effects based on compartment type
        switch (type)
        {
            case CompartmentType.Engine:
                ApplyEngineEffects();
                break;
                
            case CompartmentType.Lab:
                ApplyLabEffects();
                break;
                
            case CompartmentType.Storage:
                ApplyStorageEffects();
                break;
                
            case CompartmentType.Bridge:
                ApplyBridgeEffects();
                break;
                
            case CompartmentType.Recycling:
                ApplyRecyclingEffects();
                break;
                
            case CompartmentType.Stabilizer:
                ApplyStabilizerEffects();
                break;
                
            case CompartmentType.Communications:
                ApplyCommunicationsEffects();
                break;
                
            case CompartmentType.Scanner:
                ApplyScannerEffects();
                break;
        }
        
        // Check for special unlocks at this level
        CheckSpecialUnlocks();
    }
    
    /// <summary>
    /// Check for any special features unlocked at the current level
    /// </summary>
    private void CheckSpecialUnlocks()
    {
        foreach (var unlock in specialUnlocks)
        {
            if (unlock.unlockLevel == currentLevel)
            {
                // Notify the player about the unlock
                Debug.Log($"Unlocked: {unlock.unlockName} - {unlock.unlockDescription}");
                
                // You could trigger a UI notification here
                // For example: ShipManager.Instance.ShowUnlockNotification(unlock);
            }
        }
    }
    
    #region Compartment Type Specific Effects
    
    private void ApplyEngineEffects()
    {
        // Get ProbeManager instance with fallback
        ProbeManager probeManager = ProbeManager.Instance;
        if (probeManager == null)
        {
            probeManager = FindFirstObjectByType<ProbeManager>();
            if (probeManager == null)
            {
                Debug.LogWarning($"Engine Room Lv{currentLevel}: Cannot apply effects - ProbeManager not found");
                return;
            }
        }
        
        // Apply effects since we have a valid reference
        float speedBonus = GetTotalEffectValue();
        
        // Apply to all probes
        foreach (var probe in probeManager.GetActiveProbes())
        {
            probe.collectionRate *= (1f + speedBonus);
        }
        
        // Update the collection interval
        probeManager.UpdateCollectionInterval();
        
        Debug.Log($"Engine Room Lv{currentLevel}: Increased probe collection rate by {speedBonus*100:F0}%");
    }
    
    private void ApplyLabEffects()
    {
        // Get ResourceManager instance with fallback
        ResourceManager resourceManager = ResourceManager.Instance;
        if (resourceManager == null)
        {
            resourceManager = FindFirstObjectByType<ResourceManager>();
            if (resourceManager == null)
            {
                Debug.LogWarning($"Research Lab Lv{currentLevel}: Cannot apply effects - ResourceManager not found");
                return;
            }
        }
        
        float recyclingBonus = GetTotalEffectValue();
        resourceManager.SetRecyclingMultiplier(1f + recyclingBonus);
        
        Debug.Log($"Research Lab Lv{currentLevel}: Increased recycling efficiency by {recyclingBonus*100:F0}%");
    }
    
    private void ApplyStorageEffects()
    {
        // Get WasteInventoryManager instance with fallback
        WasteInventoryManager inventoryManager = WasteInventoryManager.Instance;
        if (inventoryManager == null)
        {
            inventoryManager = FindFirstObjectByType<WasteInventoryManager>();
            if (inventoryManager == null)
            {
                Debug.LogWarning($"Storage Bay Lv{currentLevel}: Cannot apply effects - WasteInventoryManager not found");
                return;
            }
        }
        
        int baseCapacityBonus = 10;
        int capacityBonus = Mathf.RoundToInt(baseCapacityBonus * CurrentEffectMultiplier);
        inventoryManager.IncreaseCapacity(capacityBonus);
        
        Debug.Log($"Storage Bay Lv{currentLevel}: Increased inventory capacity by {capacityBonus} slots");
    }
    
    private void ApplyBridgeEffects()
    {
        // Get LocationManager instance with fallback
        LocationManager locationManager = LocationManager.Instance;
        if (locationManager == null)
        {
            locationManager = FindFirstObjectByType<LocationManager>();
            if (locationManager == null)
            {
                Debug.LogWarning($"Bridge Lv{currentLevel}: Cannot apply effects - LocationManager not found");
                return;
            }
        }
        
        float discoveryBonus = GetTotalEffectValue();
        locationManager.SetDiscoveryBonus(discoveryBonus);
        
        Debug.Log($"Bridge Lv{currentLevel}: Improved location discovery rate by {discoveryBonus*100:F0}%");
    }
    
    private void ApplyRecyclingEffects()
    {
        // Get ResourceManager instance with fallback
        ResourceManager resourceManager = ResourceManager.Instance;
        if (resourceManager == null)
        {
            resourceManager = FindFirstObjectByType<ResourceManager>();
            if (resourceManager == null)
            {
                Debug.LogWarning($"Recycling Center Lv{currentLevel}: Cannot apply effects - ResourceManager not found");
                return;
            }
        }
        
        float contaminationReduction = GetTotalEffectValue();
        resourceManager.SetContaminationReductionModifier(contaminationReduction);
        
        Debug.Log($"Recycling Center Lv{currentLevel}: Reduced contamination by {contaminationReduction*100:F0}%");
    }
    
    private void ApplyStabilizerEffects()
    {
        // Get WasteGenerator instance with fallback
        WasteGenerator wasteGenerator = WasteGenerator.Instance;
        if (wasteGenerator == null)
        {
            wasteGenerator = FindFirstObjectByType<WasteGenerator>();
            if (wasteGenerator == null)
            {
                Debug.LogWarning($"Stabilizer Lv{currentLevel}: Cannot apply effects - WasteGenerator not found");
                return;
            }
        }
        
        float stabilityBonus = GetTotalEffectValue();
        wasteGenerator.SetStabilityModifier(stabilityBonus);
        
        Debug.Log($"Stabilizer Lv{currentLevel}: Increased waste stability by {stabilityBonus*100:F0}%");
    }
    
    private void ApplyCommunicationsEffects()
    {
        // Get WasteGenerator instance with fallback
        WasteGenerator wasteGenerator = WasteGenerator.Instance;
        if (wasteGenerator == null)
        {
            wasteGenerator = FindFirstObjectByType<WasteGenerator>();
            if (wasteGenerator == null)
            {
                Debug.LogWarning($"Communications Lv{currentLevel}: Cannot apply effects - WasteGenerator not found");
                return;
            }
        }
        
        float rarityBonus = GetTotalEffectValue();
        wasteGenerator.SetRarityModifier(rarityBonus);
        
        Debug.Log($"Communications Lv{currentLevel}: Increased rare waste chance by {rarityBonus*100:F0}%");
    }
    
    private void ApplyScannerEffects()
    {
        // Get GameManager instance with fallback
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogWarning($"Scanner Lv{currentLevel}: Cannot apply effects - GameManager not found");
                return;
            }
        }
        
        float detailLevel = GetTotalEffectValue();
        gameManager.SetWasteDetailLevel(detailLevel);
        
        Debug.Log($"Scanner Lv{currentLevel}: Enhanced waste property visibility to {detailLevel*100:F0}%");
    }
    
    #endregion
    
    /// <summary>
    /// Handle Unity pointer clicks
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Clicked on compartment: {DisplayName}");
        
        // Notify the ShipManager
        if (ShipManager.Instance != null)
        {
            ShipManager.Instance.SelectCompartment(this);
        }
        
        // Notify any listeners
        OnCompartmentClicked?.Invoke(this);
    }
    
    /// <summary>
    /// Handle hover/mouse enter events
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"Mouse entered compartment: {DisplayName}");
        
        // Optional: Highlight the compartment
        // You could change its color or scale slightly
    }
    
    /// <summary>
    /// Handle mouse exit events
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"Mouse exited compartment: {DisplayName}");
        
        // Optional: Remove highlight
    }
    
    /// <summary>
    /// Save this compartment's state
    /// </summary>
    /// <returns>Data to be saved</returns>
    public CompartmentSaveData GetSaveData()
    {
        return new CompartmentSaveData
        {
            id = compartmentId,
            level = currentLevel
        };
    }
    
    /// <summary>
    /// Restore this compartment from saved state
    /// </summary>
    /// <param name="data">The saved data</param>
    public void LoadFromSaveData(CompartmentSaveData data)
    {
        // Only load if IDs match
        if (data.id == compartmentId)
        {
            // Set level directly (don't use UpgradeLevel to avoid side effects)
            currentLevel = Mathf.Clamp(data.level, 1, maxLevel);
            UpdateVisuals();
        }
    }
    
    /// <summary>
    /// Get a description of the effect at the current level
    /// </summary>
    public string GetEffectDescription()
    {
        float totalEffect = GetTotalEffectValue();
        
        switch (type)
        {
            case CompartmentType.Engine:
                return $"Increases probe collection rate by {totalEffect * 100:F0}%";
                
            case CompartmentType.Lab:
                return $"Improves recycling efficiency by {totalEffect * 100:F0}%";
                
            case CompartmentType.Storage:
                return $"Provides {totalEffect:F0} units of storage capacity";
                
            case CompartmentType.Bridge:
                return $"Enhances command efficiency by {totalEffect * 100:F0}%";
                
            case CompartmentType.Recycling:
                return $"Increases recycling speed by {totalEffect * 100:F0}%";
                
            case CompartmentType.Stabilizer:
                return $"Improves dimensional stability by {totalEffect * 100:F0}%";
                
            case CompartmentType.Communications:
                return $"Extends communication range by {totalEffect * 100:F0}%";
                
            case CompartmentType.Scanner:
                return $"Enhances scanning efficiency by {totalEffect * 100:F0}%";
                
            default:
                return "Unknown effect";
        }
    }
    
    /// <summary>
    /// Get description of what would improve at the next level
    /// </summary>
    public string GetNextLevelPreview()
    {
        if (currentLevel >= maxLevel)
            return "Maximum level reached!";
            
        // Calculate current and next level effects
        float currentEffect = CurrentEffectMultiplier;
        float nextEffect = currentLevel < effectMultipliers.Length ? 
            effectMultipliers[currentLevel] : 
            effectMultipliers[effectMultipliers.Length - 1];
            
        // Calculate the difference
        float improvement = nextEffect - currentEffect;
        
        // Generate description based on compartment type
        string previewText = "";
        switch (type)
        {
            case CompartmentType.Engine:
                float speedBonus = 0.1f * improvement * 100;
                previewText = $"Collection rate: +{speedBonus:F1}%";
                break;
                
            case CompartmentType.Lab:
                float recyclingBonus = 0.15f * improvement * 100;
                previewText = $"Recycling efficiency: +{recyclingBonus:F1}%";
                break;
                
            // Add similar cases for other compartment types
            
            default:
                previewText = "Improved performance";
                break;
        }
        
        // Check for special unlocks at next level
        foreach (var unlock in specialUnlocks)
        {
            if (unlock.unlockLevel == currentLevel + 1)
            {
                previewText += $"\nUnlocks: {unlock.unlockName}";
            }
        }
        
        return previewText;
    }
    
    /// <summary>
    /// Handle direct mouse clicks (fallback for non-EventSystem clicks)
    /// </summary>
    private void OnMouseDown()
    {
        Debug.Log($"Clicked on compartment: {DisplayName}");
        
        // Notify the ShipManager
        if (ShipManager.Instance != null)
        {
            ShipManager.Instance.SelectCompartment(this);
        }
        
        // Notify any listeners
        OnCompartmentClicked?.Invoke(this);
    }
}

/// <summary>
/// Data structure for saving compartment state
/// </summary>
[System.Serializable]
public class CompartmentSaveData
{
    public string id;
    public int level;
}

/// <summary>
/// Types of ship compartments
/// </summary>
