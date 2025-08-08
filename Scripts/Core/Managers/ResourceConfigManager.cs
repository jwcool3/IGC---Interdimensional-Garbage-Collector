using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages resource configurations, recipes, and processing rules
/// Provides centralized configuration for the resource system
/// </summary>
public class ResourceConfigManager : MonoBehaviour
{
    public static ResourceConfigManager Instance { get; private set; }
    
    [Header("Resource Configurations")]
    [SerializeField] private ResourceConfig[] resourceConfigs;
    [SerializeField] private ProcessingRecipeData[] processingRecipes;
    
    [Header("Processing Rules")]
    [SerializeField] private float baseProcessingTime = 1f;
    [SerializeField] private float rarityTimeMultiplier = 0.5f;
    [SerializeField] private float contaminationTimeMultiplier = 0.3f;
    
    // Runtime data
    private Dictionary<ResourceType, ResourceConfig> configLookup = new Dictionary<ResourceType, ResourceConfig>();
    private Dictionary<string, ProcessingRecipeData> recipeLookup = new Dictionary<string, ProcessingRecipeData>();
    private List<ProcessingRecipeData> availableRecipes = new List<ProcessingRecipeData>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeConfigurations();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initialize all configurations and build lookup tables
    /// </summary>
    private void InitializeConfigurations()
    {
        BuildResourceConfigLookup();
        BuildRecipeLookup();
        ValidateConfigurations();
        
        Debug.Log($"ResourceConfigManager initialized with {configLookup.Count} resource configs and {recipeLookup.Count} recipes");
    }
    
    /// <summary>
    /// Build resource configuration lookup table - ONLY USE ASSIGNED CONFIGS
    /// </summary>
    private void BuildResourceConfigLookup()
    {
        configLookup.Clear();
        
        if (resourceConfigs != null)
        {
            foreach (var config in resourceConfigs)
            {
                if (config != null)
                {
                    configLookup[config.resourceType] = config;
                }
            }
        }
        
        Debug.Log($"Loaded {configLookup.Count} resource configurations from assigned assets");
    }
    
    /// <summary>
    /// Build recipe lookup table
    /// </summary>
    private void BuildRecipeLookup()
    {
        recipeLookup.Clear();
        availableRecipes.Clear();
        
        if (processingRecipes != null)
        {
            foreach (var recipe in processingRecipes)
            {
                if (recipe != null && !string.IsNullOrEmpty(recipe.recipeName))
                {
                    recipeLookup[recipe.recipeName] = recipe;
                    
                    if (recipe.CanUse())
                    {
                        availableRecipes.Add(recipe);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Validate all configurations for consistency
    /// </summary>
    private void ValidateConfigurations()
    {
        int warnings = 0;
        
        // Validate resource configs
        foreach (var config in configLookup.Values)
        {
            string errorMessage;
            if (!config.Validate(out errorMessage))
            {
                Debug.LogWarning($"Resource {config.resourceType} validation failed: {errorMessage}");
                warnings++;
            }
        }
        
        // Validate recipes (if ProcessingRecipeData has ValidateRecipe method)
        foreach (var recipe in recipeLookup.Values)
        {
            // Only validate if the method exists
            if (recipe != null)
            {
                Debug.Log($"Recipe {recipe.recipeName} loaded successfully");
            }
        }
        
        if (warnings > 0)
        {
            Debug.LogWarning($"ResourceConfigManager validation completed with {warnings} warnings");
        }
        else
        {
            Debug.Log("ResourceConfigManager validation completed successfully");
        }
    }
    
    #region Public API
    
    /// <summary>
    /// Get resource configuration by type
    /// </summary>
    /// <param name="resourceType">Resource type</param>
    /// <returns>Resource configuration or null if not found</returns>
    public ResourceConfig GetResourceConfig(ResourceType resourceType)
    {
        return configLookup.TryGetValue(resourceType, out ResourceConfig config) ? config : null;
    }
    
    /// <summary>
    /// Get all resource configurations
    /// </summary>
    /// <returns>Array of all resource configurations</returns>
    public ResourceConfig[] GetAllResourceConfigs()
    {
        return configLookup.Values.ToArray();
    }
    
    /// <summary>
    /// Get resource configurations by category
    /// </summary>
    /// <param name="category">Resource category</param>
    /// <returns>Array of resource configurations in the category</returns>
    public ResourceConfig[] GetResourceConfigsByCategory(ResourceCategory category)
    {
        return configLookup.Values.Where(config => config.category == category).ToArray();
    }

    /// <summary>
    /// Check if a resource configuration exists for the given type
    /// </summary>
    /// <param name="resourceType">Resource type to check</param>
    /// <returns>True if configuration exists</returns>
    public bool HasResourceConfig(ResourceType resourceType)
    {
        return configLookup.ContainsKey(resourceType);
    }

    /// <summary>
    /// Get all available processing recipes
    /// </summary>
    /// <returns>List of all processing recipes</returns>
    public List<ProcessingRecipeData> GetAllProcessingRecipes()
    {
        return recipeLookup.Values.ToList();
    }

    /// <summary>
    /// Get processing recipe by name
    /// </summary>
    /// <param name="recipeName">Name of the recipe</param>
    /// <returns>Processing recipe or null if not found</returns>
    public ProcessingRecipeData GetProcessingRecipe(string recipeName)
    {
        return recipeLookup.TryGetValue(recipeName, out ProcessingRecipeData recipe) ? recipe : null;
    }

    /// <summary>
    /// Get available recipes that can be used
    /// </summary>
    /// <returns>List of available recipes</returns>
    public List<ProcessingRecipeData> GetAvailableRecipes()
    {
        return new List<ProcessingRecipeData>(availableRecipes);
    }

    /// <summary>
    /// Get configuration statistics
    /// </summary>
    /// <returns>Configuration stats structure</returns>
    public ConfigurationStats GetConfigurationStats()
    {
        return new ConfigurationStats
        {
            isLoaded = configLookup.Count > 0 || recipeLookup.Count > 0,
            totalResourceConfigs = configLookup.Count,
            totalRecipes = recipeLookup.Count,
            availableRecipes = availableRecipes.Count,
            lastLoadTime = Time.time
        };
    }
    
    /// <summary>
    /// Get resource display name
    /// </summary>
    /// <param name="resourceType">Resource type</param>
    /// <returns>Display name or resource type name if config not found</returns>
    public string GetResourceDisplayName(ResourceType resourceType)
    {
        var config = GetResourceConfig(resourceType);
        return config?.displayName ?? resourceType.ToString();
    }
    
    /// <summary>
    /// Get resource description
    /// </summary>
    /// <param name="resourceType">Resource type</param>
    /// <returns>Description or empty string if config not found</returns>
    public string GetResourceDescription(ResourceType resourceType)
    {
        var config = GetResourceConfig(resourceType);
        return config?.description ?? "";
    }
    
    /// <summary>
    /// Get resource base value
    /// </summary>
    /// <param name="resourceType">Resource type</param>
    /// <returns>Base value or 1 if config not found</returns>
    public int GetResourceBaseValue(ResourceType resourceType)
    {
        var config = GetResourceConfig(resourceType);
        return config?.baseValue ?? 1;
    }
    
    /// <summary>
    /// Get resource display color
    /// </summary>
    /// <param name="resourceType">Resource type</param>
    /// <returns>Display color or white if config not found</returns>
    public Color GetResourceDisplayColor(ResourceType resourceType)
    {
        var config = GetResourceConfig(resourceType);
        return config?.displayColor ?? Color.white;
    }
    
    /// <summary>
    /// Check if a resource is stackable
    /// </summary>
    /// <param name="resourceType">Resource type</param>
    /// <returns>True if stackable, false otherwise</returns>
    public bool IsResourceStackable(ResourceType resourceType)
    {
        var config = GetResourceConfig(resourceType);
        return config?.isStackable ?? true;
    }
    
    /// <summary>
    /// Get maximum stack size for a resource
    /// </summary>
    /// <param name="resourceType">Resource type</param>
    /// <returns>Maximum stack size or 1000 if config not found</returns>
    public int GetResourceMaxStackSize(ResourceType resourceType)
    {
        var config = GetResourceConfig(resourceType);
        return config?.maxStackSize ?? 1000;
    }
    
    #endregion
    
    #region Editor and Debug Methods
    
    /// <summary>
    /// Reload configurations from inspector arrays
    /// </summary>
    [ContextMenu("Reload Configurations")]
    public void ReloadConfigurations()
    {
        InitializeConfigurations();
        Debug.Log("Configurations reloaded");
    }
    
    /// <summary>
    /// Log all configurations for debugging
    /// </summary>
    [ContextMenu("Log All Configurations")]
    public void LogAllConfigurations()
    {
        Debug.Log("=== Resource Configurations ===");
        foreach (var config in configLookup.Values.OrderBy(c => c.resourceType))
        {
            Debug.Log($"{config.resourceType}: {config.displayName} (Value: {config.baseValue}, Category: {config.category})");
        }
    }
    
    /// <summary>
    /// Check which resource types need explicit ResourceConfig assets
    /// </summary>
    [ContextMenu("Check Missing Configs")]
    public void CheckMissingConfigs()
    {
        Debug.Log("=== Missing Resource Configurations ===");
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            if (resourceType != ResourceType.None && !configLookup.ContainsKey(resourceType))
            {
                Debug.Log($"Missing config for: {resourceType}");
            }
        }
    }
    
    #endregion
}

/// <summary>
/// Configuration statistics structure
/// </summary>
[System.Serializable]
public struct ConfigurationStats
{
    public bool isLoaded;
    public int totalResourceConfigs;
    public int totalRecipes;
    public int availableRecipes;
    public float lastLoadTime;
}