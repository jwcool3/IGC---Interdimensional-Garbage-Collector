using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Complete resource management system that handles all resource types
/// Combines legacy compatibility with modern resource management
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Resource Storage")]
    [SerializeField] private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    [SerializeField] private Dictionary<ResourceType, int> resourceLimits = new Dictionary<ResourceType, int>();

    [Header("Legacy Compatibility Settings")]
    [SerializeField] private bool enableLegacyMode = true;
    [SerializeField] private float rpToPlasticRatio = 10f; // 10 RP = 1 Plastic
    [SerializeField] private float dpToCrystalRatio = 5f;  // 5 DP = 1 Crystal Fragment

    [Header("Conversion Rates")]
    [SerializeField] private float plasticToRpRatio = 10f;
    [SerializeField] private float crystalToDpRatio = 5f;
    [SerializeField] private float metalToRpRatio = 20f;

    [Header("Configuration")]
    [SerializeField] private bool enableResourceLimits = true;
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 30f;

    // Resource configurations
    private Dictionary<ResourceType, ResourceConfig> resourceConfigs = new Dictionary<ResourceType, ResourceConfig>();

    // Effect modifiers for ship compartments
    private float recyclingMultiplier = 1f;
    private float contaminationReductionModifier = 0f;

    // Modern events
    public event Action<ResourceType, int, int> OnResourceChanged; // type, oldAmount, newAmount
    public event Action<ResourceType, int> OnResourceAdded; // type, amount
    public event Action<ResourceType, int> OnResourceSpent; // type, amount
    public event Action<ResourceType> OnResourceLimitReached; // type
    public event Action OnResourcesUpdated;
    public event Action OnResourceInventoryChanged; // Legacy compatibility event

    // Legacy events (maintain existing API)
    public event Action<float> OnRecyclingPointsChanged;
    public event Action<float> OnDimensionalPotentialChanged;
    public event Action<float> OnContaminationChanged;
    public event Action OnResourcesChanged;

    // Legacy properties that delegate to new system
    public float RecyclingPoints => GetLegacyRecyclingPoints();
    public float DimensionalPotential => GetLegacyDimensionalPotential();
    public float ContaminationLevel { get; private set; }

    // Combat resources (delegate to new system)
    public int ShipParts => GetResourceAmount(ResourceType.ShipParts);
    public int AlienTech => GetResourceAmount(ResourceType.AlienTech);
    public int CombatData => GetResourceAmount(ResourceType.CombatData);

    // Auto-save
    private float lastSaveTime;

    private void Awake()
    {
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
        // Auto-save functionality
        if (enableAutoSave && Time.time - lastSaveTime >= autoSaveInterval)
        {
            SaveResourceData();
            lastSaveTime = Time.time;
        }
    }

    private void InitializeResourceSystem()
    {
        // Initialize all resource types with zero amounts
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            if (resourceType != ResourceType.None)
            {
                resources[resourceType] = 0;
                resourceLimits[resourceType] = enableResourceLimits ? 1000 : int.MaxValue;
            }
        }

        Debug.Log("ResourceManager initialized successfully");
    }

    #region Core Resource Management

    /// <summary>
    /// Add resources to the inventory
    /// </summary>
    /// <param name="resourceType">Type of resource to add</param>
    /// <param name="amount">Amount to add</param>
    /// <returns>True if resources were added successfully</returns>
    public bool AddResource(ResourceType resourceType, int amount)
    {
        if (resourceType == ResourceType.None || amount <= 0) return false;

        int oldAmount = GetResourceAmount(resourceType);
        int newAmount = oldAmount + amount;

        // Check resource limits
        if (enableResourceLimits && newAmount > resourceLimits[resourceType])
        {
            newAmount = resourceLimits[resourceType];
            OnResourceLimitReached?.Invoke(resourceType);
        }

        resources[resourceType] = newAmount;

        // Fire events
        OnResourceChanged?.Invoke(resourceType, oldAmount, newAmount);
        OnResourceAdded?.Invoke(resourceType, newAmount - oldAmount);
        OnResourcesUpdated?.Invoke();

        // Fire legacy events if needed
        FireLegacyEvents(resourceType);

        return true;
    }

    /// <summary>
    /// Spend resources from the inventory
    /// </summary>
    /// <param name="resourceType">Type of resource to spend</param>
    /// <param name="amount">Amount to spend</param>
    /// <returns>True if resources were spent successfully</returns>
    public bool SpendResource(ResourceType resourceType, int amount)
    {
        if (resourceType == ResourceType.None || amount <= 0) return false;

        int currentAmount = GetResourceAmount(resourceType);
        if (currentAmount < amount) return false;

        int oldAmount = currentAmount;
        int newAmount = currentAmount - amount;
        resources[resourceType] = newAmount;

        // Fire events
        OnResourceChanged?.Invoke(resourceType, oldAmount, newAmount);
        OnResourceSpent?.Invoke(resourceType, amount);
        OnResourcesUpdated?.Invoke();

        // Fire legacy events if needed
        FireLegacyEvents(resourceType);

        return true;
    }

    /// <summary>
    /// Get the current amount of a specific resource
    /// </summary>
    /// <param name="resourceType">Type of resource to check</param>
    /// <returns>Current amount of the resource</returns>
    public int GetResourceAmount(ResourceType resourceType)
    {
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
    /// Check if the player can afford a recipe's input requirements
    /// </summary>
    /// <param name="inputs">Array of required resource amounts</param>
    /// <returns>True if the player has enough of all required resources</returns>
    public bool CanAffordRecipe(ResourceAmount[] inputs)
    {
        if (inputs == null || inputs.Length == 0) return true;
        
        foreach (var input in inputs)
        {
            if (!HasResource(input.type, input.amount))
            {
                return false;
            }
        }
        
        return true;
    }

    /// <summary>
    /// Spend multiple resources at once (used by processing recipes)
    /// </summary>
    /// <param name="resources">Array of resources to spend</param>
    /// <returns>True if all resources were spent successfully</returns>
    public bool SpendResources(ResourceAmount[] resources)
    {
        if (resources == null || resources.Length == 0) return true;
        
        // First check if we can afford all resources
        if (!CanAffordRecipe(resources))
        {
            return false;
        }
        
        // Then spend all resources
        foreach (var resource in resources)
        {
            if (!SpendResource(resource.type, resource.amount))
            {
                Debug.LogError($"Failed to spend {resource.amount} {resource.type}");
                return false;
            }
        }
        
        return true;
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

        // Fire legacy events if needed
        FireLegacyEvents(resourceType);
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
    /// Add resources from a ResourceYield
    /// </summary>
    /// <param name="yield">ResourceYield containing resources to add</param>
    /// <returns>True if resources were added successfully</returns>
    public bool AddResourceYield(ResourceYield yield)
    {
        if (yield == null) return false;

        bool success = true;

        // Add primary resources
        if (yield.primaryResources != null)
        {
            foreach (var resource in yield.primaryResources)
            {
                if (!AddResource(resource.type, resource.amount))
                {
                    success = false;
                }
            }
        }

        // Add secondary resources (with chance)
        if (yield.secondaryResources != null)
        {
            foreach (var resource in yield.secondaryResources)
            {
                if (UnityEngine.Random.value <= resource.chance)
                {
                    if (!AddResource(resource.type, resource.amount))
                    {
                        success = false;
                    }
                }
            }
        }

        return success;
    }

    #endregion

    #region Legacy API Methods

    /// <summary>
    /// Legacy method: Add recycling points (converts to resources)
    /// </summary>
    public void AddRecyclingPoints(float amount)
    {
        // Convert RP to plastic resources
        int plasticAmount = Mathf.RoundToInt(amount / rpToPlasticRatio);
        if (plasticAmount > 0)
        {
            AddResource(ResourceType.Plastic, plasticAmount);
        }

        Debug.Log($"Legacy: Added {amount} RP → {plasticAmount} Plastic");
    }

    /// <summary>
    /// Legacy method: Spend recycling points
    /// </summary>
    public bool SpendRecyclingPoints(float amount)
    {
        // Calculate how much plastic we need
        int plasticNeeded = Mathf.CeilToInt(amount / rpToPlasticRatio);

        // Try to spend plastic first, then other basic resources
        if (GetResourceAmount(ResourceType.Plastic) >= plasticNeeded)
        {
            return SpendResource(ResourceType.Plastic, plasticNeeded);
        }

        // Fallback: spend equivalent value in other resources
        return SpendEquivalentResources(amount);
    }

    /// <summary>
    /// Legacy method: Add dimensional potential (converts to crystal fragments)
    /// </summary>
    public void AddDimensionalPotential(float amount)
    {
        // Convert DP to crystal fragments
        int crystalAmount = Mathf.RoundToInt(amount / dpToCrystalRatio);
        if (crystalAmount > 0)
        {
            AddResource(ResourceType.CrystalFragments, crystalAmount);
        }

        Debug.Log($"Legacy: Added {amount} DP → {crystalAmount} Crystal Fragments");
    }

    /// <summary>
    /// Legacy method: Spend dimensional potential
    /// </summary>
    public bool SpendDimensionalPotential(float amount)
    {
        // Calculate how many crystals we need
        int crystalsNeeded = Mathf.CeilToInt(amount / dpToCrystalRatio);

        return SpendResource(ResourceType.CrystalFragments, crystalsNeeded);
    }

    /// <summary>
    /// Legacy method: Process waste item (simplified version without UpdatedWasteProcessor)
    /// </summary>
    public void ProcessWasteItem(WasteItem item)
    {
        if (item == null) return;

        // Direct resource conversion for now
        ProcessLegacyWasteItem(item);
    }

    /// <summary>
    /// Legacy contamination methods
    /// </summary>
    public void IncreaseContamination(float amount)
    {
        ContaminationLevel += amount;
        OnContaminationChanged?.Invoke(ContaminationLevel);
    }

    public void DecreaseContamination(float amount)
    {
        ContaminationLevel = Mathf.Max(0, ContaminationLevel - amount);
        OnContaminationChanged?.Invoke(ContaminationLevel);
    }

    /// <summary>
    /// Legacy combat resource methods
    /// </summary>
    public void AddShipParts(int amount)
    {
        AddResource(ResourceType.ShipParts, amount);
    }

    public void AddAlienTech(int amount)
    {
        AddResource(ResourceType.AlienTech, amount);
    }

    public void AddCombatData(int amount)
    {
        AddResource(ResourceType.CombatData, amount);
    }

    public bool SpendShipParts(int amount)
    {
        return SpendResource(ResourceType.ShipParts, amount);
    }

    public bool SpendAlienTech(int amount)
    {
        return SpendResource(ResourceType.AlienTech, amount);
    }

    public bool SpendCombatData(int amount)
    {
        return SpendResource(ResourceType.CombatData, amount);
    }

    #endregion

    #region Helper Methods

    private void FireLegacyEvents(ResourceType resourceType)
    {
        if (!enableLegacyMode) return;

        switch (resourceType)
        {
            case ResourceType.Plastic:
            case ResourceType.MetalScraps:
            case ResourceType.OrganicMatter:
                OnRecyclingPointsChanged?.Invoke(GetLegacyRecyclingPoints());
                break;

            case ResourceType.CrystalFragments:
            case ResourceType.NeuralResidue:
                OnDimensionalPotentialChanged?.Invoke(GetLegacyDimensionalPotential());
                break;
        }

        OnResourcesChanged?.Invoke();
        OnResourceInventoryChanged?.Invoke();
    }

    private float GetLegacyRecyclingPoints()
    {
        // Convert current resources back to legacy RP for UI compatibility
        float totalRP = 0f;
        totalRP += GetResourceAmount(ResourceType.Plastic) * plasticToRpRatio;
        totalRP += GetResourceAmount(ResourceType.MetalScraps) * metalToRpRatio;
        totalRP += GetResourceAmount(ResourceType.OrganicMatter) * plasticToRpRatio;

        return totalRP;
    }

    private float GetLegacyDimensionalPotential()
    {
        // Convert crystal fragments and neural residue to legacy DP
        float totalDP = 0f;
        totalDP += GetResourceAmount(ResourceType.CrystalFragments) * crystalToDpRatio;
        totalDP += GetResourceAmount(ResourceType.NeuralResidue) * crystalToDpRatio;

        return totalDP;
    }

    private bool SpendEquivalentResources(float rpAmount)
    {
        // Try to spend equivalent value from available resources
        float remainingValue = rpAmount;

        // Try metal scraps (worth more RP)
        int metalAvailable = GetResourceAmount(ResourceType.MetalScraps);
        int metalToSpend = Mathf.Min(metalAvailable, Mathf.FloorToInt(remainingValue / metalToRpRatio));
        if (metalToSpend > 0)
        {
            SpendResource(ResourceType.MetalScraps, metalToSpend);
            remainingValue -= metalToSpend * metalToRpRatio;
        }

        // Try organic matter
        if (remainingValue > 0)
        {
            int organicNeeded = Mathf.CeilToInt(remainingValue / plasticToRpRatio);
            int organicAvailable = GetResourceAmount(ResourceType.OrganicMatter);
            if (organicAvailable >= organicNeeded)
            {
                SpendResource(ResourceType.OrganicMatter, organicNeeded);
                remainingValue = 0;
            }
        }

        return remainingValue <= 0;
    }

    private void ProcessLegacyWasteItem(WasteItem item)
    {
        // Simplified processing for compatibility
        float rpValue = item.RecyclingValue * 10f;
        float dpValue = item.RecyclingPotential * 5f;

        AddRecyclingPoints(rpValue);
        AddDimensionalPotential(dpValue);

        // Add contamination
        IncreaseContamination(item.ContaminationLevel * 0.1f);
    }

    private void LoadResourceData()
    {
        // Load saved resource data from PlayerPrefs or save file
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            if (resourceType != ResourceType.None)
            {
                string key = $"Resource_{resourceType}";
                int savedAmount = PlayerPrefs.GetInt(key, 0);
                resources[resourceType] = savedAmount;
            }
        }

        Debug.Log("Resource data loaded");
    }

    private void SaveResourceData()
    {
        // Save resource data to PlayerPrefs
        foreach (var resource in resources)
        {
            string key = $"Resource_{resource.Key}";
            PlayerPrefs.SetInt(key, resource.Value);
        }

        PlayerPrefs.Save();
        lastSaveTime = Time.time;
    }

    #endregion

    #region Public Utility Methods

    public float GetRecyclingPoints() => RecyclingPoints;
    public float GetDimensionalPotential() => DimensionalPotential;
    public float GetContamination() => ContaminationLevel;

    public void SetRecyclingPoints(float value)
    {
        // Convert to plastic resources
        int targetPlastic = Mathf.RoundToInt(value / rpToPlasticRatio);
        int currentPlastic = GetResourceAmount(ResourceType.Plastic);

        if (targetPlastic > currentPlastic)
        {
            AddResource(ResourceType.Plastic, targetPlastic - currentPlastic);
        }
    }

    public void SetDimensionalPotential(float value)
    {
        // Convert to crystal fragments
        int targetCrystals = Mathf.RoundToInt(value / dpToCrystalRatio);
        int currentCrystals = GetResourceAmount(ResourceType.CrystalFragments);

        if (targetCrystals > currentCrystals)
        {
            AddResource(ResourceType.CrystalFragments, targetCrystals - currentCrystals);
        }
    }

    /// <summary>
    /// Enable or disable legacy compatibility mode
    /// </summary>
    public void SetLegacyMode(bool enabled)
    {
        enableLegacyMode = enabled;
        Debug.Log($"Legacy compatibility mode: {(enabled ? "Enabled" : "Disabled")}");
    }

    /// <summary>
    /// Set resource limit for a specific resource type
    /// </summary>
    public void SetResourceLimit(ResourceType resourceType, int limit)
    {
        if (resourceType != ResourceType.None)
        {
            resourceLimits[resourceType] = limit;
        }
    }

    /// <summary>
    /// Get resource limit for a specific resource type
    /// </summary>
    public int GetResourceLimit(ResourceType resourceType)
    {
        return resourceLimits.ContainsKey(resourceType) ? resourceLimits[resourceType] : int.MaxValue;
    }

    /// <summary>
    /// Set recycling efficiency multiplier (used by ship compartments)
    /// </summary>
    /// <param name="multiplier">Multiplier value (1.0 = normal, 1.5 = 50% bonus)</param>
    public void SetRecyclingMultiplier(float multiplier)
    {
        recyclingMultiplier = Mathf.Max(0f, multiplier);
        Debug.Log($"Recycling multiplier set to {recyclingMultiplier:F2}x");
    }

    /// <summary>
    /// Set contamination reduction modifier (used by ship compartments)
    /// </summary>
    /// <param name="reduction">Contamination reduction value</param>
    public void SetContaminationReductionModifier(float reduction)
    {
        contaminationReductionModifier = Mathf.Clamp01(reduction);
        Debug.Log($"Contamination reduction modifier set to {contaminationReductionModifier:F2}");
    }

    /// <summary>
    /// Get current recycling multiplier
    /// </summary>
    public float GetRecyclingMultiplier()
    {
        return recyclingMultiplier;
    }

    /// <summary>
    /// Get current contamination reduction modifier
    /// </summary>
    public float GetContaminationReductionModifier()
    {
        return contaminationReductionModifier;
    }

    /// <summary>
    /// Get resource configuration for a specific resource type
    /// </summary>
    /// <param name="resourceType">Resource type to get config for</param>
    /// <returns>ResourceConfig or null if not found</returns>
    public ResourceConfig GetResourceConfig(ResourceType resourceType)
    {
        // Try to get from ResourceConfigManager first
        if (ResourceConfigManager.Instance != null)
        {
            return ResourceConfigManager.Instance.GetResourceConfig(resourceType);
        }
        
        // Fallback: check local configs
        return resourceConfigs.ContainsKey(resourceType) ? resourceConfigs[resourceType] : null;
    }

    /// <summary>
    /// Get storage limit for a specific resource type (alias for GetResourceLimit)
    /// </summary>
    /// <param name="resourceType">Resource type to check</param>
    /// <returns>Storage limit for the resource</returns>
    public int GetStorageLimit(ResourceType resourceType)
    {
        return GetResourceLimit(resourceType);
    }

    /// <summary>
    /// Get storage capacity for a specific resource type (alias for GetResourceLimit)
    /// </summary>
    /// <param name="resourceType">Resource type to check</param>
    /// <returns>Storage capacity for the resource</returns>
    public int GetStorageCapacity(ResourceType resourceType)
    {
        return GetResourceLimit(resourceType);
    }

    /// <summary>
    /// Check if resources can be added to inventory (considering storage limits)
    /// </summary>
    /// <param name="resourceType">Type of resource to add</param>
    /// <param name="amount">Amount to add</param>
    /// <returns>True if resources can be added</returns>
    public bool CanAddResource(ResourceType resourceType, int amount)
    {
        if (resourceType == ResourceType.None || amount <= 0) return false;
        
        int currentAmount = GetResourceAmount(resourceType);
        int storageLimit = GetStorageCapacity(resourceType);
        
        return currentAmount + amount <= storageLimit;
    }

    /// <summary>
    /// Check if resources can be spent from inventory
    /// </summary>
    /// <param name="resourceType">Type of resource to spend</param>
    /// <param name="amount">Amount to spend</param>
    /// <returns>True if resources can be spent</returns>
    public bool CanSpendResource(ResourceType resourceType, int amount)
    {
        if (resourceType == ResourceType.None || amount <= 0) return false;
        
        return GetResourceAmount(resourceType) >= amount;
    }

    #endregion

    private void OnDestroy()
    {
        SaveResourceData();
    }
}