using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }
    
    [Header("Crafting Settings")]
    [SerializeField] private List<CraftingRecipeData> availableRecipes = new List<CraftingRecipeData>();
    [SerializeField] private bool enableBatchCrafting = true;
    [SerializeField] private int maxQueueSize = 20;
    [SerializeField] private bool enableDebugLogging = true;
    
    // Active crafting operations
    private Dictionary<ProcessingFacilityBase, CraftingJob> activeCraftingJobs = new Dictionary<ProcessingFacilityBase, CraftingJob>();
    private Queue<CraftingJob> craftingQueue = new Queue<CraftingJob>();
    
    // Events
    public event Action<CraftingJob> OnCraftingStarted;
    public event Action<CraftingJob, bool> OnCraftingCompleted; // job, success
    public event Action<CraftingJob> OnCraftingFailed;
    public event Action<CraftingRecipeData> OnRecipeUnlocked;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadRecipes();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Initialize after other managers are ready
        InitializeCraftingSystem();
    }
    
    private void InitializeCraftingSystem()
    {
        DebugManager.Log("CraftingManager initialized", DebugCategory.ResourceSystem);
    }
    
    private void LoadRecipes()
    {
        // Load recipes from Resources folder
        var loadedRecipes = Resources.LoadAll<CraftingRecipeData>("CraftingRecipes");
        if (loadedRecipes != null && loadedRecipes.Length > 0)
        {
            availableRecipes.AddRange(loadedRecipes);
            DebugManager.Log($"Loaded {loadedRecipes.Length} crafting recipes from Resources", DebugCategory.ResourceSystem);
        }
        else
        {
            DebugManager.LogWarning("No crafting recipes found in Resources/CraftingRecipes folder", DebugCategory.ResourceSystem);
        }
        
        DebugManager.Log($"Total available recipes: {availableRecipes.Count}", DebugCategory.ResourceSystem);
    }
    
    /// <summary>
    /// Start crafting a recipe at a specific facility
    /// </summary>
    public bool StartCrafting(CraftingRecipeData recipe, ProcessingFacilityBase facility, int batchSize = 1)
    {
        if (recipe == null || facility == null) 
        {
            DebugManager.LogWarning("Cannot start crafting: recipe or facility is null", DebugCategory.ResourceSystem);
            return false;
        }
        
        // Check if recipe is unlocked
        if (!recipe.IsRecipeUnlocked())
        {
            DebugManager.LogWarning($"Recipe {recipe.recipeName} is not unlocked", DebugCategory.ResourceSystem);
            return false;
        }
        
        // Check if facility can handle this recipe
        if (!CanFacilityHandleRecipe(facility, recipe)) 
        {
            DebugManager.LogWarning($"Facility {facility.FacilityName} cannot handle recipe {recipe.recipeName}", DebugCategory.ResourceSystem);
            return false;
        }
        
        // Check if facility is available
        if (activeCraftingJobs.ContainsKey(facility)) 
        {
            DebugManager.LogWarning($"Facility {facility.FacilityName} is already busy", DebugCategory.ResourceSystem);
            return false;
        }
        
        // Validate batch size
        if (batchSize > recipe.maxBatchSize || !recipe.canBatchProcess && batchSize > 1)
        {
            DebugManager.LogWarning($"Invalid batch size {batchSize} for recipe {recipe.recipeName}", DebugCategory.ResourceSystem);
            return false;
        }
        
        // Check if player has ingredients
        if (!recipe.CanCraft(batchSize)) 
        {
            DebugManager.LogWarning($"Insufficient ingredients for {batchSize}x {recipe.recipeName}", DebugCategory.ResourceSystem);
            return false;
        }
        
        // Consume ingredients
        if (!ConsumeIngredients(recipe, batchSize)) 
        {
            DebugManager.LogError($"Failed to consume ingredients for {recipe.recipeName}", DebugCategory.ResourceSystem);
            return false;
        }
        
        // Create crafting job
        var job = new CraftingJob
        {
            recipe = recipe,
            facility = facility,
            batchSize = batchSize,
            startTime = Time.time,
            duration = recipe.GetProcessingTime(facility, batchSize),
            successChance = recipe.GetSuccessChance(facility)
        };
        
        // Start the job
        activeCraftingJobs[facility] = job;
        StartCoroutine(ProcessCraftingJob(job));
        
        OnCraftingStarted?.Invoke(job);
        DebugManager.Log($"Started crafting {batchSize}x {recipe.recipeName} at {facility.FacilityName}", DebugCategory.ResourceSystem);
        
        return true;
    }
    
    private IEnumerator ProcessCraftingJob(CraftingJob job)
    {
        yield return new WaitForSeconds(job.duration);
        
        // Roll for success
        bool success = UnityEngine.Random.value <= job.successChance;
        
        if (success)
        {
            // Roll for critical success
            bool critical = UnityEngine.Random.value <= job.recipe.criticalSuccessChance;
            int outputAmount = job.recipe.outputAmount * job.batchSize;
            
            if (critical)
            {
                outputAmount += job.recipe.criticalSuccessBonus * job.batchSize;
                DebugManager.Log($"Critical success! Bonus output: +{job.recipe.criticalSuccessBonus * job.batchSize}", DebugCategory.ResourceSystem);
            }
            
            // Add output to inventory
            bool addedSuccessfully = CraftedItemInventory.Instance.AddCraftedItem(job.recipe.outputItemType, outputAmount);
            
            if (!addedSuccessfully)
            {
                DebugManager.LogWarning($"Failed to add crafted items to inventory: {outputAmount}x {job.recipe.outputItemType}", DebugCategory.ResourceSystem);
                // Could implement overflow handling here
            }
            
            DebugManager.Log($"Successfully crafted {outputAmount}x {job.recipe.outputItemType}", DebugCategory.ResourceSystem);
            OnCraftingCompleted?.Invoke(job, true);
        }
        else
        {
            DebugManager.Log($"Crafting failed for {job.recipe.recipeName}", DebugCategory.ResourceSystem);
            
            // Refund ingredients if specified
            if (job.recipe.refundsIngredientsOnFailure)
            {
                RefundIngredients(job.recipe, job.batchSize);
                DebugManager.Log($"Refunded ingredients for failed {job.recipe.recipeName}", DebugCategory.ResourceSystem);
            }
            
            OnCraftingFailed?.Invoke(job);
            OnCraftingCompleted?.Invoke(job, false);
        }
        
        // Remove from active jobs
        activeCraftingJobs.Remove(job.facility);
    }
    
    private bool ConsumeIngredients(CraftingRecipeData recipe, int batchSize)
    {
        // First check if we have everything (validation pass)
        foreach (var ingredient in recipe.requiredIngredients)
        {
            if (ingredient.isOptional) continue; // Skip optional ingredients in validation
            
            int required = ingredient.amount * batchSize;
            
            if (ingredient.isCraftedItem)
            {
                if (!CraftedItemInventory.Instance.HasCraftedItem(ingredient.resourceType, required))
                    return false;
            }
            else
            {
                if (!ResourceManager.Instance.HasResource(ingredient.resourceType, required))
                    return false;
            }
        }
        
        // Actually consume the ingredients
        foreach (var ingredient in recipe.requiredIngredients)
        {
            int required = ingredient.amount * batchSize;
            
            // Optional ingredients are consumed if available (for success bonus)
            if (ingredient.isOptional)
            {
                if (ingredient.isCraftedItem)
                {
                    CraftedItemInventory.Instance.RemoveCraftedItem(ingredient.resourceType, required);
                }
                else
                {
                    ResourceManager.Instance.SpendResource(ingredient.resourceType, required);
                }
            }
            else
            {
                // Required ingredients must be consumed successfully
                if (ingredient.isCraftedItem)
                {
                    if (!CraftedItemInventory.Instance.RemoveCraftedItem(ingredient.resourceType, required))
                        return false;
                }
                else
                {
                    if (!ResourceManager.Instance.SpendResource(ingredient.resourceType, required))
                        return false;
                }
            }
        }
        return true;
    }
    
    private void RefundIngredients(CraftingRecipeData recipe, int batchSize)
    {
        foreach (var ingredient in recipe.requiredIngredients)
        {
            int refundAmount = ingredient.amount * batchSize;
            
            if (ingredient.isCraftedItem)
            {
                CraftedItemInventory.Instance.AddCraftedItem(ingredient.resourceType, refundAmount);
            }
            else
            {
                ResourceManager.Instance.AddResource(ingredient.resourceType, refundAmount);
            }
        }
    }
    
    private bool CanFacilityHandleRecipe(ProcessingFacilityBase facility, CraftingRecipeData recipe)
    {
        // Check if facility is available
        if (!facility.IsOperational || facility.AvailableSlots <= 0) return false;
        
        // Check facility type
        if (facility.FacilityType != recipe.requiredFacilityType) return false;
        
        // Check facility level
        if (facility.Level < recipe.minimumFacilityLevel) return false;
        
        // Check complexity
        if (recipe.recipeComplexity > GetFacilityMaxComplexity(facility)) return false;
        
        return true;
    }
    
    private int GetFacilityMaxComplexity(ProcessingFacilityBase facility)
    {
        // Different facilities handle different complexity levels
        return facility.FacilityType switch
        {
            ProcessingType.Recycling => 2, // Simple recipes only
            ProcessingType.Fabrication => 4, // Most recipes
            ProcessingType.Compaction => 3, // Medium complexity
            ProcessingType.Synthesis => 5, // All recipes
            _ => 1
        };
    }
    
    /// <summary>
    /// Get all recipes available to the player
    /// </summary>
    public List<CraftingRecipeData> GetAvailableRecipes()
    {
        var available = new List<CraftingRecipeData>();
        
        foreach (var recipe in availableRecipes)
        {
            if (recipe.IsRecipeUnlocked())
            {
                available.Add(recipe);
            }
        }
        
        return available;
    }
    
    /// <summary>
    /// Get recipes that can be crafted with current resources
    /// </summary>
    public List<CraftingRecipeData> GetCraftableRecipes()
    {
        var craftable = new List<CraftingRecipeData>();
        
        foreach (var recipe in availableRecipes)
        {
            if (recipe.IsRecipeUnlocked() && recipe.CanCraft())
            {
                craftable.Add(recipe);
            }
        }
        
        return craftable;
    }
    
    /// <summary>
    /// Get all active crafting jobs
    /// </summary>
    public List<CraftingJob> GetActiveCraftingJobs()
    {
        return new List<CraftingJob>(activeCraftingJobs.Values);
    }
    
    /// <summary>
    /// Check if a facility is currently crafting
    /// </summary>
    public bool IsFacilityCrafting(ProcessingFacilityBase facility)
    {
        return activeCraftingJobs.ContainsKey(facility);
    }
    
    /// <summary>
    /// Get the crafting job for a specific facility
    /// </summary>
    public CraftingJob GetCraftingJob(ProcessingFacilityBase facility)
    {
        return activeCraftingJobs.TryGetValue(facility, out CraftingJob job) ? job : null;
    }
    
    /// <summary>
    /// Cancel a crafting job at a facility
    /// </summary>
    public bool CancelCrafting(ProcessingFacilityBase facility, bool refundIngredients = true)
    {
        if (!activeCraftingJobs.TryGetValue(facility, out CraftingJob job))
            return false;
        
        // Stop the coroutine and remove the job
        StopCoroutine(ProcessCraftingJob(job));
        activeCraftingJobs.Remove(facility);
        
        // Refund ingredients if requested
        if (refundIngredients)
        {
            RefundIngredients(job.recipe, job.batchSize);
            DebugManager.Log($"Cancelled crafting and refunded ingredients for {job.recipe.recipeName}", DebugCategory.ResourceSystem);
        }
        
        return true;
    }
}

[System.Serializable]
public class CraftingJob
{
    public CraftingRecipeData recipe;
    public ProcessingFacilityBase facility;
    public int batchSize;
    public float startTime;
    public float duration;
    public float successChance;
    
    public float Progress => Mathf.Clamp01((Time.time - startTime) / duration);
    public bool IsComplete => Progress >= 1.0f;
    public float TimeRemaining => Mathf.Max(0f, duration - (Time.time - startTime));
} 