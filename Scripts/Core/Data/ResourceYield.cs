using System;
using UnityEngine;

/// <summary>
/// Represents the yield data for a specific resource type from waste processing
/// Used by UpdatedWasteItem and ResourceProcessingManager
/// </summary>
[Serializable]
public class ResourceYield
{
    [Header("Resource Information")]
    public ResourceType resourceType;
    
    [Header("Yield Properties")]
    [Tooltip("Base amount of this resource that can be yielded")]
    public int baseAmount = 1;
    
    [Tooltip("Multiplier applied to base amount based on processing conditions")]
    [Range(0f, 3f)]
    public float yieldMultiplier = 1f;
    
    [Tooltip("Percentage chance this resource will be yielded")]
    [Range(0f, 100f)]
    public float chancePercentage = 100f;
    
    [Header("Quality Factors")]
    [Tooltip("Minimum quality required to yield this resource")]
    [Range(0f, 1f)]
    public float minimumQuality = 0f;
    
    [Tooltip("Quality bonus multiplier")]
    [Range(0f, 2f)]
    public float qualityBonus = 0f;
    
    [Header("Processing Requirements")]
    [Tooltip("Processing types that can yield this resource")]
    public ProcessingType[] compatibleProcessingTypes = { ProcessingType.Sorting };
    
    [Tooltip("Minimum processing time required (in seconds)")]
    public float minimumProcessingTime = 1f;
    
    // New properties for enhanced functionality
    [Header("Enhanced Properties")]
    public ResourceAmount[] primaryResources = new ResourceAmount[0];
    public ResourceChance[] secondaryResources = new ResourceChance[0];
    public float contaminationRisk = 0f;
    
    /// <summary>
    /// Calculate the actual amount that will be yielded based on current conditions
    /// </summary>
    /// <param name="wasteQuality">Quality of the waste item (0-1)</param>
    /// <param name="processingEfficiency">Efficiency of the processing facility (0-1)</param>
    /// <param name="facilityBonus">Additional bonus from facility upgrades (0-2)</param>
    /// <returns>Final amount to be yielded</returns>
    public int CalculateActualYield(float wasteQuality = 1f, float processingEfficiency = 1f, float facilityBonus = 0f)
    {
        // Check minimum quality requirement
        if (wasteQuality < minimumQuality)
            return 0;
        
        // Calculate base yield with multiplier
        float baseYield = baseAmount * yieldMultiplier;
        
        // Apply quality bonus
        float qualityMultiplier = 1f + (qualityBonus * wasteQuality);
        
        // Apply processing efficiency
        float efficiencyMultiplier = 0.5f + (processingEfficiency * 0.5f); // 50% to 100% based on efficiency
        
        // Apply facility bonus
        float facilityMultiplier = 1f + facilityBonus;
        
        // Calculate final amount
        float finalAmount = baseYield * qualityMultiplier * efficiencyMultiplier * facilityMultiplier;
        
        return Mathf.RoundToInt(finalAmount);
    }
    
    /// <summary>
    /// Check if this resource will be yielded based on chance
    /// </summary>
    /// <param name="luckModifier">Additional luck modifier (0-1)</param>
    /// <returns>True if the resource should be yielded</returns>
    public bool RollForYield(float luckModifier = 0f)
    {
        float adjustedChance = chancePercentage + (luckModifier * 20f); // Luck can add up to 20% chance
        adjustedChance = Mathf.Clamp(adjustedChance, 0f, 100f);
        
        float roll = UnityEngine.Random.Range(0f, 100f);
        return roll <= adjustedChance;
    }
    
    /// <summary>
    /// Check if this yield is compatible with the given processing type
    /// </summary>
    /// <param name="processingType">Type of processing being used</param>
    /// <returns>True if compatible</returns>
    public bool IsCompatibleWith(ProcessingType processingType)
    {
        if (compatibleProcessingTypes == null || compatibleProcessingTypes.Length == 0)
            return true; // If no restrictions, compatible with all
        
        foreach (var compatibleType in compatibleProcessingTypes)
        {
            if (compatibleType == processingType)
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Get the estimated value of this yield
    /// </summary>
    /// <param name="resourceValue">Base value per unit of this resource type</param>
    /// <returns>Estimated total value</returns>
    public float GetEstimatedValue(float resourceValue = 1f)
    {
        float expectedAmount = baseAmount * yieldMultiplier * (chancePercentage / 100f);
        return expectedAmount * resourceValue;
    }
    
    /// <summary>
    /// Create a copy of this ResourceYield with modified values
    /// </summary>
    /// <param name="amountMultiplier">Multiplier for base amount</param>
    /// <param name="chanceModifier">Modifier for chance percentage</param>
    /// <returns>New ResourceYield instance</returns>
    public ResourceYield CreateVariant(float amountMultiplier = 1f, float chanceModifier = 0f)
    {
        return new ResourceYield
        {
            resourceType = resourceType,
            baseAmount = Mathf.RoundToInt(baseAmount * amountMultiplier),
            yieldMultiplier = yieldMultiplier,
            chancePercentage = Mathf.Clamp(chancePercentage + chanceModifier, 0f, 100f),
            minimumQuality = minimumQuality,
            qualityBonus = qualityBonus,
            compatibleProcessingTypes = compatibleProcessingTypes,
            minimumProcessingTime = minimumProcessingTime,
            primaryResources = (ResourceAmount[])primaryResources.Clone(),
            secondaryResources = (ResourceChance[])secondaryResources.Clone(),
            contaminationRisk = contaminationRisk
        };
    }
    
    /// <summary>
    /// Validate this yield configuration
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid()
    {
        return baseAmount > 0 && 
               yieldMultiplier >= 0f && 
               chancePercentage >= 0f && chancePercentage <= 100f &&
               minimumQuality >= 0f && minimumQuality <= 1f &&
               qualityBonus >= 0f &&
               minimumProcessingTime > 0f;
    }
    
    /// <summary>
    /// Get a human-readable description of this yield
    /// </summary>
    /// <returns>Description string</returns>
    public string GetDescription()
    {
        string desc = $"{baseAmount} {resourceType}";
        
        if (yieldMultiplier != 1f)
            desc += $" (×{yieldMultiplier:F1})";
        
        if (chancePercentage < 100f)
            desc += $" ({chancePercentage:F0}% chance)";
        
        if (minimumQuality > 0f)
            desc += $" (min quality: {minimumQuality:F1})";
        
        return desc;
    }
    
    public override string ToString()
    {
        return GetDescription();
    }
}

/// <summary>
/// Legacy ResourceAmount class for backward compatibility
/// </summary>
[Serializable]
public class ResourceAmount
{
    public ResourceType type;
    public int amount;
    
    public ResourceAmount()
    {
        type = ResourceType.None;
        amount = 0;
    }
    
    public ResourceAmount(ResourceType resourceType, int resourceAmount)
    {
        type = resourceType;
        amount = resourceAmount;
    }
}

/// <summary>
/// Legacy ResourceChance class for backward compatibility
/// </summary>
[Serializable]
public class ResourceChance
{
    public ResourceType type;
    public int amount;
    public float chance; // 0.0 to 1.0
    
    /// <summary>
    /// Alias for type property for compatibility
    /// </summary>
    public ResourceType resourceType => type;
    
    public ResourceChance()
    {
        type = ResourceType.None;
        amount = 0;
        chance = 0f;
    }
    
    public ResourceChance(ResourceType resourceType, int resourceAmount, float chancePercentage)
    {
        type = resourceType;
        amount = resourceAmount;
        chance = chancePercentage;
    }
} 