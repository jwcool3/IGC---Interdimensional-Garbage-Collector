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
        
        // Create new updated waste item using the proper constructor
        var updatedItem = new UpdatedWasteItem(
            legacyItem.Name,
            WasteType.Unknown, // Default type, will be mapped below
            legacyItem.Quantity
        );
        
        // Copy all properties
        updatedItem.Description = legacyItem.Description;
        updatedItem.Rarity = legacyItem.Rarity;
        updatedItem.Icon = legacyItem.Icon;
        updatedItem.DimensionalOrigin = legacyItem.DimensionalOrigin;
        
        // Map legacy properties to new system
        updatedItem.DimensionalStability = legacyItem.DimensionalStability;
        updatedItem.ContaminationLevel = legacyItem.ContaminationLevel;
        
        // Map waste type based on name or other properties
        updatedItem.Type = MapLegacyWasteType(legacyItem);
        
        Debug.Log($"Converted legacy waste item: {legacyItem.Name} → {updatedItem.Name}");
        
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
        
        // Create legacy waste item using the proper constructor
        var legacyItem = new WasteItem(
            updatedItem.Name,
            updatedItem.DimensionalOrigin,
            updatedItem.Rarity,
            updatedItem.Icon
        );
        
        // Copy properties
        legacyItem.Description = updatedItem.Description;
        legacyItem.Quantity = updatedItem.Quantity;
        legacyItem.DimensionalStability = updatedItem.DimensionalStability;
        legacyItem.ContaminationLevel = updatedItem.ContaminationLevel;
        
        // Calculate legacy recycling value from resource yield
        legacyItem.RecyclingValue = CalculateLegacyRecyclingValue(updatedItem);
        
        return legacyItem;
    }
    
    /// <summary>
    /// Map legacy waste item to new waste type system
    /// </summary>
    private static WasteType MapLegacyWasteType(WasteItem legacyItem)
    {
        string name = legacyItem.Name.ToLower();
        
        if (name.Contains("metal") || name.Contains("scrap")) return WasteType.Metal;
        if (name.Contains("plastic") || name.Contains("polymer")) return WasteType.Plastic;
        if (name.Contains("organic") || name.Contains("bio")) return WasteType.Organic;
        if (name.Contains("electronic") || name.Contains("circuit")) return WasteType.Electronics;
        if (name.Contains("glass")) return WasteType.Glass;
        if (name.Contains("paper") || name.Contains("cardboard")) return WasteType.Paper;
        if (name.Contains("chemical") || name.Contains("toxic")) return WasteType.Chemical;
        
        return WasteType.Unknown;
    }
    
    /// <summary>
    /// Calculate equivalent legacy recycling value from new resource yield
    /// </summary>
    private static float CalculateLegacyRecyclingValue(UpdatedWasteItem updatedItem)
    {
        if (updatedItem.resourceYield == null) return 1f;
        
        float totalValue = 0f;
        
        // Convert resource amounts to legacy recycling value
        if (updatedItem.resourceYield.primaryResources != null)
        {
            foreach (var resource in updatedItem.resourceYield.primaryResources)
            {
                float resourceValue = GetLegacyValueForResource(resource.type, resource.amount);
                totalValue += resourceValue;
            }
        }
        
        // Add potential value from secondary resources (reduced by chance)
        if (updatedItem.resourceYield.secondaryResources != null)
        {
            foreach (var chance in updatedItem.resourceYield.secondaryResources)
            {
                float resourceValue = GetLegacyValueForResource(chance.type, chance.amount);
                totalValue += resourceValue * chance.chance; // Multiply by chance
            }
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
        Debug.Log($"Inventory conversion complete: {legacyItems.Count} items already in new format");
        
        // Items are already in UpdatedWasteItem format, no conversion needed
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
        
        if (original.Quantity != converted.Quantity)
        {
            Debug.LogWarning($"Quantity mismatch: {original.Quantity} != {converted.Quantity}");
            isValid = false;
        }
        
        return isValid;
    }
    
    /// <summary>
    /// Get statistics about a conversion operation
    /// </summary>
    public static ConversionStats GetConversionStats(List<WasteItem> legacyItems)
    {
        var stats = new ConversionStats();
        
        foreach (var item in legacyItems)
        {
            stats.totalItems++;
            
            // Count by rarity
            switch (item.Rarity)
            {
                case WasteRarity.Common: stats.commonItems++; break;
                case WasteRarity.Uncommon: stats.uncommonItems++; break;
                case WasteRarity.Rare: stats.rareItems++; break;
                case WasteRarity.Epic: stats.epicItems++; break;
                case WasteRarity.Legendary: stats.legendaryItems++; break;
            }
            
            // Count by dimensional origin
            string origin = item.DimensionalOrigin ?? "Unknown";
            if (stats.dimensionalOrigins.ContainsKey(origin))
                stats.dimensionalOrigins[origin]++;
            else
                stats.dimensionalOrigins[origin] = 1;
        }
        
        return stats;
    }
}

/// <summary>
/// Statistics about a conversion operation
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
               $"(C:{commonItems}, U:{uncommonItems}, R:{rareItems}, E:{epicItems}, L:{legendaryItems})";
    }
} 