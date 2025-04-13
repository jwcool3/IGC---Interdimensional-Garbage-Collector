using System.Collections.Generic;
using UnityEngine;
using System;

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

        [Tooltip("Default icon for this dimension type")]
        public Sprite DefaultIcon;
    }

    [Header("Dimension Configuration")]
    [SerializeField] private List<DimensionType> dimensionTypes = new List<DimensionType>();

    [Header("Generation Settings")]
    [SerializeField] private float propertyVariance = 0.2f;

    [Header("Fallback Settings")]
    [SerializeField] private Sprite defaultItemSprite;

    // Procedural generation data
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

    public WasteItem GenerateWasteItem(string specificIdentifier = null)
    {
        try
        {
            // Validate database access
            WasteItemDatabase database = WasteItemDatabase.Instance;
            if (database == null)
            {
                Debug.LogError("WasteItemDatabase.Instance is null! Creating procedural waste item instead.");
                return CreateProceduralWasteItem();
            }

            WasteItemData itemData = null;

            if (!string.IsNullOrEmpty(specificIdentifier))
            {
                // Generate specific item
                itemData = database.GetItemByIdentifier(specificIdentifier);
            }
            else
            {
                // Random item selection
                DimensionType dimension = GetRandomDimension();
                itemData = database.GetRandomItemByOrigin(dimension.Name);

                // If no item was found, try to create a default one for this dimension
                if (itemData == null)
                {
                    Debug.LogWarning($"No items found for dimension: {dimension.Name}. Creating a default item.");
                    itemData = database.CreateDefaultItemForDimension(dimension.Name);

                    // If still null, create a procedural item
                    if (itemData == null)
                    {
                        return CreateProceduralWasteItem(dimension.Name);
                    }
                }
            }

            // Generate rarity based on dimension's probability
            WasteRarity rarity = GenerateRarity(GetDimensionType(itemData.dimensionalOrigin));

            // Get a sprite for the item
            Sprite itemSprite = GetSpriteForItem(itemData);

            // Create WasteItem with base data and icon
            WasteItem wasteItem = new WasteItem(
                itemData.itemName,
                itemData.dimensionalOrigin,
                rarity,
                itemSprite
            );

            // Set additional properties
            wasteItem.Description = itemData.description;
            wasteItem.WasteStability = RandomizeProperty(itemData.baseStability);
            wasteItem.ContaminationLevel = RandomizeProperty(itemData.baseContamination);
            wasteItem.RecyclingPotential = RandomizeProperty(itemData.baseRecyclingPotential);

            return wasteItem;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error generating waste item: {e.Message}\n{e.StackTrace}");
            return CreateProceduralWasteItem();
        }
    }

    private Sprite GetSpriteForItem(WasteItemData itemData)
    {
        // Try to get a sprite from the item data
        if (itemData.itemSprites != null && itemData.itemSprites.Length > 0)
        {
            return itemData.itemSprites[UnityEngine.Random.Range(0, itemData.itemSprites.Length)];
        }

        // Try to get a sprite from the dimension type
        DimensionType dimension = GetDimensionType(itemData.dimensionalOrigin);
        if (dimension != null && dimension.DefaultIcon != null)
        {
            return dimension.DefaultIcon;
        }

        // Fall back to default sprite
        return defaultItemSprite;
    }

    private WasteItem CreateProceduralWasteItem(string dimensionName = null)
    {
        // If no dimension name is provided, pick a random one
        if (string.IsNullOrEmpty(dimensionName))
        {
            dimensionName = GetRandomDimension().Name;
        }

        // Generate a random rarity
        WasteRarity rarity = GenerateRarity(GetDimensionType(dimensionName));

        // Generate a name
        string itemName = GenerateDetailedName(dimensionName, rarity);

        // Create the waste item
        WasteItem wasteItem = new WasteItem(
            itemName,
            dimensionName,
            rarity,
            defaultItemSprite
        );

        // Set additional properties
        wasteItem.Description = GenerateDescription(dimensionName, rarity);
        wasteItem.WasteStability = 0.5f + ((int)rarity * 0.1f) + UnityEngine.Random.Range(-0.1f, 0.1f);
        wasteItem.ContaminationLevel = 0.5f - ((int)rarity * 0.1f) + UnityEngine.Random.Range(-0.1f, 0.1f);
        wasteItem.RecyclingPotential = 0.3f + ((int)rarity * 0.15f) + UnityEngine.Random.Range(-0.1f, 0.1f);

        return wasteItem;
    }

    private float RandomizeProperty(float baseValue)
    {
        return Mathf.Clamp01(baseValue + UnityEngine.Random.Range(-propertyVariance, propertyVariance));
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

    private DimensionType GetRandomDimension()
    {
        if (dimensionTypes.Count == 0)
        {
            Debug.LogWarning("No dimension types defined. Creating a default dimension.");
            DimensionType defaultDimension = new DimensionType() { Name = "Default" };
            dimensionTypes.Add(defaultDimension);
            return defaultDimension;
        }

        return dimensionTypes[UnityEngine.Random.Range(0, dimensionTypes.Count)];
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
        string prefix = rarityPrefixes[UnityEngine.Random.Range(0, rarityPrefixes.Length)];
        string suffix = suffixes[UnityEngine.Random.Range(0, suffixes.Length)];

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
        string baseDescription = descriptions[UnityEngine.Random.Range(0, descriptions.Length)];
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
        try
        {
            if (WasteItemDatabase.Instance == null)
            {
                Debug.LogError("WasteItemDatabase.Instance is null! Creating procedural waste item instead.");
                return CreateProceduralWasteItem(dimensionType);
            }

            var itemData = WasteItemDatabase.Instance.GetRandomItemByOrigin(dimensionType);
            if (itemData == null)
            {
                Debug.LogWarning($"No item data found for dimension: {dimensionType}. Creating procedural item.");
                return CreateProceduralWasteItem(dimensionType);
            }

            // Get a sprite
            Sprite itemSprite = GetSpriteForItem(itemData);

            // Create the item
            WasteItem wasteItem = new WasteItem(
                itemData.itemName,
                dimensionType,
                rarity,
                itemSprite
            );

            // Set properties
            wasteItem.Description = itemData.description;
            wasteItem.WasteStability = RandomizeProperty(itemData.baseStability);
            wasteItem.ContaminationLevel = RandomizeProperty(itemData.baseContamination);
            wasteItem.RecyclingPotential = RandomizeProperty(itemData.baseRecyclingPotential);

            return wasteItem;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error generating specific waste: {e.Message}");
            return CreateProceduralWasteItem(dimensionType);
        }
    }

    // Generate a batch of similar waste
    public List<WasteItem> GenerateSimilarWaste(int count, string dimensionType)
    {
        var wasteItems = new List<WasteItem>();
        WasteRarity baseRarity = UnityEngine.Random.value < 0.3f ? WasteRarity.Uncommon : WasteRarity.Common;

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
        float roll = UnityEngine.Random.value;

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
}