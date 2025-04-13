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

    [SerializeField] private List<DimensionType> dimensionTypes = new List<DimensionType>();

    private string[] prefixes;
    private string[] suffixes;
    private string[] descriptions;

    private void Awake()
    {
        InitializeGenerationData();
    }

    private void InitializeGenerationData()
    {
        prefixes = new string[] {
            "Unstable", "Quantum", "Temporal", "Ethereal", "Void-touched",
            "Crystalline", "Anomalous", "Prismatic", "Corrupted", "Ancient"
        };

        suffixes = new string[] {
            "Fragment", "Remnant", "Particle", "Essence", "Core",
            "Shard", "Residue", "Matter", "Echo", "Artifact"
        };

        descriptions = new string[] {
            "A mysterious fragment of interdimensional origin.",
            "Traces of an unknown reality bleed through this waste.",
            "A peculiar remnant that defies conventional understanding.",
            "Quantum echoes resonate within this discarded matter.",
            "An enigmatic piece of dimensional debris.",
            "Reality seems to warp around this strange object.",
            "Emits a faint hum of interdimensional energy.",
            "Shows signs of exposure to exotic dimensional forces."
        };
    }

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
        // Select a random dimension type
        DimensionType dimension = dimensionTypes[Random.Range(0, dimensionTypes.Count)];
        
        // Generate rarity based on dimension's probability
        WasteRarity rarity = GenerateRarity(dimension);
        
        // Create a more detailed name
        string name = GenerateDetailedName(dimension.Name, rarity);
        
        // Create description based on properties
        string description = GenerateDescription(dimension.Name, rarity);
        
        // Create the waste item with detailed properties
        WasteItem wasteItem = new WasteItem(name, dimension.Name, rarity)
        {
            Description = description,
            WasteStability = CalculateStability(rarity),
            ContaminationLevel = CalculateContamination(rarity)
        };
        
        return wasteItem;
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

    private string GenerateDetailedName(string dimensionType, WasteRarity rarity)
    {
        string[] rarityPrefixes = GetPrefixesForRarity(rarity);
        string prefix = rarityPrefixes[Random.Range(0, rarityPrefixes.Length)];
        string suffix = suffixes[Random.Range(0, suffixes.Length)];
        
        return $"{prefix} {dimensionType} {suffix}";
    }

    private string[] GetPrefixesForRarity(WasteRarity rarity)
    {
        switch (rarity)
        {
            case WasteRarity.Legendary:
                return new string[] { "Mythical", "Divine", "Transcendent", "Ultimate", "Supreme" };
            case WasteRarity.Epic:
                return new string[] { "Magnificent", "Extraordinary", "Phenomenal", "Majestic", "Ethereal" };
            case WasteRarity.Rare:
                return new string[] { "Exceptional", "Superior", "Advanced", "Enhanced", "Refined" };
            case WasteRarity.Uncommon:
                return new string[] { "Unusual", "Peculiar", "Strange", "Curious", "Odd" };
            default:
                return new string[] { "Common", "Basic", "Simple", "Regular", "Standard" };
        }
    }

    private string GenerateDescription(string dimensionType, WasteRarity rarity)
    {
        string baseDescription = descriptions[Random.Range(0, descriptions.Length)];
        string rarityDesc = GetRarityDescription(rarity);
        
        return $"{baseDescription} {rarityDesc}";
    }

    private string GetRarityDescription(WasteRarity rarity)
    {
        switch (rarity)
        {
            case WasteRarity.Legendary:
                return "Its legendary nature makes it highly sought after by dimensional researchers.";
            case WasteRarity.Epic:
                return "The epic qualities of this item are immediately apparent.";
            case WasteRarity.Rare:
                return "A rare find that could prove valuable for study.";
            case WasteRarity.Uncommon:
                return "Shows some unusual properties worth investigating.";
            default:
                return "A common example of interdimensional waste.";
        }
    }

    private float CalculateStability(WasteRarity rarity)
    {
        // More rare items have higher stability
        float baseStability = Random.Range(0.3f, 1f);
        float rarityBonus = (float)rarity * 0.1f;
        return Mathf.Clamp01(baseStability + rarityBonus);
    }

    private float CalculateContamination(WasteRarity rarity)
    {
        // More rare items have lower contamination
        float baseContamination = Random.Range(0f, 0.5f);
        float rarityReduction = (float)rarity * 0.05f;
        return Mathf.Clamp01(baseContamination - rarityReduction);
    }

    private WasteRarity GenerateRarity(DimensionType dimension)
    {
        float roll = Random.value;
        
        if (roll < 0.01f * dimension.LegendaryChance)
            return WasteRarity.Legendary;
        if (roll < 0.05f * dimension.EpicChance)
            return WasteRarity.Epic;
        if (roll < 0.15f * dimension.RareChance)
            return WasteRarity.Rare;
        if (roll < 0.30f * dimension.UncommonChance)
            return WasteRarity.Uncommon;
            
        return WasteRarity.Common;
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
        string wasteName = GenerateDetailedName(dimensionType, rarity);
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