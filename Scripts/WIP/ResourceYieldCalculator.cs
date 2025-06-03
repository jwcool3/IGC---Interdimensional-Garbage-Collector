using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Utility class for calculating resource yields from waste items
/// Handles all the complex math for bonuses, multipliers, and random variations
/// </summary>
public static class ResourceYieldCalculator
{
    /// <summary>
    /// Calculate the final resource yield from a waste item
    /// </summary>
    /// <param name="wasteItem">The waste item being processed</param>
    /// <param name="facilityEfficiency">Processing facility efficiency (0.5 to 2.0)</param>
    /// <param name="locationMultiplier">Location-based multiplier</param>
    /// <param name="playerSkillBonus">Player skill bonus (0.0 to 1.0)</param>
    /// <returns>Dictionary of resource types and amounts</returns>
    public static Dictionary<ResourceType, int> CalculateYield(
        UpdatedWasteItem wasteItem, 
        float facilityEfficiency = 1f, 
        float locationMultiplier = 1f, 
        float playerSkillBonus = 0f)
    {
        var result = new Dictionary<ResourceType, int>();
        
        if (wasteItem?.resourceYield == null) return result;
        
        // Calculate base yield
        var baseYield = CalculateBaseYield(wasteItem.resourceYield);
        
        // Apply all multipliers
        var finalYield = ApplyMultipliers(baseYield, facilityEfficiency, locationMultiplier, playerSkillBonus);
        
        // Apply rarity bonuses
        finalYield = ApplyRarityBonus(finalYield, wasteItem.rarity);
        
        // Apply contamination penalties
        finalYield = ApplyContaminationPenalty(finalYield, wasteItem.contamination);
        
        // Apply random variation
        finalYield = ApplyRandomVariation(finalYield);
        
        // Ensure minimum yields
        finalYield = EnsureMinimumYields(finalYield);
        
        return finalYield;
    }
    
    /// <summary>
    /// Calculate base yield from resource yield data
    /// </summary>
    private static Dictionary<ResourceType, float> CalculateBaseYield(ResourceYield resourceYield)
    {
        var result = new Dictionary<ResourceType, float>();
        
        // Primary resource (always guaranteed)
        if (resourceYield.primaryResource.amount > 0)
        {
            result[resourceYield.primaryResource.type] = resourceYield.primaryResource.amount;
        }
        
        // Secondary resources (chance-based)
        foreach (var secondary in resourceYield.secondaryResources)
        {
            if (Random.value <= secondary.chance)
            {
                if (result.ContainsKey(secondary.type))
                {
                    result[secondary.type] += secondary.amount;
                }
                else
                {
                    result[secondary.type] = secondary.amount;
                }
            }
        }
        
        // Rare resources (low chance, high value)
        foreach (var rare in resourceYield.rareResources)
        {
            if (Random.value <= rare.chance)
            {
                if (result.ContainsKey(rare.type))
                {
                    result[rare.type] += rare.amount;
                }
                else
                {
                    result[rare.type] = rare.amount;
                }
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Apply facility efficiency, location, and skill multipliers
    /// </summary>
    private static Dictionary<ResourceType, float> ApplyMultipliers(
        Dictionary<ResourceType, float> baseYield,
        float facilityEfficiency,
        float locationMultiplier,
        float playerSkillBonus)
    {
        var result = new Dictionary<ResourceType, float>();
        
        float totalMultiplier = facilityEfficiency * locationMultiplier * (1f + playerSkillBonus);
        
        foreach (var kvp in baseYield)
        {
            result[kvp.Key] = kvp.Value * totalMultiplier;
        }
        
        return result;
    }
    
    /// <summary>
    /// Apply rarity-based bonuses
    /// </summary>
    private static Dictionary<ResourceType, float> ApplyRarityBonus(
        Dictionary<ResourceType, float> yield,
        WasteRarity rarity)
    {
        float rarityMultiplier = GetRarityMultiplier(rarity);
        
        var result = new Dictionary<ResourceType, float>();
        foreach (var kvp in yield)
        {
            result[kvp.Key] = kvp.Value * rarityMultiplier;
        }
        
        return result;
    }
    
    /// <summary>
    /// Get multiplier based on waste rarity
    /// </summary>
    private static float GetRarityMultiplier(WasteRarity rarity)
    {
        switch (rarity)
        {
            case WasteRarity.Common: return 1.0f;
            case WasteRarity.Uncommon: return 1.2f;
            case WasteRarity.Rare: return 1.5f;
            case WasteRarity.Epic: return 2.0f;
            case WasteRarity.Legendary: return 3.0f;
            default: return 1.0f;
        }
    }
    
    /// <summary>
    /// Apply contamination penalties
    /// </summary>
    private static Dictionary<ResourceType, float> ApplyContaminationPenalty(
        Dictionary<ResourceType, float> yield,
        float contamination)
    {
        // Contamination reduces yield (0 = no penalty, 1 = 50% penalty)
        float penalty = 1f - (contamination * 0.5f);
        penalty = Mathf.Clamp01(penalty);
        
        var result = new Dictionary<ResourceType, float>();
        foreach (var kvp in yield)
        {
            result[kvp.Key] = kvp.Value * penalty;
        }
        
        return result;
    }
    
    /// <summary>
    /// Apply random variation to yields (±10%)
    /// </summary>
    private static Dictionary<ResourceType, float> ApplyRandomVariation(
        Dictionary<ResourceType, float> yield)
    {
        var result = new Dictionary<ResourceType, float>();
        
        foreach (var kvp in yield)
        {
            float variation = Random.Range(0.9f, 1.1f);
            result[kvp.Key] = kvp.Value * variation;
        }
        
        return result;
    }
    
    /// <summary>
    /// Ensure minimum yields (at least 1 of primary resource)
    /// </summary>
    private static Dictionary<ResourceType, int> EnsureMinimumYields(
        Dictionary<ResourceType, float> yield)
    {
        var result = new Dictionary<ResourceType, int>();
        
        foreach (var kvp in yield)
        {
            int amount = Mathf.RoundToInt(kvp.Value);
            
            // Ensure at least 1 of any resource that was supposed to be yielded
            if (amount < 1 && kvp.Value > 0)
            {
                amount = 1;
            }
            
            if (amount > 0)
            {
                result[kvp.Key] = amount;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Calculate expected yield without randomness (for UI previews)
    /// </summary>
    public static Dictionary<ResourceType, float> CalculateExpectedYield(
        UpdatedWasteItem wasteItem,
        float facilityEfficiency = 1f,
        float locationMultiplier = 1f,
        float playerSkillBonus = 0f)
    {
        var result = new Dictionary<ResourceType, float>();
        
        if (wasteItem?.resourceYield == null) return result;
        
        // Calculate expected base yield (using probabilities)
        var expectedYield = CalculateExpectedBaseYield(wasteItem.resourceYield);
        
        // Apply multipliers
        expectedYield = ApplyMultipliers(expectedYield, facilityEfficiency, locationMultiplier, playerSkillBonus);
        
        // Apply rarity bonus
        expectedYield = ApplyRarityBonus(expectedYield, wasteItem.rarity);
        
        // Apply contamination penalty
        expectedYield = ApplyContaminationPenalty(expectedYield, wasteItem.contamination);
        
        return expectedYield;
    }
    
    /// <summary>
    /// Calculate expected base yield using probabilities
    /// </summary>
    private static Dictionary<ResourceType, float> CalculateExpectedBaseYield(ResourceYield resourceYield)
    {
        var result = new Dictionary<ResourceType, float>();
        
        // Primary resource (always guaranteed)
        if (resourceYield.primaryResource.amount > 0)
        {
            result[resourceYield.primaryResource.type] = resourceYield.primaryResource.amount;
        }
        
        // Secondary resources (expected value = amount * chance)
        foreach (var secondary in resourceYield.secondaryResources)
        {
            float expectedAmount = secondary.amount * secondary.chance;
            
            if (result.ContainsKey(secondary.type))
            {
                result[secondary.type] += expectedAmount;
            }
            else
            {
                result[secondary.type] = expectedAmount;
            }
        }
        
        // Rare resources (expected value = amount * chance)
        foreach (var rare in resourceYield.rareResources)
        {
            float expectedAmount = rare.amount * rare.chance;
            
            if (result.ContainsKey(rare.type))
            {
                result[rare.type] += expectedAmount;
            }
            else
            {
                result[rare.type] = expectedAmount;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Calculate total value of expected yield
    /// </summary>
    public static int CalculateExpectedValue(
        UpdatedWasteItem wasteItem,
        float facilityEfficiency = 1f,
        float locationMultiplier = 1f,
        float playerSkillBonus = 0f)
    {
        var expectedYield = CalculateExpectedYield(wasteItem, facilityEfficiency, locationMultiplier, playerSkillBonus);
        
        int totalValue = 0;
        foreach (var kvp in expectedYield)
        {
            var config = NewResourceManager.Instance?.GetResourceConfig(kvp.Key);
            int baseValue = config?.baseValue ?? 1;
            totalValue += Mathf.RoundToInt(kvp.Value * baseValue);
        }
        
        return totalValue;
    }
    
    /// <summary>
    /// Get processing efficiency recommendations
    /// </summary>
    public static ProcessingRecommendation GetProcessingRecommendation(UpdatedWasteItem wasteItem)
    {
        var recommendation = new ProcessingRecommendation();
        
        if (wasteItem == null)
        {
            recommendation.message = "Invalid waste item";
            recommendation.priority = ProcessingPriority.Skip;
            return recommendation;
        }
        
        // Calculate base value
        int baseValue = CalculateExpectedValue(wasteItem);
        
        // Factor in contamination
        if (wasteItem.contamination > 0.7f)
        {
            recommendation.message = "High contamination - consider decontamination first";
            recommendation.priority = ProcessingPriority.Low;
        }
        else if (wasteItem.contamination > 0.4f)
        {
            recommendation.message = "Moderate contamination - reduced yield expected";
            recommendation.priority = ProcessingPriority.Medium;
        }
        else
        {
            recommendation.message = "Good condition - process immediately";
            recommendation.priority = ProcessingPriority.High;
        }
        
        // Factor in rarity
        if (wasteItem.rarity >= WasteRarity.Rare)
        {
            recommendation.message += " - High value item!";
            recommendation.priority = ProcessingPriority.High;
        }
        
        // Factor in stability
        if (wasteItem.stability < 0.3f)
        {
            recommendation.message += " - Unstable, process quickly!";
            recommendation.priority = ProcessingPriority.Urgent;
        }
        
        recommendation.expectedValue = baseValue;
        recommendation.recommendedFacility = GetRecommendedFacility(wasteItem);
        
        return recommendation;
    }
    
    /// <summary>
    /// Get recommended processing facility for a waste item
    /// </summary>
    private static string GetRecommendedFacility(UpdatedWasteItem wasteItem)
    {
        // Basic logic - can be expanded based on waste type and rarity
        switch (wasteItem.rarity)
        {
            case WasteRarity.Common:
            case WasteRarity.Uncommon:
                return "Basic Processor";
            case WasteRarity.Rare:
                return "Advanced Processor";
            case WasteRarity.Epic:
            case WasteRarity.Legendary:
                return "Quantum Processor";
            default:
                return "Basic Processor";
        }
    }
    
    /// <summary>
    /// Calculate batch processing efficiency
    /// </summary>
    public static BatchProcessingResult CalculateBatchYield(
        List<UpdatedWasteItem> wasteItems,
        float facilityEfficiency = 1f,
        float locationMultiplier = 1f,
        float playerSkillBonus = 0f)
    {
        var result = new BatchProcessingResult();
        result.totalYield = new Dictionary<ResourceType, int>();
        
        foreach (var item in wasteItems)
        {
            var itemYield = CalculateYield(item, facilityEfficiency, locationMultiplier, playerSkillBonus);
            
            foreach (var kvp in itemYield)
            {
                if (result.totalYield.ContainsKey(kvp.Key))
                {
                    result.totalYield[kvp.Key] += kvp.Value;
                }
                else
                {
                    result.totalYield[kvp.Key] = kvp.Value;
                }
            }
            
            result.processedItems++;
        }
        
        // Calculate batch bonus (small bonus for processing multiple items)
        if (result.processedItems > 1)
        {
            float batchBonus = 1f + (result.processedItems * 0.01f); // 1% per item
            batchBonus = Mathf.Min(batchBonus, 1.2f); // Cap at 20% bonus
            
            var bonusYield = new Dictionary<ResourceType, int>();
            foreach (var kvp in result.totalYield)
            {
                bonusYield[kvp.Key] = Mathf.RoundToInt(kvp.Value * batchBonus);
            }
            result.totalYield = bonusYield;
        }
        
        return result;
    }
}

/// <summary>
/// Processing recommendation data
/// </summary>
public class ProcessingRecommendation
{
    public string message;
    public ProcessingPriority priority;
    public int expectedValue;
    public string recommendedFacility;
}

/// <summary>
/// Processing priority levels
/// </summary>
public enum ProcessingPriority
{
    Skip,       // Don't process
    Low,        // Process when convenient
    Medium,     // Normal priority
    High,       // High priority
    Urgent      // Process immediately
}

/// <summary>
/// Batch processing result
/// </summary>
public class BatchProcessingResult
{
    public Dictionary<ResourceType, int> totalYield;
    public int processedItems;
    public float totalProcessingTime;
} 