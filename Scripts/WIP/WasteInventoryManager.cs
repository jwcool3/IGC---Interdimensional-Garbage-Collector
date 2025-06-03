using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Manages waste inventory, storage, and organization
/// Handles waste collection, sorting, and retrieval for processing
/// </summary>
public class WasteInventoryManager : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxInventorySlots = 100;
    [SerializeField] private float maxTotalWeight = 1000f;
    [SerializeField] private bool enableAutoSorting = true;
    [SerializeField] private bool enableAutoStacking = true;
    [SerializeField] private float autoSortInterval = 5f;
    
    [Header("Storage Categories")]
    [SerializeField] private List<WasteStorageCategory> storageCategories = new List<WasteStorageCategory>();
    [SerializeField] private WasteStorageCategory defaultCategory;
    
    [Header("Quality Control")]
    [SerializeField] private bool enableQualityFiltering = true;
    [SerializeField] private float minimumQualityThreshold = 0.1f;
    [SerializeField] private bool autoRejectContaminated = false;
    [SerializeField] private float contaminationThreshold = 0.8f;
    
    // Events
    public static event Action<WasteItem> OnWasteAdded;
    public static event Action<WasteItem> OnWasteRemoved;
    public static event Action<WasteItem> OnWasteRejected;
    public static event Action OnInventoryChanged;
    public static event Action<string> OnInventoryFull;
    public static event Action<WasteStorageCategory> OnCategoryFull;
    
    // Singleton instance
    public static WasteInventoryManager Instance { get; private set; }
    
    // Inventory data
    private Dictionary<string, WasteInventorySlot> inventorySlots = new Dictionary<string, WasteInventorySlot>();
    private Dictionary<WasteType, List<WasteInventorySlot>> wasteTypeIndex = new Dictionary<WasteType, List<WasteInventorySlot>>();
    private Dictionary<WasteOrigin, List<WasteInventorySlot>> originIndex = new Dictionary<WasteOrigin, List<WasteInventorySlot>>();
    private List<WasteInventorySlot> sortedSlots = new List<WasteInventorySlot>();
    
    // Auto-sorting
    private float lastSortTime;
    private bool needsSorting = false;
    
    // Statistics
    private WasteInventoryStats stats = new WasteInventoryStats();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        SetupDefaultCategories();
        LoadInventoryData();
    }
    
    private void Update()
    {
        if (enableAutoSorting && needsSorting && Time.time - lastSortTime >= autoSortInterval)
        {
            SortInventory();
            needsSorting = false;
            lastSortTime = Time.time;
        }
    }
    
    private void InitializeInventory()
    {
        inventorySlots.Clear();
        wasteTypeIndex.Clear();
        originIndex.Clear();
        sortedSlots.Clear();
        
        // Initialize waste type index
        foreach (WasteType wasteType in Enum.GetValues(typeof(WasteType)))
        {
            wasteTypeIndex[wasteType] = new List<WasteInventorySlot>();
        }
        
        // Initialize origin index
        foreach (WasteOrigin origin in Enum.GetValues(typeof(WasteOrigin)))
        {
            originIndex[origin] = new List<WasteInventorySlot>();
        }
    }
    
    private void SetupDefaultCategories()
    {
        if (storageCategories.Count == 0)
        {
            // Create default categories
            storageCategories.Add(new WasteStorageCategory
            {
                name = "High Value",
                priority = 1,
                acceptedTypes = new List<WasteType> { WasteType.Electronics, WasteType.Metals },
                maxSlots = 20,
                qualityThreshold = 0.7f
            });
            
            storageCategories.Add(new WasteStorageCategory
            {
                name = "Organic",
                priority = 2,
                acceptedTypes = new List<WasteType> { WasteType.Organic, WasteType.Food },
                maxSlots = 30,
                qualityThreshold = 0.3f
            });
            
            storageCategories.Add(new WasteStorageCategory
            {
                name = "General",
                priority = 3,
                acceptedTypes = new List<WasteType>(),
                maxSlots = 50,
                qualityThreshold = 0.1f
            });
        }
        
        if (defaultCategory == null)
        {
            defaultCategory = storageCategories.LastOrDefault();
        }
    }
    
    /// <summary>
    /// Add waste item to inventory
    /// </summary>
    /// <param name="wasteItem">Waste item to add</param>
    /// <returns>True if successfully added</returns>
    public bool AddWasteItem(WasteItem wasteItem)
    {
        if (wasteItem == null)
            return false;
        
        // Quality control checks
        if (!PassesQualityControl(wasteItem))
        {
            OnWasteRejected?.Invoke(wasteItem);
            return false;
        }
        
        // Check if inventory is full
        if (IsInventoryFull())
        {
            OnInventoryFull?.Invoke("Inventory is full");
            return false;
        }
        
        // Try to stack with existing items
        if (enableAutoStacking && TryStackWasteItem(wasteItem))
        {
            UpdateStats(wasteItem, true);
            OnWasteAdded?.Invoke(wasteItem);
            OnInventoryChanged?.Invoke();
            needsSorting = true;
            return true;
        }
        
        // Find appropriate storage category
        var category = FindBestStorageCategory(wasteItem);
        if (category == null || IsCategoryFull(category))
        {
            if (category != null)
            {
                OnCategoryFull?.Invoke(category);
            }
            
            // Try default category
            category = defaultCategory;
            if (category == null || IsCategoryFull(category))
            {
                OnInventoryFull?.Invoke("No available storage category");
                return false;
            }
        }
        
        // Create new inventory slot
        var slot = CreateInventorySlot(wasteItem, category);
        if (slot == null)
            return false;
        
        // Add to inventory
        string slotId = GenerateSlotId();
        inventorySlots[slotId] = slot;
        slot.slotId = slotId;
        
        // Update indices
        UpdateIndices(slot, true);
        
        // Update statistics
        UpdateStats(wasteItem, true);
        
        // Trigger events
        OnWasteAdded?.Invoke(wasteItem);
        OnInventoryChanged?.Invoke();
        needsSorting = true;
        
        return true;
    }
    
    /// <summary>
    /// Remove waste item from inventory
    /// </summary>
    /// <param name="slotId">Slot ID to remove</param>
    /// <param name="quantity">Quantity to remove (0 = all)</param>
    /// <returns>Removed waste item</returns>
    public WasteItem RemoveWasteItem(string slotId, int quantity = 0)
    {
        if (!inventorySlots.ContainsKey(slotId))
            return null;
        
        var slot = inventorySlots[slotId];
        var wasteItem = slot.wasteItem;
        
        if (quantity <= 0 || quantity >= wasteItem.Quantity)
        {
            // Remove entire slot
            inventorySlots.Remove(slotId);
            UpdateIndices(slot, false);
            UpdateStats(wasteItem, false);
            
            OnWasteRemoved?.Invoke(wasteItem);
            OnInventoryChanged?.Invoke();
            needsSorting = true;
            
            return wasteItem;
        }
        else
        {
            // Partial removal
            var removedItem = wasteItem.CreateCopy();
            removedItem.Quantity = quantity;
            
            wasteItem.Quantity -= quantity;
            slot.lastModified = DateTime.Now;
            
            UpdateStats(removedItem, false);
            
            OnWasteRemoved?.Invoke(removedItem);
            OnInventoryChanged?.Invoke();
            
            return removedItem;
        }
    }
    
    /// <summary>
    /// Get waste items by type
    /// </summary>
    /// <param name="wasteType">Type of waste to find</param>
    /// <param name="maxQuantity">Maximum quantity to retrieve</param>
    /// <returns>List of waste items</returns>
    public List<WasteItem> GetWasteByType(WasteType wasteType, int maxQuantity = int.MaxValue)
    {
        var result = new List<WasteItem>();
        int remainingQuantity = maxQuantity;
        
        if (!wasteTypeIndex.ContainsKey(wasteType))
            return result;
        
        var slots = wasteTypeIndex[wasteType].OrderBy(s => s.addedTime).ToList();
        
        foreach (var slot in slots)
        {
            if (remainingQuantity <= 0)
                break;
            
            var item = slot.wasteItem;
            if (item.Quantity <= remainingQuantity)
            {
                result.Add(item);
                remainingQuantity -= item.Quantity;
            }
            else
            {
                var partialItem = item.CreateCopy();
                partialItem.Quantity = remainingQuantity;
                result.Add(partialItem);
                remainingQuantity = 0;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Get waste items by origin
    /// </summary>
    /// <param name="origin">Origin to filter by</param>
    /// <returns>List of waste items</returns>
    public List<WasteItem> GetWasteByOrigin(WasteOrigin origin)
    {
        var result = new List<WasteItem>();
        
        if (!originIndex.ContainsKey(origin))
            return result;
        
        foreach (var slot in originIndex[origin])
        {
            result.Add(slot.wasteItem);
        }
        
        return result;
    }
    
    /// <summary>
    /// Get best quality waste items for processing
    /// </summary>
    /// <param name="wasteType">Type of waste</param>
    /// <param name="quantity">Desired quantity</param>
    /// <returns>List of high-quality waste items</returns>
    public List<WasteItem> GetBestQualityWaste(WasteType wasteType, int quantity)
    {
        if (!wasteTypeIndex.ContainsKey(wasteType))
            return new List<WasteItem>();
        
        var slots = wasteTypeIndex[wasteType]
            .OrderByDescending(s => s.wasteItem.Quality)
            .ThenBy(s => s.wasteItem.ContaminationLevel)
            .ToList();
        
        var result = new List<WasteItem>();
        int remainingQuantity = quantity;
        
        foreach (var slot in slots)
        {
            if (remainingQuantity <= 0)
                break;
            
            var item = slot.wasteItem;
            if (item.Quantity <= remainingQuantity)
            {
                result.Add(item);
                remainingQuantity -= item.Quantity;
            }
            else
            {
                var partialItem = item.CreateCopy();
                partialItem.Quantity = remainingQuantity;
                result.Add(partialItem);
                remainingQuantity = 0;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Sort inventory by specified criteria
    /// </summary>
    /// <param name="sortBy">Sorting criteria</param>
    public void SortInventory(WasteSortCriteria sortBy = WasteSortCriteria.Quality)
    {
        sortedSlots.Clear();
        sortedSlots.AddRange(inventorySlots.Values);
        
        switch (sortBy)
        {
            case WasteSortCriteria.Quality:
                sortedSlots.Sort((a, b) => b.wasteItem.Quality.CompareTo(a.wasteItem.Quality));
                break;
            case WasteSortCriteria.Type:
                sortedSlots.Sort((a, b) => a.wasteItem.Type.CompareTo(b.wasteItem.Type));
                break;
            case WasteSortCriteria.Origin:
                sortedSlots.Sort((a, b) => a.wasteItem.Origin.CompareTo(b.wasteItem.Origin));
                break;
            case WasteSortCriteria.AddedTime:
                sortedSlots.Sort((a, b) => a.addedTime.CompareTo(b.addedTime));
                break;
            case WasteSortCriteria.Value:
                sortedSlots.Sort((a, b) => b.wasteItem.EstimatedValue.CompareTo(a.wasteItem.EstimatedValue));
                break;
        }
        
        lastSortTime = Time.time;
    }
    
    /// <summary>
    /// Get inventory statistics
    /// </summary>
    /// <returns>Current inventory statistics</returns>
    public WasteInventoryStats GetInventoryStats()
    {
        // Update real-time stats
        stats.currentSlots = inventorySlots.Count;
        stats.totalWeight = inventorySlots.Values.Sum(s => s.wasteItem.Weight * s.wasteItem.Quantity);
        stats.averageQuality = inventorySlots.Values.Average(s => s.wasteItem.Quality);
        stats.averageContamination = inventorySlots.Values.Average(s => s.wasteItem.ContaminationLevel);
        
        return stats;
    }
    
    /// <summary>
    /// Clear all inventory
    /// </summary>
    public void ClearInventory()
    {
        inventorySlots.Clear();
        wasteTypeIndex.Clear();
        originIndex.Clear();
        sortedSlots.Clear();
        
        // Reinitialize indices
        InitializeInventory();
        
        // Reset stats
        stats = new WasteInventoryStats();
        
        OnInventoryChanged?.Invoke();
    }
    
    /// <summary>
    /// Get all inventory slots
    /// </summary>
    /// <returns>List of all inventory slots</returns>
    public List<WasteInventorySlot> GetAllSlots()
    {
        return new List<WasteInventorySlot>(inventorySlots.Values);
    }
    
    /// <summary>
    /// Check if inventory has space
    /// </summary>
    /// <returns>True if inventory has space</returns>
    public bool HasSpace()
    {
        return inventorySlots.Count < maxInventorySlots && 
               GetInventoryStats().totalWeight < maxTotalWeight;
    }
    
    private bool PassesQualityControl(WasteItem wasteItem)
    {
        if (!enableQualityFiltering)
            return true;
        
        if (wasteItem.Quality < minimumQualityThreshold)
            return false;
        
        if (autoRejectContaminated && wasteItem.ContaminationLevel > contaminationThreshold)
            return false;
        
        return true;
    }
    
    private bool TryStackWasteItem(WasteItem wasteItem)
    {
        foreach (var slot in inventorySlots.Values)
        {
            if (slot.wasteItem.CanStackWith(wasteItem))
            {
                slot.wasteItem.Quantity += wasteItem.Quantity;
                slot.lastModified = DateTime.Now;
                return true;
            }
        }
        return false;
    }
    
    private WasteStorageCategory FindBestStorageCategory(WasteItem wasteItem)
    {
        return storageCategories
            .Where(c => c.CanAccept(wasteItem))
            .OrderBy(c => c.priority)
            .FirstOrDefault();
    }
    
    private bool IsCategoryFull(WasteStorageCategory category)
    {
        int slotsInCategory = inventorySlots.Values.Count(s => s.category == category);
        return slotsInCategory >= category.maxSlots;
    }
    
    private bool IsInventoryFull()
    {
        return inventorySlots.Count >= maxInventorySlots || 
               GetInventoryStats().totalWeight >= maxTotalWeight;
    }
    
    private WasteInventorySlot CreateInventorySlot(WasteItem wasteItem, WasteStorageCategory category)
    {
        return new WasteInventorySlot
        {
            wasteItem = wasteItem,
            category = category,
            addedTime = DateTime.Now,
            lastModified = DateTime.Now
        };
    }
    
    private void UpdateIndices(WasteInventorySlot slot, bool add)
    {
        var wasteItem = slot.wasteItem;
        
        if (add)
        {
            wasteTypeIndex[wasteItem.Type].Add(slot);
            originIndex[wasteItem.Origin].Add(slot);
        }
        else
        {
            wasteTypeIndex[wasteItem.Type].Remove(slot);
            originIndex[wasteItem.Origin].Remove(slot);
        }
    }
    
    private void UpdateStats(WasteItem wasteItem, bool add)
    {
        int multiplier = add ? 1 : -1;
        
        stats.totalItemsProcessed += multiplier * wasteItem.Quantity;
        stats.totalValueProcessed += multiplier * wasteItem.EstimatedValue;
        
        if (add)
        {
            stats.itemsAdded += wasteItem.Quantity;
        }
        else
        {
            stats.itemsRemoved += wasteItem.Quantity;
        }
    }
    
    private string GenerateSlotId()
    {
        return Guid.NewGuid().ToString("N")[..8];
    }
    
    private void LoadInventoryData()
    {
        // TODO: Implement save/load functionality
        // This would load inventory data from persistent storage
    }
    
    private void SaveInventoryData()
    {
        // TODO: Implement save/load functionality
        // This would save inventory data to persistent storage
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveInventoryData();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveInventoryData();
        }
    }
}

/// <summary>
/// Waste storage category configuration
/// </summary>
[System.Serializable]
public class WasteStorageCategory
{
    public string name;
    public int priority;
    public List<WasteType> acceptedTypes = new List<WasteType>();
    public int maxSlots;
    public float qualityThreshold;
    public Color categoryColor = Color.white;
    
    public bool CanAccept(WasteItem wasteItem)
    {
        if (acceptedTypes.Count > 0 && !acceptedTypes.Contains(wasteItem.Type))
            return false;
        
        if (wasteItem.Quality < qualityThreshold)
            return false;
        
        return true;
    }
}

/// <summary>
/// Individual inventory slot data
/// </summary>
[System.Serializable]
public class WasteInventorySlot
{
    public string slotId;
    public WasteItem wasteItem;
    public WasteStorageCategory category;
    public DateTime addedTime;
    public DateTime lastModified;
    
    public float GetAgeInHours()
    {
        return (float)(DateTime.Now - addedTime).TotalHours;
    }
}

/// <summary>
/// Inventory statistics data
/// </summary>
[System.Serializable]
public class WasteInventoryStats
{
    public int currentSlots;
    public float totalWeight;
    public float averageQuality;
    public float averageContamination;
    public int totalItemsProcessed;
    public float totalValueProcessed;
    public int itemsAdded;
    public int itemsRemoved;
    public DateTime lastUpdated = DateTime.Now;
}

/// <summary>
/// Sorting criteria for waste inventory
/// </summary>
public enum WasteSortCriteria
{
    Quality,
    Type,
    Origin,
    AddedTime,
    Value,
    Contamination
} 