using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Enhanced resource management system that replaces the legacy ResourceManager
/// Handles all resource types with improved tracking, events, and configuration
/// </summary>
public class NewResourceManager : MonoBehaviour
{
    // Singleton pattern
    public static NewResourceManager Instance { get; private set; }
    
    [Header("Resource Storage")]
    [SerializeField] private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    [SerializeField] private Dictionary<ResourceType, int> resourceLimits = new Dictionary<ResourceType, int>();
    
    [Header("Configuration")]
    [SerializeField] private bool enableResourceLimits = true;
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 30f;
    
    // Resource configurations - get from ResourceConfigManager instead of creating them
    private ResourceConfigManager configManager;
    
    // Events
    public event Action<ResourceType, int, int> OnResourceChanged; // type, oldAmount, newAmount
    public event Action<ResourceType, int> OnResourceAdded; // type, amount
    public event Action<ResourceType, int> OnResourceSpent; // type, amount
    public event Action<ResourceType> OnResourceLimitReached; // type
    public event Action OnResourcesUpdated;
    
    // Auto-save
    private float lastSaveTime;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeResourceSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Connect to ResourceConfigManager
        configManager = ResourceConfigManager.Instance;
        LoadResourceData();
    }
    
    private void Update()
    {
        if (enableAutoSave && Time.time - lastSaveTime > autoSaveInterval)
        {
            SaveResourceData();
            lastSaveTime = Time.time;
        }
    }
    
    /// <summary>
    /// Initialize the resource system with default configurations
    /// </summary>
    private void InitializeResourceSystem()
    {
        // Initialize all resource types with zero amounts
        foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
        {
            if (resourceType != ResourceType.None)
            {
                resources[resourceType] = 0;
                resourceLimits[resourceType] = GetDefaultResourceLimit(resourceType);
            }
        }
        
        // Set some starting resources
        SetStartingResources();
    }
    
    /// <summary>
    /// Set starting resources for new games
    /// </summary>
    private void SetStartingResources()
    {
        resources[ResourceType.RecyclingPoints] = 10;
        resources[ResourceType.DimensionalPotential] = 5;
        resources[ResourceType.Plastic] = 0;
        resources[ResourceType.MetalScraps] = 0;
        resources[ResourceType.OrganicMatter] = 0;
    }
    
    /// <summary>
    /// Get default resource limit for a resource type
    /// </summary>
    private int GetDefaultResourceLimit(ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.RecyclingPoints => 999999,
            ResourceType.DimensionalPotential => 999999,
            ResourceType.Plastic => 1000,
            ResourceType.MetalScraps => 1000,
            ResourceType.OrganicMatter => 1000,
            ResourceType.CrystalFragments => 500,
            ResourceType.NeuralResidue => 500,
            ResourceType.ToxicSludge => 100,
            ResourceType.AlienTech => 50,
            ResourceType.ShipParts => 200,
            ResourceType.CombatData => 100,
            _ => 1000
        };
    }
    
    #region Public API Methods
    
    /// <summary>
    /// Add resources to the inventory
    /// </summary>
    /// <param name="resourceType">Type of resource to add</param>
    /// <param name="amount">Amount to add</param>
    /// <returns>True if resources were added successfully</returns>
    public bool AddResource(ResourceType resourceType, int amount)
    {
        if (amount <= 0 || resourceType == ResourceType.None) return false;
        
        int oldAmount = GetResourceAmount(resourceType);
        int newAmount = oldAmount + amount;
        
        // Check resource limits
        if (enableResourceLimits && newAmount > resourceLimits[resourceType])
        {
            newAmount = resourceLimits[resourceType];
            OnResourceLimitReached?.Invoke(resourceType);
        }
        
        resources[resourceType] = newAmount;
        int actualAdded = newAmount - oldAmount;
        
        if (actualAdded > 0)
        {
            OnResourceChanged?.Invoke(resourceType, oldAmount, newAmount);
            OnResourceAdded?.Invoke(resourceType, actualAdded);
            OnResourcesUpdated?.Invoke();
            
            Debug.Log($"Added {actualAdded} {resourceType}. Total: {newAmount}");
        }
        
        return actualAdded > 0;
    }
    
    /// <summary>
    /// Spend/remove resources from inventory
    /// </summary>
    /// <param name="resourceType">Type of resource to spend</param>
    /// <param name="amount">Amount to spend</param>
    /// <returns>True if resources were spent successfully</returns>
    public bool SpendResource(ResourceType resourceType, int amount)
    {
        if (amount <= 0 || resourceType == ResourceType.None) return false;
        
        int currentAmount = GetResourceAmount(resourceType);
        if (currentAmount < amount) return false;
        
        int newAmount = currentAmount - amount;
        resources[resourceType] = newAmount;
        
        OnResourceChanged?.Invoke(resourceType, currentAmount, newAmount);
        OnResourceSpent?.Invoke(resourceType, amount);
        OnResourcesUpdated?.Invoke();
        
        Debug.Log($"Spent {amount} {resourceType}. Remaining: {newAmount}");
        return true;
    }
    
    /// <summary>
    /// Get the current amount of a specific resource
    /// </summary>
    /// <param name="resourceType">Type of resource to check</param>
    /// <returns>Current amount of the resource</returns>
    public int GetResourceAmount(ResourceType resourceType)
    {
        if (resourceType == ResourceType.None) return 0;
        return resources.ContainsKey(resourceType) ? resources[resourceType] : 0;
    }
    
    /// <summary>
    /// Check if the player has enough of a specific resource
    /// </summary>
    /// <param name="resourceType">Type of resource to check</param>
    /// <param name="amount">Amount required</param>
    /// <returns>True if the player has enough resources</returns>
    public bool HasResource(ResourceType resourceType, int amount)
    {
        if (resourceType == ResourceType.None || amount <= 0) return true;
        return GetResourceAmount(resourceType) >= amount;
    }

    /// <summary>
    /// Set the amount of a specific resource
    /// </summary>
    /// <param name="resourceType">Type of resource to set</param>
    /// <param name="amount">Amount to set</param>
    public void SetResource(ResourceType resourceType, int amount)
    {
        if (resourceType == ResourceType.None) return;
        
        int oldAmount = GetResourceAmount(resourceType);
        int newAmount = Mathf.Max(0, amount);
        
        // Check resource limits
        if (enableResourceLimits && newAmount > resourceLimits[resourceType])
        {
            newAmount = resourceLimits[resourceType];
            OnResourceLimitReached?.Invoke(resourceType);
        }
        
        resources[resourceType] = newAmount;
        
        // Fire events
        OnResourceChanged?.Invoke(resourceType, oldAmount, newAmount);
        OnResourcesUpdated?.Invoke();
    }
    
    /// <summary>
    /// Get all resources as a dictionary
    /// </summary>
    /// <returns>Dictionary containing all resource types and their amounts</returns>
    public Dictionary<ResourceType, int> GetAllResources()
    {
        return new Dictionary<ResourceType, int>(resources);
    }
    
    /// <summary>
    /// Get all non-zero resources
    /// </summary>
    /// <returns>Dictionary containing all resources with amounts > 0</returns>
    public Dictionary<ResourceType, int> GetAllNonZeroResources()
    {
        return resources.Where(r => r.Value > 0).ToDictionary(r => r.Key, r => r.Value);
    }
    
    /// <summary>
    /// Get resource configuration from ResourceConfigManager
    /// </summary>
    /// <param name="resourceType">Resource type</param>
    /// <returns>Resource configuration or null if not found</returns>
    public ResourceConfig GetResourceConfig(ResourceType resourceType)
    {
        if (configManager != null)
        {
            return configManager.GetResourceConfig(resourceType);
        }
        return null;
    }
    
    /// <summary>
    /// Set resource limit
    /// </summary>
    /// <param name="resourceType">Resource type</param>
    /// <param name="limit">New limit</param>
    public void SetResourceLimit(ResourceType resourceType, int limit)
    {
        resourceLimits[resourceType] = limit;
        
        // Clamp current amount if it exceeds new limit
        if (resources[resourceType] > limit)
        {
            int oldAmount = resources[resourceType];
            resources[resourceType] = limit;
            OnResourceChanged?.Invoke(resourceType, oldAmount, limit);
            OnResourcesUpdated?.Invoke();
        }
    }
    
    /// <summary>
    /// Get resource limit
    /// </summary>
    /// <param name="resourceType">Resource type</param>
    /// <returns>Resource limit</returns>
    public int GetResourceLimit(ResourceType resourceType)
    {
        return resourceLimits.ContainsKey(resourceType) ? resourceLimits[resourceType] : 0;
    }
    
    /// <summary>
    /// Check if resource is at limit
    /// </summary>
    /// <param name="resourceType">Resource type</param>
    /// <returns>True if at limit</returns>
    public bool IsAtResourceLimit(ResourceType resourceType)
    {
        return GetResourceAmount(resourceType) >= GetResourceLimit(resourceType);
    }
    
    #endregion
    
    #region Save/Load System
    
    /// <summary>
    /// Save resource data to PlayerPrefs
    /// </summary>
    public void SaveResourceData()
    {
        foreach (var resource in resources)
        {
            PlayerPrefs.SetInt($"Resource_{resource.Key}", resource.Value);
        }
        
        foreach (var limit in resourceLimits)
        {
            PlayerPrefs.SetInt($"ResourceLimit_{limit.Key}", limit.Value);
        }
        
        PlayerPrefs.Save();
        Debug.Log("Resource data saved");
    }
    
    /// <summary>
    /// Load resource data from PlayerPrefs
    /// </summary>
    public void LoadResourceData()
    {
        foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
        {
            if (resourceType != ResourceType.None)
            {
                string resourceKey = $"Resource_{resourceType}";
                string limitKey = $"ResourceLimit_{resourceType}";
                
                if (PlayerPrefs.HasKey(resourceKey))
                {
                    resources[resourceType] = PlayerPrefs.GetInt(resourceKey);
                }
                
                if (PlayerPrefs.HasKey(limitKey))
                {
                    resourceLimits[resourceType] = PlayerPrefs.GetInt(limitKey);
                }
            }
        }
        
        OnResourcesUpdated?.Invoke();
        Debug.Log("Resource data loaded");
    }
    
    #endregion
    
    #region Debug and Utility Methods
    
    /// <summary>
    /// Add debug resources for testing
    /// </summary>
    [ContextMenu("Add Debug Resources")]
    public void AddDebugResources()
    {
        AddResource(ResourceType.Plastic, 25);
        AddResource(ResourceType.MetalScraps, 15);
        AddResource(ResourceType.OrganicMatter, 20);
        AddResource(ResourceType.CrystalFragments, 8);
        AddResource(ResourceType.NeuralResidue, 12);
        AddResource(ResourceType.ToxicSludge, 5);
        
        Debug.Log("Debug resources added");
    }
    
    /// <summary>
    /// Get resource summary for debugging
    /// </summary>
    /// <returns>String summary of all resources</returns>
    public string GetResourceSummary()
    {
        var summary = "Current Resources:\n";
        
        foreach (var resource in resources.Where(r => r.Value > 0).OrderByDescending(r => r.Value))
        {
            summary += $"• {resource.Key}: {resource.Value:N0}\n";
        }
        
        return summary;
    }
    
    /// <summary>
    /// Log current resource state
    /// </summary>
    [ContextMenu("Log Resource State")]
    public void LogResourceState()
    {
        Debug.Log(GetResourceSummary());
    }
    
    #endregion
    
    private void OnDestroy()
    {
        if (enableAutoSave)
        {
            SaveResourceData();
        }
    }
}