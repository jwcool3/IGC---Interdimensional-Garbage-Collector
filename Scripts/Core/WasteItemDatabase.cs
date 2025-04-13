using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WasteItemDatabase : MonoBehaviour
{
    public static WasteItemDatabase Instance { get; private set; }
    
    [SerializeField] private List<WasteItemData> itemDatabase;
    
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
        // Load all WasteItemData from Resources folder
        itemDatabase = Resources.LoadAll<WasteItemData>("ItemDatabase").ToList();
        
        // Create dictionary for quick lookup
        itemDictionary = itemDatabase.ToDictionary(
            item => item.uniqueIdentifier, 
            item => item
        );

        // Create dictionary for origin-based lookup
        itemsByOrigin = itemDatabase
            .GroupBy(item => item.dimensionalOrigin)
            .ToDictionary(
                group => group.Key,
                group => group.ToList()
            );

        Debug.Log($"Initialized WasteItemDatabase with {itemDatabase.Count} items from {itemsByOrigin.Count} dimensions");
    }

    // Find item by unique identifier
    public WasteItemData GetItemByIdentifier(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
        {
            Debug.LogWarning("Attempted to get item with null or empty identifier");
            return null;
        }

        return itemDictionary.TryGetValue(identifier, out var itemData) 
            ? itemData 
            : null;
    }

    // Get random item from a specific dimensional origin
    public WasteItemData GetRandomItemByOrigin(string origin)
    {
        if (string.IsNullOrEmpty(origin))
        {
            Debug.LogWarning("Attempted to get item with null or empty origin");
            return null;
        }

        if (itemsByOrigin.TryGetValue(origin, out var itemsInOrigin) && itemsInOrigin.Count > 0)
        {
            return itemsInOrigin[Random.Range(0, itemsInOrigin.Count)];
        }

        Debug.LogWarning($"No items found for origin: {origin}");
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
} 