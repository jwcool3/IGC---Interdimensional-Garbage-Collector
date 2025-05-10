using System.Collections.Generic;
using UnityEngine;
using System;

public class WasteGenerator : MonoBehaviour
{
    // Singleton pattern
    public static WasteGenerator Instance { get; private set; }

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

    private LocationData currentLocation;

    [Header("Dimension Configuration")]
    [SerializeField] private List<DimensionType> dimensionTypes = new List<DimensionType>();

    [Header("Generation Settings")]
    [SerializeField] private float propertyVariance = 0.2f;

    [Header("Fallback Settings")]
    [SerializeField] private Sprite defaultItemSprite;

    // Effect modifiers from ship compartments
    private float rarityModifier = 0f;
    private float stabilityModifier = 0f;

    // Procedural generation data
    private string[] prefixes;
    private string[] suffixes;
    private string[] descriptions;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize the generator
            InitializeGenerator();
        }
        else
        {
            Destroy(gameObject);
            return; // Skip the rest of Awake if this is a duplicate
        }
    }
    
    private void InitializeGenerator()
    {
        // Ensure dimensions are initialized
        if (dimensionTypes.Count == 0)
        {
            OnValidate();
        }

        InitializeGenerationData();

        // Add default sprite loading code
        if (defaultItemSprite == null)
        {
            // Try to load a default sprite
            defaultItemSprite = Resources.Load<Sprite>("DefaultWasteIcon");

            // If still null, create a fallback
            if (defaultItemSprite == null)
            {
                Debug.LogWarning("No default sprite found! Items may appear without icons.");
            }
        }
    }

    private void Start()
    {
        // Subscribe to location change events
        if (LocationManager.Instance != null)
        {
            LocationManager.Instance.OnLocationChanged += OnLocationChanged;
            UpdateCurrentLocation();
        }
    }

    private void OnDestroy()
    {
        if (LocationManager.Instance != null)
        {
            LocationManager.Instance.OnLocationChanged -= OnLocationChanged;
        }
    }

    private void OnLocationChanged(LocationData newLocation)
    {
        currentLocation = newLocation;
        Debug.Log($"WasteGenerator: Location changed to {currentLocation.displayName}");
    }

    private void UpdateCurrentLocation()
    {
        if (LocationManager.Instance != null)
        {
            currentLocation = LocationManager.Instance.GetCurrentLocation();
        }
    }

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
            UpdateCurrentLocation();

            if (currentLocation == null)
            {
                Debug.LogError("No current location set!");
                return CreateProceduralWasteItem();
            }

            WasteItemDatabase database = WasteItemDatabase.Instance;
            if (database == null)
            {
                Debug.LogError("WasteItemDatabase.Instance is null!");
                return CreateProceduralWasteItem();
            }

            WasteItemData itemData = null;

            if (!string.IsNullOrEmpty(specificIdentifier))
            {
                itemData = database.GetItemByIdentifier(specificIdentifier);
            }
            else
            {
                // Use ONLY the allowed waste types for this location
                List<string> allowedTypes = currentLocation.wasteTypes;

                if (allowedTypes == null || allowedTypes.Count == 0)
                {
                    Debug.LogError($"No waste types defined for location {currentLocation.displayName}!");
                    return CreateProceduralWasteItem();
                }

                // Pick a random allowed dimension type
                string selectedType = allowedTypes[UnityEngine.Random.Range(0, allowedTypes.Count)];
                Debug.Log($"Generating waste of type: {selectedType} for location: {currentLocation.displayName}");

                itemData = database.GetRandomItemByOrigin(selectedType);

                if (itemData == null)
                {
                    Debug.LogWarning($"No items found for dimension: {selectedType}. Creating procedural item.");
                    return CreateProceduralWasteItem(selectedType);
                }
            }

            // Generate rarity based on LOCATION's probability
            WasteRarity rarity = GenerateRarityForLocation(currentLocation);

            // Create the waste item
            Sprite itemSprite = GetSpriteForItem(itemData);
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

            // Apply location modifiers
            ApplyLocationModifiers(wasteItem);

            Debug.Log($"Generated {wasteItem.Rarity} waste item: {wasteItem.Name}, Origin: {wasteItem.DimensionalOrigin}");

            return wasteItem;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error generating waste item: {e.Message}\n{e.StackTrace}");
            return CreateProceduralWasteItem();
        }
    }

    /// <summary>
    /// Sets the rarity modifier from the Communications compartment
    /// </summary>
    public void SetRarityModifier(float modifier)
    {
        rarityModifier = Mathf.Clamp01(modifier);
        Debug.Log($"WasteGenerator: Rarity modifier set to {rarityModifier:P0}");
    }

    /// <summary>
    /// Sets the stability modifier from the Stabilizer compartment
    /// </summary>
    public void SetStabilityModifier(float modifier)
    {
        stabilityModifier = Mathf.Clamp01(modifier);
        Debug.Log($"WasteGenerator: Stability modifier set to {stabilityModifier:P0}");
    }

    private WasteRarity GenerateRarityForLocation(LocationData location)
    {
        // Apply rarity modifier to increase chances of better items
        float roll = UnityEngine.Random.value;
        roll = Mathf.Max(roll - rarityModifier, 0f); // Higher modifier means better chance of rare items
        float cumulative = 0f;

        // Add each rarity chance in order
        cumulative += location.commonChance;
        if (roll < cumulative) return WasteRarity.Common;

        cumulative += location.uncommonChance;
        if (roll < cumulative) return WasteRarity.Uncommon;

        cumulative += location.rareChance;
        if (roll < cumulative) return WasteRarity.Rare;

        cumulative += location.epicChance;
        if (roll < cumulative) return WasteRarity.Epic;

        cumulative += location.legendaryChance;
        if (roll < cumulative) return WasteRarity.Legendary;

        // Fallback to common if somehow none matched
        return WasteRarity.Common;
    }

    private void ApplyLocationModifiers(WasteItem item)
    {
        if (currentLocation == null) return;

        // Apply value multiplier
        item.RecyclingValue *= currentLocation.averageValueMultiplier;

        // Apply danger level effects
        item.ContaminationLevel += currentLocation.dangerLevel * 0.2f;
        item.ContaminationLevel = Mathf.Clamp01(item.ContaminationLevel);

        // Apply discovery rate bonus
        if (UnityEngine.Random.value < currentLocation.discoveryRateMultiplier * 0.1f)
        {
            // Small chance to upgrade rarity
            int currentRarity = (int)item.Rarity;
            int upgradedRarity = Mathf.Min(currentRarity + 1, (int)WasteRarity.Legendary);
            item.Rarity = (WasteRarity)upgradedRarity;
            Debug.Log($"Discovery bonus! Upgraded {item.Name} to {item.Rarity}");
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

        // Add debug logging
        Debug.Log($"Generated procedural waste item: {wasteItem.Name}, Origin: {wasteItem.DimensionalOrigin}, Has Icon: {wasteItem.Icon != null}");

        return wasteItem;
    }

    private float RandomizeProperty(float baseValue)
    {
        // Apply stability modifier to reduce randomness and improve base values
        float variance = propertyVariance * (1f - stabilityModifier);
        float minValue = baseValue * (1f - variance);
        float maxValue = baseValue * (1f + variance);
        
        // Higher stability also provides a small bonus to the base value
        float stabilityBonus = baseValue * (stabilityModifier * 0.2f);
        
        return UnityEngine.Random.Range(minValue, maxValue) + stabilityBonus;
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
        // Apply rarity modifier to increase chances of better items
        float roll = UnityEngine.Random.value;
        roll = Mathf.Max(roll - rarityModifier, 0f); // Higher modifier means better chance of rare items

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