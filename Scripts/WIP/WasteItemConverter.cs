using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Utility class to convert old WasteItem objects to new UpdatedWasteItem format
/// Preserves all existing data while adding new resource yield functionality
/// </summary>
public static class WasteItemConverter
{
    /// <summary>
    /// Convert a legacy WasteItem to the new UpdatedWasteItem format
    /// </summary>
    public static UpdatedWasteItem ConvertToUpdatedWasteItem(WasteItem legacyItem)
    {
        if (legacyItem == null) return null;
        
        // Create new updated waste item
        var updatedItem = new UpdatedWasteItem(
            legacyItem.Name,
            legacyItem.DimensionalOrigin,
            legacyItem.Rarity,
            legacyItem.Icon
        );
        
        // Copy all properties
        updatedItem.Description = legacyItem.Description;
        updatedItem.Quantity = legacyItem.Quantity;
        
        // Map legacy properties to new system
        updatedItem.WasteStability = legacyItem.WasteStability;
        updatedItem.ContaminationLevel = legacyItem.ContaminationLevel;
        updatedItem.RecyclingPotential = legacyItem.RecyclingPotential;
        
        // Initialize the new resource yield system
        updatedItem.InitializeProperties();
        
        Debug.Log($"Converted legacy waste item: {legacyItem.Name} → {updatedItem.GetResourcePreview()}");
        
        return updatedItem;
    }
    
    /// <summary>
    /// Convert a list of legacy WasteItems to UpdatedWasteItems
    /// </summary>
    public static List<UpdatedWasteItem> ConvertWasteItemList(List<WasteItem> legacyItems)
    {
        var convertedItems = new List<UpdatedWasteItem>();
        
        if (legacyItems == null) return convertedItems;
        
        foreach (var legacyItem in legacyItems)
        {
            var converted = ConvertToUpdatedWasteItem(legacyItem);
            if (converted != null)
            {
                convertedItems.Add(converted);
            }
        }
        
        Debug.Log($"Converted {convertedItems.Count} legacy waste items to new format");
        return convertedItems;
    }
    
    /// <summary>
    /// Convert UpdatedWasteItem back to legacy WasteItem (for backward compatibility)
    /// </summary>
    public static WasteItem ConvertToLegacyWasteItem(UpdatedWasteItem updatedItem)
    {
        if (updatedItem == null) return null;
        
        // Create legacy waste item
        var legacyItem = new WasteItem(
            updatedItem.Name,
            updatedItem.DimensionalOrigin,
            updatedItem.Rarity,
            updatedItem.Icon
        );
        
        // Copy properties
        legacyItem.Description = updatedItem.Description;
        legacyItem.Quantity = updatedItem.Quantity;
        legacyItem.WasteStability = updatedItem.WasteStability;
        legacyItem.ContaminationLevel = updatedItem.ContaminationLevel;
        legacyItem.RecyclingPotential = updatedItem.RecyclingPotential;
        
        // Calculate legacy recycling value from resource yield
        legacyItem.RecyclingValue = CalculateLegacyRecyclingValue(updatedItem);
        
        return legacyItem;
    }
    
    /// <summary>
    /// Calculate equivalent legacy recycling value from new resource yield
    /// </summary>
    private static float CalculateLegacyRecyclingValue(UpdatedWasteItem updatedItem)
    {
        if (updatedItem.ResourceYield == null) return 1f;
        
        float totalValue = 0f;
        
        // Convert resource amounts to legacy recycling value
        foreach (var resource in updatedItem.ResourceYield.primaryResources)
        {
            float resourceValue = GetLegacyValueForResource(resource.type, resource.amount);
            totalValue += resourceValue;
        }
        
        // Add potential value from secondary resources (reduced by chance)
        foreach (var chance in updatedItem.ResourceYield.secondaryResources)
        {
            float resourceValue = GetLegacyValueForResource(chance.type, chance.amount);
            totalValue += resourceValue * chance.chance; // Multiply by chance
        }
        
        return Mathf.Max(0.1f, totalValue / 10f); // Convert back to legacy scale
    }
    
    /// <summary>
    /// Get legacy recycling point value for a resource type and amount
    /// </summary>
    private static float GetLegacyValueForResource(ResourceType type, int amount)
    {
        float baseValue = type switch
        {
            ResourceType.Plastic => 10f,        // 1 Plastic = 10 RP
            ResourceType.MetalScraps => 20f,    // 1 Metal = 20 RP
            ResourceType.OrganicMatter => 10f,  // 1 Organic = 10 RP
            ResourceType.CrystalFragments => 50f, // 1 Crystal = 50 RP (high value)
            ResourceType.NeuralResidue => 30f,  // 1 Neural = 30 RP
            ResourceType.ToxicSludge => 25f,    // 1 Sludge = 25 RP
            ResourceType.Fuel => 15f,           // Processed resources worth more
            ResourceType.Food => 12f,
            ResourceType.Parts => 25f,
            ResourceType.Energy => 40f,
            _ => 10f
        };
        
        return baseValue * amount;
    }
    
    /// <summary>
    /// Batch convert all waste items in the inventory manager
    /// </summary>
    public static void ConvertInventoryToNewSystem()
    {
        if (WasteInventoryManager.Instance == null)
        {
            Debug.LogWarning("WasteInventoryManager.Instance not found - cannot convert inventory");
            return;
        }
        
        var legacyItems = WasteInventoryManager.Instance.GetAllWaste();
        var convertedItems = ConvertWasteItemList(legacyItems);
        
        Debug.Log($"Inventory conversion complete: {legacyItems.Count} legacy items → {convertedItems.Count} updated items");
        
        // You could store the converted items somewhere or trigger an event here
        // For now, we just log the conversion
    }
    
    /// <summary>
    /// Create a mapping between legacy and new waste items for reference
    /// </summary>
    public static Dictionary<string, UpdatedWasteItem> CreateConversionMapping(List<WasteItem> legacyItems)
    {
        var mapping = new Dictionary<string, UpdatedWasteItem>();
        
        foreach (var legacyItem in legacyItems)
        {
            var converted = ConvertToUpdatedWasteItem(legacyItem);
            if (converted != null)
            {
                mapping[legacyItem.Id] = converted;
            }
        }
        
        return mapping;
    }
    
    /// <summary>
    /// Validate that conversion preserves important data
    /// </summary>
    public static bool ValidateConversion(WasteItem original, UpdatedWasteItem converted)
    {
        if (original == null || converted == null) return false;
        
        bool isValid = true;
        
        // Check basic properties
        if (original.Name != converted.Name)
        {
            Debug.LogWarning($"Name mismatch: {original.Name} != {converted.Name}");
            isValid = false;
        }
        
        if (original.DimensionalOrigin != converted.DimensionalOrigin)
        {
            Debug.LogWarning($"Origin mismatch: {original.DimensionalOrigin} != {converted.DimensionalOrigin}");
            isValid = false;
        }
        
        if (original.Rarity != converted.Rarity)
        {
            Debug.LogWarning($"Rarity mismatch: {original.Rarity} != {converted.Rarity}");
            isValid = false;
        }
        
        // Check gameplay properties (allow small floating point differences)
        if (Mathf.Abs(original.WasteStability - converted.WasteStability) > 0.01f)
        {
            Debug.LogWarning($"Stability mismatch: {original.WasteStability} != {converted.WasteStability}");
            isValid = false;
        }
        
        if (isValid)
        {
            Debug.Log($"Conversion validation passed for: {original.Name}");
        }
        
        return isValid;
    }
    
    /// <summary>
    /// Get conversion statistics for debugging
    /// </summary>
    public static ConversionStats GetConversionStats(List<WasteItem> legacyItems)
    {
        var stats = new ConversionStats();
        
        foreach (var item in legacyItems)
        {
            stats.totalItems++;
            
            switch (item.Rarity)
            {
                case WasteRarity.Common:
                    stats.commonItems++;
                    break;
                case WasteRarity.Uncommon:
                    stats.uncommonItems++;
                    break;
                case WasteRarity.Rare:
                    stats.rareItems++;
                    break;
                case WasteRarity.Epic:
                    stats.epicItems++;
                    break;
                case WasteRarity.Legendary:
                    stats.legendaryItems++;
                    break;
            }
            
            // Track dimensional origins
            if (!stats.dimensionalOrigins.ContainsKey(item.DimensionalOrigin))
            {
                stats.dimensionalOrigins[item.DimensionalOrigin] = 0;
            }
            stats.dimensionalOrigins[item.DimensionalOrigin]++;
        }
        
        return stats;
    }
}

/// <summary>
/// Statistics about waste item conversion
/// </summary>
[System.Serializable]
public class ConversionStats
{
    public int totalItems;
    public int commonItems;
    public int uncommonItems;
    public int rareItems;
    public int epicItems;
    public int legendaryItems;
    public Dictionary<string, int> dimensionalOrigins = new Dictionary<string, int>();
    
    public override string ToString()
    {
        return $"Conversion Stats: {totalItems} total items " +
               $"(C:{commonItems}, U:{uncommonItems}, R:{rareItems}, E:{epicItems}, L:{legendaryItems}) " +
               $"from {dimensionalOrigins.Count} dimensional origins";
    }
} 