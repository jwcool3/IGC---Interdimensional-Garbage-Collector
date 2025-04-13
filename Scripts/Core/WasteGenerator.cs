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
    [SerializeField] private float propertyVariance = 0.2f;

    private string[] prefixes;
    private string[] suffixes;
    private string[] descriptions;

    private void OnValidate()
    {
        // Auto-populate dimension types if empty
        if (dimensionTypes.Count == 0)
        {
            dimensionTypes.Add(new DimensionType() { Name = "Earth" });
            dimensionTypes.Add(new DimensionType() { Name = "Technological Waste" });
            dimensionTypes.Add(new DimensionType() { Name = "Biological Remnants" });
            dimensionTypes.Add(new DimensionType() { Name = "Quantum Residue" });
            dimensionTypes.Add(new DimensionType() { Name = "Philosophical Byproducts" });
            dimensionTypes.Add(new DimensionType() { Name = "Cosmic Debris" });
            dimensionTypes.Add(new DimensionType() { Name = "Temporal Anomaly" });
            dimensionTypes.Add(new DimensionType() { Name = "Ethereal Plane" });
            dimensionTypes.Add(new DimensionType() { Name = "Archaeological Waste" });
        }
    }

    private void Awake()
    {
        // Ensure dimensions are initialized
        if (dimensionTypes.Count == 0)
        {
            OnValidate();
        }

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

    public WasteItem GenerateWasteItem(string specificIdentifier = null)
    {
        if (WasteItemDatabase.Instance == null)
        {
            Debug.LogError("WasteItemDatabase.Instance is null! Ensure it's initialized before generating waste.");
            return null;
        }

        WasteItemData itemData;

        if (!string.IsNullOrEmpty(specificIdentifier))
        {
            // Generate specific item
            itemData = WasteItemDatabase.Instance.GetItemByIdentifier(specificIdentifier);
        }
        else
        {
            // Random item selection
            var dimension = dimensionTypes[Random.Range(0, dimensionTypes.Count)];
            itemData = WasteItemDatabase.Instance.GetRandomItemByOrigin(dimension.Name);
        }

        if (itemData == null)
        {
            Debug.LogError("Could not find item data!");
            return null;
        }

        // Generate rarity based on dimension's probability
        WasteRarity rarity = GenerateRarity(GetDimensionType(itemData.dimensionalOrigin));

        // Get a random sprite from the item's sprites
        Sprite itemSprite = GetRandomSprite(itemData.itemSprites);

        // Create WasteItem with base data and icon
        WasteItem wasteItem = new WasteItem(
            itemData.itemName,
            itemData.dimensionalOrigin,
            rarity,
            itemSprite
        )
        {
            Description = itemData.description,
            WasteStability = RandomizeProperty(itemData.baseStability),
            ContaminationLevel = RandomizeProperty(itemData.baseContamination),
            RecyclingPotential = RandomizeProperty(itemData.baseRecyclingPotential)
        };

        return wasteItem;
    }

    private float RandomizeProperty(float baseValue)
    {
        return Mathf.Clamp01(baseValue + Random.Range(-propertyVariance, propertyVariance));
    }

    private Sprite GetRandomSprite(Sprite[] sprites)
    {
        return sprites != null && sprites.Length > 0 
            ? sprites[Random.Range(0, sprites.Length)] 
            : null;
    }

    private DimensionType GetDimensionType(string dimensionName)
    {
        var dimension = dimensionTypes.Find(d => d.Name == dimensionName);
        if (dimension == null)
        {
            Debug.LogWarning($"Dimension type '{dimensionName}' not found. Using default dimension.");
            
            // Create a default dimension if none exists
            if (dimensionTypes.Count == 0)
            {
                var defaultDimension = new DimensionType() { Name = "Default" };
                dimensionTypes.Add(defaultDimension);
                return defaultDimension;
            }
            
            return dimensionTypes[0];
        }
        return dimension;
    }

    // Generate multiple waste items
    public List<WasteItem> GenerateMultipleWaste(int count, string specificIdentifier = null)
    {
        var items = new List<WasteItem>();
        for (int i = 0; i < count; i++)
        {
            var item = GenerateWasteItem(specificIdentifier);
            if (item != null)
            {
                items.Add(item);
            }
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

    // Generate waste with specific characteristics
    public WasteItem GenerateSpecificWaste(string dimensionType, WasteRarity rarity = WasteRarity.Common)
    {
        if (WasteItemDatabase.Instance == null)
        {
            Debug.LogError("WasteItemDatabase.Instance is null! Ensure it's initialized before generating waste.");
            return null;
        }

        var itemData = WasteItemDatabase.Instance.GetRandomItemByOrigin(dimensionType);
        if (itemData == null)
        {
            Debug.LogWarning($"No item data found for dimension: {dimensionType}");
            return null;
        }

        // Get a random sprite
        Sprite itemSprite = GetRandomSprite(itemData.itemSprites);

        return new WasteItem(
            itemData.itemName,
            dimensionType,
            rarity,
            itemSprite
        )
        {
            Description = itemData.description,
            WasteStability = RandomizeProperty(itemData.baseStability),
            ContaminationLevel = RandomizeProperty(itemData.baseContamination),
            RecyclingPotential = RandomizeProperty(itemData.baseRecyclingPotential)
        };
    }

    // Generate a batch of similar waste
    public List<WasteItem> GenerateSimilarWaste(int count, string dimensionType)
    {
        if (WasteItemDatabase.Instance == null)
        {
            Debug.LogError("WasteItemDatabase.Instance is null! Ensure it's initialized before generating waste.");
            return new List<WasteItem>();
        }

        var wasteItems = new List<WasteItem>();
        WasteRarity baseRarity = Random.value < 0.3f ? WasteRarity.Uncommon : WasteRarity.Common;

        for (int i = 0; i < count; i++)
        {
            var item = GenerateSpecificWaste(dimensionType, baseRarity);
            if (item != null)
            {
                wasteItems.Add(item);
            }
        }

        return wasteItems;
    }

    private WasteRarity GenerateRarity(DimensionType dimension)
    {
        float roll = Random.value;
        
        if (roll < dimension.LegendaryChance)
            return WasteRarity.Legendary;
        if (roll < dimension.EpicChance)
            return WasteRarity.Epic;
        if (roll < dimension.RareChance)
            return WasteRarity.Rare;
        if (roll < dimension.UncommonChance)
            return WasteRarity.Uncommon;
            
        return WasteRarity.Common;
    }
} 