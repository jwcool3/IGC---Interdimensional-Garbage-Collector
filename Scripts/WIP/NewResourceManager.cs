using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class NewResourceManager : MonoBehaviour
{
    // Singleton pattern
    public static NewResourceManager Instance { get; private set; }
    
    [Header("Resource Storage")]
    [SerializeField] private Dictionary<ResourceType, int> resourceInventory = new Dictionary<ResourceType, int>();
    
    [Header("Resource Configurations")]
    [SerializeField] private ResourceConfig[] resourceConfigs;
    
    [Header("Storage Limits")]
    [SerializeField] private int defaultStorageLimit = 1000;
    [SerializeField] private Dictionary<ResourceType, int> storageLimits = new Dictionary<ResourceType, int>();
    
    // Events for UI updates
    public event Action<ResourceType, int> OnResourceChanged;
    public event Action<ResourceType, int, int> OnResourceAddedWithLimit; // resource, amount, current total
    public event Action OnResourceInventoryChanged;
    
    // Processing facilities and modifiers
    private Dictionary<string, float> facilityEfficiencyModifiers = new Dictionary<string, float>();
    
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
    
    private void InitializeResourceSystem()
    {
        // Initialize all resource types to 0
        foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
        {
            resourceInventory[resourceType] = 0;
            storageLimits[resourceType] = defaultStorageLimit;
        }
        
        // Load resource configs if assigned
        if (resourceConfigs != null)
        {
            foreach (var config in resourceConfigs)
            {
                Debug.Log($"Loaded config for {config.resourceType}: {config.displayName}");
            }
        }
        
        Debug.Log("NewResourceManager initialized with multi-resource system");
    }
    
    #region Resource Management
    
    /// <summary>
    /// Add a specific amount of a resource type
    /// </summary>
    public bool AddResource(ResourceType type, int amount)
    {
        if (amount <= 0) return false;
        
        int currentAmount = GetResourceAmount(type);
        int storageLimit = GetStorageLimit(type);
        int actualAddAmount = Mathf.Min(amount, storageLimit - currentAmount);
        
        if (actualAddAmount > 0)
        {
            resourceInventory[type] = currentAmount + actualAddAmount;
            OnResourceChanged?.Invoke(type, resourceInventory[type]);
            OnResourceAddedWithLimit?.Invoke(type, actualAddAmount, resourceInventory[type]);
            OnResourceInventoryChanged?.Invoke();
            
            Debug.Log($"Added {actualAddAmount} {type}. Total: {resourceInventory[type]}/{storageLimit}");
            return true;
        }
        
        return false; // Storage full
    }
    
    /// <summary>
    /// Add multiple resources from a resource yield
    /// </summary>
    public bool AddResourceYield(ResourceYield yield)
    {
        bool anyAdded = false;
        
        // Add primary resources
        foreach (var resource in yield.primaryResources)
        {
            if (AddResource(resource.type, resource.amount))
            {
                anyAdded = true;
            }
        }
        
        // Roll for secondary resources
        foreach (var chance in yield.secondaryResources)
        {
            if (UnityEngine.Random.value <= chance.chance)
            {
                if (AddResource(chance.type, chance.amount))
                {
                    anyAdded = true;
                    Debug.Log($"Bonus resource! +{chance.amount} {chance.type}");
                }
            }
        }
        
        // Handle contamination
        if (yield.contaminationRisk > 0 && UnityEngine.Random.value <= yield.contaminationRisk)
        {
            // You can integrate this with your existing contamination system
            Debug.Log($"Contamination risk triggered! Risk: {yield.contaminationRisk:P0}");
        }
        
        return anyAdded;
    }
    
    /// <summary>
    /// Spend/consume resources
    /// </summary>
    public bool SpendResource(ResourceType type, int amount)
    {
        if (amount <= 0) return true;
        
        int currentAmount = GetResourceAmount(type);
        if (currentAmount >= amount)
        {
            resourceInventory[type] = currentAmount - amount;
            OnResourceChanged?.Invoke(type, resourceInventory[type]);
            OnResourceInventoryChanged?.Invoke();
            
            Debug.Log($"Spent {amount} {type}. Remaining: {resourceInventory[type]}");
            return true;
        }
        
        Debug.LogWarning($"Insufficient {type}! Need {amount}, have {currentAmount}");
        return false;
    }
    
    /// <summary>
    /// Spend multiple resources (for crafting recipes)
    /// </summary>
    public bool SpendResources(ResourceAmount[] costs)
    {
        // First check if we have enough of everything
        foreach (var cost in costs)
        {
            if (GetResourceAmount(cost.type) < cost.amount)
            {
                Debug.LogWarning($"Cannot afford recipe: need {cost.amount} {cost.type}, have {GetResourceAmount(cost.type)}");
                return false;
            }
        }
        
        // If we can afford everything, spend it
        foreach (var cost in costs)
        {
            SpendResource(cost.type, cost.amount);
        }
        
        return true;
    }
    
    /// <summary>
    /// Get current amount of a resource
    /// </summary>
    public int GetResourceAmount(ResourceType type)
    {
        return resourceInventory.TryGetValue(type, out int amount) ? amount : 0;
    }
    
    /// <summary>
    /// Get storage limit for a resource
    /// </summary>
    public int GetStorageLimit(ResourceType type)
    {
        return storageLimits.TryGetValue(type, out int limit) ? limit : defaultStorageLimit;
    }
    
    /// <summary>
    /// Check if we have enough resources for a recipe
    /// </summary>
    public bool CanAffordRecipe(ResourceAmount[] costs)
    {
        return costs.All(cost => GetResourceAmount(cost.type) >= cost.amount);
    }
    
    #endregion
    
    #region Processing System
    
    /// <summary>
    /// Process resources according to a recipe (e.g., Plastic → Fuel)
    /// </summary>
    public bool ProcessResources(ProcessingRecipe recipe)
    {
        // Check if we have the required facility
        if (!string.IsNullOrEmpty(recipe.requiredFacility))
        {
            float efficiency = GetFacilityEfficiency(recipe.requiredFacility);
            if (efficiency <= 0)
            {
                Debug.LogWarning($"Processing failed: {recipe.requiredFacility} not available or not functional");
                return false;
            }
        }
        
        // Check if we can afford the inputs
        if (!CanAffordRecipe(recipe.inputs))
        {
            return false;
        }
        
        // Spend input resources
        if (!SpendResources(recipe.inputs))
        {
            return false;
        }
        
        // Add output resources (with facility efficiency bonus)
        float efficiency = GetFacilityEfficiency(recipe.requiredFacility);
        foreach (var output in recipe.outputs)
        {
            int bonusAmount = Mathf.RoundToInt(output.amount * efficiency);
            AddResource(output.type, bonusAmount);
        }
        
        Debug.Log($"Processed {recipe.recipeName} successfully!");
        return true;
    }
    
    /// <summary>
    /// Set facility efficiency modifier
    /// </summary>
    public void SetFacilityEfficiency(string facilityName, float efficiency)
    {
        facilityEfficiencyModifiers[facilityName] = efficiency;
        Debug.Log($"Set {facilityName} efficiency to {efficiency:P0}");
    }
    
    private float GetFacilityEfficiency(string facilityName)
    {
        if (string.IsNullOrEmpty(facilityName)) return 1f;
        return facilityEfficiencyModifiers.TryGetValue(facilityName, out float efficiency) ? efficiency : 1f;
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Get all non-zero resources for UI display
    /// </summary>
    public Dictionary<ResourceType, int> GetAllNonZeroResources()
    {
        return resourceInventory.Where(kvp => kvp.Value > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    
    /// <summary>
    /// Get resource config for UI display
    /// </summary>
    public ResourceConfig GetResourceConfig(ResourceType type)
    {
        return resourceConfigs?.FirstOrDefault(config => config.resourceType == type);
    }
    
    /// <summary>
    /// Increase storage capacity for a specific resource type
    /// </summary>
    public void IncreaseStorageCapacity(ResourceType type, int additionalCapacity)
    {
        storageLimits[type] = GetStorageLimit(type) + additionalCapacity;
        Debug.Log($"Increased {type} storage capacity by {additionalCapacity}. New limit: {storageLimits[type]}");
    }
    
    /// <summary>
    /// Get total "value" of all resources for scoring/trading
    /// </summary>
    public int GetTotalResourceValue()
    {
        int totalValue = 0;
        foreach (var kvp in resourceInventory)
        {
            var config = GetResourceConfig(kvp.Key);
            int baseValue = config?.baseValue ?? 1;
            totalValue += kvp.Value * baseValue;
        }
        return totalValue;
    }
    
    #endregion
    
    #region Legacy Compatibility (Optional)
    
    /// <summary>
    /// For backward compatibility - convert old RP system to new resources
    /// </summary>
    public void ConvertLegacyRecyclingPoints(float recyclingPoints)
    {
        // Example conversion: 10 RP = 1 random basic resource
        int resourceCount = Mathf.FloorToInt(recyclingPoints / 10f);
        
        ResourceType[] basicResources = { ResourceType.Plastic, ResourceType.MetalScraps, ResourceType.OrganicMatter };
        ResourceType randomResource = basicResources[UnityEngine.Random.Range(0, basicResources.Length)];
        
        AddResource(randomResource, resourceCount);
        Debug.Log($"Converted {recyclingPoints} RP to {resourceCount} {randomResource}");
    }
    
    #endregion
}