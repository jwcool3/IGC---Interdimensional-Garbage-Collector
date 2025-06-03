using UnityEngine;
using System;

/// <summary>
/// ScriptableObject for creating processing recipes as Unity assets
/// Allows designers to easily create and modify resource conversion recipes
/// </summary>
[CreateAssetMenu(fileName = "New Processing Recipe", menuName = "Resources/Processing Recipe")]
public class ProcessingRecipeData : ScriptableObject
{
    [Header("Recipe Information")]
    public string recipeName;
    [TextArea(2, 4)]
    public string description;
    public Sprite recipeIcon;
    
    [Header("Processing Requirements")]
    public string requiredFacility = "Basic Processor";
    [Range(0.1f, 60f)]
    public float processingTime = 2f;
    [Range(0f, 1f)]
    public float successChance = 1f; // 1 = always succeeds, 0.8 = 80% chance
    
    [Header("Resource Inputs")]
    public ResourceAmount[] inputResources;
    
    [Header("Resource Outputs")]
    public ResourceAmount[] outputResources;
    
    [Header("Bonus Outputs (Chance-based)")]
    public ResourceChance[] bonusOutputs;
    
    [Header("Recipe Properties")]
    public RecipeCategory category = RecipeCategory.Basic;
    public RecipeDifficulty difficulty = RecipeDifficulty.Easy;
    public bool isUnlocked = true;
    public bool requiresResearch = false;
    
    [Header("Unlock Requirements")]
    [Tooltip("Other recipes that must be completed before this one unlocks")]
    public ProcessingRecipeData[] prerequisiteRecipes;
    [Tooltip("Minimum facility level required")]
    public int minimumFacilityLevel = 1;
    
    [Header("Visual Effects")]
    public Color processingEffectColor = Color.white;
    public AudioClip processingSound;
    public ParticleSystem processingParticles;
    
    /// <summary>
    /// Convert this ScriptableObject to a ProcessingRecipe for use in the system
    /// </summary>
    public ProcessingRecipe ToProcessingRecipe()
    {
        var recipe = new ProcessingRecipe
        {
            recipeName = this.recipeName,
            inputs = this.inputResources ?? new ResourceAmount[0],
            outputs = this.outputResources ?? new ResourceAmount[0],
            processingTime = this.processingTime,
            requiredFacility = this.requiredFacility
        };
        
        return recipe;
    }
    
    /// <summary>
    /// Check if this recipe can be used (unlocked and requirements met)
    /// </summary>
    public bool CanUse()
    {
        if (!isUnlocked) return false;
        
        // Check prerequisite recipes
        if (prerequisiteRecipes != null)
        {
            foreach (var prereq in prerequisiteRecipes)
            {
                if (prereq != null && !prereq.isUnlocked)
                {
                    return false;
                }
            }
        }
        
        // Check facility level (would need facility manager integration)
        // For now, assume facility level is met
        
        return true;
    }
    
    /// <summary>
    /// Check if player has enough resources to use this recipe
    /// </summary>
    public bool CanAfford()
    {
        if (NewResourceManager.Instance == null) return false;
        
        return NewResourceManager.Instance.CanAffordRecipe(inputResources);
    }
    
    /// <summary>
    /// Get the total input value for balancing purposes
    /// </summary>
    public int GetInputValue()
    {
        int totalValue = 0;
        
        if (inputResources != null)
        {
            foreach (var input in inputResources)
            {
                var config = NewResourceManager.Instance?.GetResourceConfig(input.type);
                int baseValue = config?.baseValue ?? 1;
                totalValue += input.amount * baseValue;
            }
        }
        
        return totalValue;
    }
    
    /// <summary>
    /// Get the total output value for balancing purposes
    /// </summary>
    public int GetOutputValue()
    {
        int totalValue = 0;
        
        if (outputResources != null)
        {
            foreach (var output in outputResources)
            {
                var config = NewResourceManager.Instance?.GetResourceConfig(output.type);
                int baseValue = config?.baseValue ?? 1;
                totalValue += output.amount * baseValue;
            }
        }
        
        // Add expected value from bonus outputs
        if (bonusOutputs != null)
        {
            foreach (var bonus in bonusOutputs)
            {
                var config = NewResourceManager.Instance?.GetResourceConfig(bonus.type);
                int baseValue = config?.baseValue ?? 1;
                totalValue += Mathf.RoundToInt(bonus.amount * baseValue * bonus.chance);
            }
        }
        
        return totalValue;
    }
    
    /// <summary>
    /// Get efficiency ratio (output value / input value)
    /// </summary>
    public float GetEfficiencyRatio()
    {
        int inputValue = GetInputValue();
        if (inputValue == 0) return 0f;
        
        return (float)GetOutputValue() / inputValue;
    }
    
    /// <summary>
    /// Get a formatted description of inputs and outputs
    /// </summary>
    public string GetFormattedDescription()
    {
        string desc = description;
        
        if (string.IsNullOrEmpty(desc))
        {
            desc = $"Convert {FormatResourceList(inputResources)} into {FormatResourceList(outputResources)}";
        }
        
        if (bonusOutputs != null && bonusOutputs.Length > 0)
        {
            desc += $"\nBonus chance: {FormatBonusList(bonusOutputs)}";
        }
        
        return desc;
    }
    
    private string FormatResourceList(ResourceAmount[] resources)
    {
        if (resources == null || resources.Length == 0) return "nothing";
        
        string[] formatted = new string[resources.Length];
        for (int i = 0; i < resources.Length; i++)
        {
            formatted[i] = $"{resources[i].amount} {resources[i].type}";
        }
        
        return string.Join(", ", formatted);
    }
    
    private string FormatBonusList(ResourceChance[] bonuses)
    {
        if (bonuses == null || bonuses.Length == 0) return "none";
        
        string[] formatted = new string[bonuses.Length];
        for (int i = 0; i < bonuses.Length; i++)
        {
            formatted[i] = $"{bonuses[i].amount} {bonuses[i].type} ({bonuses[i].chance:P0})";
        }
        
        return string.Join(", ", formatted);
    }
    
    /// <summary>
    /// Unlock this recipe (for progression systems)
    /// </summary>
    public void Unlock()
    {
        isUnlocked = true;
        Debug.Log($"Recipe unlocked: {recipeName}");
    }
    
    /// <summary>
    /// Lock this recipe
    /// </summary>
    public void Lock()
    {
        isUnlocked = false;
        Debug.Log($"Recipe locked: {recipeName}");
    }
    
    #region Validation
    
    /// <summary>
    /// Validate this recipe for common issues
    /// </summary>
    public bool ValidateRecipe(out string errorMessage)
    {
        errorMessage = "";
        
        // Check basic properties
        if (string.IsNullOrEmpty(recipeName))
        {
            errorMessage = "Recipe name cannot be empty";
            return false;
        }
        
        if (processingTime <= 0)
        {
            errorMessage = "Processing time must be greater than 0";
            return false;
        }
        
        // Check inputs
        if (inputResources == null || inputResources.Length == 0)
        {
            errorMessage = "Recipe must have at least one input resource";
            return false;
        }
        
        foreach (var input in inputResources)
        {
            if (input.amount <= 0)
            {
                errorMessage = $"Input amount for {input.type} must be greater than 0";
                return false;
            }
        }
        
        // Check outputs
        if (outputResources == null || outputResources.Length == 0)
        {
            errorMessage = "Recipe must have at least one output resource";
            return false;
        }
        
        foreach (var output in outputResources)
        {
            if (output.amount <= 0)
            {
                errorMessage = $"Output amount for {output.type} must be greater than 0";
                return false;
            }
        }
        
        // Check for circular dependencies in prerequisites
        if (HasCircularDependency())
        {
            errorMessage = "Recipe has circular dependency in prerequisites";
            return false;
        }
        
        return true;
    }
    
    private bool HasCircularDependency()
    {
        return HasCircularDependencyRecursive(this, new System.Collections.Generic.HashSet<ProcessingRecipeData>());
    }
    
    private bool HasCircularDependencyRecursive(ProcessingRecipeData recipe, System.Collections.Generic.HashSet<ProcessingRecipeData> visited)
    {
        if (visited.Contains(recipe)) return true;
        
        visited.Add(recipe);
        
        if (recipe.prerequisiteRecipes != null)
        {
            foreach (var prereq in recipe.prerequisiteRecipes)
            {
                if (prereq != null && HasCircularDependencyRecursive(prereq, visited))
                {
                    return true;
                }
            }
        }
        
        visited.Remove(recipe);
        return false;
    }
    
    #endregion
}

/// <summary>
/// Categories for organizing recipes
/// </summary>
public enum RecipeCategory
{
    Basic,          // Simple conversions
    Fuel,           // Fuel production
    Food,           // Food production
    Parts,          // Component crafting
    Advanced,       // Complex multi-resource recipes
    Experimental    // High-risk, high-reward recipes
}

/// <summary>
/// Difficulty levels for recipes
/// </summary>
public enum RecipeDifficulty
{
    Easy,           // Always succeeds, low resource cost
    Medium,         // High success rate, moderate cost
    Hard,           // Lower success rate, high cost
    Expert          // Risky but very rewarding
} 