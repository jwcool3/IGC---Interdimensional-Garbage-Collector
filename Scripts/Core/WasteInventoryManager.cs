using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// Manages the inventory of waste items
/// </summary>
public class WasteInventoryManager : MonoBehaviour
{
    // Singleton pattern
    public static WasteInventoryManager Instance { get; private set; }

    // Events
    public event Action<WasteItem> OnWasteAdded;
    public event Action<WasteItem> OnWasteRemoved;
    public event Action<List<WasteItem>> OnInventoryChanged;
    public event Action<WasteItem> OnItemQuantityChanged;
    public event Action<WasteItem> OnItemDataChanged;

    // Legacy event names for compatibility
    public event Action<WasteItem> OnItemAdded
    {
        add { OnWasteAdded += value; }
        remove { OnWasteAdded -= value; }
    }
    public event Action<WasteItem> OnItemRemoved
    {
        add { OnWasteRemoved += value; }
        remove { OnWasteRemoved -= value; }
    }

    // Maximum inventory capacity
    [SerializeField] private int maxCapacity = 100;

    // Inventory storage
    private Dictionary<string, WasteItem> inventory = new Dictionary<string, WasteItem>();

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Get all waste items (legacy method name)
    public List<WasteItem> GetAllWaste()
    {
        return GetAllItems();
    }

    // Add waste item to inventory
    public bool AddWasteItem(WasteItem item)
    {
        if (item == null) return false;

        // Check capacity
        if (inventory.Count >= maxCapacity)
        {
            Debug.LogWarning("Inventory is at maximum capacity!");
            return false;
        }

        // Check if similar item exists
        var existingItem = GetSimilarItem(item);
        if (existingItem != null)
        {
            // Increase quantity of existing item
            existingItem.AddQuantity();
            OnItemQuantityChanged?.Invoke(existingItem);
            OnInventoryChanged?.Invoke(GetAllItems());
            Debug.Log($"Increased quantity of existing item: {existingItem.Name} (ID: {existingItem.Id})");
            return true;
        }

        // Add new item
        inventory[item.Id] = item;
        OnWasteAdded?.Invoke(item);
        OnInventoryChanged?.Invoke(GetAllItems());
        Debug.Log($"Added new waste item to inventory: {item.Name} (ID: {item.Id})");
        return true;
    }

    // Find similar item in inventory
    private WasteItem GetSimilarItem(WasteItem item)
    {
        return GetAllItems().Find(i =>
            i.Name == item.Name &&
            i.DimensionalOrigin == item.DimensionalOrigin &&
            i.Rarity == item.Rarity);
    }

    // Update item quantity
    public bool UpdateItemQuantity(string itemId, int newQuantity)
    {
        if (inventory.TryGetValue(itemId, out WasteItem item))
        {
            item.SetQuantity(newQuantity);
            OnItemQuantityChanged?.Invoke(item);
            OnInventoryChanged?.Invoke(GetAllItems());

            // Remove item if quantity is 0
            if (item.Quantity <= 0)
            {
                RemoveWasteItem(itemId);
            }
            return true;
        }
        return false;
    }

    // Update item data
    public bool UpdateItemData(string itemId, WasteData newData)
    {
        if (inventory.TryGetValue(itemId, out WasteItem item))
        {
            // Update item properties
            item.Name = newData.Name;
            item.Description = newData.Description;
            item.Rarity = newData.Rarity;
            // ... update other properties as needed

            OnItemDataChanged?.Invoke(item);
            OnInventoryChanged?.Invoke(GetAllItems());
            return true;
        }
        return false;
    }

    // Remove waste item from inventory
    public bool RemoveWasteItem(string itemId)
    {
        if (inventory.TryGetValue(itemId, out WasteItem item))
        {
            inventory.Remove(itemId);
            OnWasteRemoved?.Invoke(item);
            OnInventoryChanged?.Invoke(GetAllItems());
            Debug.Log($"Removed waste item from inventory: {item.Name} (ID: {itemId})");
            return true;
        }
        Debug.LogWarning($"Failed to remove item: ID {itemId} not found in inventory");
        return false;
    }

    // Remove waste item by reference
    public bool RemoveWasteItem(WasteItem item)
    {
        if (item == null) return false;
        return RemoveWasteItem(item.Id);
    }

    // Check if waste item exists in inventory
    public bool HasWasteItem(string itemId)
    {
        return inventory.ContainsKey(itemId);
    }

    // Check if waste item exists in inventory by reference
    public bool HasWasteItem(WasteItem item)
    {
        return item != null && inventory.ContainsKey(item.Id);
    }

    // Get item by ID
    public WasteItem GetItemById(string itemId)
    {
        return inventory.TryGetValue(itemId, out WasteItem item) ? item : null;
    }

    // Get all waste items
    public List<WasteItem> GetAllItems()
    {
        return new List<WasteItem>(inventory.Values);
    }

    // Get waste items by dimension
    public List<WasteItem> GetWasteByDimension(string dimensionType)
    {
        return GetAllItems().FindAll(w => w.DimensionalOrigin == dimensionType);
    }

    // Get items by rarity
    public List<WasteItem> GetItemsByRarity(WasteRarity rarity)
    {
        return GetAllItems().FindAll(w => w.Rarity == rarity);
    }

    // Get inventory count
    public int GetInventoryCount()
    {
        return inventory.Count;
    }

    // Get remaining capacity
    public int GetRemainingCapacity()
    {
        return maxCapacity - inventory.Count;
    }

    // Sort items by various criteria
    public List<WasteItem> GetSortedItems(
        Func<WasteItem, IComparable> sortKey,
        bool descending = false)
    {
        var items = GetAllItems();
        items.Sort((a, b) => {
            var comparison = sortKey(a).CompareTo(sortKey(b));
            return descending ? -comparison : comparison;
        });
        return items;
    }

    // Clear inventory
    public void ClearInventory()
    {
        inventory.Clear();
        OnInventoryChanged?.Invoke(new List<WasteItem>());
        Debug.Log("Inventory cleared");
    }

    // Get items filtered by a custom predicate
    public List<WasteItem> GetFilteredItems(Predicate<WasteItem> filter)
    {
        return GetAllItems().FindAll(filter);
    }
}