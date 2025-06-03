using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Maps waste origins to resource types and defines conversion relationships
/// Handles the logic for determining what resources can be extracted from different waste sources
/// </summary>
[CreateAssetMenu(fileName = "New Waste Resource Mapping", menuName = "Resource System/Waste Resource Mapping")]
public class WasteResourceMapping : ScriptableObject
{
    [Header("Mapping Configuration")]
    [SerializeField] private List<WasteToResourceMapping> mappings = new List<WasteToResourceMapping>();
    [SerializeField] private List<OriginBasedMapping> originMappings = new List<OriginBasedMapping>();
    [SerializeField] private List<ContaminationMapping> contaminationMappings = new List<ContaminationMapping>();
    
    [Header("Default Settings")]
    [SerializeField] private float defaultYieldMultiplier = 1f;
    [SerializeField] private ResourceType fallbackResourceType = ResourceType.Plastic;
    [SerializeField] private bool enableOriginBasedBonuses = true;
    [SerializeField] private bool enableContaminationPenalties = true;
    
    /// <summary>
    /// Get possible resource yields for a waste item
    /// </summary>
    /// <param name="wasteItem">The waste item to analyze</param>
    /// <returns>Dictionary of possible resource yields</returns>
    public Dictionary<ResourceType, ResourceYield> GetResourceYields(WasteItem wasteItem)
    {
        var yields = new Dictionary<ResourceType, ResourceYield>();
        
        // Get base mappings for waste type
        var baseMappings = GetMappingsForWasteType(wasteItem.WasteType);
        
        foreach (var mapping in baseMappings)
        {
            var yield = CalculateYield(wasteItem, mapping);
            if (yield.baseAmount > 0)
            {
                yields[mapping.targetResource] = yield;
            }
        }
        
        // Apply origin-based bonuses
        if (enableOriginBasedBonuses)
        {
            ApplyOriginBonuses(wasteItem, yields);
        }
        
        // Apply contamination penalties
        if (enableContaminationPenalties)
        {
            ApplyContaminationPenalties(wasteItem, yields);
        }
        
        // Ensure at least one resource if none found
        if (yields.Count == 0)
        {
            yields[fallbackResourceType] = new ResourceYield
            {
                resourceType = fallbackResourceType,
                baseAmount = 1,
                yieldMultiplier = defaultYieldMultiplier,
                chancePercentage = 100f
            };
        }
        
        return yields;
    }
    
    /// <summary>
    /// Get the primary resource type for a waste item
    /// </summary>
    /// <param name="wasteItem">The waste item to analyze</param>
    /// <returns>Primary resource type</returns>
    public ResourceType GetPrimaryResourceType(WasteItem wasteItem)
    {
        var yields = GetResourceYields(wasteItem);
        
        if (yields.Count == 0)
            return fallbackResourceType;
        
        // Return the resource with highest expected yield
        return yields.OrderByDescending(kv => kv.Value.baseAmount * kv.Value.yieldMultiplier)
                    .First().Key;
    }
    
    /// <summary>
    /// Check if a waste type can produce a specific resource
    /// </summary>
    /// <param name="wasteType">Waste type to check</param>
    /// <param name="resourceType">Resource type to check for</param>
    /// <returns>True if the waste can produce the resource</returns>
    public bool CanProduceResource(WasteType wasteType, ResourceType resourceType)
    {
        return mappings.Any(m => m.wasteType == wasteType && m.targetResource == resourceType);
    }
    
    /// <summary>
    /// Get all possible resources for a waste type
    /// </summary>
    /// <param name="wasteType">Waste type to check</param>
    /// <returns>List of possible resource types</returns>
    public List<ResourceType> GetPossibleResources(WasteType wasteType)
    {
        return mappings.Where(m => m.wasteType == wasteType)
                      .Select(m => m.targetResource)
                      .Distinct()
                      .ToList();
    }
    
    /// <summary>
    /// Get origin bonus multiplier for a specific origin
    /// </summary>
    /// <param name="origin">Waste origin to check</param>
    /// <param name="resourceType">Resource type to get bonus for</param>
    /// <returns>Bonus multiplier</returns>
    public float GetOriginBonus(WasteOrigin origin, ResourceType resourceType)
    {
        var originMapping = originMappings.FirstOrDefault(o => o.origin == origin);
        if (originMapping == null)
            return 1f;
        
        var bonus = originMapping.resourceBonuses.FirstOrDefault(b => b.resourceType == resourceType);
        return bonus?.bonusMultiplier ?? 1f;
    }
    
    /// <summary>
    /// Get contamination penalty for a contamination level
    /// </summary>
    /// <param name="contaminationLevel">Contamination level (0-1)</param>
    /// <param name="resourceType">Resource type to get penalty for</param>
    /// <returns>Penalty multiplier (less than 1 = penalty)</returns>
    public float GetContaminationPenalty(float contaminationLevel, ResourceType resourceType)
    {
        var contaminationMapping = contaminationMappings.FirstOrDefault(c => c.resourceType == resourceType);
        if (contaminationMapping == null)
            return 1f;
        
        // Linear interpolation between no contamination and max contamination
        float penalty = Mathf.Lerp(1f, contaminationMapping.maxPenalty, contaminationLevel);
        return penalty;
    }
    
    private List<WasteToResourceMapping> GetMappingsForWasteType(WasteType wasteType)
    {
        return mappings.Where(m => m.wasteType == wasteType).ToList();
    }
    
    private ResourceYield CalculateYield(WasteItem wasteItem, WasteToResourceMapping mapping)
    {
        var yield = new ResourceYield
        {
            resourceType = mapping.targetResource,
            baseAmount = mapping.baseYield,
            yieldMultiplier = mapping.yieldMultiplier,
            chancePercentage = mapping.successChance
        };
        
        // Apply rarity modifiers
        yield.yieldMultiplier *= GetRarityMultiplier(wasteItem.Rarity);
        
        // Apply quality modifiers
        yield.yieldMultiplier *= GetQualityMultiplier(wasteItem.RecyclingPotential);
        
        return yield;
    }
    
    private void ApplyOriginBonuses(WasteItem wasteItem, Dictionary<ResourceType, ResourceYield> yields)
    {
        var origin = wasteItem.Origin;
        
        foreach (var yieldPair in yields.ToList())
        {
            float bonus = GetOriginBonus(origin, yieldPair.Key);
            var updatedYield = yieldPair.Value;
            updatedYield.yieldMultiplier *= bonus;
            yields[yieldPair.Key] = updatedYield;
        }
    }
    
    private void ApplyContaminationPenalties(WasteItem wasteItem, Dictionary<ResourceType, ResourceYield> yields)
    {
        float contaminationLevel = wasteItem.ContaminationLevel;
        
        foreach (var yieldPair in yields.ToList())
        {
            float penalty = GetContaminationPenalty(contaminationLevel, yieldPair.Key);
            var updatedYield = yieldPair.Value;
            updatedYield.yieldMultiplier *= penalty;
            yields[yieldPair.Key] = updatedYield;
        }
    }
    
    private float GetRarityMultiplier(WasteRarity rarity)
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
    
    private float GetQualityMultiplier(float recyclingPotential)
    {
        // Higher recycling potential = better yield
        return Mathf.Lerp(0.5f, 1.5f, recyclingPotential);
    }
    
    /// <summary>
    /// Add a new waste to resource mapping
    /// </summary>
    /// <param name="mapping">Mapping to add</param>
    public void AddMapping(WasteToResourceMapping mapping)
    {
        if (!mappings.Contains(mapping))
        {
            mappings.Add(mapping);
        }
    }
    
    /// <summary>
    /// Remove a waste to resource mapping
    /// </summary>
    /// <param name="mapping">Mapping to remove</param>
    public void RemoveMapping(WasteToResourceMapping mapping)
    {
        mappings.Remove(mapping);
    }
    
    /// <summary>
    /// Validate all mappings for consistency
    /// </summary>
    /// <returns>Validation result</returns>
    public MappingValidationResult ValidateMappings()
    {
        var result = new MappingValidationResult();
        
        // Check for duplicate mappings
        var duplicates = mappings.GroupBy(m => new { m.wasteType, m.targetResource })
                                .Where(g => g.Count() > 1)
                                .Select(g => g.Key);
        
        foreach (var duplicate in duplicates)
        {
            result.warnings.Add($"Duplicate mapping found: {duplicate.wasteType} -> {duplicate.targetResource}");
        }
        
        // Check for invalid yield values
        foreach (var mapping in mappings)
        {
            if (mapping.baseYield <= 0)
            {
                result.errors.Add($"Invalid base yield for {mapping.wasteType} -> {mapping.targetResource}");
            }
            
            if (mapping.successChance <= 0 || mapping.successChance > 100)
            {
                result.errors.Add($"Invalid success chance for {mapping.wasteType} -> {mapping.targetResource}");
            }
        }
        
        result.isValid = result.errors.Count == 0;
        return result;
    }
    
    private void OnValidate()
    {
        // Ensure valid values
        defaultYieldMultiplier = Mathf.Max(0.1f, defaultYieldMultiplier);
        
        // Validate mappings
        foreach (var mapping in mappings)
        {
            mapping.baseYield = Mathf.Max(0, mapping.baseYield);
            mapping.yieldMultiplier = Mathf.Max(0.1f, mapping.yieldMultiplier);
            mapping.successChance = Mathf.Clamp(mapping.successChance, 0f, 100f);
        }
    }
}

/// <summary>
/// Mapping from waste type to resource type
/// </summary>
[System.Serializable]
public class WasteToResourceMapping
{
    public WasteType wasteType;
    public ResourceType targetResource;
    public int baseYield = 1;
    public float yieldMultiplier = 1f;
    public float successChance = 100f;
    public bool requiresSpecialProcessing = false;
    
    public override bool Equals(object obj)
    {
        if (obj is WasteToResourceMapping other)
        {
            return wasteType == other.wasteType && targetResource == other.targetResource;
        }
        return false;
    }
    
    public override int GetHashCode()
    {
        return wasteType.GetHashCode() ^ targetResource.GetHashCode();
    }
}

/// <summary>
/// Origin-based resource bonuses
/// </summary>
[System.Serializable]
public class OriginBasedMapping
{
    public WasteOrigin origin;
    public List<ResourceBonus> resourceBonuses = new List<ResourceBonus>();
}

/// <summary>
/// Resource bonus definition
/// </summary>
[System.Serializable]
public class ResourceBonus
{
    public ResourceType resourceType;
    public float bonusMultiplier = 1f;
}

/// <summary>
/// Contamination penalty mapping
/// </summary>
[System.Serializable]
public class ContaminationMapping
{
    public ResourceType resourceType;
    public float maxPenalty = 0.5f; // At 100% contamination
}

/// <summary>
/// Mapping validation result
/// </summary>
[System.Serializable]
public class MappingValidationResult
{
    public bool isValid = true;
    public List<string> errors = new List<string>();
    public List<string> warnings = new List<string>();
    
    public override string ToString()
    {
        if (isValid)
            return "All mappings are valid";
        
        return $"Validation failed: {errors.Count} errors, {warnings.Count} warnings";
    }
} 