using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class WasteItemDatabase : MonoBehaviour
{
    public static WasteItemDatabase Instance { get; private set; }

    [SerializeField] private List<WasteItemData> itemDatabase = new List<WasteItemData>();

    // Option to manually assign item data in the inspector
    [SerializeField] private bool useManualAssignment = false;

    // Fallback data for when no items are found for a dimension
    [SerializeField] private WasteItemData fallbackItemData;

    private Dictionary<string, WasteItemData> itemDictionary;
    private Dictionary<string, List<WasteItemData>> itemsByOrigin;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDatabase()
    {
        Debug.Log("WasteItemDatabase: Initializing database...");

        itemDictionary = new Dictionary<string, WasteItemData>(StringComparer.OrdinalIgnoreCase);
        itemsByOrigin = new Dictionary<string, List<WasteItemData>>(StringComparer.OrdinalIgnoreCase);

        if (!useManualAssignment)
        {
            LoadItemsFromResources();
        }

        PopulateDictionaries();

        // Ensure we have at least one item for common dimensions
        EnsureBasicItems();

        Debug.Log($"WasteItemDatabase: Initialized with {itemDatabase.Count} items from {itemsByOrigin.Count} dimensions");

        foreach (var dimension in itemsByOrigin.Keys)
        {
            Debug.Log($"WasteItemDatabase: Dimension: {dimension}, Items: {itemsByOrigin[dimension].Count}");
        }
    }

    private void LoadItemsFromResources()
    {
        try
        {
            WasteItemData[] loadedItems = Resources.LoadAll<WasteItemData>("ItemDatabase");
            Debug.Log($"WasteItemDatabase: Found {loadedItems.Length} items in Resources/ItemDatabase");

            if (loadedItems != null && loadedItems.Length > 0)
            {
                itemDatabase.AddRange(loadedItems);
            }
            else
            {
                Debug.LogWarning("WasteItemDatabase: No items found in Resources/ItemDatabase");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"WasteItemDatabase: Error loading items from Resources: {e.Message}");
        }
    }

    private void PopulateDictionaries()
    {
        Debug.Log("WasteItemDatabase: Populating dictionaries...");

        itemDictionary.Clear();
        itemsByOrigin.Clear();

        foreach (var item in itemDatabase)
        {
            if (item == null) continue;

            if (!string.IsNullOrEmpty(item.uniqueIdentifier))
            {
                itemDictionary[item.uniqueIdentifier] = item;
            }
            else
            {
                Debug.LogWarning($"WasteItemDatabase: Item {item.name} has no uniqueIdentifier!");
            }

            if (!string.IsNullOrEmpty(item.dimensionalOrigin))
            {
                string origin = item.dimensionalOrigin.Trim();
                if (!itemsByOrigin.ContainsKey(origin))
                {
                    itemsByOrigin[origin] = new List<WasteItemData>();
                }

                itemsByOrigin[origin].Add(item);
                Debug.Log($"WasteItemDatabase: Added item '{item.itemName}' to origin '{origin}'");
            }
            else
            {
                Debug.LogWarning($"WasteItemDatabase: Item {item.name} has no dimensional origin!");
            }
        }

        if (!itemsByOrigin.ContainsKey("Default") && fallbackItemData != null)
        {
            itemsByOrigin["Default"] = new List<WasteItemData> { fallbackItemData };
            Debug.Log("WasteItemDatabase: Added fallback item to Default dimension");
        }
    }

    private void EnsureBasicItems()
    {
        Debug.Log("WasteItemDatabase: Checking for basic items...");

        // Ensure we have at least one Earth item
        if (!itemsByOrigin.ContainsKey("Earth") || itemsByOrigin["Earth"].Count == 0)
        {
            Debug.LogWarning("WasteItemDatabase: No Earth items found. Creating default Earth items...");

            // Create multiple Earth items for variety
            CreateBasicEarthItem("Discarded Plastic", "Common plastic waste material.", 0.8f, 0.2f, 0.4f);
            CreateBasicEarthItem("Paper Waste", "Discarded paper and cardboard.", 0.9f, 0.1f, 0.6f);
            CreateBasicEarthItem("Metal Scrap", "Various metal waste pieces.", 0.7f, 0.3f, 0.5f);
            CreateBasicEarthItem("Glass Fragments", "Broken glass pieces.", 0.6f, 0.4f, 0.3f);

            Debug.Log($"WasteItemDatabase: Created {itemsByOrigin["Earth"].Count} default Earth items");
        }
    }

    private void CreateBasicEarthItem(string name, string description, float stability, float contamination, float recyclingPotential)
    {
        WasteItemData earthItem = ScriptableObject.CreateInstance<WasteItemData>();
        earthItem.itemName = name;
        earthItem.uniqueIdentifier = $"earth_{name.ToLower().Replace(" ", "_")}_{DateTime.Now.Ticks % 10000}";
        earthItem.description = description;
        earthItem.dimensionalOrigin = "Earth";
        earthItem.defaultRarity = WasteRarity.Common;
        earthItem.baseStability = stability;
        earthItem.baseContamination = contamination;
        earthItem.baseRecyclingPotential = recyclingPotential;

        AddItemData(earthItem);
        Debug.Log($"WasteItemDatabase: Created Earth item: {earthItem.itemName}");
    }

    // Find item by unique identifier
    public WasteItemData GetItemByIdentifier(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            Debug.LogWarning("Attempted to get item with null or empty identifier");
            return fallbackItemData;
        }

        return itemDictionary.TryGetValue(identifier, out var itemData)
            ? itemData
            : fallbackItemData;
    }

    // Get random item from a specific dimensional origin
    public WasteItemData GetRandomItemByOrigin(string origin)
    {
        if (string.IsNullOrEmpty(origin))
        {
            Debug.LogWarning("WasteItemDatabase: GetRandomItemByOrigin called with null or empty origin");
            return fallbackItemData;
        }

        Debug.Log($"WasteItemDatabase: Looking for items with origin '{origin}'");

        // Try to get items for the exact origin
        if (itemsByOrigin.TryGetValue(origin, out var itemsInOrigin) && itemsInOrigin.Count > 0)
        {
            var selectedItem = itemsInOrigin[UnityEngine.Random.Range(0, itemsInOrigin.Count)];
            Debug.Log($"WasteItemDatabase: Found {itemsInOrigin.Count} items for '{origin}', selected: {selectedItem.itemName}");
            return selectedItem;
        }

        Debug.LogWarning($"WasteItemDatabase: No items found for origin: {origin}");

        // Try to create a default item for this origin
        var defaultItem = CreateDefaultItemForDimension(origin);
        if (defaultItem != null)
        {
            Debug.Log($"WasteItemDatabase: Created default item for origin '{origin}'");
            return defaultItem;
        }

        // Last resort: use fallback
        Debug.LogWarning($"WasteItemDatabase: Using fallback item for origin '{origin}'");
        return fallbackItemData;
    }

    // Get all items from a specific origin
    public List<WasteItemData> GetAllItemsByOrigin(string origin)
    {
        return itemsByOrigin.TryGetValue(origin, out var items)
            ? items
            : new List<WasteItemData>();
    }

    // Get all available dimensional origins
    public List<string> GetAllDimensionalOrigins()
    {
        return itemsByOrigin.Keys.ToList();
    }

    // Helper method to add a new item data to the database at runtime
    public void AddItemData(WasteItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogError("WasteItemDatabase: Attempted to add null item data");
            return;
        }

        // Add to database list
        if (!itemDatabase.Contains(itemData))
        {
            itemDatabase.Add(itemData);
            Debug.Log($"WasteItemDatabase: Added new item to database: {itemData.itemName}");
        }

        // Update dictionaries
        if (!string.IsNullOrEmpty(itemData.uniqueIdentifier))
        {
            itemDictionary[itemData.uniqueIdentifier] = itemData;
        }

        if (!string.IsNullOrEmpty(itemData.dimensionalOrigin))
        {
            string origin = itemData.dimensionalOrigin.Trim();
            if (!itemsByOrigin.ContainsKey(origin))
            {
                itemsByOrigin[origin] = new List<WasteItemData>();
            }

            if (!itemsByOrigin[origin].Contains(itemData))
            {
                itemsByOrigin[origin].Add(itemData);
                Debug.Log($"WasteItemDatabase: Added item '{itemData.itemName}' to origin '{origin}'");
            }
        }
    }

    // Helper method to create a default item data for a given dimension
    public WasteItemData CreateDefaultItemForDimension(string dimensionName)
    {
        if (string.IsNullOrEmpty(dimensionName))
        {
            Debug.LogError("WasteItemDatabase: Cannot create default item for null or empty dimension name");
            return null;
        }

        Debug.Log($"WasteItemDatabase: Creating default item for dimension '{dimensionName}'");

        WasteItemData newItem = ScriptableObject.CreateInstance<WasteItemData>();
        newItem.itemName = $"{dimensionName} Waste";
        newItem.uniqueIdentifier = $"{dimensionName.ToLower().Replace(" ", "_")}_default_{DateTime.Now.Ticks % 10000}";
        newItem.description = $"A piece of waste from the {dimensionName} dimension.";
        newItem.dimensionalOrigin = dimensionName;
        newItem.defaultRarity = WasteRarity.Common;
        newItem.baseStability = 0.5f;
        newItem.baseContamination = 0.2f;
        newItem.baseRecyclingPotential = 0.3f;

        AddItemData(newItem);
        Debug.Log($"WasteItemDatabase: Created default item: {newItem.itemName} for dimension: {dimensionName}");

        return newItem;
    }
}