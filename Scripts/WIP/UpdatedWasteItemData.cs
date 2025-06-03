using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ScriptableObject template for defining waste item types
/// Contains base properties and resource yields that UpdatedWasteItem instances use
/// </summary>
[CreateAssetMenu(fileName = "New Waste Item Data", menuName = "Resource System/Waste Item Data")]
public class UpdatedWasteItemData : ScriptableObject
{
    [Header("Basic Information")]
    [SerializeField] private string itemName;
    [SerializeField] private string uniqueIdentifier;
    [SerializeField] private string description;
    [SerializeField] private Sprite iconSprite;
    
    [Header("Waste Properties")]
    [SerializeField] private WasteType wasteType;
    [SerializeField] private WasteRarity defaultRarity = WasteRarity.Common;
    [SerializeField] private string dimensionalOrigin;
    [SerializeField] private List<WasteOrigin> compatibleOrigins = new List<WasteOrigin>();
    
    [Header("Base Stats")]
    [SerializeField, Range(0f, 1f)] private float baseWasteStability = 0.5f;
    [SerializeField, Range(0f, 1f)] private float baseContaminationLevel = 0.2f;
    [SerializeField, Range(0f, 1f)] private float baseRecyclingPotential = 0.6f;
    [SerializeField] private float baseWeight = 1f;
    [SerializeField] private float baseProcessingTime = 10f;
    
    [Header("Resource Yields")]
    [SerializeField] private List<ResourceYieldDefinition> baseRawYields = new List<ResourceYieldDefinition>();
    [SerializeField] private float totalYieldBudget = 100f;
    [SerializeField] private bool allowRandomYieldVariation = true;
    [SerializeField, Range(0f, 0.5f)] private float yieldVariationRange = 0.2f;
    
    [Header("Rarity Modifiers")]
    [SerializeField] private List<RarityModifier> rarityModifiers = new List<RarityModifier>();
    
    [Header("Special Properties")]
    [SerializeField] private bool isHazardous = false;
    [SerializeField] private bool requiresSpecialHandling = false;
    [SerializeField] private bool canDegrade = true;
    [SerializeField] private float degradationRate = 0.01f;
    [SerializeField] private List<string> specialTags = new List<string>();
    
    // Public properties
    public string ItemName => itemName;
    public string UniqueIdentifier => uniqueIdentifier;
    public string Description => description;
    public Sprite IconSprite => iconSprite;
    public WasteType WasteType => wasteType;
    public WasteRarity DefaultRarity => defaultRarity;
    public string DimensionalOrigin => dimensionalOrigin;
    public List<WasteOrigin> CompatibleOrigins => compatibleOrigins;
    public float BaseWasteStability => baseWasteStability;
    public float BaseContaminationLevel => baseContaminationLevel;
    public float BaseRecyclingPotential => baseRecyclingPotential;
    public float BaseWeight => baseWeight;
    public float BaseProcessingTime => baseProcessingTime;
    public List<ResourceYieldDefinition> BaseRawYields => baseRawYields;
    public float TotalYieldBudget => totalYieldBudget;
    public bool IsHazardous => isHazardous;
    public bool RequiresSpecialHandling => requiresSpecialHandling;
    public bool CanDegrade => canDegrade;
    public float DegradationRate => degradationRate;
    public List<string> SpecialTags => specialTags;
    
    /// <summary>
    /// Generate resource yields for a specific waste item instance
    /// </summary>
    /// <param name="rarity">The rarity of the specific instance</param>
    /// <param name="wasteStability">Instance-specific waste stability</param>
    /// <param name="recyclingPotential">Instance-specific recycling potential</param>
    /// <param name="contaminationLevel">Instance-specific contamination level</param>
    /// <returns>Dictionary of resource yields</returns>
    public Dictionary<ResourceType, ResourceYield> GenerateResourceYields(
        WasteRarity rarity, 
        float wasteStability, 
        float recyclingPotential, 
        float contaminationLevel)
    {
        var yields = new Dictionary<ResourceType, ResourceYield>();
        
        // Get rarity modifier
        var rarityMod = GetRarityModifier(rarity);
        
        // Calculate base multipliers
        float stabilityMultiplier = Mathf.Lerp(0.5f, 1.5f, wasteStability);
        float recyclingMultiplier = Mathf.Lerp(0.3f, 1.8f, recyclingPotential);
        float contaminationPenalty = Mathf.Lerp(1f, 0.2f, contaminationLevel);
        
        // Process each base yield
        foreach (var baseYield in baseRawYields)
        {
            var resourceYield = new ResourceYield
            {
                resourceType = baseYield.resourceType,
                baseAmount = CalculateYieldAmount(baseYield, rarityMod, stabilityMultiplier, recyclingMultiplier),
                yieldMultiplier = stabilityMultiplier * recyclingMultiplier * contaminationPenalty * rarityMod.yieldMultiplier,
                chancePercentage = Mathf.Clamp(baseYield.baseChance * rarityMod.chanceMultiplier, 0f, 100f)
            };
            
            // Apply yield variation if enabled
            if (allowRandomYieldVariation)
            {
                float variation = Random.Range(-yieldVariationRange, yieldVariationRange);
                resourceYield.baseAmount = Mathf.Max(0, Mathf.RoundToInt(resourceYield.baseAmount * (1f + variation)));
            }
            
            if (resourceYield.baseAmount > 0 && resourceYield.chancePercentage > 0)
            {
                yields[baseYield.resourceType] = resourceYield;
            }
        }
        
        return yields;
    }
    
    /// <summary>
    /// Check if this waste item data is compatible with a specific origin
    /// </summary>
    /// <param name="origin">Origin to check</param>
    /// <returns>True if compatible</returns>
    public bool IsCompatibleWithOrigin(WasteOrigin origin)
    {
        return compatibleOrigins.Count == 0 || compatibleOrigins.Contains(origin);
    }
    
    /// <summary>
    /// Get the estimated value for this waste item type
    /// </summary>
    /// <param name="rarity">Rarity to calculate for</param>
    /// <returns>Estimated value</returns>
    public float GetEstimatedValue(WasteRarity rarity = WasteRarity.Common)
    {
        var rarityMod = GetRarityModifier(rarity);
        float baseValue = 0f;
        
        foreach (var yield in baseRawYields)
        {
            // Simplified value calculation - in practice, you'd use ResourceConfig values
            float resourceValue = GetResourceBaseValue(yield.resourceType);
            baseValue += yield.minAmount * resourceValue * (yield.baseChance / 100f);
        }
        
        return baseValue * rarityMod.valueMultiplier;
    }
    
    /// <summary>
    /// Create a copy of this data with modified properties
    /// </summary>
    /// <returns>New UpdatedWasteItemData instance</returns>
    public UpdatedWasteItemData CreateVariant()
    {
        var variant = CreateInstance<UpdatedWasteItemData>();
        variant.itemName = itemName;
        variant.uniqueIdentifier = uniqueIdentifier + "_variant";
        variant.description = description;
        variant.iconSprite = iconSprite;
        variant.wasteType = wasteType;
        variant.defaultRarity = defaultRarity;
        variant.dimensionalOrigin = dimensionalOrigin;
        variant.compatibleOrigins = new List<WasteOrigin>(compatibleOrigins);
        variant.baseWasteStability = baseWasteStability;
        variant.baseContaminationLevel = baseContaminationLevel;
        variant.baseRecyclingPotential = baseRecyclingPotential;
        variant.baseWeight = baseWeight;
        variant.baseProcessingTime = baseProcessingTime;
        variant.baseRawYields = new List<ResourceYieldDefinition>(baseRawYields);
        variant.totalYieldBudget = totalYieldBudget;
        variant.rarityModifiers = new List<RarityModifier>(rarityModifiers);
        variant.isHazardous = isHazardous;
        variant.requiresSpecialHandling = requiresSpecialHandling;
        variant.canDegrade = canDegrade;
        variant.degradationRate = degradationRate;
        variant.specialTags = new List<string>(specialTags);
        
        return variant;
    }
    
    private RarityModifier GetRarityModifier(WasteRarity rarity)
    {
        var modifier = rarityModifiers.FirstOrDefault(r => r.rarity == rarity);
        if (modifier != null)
            return modifier;
        
        // Default modifiers if not specified
        return new RarityModifier
        {
            rarity = rarity,
            yieldMultiplier = GetDefaultRarityYieldMultiplier(rarity),
            chanceMultiplier = GetDefaultRarityChanceMultiplier(rarity),
            valueMultiplier = GetDefaultRarityValueMultiplier(rarity)
        };
    }
    
    private int CalculateYieldAmount(ResourceYieldDefinition baseYield, RarityModifier rarityMod, 
        float stabilityMultiplier, float recyclingMultiplier)
    {
        float baseAmount = Random.Range(baseYield.minAmount, baseYield.maxAmount + 1);
        float modifiedAmount = baseAmount * rarityMod.yieldMultiplier * stabilityMultiplier * recyclingMultiplier;
        return Mathf.Max(0, Mathf.RoundToInt(modifiedAmount));
    }
    
    private float GetDefaultRarityYieldMultiplier(WasteRarity rarity)
    {
        switch (rarity)
        {
            case WasteRarity.Common: return 1f;
            case WasteRarity.Uncommon: return 1.2f;
            case WasteRarity.Rare: return 1.5f;
            case WasteRarity.Epic: return 2f;
            case WasteRarity.Legendary: return 3f;
            default: return 1f;
        }
    }
    
    private float GetDefaultRarityChanceMultiplier(WasteRarity rarity)
    {
        switch (rarity)
        {
            case WasteRarity.Common: return 1f;
            case WasteRarity.Uncommon: return 1.1f;
            case WasteRarity.Rare: return 1.25f;
            case WasteRarity.Epic: return 1.5f;
            case WasteRarity.Legendary: return 2f;
            default: return 1f;
        }
    }
    
    private float GetDefaultRarityValueMultiplier(WasteRarity rarity)
    {
        switch (rarity)
        {
            case WasteRarity.Common: return 1f;
            case WasteRarity.Uncommon: return 1.5f;
            case WasteRarity.Rare: return 2.5f;
            case WasteRarity.Epic: return 5f;
            case WasteRarity.Legendary: return 10f;
            default: return 1f;
        }
    }
    
    private float GetResourceBaseValue(ResourceType resourceType)
    {
        // Simplified base values - in practice, use ResourceConfigManager
        switch (resourceType)
        {
            case ResourceType.Plastic: return 1f;
            case ResourceType.MetalScraps: return 2f;
            case ResourceType.OrganicMatter: return 0.5f;
            case ResourceType.Glass: return 1.5f;
            case ResourceType.Electronics: return 10f;
            case ResourceType.RareMetals: return 20f;
            case ResourceType.Crystals: return 15f;
            case ResourceType.Nanomaterials: return 50f;
            case ResourceType.QuantumMatter: return 100f;
            default: return 1f;
        }
    }
    
    private void OnValidate()
    {
        // Ensure valid values
        baseWasteStability = Mathf.Clamp01(baseWasteStability);
        baseContaminationLevel = Mathf.Clamp01(baseContaminationLevel);
        baseRecyclingPotential = Mathf.Clamp01(baseRecyclingPotential);
        baseWeight = Mathf.Max(0.1f, baseWeight);
        baseProcessingTime = Mathf.Max(1f, baseProcessingTime);
        totalYieldBudget = Mathf.Max(1f, totalYieldBudget);
        degradationRate = Mathf.Max(0f, degradationRate);
        
        // Ensure unique identifier is set
        if (string.IsNullOrEmpty(uniqueIdentifier))
        {
            uniqueIdentifier = name.Replace(" ", "_").ToLower();
        }
        
        // Validate base yields
        foreach (var yield in baseRawYields)
        {
            yield.minAmount = Mathf.Max(0, yield.minAmount);
            yield.maxAmount = Mathf.Max(yield.minAmount, yield.maxAmount);
            yield.baseChance = Mathf.Clamp(yield.baseChance, 0f, 100f);
        }
    }
}

/// <summary>
/// Defines the base resource yield for a waste item type
/// </summary>
[System.Serializable]
public class ResourceYieldDefinition
{
    public ResourceType resourceType;
    public int minAmount = 1;
    public int maxAmount = 3;
    public float baseChance = 100f;
    public bool isGuaranteed = false;
    
    public override string ToString()
    {
        return $"{resourceType}: {minAmount}-{maxAmount} ({baseChance}%)";
    }
}

/// <summary>
/// Modifiers applied based on waste item rarity
/// </summary>
[System.Serializable]
public class RarityModifier
{
    public WasteRarity rarity;
    public float yieldMultiplier = 1f;
    public float chanceMultiplier = 1f;
    public float valueMultiplier = 1f;
    public float stabilityBonus = 0f;
    public float contaminationReduction = 0f;
    
    public override string ToString()
    {
        return $"{rarity}: Yield x{yieldMultiplier}, Chance x{chanceMultiplier}, Value x{valueMultiplier}";
    }
} 