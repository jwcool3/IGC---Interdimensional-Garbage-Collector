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
    
    // Resource configurations
    private Dictionary<ResourceType, ResourceConfig> resourceConfigs = new Dictionary<ResourceType, ResourceConfig>();
    
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
                resourceConfigs[resourceType] = CreateDefaultResourceConfig(resourceType);
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
        resources[ResourceType.Energy] = 100;
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
            ResourceType.Energy => 1000,
            ResourceType.Fuel => 500,
            ResourceType.Food => 200,
            ResourceType.Parts => 100,
            _ => 1000
        };
    }
    
    /// <summary>
    /// Create default resource configuration
    /// </summary>
    private ResourceConfig CreateDefaultResourceConfig(ResourceType resourceType)
    {
        return new ResourceConfig
        {
            resourceType = resourceType,
            displayName = resourceType.ToString(),
            description = GetDefaultResourceDescription(resourceType),
            baseValue = GetDefaultResourceValue(resourceType),
            category = GetDefaultResourceCategory(resourceType),
            isStackable = true,
            maxStackSize = GetDefaultResourceLimit(resourceType)
        };
    }
    
    private string GetDefaultResourceDescription(ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.RecyclingPoints => "Points earned from recycling waste materials",
            ResourceType.DimensionalPotential => "Energy from dimensional anomalies",
            ResourceType.Plastic => "Recycled plastic materials",
            ResourceType.MetalScraps => "Scrap metal from various sources",
            ResourceType.OrganicMatter => "Organic waste and biomass",
            ResourceType.Energy => "Processed energy for ship systems",
            ResourceType.Fuel => "Refined fuel for ship propulsion",
            ResourceType.Food => "Processed organic matter for sustenance",
            ResourceType.Parts => "Manufactured parts for upgrades",
            _ => $"Resource: {resourceType}"
        };
    }
    
    private int GetDefaultResourceValue(ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.RecyclingPoints => 1,
            ResourceType.DimensionalPotential => 10,
            ResourceType.Plastic => 2,
            ResourceType.MetalScraps => 3,
            ResourceType.OrganicMatter => 1,
            ResourceType.Energy => 5,
            ResourceType.Fuel => 8,
            ResourceType.Food => 4,
            ResourceType.Parts => 15,
            ResourceType.CrystalFragments => 25,
            ResourceType.RareMetals => 20,
            ResourceType.QuantumMatter => 100,
            _ => 1
        };
    }
    
    private ResourceCategory GetDefaultResourceCategory(ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.RecyclingPoints or ResourceType.DimensionalPotential => ResourceCategory.Basic,
            ResourceType.Plastic or ResourceType.MetalScraps or ResourceType.OrganicMatter => ResourceCategory.RawMaterial,
            ResourceType.Energy or ResourceType.Fuel or ResourceType.Food or ResourceType.Parts => ResourceCategory.Processed,
            ResourceType.CrystalFragments or ResourceType.RareMetals => ResourceCategory.Advanced,
            ResourceType.QuantumMatter or ResourceType.AlienTech => ResourceCategory.ExoticMaterials,
            _ => ResourceCategory.Basic
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
    /// Add multiple resources at once
    /// </summary>
    /// <param name="resourcesToAdd">Dictionary of resource types and amounts</param>
    /// <returns>True if all resources were added successfully</returns>
    public bool AddResources(Dictionary<ResourceType, int> resourcesToAdd)
    {
        bool allSuccessful = true;
        
        foreach (var resource in resourcesToAdd)
        {
            if (!AddResource(resource.Key, resource.Value))
            {
                allSuccessful = false;
            }
        }
        
        return allSuccessful;
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
    /// Spend multiple resources at once
    /// </summary>
    /// <param name="resourcesToSpend">Dictionary of resource types and amounts</param>
    /// <returns>True if all resources were spent successfully</returns>
    public bool SpendResources(Dictionary<ResourceType, int> resourcesToSpend)
    {
        // First check if we can afford all resources
        if (!CanAffordResources(resourcesToSpend))
        {
            return false;
        }
        
        // Spend all resources
        foreach (var resource in resourcesToSpend)
        {
            SpendResource(resource.Key, resource.Value);
        }
        
        return true;
    }
    
    /// <summary>
    /// Spend resources from ResourceAmount array (for recipes)
    /// </summary>
    /// <param name="resourceAmounts">Array of ResourceAmount</param>
    /// <returns>True if all resources were spent successfully</returns>
    public bool SpendResources(ResourceAmount[] resourceAmounts)
    {
        if (resourceAmounts == null) return true;
        
        var resourceDict = new Dictionary<ResourceType, int>();
        foreach (var resourceAmount in resourceAmounts)
        {
            if (resourceDict.ContainsKey(resourceAmount.type))
            {
                resourceDict[resourceAmount.type] += resourceAmount.amount;
            }
            else
            {
                resourceDict[resourceAmount.type] = resourceAmount.amount;
            }
        }
        
        return SpendResources(resourceDict);
    }
    
    /// <summary>
    /// Check if player can afford specific resources
    /// </summary>
    /// <param name="resourcesToCheck">Dictionary of resource types and amounts</param>
    /// <returns>True if player can afford all resources</returns>
    public bool CanAffordResources(Dictionary<ResourceType, int> resourcesToCheck)
    {
        foreach (var resource in resourcesToCheck)
        {
            if (GetResourceAmount(resource.Key) < resource.Value)
            {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// Check if player can afford resources from ResourceAmount array
    /// </summary>
    /// <param name="resourceAmounts">Array of ResourceAmount</param>
    /// <returns>True if player can afford all resources</returns>
    public bool CanAffordRecipe(ResourceAmount[] resourceAmounts)
    {
        if (resourceAmounts == null) return true;
        
        var resourceDict = new Dictionary<ResourceType, int>();
        foreach (var resourceAmount in resourceAmounts)
        {
            if (resourceDict.ContainsKey(resourceAmount.type))
            {
                resourceDict[resourceAmount.type] += resourceAmount.amount;
            }
            else
            {
                resourceDict[resourceAmount.type] = resourceAmount.amount;
            }
        }
        
        return CanAffordResources(resourceDict);
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
    /// Get resource configuration
    /// </summary>
    /// <param name="resourceType">Resource type</param>
    /// <returns>Resource configuration or null if not found</returns>
    public ResourceConfig GetResourceConfig(ResourceType resourceType)
    {
        return resourceConfigs.ContainsKey(resourceType) ? resourceConfigs[resourceType] : null;
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
    
    /// <summary>
    /// Get resources by category
    /// </summary>
    /// <param name="category">Resource category</param>
    /// <returns>Dictionary of resources in the category</returns>
    public Dictionary<ResourceType, int> GetResourcesByCategory(ResourceCategory category)
    {
        var categoryResources = new Dictionary<ResourceType, int>();
        
        foreach (var resource in resources)
        {
            if (resourceConfigs.ContainsKey(resource.Key) && 
                resourceConfigs[resource.Key].category == category)
            {
                categoryResources[resource.Key] = resource.Value;
            }
        }
        
        return categoryResources;
    }
    
    /// <summary>
    /// Get total value of all resources
    /// </summary>
    /// <returns>Total resource value</returns>
    public int GetTotalResourceValue()
    {
        int totalValue = 0;
        
        foreach (var resource in resources)
        {
            if (resourceConfigs.ContainsKey(resource.Key))
            {
                totalValue += resource.Value * resourceConfigs[resource.Key].baseValue;
            }
        }
        
        return totalValue;
    }
    
    /// <summary>
    /// Add resources from ResourceAmount array (for recipes)
    /// </summary>
    /// <param name="resourceAmounts">Array of ResourceAmount</param>
    /// <returns>True if all resources were added successfully</returns>
    public bool AddResources(ResourceAmount[] resourceAmounts)
    {
        if (resourceAmounts == null) return true;
        
        bool allSuccessful = true;
        foreach (var resourceAmount in resourceAmounts)
        {
            if (!AddResource(resourceAmount.type, resourceAmount.amount))
            {
                allSuccessful = false;
            }
        }
        
        return allSuccessful;
    }
    
    /// <summary>
    /// Add resources from a ResourceYield (from waste processing)
    /// </summary>
    /// <param name="resourceYield">ResourceYield containing resource data</param>
    /// <returns>True if resources were added successfully</returns>
    public bool AddResourceYield(ResourceYield resourceYield)
    {
        if (resourceYield == null) return false;
        
        bool anyAdded = false;
        
        // Add primary resources
        if (resourceYield.primaryResources != null)
        {
            foreach (var primaryResource in resourceYield.primaryResources)
            {
                if (AddResource(primaryResource.type, primaryResource.amount))
                {
                    anyAdded = true;
                }
            }
        }
        
        // Add secondary resources (with chance)
        if (resourceYield.secondaryResources != null)
        {
            foreach (var secondaryResource in resourceYield.secondaryResources)
            {
                // Roll for chance
                float roll = UnityEngine.Random.Range(0f, 1f);
                if (roll <= secondaryResource.chance)
                {
                    if (AddResource(secondaryResource.type, secondaryResource.amount))
                    {
                        anyAdded = true;
                    }
                }
            }
        }
        
        // Add legacy single resource if no primary/secondary resources defined
        if ((resourceYield.primaryResources == null || resourceYield.primaryResources.Length == 0) &&
            (resourceYield.secondaryResources == null || resourceYield.secondaryResources.Length == 0))
        {
            if (resourceYield.resourceType != ResourceType.None && resourceYield.baseAmount > 0)
            {
                if (AddResource(resourceYield.resourceType, resourceYield.baseAmount))
                {
                    anyAdded = true;
                }
            }
        }
        
        return anyAdded;
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
    
    /// <summary>
    /// Reset all resources to default values
    /// </summary>
    public void ResetResources()
    {
        foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
        {
            if (resourceType != ResourceType.None)
            {
                resources[resourceType] = 0;
                resourceLimits[resourceType] = GetDefaultResourceLimit(resourceType);
            }
        }
        
        SetStartingResources();
        OnResourcesUpdated?.Invoke();
        Debug.Log("Resources reset to default values");
    }
    
    #endregion
    
    #region Debug and Utility Methods
    
    /// <summary>
    /// Add debug resources for testing
    /// </summary>
    [ContextMenu("Add Debug Resources")]
    public void AddDebugResources()
    {
        AddResource(ResourceType.RecyclingPoints, 1000);
        AddResource(ResourceType.DimensionalPotential, 100);
        AddResource(ResourceType.Plastic, 50);
        AddResource(ResourceType.MetalScraps, 50);
        AddResource(ResourceType.OrganicMatter, 50);
        AddResource(ResourceType.Energy, 500);
        
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