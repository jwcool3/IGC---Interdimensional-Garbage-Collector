using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class UpdatedWasteItem
{
    // Core properties (unchanged)
    public string Id { get; private set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public WasteRarity Rarity { get; set; }
    public string DimensionalOrigin { get; set; }
    public int Quantity { get; set; } = 1;
    public Sprite Icon { get; set; }

    // Gameplay properties (unchanged)
    public float WasteStability { get; set; }
    public float ContaminationLevel { get; set; }
    public float RecyclingPotential { get; set; }

    // NEW: Resource yield potential
    public ResourceYield ResourceYield { get; private set; }

    // Visual properties
    public Color RarityColor => GetRarityColor();

    private bool propertiesInitialized = false;

    public UpdatedWasteItem()
    {
        Id = Guid.NewGuid().ToString();
    }

    public UpdatedWasteItem(string name, string dimensionalOrigin, WasteRarity rarity = WasteRarity.Common, Sprite icon = null)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        DimensionalOrigin = dimensionalOrigin;
        Rarity = rarity;
        Icon = icon;

        InitializeProperties();
    }

    public void InitializeProperties()
    {
        if (!propertiesInitialized)
        {
            WasteStability = CalculateInitialStability();
            ContaminationLevel = CalculateInitialContamination();
            RecyclingPotential = CalculateRecyclingPotential();
            
            // NEW: Generate resource yield based on properties
            ResourceYield = GenerateResourceYield();
            
            propertiesInitialized = true;
        }
    }

    /// <summary>
    /// Generate what resources this waste item will yield when processed
    /// </summary>
    private ResourceYield GenerateResourceYield()
    {
        ResourceYield yield = new ResourceYield();
        List<ResourceAmount> primaryResources = new List<ResourceAmount>();
        List<ResourceChance> secondaryResources = new List<ResourceChance>();

        // Base resource generation based on dimensional origin
        var baseResources = GetBaseResourcesForOrigin(DimensionalOrigin);
        
        // Calculate total value budget based on rarity
        int totalValueBudget = GetValueBudgetForRarity(Rarity);
        
        // Apply stability and recycling potential modifiers
        float stabilityMultiplier = WasteStability; // 0.2 to 1.0
        float potentialMultiplier = 0.5f + (RecyclingPotential * 0.5f); // 0.5 to 1.0
        
        float finalMultiplier = stabilityMultiplier * potentialMultiplier;
        int adjustedBudget = Mathf.RoundToInt(totalValueBudget * finalMultiplier);
        
        // Distribute budget across base resources
        DistributeResourceBudget(baseResources, adjustedBudget, primaryResources, secondaryResources);
        
        // Set contamination risk
        yield.contaminationRisk = ContaminationLevel * GetContaminationRiskForOrigin(DimensionalOrigin);
        
        yield.primaryResources = primaryResources.ToArray();
        yield.secondaryResources = secondaryResources.ToArray();
        
        return yield;
    }

    /// <summary>
    /// Get base resource types that this dimensional origin can produce
    /// </summary>
    private ResourceType[] GetBaseResourcesForOrigin(string origin)
    {
        return origin.ToLower() switch
        {
            var x when x.Contains("earth") => new[] { ResourceType.Plastic, ResourceType.MetalScraps, ResourceType.OrganicMatter },
            
            var x when x.Contains("technological") => new[] { ResourceType.MetalScraps, ResourceType.Plastic, ResourceType.CrystalFragments },
            
            var x when x.Contains("biological") => new[] { ResourceType.OrganicMatter, ResourceType.NeuralResidue, ResourceType.ToxicSludge },
            
            var x when x.Contains("quantum") => new[] { ResourceType.CrystalFragments, ResourceType.ToxicSludge, ResourceType.NeuralResidue },
            
            var x when x.Contains("philosophical") => new[] { ResourceType.NeuralResidue, ResourceType.CrystalFragments },
            
            var x when x.Contains("cosmic") => new[] { ResourceType.CrystalFragments, ResourceType.ToxicSludge, ResourceType.MetalScraps },
            
            var x when x.Contains("temporal") => new[] { ResourceType.ToxicSludge, ResourceType.CrystalFragments, ResourceType.NeuralResidue },
            
            var x when x.Contains("ethereal") => new[] { ResourceType.CrystalFragments, ResourceType.NeuralResidue },
            
            var x when x.Contains("archaeological") => new[] { ResourceType.MetalScraps, ResourceType.CrystalFragments, ResourceType.OrganicMatter },
            
            _ => new[] { ResourceType.Plastic, ResourceType.OrganicMatter } // Default fallback
        };
    }

    /// <summary>
    /// Get total resource value budget based on item rarity
    /// </summary>
    private int GetValueBudgetForRarity(WasteRarity rarity)
    {
        return rarity switch
        {
            WasteRarity.Common => UnityEngine.Random.Range(1, 3),      // 1-2 value units
            WasteRarity.Uncommon => UnityEngine.Random.Range(3, 6),   // 3-5 value units
            WasteRarity.Rare => UnityEngine.Random.Range(6, 9),       // 6-8 value units
            WasteRarity.Epic => UnityEngine.Random.Range(8, 12),      // 8-11 value units
            WasteRarity.Legendary => UnityEngine.Random.Range(10, 16), // 10-15 value units
            _ => 1
        };
    }

    /// <summary>
    /// Get contamination risk multiplier for different origins
    /// </summary>
    private float GetContaminationRiskForOrigin(string origin)
    {
        return origin.ToLower() switch
        {
            var x when x.Contains("earth") => 0.1f,
            var x when x.Contains("technological") => 0.2f,
            var x when x.Contains("biological") => 0.4f,
            var x when x.Contains("quantum") => 0.6f,
            var x when x.Contains("philosophical") => 0.1f,
            var x when x.Contains("cosmic") => 0.5f,
            var x when x.Contains("temporal") => 0.8f,
            var x when x.Contains("ethereal") => 0.2f,
            var x when x.Contains("archaeological") => 0.3f,
            _ => 0.2f
        };
    }

    /// <summary>
    /// Distribute value budget across available resource types
    /// </summary>
    private void DistributeResourceBudget(ResourceType[] baseResources, int budget, 
        List<ResourceAmount> primary, List<ResourceChance> secondary)
    {
        if (budget <= 0 || baseResources.Length == 0) return;

        // Resource values for budget calculation
        Dictionary<ResourceType, int> resourceValues = new Dictionary<ResourceType, int>
        {
            { ResourceType.Plastic, 1 },
            { ResourceType.OrganicMatter, 1 },
            { ResourceType.MetalScraps, 2 },
            { ResourceType.ToxicSludge, 2 },
            { ResourceType.NeuralResidue, 3 },
            { ResourceType.CrystalFragments, 4 }
        };

        int remainingBudget = budget;
        
        // Always generate at least one primary resource
        ResourceType primaryType = baseResources[UnityEngine.Random.Range(0, baseResources.Length)];
        int primaryValue = resourceValues.GetValueOrDefault(primaryType, 1);
        int primaryAmount = Mathf.Max(1, remainingBudget / primaryValue);
        
        primary.Add(new ResourceAmount(primaryType, primaryAmount));
        remainingBudget -= primaryAmount * primaryValue;

        // If we have remaining budget and this is a higher rarity item, add secondary resources
        if (remainingBudget > 0 && (int)Rarity >= (int)WasteRarity.Uncommon)
        {
            foreach (var resourceType in baseResources)
            {
                if (resourceType == primaryType) continue; // Skip primary resource
                
                int resourceValue = resourceValues.GetValueOrDefault(resourceType, 1);
                if (remainingBudget >= resourceValue)
                {
                    int amount = remainingBudget / resourceValue;
                    float chance = 0.3f + ((int)Rarity * 0.15f); // 30% base + 15% per rarity level
                    
                    secondary.Add(new ResourceChance(resourceType, amount, chance));
                    break; // Only add one secondary resource to keep it balanced
                }
            }
        }
    }

    /// <summary>
    /// Get a preview of what resources this item would yield (for UI display)
    /// </summary>
    public string GetResourcePreview()
    {
        if (ResourceYield == null) return "Unknown yield";

        List<string> preview = new List<string>();
        
        foreach (var resource in ResourceYield.primaryResources)
        {
            preview.Add($"{resource.amount} {resource.type}");
        }
        
        foreach (var chance in ResourceYield.secondaryResources)
        {
            preview.Add($"{chance.amount} {chance.type} ({chance.chance:P0} chance)");
        }
        
        return string.Join(", ", preview);
    }

    // Keep existing methods for compatibility
    public void SetQuantity(int newQuantity)
    {
        Quantity = Mathf.Max(0, newQuantity);
    }

    public void AddQuantity(int amount = 1)
    {
        Quantity = Mathf.Max(0, Quantity + amount);
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

    private float CalculateInitialStability()
    {
        float baseStability = 0.5f + ((int)Rarity * 0.1f);
        if (Application.isPlaying)
        {
            baseStability += UnityEngine.Random.Range(-0.1f, 0.1f);
        }
        return Mathf.Clamp01(baseStability);
    }

    private float CalculateInitialContamination()
    {
        float baseContamination = 0.5f - ((int)Rarity * 0.1f);
        if (Application.isPlaying)
        {
            baseContamination += UnityEngine.Random.Range(-0.1f, 0.1f);
        }
        return Mathf.Clamp01(baseContamination);
    }

    private float CalculateRecyclingPotential()
    {
        float basePotential = 0.3f + ((int)Rarity * 0.15f);
        if (Application.isPlaying)
        {
            basePotential += UnityEngine.Random.Range(-0.1f, 0.1f);
        }
        return Mathf.Clamp01(basePotential);
    }

    public Color GetRarityColor()
    {
        switch (Rarity)
        {
            case WasteRarity.Common:
                return Color.gray;
            case WasteRarity.Uncommon:
                return Color.green;
            case WasteRarity.Rare:
                return Color.blue;
            case WasteRarity.Epic:
                return new Color(0.5f, 0f, 0.5f); // Purple
            case WasteRarity.Legendary:
                return Color.yellow;
            default:
                return Color.white;
        }
    }

    public bool CanStackWith(UpdatedWasteItem other)
    {
        if (other == null) return false;

        return Name == other.Name &&
               DimensionalOrigin == other.DimensionalOrigin &&
               Rarity == other.Rarity &&
               Mathf.Approximately(RecyclingPotential, other.RecyclingPotential) &&
               Mathf.Approximately(WasteStability, other.WasteStability) &&
               Mathf.Approximately(ContaminationLevel, other.ContaminationLevel);
    }
}