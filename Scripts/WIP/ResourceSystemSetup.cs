using UnityEngine;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Utility class for setting up the resource management system
/// Provides easy setup methods and editor tools
/// </summary>
public static class ResourceSystemSetup
{
    private const string RESOURCE_CONFIGS_PATH = "Assets/Resources/ResourceConfigs";
    private const string PROCESSING_RECIPES_PATH = "Assets/Resources/ProcessingRecipes";
    private const string PREFABS_PATH = "Assets/Prefabs/ResourceSystem";
    
    /// <summary>
    /// Setup the complete resource system in the current scene
    /// </summary>
    public static GameObject SetupResourceSystemInScene()
    {
        // Create main system coordinator
        GameObject systemRoot = new GameObject("ResourceSystem");
        var coordinator = systemRoot.AddComponent<ResourceSystemCoordinator>();
        
        // Create child objects for organization
        GameObject managers = new GameObject("Managers");
        managers.transform.SetParent(systemRoot.transform);
        
        GameObject bridges = new GameObject("Bridges");
        bridges.transform.SetParent(systemRoot.transform);
        
        GameObject utilities = new GameObject("Utilities");
        utilities.transform.SetParent(systemRoot.transform);
        
        // Add core managers
        SetupCoreManagers(managers);
        
        // Add bridge components
        SetupBridgeComponents(bridges);
        
        // Add utility components
        SetupUtilityComponents(utilities);
        
        Debug.Log("Resource system setup complete!");
        return systemRoot;
    }
    
    private static void SetupCoreManagers(GameObject parent)
    {
        // New Resource Manager
        GameObject resourceManagerGO = new GameObject("NewResourceManager");
        resourceManagerGO.transform.SetParent(parent.transform);
        resourceManagerGO.AddComponent<NewResourceManager>();
        
        // Resource Config Manager
        GameObject configManagerGO = new GameObject("ResourceConfigManager");
        configManagerGO.transform.SetParent(parent.transform);
        configManagerGO.AddComponent<ResourceConfigManager>();
        
        // Updated Waste Processor
        GameObject processorGO = new GameObject("UpdatedWasteProcessor");
        processorGO.transform.SetParent(parent.transform);
        processorGO.AddComponent<UpdatedWasteProcessor>();
    }
    
    private static void SetupBridgeComponents(GameObject parent)
    {
        // Resource Manager Bridge
        GameObject bridgeGO = new GameObject("ResourceManagerBridge");
        bridgeGO.transform.SetParent(parent.transform);
        bridgeGO.AddComponent<ResourceManagerBridge>();
        
        // Resource Event Bridge
        GameObject eventBridgeGO = new GameObject("ResourceEventBridge");
        eventBridgeGO.transform.SetParent(parent.transform);
        eventBridgeGO.AddComponent<ResourceEventBridge>();
        
        // Legacy System Integrator
        GameObject integratorGO = new GameObject("LegacySystemIntegrator");
        integratorGO.transform.SetParent(parent.transform);
        integratorGO.AddComponent<LegacySystemIntegrator>();
    }
    
    private static void SetupUtilityComponents(GameObject parent)
    {
        // Resource Display UI (if needed)
        GameObject displayGO = new GameObject("ResourceDisplayUI");
        displayGO.transform.SetParent(parent.transform);
        displayGO.AddComponent<ResourceDisplayUI>();
    }
    
    /// <summary>
    /// Create default resource configurations
    /// </summary>
    public static void CreateDefaultResourceConfigs()
    {
        #if UNITY_EDITOR
        // Ensure directories exist
        EnsureDirectoryExists(RESOURCE_CONFIGS_PATH);
        
        // Create basic resource configs
        CreateResourceConfig("Plastic", ResourceType.Plastic, "Common recyclable plastic materials");
        CreateResourceConfig("MetalScraps", ResourceType.MetalScraps, "Various metal scraps and components");
        CreateResourceConfig("OrganicMatter", ResourceType.OrganicMatter, "Biodegradable organic waste");
        CreateResourceConfig("ElectronicWaste", ResourceType.ElectronicWaste, "Electronic components and circuits");
        CreateResourceConfig("ToxicWaste", ResourceType.ToxicWaste, "Hazardous toxic materials");
        CreateResourceConfig("RareMetals", ResourceType.RareMetals, "Valuable rare metal elements");
        CreateResourceConfig("Crystals", ResourceType.Crystals, "Crystalline energy sources");
        CreateResourceConfig("Biomass", ResourceType.Biomass, "Processed organic biomass");
        CreateResourceConfig("Nanomaterials", ResourceType.Nanomaterials, "Advanced nanotechnology materials");
        CreateResourceConfig("QuantumMatter", ResourceType.QuantumMatter, "Exotic quantum materials");
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Default resource configurations created!");
        #endif
    }
    
    /// <summary>
    /// Create default processing recipes
    /// </summary>
    public static void CreateDefaultProcessingRecipes()
    {
        #if UNITY_EDITOR
        // Ensure directories exist
        EnsureDirectoryExists(PROCESSING_RECIPES_PATH);
        
        // Create basic processing recipes
        CreateBasicRecyclingRecipes();
        CreateAdvancedProcessingRecipes();
        CreateSpecialtyRecipes();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Default processing recipes created!");
        #endif
    }
    
    #if UNITY_EDITOR
    private static void CreateResourceConfig(string name, ResourceType type, string description)
    {
        var config = ScriptableObject.CreateInstance<ResourceConfig>();
        config.resourceType = type;
        config.displayName = name;
        config.description = description;
        config.baseValue = GetDefaultBaseValue(type);
        config.rarity = GetDefaultRarity(type);
        config.category = GetDefaultCategory(type);
        config.isStackable = true;
        config.maxStackSize = 1000;
        
        string path = $"{RESOURCE_CONFIGS_PATH}/{name}Config.asset";
        AssetDatabase.CreateAsset(config, path);
    }
    
    private static void CreateBasicRecyclingRecipes()
    {
        // Plastic recycling
        CreateProcessingRecipe("PlasticRecycling", "Basic Plastic Recycling",
            new ResourceAmount[] { new ResourceAmount(ResourceType.Plastic, 10) },
            new ResourceAmount[] { new ResourceAmount(ResourceType.Plastic, 8) },
            2f, ProcessingType.Recycling);
        
        // Metal recycling
        CreateProcessingRecipe("MetalRecycling", "Basic Metal Recycling",
            new ResourceAmount[] { new ResourceAmount(ResourceType.MetalScraps, 5) },
            new ResourceAmount[] { new ResourceAmount(ResourceType.MetalScraps, 4) },
            3f, ProcessingType.Recycling);
        
        // Organic composting
        CreateProcessingRecipe("OrganicComposting", "Organic Matter Composting",
            new ResourceAmount[] { new ResourceAmount(ResourceType.OrganicMatter, 15) },
            new ResourceAmount[] { new ResourceAmount(ResourceType.Biomass, 5) },
            5f, ProcessingType.Composting);
    }
    
    private static void CreateAdvancedProcessingRecipes()
    {
        // Electronic breakdown
        CreateProcessingRecipe("ElectronicBreakdown", "Electronic Waste Processing",
            new ResourceAmount[] { new ResourceAmount(ResourceType.ElectronicWaste, 3) },
            new ResourceAmount[] { 
                new ResourceAmount(ResourceType.MetalScraps, 2),
                new ResourceAmount(ResourceType.RareMetals, 1)
            },
            8f, ProcessingType.Breakdown);
        
        // Crystal synthesis
        CreateProcessingRecipe("CrystalSynthesis", "Crystal Energy Synthesis",
            new ResourceAmount[] { 
                new ResourceAmount(ResourceType.RareMetals, 2),
                new ResourceAmount(ResourceType.Biomass, 3)
            },
            new ResourceAmount[] { new ResourceAmount(ResourceType.Crystals, 1) },
            12f, ProcessingType.Synthesis);
    }
    
    private static void CreateSpecialtyRecipes()
    {
        // Quantum matter creation
        CreateProcessingRecipe("QuantumCreation", "Quantum Matter Creation",
            new ResourceAmount[] { 
                new ResourceAmount(ResourceType.Crystals, 5),
                new ResourceAmount(ResourceType.Nanomaterials, 3)
            },
            new ResourceAmount[] { new ResourceAmount(ResourceType.QuantumMatter, 1) },
            20f, ProcessingType.Synthesis);
        
        // Toxic waste neutralization
        CreateProcessingRecipe("ToxicNeutralization", "Toxic Waste Neutralization",
            new ResourceAmount[] { new ResourceAmount(ResourceType.ToxicWaste, 1) },
            new ResourceAmount[] { new ResourceAmount(ResourceType.OrganicMatter, 2) },
            15f, ProcessingType.Neutralization);
    }
    
    private static void CreateProcessingRecipe(string fileName, string recipeName, 
        ResourceAmount[] inputs, ResourceAmount[] outputs, float processingTime, ProcessingType type)
    {
        var recipe = ScriptableObject.CreateInstance<ProcessingRecipeData>();
        recipe.recipeName = recipeName;
        recipe.description = $"Processes {string.Join(", ", System.Array.ConvertAll(inputs, i => i.type.ToString()))}";
        recipe.processingTime = processingTime;
        recipe.processingType = type;
        recipe.requiredInputs = new List<ResourceAmount>(inputs);
        recipe.guaranteedOutputs = new List<ResourceAmount>(outputs);
        recipe.isEnabled = true;
        recipe.requiredFacilityLevel = 1;
        
        string path = $"{PROCESSING_RECIPES_PATH}/{fileName}.asset";
        AssetDatabase.CreateAsset(recipe, path);
    }
    
    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
    
    private static float GetDefaultBaseValue(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Plastic: return 1f;
            case ResourceType.MetalScraps: return 2f;
            case ResourceType.OrganicMatter: return 0.5f;
            case ResourceType.ElectronicWaste: return 5f;
            case ResourceType.ToxicWaste: return 10f;
            case ResourceType.RareMetals: return 15f;
            case ResourceType.Crystals: return 25f;
            case ResourceType.Biomass: return 3f;
            case ResourceType.Nanomaterials: return 50f;
            case ResourceType.QuantumMatter: return 100f;
            default: return 1f;
        }
    }
    
    private static ResourceRarity GetDefaultRarity(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Plastic:
            case ResourceType.MetalScraps:
            case ResourceType.OrganicMatter:
                return ResourceRarity.Common;
            case ResourceType.ElectronicWaste:
            case ResourceType.Biomass:
                return ResourceRarity.Uncommon;
            case ResourceType.ToxicWaste:
            case ResourceType.RareMetals:
                return ResourceRarity.Rare;
            case ResourceType.Crystals:
            case ResourceType.Nanomaterials:
                return ResourceRarity.Epic;
            case ResourceType.QuantumMatter:
                return ResourceRarity.Legendary;
            default:
                return ResourceRarity.Common;
        }
    }
    
    private static ResourceCategory GetDefaultCategory(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Plastic:
            case ResourceType.MetalScraps:
            case ResourceType.ElectronicWaste:
                return ResourceCategory.RawMaterial;
            case ResourceType.OrganicMatter:
            case ResourceType.ToxicWaste:
                return ResourceCategory.Waste;
            case ResourceType.RareMetals:
            case ResourceType.Crystals:
            case ResourceType.Nanomaterials:
            case ResourceType.QuantumMatter:
                return ResourceCategory.Refined;
            case ResourceType.Biomass:
                return ResourceCategory.Processed;
            default:
                return ResourceCategory.RawMaterial;
        }
    }
    #endif
    
    /// <summary>
    /// Validate the current resource system setup
    /// </summary>
    public static SystemValidationReport ValidateSetup()
    {
        return SystemValidationUtility.ValidateCompleteSystem();
    }
    
    /// <summary>
    /// Get setup recommendations based on current state
    /// </summary>
    public static List<string> GetSetupRecommendations()
    {
        var recommendations = new List<string>();
        
        // Check for coordinator
        if (ResourceSystemCoordinator.Instance == null)
        {
            recommendations.Add("Create ResourceSystemCoordinator in scene");
        }
        
        // Check for core managers
        if (NewResourceManager.Instance == null)
        {
            recommendations.Add("Add NewResourceManager to scene");
        }
        
        if (ResourceConfigManager.Instance == null)
        {
            recommendations.Add("Add ResourceConfigManager to scene");
        }
        
        // Check for bridge components
        if (ResourceManagerBridge.Instance == null)
        {
            recommendations.Add("Add ResourceManagerBridge for legacy compatibility");
        }
        
        if (ResourceEventBridge.Instance == null)
        {
            recommendations.Add("Add ResourceEventBridge for event translation");
        }
        
        #if UNITY_EDITOR
        // Check for configurations
        var configs = Resources.LoadAll<ResourceConfig>("");
        if (configs.Length == 0)
        {
            recommendations.Add("Create default resource configurations");
        }
        
        var recipes = Resources.LoadAll<ProcessingRecipeData>("");
        if (recipes.Length == 0)
        {
            recommendations.Add("Create default processing recipes");
        }
        #endif
        
        return recommendations;
    }
    
    /// <summary>
    /// Auto-setup the resource system with default configurations
    /// </summary>
    public static void AutoSetup()
    {
        Debug.Log("Starting auto-setup of resource system...");
        
        // Setup scene components
        SetupResourceSystemInScene();
        
        #if UNITY_EDITOR
        // Create default configurations
        CreateDefaultResourceConfigs();
        CreateDefaultProcessingRecipes();
        #endif
        
        // Validate setup
        var report = ValidateSetup();
        
        if (report.overallStatus == ValidationStatus.Passed)
        {
            Debug.Log("Resource system auto-setup completed successfully!");
        }
        else
        {
            Debug.LogWarning($"Resource system setup completed with {report.TotalIssues} issues. Check validation report for details.");
        }
    }
}

#if UNITY_EDITOR
/// <summary>
/// Editor menu items for resource system setup
/// </summary>
public static class ResourceSystemSetupMenu
{
    [MenuItem("Tools/Resource System/Auto Setup")]
    public static void AutoSetup()
    {
        ResourceSystemSetup.AutoSetup();
    }
    
    [MenuItem("Tools/Resource System/Setup Scene Components")]
    public static void SetupSceneComponents()
    {
        ResourceSystemSetup.SetupResourceSystemInScene();
    }
    
    [MenuItem("Tools/Resource System/Create Default Configs")]
    public static void CreateDefaultConfigs()
    {
        ResourceSystemSetup.CreateDefaultResourceConfigs();
    }
    
    [MenuItem("Tools/Resource System/Create Default Recipes")]
    public static void CreateDefaultRecipes()
    {
        ResourceSystemSetup.CreateDefaultProcessingRecipes();
    }
    
    [MenuItem("Tools/Resource System/Validate Setup")]
    public static void ValidateSetup()
    {
        var report = ResourceSystemSetup.ValidateSetup();
        Debug.Log($"Validation Report:\n{report}");
    }
    
    [MenuItem("Tools/Resource System/Get Recommendations")]
    public static void GetRecommendations()
    {
        var recommendations = ResourceSystemSetup.GetSetupRecommendations();
        
        if (recommendations.Count == 0)
        {
            Debug.Log("Resource system setup is complete!");
        }
        else
        {
            Debug.Log("Setup Recommendations:\n" + string.Join("\n- ", recommendations));
        }
    }
}
#endif 