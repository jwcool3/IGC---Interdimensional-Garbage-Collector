using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages all resource configurations, processing recipes, and system settings
/// Loads and caches ScriptableObject configurations for efficient access
/// </summary>
public class ResourceConfigManager : MonoBehaviour
{
    public static ResourceConfigManager Instance { get; private set; }
    
    [Header("Configuration Paths")]
    [SerializeField] private string resourceConfigPath = "ResourceConfigs";
    [SerializeField] private string processingRecipePath = "ProcessingRecipes";
    [SerializeField] private string systemSettingsPath = "SystemSettings";
    
    [Header("Loaded Configurations")]
    [SerializeField] private List<ResourceConfig> loadedResourceConfigs = new List<ResourceConfig>();
    [SerializeField] private List<ProcessingRecipeData> loadedProcessingRecipes = new List<ProcessingRecipeData>();
    
    // Cached lookups for performance
    private Dictionary<ResourceType, ResourceConfig> resourceConfigLookup = new Dictionary<ResourceType, ResourceConfig>();
    private Dictionary<string, ProcessingRecipeData> recipeNameLookup = new Dictionary<string, ProcessingRecipeData>();
    private Dictionary<ResourceType, List<ProcessingRecipeData>> recipesByInputType = new Dictionary<ResourceType, List<ProcessingRecipeData>>();
    private Dictionary<ResourceType, List<ProcessingRecipeData>> recipesByOutputType = new Dictionary<ResourceType, List<ProcessingRecipeData>>();
    
    [Header("System Settings")]
    [SerializeField] private bool autoLoadOnStart = true;
    [SerializeField] private bool validateConfigsOnLoad = true;
    [SerializeField] private bool logLoadingDetails = false;
    
    // Events
    public System.Action OnConfigurationsLoaded;
    public System.Action<string> OnConfigurationError;
    
    private void Awake()
    {
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
    
    private void Start()
    {
        if (autoLoadOnStart)
        {
            LoadAllConfigurations();
        }
    }
    
    /// <summary>
    /// Load all configurations from Resources folders
    /// </summary>
    public void LoadAllConfigurations()
    {
        Debug.Log("Loading resource configurations...");
        
        LoadResourceConfigs();
        LoadProcessingRecipes();
        BuildLookupTables();
        
        if (validateConfigsOnLoad)
        {
            ValidateConfigurations();
        }
        
        OnConfigurationsLoaded?.Invoke();
        Debug.Log($"Loaded {loadedResourceConfigs.Count} resource configs and {loadedProcessingRecipes.Count} processing recipes");
    }
    
    private void LoadResourceConfigs()
    {
        var configs = Resources.LoadAll<ResourceConfig>(resourceConfigPath);
        loadedResourceConfigs.Clear();
        loadedResourceConfigs.AddRange(configs);
        
        if (logLoadingDetails)
        {
            Debug.Log($"Loaded {configs.Length} resource configurations from {resourceConfigPath}");
        }
    }
    
    private void LoadProcessingRecipes()
    {
        var recipes = Resources.LoadAll<ProcessingRecipeData>(processingRecipePath);
        loadedProcessingRecipes.Clear();
        loadedProcessingRecipes.AddRange(recipes);
        
        if (logLoadingDetails)
        {
            Debug.Log($"Loaded {recipes.Length} processing recipes from {processingRecipePath}");
        }
    }
    
    private void BuildLookupTables()
    {
        // Build resource config lookup
        resourceConfigLookup.Clear();
        foreach (var config in loadedResourceConfigs)
        {
            if (config != null)
            {
                resourceConfigLookup[config.resourceType] = config;
            }
        }
        
        // Build recipe lookups
        recipeNameLookup.Clear();
        recipesByInputType.Clear();
        recipesByOutputType.Clear();
        
        foreach (var recipe in loadedProcessingRecipes)
        {
            if (recipe == null) continue;
            
            // Name lookup
            recipeNameLookup[recipe.recipeName] = recipe;
            
            // Input type lookup
            foreach (var input in recipe.requiredInputs)
            {
                if (!recipesByInputType.ContainsKey(input.resourceType))
                {
                    recipesByInputType[input.resourceType] = new List<ProcessingRecipeData>();
                }
                recipesByInputType[input.resourceType].Add(recipe);
            }
            
            // Output type lookup
            foreach (var output in recipe.guaranteedOutputs)
            {
                if (!recipesByOutputType.ContainsKey(output.resourceType))
                {
                    recipesByOutputType[output.resourceType] = new List<ProcessingRecipeData>();
                }
                recipesByOutputType[output.resourceType].Add(recipe);
            }
            
            foreach (var output in recipe.possibleOutputs)
            {
                if (!recipesByOutputType.ContainsKey(output.resourceType))
                {
                    recipesByOutputType[output.resourceType] = new List<ProcessingRecipeData>();
                }
                if (!recipesByOutputType[output.resourceType].Contains(recipe))
                {
                    recipesByOutputType[output.resourceType].Add(recipe);
                }
            }
        }
        
        if (logLoadingDetails)
        {
            Debug.Log($"Built lookup tables: {resourceConfigLookup.Count} resource configs, {recipeNameLookup.Count} recipes");
        }
    }
    
    private void ValidateConfigurations()
    {
        int errors = 0;
        
        // Validate resource configs
        foreach (var config in loadedResourceConfigs)
        {
            if (config == null)
            {
                errors++;
                OnConfigurationError?.Invoke("Found null resource config");
                continue;
            }
            
            if (config.baseValue <= 0)
            {
                errors++;
                OnConfigurationError?.Invoke($"Resource {config.resourceType} has invalid base value: {config.baseValue}");
            }
            
            if (string.IsNullOrEmpty(config.displayName))
            {
                errors++;
                OnConfigurationError?.Invoke($"Resource {config.resourceType} has empty display name");
            }
        }
        
        // Validate processing recipes
        foreach (var recipe in loadedProcessingRecipes)
        {
            if (recipe == null)
            {
                errors++;
                OnConfigurationError?.Invoke("Found null processing recipe");
                continue;
            }
            
            var validation = recipe.ValidateRecipe();
            if (!validation.isValid)
            {
                errors++;
                OnConfigurationError?.Invoke($"Recipe {recipe.recipeName} validation failed: {validation.errorMessage}");
            }
        }
        
        if (errors > 0)
        {
            Debug.LogWarning($"Configuration validation completed with {errors} errors");
        }
        else
        {
            Debug.Log("All configurations validated successfully");
        }
    }
    
    #region Resource Config Access
    
    /// <summary>
    /// Get resource configuration for a specific type
    /// </summary>
    /// <param name="resourceType">The resource type</param>
    /// <returns>Resource configuration or null if not found</returns>
    public ResourceConfig GetResourceConfig(ResourceType resourceType)
    {
        resourceConfigLookup.TryGetValue(resourceType, out ResourceConfig config);
        return config;
    }
    
    /// <summary>
    /// Get all loaded resource configurations
    /// </summary>
    /// <returns>List of all resource configurations</returns>
    public List<ResourceConfig> GetAllResourceConfigs()
    {
        return new List<ResourceConfig>(loadedResourceConfigs);
    }
    
    /// <summary>
    /// Check if a resource type has a configuration
    /// </summary>
    /// <param name="resourceType">The resource type to check</param>
    /// <returns>True if configuration exists</returns>
    public bool HasResourceConfig(ResourceType resourceType)
    {
        return resourceConfigLookup.ContainsKey(resourceType);
    }
    
    /// <summary>
    /// Get resource configurations by category
    /// </summary>
    /// <param name="category">The resource category</param>
    /// <returns>List of matching configurations</returns>
    public List<ResourceConfig> GetResourceConfigsByCategory(ResourceCategory category)
    {
        return loadedResourceConfigs.Where(config => config != null && config.category == category).ToList();
    }
    
    #endregion
    
    #region Processing Recipe Access
    
    /// <summary>
    /// Get processing recipe by name
    /// </summary>
    /// <param name="recipeName">Name of the recipe</param>
    /// <returns>Processing recipe or null if not found</returns>
    public ProcessingRecipeData GetProcessingRecipe(string recipeName)
    {
        recipeNameLookup.TryGetValue(recipeName, out ProcessingRecipeData recipe);
        return recipe;
    }
    
    /// <summary>
    /// Get all processing recipes that use a specific input resource
    /// </summary>
    /// <param name="inputType">The input resource type</param>
    /// <returns>List of matching recipes</returns>
    public List<ProcessingRecipeData> GetRecipesByInput(ResourceType inputType)
    {
        if (recipesByInputType.TryGetValue(inputType, out List<ProcessingRecipeData> recipes))
        {
            return new List<ProcessingRecipeData>(recipes);
        }
        return new List<ProcessingRecipeData>();
    }
    
    /// <summary>
    /// Get all processing recipes that produce a specific output resource
    /// </summary>
    /// <param name="outputType">The output resource type</param>
    /// <returns>List of matching recipes</returns>
    public List<ProcessingRecipeData> GetRecipesByOutput(ResourceType outputType)
    {
        if (recipesByOutputType.TryGetValue(outputType, out List<ProcessingRecipeData> recipes))
        {
            return new List<ProcessingRecipeData>(recipes);
        }
        return new List<ProcessingRecipeData>();
    }
    
    /// <summary>
    /// Get all loaded processing recipes
    /// </summary>
    /// <returns>List of all processing recipes</returns>
    public List<ProcessingRecipeData> GetAllProcessingRecipes()
    {
        return new List<ProcessingRecipeData>(loadedProcessingRecipes);
    }
    
    /// <summary>
    /// Find recipes that can be used with available resources
    /// </summary>
    /// <param name="availableResources">Dictionary of available resource amounts</param>
    /// <returns>List of usable recipes</returns>
    public List<ProcessingRecipeData> GetUsableRecipes(Dictionary<ResourceType, int> availableResources)
    {
        var usableRecipes = new List<ProcessingRecipeData>();
        
        foreach (var recipe in loadedProcessingRecipes)
        {
            if (recipe != null && recipe.CanUseRecipe(availableResources))
            {
                usableRecipes.Add(recipe);
            }
        }
        
        return usableRecipes;
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Get the display name for a resource type
    /// </summary>
    /// <param name="resourceType">The resource type</param>
    /// <returns>Display name or resource type name if config not found</returns>
    public string GetResourceDisplayName(ResourceType resourceType)
    {
        var config = GetResourceConfig(resourceType);
        return config?.displayName ?? resourceType.ToString();
    }
    
    /// <summary>
    /// Get the description for a resource type
    /// </summary>
    /// <param name="resourceType">The resource type</param>
    /// <returns>Description or empty string if config not found</returns>
    public string GetResourceDescription(ResourceType resourceType)
    {
        var config = GetResourceConfig(resourceType);
        return config?.description ?? "";
    }
    
    /// <summary>
    /// Get the base value for a resource type
    /// </summary>
    /// <param name="resourceType">The resource type</param>
    /// <returns>Base value or 1 if config not found</returns>
    public int GetResourceBaseValue(ResourceType resourceType)
    {
        var config = GetResourceConfig(resourceType);
        return config?.baseValue ?? 1;
    }
    
    /// <summary>
    /// Get the icon for a resource type
    /// </summary>
    /// <param name="resourceType">The resource type</param>
    /// <returns>Icon sprite or null if config not found</returns>
    public Sprite GetResourceIcon(ResourceType resourceType)
    {
        var config = GetResourceConfig(resourceType);
        return config?.icon;
    }
    
    /// <summary>
    /// Check if a resource type is stackable
    /// </summary>
    /// <param name="resourceType">The resource type</param>
    /// <returns>True if stackable, false otherwise</returns>
    public bool IsResourceStackable(ResourceType resourceType)
    {
        var config = GetResourceConfig(resourceType);
        return config?.isStackable ?? true;
    }
    
    /// <summary>
    /// Get the maximum stack size for a resource type
    /// </summary>
    /// <param name="resourceType">The resource type</param>
    /// <returns>Maximum stack size or 999 if config not found</returns>
    public int GetResourceMaxStackSize(ResourceType resourceType)
    {
        var config = GetResourceConfig(resourceType);
        return config?.maxStackSize ?? 999;
    }
    
    /// <summary>
    /// Reload all configurations from disk
    /// </summary>
    public void ReloadConfigurations()
    {
        Debug.Log("Reloading all configurations...");
        LoadAllConfigurations();
    }
    
    /// <summary>
    /// Get configuration loading statistics
    /// </summary>
    /// <returns>Configuration statistics</returns>
    public ConfigurationStats GetConfigurationStats()
    {
        return new ConfigurationStats
        {
            totalResourceConfigs = loadedResourceConfigs.Count,
            totalProcessingRecipes = loadedProcessingRecipes.Count,
            resourceTypesWithConfigs = resourceConfigLookup.Count,
            recipesWithInputLookups = recipesByInputType.Count,
            recipesWithOutputLookups = recipesByOutputType.Count,
            isLoaded = loadedResourceConfigs.Count > 0 || loadedProcessingRecipes.Count > 0
        };
    }
    
    #endregion
    
    #region Editor Support
    
#if UNITY_EDITOR
    /// <summary>
    /// Create a new resource configuration asset
    /// </summary>
    /// <param name="resourceType">The resource type</param>
    /// <param name="displayName">Display name for the resource</param>
    /// <returns>Created resource configuration</returns>
    public ResourceConfig CreateResourceConfig(ResourceType resourceType, string displayName)
    {
        var config = ScriptableObject.CreateInstance<ResourceConfig>();
        config.resourceType = resourceType;
        config.displayName = displayName;
        config.description = $"Configuration for {displayName}";
        config.baseValue = 1;
        config.isStackable = true;
        config.maxStackSize = 999;
        
        string path = $"Assets/Resources/{resourceConfigPath}/{resourceType}Config.asset";
        UnityEditor.AssetDatabase.CreateAsset(config, path);
        UnityEditor.AssetDatabase.SaveAssets();
        
        return config;
    }
    
    /// <summary>
    /// Create a new processing recipe asset
    /// </summary>
    /// <param name="recipeName">Name of the recipe</param>
    /// <returns>Created processing recipe</returns>
    public ProcessingRecipeData CreateProcessingRecipe(string recipeName)
    {
        var recipe = ScriptableObject.CreateInstance<ProcessingRecipeData>();
        recipe.recipeName = recipeName;
        recipe.description = $"Processing recipe for {recipeName}";
        recipe.processingTime = 5f;
        recipe.energyCost = 10;
        
        string path = $"Assets/Resources/{processingRecipePath}/{recipeName}Recipe.asset";
        UnityEditor.AssetDatabase.CreateAsset(recipe, path);
        UnityEditor.AssetDatabase.SaveAssets();
        
        return recipe;
    }
#endif
    
    #endregion
}

/// <summary>
/// Configuration loading statistics
/// </summary>
[System.Serializable]
public class ConfigurationStats
{
    public int totalResourceConfigs;
    public int totalProcessingRecipes;
    public int resourceTypesWithConfigs;
    public int recipesWithInputLookups;
    public int recipesWithOutputLookups;
    public bool isLoaded;
    
    public override string ToString()
    {
        return $"Configuration Stats:\n" +
               $"Resource Configs: {totalResourceConfigs}\n" +
               $"Processing Recipes: {totalProcessingRecipes}\n" +
               $"Resource Types with Configs: {resourceTypesWithConfigs}\n" +
               $"Loaded: {isLoaded}";
    }
} 