using System.Collections.Generic;
using UnityEngine;

public class WasteGenerator : MonoBehaviour
{
    [System.Serializable]
    public class DimensionType
    {
        public string Name;
        public float CommonChance = 0.6f;
        public float UncommonChance = 0.25f;
        public float RareChance = 0.1f;
        public float EpicChance = 0.04f;
        public float LegendaryChance = 0.01f;
    }

    [SerializeField] private List<DimensionType> dimensionTypes = new List<DimensionType>
    {
        new DimensionType { Name = "Technological Waste" },
        new DimensionType { Name = "Biological Remnants" },
        new DimensionType { Name = "Quantum Residue" },
        new DimensionType { Name = "Philosophical Byproducts" },
        new DimensionType { Name = "Cosmic Debris" }
    };

    // Dimension types for waste generation
    private readonly string[] WasteOrigins = 
    {
        "Abandoned",
        "Discarded",
        "Rejected",
        "Obsolete",
        "Malfunctioning",
        "Corrupted",
        "Unstable",
        "Contaminated"
    };

    private readonly string[] WasteTypes = 
    {
        "Machinery",
        "Biomatter",
        "Energy Core",
        "Circuit Array",
        "Containment Unit",
        "Processing Node",
        "Storage System",
        "Filter Component"
    };

    private readonly string[] wasteNames = new string[]
    {
        "Debris",
        "Residue",
        "Matter",
        "Fragment",
        "Essence"
    };

    // Generate a single waste item
    public WasteItem GenerateWasteItem()
    {
        var dimension = GetRandomDimension();
        var rarity = GenerateRarity(dimension);
        
        return new WasteItem(
            GenerateName(dimension.Name, rarity),
            dimension.Name,
            rarity
        );
    }

    // Generate multiple waste items
    public List<WasteItem> GenerateMultipleWaste(int count)
    {
        var items = new List<WasteItem>();
        for (int i = 0; i < count; i++)
        {
            items.Add(GenerateWasteItem());
        }
        return items;
    }

    private DimensionType GetRandomDimension()
    {
        return dimensionTypes[Random.Range(0, dimensionTypes.Count)];
    }

    private WasteRarity GenerateRarity(DimensionType dimension)
    {
        float roll = Random.value;
        
        if (roll < dimension.LegendaryChance)
            return WasteRarity.Legendary;
        if (roll < dimension.LegendaryChance + dimension.EpicChance)
            return WasteRarity.Epic;
        if (roll < dimension.LegendaryChance + dimension.EpicChance + dimension.RareChance)
            return WasteRarity.Rare;
        if (roll < dimension.LegendaryChance + dimension.EpicChance + dimension.RareChance + dimension.UncommonChance)
            return WasteRarity.Uncommon;
            
        return WasteRarity.Common;
    }

    private string GenerateName(string dimensionName, WasteRarity rarity)
    {
        string[] prefixes = GetPrefixesForRarity(rarity);
        string[] suffixes = { "Waste", "Debris", "Residue", "Matter", "Substance" };
        
        string prefix = prefixes[Random.Range(0, prefixes.Length)];
        string suffix = suffixes[Random.Range(0, suffixes.Length)];
        
        return $"{prefix} {dimensionName} {suffix}";
    }

    private string[] GetPrefixesForRarity(WasteRarity rarity)
    {
        switch (rarity)
        {
            case WasteRarity.Legendary:
                return new[] { "Mythical", "Ancient", "Legendary", "Divine" };
            case WasteRarity.Epic:
                return new[] { "Mystical", "Ethereal", "Transcendent" };
            case WasteRarity.Rare:
                return new[] { "Unstable", "Exotic", "Peculiar" };
            case WasteRarity.Uncommon:
                return new[] { "Unusual", "Strange", "Curious" };
            default:
                return new[] { "Common", "Basic", "Standard" };
        }
    }

    // Enhanced waste name generation
    private string GenerateWasteName()
    {
        string origin = WasteOrigins[Random.Range(0, WasteOrigins.Length)];
        string type = WasteTypes[Random.Range(0, WasteTypes.Length)];
        return $"{origin} {type}";
    }

    // Get a random dimension type
    private string GetRandomDimensionType()
    {
        return dimensionTypes[Random.Range(0, dimensionTypes.Count)].Name;
    }

    // Calculate waste stability with interesting variance
    private float CalculateWasteStability()
    {
        // Base stability
        float baseStability = Random.Range(0.1f, 1f);
        
        // Add dimensional fluctuation
        float fluctuation = Random.Range(-0.2f, 0.2f);
        
        // Add occasional quantum instability spikes
        if (Random.value < 0.1f) // 10% chance
        {
            fluctuation *= 2f;
        }
        
        return Mathf.Clamp01(baseStability + fluctuation);
    }

    // Generate waste with specific characteristics
    public WasteItem GenerateSpecificWaste(string dimensionType, WasteRarity rarity = WasteRarity.Common)
    {
        string wasteName = GenerateName(dimensionType, rarity);
        return new WasteItem(wasteName, dimensionType, rarity);
    }

    // Generate a batch of similar waste
    public List<WasteItem> GenerateSimilarWaste(int count, string dimensionType)
    {
        var wasteItems = new List<WasteItem>();
        WasteRarity baseRarity = Random.value < 0.3f ? WasteRarity.Uncommon : WasteRarity.Common;

        for (int i = 0; i < count; i++)
        {
            wasteItems.Add(GenerateSpecificWaste(dimensionType, baseRarity));
        }

        return wasteItems;
    }
} 