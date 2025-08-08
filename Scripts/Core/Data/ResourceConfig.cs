using UnityEngine;

/// <summary>
/// Configuration data for a resource type
/// Defines display properties, behavior, and metadata for resources
/// </summary>
[CreateAssetMenu(fileName = "New Resource Config", menuName = "Resources/Resource Configuration", order = 0)]
[System.Serializable]
public class ResourceConfig : ScriptableObject
{
    [Header("Basic Properties")]
    public ResourceType resourceType = ResourceType.None;
    public string displayName = "";
    [TextArea(2, 4)]
    public string description = "";
    
    [Header("Value and Economy")]
    public int baseValue = 1;
    public ResourceCategory category = ResourceCategory.Basic;
    public ResourceRarity rarity = ResourceRarity.Common;
    
    [Header("Stacking and Storage")]
    public bool isStackable = true;
    public int maxStackSize = 1000;
    
    [Header("Visual Properties")]
    public Color displayColor = Color.white;
    public Color resourceColor = Color.white; // Alternative name for compatibility
    public Sprite icon;
    
    [Header("Processing Properties")]
    public bool canBeProcessed = true;
    public bool isRenewable = false;
    public float processingDifficulty = 1f;
    public bool requiresSpecialFacility = false;
    public string requiredFacilityType = "";
    public bool isProcessedResource = false; // Whether this resource is created through processing
    
    [Header("Special Properties")]
    public bool isHazardous = false;
    public bool requiresSpecialStorage = false;
    public float decayRate = 0f; // Resources per second lost to decay
    public int storageWeight = 1;
    public bool canDecay = false;
    
    // Ensure color consistency
    private void OnValidate()
    {
        // Keep both color fields in sync for compatibility
        if (displayColor != resourceColor)
        {
            resourceColor = displayColor;
        }
    }
    
    /// <summary>
    /// Get the effective value of this resource considering quantity
    /// </summary>
    /// <param name="quantity">Quantity of the resource</param>
    /// <returns>Total value</returns>
    public int GetTotalValue(int quantity)
    {
        return baseValue * quantity;
    }
    
    /// <summary>
    /// Check if this resource can stack with another
    /// </summary>
    /// <param name="other">Other resource config</param>
    /// <returns>True if they can stack</returns>
    public bool CanStackWith(ResourceConfig other)
    {
        if (other == null) return false;
        return isStackable && other.isStackable && resourceType == other.resourceType;
    }
    
    /// <summary>
    /// Get the maximum quantity that can be added to a stack
    /// </summary>
    /// <param name="currentQuantity">Current stack quantity</param>
    /// <returns>Maximum additional quantity</returns>
    public int GetMaxAddableQuantity(int currentQuantity)
    {
        if (!isStackable) return currentQuantity >= 1 ? 0 : 1;
        return Mathf.Max(0, maxStackSize - currentQuantity);
    }
    
    /// <summary>
    /// Calculate decay amount for a given time period
    /// </summary>
    /// <param name="deltaTime">Time period in seconds</param>
    /// <param name="currentQuantity">Current quantity</param>
    /// <returns>Amount lost to decay</returns>
    public int CalculateDecay(float deltaTime, int currentQuantity)
    {
        if (!canDecay || decayRate <= 0f || currentQuantity <= 0) return 0;
        
        float decayAmount = decayRate * deltaTime;
        return Mathf.Min(currentQuantity, Mathf.FloorToInt(decayAmount));
    }
    
    /// <summary>
    /// Calculate the effective value considering various factors
    /// </summary>
    /// <param name="quantity">Quantity of the resource</param>
    /// <returns>Effective value</returns>
    public float CalculateEffectiveValue(int quantity)
    {
        float value = GetTotalValue(quantity);
        
        // Apply rarity multiplier
        float rarityMultiplier = rarity switch
        {
            ResourceRarity.Common => 1.0f,
            ResourceRarity.Uncommon => 1.2f,
            ResourceRarity.Rare => 1.5f,
            ResourceRarity.Epic => 2.0f,
            ResourceRarity.Legendary => 3.0f,
            _ => 1.0f
        };
        
        return value * rarityMultiplier;
    }
    
    /// <summary>
    /// Get the display color for this resource
    /// </summary>
    /// <returns>Color for UI display</returns>
    public Color GetDisplayColor()
    {
        return displayColor;
    }
    
    /// <summary>
    /// Get formatted display text for this resource
    /// </summary>
    /// <param name="quantity">Quantity to display</param>
    /// <param name="showValue">Whether to show value information</param>
    /// <returns>Formatted display string</returns>
    public string GetDisplayText(int quantity, bool showValue = false)
    {
        string text = $"{displayName}";
        
        if (quantity > 1 || !isStackable)
        {
            text += $" x{quantity}";
        }
        
        if (showValue && baseValue > 0)
        {
            text += $" (Value: {GetTotalValue(quantity)})";
        }
        
        return text;
    }
    
    /// <summary>
    /// Get a detailed description including special properties
    /// </summary>
    /// <returns>Detailed description string</returns>
    public string GetDetailedDescription()
    {
        string details = description;
        
        if (isHazardous)
        {
            details += "\n⚠️ HAZARDOUS MATERIAL";
        }
        
        if (requiresSpecialStorage)
        {
            details += "\n🔒 Requires Special Storage";
        }
        
        if (requiresSpecialFacility && !string.IsNullOrEmpty(requiredFacilityType))
        {
            details += $"\n🏭 Requires {requiredFacilityType}";
        }
        
        if (canDecay && decayRate > 0f)
        {
            details += $"\n⏰ Decays at {decayRate}/sec";
        }
        
        if (!isRenewable)
        {
            details += "\n♻️ Non-renewable Resource";
        }
        
        if (storageWeight > 1)
        {
            details += $"\n⚖️ Storage Weight: {storageWeight}";
        }
        
        return details;
    }
    
    /// <summary>
    /// Validate this resource configuration
    /// </summary>
    /// <param name="errorMessage">Output error message if validation fails</param>
    /// <returns>True if valid</returns>
    public bool Validate(out string errorMessage)
    {
        errorMessage = "";
        
        if (resourceType == ResourceType.None)
        {
            errorMessage = "Resource type cannot be None";
            return false;
        }
        
        if (string.IsNullOrEmpty(displayName))
        {
            errorMessage = "Display name cannot be empty";
            return false;
        }
        
        if (baseValue < 0)
        {
            errorMessage = "Base value cannot be negative";
            return false;
        }
        
        if (isStackable && maxStackSize <= 0)
        {
            errorMessage = "Max stack size must be positive for stackable resources";
            return false;
        }
        
        if (decayRate < 0f)
        {
            errorMessage = "Decay rate cannot be negative";
            return false;
        }
        
        if (processingDifficulty < 0f)
        {
            errorMessage = "Processing difficulty cannot be negative";
            return false;
        }
        
        if (storageWeight < 1)
        {
            errorMessage = "Storage weight must be at least 1";
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Create a copy of this resource configuration
    /// </summary>
    /// <returns>New ResourceConfig instance with copied values</returns>
    public ResourceConfig Clone()
    {
        ResourceConfig clone = CreateInstance<ResourceConfig>();
        clone.resourceType = this.resourceType;
        clone.displayName = this.displayName;
        clone.description = this.description;
        clone.baseValue = this.baseValue;
        clone.category = this.category;
        clone.rarity = this.rarity;
        clone.isStackable = this.isStackable;
        clone.maxStackSize = this.maxStackSize;
        clone.displayColor = this.displayColor;
        clone.resourceColor = this.resourceColor;
        clone.icon = this.icon;
        clone.canBeProcessed = this.canBeProcessed;
        clone.isRenewable = this.isRenewable;
        clone.processingDifficulty = this.processingDifficulty;
        clone.requiresSpecialFacility = this.requiresSpecialFacility;
        clone.requiredFacilityType = this.requiredFacilityType;
        clone.isProcessedResource = this.isProcessedResource;
        clone.isHazardous = this.isHazardous;
        clone.requiresSpecialStorage = this.requiresSpecialStorage;
        clone.decayRate = this.decayRate;
        clone.storageWeight = this.storageWeight;
        clone.canDecay = this.canDecay;
        
        return clone;
    }
}