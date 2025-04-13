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

    public WasteItem GenerateWasteItem(string specificIdentifier = null)
    {
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
        return dimensionTypes.Find(d => d.Name == dimensionName) ?? dimensionTypes[0];
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
        var itemData = WasteItemDatabase.Instance.GetRandomItemByOrigin(dimensionType);
        if (itemData == null) return null;

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
} 