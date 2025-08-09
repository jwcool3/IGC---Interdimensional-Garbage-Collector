using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Crafting/Recipe")]
public class CraftingRecipeData : ScriptableObject
{
    [Header("Recipe Info")]
    public string recipeName;
    [TextArea(2, 4)]
    public string description;
    public Sprite recipeIcon;
    [Range(1, 5)]
    public int recipeComplexity = 1; // 1-5, affects which facilities can handle it
    
    [Header("Requirements")]
    public List<CraftingIngredient> requiredIngredients = new List<CraftingIngredient>();
    public ProcessingType requiredFacilityType = ProcessingType.Fabrication;
    public int minimumFacilityLevel = 1;
    
    [Header("Output")]
    public ResourceType outputItemType;
    public int outputAmount = 1;
    
    [Header("Processing")]
    public float baseProcessingTime = 60f; // seconds
    [Range(0f, 1f)]
    public float successChance = 1.0f; // 0.0 to 1.0 (1.0 = always succeeds)
    public bool canBatchProcess = true;
    public int maxBatchSize = 10;
    
    [Header("Risk & Rewards")]
    [Range(0f, 1f)]
    public float failureChance = 0.0f; // 0.0 to 1.0
    public bool refundsIngredientsOnFailure = false;
    [Range(0f, 1f)]
    public float criticalSuccessChance = 0.05f; // 5% chance for bonus output
    public int criticalSuccessBonus = 1; // Extra items on critical success
    
    [Header("Facility Bonuses")]
    public List<FacilityBonus> facilityBonuses = new List<FacilityBonus>();
    
    [Header("Unlock Requirements")]
    public bool isUnlocked = true;
    public List<UnlockRequirement> unlockRequirements = new List<UnlockRequirement>();
    
    /// <summary>
    /// Check if player has all required ingredients
    /// </summary>
    public bool CanCraft(int batchSize = 1)
    {
        if (CraftedItemInventory.Instance == null || ResourceManager.Instance == null)
            return false;
            
        foreach (var ingredient in requiredIngredients)
        {
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
        return true;
    }
    
    /// <summary>
    /// Check if this recipe is unlocked for the player
    /// </summary>
    public bool IsRecipeUnlocked()
    {
        if (isUnlocked) return true;
        
        foreach (var requirement in unlockRequirements)
        {
            if (!requirement.IsMet()) return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Calculate actual processing time with facility bonuses
    /// </summary>
    public float GetProcessingTime(ProcessingFacilityBase facility, int batchSize = 1)
    {
        float time = baseProcessingTime * batchSize;
        
        // Apply facility-specific bonuses
        foreach (var bonus in facilityBonuses)
        {
            if (bonus.facilityType == facility.FacilityType)
            {
                time *= bonus.timeMultiplier;
                break;
            }
        }
        
        // Apply facility efficiency
        time /= facility.Efficiency;
        
        return time;
    }
    
    /// <summary>
    /// Calculate success chance with facility bonuses
    /// </summary>
    public float GetSuccessChance(ProcessingFacilityBase facility)
    {
        float chance = successChance;
        
        // Apply facility-specific bonuses
        foreach (var bonus in facilityBonuses)
        {
            if (bonus.facilityType == facility.FacilityType)
            {
                chance = Mathf.Min(1.0f, chance + bonus.successBonus);
                break;
            }
        }
        
        return chance;
    }
    
    /// <summary>
    /// Get ingredient cost breakdown for display
    /// </summary>
    public string GetIngredientSummary(int batchSize = 1)
    {
        var summary = new System.Text.StringBuilder();
        
        foreach (var ingredient in requiredIngredients)
        {
            int required = ingredient.amount * batchSize;
            summary.AppendLine($"• {required} {ingredient.resourceType}");
        }
        
        return summary.ToString();
    }
}

[System.Serializable]
public class CraftingIngredient
{
    public ResourceType resourceType;
    public int amount;
    public bool isCraftedItem = false; // True if this ingredient is a crafted component
    [Tooltip("If true, this ingredient is optional and affects success chance instead")]
    public bool isOptional = false;
    [Range(0f, 1f)]
    public float successBonusIfPresent = 0.1f; // Bonus if optional ingredient is provided
}

[System.Serializable]
public class FacilityBonus
{
    public ProcessingType facilityType;
    [Range(0.1f, 2f)]
    public float timeMultiplier = 1.0f; // Lower = faster
    [Range(0f, 1f)]
    public float successBonus = 0.0f; // Added to success chance
    [Range(0f, 1f)]
    public float efficiencyBonus = 0.0f; // Added to facility efficiency
}

[System.Serializable]
public class UnlockRequirement
{
    public UnlockCondition conditionType;
    public ResourceType requiredResourceType;
    public int requiredAmount;
    public string requiredResearchId;
    public int requiredPlayerLevel;
    
    public bool IsMet()
    {
        return conditionType switch
        {
            UnlockCondition.Resource => ResourceManager.Instance?.GetResourceAmount(requiredResourceType) >= requiredAmount,
            UnlockCondition.Level => GameManager.Instance?.TotalWasteCollected >= requiredPlayerLevel,
            // Add more unlock conditions as needed
            _ => true
        };
    }
} 