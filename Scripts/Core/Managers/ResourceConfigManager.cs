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
    /// Build resource configuration lookup table
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
        
        // Add default configs for any missing resource types
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            if (resourceType != ResourceType.None && !configLookup.ContainsKey(resourceType))
            {
                configLookup[resourceType] = CreateDefaultResourceConfig(resourceType);
            }
        }
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
    /// Create a default resource configuration
    /// </summary>
    private ResourceConfig CreateDefaultResourceConfig(ResourceType resourceType)
    {
        return new ResourceConfig
        {
            resourceType = resourceType,
            displayName = resourceType.ToString(),
            description = $"Default configuration for {resourceType}",
            baseValue = 1,
            category = ResourceCategory.Basic,
            isStackable = true,
            maxStackSize = 1000,
            displayColor = Color.white
        };
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
            if (config.baseValue <= 0)
            {
                Debug.LogWarning($"Resource {config.resourceType} has invalid base value: {config.baseValue}");
                warnings++;
            }
            
            if (config.maxStackSize <= 0)
            {
                Debug.LogWarning($"Resource {config.resourceType} has invalid max stack size: {config.maxStackSize}");
                warnings++;
            }
        }
        
        // Validate recipes
        foreach (var recipe in recipeLookup.Values)
        {
            string errorMessage;
            if (!recipe.ValidateRecipe(out errorMessage))
            {
                Debug.LogWarning($"Recipe {recipe.recipeName} validation failed: {errorMessage}");
                warnings++;
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
    /// Get processing recipe by name
    /// </summary>
    /// <param name="recipeName">Recipe name</param>
    /// <returns>Processing recipe or null if not found</returns>
    public ProcessingRecipeData GetRecipe(string recipeName)
    {
        return recipeLookup.TryGetValue(recipeName, out ProcessingRecipeData recipe) ? recipe : null;
    }
    
    /// <summary>
    /// Get all available recipes
    /// </summary>
    /// <returns>List of available recipes</returns>
    public List<ProcessingRecipeData> GetAvailableRecipes()
    {
        return new List<ProcessingRecipeData>(availableRecipes);
    }
    
    /// <summary>
    /// Get recipes by category
    /// </summary>
    /// <param name="category">Recipe category</param>
    /// <returns>List of recipes in the category</returns>
    public List<ProcessingRecipeData> GetRecipesByCategory(RecipeCategory category)
    {
        return availableRecipes.Where(recipe => recipe.category == category).ToList();
    }
    
    /// <summary>
    /// Calculate processing time for a waste item
    /// </summary>
    /// <param name="wasteItem">Waste item to process</param>
    /// <param name="facilityEfficiency">Facility efficiency multiplier</param>
    /// <returns>Processing time in seconds</returns>
    public float CalculateProcessingTime(UpdatedWasteItem wasteItem, float facilityEfficiency = 1f)
    {
        if (wasteItem == null) return baseProcessingTime;
        
        float time = baseProcessingTime;
        
        // Apply rarity modifier
        time += (int)wasteItem.Rarity * rarityTimeMultiplier;
        
        // Apply contamination modifier
        time += wasteItem.ContaminationLevel * contaminationTimeMultiplier;
        
        // Apply condition modifier
        switch (wasteItem.Condition)
        {
            case WasteCondition.Pristine: time *= 0.8f; break;
            case WasteCondition.Good: time *= 1f; break;
            case WasteCondition.Damaged: time *= 1.2f; break;
            case WasteCondition.Deteriorated: time *= 1.5f; break;
            case WasteCondition.Corrupted: time *= 2f; break;
        }
        
        // Apply facility efficiency
        time /= facilityEfficiency;
        
        return Mathf.Max(0.1f, time);
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
    
    /// <summary>
    /// Refresh available recipes (call when unlocks change)
    /// </summary>
    public void RefreshAvailableRecipes()
    {
        availableRecipes.Clear();
        
        foreach (var recipe in recipeLookup.Values)
        {
            if (recipe.CanUse())
            {
                availableRecipes.Add(recipe);
            }
        }
        
        Debug.Log($"Refreshed available recipes: {availableRecipes.Count} available");
    }
    
    /// <summary>
    /// Add a new recipe at runtime
    /// </summary>
    /// <param name="recipe">Recipe to add</param>
    /// <returns>True if added successfully</returns>
    public bool AddRecipe(ProcessingRecipeData recipe)
    {
        if (recipe == null || string.IsNullOrEmpty(recipe.recipeName))
        {
            return false;
        }
        
        if (recipeLookup.ContainsKey(recipe.recipeName))
        {
            Debug.LogWarning($"Recipe {recipe.recipeName} already exists");
            return false;
        }
        
        recipeLookup[recipe.recipeName] = recipe;
        
        if (recipe.CanUse())
        {
            availableRecipes.Add(recipe);
        }
        
        Debug.Log($"Added recipe: {recipe.recipeName}");
        return true;
    }
    
    /// <summary>
    /// Remove a recipe at runtime
    /// </summary>
    /// <param name="recipeName">Name of recipe to remove</param>
    /// <returns>True if removed successfully</returns>
    public bool RemoveRecipe(string recipeName)
    {
        if (string.IsNullOrEmpty(recipeName) || !recipeLookup.ContainsKey(recipeName))
        {
            return false;
        }
        
        var recipe = recipeLookup[recipeName];
        recipeLookup.Remove(recipeName);
        availableRecipes.Remove(recipe);
        
        Debug.Log($"Removed recipe: {recipeName}");
        return true;
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
        
        Debug.Log("=== Processing Recipes ===");
        foreach (var recipe in recipeLookup.Values.OrderBy(r => r.recipeName))
        {
            Debug.Log($"{recipe.recipeName}: {recipe.category} (Available: {recipe.CanUse()})");
        }
    }
    
    /// <summary>
    /// Create default resource configurations for all resource types
    /// </summary>
    [ContextMenu("Create Default Configs")]
    public void CreateDefaultConfigs()
    {
        var configs = new List<ResourceConfig>();
        
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            if (resourceType != ResourceType.None)
            {
                configs.Add(CreateDefaultResourceConfig(resourceType));
            }
        }
        
        resourceConfigs = configs.ToArray();
        InitializeConfigurations();
        
        Debug.Log($"Created {configs.Count} default resource configurations");
    }
    
    #endregion
} 