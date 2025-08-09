using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for showing recipe ingredients
/// </summary>
public class IngredientDisplay : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image ingredientIcon;
    [SerializeField] private TextMeshProUGUI ingredientNameText;
    [SerializeField] private TextMeshProUGUI requiredAmountText;
    [SerializeField] private TextMeshProUGUI availableAmountText;
    [SerializeField] private Image availabilityIndicator;
    
    public void UpdateDisplay(CraftingIngredient ingredient, int batchSize)
    {
        int requiredAmount = ingredient.amount * batchSize;
        int availableAmount = GetAvailableAmount(ingredient);
        bool hasEnough = availableAmount >= requiredAmount;
        
        // Update name
        if (ingredientNameText != null)
            ingredientNameText.text = GetResourceDisplayName(ingredient.resourceType);
        
        // Update amounts
        if (requiredAmountText != null)
            requiredAmountText.text = requiredAmount.ToString();
            
        if (availableAmountText != null)
        {
            availableAmountText.text = availableAmount.ToString();
            availableAmountText.color = hasEnough ? Color.green : Color.red;
        }
        
        // Update icon
        if (ingredientIcon != null)
        {
            var config = ResourceConfigManager.Instance?.GetResourceConfig(ingredient.resourceType);
            if (config?.icon != null)
                ingredientIcon.sprite = config.icon;
        }
        
        // Update availability indicator
        if (availabilityIndicator != null)
            availabilityIndicator.color = hasEnough ? Color.green : Color.red;
    }
    
    private int GetAvailableAmount(CraftingIngredient ingredient)
    {
        if (ingredient.isCraftedItem)
        {
            return CraftedItemInventory.Instance?.GetCraftedItemAmount(ingredient.resourceType) ?? 0;
        }
        else
        {
            return NewResourceManager.Instance?.GetResourceAmount(ingredient.resourceType) ?? 0;
        }
    }
    
    private string GetResourceDisplayName(ResourceType resourceType)
    {
        var config = ResourceConfigManager.Instance?.GetResourceConfig(resourceType);
        return config?.displayName ?? resourceType.ToString();
    }
} 