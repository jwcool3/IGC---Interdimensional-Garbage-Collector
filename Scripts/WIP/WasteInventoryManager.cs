using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Manages waste inventory, storage, and organization
/// Handles waste collection, sorting, and retrieval for processing
/// Now uses UpdatedWasteItem for enhanced resource management
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
    [SerializeField] private float minimumEstimatedValue = 0f;
    
    // Events - Updated to use UpdatedWasteItem
    public static event Action<UpdatedWasteItem> OnWasteAdded;
    public static event Action<UpdatedWasteItem> OnWasteRemoved;
    public static event Action<UpdatedWasteItem> OnWasteRejected;
    public static event Action OnInventoryChanged;
    public static event Action<string> OnInventoryFull;
    public static event Action<WasteStorageCategory> OnCategoryFull;
    
    // Singleton instance
    public static WasteInventoryManager Instance { get; private set; }
    
    // Inventory data - Updated to use UpdatedWasteItem
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
                qualityThreshold = 0.7f,
                minimumEstimatedValue = 10f
            });
            
            storageCategories.Add(new WasteStorageCategory
            {
                name = "Rare Materials",
                priority = 1,
                acceptedTypes = new List<WasteType> { WasteType.Quantum, WasteType.Crystalline, WasteType.Dimensional },
                maxSlots = 15,
                qualityThreshold = 0.5f,
                minimumEstimatedValue = 20f
            });
            
            storageCategories.Add(new WasteStorageCategory
            {
                name = "Organic",
                priority = 2,
                acceptedTypes = new List<WasteType> { WasteType.Organic, WasteType.Food, WasteType.Biological },
                maxSlots = 30,
                qualityThreshold = 0.3f
            });
            
            storageCategories.Add(new WasteStorageCategory
            {
                name = "Hazardous",
                priority = 2,
                acceptedTypes = new List<WasteType> { WasteType.Chemical, WasteType.Radioactive, WasteType.Toxic },
                maxSlots = 10,
                qualityThreshold = 0.4f,
                requiresSpecialHandling = true
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
    /// <param name="wasteItem">UpdatedWasteItem to add</param>
    /// <returns>True if successfully added</returns>
    public bool AddWasteItem(UpdatedWasteItem wasteItem)
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
        slot.slotId = slotId;
        inventorySlots[slotId] = slot;
        
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
    /// <param name="slotId">Slot ID to remove from</param>
    /// <param name="quantity">Quantity to remove (0 = all)</param>
    /// <returns>Removed waste item or null</returns>
    public UpdatedWasteItem RemoveWasteItem(string slotId, int quantity = 0)
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
            return wasteItem;
        }
        else
        {
            // Remove partial quantity
            var removedItem = wasteItem.CreateCopy();
            removedItem.SetQuantity(quantity);
            wasteItem.RemoveQuantity(quantity);
            
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
    /// <param name="wasteType">Type of waste to retrieve</param>
    /// <param name="maxQuantity">Maximum quantity to retrieve</param>
    /// <returns>List of waste items</returns>
    public List<UpdatedWasteItem> GetWasteByType(WasteType wasteType, int maxQuantity = int.MaxValue)
    {
        var result = new List<UpdatedWasteItem>();
        int currentQuantity = 0;
        
        if (!wasteTypeIndex.ContainsKey(wasteType))
            return result;
        
        var slots = wasteTypeIndex[wasteType].OrderByDescending(s => s.wasteItem.Quality).ToList();
        
        foreach (var slot in slots)
        {
            if (currentQuantity >= maxQuantity)
                break;
            
            var item = slot.wasteItem;
            int takeQuantity = Mathf.Min(item.Quantity, maxQuantity - currentQuantity);
            
            if (takeQuantity == item.Quantity)
            {
                result.Add(item);
            }
            else
            {
                var partialItem = item.CreateCopy();
                partialItem.SetQuantity(takeQuantity);
                result.Add(partialItem);
            }
            
            currentQuantity += takeQuantity;
        }
        
        return result;
    }
    
    /// <summary>
    /// Get waste items by origin
    /// </summary>
    /// <param name="origin">Origin to filter by</param>
    /// <returns>List of waste items</returns>
    public List<UpdatedWasteItem> GetWasteByOrigin(WasteOrigin origin)
    {
        var result = new List<UpdatedWasteItem>();
        
        if (!originIndex.ContainsKey(origin))
            return result;
        
        foreach (var slot in originIndex[origin])
        {
            result.Add(slot.wasteItem);
        }
        
        return result;
    }
    
    /// <summary>
    /// Get best quality waste of a specific type
    /// </summary>
    /// <param name="wasteType">Type of waste</param>
    /// <param name="quantity">Desired quantity</param>
    /// <returns>List of best quality waste items</returns>
    public List<UpdatedWasteItem> GetBestQualityWaste(WasteType wasteType, int quantity)
    {
        var result = new List<UpdatedWasteItem>();
        
        if (!wasteTypeIndex.ContainsKey(wasteType))
            return result;
        
        var sortedSlots = wasteTypeIndex[wasteType]
            .OrderByDescending(s => s.wasteItem.Quality)
            .ThenByDescending(s => s.wasteItem.EstimatedValue)
            .ToList();
        
        int remainingQuantity = quantity;
        
        foreach (var slot in sortedSlots)
        {
            if (remainingQuantity <= 0)
                break;
            
            var item = slot.wasteItem;
            int takeQuantity = Mathf.Min(item.Quantity, remainingQuantity);
            
            if (takeQuantity == item.Quantity)
            {
                result.Add(item);
            }
            else
            {
                var partialItem = item.CreateCopy();
                partialItem.SetQuantity(takeQuantity);
                result.Add(partialItem);
            }
            
            remainingQuantity -= takeQuantity;
        }
        
        return result;
    }
    
    /// <summary>
    /// Get waste items with highest estimated value
    /// </summary>
    /// <param name="maxItems">Maximum number of items to return</param>
    /// <returns>List of highest value waste items</returns>
    public List<UpdatedWasteItem> GetHighestValueWaste(int maxItems = 10)
    {
        var result = new List<UpdatedWasteItem>();
        
        var sortedSlots = inventorySlots.Values
            .OrderByDescending(s => s.wasteItem.EstimatedValue)
            .ThenByDescending(s => s.wasteItem.Quality)
            .Take(maxItems)
            .ToList();
        
        foreach (var slot in sortedSlots)
        {
            result.Add(slot.wasteItem);
        }
        
        return result;
    }
    
    /// <summary>
    /// Sort inventory based on criteria
    /// </summary>
    /// <param name="sortBy">Sorting criteria</param>
    public void SortInventory(WasteSortCriteria sortBy = WasteSortCriteria.Quality)
    {
        sortedSlots = inventorySlots.Values.ToList();
        
        switch (sortBy)
        {
            case WasteSortCriteria.Quality:
                sortedSlots = sortedSlots.OrderByDescending(s => s.wasteItem.Quality).ToList();
                break;
            case WasteSortCriteria.Type:
                sortedSlots = sortedSlots.OrderBy(s => s.wasteItem.Type).ToList();
                break;
            case WasteSortCriteria.Origin:
                sortedSlots = sortedSlots.OrderBy(s => s.wasteItem.Origin).ToList();
                break;
            case WasteSortCriteria.AddedTime:
                sortedSlots = sortedSlots.OrderBy(s => s.addedTime).ToList();
                break;
            case WasteSortCriteria.Value:
                sortedSlots = sortedSlots.OrderByDescending(s => s.wasteItem.EstimatedValue).ToList();
                break;
            case WasteSortCriteria.Contamination:
                sortedSlots = sortedSlots.OrderBy(s => s.wasteItem.ContaminationLevel).ToList();
                break;
            case WasteSortCriteria.Rarity:
                sortedSlots = sortedSlots.OrderByDescending(s => (int)s.wasteItem.Rarity).ToList();
                break;
            case WasteSortCriteria.ProcessingPotential:
                sortedSlots = sortedSlots.OrderByDescending(s => s.wasteItem.RecyclingPotential).ToList();
                break;
        }
        
        OnInventoryChanged?.Invoke();
    }
    
    /// <summary>
    /// Get inventory statistics
    /// </summary>
    /// <returns>Current inventory stats</returns>
    public WasteInventoryStats GetInventoryStats()
    {
        stats.currentSlots = inventorySlots.Count;
        stats.totalWeight = inventorySlots.Values.Sum(s => s.wasteItem.Weight * s.wasteItem.Quantity);
        stats.averageQuality = inventorySlots.Values.Average(s => s.wasteItem.Quality);
        stats.averageContamination = inventorySlots.Values.Average(s => s.wasteItem.ContaminationLevel);
        stats.totalEstimatedValue = inventorySlots.Values.Sum(s => s.wasteItem.EstimatedValue * s.wasteItem.Quantity);
        stats.lastUpdated = DateTime.Now;
        
        return stats;
    }
    
    /// <summary>
    /// Clear all inventory
    /// </summary>
    public void ClearInventory()
    {
        foreach (var slot in inventorySlots.Values)
        {
            OnWasteRemoved?.Invoke(slot.wasteItem);
        }
        
        inventorySlots.Clear();
        wasteTypeIndex.Clear();
        originIndex.Clear();
        sortedSlots.Clear();
        
        InitializeInventory();
        OnInventoryChanged?.Invoke();
    }
    
    /// <summary>
    /// Get all inventory slots
    /// </summary>
    /// <returns>List of all slots</returns>
    public List<WasteInventorySlot> GetAllSlots()
    {
        return inventorySlots.Values.ToList();
    }
    
    /// <summary>
    /// Check if inventory has space
    /// </summary>
    /// <returns>True if space available</returns>
    public bool HasSpace()
    {
        return inventorySlots.Count < maxInventorySlots && 
               inventorySlots.Values.Sum(s => s.wasteItem.Weight * s.wasteItem.Quantity) < maxTotalWeight;
    }
    
    /// <summary>
    /// Check if waste item passes quality control
    /// </summary>
    /// <param name="wasteItem">Item to check</param>
    /// <returns>True if passes quality control</returns>
    private bool PassesQualityControl(UpdatedWasteItem wasteItem)
    {
        if (!enableQualityFiltering)
            return true;
        
        // Check minimum quality threshold
        if (wasteItem.Quality < minimumQualityThreshold)
            return false;
        
        // Check contamination threshold
        if (autoRejectContaminated && wasteItem.ContaminationLevel > contaminationThreshold)
            return false;
        
        // Check minimum estimated value
        if (wasteItem.EstimatedValue < minimumEstimatedValue)
            return false;
        
        // Check if item requires special handling but we can't provide it
        if (wasteItem.ItemData != null && wasteItem.ItemData.RequiresSpecialHandling)
        {
            var category = FindBestStorageCategory(wasteItem);
            if (category == null || !category.requiresSpecialHandling)
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Try to stack waste item with existing items
    /// </summary>
    /// <param name="wasteItem">Item to stack</param>
    /// <returns>True if successfully stacked</returns>
    private bool TryStackWasteItem(UpdatedWasteItem wasteItem)
    {
        foreach (var slot in inventorySlots.Values)
        {
            if (slot.wasteItem.CanStackWith(wasteItem))
            {
                slot.wasteItem.AddQuantity(wasteItem.Quantity);
                slot.lastModified = DateTime.Now;
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Find best storage category for waste item
    /// </summary>
    /// <param name="wasteItem">Item to categorize</param>
    /// <returns>Best matching category</returns>
    private WasteStorageCategory FindBestStorageCategory(UpdatedWasteItem wasteItem)
    {
        return storageCategories
            .Where(c => c.CanAccept(wasteItem))
            .OrderBy(c => c.priority)
            .FirstOrDefault();
    }
    
    /// <summary>
    /// Check if storage category is full
    /// </summary>
    /// <param name="category">Category to check</param>
    /// <returns>True if full</returns>
    private bool IsCategoryFull(WasteStorageCategory category)
    {
        int slotsInCategory = inventorySlots.Values.Count(s => s.category == category);
        return slotsInCategory >= category.maxSlots;
    }
    
    /// <summary>
    /// Check if inventory is full
    /// </summary>
    /// <returns>True if full</returns>
    private bool IsInventoryFull()
    {
        return inventorySlots.Count >= maxInventorySlots ||
               inventorySlots.Values.Sum(s => s.wasteItem.Weight * s.wasteItem.Quantity) >= maxTotalWeight;
    }
    
    /// <summary>
    /// Create new inventory slot
    /// </summary>
    /// <param name="wasteItem">Item for the slot</param>
    /// <param name="category">Storage category</param>
    /// <returns>New inventory slot</returns>
    private WasteInventorySlot CreateInventorySlot(UpdatedWasteItem wasteItem, WasteStorageCategory category)
    {
        return new WasteInventorySlot
        {
            wasteItem = wasteItem,
            category = category,
            addedTime = DateTime.Now,
            lastModified = DateTime.Now
        };
    }
    
    /// <summary>
    /// Update search indices
    /// </summary>
    /// <param name="slot">Slot to update</param>
    /// <param name="add">True to add, false to remove</param>
    private void UpdateIndices(WasteInventorySlot slot, bool add)
    {
        var wasteType = slot.wasteItem.Type;
        var origin = slot.wasteItem.Origin;
        
        if (add)
        {
            wasteTypeIndex[wasteType].Add(slot);
            originIndex[origin].Add(slot);
        }
        else
        {
            wasteTypeIndex[wasteType].Remove(slot);
            originIndex[origin].Remove(slot);
        }
    }
    
    /// <summary>
    /// Update inventory statistics
    /// </summary>
    /// <param name="wasteItem">Item being added/removed</param>
    /// <param name="add">True if adding, false if removing</param>
    private void UpdateStats(UpdatedWasteItem wasteItem, bool add)
    {
        if (add)
        {
            stats.itemsAdded++;
            stats.totalValueProcessed += wasteItem.EstimatedValue * wasteItem.Quantity;
        }
        else
        {
            stats.itemsRemoved++;
        }
        
        stats.totalItemsProcessed = stats.itemsAdded + stats.itemsRemoved;
        stats.lastUpdated = DateTime.Now;
    }
    
    /// <summary>
    /// Generate unique slot ID
    /// </summary>
    /// <returns>Unique slot identifier</returns>
    private string GenerateSlotId()
    {
        return System.Guid.NewGuid().ToString("N")[..8];
    }
    
    /// <summary>
    /// Load inventory data from persistent storage
    /// </summary>
    private void LoadInventoryData()
    {
        // TODO: Implement persistent storage loading
        // This would load saved inventory data from PlayerPrefs or file system
    }
    
    /// <summary>
    /// Save inventory data to persistent storage
    /// </summary>
    private void SaveInventoryData()
    {
        // TODO: Implement persistent storage saving
        // This would save current inventory data to PlayerPrefs or file system
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
    
    private void OnDestroy()
    {
        SaveInventoryData();
    }
}

/// <summary>
/// Storage category for organizing waste items
/// Updated to work with UpdatedWasteItem
/// </summary>
[System.Serializable]
public class WasteStorageCategory
{
    public string name;
    public int priority;
    public List<WasteType> acceptedTypes = new List<WasteType>();
    public int maxSlots;
    public float qualityThreshold;
    public float minimumEstimatedValue = 0f;
    public bool requiresSpecialHandling = false;
    public Color categoryColor = Color.white;
    
    /// <summary>
    /// Check if this category can accept the waste item
    /// </summary>
    /// <param name="wasteItem">UpdatedWasteItem to check</param>
    /// <returns>True if can accept</returns>
    public bool CanAccept(UpdatedWasteItem wasteItem)
    {
        // Check quality threshold
        if (wasteItem.Quality < qualityThreshold)
            return false;
        
        // Check estimated value threshold
        if (wasteItem.EstimatedValue < minimumEstimatedValue)
            return false;
        
        // Check special handling requirements
        if (wasteItem.ItemData != null && wasteItem.ItemData.RequiresSpecialHandling && !requiresSpecialHandling)
            return false;
        
        // Check accepted types (empty list means accepts all)
        if (acceptedTypes.Count > 0 && !acceptedTypes.Contains(wasteItem.Type))
            return false;
        
        return true;
    }
}

/// <summary>
/// Individual inventory slot containing waste item
/// Updated to use UpdatedWasteItem
/// </summary>
[System.Serializable]
public class WasteInventorySlot
{
    public string slotId;
    public UpdatedWasteItem wasteItem;
    public WasteStorageCategory category;
    public DateTime addedTime;
    public DateTime lastModified;
    
    /// <summary>
    /// Get age of this slot in hours
    /// </summary>
    /// <returns>Age in hours</returns>
    public float GetAgeInHours()
    {
        return (float)(DateTime.Now - addedTime).TotalHours;
    }
    
    /// <summary>
    /// Get time since last modification in hours
    /// </summary>
    /// <returns>Hours since last modified</returns>
    public float GetTimeSinceModifiedInHours()
    {
        return (float)(DateTime.Now - lastModified).TotalHours;
    }
}

/// <summary>
/// Statistics for waste inventory
/// Updated with new metrics for UpdatedWasteItem
/// </summary>
[System.Serializable]
public class WasteInventoryStats
{
    public int currentSlots;
    public float totalWeight;
    public float averageQuality;
    public float averageContamination;
    public float totalEstimatedValue;
    public int totalItemsProcessed;
    public float totalValueProcessed;
    public int itemsAdded;
    public int itemsRemoved;
    public DateTime lastUpdated = DateTime.Now;
    
    /// <summary>
    /// Get inventory efficiency percentage
    /// </summary>
    /// <returns>Efficiency as percentage (0-100)</returns>
    public float GetEfficiencyPercentage()
    {
        if (totalItemsProcessed == 0) return 0f;
        return (averageQuality * 100f);
    }
    
    /// <summary>
    /// Get average value per item
    /// </summary>
    /// <returns>Average estimated value per item</returns>
    public float GetAverageValuePerItem()
    {
        if (currentSlots == 0) return 0f;
        return totalEstimatedValue / currentSlots;
    }
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
    Contamination,
    Rarity,
    ProcessingPotential
} 