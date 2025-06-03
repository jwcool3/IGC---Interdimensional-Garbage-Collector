using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class UpdatedWasteItem
{
    // Core properties
    public string Id { get; private set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public WasteRarity Rarity { get; set; }
    public string DimensionalOrigin { get; set; }
    public int Quantity { get; set; } = 1;
    public Sprite Icon { get; set; }

    // NEW: Reference to the data template
    public UpdatedWasteItemData ItemData { get; private set; }

    // NEW: Waste type and origin from enums
    public WasteType Type { get; set; }
    public WasteOrigin Origin { get; set; }

    // Gameplay properties
    public float WasteStability { get; set; }
    public float ContaminationLevel { get; set; }
    public float RecyclingPotential { get; set; }
    public float Weight { get; set; }
    public float ProcessingTime { get; set; }
    public float Quality { get; set; }
    public float EstimatedValue { get; private set; }

    // NEW: Resource yield potential (now uses new system)
    public Dictionary<ResourceType, ResourceYield> ResourceYields { get; private set; }

    // Visual properties
    public Color RarityColor => GetRarityColor();

    private bool propertiesInitialized = false;

    public UpdatedWasteItem()
    {
        Id = Guid.NewGuid().ToString();
        ResourceYields = new Dictionary<ResourceType, ResourceYield>();
    }

    public UpdatedWasteItem(UpdatedWasteItemData itemData, WasteOrigin origin = WasteOrigin.Unknown)
    {
        Id = Guid.NewGuid().ToString();
        ItemData = itemData;
        ResourceYields = new Dictionary<ResourceType, ResourceYield>();
        
        // Set basic properties from data
        Name = itemData.ItemName;
        Description = itemData.Description;
        DimensionalOrigin = itemData.DimensionalOrigin;
        Icon = itemData.IconSprite;
        Type = itemData.WasteType;
        Origin = origin;
        Rarity = itemData.DefaultRarity;
        
        // Set base stats from data (will be modified during initialization)
        WasteStability = itemData.BaseWasteStability;
        ContaminationLevel = itemData.BaseContaminationLevel;
        RecyclingPotential = itemData.BaseRecyclingPotential;
        Weight = itemData.BaseWeight;
        ProcessingTime = itemData.BaseProcessingTime;

        InitializeProperties();
    }

    // Legacy constructor for backward compatibility
    public UpdatedWasteItem(string name, string dimensionalOrigin, WasteRarity rarity = WasteRarity.Common, Sprite icon = null)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        DimensionalOrigin = dimensionalOrigin;
        Rarity = rarity;
        Icon = icon;
        ResourceYields = new Dictionary<ResourceType, ResourceYield>();
        
        // Set default values for new properties
        Type = WasteType.Unknown;
        Origin = WasteOrigin.Unknown;
        Weight = 1f;
        ProcessingTime = 10f;

        InitializeProperties();
    }

    public void InitializeProperties()
    {
        if (!propertiesInitialized)
        {
            // Apply random variations to base stats
            if (ItemData != null)
            {
                WasteStability = ApplyVariation(ItemData.BaseWasteStability, 0.2f);
                ContaminationLevel = ApplyVariation(ItemData.BaseContaminationLevel, 0.3f);
                RecyclingPotential = ApplyVariation(ItemData.BaseRecyclingPotential, 0.25f);
                Weight = ApplyVariation(ItemData.BaseWeight, 0.15f);
                ProcessingTime = ApplyVariation(ItemData.BaseProcessingTime, 0.2f);
            }
            else
            {
                // Legacy initialization
                WasteStability = CalculateInitialStability();
                ContaminationLevel = CalculateInitialContamination();
                RecyclingPotential = CalculateRecyclingPotential();
            }
            
            // Calculate quality based on all factors
            Quality = CalculateQuality();
            
            // Generate resource yields
            ResourceYields = GenerateResourceYields();
            
            // Calculate estimated value
            EstimatedValue = CalculateEstimatedValue();
            
            propertiesInitialized = true;
        }
    }

    /// <summary>
    /// Generate what resources this waste item will yield when processed
    /// Now uses UpdatedWasteItemData.baseRawYields as the foundation
    /// </summary>
    private Dictionary<ResourceType, ResourceYield> GenerateResourceYields()
    {
        if (ItemData != null)
        {
            // Use the new data-driven approach
            return ItemData.GenerateResourceYields(Rarity, WasteStability, RecyclingPotential, ContaminationLevel);
        }
        else
        {
            // Legacy fallback for items created without ItemData
            return GenerateLegacyResourceYields();
        }
    }

    /// <summary>
    /// Legacy resource yield generation for backward compatibility
    /// </summary>
    private Dictionary<ResourceType, ResourceYield> GenerateLegacyResourceYields()
    {
        var yields = new Dictionary<ResourceType, ResourceYield>();
        
        // Get base resources from dimensional origin (legacy method)
        var baseResources = GetBaseResourcesForOrigin(DimensionalOrigin);
        
        // Calculate total value budget based on rarity
        int totalValueBudget = GetValueBudgetForRarity(Rarity);
        
        // Apply stability and recycling potential modifiers
        float stabilityMultiplier = WasteStability;
        float potentialMultiplier = 0.5f + (RecyclingPotential * 0.5f);
        float contaminationPenalty = Mathf.Lerp(1f, 0.3f, ContaminationLevel);
        
        float finalMultiplier = stabilityMultiplier * potentialMultiplier * contaminationPenalty;
        int adjustedBudget = Mathf.RoundToInt(totalValueBudget * finalMultiplier);
        
        // Convert to new yield format
        if (baseResources.Length > 0)
        {
            ResourceType primaryType = baseResources[UnityEngine.Random.Range(0, baseResources.Length)];
            yields[primaryType] = new ResourceYield
            {
                resourceType = primaryType,
                baseAmount = Mathf.Max(1, adjustedBudget / 2),
                yieldMultiplier = finalMultiplier,
                chancePercentage = 100f
            };
            
            // Add secondary resources for higher rarity items
            if ((int)Rarity >= (int)WasteRarity.Uncommon && baseResources.Length > 1)
            {
                foreach (var resourceType in baseResources)
                {
                    if (resourceType != primaryType && yields.Count < 3)
                    {
                        yields[resourceType] = new ResourceYield
                        {
                            resourceType = resourceType,
                            baseAmount = 1,
                            yieldMultiplier = finalMultiplier * 0.5f,
                            chancePercentage = 30f + ((int)Rarity * 15f)
                        };
                    }
                }
            }
        }
        
        return yields;
    }

    /// <summary>
    /// Apply random variation to a base value
    /// </summary>
    private float ApplyVariation(float baseValue, float variationRange)
    {
        float variation = UnityEngine.Random.Range(-variationRange, variationRange);
        return Mathf.Clamp01(baseValue + variation);
    }

    /// <summary>
    /// Calculate overall quality based on all factors
    /// </summary>
    private float CalculateQuality()
    {
        float stabilityFactor = WasteStability * 0.3f;
        float recyclingFactor = RecyclingPotential * 0.4f;
        float contaminationPenalty = (1f - ContaminationLevel) * 0.3f;
        
        return Mathf.Clamp01(stabilityFactor + recyclingFactor + contaminationPenalty);
    }

    /// <summary>
    /// Calculate estimated value based on resource yields
    /// </summary>
    private float CalculateEstimatedValue()
    {
        float totalValue = 0f;
        
        foreach (var yieldPair in ResourceYields)
        {
            var yield = yieldPair.Value;
            float resourceValue = GetResourceBaseValue(yield.resourceType);
            float expectedAmount = yield.baseAmount * yield.yieldMultiplier * (yield.chancePercentage / 100f);
            totalValue += expectedAmount * resourceValue;
        }
        
        // Apply rarity multiplier
        totalValue *= GetRarityValueMultiplier(Rarity);
        
        return totalValue;
    }

    /// <summary>
    /// Get base value for a resource type
    /// </summary>
    private float GetResourceBaseValue(ResourceType resourceType)
    {
        // This should eventually use ResourceConfigManager, but for now use simple values
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

    /// <summary>
    /// Get rarity value multiplier
    /// </summary>
    private float GetRarityValueMultiplier(WasteRarity rarity)
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

    /// <summary>
    /// Check if this item can stack with another
    /// </summary>
    /// <param name="other">Other waste item to check</param>
    /// <returns>True if they can stack</returns>
    public bool CanStackWith(UpdatedWasteItem other)
    {
        if (other == null) return false;
        
        // Items can stack if they have the same core properties
        return Name == other.Name &&
               Type == other.Type &&
               Origin == other.Origin &&
               Rarity == other.Rarity &&
               DimensionalOrigin == other.DimensionalOrigin &&
               Mathf.Approximately(Quality, other.Quality);
    }

    /// <summary>
    /// Create a copy of this waste item
    /// </summary>
    /// <returns>New UpdatedWasteItem instance</returns>
    public UpdatedWasteItem CreateCopy()
    {
        var copy = new UpdatedWasteItem();
        copy.Id = Guid.NewGuid().ToString(); // New ID for the copy
        copy.Name = Name;
        copy.Description = Description;
        copy.Rarity = Rarity;
        copy.DimensionalOrigin = DimensionalOrigin;
        copy.Quantity = Quantity;
        copy.Icon = Icon;
        copy.ItemData = ItemData;
        copy.Type = Type;
        copy.Origin = Origin;
        copy.WasteStability = WasteStability;
        copy.ContaminationLevel = ContaminationLevel;
        copy.RecyclingPotential = RecyclingPotential;
        copy.Weight = Weight;
        copy.ProcessingTime = ProcessingTime;
        copy.Quality = Quality;
        copy.EstimatedValue = EstimatedValue;
        copy.ResourceYields = new Dictionary<ResourceType, ResourceYield>(ResourceYields);
        copy.propertiesInitialized = propertiesInitialized;
        
        return copy;
    }

    /// <summary>
    /// Get base resource types that this dimensional origin can produce (legacy)
    /// </summary>
    private ResourceType[] GetBaseResourcesForOrigin(string origin)
    {
        return origin.ToLower() switch
        {
            var x when x.Contains("earth") => new[] { ResourceType.Plastic, ResourceType.MetalScraps, ResourceType.OrganicMatter },
            var x when x.Contains("technological") => new[] { ResourceType.MetalScraps, ResourceType.Plastic, ResourceType.Electronics },
            var x when x.Contains("biological") => new[] { ResourceType.OrganicMatter, ResourceType.Biomass },
            var x when x.Contains("quantum") => new[] { ResourceType.QuantumMatter, ResourceType.Crystals },
            var x when x.Contains("cosmic") => new[] { ResourceType.Crystals, ResourceType.RareMetals },
            _ => new[] { ResourceType.Plastic, ResourceType.OrganicMatter }
        };
    }

    /// <summary>
    /// Get total resource value budget based on item rarity (legacy)
    /// </summary>
    private int GetValueBudgetForRarity(WasteRarity rarity)
    {
        return rarity switch
        {
            WasteRarity.Common => UnityEngine.Random.Range(1, 3),
            WasteRarity.Uncommon => UnityEngine.Random.Range(3, 6),
            WasteRarity.Rare => UnityEngine.Random.Range(6, 9),
            WasteRarity.Epic => UnityEngine.Random.Range(8, 12),
            WasteRarity.Legendary => UnityEngine.Random.Range(10, 16),
            _ => 1
        };
    }

    private float CalculateInitialStability()
    {
        float baseStability = 0.5f;
        float rarityBonus = (int)Rarity * 0.1f;
        float randomVariation = UnityEngine.Random.Range(-0.2f, 0.2f);
        return Mathf.Clamp01(baseStability + rarityBonus + randomVariation);
    }

    private float CalculateInitialContamination()
    {
        float baseContamination = 0.3f;
        float rarityReduction = (int)Rarity * 0.05f;
        float randomVariation = UnityEngine.Random.Range(-0.1f, 0.3f);
        return Mathf.Clamp01(baseContamination - rarityReduction + randomVariation);
    }

    private float CalculateRecyclingPotential()
    {
        float basePotential = 0.4f;
        float stabilityBonus = WasteStability * 0.3f;
        float contaminationPenalty = ContaminationLevel * 0.2f;
        float rarityBonus = (int)Rarity * 0.1f;
        return Mathf.Clamp01(basePotential + stabilityBonus - contaminationPenalty + rarityBonus);
    }

    public Color GetRarityColor()
    {
        return Rarity switch
        {
            WasteRarity.Common => Color.white,
            WasteRarity.Uncommon => Color.green,
            WasteRarity.Rare => Color.blue,
            WasteRarity.Epic => Color.magenta,
            WasteRarity.Legendary => Color.yellow,
            _ => Color.gray
        };
    }

    public void SetQuantity(int newQuantity)
    {
        Quantity = Mathf.Max(0, newQuantity);
    }

    public void AddQuantity(int amount = 1)
    {
        Quantity += amount;
    }

    public bool RemoveQuantity(int amount = 1)
    {
        if (Quantity >= amount)
        {
            Quantity -= amount;
            return true;
        }
        return false;
    }

    public void SetIcon(Sprite newIcon)
    {
        Icon = newIcon;
    }

    public string GetResourcePreview()
    {
        if (ResourceYields.Count == 0)
            return "No resources available";

        var preview = "Potential yields:\n";
        foreach (var yieldPair in ResourceYields.OrderByDescending(y => y.Value.baseAmount))
        {
            var yield = yieldPair.Value;
            preview += $"• {yield.resourceType}: {yield.baseAmount} ({yield.chancePercentage:F0}%)\n";
        }
        
        return preview.TrimEnd('\n');
    }
}