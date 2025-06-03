using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject configuration for individual resource types
/// Defines properties, behavior, and metadata for each resource
/// </summary>
[CreateAssetMenu(fileName = "New Resource Config", menuName = "Resource System/Resource Config")]
public class ResourceConfig : ScriptableObject
{
    [Header("Resource Identity")]
    public ResourceType resourceType;
    public string displayName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    public Color resourceColor = Color.white;
    
    [Header("Game Balance")]
    public int baseValue = 1;
    public ResourceRarity rarity = ResourceRarity.Common;
    public ResourceCategory category = ResourceCategory.RawMaterial;
    
    [Header("Storage Properties")]
    public bool isStackable = true;
    public int maxStackSize = 999;
    public float weight = 1f;
    public float volume = 1f;
    
    [Header("Processing Information")]
    public bool canBeProcessed = true;
    public bool canBeRefined = false;
    public bool canBeSynthesized = false;
    public float processingDifficulty = 1f;
    
    [Header("Usage Categories")]
    public bool isConstructionMaterial = false;
    public bool isEnergySource = false;
    public bool isTradeable = true;
    public bool isHazardous = false;
    
    [Header("Economic Properties")]
    public float marketDemand = 1f;
    public float volatility = 0.1f;
    public int minimumTradeQuantity = 1;
    
    [Header("Visual Effects")]
    public ParticleSystem processingEffect;
    public AudioClip processingSound;
    public Material resourceMaterial;
    
    [Header("Conversion Rates")]
    [SerializeField] private List<ResourceConversion> conversionRates = new List<ResourceConversion>();
    
    /// <summary>
    /// Get the display color for this resource based on rarity
    /// </summary>
    /// <returns>Color for UI display</returns>
    public Color GetDisplayColor()
    {
        if (resourceColor != Color.white)
            return resourceColor;
        
        return GetRarityColor(rarity);
    }
    
    /// <summary>
    /// Get color based on resource rarity
    /// </summary>
    /// <param name="rarity">Resource rarity</param>
    /// <returns>Rarity color</returns>
    public static Color GetRarityColor(ResourceRarity rarity)
    {
        switch (rarity)
        {
            case ResourceRarity.Common: return Color.white;
            case ResourceRarity.Uncommon: return Color.green;
            case ResourceRarity.Rare: return Color.blue;
            case ResourceRarity.Epic: return Color.magenta;
            case ResourceRarity.Legendary: return Color.yellow;
            default: return Color.white;
        }
    }
    
    /// <summary>
    /// Calculate the effective value of this resource considering market conditions
    /// </summary>
    /// <param name="quantity">Quantity of resource</param>
    /// <param name="marketModifier">Current market modifier</param>
    /// <returns>Effective value</returns>
    public float CalculateEffectiveValue(int quantity, float marketModifier = 1f)
    {
        float baseTotal = baseValue * quantity;
        float marketAdjusted = baseTotal * marketModifier * marketDemand;
        
        // Apply rarity multiplier
        float rarityMultiplier = GetRarityMultiplier(rarity);
        
        return marketAdjusted * rarityMultiplier;
    }
    
    /// <summary>
    /// Get the multiplier for resource rarity
    /// </summary>
    /// <param name="rarity">Resource rarity</param>
    /// <returns>Rarity multiplier</returns>
    public static float GetRarityMultiplier(ResourceRarity rarity)
    {
        switch (rarity)
        {
            case ResourceRarity.Common: return 1f;
            case ResourceRarity.Uncommon: return 1.5f;
            case ResourceRarity.Rare: return 2.5f;
            case ResourceRarity.Epic: return 5f;
            case ResourceRarity.Legendary: return 10f;
            default: return 1f;
        }
    }
    
    /// <summary>
    /// Check if this resource can be converted to another type
    /// </summary>
    /// <param name="targetType">Target resource type</param>
    /// <returns>True if conversion is possible</returns>
    public bool CanConvertTo(ResourceType targetType)
    {
        return conversionRates.Exists(c => c.targetType == targetType);
    }
    
    /// <summary>
    /// Get conversion rate to another resource type
    /// </summary>
    /// <param name="targetType">Target resource type</param>
    /// <returns>Conversion rate or null if not possible</returns>
    public ResourceConversion GetConversionRate(ResourceType targetType)
    {
        return conversionRates.Find(c => c.targetType == targetType);
    }
    
    /// <summary>
    /// Get all possible conversion targets
    /// </summary>
    /// <returns>List of possible conversions</returns>
    public List<ResourceConversion> GetAllConversions()
    {
        return new List<ResourceConversion>(conversionRates);
    }
    
    /// <summary>
    /// Calculate storage space required for a quantity
    /// </summary>
    /// <param name="quantity">Quantity of resource</param>
    /// <returns>Storage space required</returns>
    public float CalculateStorageSpace(int quantity)
    {
        if (isStackable)
        {
            int stacks = Mathf.CeilToInt((float)quantity / maxStackSize);
            return stacks * volume;
        }
        else
        {
            return quantity * volume;
        }
    }
    
    /// <summary>
    /// Calculate weight for a quantity
    /// </summary>
    /// <param name="quantity">Quantity of resource</param>
    /// <returns>Total weight</returns>
    public float CalculateWeight(int quantity)
    {
        return quantity * weight;
    }
    
    /// <summary>
    /// Get processing difficulty modifier
    /// </summary>
    /// <returns>Processing difficulty (1.0 = normal, higher = more difficult)</returns>
    public float GetProcessingDifficulty()
    {
        float difficultyModifier = processingDifficulty;
        
        // Rarity affects processing difficulty
        switch (rarity)
        {
            case ResourceRarity.Rare:
                difficultyModifier *= 1.2f;
                break;
            case ResourceRarity.Epic:
                difficultyModifier *= 1.5f;
                break;
            case ResourceRarity.Legendary:
                difficultyModifier *= 2f;
                break;
        }
        
        return difficultyModifier;
    }
    
    /// <summary>
    /// Validate this resource configuration
    /// </summary>
    /// <returns>Validation result</returns>
    public ResourceConfigValidation ValidateConfig()
    {
        var validation = new ResourceConfigValidation();
        
        if (string.IsNullOrEmpty(displayName))
        {
            validation.errors.Add("Display name is required");
        }
        
        if (baseValue <= 0)
        {
            validation.errors.Add("Base value must be greater than 0");
        }
        
        if (maxStackSize <= 0)
        {
            validation.errors.Add("Max stack size must be greater than 0");
        }
        
        if (weight < 0)
        {
            validation.errors.Add("Weight cannot be negative");
        }
        
        if (volume <= 0)
        {
            validation.errors.Add("Volume must be greater than 0");
        }
        
        if (processingDifficulty <= 0)
        {
            validation.errors.Add("Processing difficulty must be greater than 0");
        }
        
        // Validate conversion rates
        foreach (var conversion in conversionRates)
        {
            if (conversion.inputQuantity <= 0)
            {
                validation.errors.Add($"Invalid input quantity for conversion to {conversion.targetType}");
            }
            
            if (conversion.outputQuantity <= 0)
            {
                validation.errors.Add($"Invalid output quantity for conversion to {conversion.targetType}");
            }
        }
        
        validation.isValid = validation.errors.Count == 0;
        return validation;
    }
    
    private void OnValidate()
    {
        // Ensure values are within reasonable ranges
        baseValue = Mathf.Max(1, baseValue);
        maxStackSize = Mathf.Max(1, maxStackSize);
        weight = Mathf.Max(0, weight);
        volume = Mathf.Max(0.1f, volume);
        processingDifficulty = Mathf.Max(0.1f, processingDifficulty);
        marketDemand = Mathf.Max(0.1f, marketDemand);
        volatility = Mathf.Clamp01(volatility);
    }
}

/// <summary>
/// Resource conversion definition
/// </summary>
[System.Serializable]
public class ResourceConversion
{
    public ResourceType targetType;
    public int inputQuantity = 1;
    public int outputQuantity = 1;
    public float conversionEfficiency = 1f;
    public float energyCost = 0f;
    public bool requiresSpecialFacility = false;
    
    /// <summary>
    /// Calculate the actual output considering efficiency
    /// </summary>
    /// <param name="inputAmount">Amount of input resource</param>
    /// <returns>Expected output amount</returns>
    public int CalculateOutput(int inputAmount)
    {
        float ratio = (float)outputQuantity / inputQuantity;
        float expectedOutput = inputAmount * ratio * conversionEfficiency;
        return Mathf.FloorToInt(expectedOutput);
    }
}

/// <summary>
/// Resource configuration validation result
/// </summary>
[System.Serializable]
public class ResourceConfigValidation
{
    public bool isValid = true;
    public List<string> errors = new List<string>();
    public List<string> warnings = new List<string>();
    
    public override string ToString()
    {
        if (isValid)
            return "Configuration is valid";
        
        return $"Configuration has {errors.Count} errors and {warnings.Count} warnings";
    }
} 