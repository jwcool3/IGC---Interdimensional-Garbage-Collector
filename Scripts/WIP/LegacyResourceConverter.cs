using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Utility class for converting between legacy resource values and new resource system
/// Provides accurate conversion rates and validation for resource migration
/// </summary>
public static class LegacyResourceConverter
{
    // Conversion rates from legacy to new system
    private static readonly Dictionary<ResourceType, float> LegacyToNewRates = new Dictionary<ResourceType, float>
    {
        { ResourceType.Plastic, 10f },          // 10 RP = 1 Plastic
        { ResourceType.MetalScraps, 20f },      // 20 RP = 1 Metal
        { ResourceType.OrganicMatter, 10f },    // 10 RP = 1 Organic
        { ResourceType.CrystalFragments, 50f }, // 50 RP = 1 Crystal (or 10 DP = 1 Crystal)
        { ResourceType.NeuralResidue, 30f },    // 30 RP = 1 Neural
        { ResourceType.ToxicSludge, 25f },      // 25 RP = 1 Sludge
        { ResourceType.Fuel, 15f },             // 15 RP = 1 Fuel
        { ResourceType.Food, 12f },             // 12 RP = 1 Food
        { ResourceType.Parts, 25f },            // 25 RP = 1 Parts
        { ResourceType.Energy, 40f }            // 40 RP = 1 Energy
    };
    
    // Dimensional Potential conversion rates
    private static readonly Dictionary<ResourceType, float> DPToNewRates = new Dictionary<ResourceType, float>
    {
        { ResourceType.CrystalFragments, 5f },  // 5 DP = 1 Crystal
        { ResourceType.NeuralResidue, 8f },     // 8 DP = 1 Neural
        { ResourceType.Energy, 10f }            // 10 DP = 1 Energy
    };
    
    /// <summary>
    /// Convert legacy recycling points to new resource amounts
    /// </summary>
    /// <param name="recyclingPoints">Amount of legacy recycling points</param>
    /// <param name="preferredDistribution">Preferred resource distribution</param>
    /// <returns>Dictionary of resource types and amounts</returns>
    public static Dictionary<ResourceType, int> ConvertRecyclingPoints(
        float recyclingPoints, 
        ResourceDistribution preferredDistribution = ResourceDistribution.Balanced)
    {
        var result = new Dictionary<ResourceType, int>();
        
        if (recyclingPoints <= 0) return result;
        
        switch (preferredDistribution)
        {
            case ResourceDistribution.Balanced:
                result = ConvertRPBalanced(recyclingPoints);
                break;
            case ResourceDistribution.BasicMaterials:
                result = ConvertRPBasicMaterials(recyclingPoints);
                break;
            case ResourceDistribution.AdvancedMaterials:
                result = ConvertRPAdvancedMaterials(recyclingPoints);
                break;
            case ResourceDistribution.ProcessedGoods:
                result = ConvertRPProcessedGoods(recyclingPoints);
                break;
        }
        
        return result;
    }
    
    /// <summary>
    /// Convert legacy dimensional potential to new resource amounts
    /// </summary>
    /// <param name="dimensionalPotential">Amount of legacy dimensional potential</param>
    /// <param name="preferredDistribution">Preferred resource distribution</param>
    /// <returns>Dictionary of resource types and amounts</returns>
    public static Dictionary<ResourceType, int> ConvertDimensionalPotential(
        float dimensionalPotential,
        DPDistribution preferredDistribution = DPDistribution.Balanced)
    {
        var result = new Dictionary<ResourceType, int>();
        
        if (dimensionalPotential <= 0) return result;
        
        switch (preferredDistribution)
        {
            case DPDistribution.Balanced:
                result = ConvertDPBalanced(dimensionalPotential);
                break;
            case DPDistribution.Crystals:
                result = ConvertDPCrystals(dimensionalPotential);
                break;
            case DPDistribution.Neural:
                result = ConvertDPNeural(dimensionalPotential);
                break;
            case DPDistribution.Energy:
                result = ConvertDPEnergy(dimensionalPotential);
                break;
        }
        
        return result;
    }
    
    /// <summary>
    /// Convert new resources back to legacy recycling points value
    /// </summary>
    /// <param name="resources">Dictionary of resource types and amounts</param>
    /// <returns>Equivalent recycling points value</returns>
    public static float ConvertToRecyclingPoints(Dictionary<ResourceType, int> resources)
    {
        float totalRP = 0f;
        
        foreach (var kvp in resources)
        {
            if (LegacyToNewRates.TryGetValue(kvp.Key, out float rate))
            {
                totalRP += kvp.Value * rate;
            }
        }
        
        return totalRP;
    }
    
    /// <summary>
    /// Convert new resources back to legacy dimensional potential value
    /// </summary>
    /// <param name="resources">Dictionary of resource types and amounts</param>
    /// <returns>Equivalent dimensional potential value</returns>
    public static float ConvertToDimensionalPotential(Dictionary<ResourceType, int> resources)
    {
        float totalDP = 0f;
        
        foreach (var kvp in resources)
        {
            if (DPToNewRates.TryGetValue(kvp.Key, out float rate))
            {
                totalDP += kvp.Value * rate;
            }
        }
        
        return totalDP;
    }
    
    /// <summary>
    /// Get the optimal resource conversion for a given legacy value
    /// </summary>
    /// <param name="recyclingPoints">Legacy recycling points</param>
    /// <param name="dimensionalPotential">Legacy dimensional potential</param>
    /// <returns>Optimized resource distribution</returns>
    public static Dictionary<ResourceType, int> GetOptimalConversion(
        float recyclingPoints, 
        float dimensionalPotential)
    {
        var result = new Dictionary<ResourceType, int>();
        
        // Convert RP to basic materials (70% of total value)
        var rpResources = ConvertRecyclingPoints(recyclingPoints * 0.7f, ResourceDistribution.BasicMaterials);
        
        // Convert remaining RP to processed goods (30% of total value)
        var processedResources = ConvertRecyclingPoints(recyclingPoints * 0.3f, ResourceDistribution.ProcessedGoods);
        
        // Convert DP to advanced materials
        var dpResources = ConvertDimensionalPotential(dimensionalPotential, DPDistribution.Balanced);
        
        // Combine all resources
        MergeResourceDictionaries(result, rpResources);
        MergeResourceDictionaries(result, processedResources);
        MergeResourceDictionaries(result, dpResources);
        
        return result;
    }
    
    /// <summary>
    /// Validate that a conversion preserves value within acceptable tolerance
    /// </summary>
    /// <param name="originalRP">Original recycling points</param>
    /// <param name="originalDP">Original dimensional potential</param>
    /// <param name="convertedResources">Converted resources</param>
    /// <param name="tolerance">Acceptable value difference (0.0 to 1.0)</param>
    /// <returns>True if conversion is valid</returns>
    public static bool ValidateConversion(
        float originalRP, 
        float originalDP, 
        Dictionary<ResourceType, int> convertedResources, 
        float tolerance = 0.1f)
    {
        float originalValue = originalRP + (originalDP * 2f); // DP worth 2x RP
        float convertedValue = CalculateResourceValue(convertedResources);
        
        float difference = Mathf.Abs(originalValue - convertedValue);
        float allowedDifference = originalValue * tolerance;
        
        return difference <= allowedDifference;
    }
    
    /// <summary>
    /// Calculate the total value of resources in legacy terms
    /// </summary>
    /// <param name="resources">Dictionary of resource types and amounts</param>
    /// <returns>Total value in legacy recycling points equivalent</returns>
    public static float CalculateResourceValue(Dictionary<ResourceType, int> resources)
    {
        float totalValue = 0f;
        
        foreach (var kvp in resources)
        {
            if (LegacyToNewRates.TryGetValue(kvp.Key, out float rpRate))
            {
                totalValue += kvp.Value * rpRate;
            }
            else if (DPToNewRates.TryGetValue(kvp.Key, out float dpRate))
            {
                totalValue += kvp.Value * dpRate * 2f; // DP worth 2x RP
            }
        }
        
        return totalValue;
    }
    
    #region Conversion Implementations
    
    private static Dictionary<ResourceType, int> ConvertRPBalanced(float recyclingPoints)
    {
        var result = new Dictionary<ResourceType, int>();
        
        // Distribute evenly among basic materials
        float rpPerType = recyclingPoints / 3f;
        
        result[ResourceType.Plastic] = Mathf.RoundToInt(rpPerType / LegacyToNewRates[ResourceType.Plastic]);
        result[ResourceType.MetalScraps] = Mathf.RoundToInt(rpPerType / LegacyToNewRates[ResourceType.MetalScraps]);
        result[ResourceType.OrganicMatter] = Mathf.RoundToInt(rpPerType / LegacyToNewRates[ResourceType.OrganicMatter]);
        
        return result;
    }
    
    private static Dictionary<ResourceType, int> ConvertRPBasicMaterials(float recyclingPoints)
    {
        var result = new Dictionary<ResourceType, int>();
        
        // Focus on basic materials with weighted distribution
        float plasticRP = recyclingPoints * 0.4f;
        float metalRP = recyclingPoints * 0.35f;
        float organicRP = recyclingPoints * 0.25f;
        
        result[ResourceType.Plastic] = Mathf.RoundToInt(plasticRP / LegacyToNewRates[ResourceType.Plastic]);
        result[ResourceType.MetalScraps] = Mathf.RoundToInt(metalRP / LegacyToNewRates[ResourceType.MetalScraps]);
        result[ResourceType.OrganicMatter] = Mathf.RoundToInt(organicRP / LegacyToNewRates[ResourceType.OrganicMatter]);
        
        return result;
    }
    
    private static Dictionary<ResourceType, int> ConvertRPAdvancedMaterials(float recyclingPoints)
    {
        var result = new Dictionary<ResourceType, int>();
        
        // Focus on advanced materials
        float sludgeRP = recyclingPoints * 0.6f;
        float neuralRP = recyclingPoints * 0.4f;
        
        result[ResourceType.ToxicSludge] = Mathf.RoundToInt(sludgeRP / LegacyToNewRates[ResourceType.ToxicSludge]);
        result[ResourceType.NeuralResidue] = Mathf.RoundToInt(neuralRP / LegacyToNewRates[ResourceType.NeuralResidue]);
        
        return result;
    }
    
    private static Dictionary<ResourceType, int> ConvertRPProcessedGoods(float recyclingPoints)
    {
        var result = new Dictionary<ResourceType, int>();
        
        // Focus on processed goods
        float fuelRP = recyclingPoints * 0.4f;
        float foodRP = recyclingPoints * 0.3f;
        float partsRP = recyclingPoints * 0.3f;
        
        result[ResourceType.Fuel] = Mathf.RoundToInt(fuelRP / LegacyToNewRates[ResourceType.Fuel]);
        result[ResourceType.Food] = Mathf.RoundToInt(foodRP / LegacyToNewRates[ResourceType.Food]);
        result[ResourceType.Parts] = Mathf.RoundToInt(partsRP / LegacyToNewRates[ResourceType.Parts]);
        
        return result;
    }
    
    private static Dictionary<ResourceType, int> ConvertDPBalanced(float dimensionalPotential)
    {
        var result = new Dictionary<ResourceType, int>();
        
        // Distribute evenly among DP-based resources
        float dpPerType = dimensionalPotential / 3f;
        
        result[ResourceType.CrystalFragments] = Mathf.RoundToInt(dpPerType / DPToNewRates[ResourceType.CrystalFragments]);
        result[ResourceType.NeuralResidue] = Mathf.RoundToInt(dpPerType / DPToNewRates[ResourceType.NeuralResidue]);
        result[ResourceType.Energy] = Mathf.RoundToInt(dpPerType / DPToNewRates[ResourceType.Energy]);
        
        return result;
    }
    
    private static Dictionary<ResourceType, int> ConvertDPCrystals(float dimensionalPotential)
    {
        var result = new Dictionary<ResourceType, int>();
        
        // Focus on crystal fragments
        result[ResourceType.CrystalFragments] = Mathf.RoundToInt(dimensionalPotential / DPToNewRates[ResourceType.CrystalFragments]);
        
        return result;
    }
    
    private static Dictionary<ResourceType, int> ConvertDPNeural(float dimensionalPotential)
    {
        var result = new Dictionary<ResourceType, int>();
        
        // Focus on neural residue
        result[ResourceType.NeuralResidue] = Mathf.RoundToInt(dimensionalPotential / DPToNewRates[ResourceType.NeuralResidue]);
        
        return result;
    }
    
    private static Dictionary<ResourceType, int> ConvertDPEnergy(float dimensionalPotential)
    {
        var result = new Dictionary<ResourceType, int>();
        
        // Focus on energy
        result[ResourceType.Energy] = Mathf.RoundToInt(dimensionalPotential / DPToNewRates[ResourceType.Energy]);
        
        return result;
    }
    
    private static void MergeResourceDictionaries(Dictionary<ResourceType, int> target, Dictionary<ResourceType, int> source)
    {
        foreach (var kvp in source)
        {
            if (target.ContainsKey(kvp.Key))
            {
                target[kvp.Key] += kvp.Value;
            }
            else
            {
                target[kvp.Key] = kvp.Value;
            }
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Get conversion rate for a specific resource type from recycling points
    /// </summary>
    /// <param name="resourceType">The resource type</param>
    /// <returns>Recycling points per unit of resource</returns>
    public static float GetRPConversionRate(ResourceType resourceType)
    {
        return LegacyToNewRates.TryGetValue(resourceType, out float rate) ? rate : 0f;
    }
    
    /// <summary>
    /// Get conversion rate for a specific resource type from dimensional potential
    /// </summary>
    /// <param name="resourceType">The resource type</param>
    /// <returns>Dimensional potential per unit of resource</returns>
    public static float GetDPConversionRate(ResourceType resourceType)
    {
        return DPToNewRates.TryGetValue(resourceType, out float rate) ? rate : 0f;
    }
    
    /// <summary>
    /// Check if a resource type can be converted from recycling points
    /// </summary>
    /// <param name="resourceType">The resource type to check</param>
    /// <returns>True if convertible from RP</returns>
    public static bool CanConvertFromRP(ResourceType resourceType)
    {
        return LegacyToNewRates.ContainsKey(resourceType);
    }
    
    /// <summary>
    /// Check if a resource type can be converted from dimensional potential
    /// </summary>
    /// <param name="resourceType">The resource type to check</param>
    /// <returns>True if convertible from DP</returns>
    public static bool CanConvertFromDP(ResourceType resourceType)
    {
        return DPToNewRates.ContainsKey(resourceType);
    }
    
    /// <summary>
    /// Get all resource types that can be converted from recycling points
    /// </summary>
    /// <returns>Array of convertible resource types</returns>
    public static ResourceType[] GetRPConvertibleTypes()
    {
        var types = new ResourceType[LegacyToNewRates.Count];
        LegacyToNewRates.Keys.CopyTo(types, 0);
        return types;
    }
    
    /// <summary>
    /// Get all resource types that can be converted from dimensional potential
    /// </summary>
    /// <returns>Array of convertible resource types</returns>
    public static ResourceType[] GetDPConvertibleTypes()
    {
        var types = new ResourceType[DPToNewRates.Count];
        DPToNewRates.Keys.CopyTo(types, 0);
        return types;
    }
    
    /// <summary>
    /// Create a conversion report for debugging and validation
    /// </summary>
    /// <param name="originalRP">Original recycling points</param>
    /// <param name="originalDP">Original dimensional potential</param>
    /// <param name="convertedResources">Converted resources</param>
    /// <returns>Detailed conversion report</returns>
    public static ConversionReport CreateConversionReport(
        float originalRP, 
        float originalDP, 
        Dictionary<ResourceType, int> convertedResources)
    {
        var report = new ConversionReport
        {
            originalRecyclingPoints = originalRP,
            originalDimensionalPotential = originalDP,
            originalTotalValue = originalRP + (originalDP * 2f),
            convertedResources = new Dictionary<ResourceType, int>(convertedResources),
            convertedTotalValue = CalculateResourceValue(convertedResources),
            conversionTimestamp = DateTime.Now
        };
        
        report.valueDifference = report.convertedTotalValue - report.originalTotalValue;
        report.valueDifferencePercent = report.originalTotalValue > 0 ? 
            (report.valueDifference / report.originalTotalValue) * 100f : 0f;
        
        return report;
    }
    
    #endregion
}

/// <summary>
/// Resource distribution preferences for recycling points conversion
/// </summary>
public enum ResourceDistribution
{
    Balanced,           // Even distribution among basic materials
    BasicMaterials,     // Focus on plastic, metal, organic
    AdvancedMaterials,  // Focus on sludge, neural residue
    ProcessedGoods      // Focus on fuel, food, parts
}

/// <summary>
/// Resource distribution preferences for dimensional potential conversion
/// </summary>
public enum DPDistribution
{
    Balanced,   // Even distribution among DP resources
    Crystals,   // Focus on crystal fragments
    Neural,     // Focus on neural residue
    Energy      // Focus on energy
}

/// <summary>
/// Detailed conversion report for validation and debugging
/// </summary>
[System.Serializable]
public class ConversionReport
{
    public float originalRecyclingPoints;
    public float originalDimensionalPotential;
    public float originalTotalValue;
    public Dictionary<ResourceType, int> convertedResources;
    public float convertedTotalValue;
    public float valueDifference;
    public float valueDifferencePercent;
    public DateTime conversionTimestamp;
    
    public override string ToString()
    {
        return $"Conversion Report:\n" +
               $"Original: {originalRecyclingPoints} RP, {originalDimensionalPotential} DP (Total: {originalTotalValue})\n" +
               $"Converted: {convertedResources.Count} resource types (Total: {convertedTotalValue})\n" +
               $"Difference: {valueDifference:F2} ({valueDifferencePercent:F1}%)";
    }
} 