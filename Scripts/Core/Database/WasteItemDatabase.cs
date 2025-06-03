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
        Debug.Log("Starting WasteItemDatabase initialization...");

        // Initialize dictionaries
        itemDictionary = new Dictionary<string, WasteItemData>();
        itemsByOrigin = new Dictionary<string, List<WasteItemData>>();

        // Only load from Resources if not using manual assignment
        if (!useManualAssignment)
        {
            Debug.Log("Loading items from Resources...");
            LoadItemsFromResources();
        }
        else
        {
            Debug.Log("Using manual assignment, skipping Resources loading");
            Debug.Log($"Manual itemDatabase count: {itemDatabase.Count}");
        }

        // Populate dictionaries from itemDatabase
        PopulateDictionaries();

        Debug.Log($"Initialized WasteItemDatabase with {itemDatabase.Count} items from {itemsByOrigin.Count} dimensions");

        // Output all dimensions for debugging
        foreach (var dimension in itemsByOrigin.Keys)
        {
            Debug.Log($"Dimension: {dimension}, Items: {itemsByOrigin[dimension].Count}");
            foreach (var item in itemsByOrigin[dimension])
            {
                Debug.Log($"  - [{dimension}] {item.itemName} (ID: {item.uniqueIdentifier})");
            }
        }

        // Specifically check for Earth
        if (itemsByOrigin.ContainsKey("Earth"))
        {
            Debug.Log($"Earth items found: {itemsByOrigin["Earth"].Count}");
            foreach (var item in itemsByOrigin["Earth"])
            {
                Debug.Log($"  - Earth Item: {item.itemName} (ID: {item.uniqueIdentifier})");
            }
        }
        else
        {
            Debug.LogError("No 'Earth' key found in itemsByOrigin dictionary!");
        }

        // Log fallback item info
        if (fallbackItemData != null)
        {
            Debug.Log($"Fallback item configured: {fallbackItemData.itemName} (Origin: {fallbackItemData.dimensionalOrigin})");
        }
        else
        {
            Debug.LogWarning("No fallback item configured!");
        }
    }

    private void LoadItemsFromResources()
    {
        try
        {
            Debug.Log("Attempting to load items from Resources...");

            // Try loading from the root ItemDatabase folder
            WasteItemData[] loadedItems = Resources.LoadAll<WasteItemData>("ItemDatabase");
            Debug.Log($"Loaded {loadedItems.Length} items from Resources/ItemDatabase");

            // Also try loading from specific subfolders
            WasteItemData[] earthItems = Resources.LoadAll<WasteItemData>("ItemDatabase/Earth");
            Debug.Log($"Loaded {earthItems.Length} items from Resources/ItemDatabase/Earth");

            // Log details of loaded items
            if (loadedItems != null && loadedItems.Length > 0)
            {
                Debug.Log("Items found in root ItemDatabase folder:");
                foreach (var item in loadedItems)
                {
                    Debug.Log($"  - {item.itemName} (Origin: {item.dimensionalOrigin})");
                }
                itemDatabase.AddRange(loadedItems);
            }
            else
            {
                Debug.LogWarning("No items loaded from Resources/ItemDatabase");
            }

            // Add Earth items if any were found
            if (earthItems != null && earthItems.Length > 0)
            {
                Debug.Log("Items found in Earth subfolder:");
                foreach (var item in earthItems)
                {
                    Debug.Log($"  - {item.itemName} (Origin: {item.dimensionalOrigin})");
                }
                itemDatabase.AddRange(earthItems);
            }

            // Log the final count
            Debug.Log($"Total items loaded into database: {itemDatabase.Count}");

            // Verify items have required fields
            foreach (var item in itemDatabase)
            {
                if (string.IsNullOrEmpty(item.dimensionalOrigin))
                {
                    Debug.LogError($"Item {item.itemName} has no dimensional origin!");
                }
                if (string.IsNullOrEmpty(item.uniqueIdentifier))
                {
                    Debug.LogError($"Item {item.itemName} has no unique identifier!");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading items from Resources: {e.Message}\nStack trace: {e.StackTrace}");
        }
    }

    private void PopulateDictionaries()
    {
        // Clear dictionaries
        itemDictionary.Clear();
        itemsByOrigin.Clear();

        // Add items to dictionaries
        foreach (var item in itemDatabase)
        {
            if (item == null) continue;

            // Add to ID dictionary
            if (!string.IsNullOrEmpty(item.uniqueIdentifier))
            {
                itemDictionary[item.uniqueIdentifier] = item;
            }
            else
            {
                Debug.LogWarning($"Item {item.name} has no uniqueIdentifier!");
            }

            // Add to dimensional origin dictionary
            if (!string.IsNullOrEmpty(item.dimensionalOrigin))
            {
                if (!itemsByOrigin.ContainsKey(item.dimensionalOrigin))
                {
                    itemsByOrigin[item.dimensionalOrigin] = new List<WasteItemData>();
                }

                itemsByOrigin[item.dimensionalOrigin].Add(item);
            }
            else
            {
                Debug.LogWarning($"Item {item.name} has no dimensional origin!");
            }
        }

        // Create a fallback dimension if none exists
        if (!itemsByOrigin.ContainsKey("Default") && fallbackItemData != null)
        {
            itemsByOrigin["Default"] = new List<WasteItemData> { fallbackItemData };
        }
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
            Debug.LogWarning("Attempted to get item with null or empty origin");
            return fallbackItemData;
        }

        if (itemsByOrigin.TryGetValue(origin, out var itemsInOrigin) && itemsInOrigin.Count > 0)
        {
            return itemsInOrigin[UnityEngine.Random.Range(0, itemsInOrigin.Count)];
        }

        Debug.LogWarning($"No items found for origin: {origin}. Using fallback item.");

        // If we don't have items for this origin but have a fallback, use it
        if (fallbackItemData != null)
        {
            return fallbackItemData;
        }

        // If we have any items at all, pick a random origin
        if (itemsByOrigin.Count > 0)
        {
            string randomOrigin = itemsByOrigin.Keys.ElementAt(UnityEngine.Random.Range(0, itemsByOrigin.Count));
            Debug.Log($"Using random origin: {randomOrigin} instead of {origin}");
            return GetRandomItemByOrigin(randomOrigin);
        }

        return null;
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
        if (itemData == null) return;

        // Add to database list
        if (!itemDatabase.Contains(itemData))
        {
            itemDatabase.Add(itemData);
        }

        // Update dictionaries
        if (!string.IsNullOrEmpty(itemData.uniqueIdentifier))
        {
            itemDictionary[itemData.uniqueIdentifier] = itemData;
        }

        if (!string.IsNullOrEmpty(itemData.dimensionalOrigin))
        {
            if (!itemsByOrigin.ContainsKey(itemData.dimensionalOrigin))
            {
                itemsByOrigin[itemData.dimensionalOrigin] = new List<WasteItemData>();
            }

            if (!itemsByOrigin[itemData.dimensionalOrigin].Contains(itemData))
            {
                itemsByOrigin[itemData.dimensionalOrigin].Add(itemData);
            }
        }
    }

    // Helper method to create a default item data for a given dimension
    public WasteItemData CreateDefaultItemForDimension(string dimensionName)
    {
        WasteItemData newItem = ScriptableObject.CreateInstance<WasteItemData>();
        newItem.itemName = $"{dimensionName} Waste";
        newItem.uniqueIdentifier = System.Guid.NewGuid().ToString();
        newItem.description = $"A piece of waste from the {dimensionName} dimension.";
        newItem.dimensionalOrigin = dimensionName;
        newItem.defaultRarity = WasteRarity.Common;
        newItem.baseStability = 0.5f;
        newItem.baseContamination = 0.2f;
        newItem.baseRecyclingPotential = 0.3f;

        // Add to database
        AddItemData(newItem);

        return newItem;
    }
}