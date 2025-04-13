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
        // Initialize dictionaries
        itemDictionary = new Dictionary<string, WasteItemData>();
        itemsByOrigin = new Dictionary<string, List<WasteItemData>>();

        // Only load from Resources if not using manual assignment
        if (!useManualAssignment)
        {
            LoadItemsFromResources();
        }

        // Populate dictionaries from itemDatabase
        PopulateDictionaries();

        Debug.Log($"Initialized WasteItemDatabase with {itemDatabase.Count} items from {itemsByOrigin.Count} dimensions");

        // Output all dimensions for debugging
        foreach (var dimension in itemsByOrigin.Keys)
        {
            Debug.Log($"Dimension: {dimension}, Items: {itemsByOrigin[dimension].Count}");
        }
    }

    private void LoadItemsFromResources()
    {
        try
        {
            WasteItemData[] loadedItems = Resources.LoadAll<WasteItemData>("ItemDatabase");
            Debug.Log($"Loaded {loadedItems.Length} items from Resources/ItemDatabase");

            if (loadedItems != null && loadedItems.Length > 0)
            {
                itemDatabase.AddRange(loadedItems);
            }
            else
            {
                Debug.LogWarning("No items loaded from Resources/ItemDatabase. Make sure your items are in the correct folder.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading items from Resources: {e.Message}");
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