using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Advanced Fabricator Facility - Processes recipes to create complex materials and components
/// Specializes in recipe-based processing and advanced material synthesis
/// </summary>
public class AdvancedFabricatorFacility : ProcessingFacilityBase
{
    [Header("Fabricator Specific Settings")]
    [SerializeField] private float recipeEfficiencyBonus = 0.3f;
    [SerializeField] private float precisionMultiplier = 1.5f;
    [SerializeField] private float qualityControlBonus = 0.25f;
    
    [Header("Recipe Management")]
    [SerializeField] private List<ProcessingRecipeData> availableRecipes = new List<ProcessingRecipeData>();
    [SerializeField] private ProcessingRecipeData currentRecipe;
    [SerializeField] private bool autoSelectOptimalRecipe = true;
    [SerializeField] private float recipeChangeTime = 5f;
    
    [Header("Advanced Processing")]
    [SerializeField] private bool hasAdvancedSensors = true;
    [SerializeField] private bool hasQualityControl = true;
    [SerializeField] private bool hasAutomatedSorting = false;
    [SerializeField] private float automationLevel = 0.5f;
    
    [Header("Material Synthesis")]
    [SerializeField] private List<ResourceType> synthesizableResources = new List<ResourceType>
    {
        ResourceType.AdvancedAlloy,
        ResourceType.Polymer,
        ResourceType.Composite,
        ResourceType.Electronics
    };
    
    // Recipe processing state
    private Queue<ProcessingRecipeData> recipeQueue = new Queue<ProcessingRecipeData>();
    private bool isChangingRecipe = false;
    private float recipeChangeTimer = 0f;
    private Dictionary<ResourceType, int> inputBuffer = new Dictionary<ResourceType, int>();
    private int recipesCompleted = 0;
    private float totalRecipeValue = 0f;
    
    protected override void Awake()
    {
        // Set default values for fabricator facility
        facilityName = "Advanced Fabricator";
        facilityType = ProcessingType.Fabrication;
        baseEfficiency = 1.3f;
        maxConcurrentJobs = 2;
        energyConsumption = 2.0f; // Higher energy consumption for advanced processing
        
        // Advanced fabricators handle complex recipes
        maxRecipeComplexity = 5;
        craftingEfficiencyBonus = 0.3f;
        specializedOutputs.AddRange(new[]
        {
            ResourceType.AssemblyRobot,
            ResourceType.QualityInspector,
            ResourceType.AutomatedFactory
        });
        
        // Fabricators can handle all waste types for recipe inputs
        supportedWasteTypes = System.Enum.GetValues(typeof(WasteType)).Cast<WasteType>().ToList();
        
        base.Awake();
    }
    
    protected override void Update()
    {
        base.Update();
        
        UpdateRecipeProcessing();
        UpdateAutomation();
    }
    
    protected override void InitializeFacilityData()
    {
        base.InitializeFacilityData();
        
        // Add fabricator-specific data
        facilityData.facilityName = "Advanced Fabricator";
        facilityData.facilityType = ProcessingType.Fabrication;
        
        // Load available recipes
        LoadAvailableRecipes();
    }
    
    public override bool CanProcessWasteItem(UpdatedWasteItem wasteItem)
    {
        if (!base.CanProcessWasteItem(wasteItem)) return false;
        
        // Check if we have a recipe that can use this waste item
        if (currentRecipe != null)
        {
            return CanUseInCurrentRecipe(wasteItem);
        }
        
        // Check if any available recipe can use this item
        return availableRecipes.Any(recipe => CanUseInRecipe(wasteItem, recipe));
    }
    
    public override bool CanProcessRecipe(ProcessingRecipeData recipe)
    {
        if (!base.CanProcessRecipe(recipe)) return false;
        
        // Check recipe complexity
        if (GetRecipeComplexity(recipe) > maxRecipeComplexity)
            return false;
        
        // Check if we have required resources
        return HasRequiredResources(recipe);
    }
    
    public override bool ProcessRecipe(ProcessingRecipeData recipe)
    {
        if (!CanProcessRecipe(recipe))
        {
            Debug.LogWarning($"Cannot process recipe {recipe.recipeName} in {facilityName}");
            return false;
        }
        
        // Add to recipe queue or process immediately
        if (currentRecipe == null)
        {
            SetCurrentRecipe(recipe);
        }
        else
        {
            recipeQueue.Enqueue(recipe);
            Debug.Log($"Added recipe {recipe.recipeName} to queue");
        }
        
        return true;
    }
    
    protected override float GetProcessingModifier(UpdatedWasteItem wasteItem)
    {
        float modifier = base.GetProcessingModifier(wasteItem);
        
        // Apply recipe efficiency bonus
        modifier += recipeEfficiencyBonus;
        
        // Apply precision multiplier
        modifier *= precisionMultiplier;
        
        // Quality control bonus
        if (hasQualityControl)
        {
            modifier += qualityControlBonus;
        }
        
        // Advanced sensors bonus
        if (hasAdvancedSensors)
        {
            modifier += 0.15f;
        }
        
        // Automation bonus
        modifier += automationLevel * 0.2f;
        
        // Recipe-specific bonuses
        if (currentRecipe != null)
        {
            modifier += GetRecipeSpecificBonus(wasteItem, currentRecipe);
        }
        
        return Mathf.Max(0.1f, modifier);
    }
    
    protected override float GetRecipeModifier(ProcessingRecipeData recipe)
    {
        float modifier = base.GetRecipeModifier(recipe);
        
        // Fabricator specialization bonus
        modifier += recipeEfficiencyBonus;
        
        // Complexity bonus (more complex recipes get better results)
        int complexity = GetRecipeComplexity(recipe);
        modifier += complexity * 0.1f;
        
        // Quality control bonus
        if (hasQualityControl)
        {
            modifier += qualityControlBonus;
        }
        
        return modifier;
    }
    
    protected override void OnFacilityUpgraded()
    {
        base.OnFacilityUpgraded();
        
        // Fabricator-specific upgrades
        recipeEfficiencyBonus += 0.1f;
        maxRecipeComplexity++;
        
        // Unlock advanced features
        if (facilityLevel >= 2 && !hasAdvancedSensors)
        {
            hasAdvancedSensors = true;
            Debug.Log("Advanced sensors installed");
        }
        
        if (facilityLevel >= 3 && !hasQualityControl)
        {
            hasQualityControl = true;
            qualityControlBonus = 0.25f;
            Debug.Log("Quality control system installed");
        }
        
        if (facilityLevel >= 4 && !hasAutomatedSorting)
        {
            hasAutomatedSorting = true;
            automationLevel = Mathf.Min(1f, automationLevel + 0.2f);
            Debug.Log("Automated sorting system installed");
        }
        
        // Improve automation level
        if (facilityLevel >= 5)
        {
            automationLevel = Mathf.Min(1f, automationLevel + 0.1f);
        }
        
        // Reduce recipe change time
        recipeChangeTime = Mathf.Max(1f, recipeChangeTime - 0.5f);
        
        Debug.Log($"Fabricator upgraded - Recipe Efficiency: {recipeEfficiencyBonus:P}, Max Complexity: {maxRecipeComplexity}");
    }
    
    protected override void OnProcessingCompleted(ProcessingJob job, Dictionary<ResourceType, int> yield)
    {
        base.OnProcessingCompleted(job, yield);
        
        // Apply fabricator-specific processing
        ApplyQualityControl(yield);
        ApplyAdvancedProcessing(yield);
        
        // Update statistics
        recipesCompleted++;
        totalRecipeValue += CalculateYieldValue(yield);
        
        // Special fabrication completion effects
        PlayFabricationEffects();
    }
    
    #region Recipe Management
    
    private void UpdateRecipeProcessing()
    {
        // Handle recipe changes
        if (isChangingRecipe)
        {
            recipeChangeTimer += Time.deltaTime;
            
            if (recipeChangeTimer >= recipeChangeTime)
            {
                CompleteRecipeChange();
            }
        }
        
        // Auto-select optimal recipe if enabled
        if (autoSelectOptimalRecipe && currentRecipe == null && recipeQueue.Count == 0)
        {
            var optimalRecipe = FindOptimalRecipe();
            if (optimalRecipe != null)
            {
                SetCurrentRecipe(optimalRecipe);
            }
        }
        
        // Process recipe queue
        if (currentRecipe == null && recipeQueue.Count > 0)
        {
            SetCurrentRecipe(recipeQueue.Dequeue());
        }
    }
    
    private void SetCurrentRecipe(ProcessingRecipeData recipe)
    {
        if (currentRecipe != recipe)
        {
            currentRecipe = recipe;
            isChangingRecipe = true;
            recipeChangeTimer = 0f;
            
            Debug.Log($"Changing to recipe: {recipe.recipeName}");
            PlayRecipeChangeEffects();
        }
    }
    
    private void CompleteRecipeChange()
    {
        isChangingRecipe = false;
        recipeChangeTimer = 0f;
        
        Debug.Log($"Recipe change complete: {currentRecipe?.recipeName}");
    }
    
    private ProcessingRecipeData FindOptimalRecipe()
    {
        // Find the best recipe based on available resources and efficiency
        ProcessingRecipeData bestRecipe = null;
        float bestScore = 0f;
        
        foreach (var recipe in availableRecipes)
        {
            if (CanProcessRecipe(recipe))
            {
                float score = CalculateRecipeScore(recipe);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestRecipe = recipe;
                }
            }
        }
        
        return bestRecipe;
    }
    
    private float CalculateRecipeScore(ProcessingRecipeData recipe)
    {
        // Score based on efficiency, complexity, and output value
        float score = 0f;
        
        // Efficiency score
        score += recipe.GetEfficiencyRatio() * 10f;
        
        // Complexity bonus
        score += GetRecipeComplexity(recipe) * 5f;
        
        // Output value score
        score += CalculateRecipeOutputValue(recipe);
        
        return score;
    }
    
    private void LoadAvailableRecipes()
    {
        // Load recipes from resources or configuration
        // This would typically load from ScriptableObject assets
        if (availableRecipes.Count == 0)
        {
            Debug.Log("No recipes loaded - fabricator will operate in direct processing mode");
        }
        else
        {
            Debug.Log($"Loaded {availableRecipes.Count} recipes for fabricator");
        }
    }
    
    #endregion
    
    #region Recipe Processing Logic
    
    private bool CanUseInCurrentRecipe(UpdatedWasteItem wasteItem)
    {
        if (currentRecipe == null) return false;
        
        // Check if waste item can be converted to required recipe inputs
        return currentRecipe.inputResources.Any(req => 
            CanConvertWasteToResource(wasteItem, req.type));
    }
    
    private bool CanUseInRecipe(UpdatedWasteItem wasteItem, ProcessingRecipeData recipe)
    {
        return recipe.inputResources.Any(req => 
            CanConvertWasteToResource(wasteItem, req.type));
    }
    
    private bool CanConvertWasteToResource(UpdatedWasteItem wasteItem, ResourceType targetResource)
    {
        // Define conversion rules based on waste type and target resource
        switch (wasteItem.Type)
        {
            case WasteType.Metal:
                return targetResource == ResourceType.ScrapMetal || 
                       targetResource == ResourceType.RefinedMetal ||
                       targetResource == ResourceType.AdvancedAlloy;
            
            case WasteType.Plastic:
                return targetResource == ResourceType.Polymer ||
                       targetResource == ResourceType.Composite;
            
            case WasteType.Electronics:
                return targetResource == ResourceType.Electronics ||
                       targetResource == ResourceType.RareMetals;
            
            default:
                return false;
        }
    }
    
    private bool HasRequiredResources(ProcessingRecipeData recipe)
    {
        // Check if we have or can obtain required resources
        foreach (var requirement in recipe.inputResources)
        {
            if (!CanObtainResource(requirement.type, requirement.amount))
            {
                return false;
            }
        }
        
        return true;
    }
    
    private bool CanObtainResource(ResourceType resourceType, int amount)
    {
        // Check current inventory
        if (resourceManager != null)
        {
            int available = resourceManager.GetResourceAmount(resourceType);
            if (available >= amount) return true;
        }
        
        // Check input buffer
        if (inputBuffer.ContainsKey(resourceType) && inputBuffer[resourceType] >= amount)
        {
            return true;
        }
        
        // Check if we can process waste to get this resource
        return wasteInventory != null && 
               wasteInventory.GetInventoryCount() > 0;
    }
    
    private int GetRecipeComplexity(ProcessingRecipeData recipe)
    {
        // Calculate complexity based on inputs, outputs, and processing requirements
        int complexity = 0;
        
        complexity += recipe.inputResources?.Length ?? 0;
        complexity += recipe.outputResources?.Length ?? 0;
        complexity += recipe.bonusOutputs?.Length ?? 0;
        
        if (recipe.minimumFacilityLevel > 1)
            complexity += recipe.minimumFacilityLevel - 1;
        
        return complexity;
    }
    
    #endregion
    
    #region Advanced Processing
    
    private void UpdateAutomation()
    {
        if (!hasAutomatedSorting) return;
        
        // Automated sorting and resource management
        if (automationLevel > 0.5f)
        {
            AutoSortInputs();
            AutoOptimizeProcessing();
        }
    }
    
    private void AutoSortInputs()
    {
        // Automatically sort and prepare inputs for current recipe
        if (currentRecipe == null) return;
        
        // This would integrate with the inventory system to automatically
        // gather required resources for the current recipe
    }
    
    private void AutoOptimizeProcessing()
    {
        // Automatically optimize processing parameters
        if (currentRecipe != null && automationLevel > 0.8f)
        {
            // Adjust processing parameters for optimal efficiency
            currentEfficiency *= (1f + automationLevel * 0.1f);
        }
    }
    
    private void ApplyQualityControl(Dictionary<ResourceType, int> yield)
    {
        if (!hasQualityControl) return;
        
        // Quality control improves yield consistency and reduces defects
        var keys = yield.Keys.ToList();
        foreach (var key in keys)
        {
            int originalAmount = yield[key];
            int improvedAmount = Mathf.FloorToInt(originalAmount * (1f + qualityControlBonus));
            yield[key] = improvedAmount;
        }
    }
    
    private void ApplyAdvancedProcessing(Dictionary<ResourceType, int> yield)
    {
        // Advanced processing can create higher-tier materials
        if (currentRecipe != null && hasAdvancedSensors)
        {
            CreateAdvancedMaterials(yield);
        }
    }
    
    private void CreateAdvancedMaterials(Dictionary<ResourceType, int> yield)
    {
        // Convert some basic materials to advanced ones
        foreach (var synthesizable in synthesizableResources)
        {
            if (CanSynthesize(synthesizable, yield))
            {
                int synthesizedAmount = CalculateSynthesisAmount(synthesizable, yield);
                if (synthesizedAmount > 0)
                {
                    ConsumeSynthesisInputs(synthesizable, yield, synthesizedAmount);
                    
                    if (yield.ContainsKey(synthesizable))
                        yield[synthesizable] += synthesizedAmount;
                    else
                        yield[synthesizable] = synthesizedAmount;
                    
                    Debug.Log($"Synthesized {synthesizedAmount} {synthesizable}");
                }
            }
        }
    }
    
    private bool CanSynthesize(ResourceType target, Dictionary<ResourceType, int> yield)
    {
        // Define synthesis rules
        switch (target)
        {
            case ResourceType.AdvancedAlloy:
                return yield.ContainsKey(ResourceType.RefinedMetal) && 
                       yield[ResourceType.RefinedMetal] >= 3;
            
            case ResourceType.Polymer:
                return yield.ContainsKey(ResourceType.Plastic) && 
                       yield[ResourceType.Plastic] >= 2;
            
            case ResourceType.Composite:
                return yield.ContainsKey(ResourceType.Polymer) && 
                       yield.ContainsKey(ResourceType.RefinedMetal);
            
            default:
                return false;
        }
    }
    
    private int CalculateSynthesisAmount(ResourceType target, Dictionary<ResourceType, int> yield)
    {
        switch (target)
        {
            case ResourceType.AdvancedAlloy:
                return yield.ContainsKey(ResourceType.RefinedMetal) ? 
                       yield[ResourceType.RefinedMetal] / 3 : 0;
            
            case ResourceType.Polymer:
                return yield.ContainsKey(ResourceType.Plastic) ? 
                       yield[ResourceType.Plastic] / 2 : 0;
            
            case ResourceType.Composite:
                if (yield.ContainsKey(ResourceType.Polymer) && 
                    yield.ContainsKey(ResourceType.RefinedMetal))
                {
                    return Mathf.Min(yield[ResourceType.Polymer], 
                                   yield[ResourceType.RefinedMetal]);
                }
                return 0;
            
            default:
                return 0;
        }
    }
    
    private void ConsumeSynthesisInputs(ResourceType target, Dictionary<ResourceType, int> yield, int amount)
    {
        switch (target)
        {
            case ResourceType.AdvancedAlloy:
                yield[ResourceType.RefinedMetal] -= amount * 3;
                break;
            
            case ResourceType.Polymer:
                yield[ResourceType.Plastic] -= amount * 2;
                break;
            
            case ResourceType.Composite:
                yield[ResourceType.Polymer] -= amount;
                yield[ResourceType.RefinedMetal] -= amount;
                break;
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    private float GetRecipeSpecificBonus(UpdatedWasteItem wasteItem, ProcessingRecipeData recipe)
    {
        // Bonus for items that are perfect matches for recipe requirements
        float bonus = 0f;
        
        foreach (var requirement in recipe.inputResources)
        {
            if (CanConvertWasteToResource(wasteItem, requirement.type))
            {
                bonus += 0.1f;
            }
        }
        
        return bonus;
    }
    
    private float CalculateYieldValue(Dictionary<ResourceType, int> yield)
    {
        float totalValue = 0f;
        
        foreach (var kvp in yield)
        {
            // This would use actual resource values from the resource manager
            totalValue += kvp.Value * GetResourceValue(kvp.Key);
        }
        
        return totalValue;
    }
    
    private float GetResourceValue(ResourceType resourceType)
    {
        // Basic resource values - would be loaded from configuration
        switch (resourceType)
        {
            case ResourceType.AdvancedAlloy:
                return 50f;
            case ResourceType.Polymer:
                return 25f;
            case ResourceType.Composite:
                return 75f;
            case ResourceType.Electronics:
                return 100f;
            default:
                return 10f;
        }
    }
    
    private float CalculateRecipeOutputValue(ProcessingRecipeData recipe)
    {
        float value = 0f;
        
        foreach (var output in recipe.outputResources)
        {
            value += output.amount * GetResourceValue(output.type);
        }
        
        return value;
    }
    
    #endregion
    
    #region Visual Effects
    
    private void PlayFabricationEffects()
    {
        // Play fabrication-specific particle effects
        if (processingEffect != null)
        {
            var main = processingEffect.main;
            main.startColor = Color.magenta;
            processingEffect.Emit(35);
        }
        
        // Play fabrication sound
        if (processingAudio != null && processingAudio.clip != null)
        {
            processingAudio.pitch = Random.Range(1.0f, 1.3f); // Higher pitch for precision work
            processingAudio.Play();
        }
    }
    
    private void PlayRecipeChangeEffects()
    {
        // Visual feedback for recipe changes
        if (statusLight != null)
        {
            statusLight.color = Color.cyan;
            statusLight.intensity = 2f;
        }
        
        Debug.Log("Recipe configuration changing...");
    }
    
    protected override void UpdateVisualEffects()
    {
        base.UpdateVisualEffects();
        
        // Update recipe-based visual effects
        if (statusLight != null)
        {
            if (isChangingRecipe)
            {
                // Pulse during recipe changes
                float pulse = Mathf.Sin(Time.time * 5f) * 0.5f + 0.5f;
                statusLight.intensity = 1f + pulse;
                statusLight.color = Color.cyan;
            }
            else if (currentRecipe != null)
            {
                // Different colors for different recipe types
                statusLight.color = GetRecipeColor(currentRecipe);
            }
        }
        
        // Update fabrication effects based on automation level
        if (processingEffect != null && isProcessing)
        {
            var emission = processingEffect.emission;
            emission.rateOverTime = 20f + (automationLevel * 30f);
        }
    }
    
    private Color GetRecipeColor(ProcessingRecipeData recipe)
    {
        // Different colors for different recipe complexities
        int complexity = GetRecipeComplexity(recipe);
        
        switch (complexity)
        {
            case 1:
            case 2:
                return Color.green;
            case 3:
            case 4:
                return Color.yellow;
            case 5:
            case 6:
                return new Color(1f, 0.5f, 0f); // Orange color
            default:
                return Color.red;
        }
    }
    
    #endregion
    
    #region Public Interface Extensions
    
    /// <summary>
    /// Get current recipe status
    /// </summary>
    public string GetRecipeStatus()
    {
        if (isChangingRecipe)
            return $"Changing Recipe ({recipeChangeTimer/recipeChangeTime:P})";
        
        if (currentRecipe != null)
            return $"Recipe: {currentRecipe.recipeName} (Complexity: {GetRecipeComplexity(currentRecipe)})";
        
        return "No Active Recipe";
    }
    
    /// <summary>
    /// Get fabrication statistics
    /// </summary>
    public string GetFabricationStats()
    {
        return $"Recipes Completed: {recipesCompleted} | Total Value: {totalRecipeValue:F1}";
    }
    
    /// <summary>
    /// Get automation status
    /// </summary>
    public string GetAutomationStatus()
    {
        string status = $"Automation: {automationLevel:P}";
        
        if (hasAdvancedSensors) status += " | Sensors: ON";
        if (hasQualityControl) status += " | QC: ON";
        if (hasAutomatedSorting) status += " | Auto-Sort: ON";
        
        return status;
    }
    
    /// <summary>
    /// Add recipe to available recipes
    /// </summary>
    public void AddRecipe(ProcessingRecipeData recipe)
    {
        if (!availableRecipes.Contains(recipe))
        {
            availableRecipes.Add(recipe);
            Debug.Log($"Added recipe: {recipe.recipeName}");
        }
    }
    
    /// <summary>
    /// Remove recipe from available recipes
    /// </summary>
    public void RemoveRecipe(ProcessingRecipeData recipe)
    {
        if (availableRecipes.Contains(recipe))
        {
            availableRecipes.Remove(recipe);
            
            if (currentRecipe == recipe)
            {
                currentRecipe = null;
            }
            
            Debug.Log($"Removed recipe: {recipe.recipeName}");
        }
    }
    
    /// <summary>
    /// Force change to specific recipe
    /// </summary>
    public void ChangeToRecipe(ProcessingRecipeData recipe)
    {
        if (availableRecipes.Contains(recipe))
        {
            SetCurrentRecipe(recipe);
            Debug.Log($"Manually changed to recipe: {recipe.recipeName}");
        }
    }
    
    /// <summary>
    /// Clear current recipe
    /// </summary>
    public void ClearCurrentRecipe()
    {
        currentRecipe = null;
        isChangingRecipe = false;
        recipeChangeTimer = 0f;
        Debug.Log("Cleared current recipe");
    }
    
    public override string GetStatusInfo()
    {
        string baseStatus = base.GetStatusInfo();
        string recipeStatus = currentRecipe != null ? $"Recipe: {currentRecipe.recipeName}" : "No Recipe";
        
        if (isChangingRecipe)
            recipeStatus = "Changing Recipe";
        
        return $"{baseStatus} | {recipeStatus}";
    }
    
    #endregion
} 