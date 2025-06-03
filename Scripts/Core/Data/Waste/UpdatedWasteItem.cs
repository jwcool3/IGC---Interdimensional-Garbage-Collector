using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Enhanced waste item class with improved processing capabilities
/// Replaces the basic WasteItem with more detailed properties and methods
/// </summary>
[System.Serializable]
public class UpdatedWasteItem
{
    [Header("Basic Properties")]
    public string Id;
    public string Name;
    [TextArea(2, 3)]
    public string Description;
    public WasteRarity Rarity = WasteRarity.Common;
    public WasteType Type = WasteType.None;
    public int Quantity = 1;
    public Sprite Icon;
    
    [Header("Processing Properties")]
    public float ProcessingTime = 1f;
    public float ContaminationLevel = 0f;
    public float DimensionalStability = 1f;
    public WasteCondition Condition = WasteCondition.Good;
    public WasteOrigin Origin = WasteOrigin.Unknown;
    
    [Header("Resource Yield")]
    public ResourceYield resourceYield;
    
    [Header("Special Properties")]
    public bool IsHazardous = false;
    public bool RequiresSpecialHandling = false;
    public float DecayRate = 0f;
    public List<ContaminationType> ContaminationTypes = new List<ContaminationType>();
    
    [Header("Metadata")]
    public float TimeAdded;
    public string DimensionalOrigin = "Unknown";
    public Dictionary<string, object> CustomProperties = new Dictionary<string, object>();
    
    // Additional properties for inventory management
    public float Weight = 1f;
    public UpdatedWasteItemData ItemData;
    
    // Tracking field for initialization
    private bool propertiesInitialized = false;
    
    // Calculated properties
    public float ProcessingComplexity => CalculateProcessingComplexity();
    public float BaseValue => CalculateBaseValue();
    public float TotalValue => BaseValue * Quantity;
    public bool CanBeProcessed => !IsHazardous || RequiresSpecialHandling;
    
    // Quality property based on condition and contamination
    public float Quality
    {
        get
        {
            float conditionMultiplier = Condition switch
            {
                WasteCondition.Pristine => 1.0f,
                WasteCondition.Good => 0.8f,
                WasteCondition.Damaged => 0.6f,
                WasteCondition.Deteriorated => 0.4f,
                WasteCondition.Corrupted => 0.2f,
                _ => 0.5f
            };
            
            float contaminationPenalty = ContaminationLevel * 0.5f;
            return Mathf.Clamp01(conditionMultiplier - contaminationPenalty);
        }
    }
    
    // Estimated value for inventory sorting
    public float EstimatedValue
    {
        get
        {
            float rarityMultiplier = Rarity switch
            {
                WasteRarity.Common => 1f,
                WasteRarity.Uncommon => 2f,
                WasteRarity.Rare => 5f,
                WasteRarity.Epic => 10f,
                WasteRarity.Legendary => 25f,
                _ => 1f
            };
            
            return BaseValue * Quality * rarityMultiplier;
        }
    }
    
    // Recycling potential for processing
    public float RecyclingPotential
    {
        get
        {
            float basePotential = Type switch
            {
                WasteType.Metal => 0.9f,
                WasteType.Plastic => 0.7f,
                WasteType.Electronics => 0.8f,
                WasteType.Glass => 0.6f,
                WasteType.Paper => 0.5f,
                WasteType.Organic => 0.4f,
                WasteType.Chemical => 0.3f,
                _ => 0.5f
            };
            
            return basePotential * Quality;
        }
    }
    
    // Backward compatibility property
    public float WasteStability
    {
        get => DimensionalStability;
        set => DimensionalStability = value;
    }
    
    /// <summary>
    /// Resource yields dictionary for compatibility with existing code
    /// </summary>
    public Dictionary<ResourceType, ResourceYield> ResourceYields
    {
        get
        {
            var yields = new Dictionary<ResourceType, ResourceYield>();
            
            if (resourceYield != null)
            {
                // Add primary resources
                if (resourceYield.primaryResources != null)
                {
                    foreach (var primary in resourceYield.primaryResources)
                    {
                        if (!yields.ContainsKey(primary.type))
                        {
                            yields[primary.type] = new ResourceYield
                            {
                                resourceType = primary.type,
                                baseAmount = primary.amount,
                                chancePercentage = 100f
                            };
                        }
                    }
                }
                
                // Add secondary resources
                if (resourceYield.secondaryResources != null)
                {
                    foreach (var secondary in resourceYield.secondaryResources)
                    {
                        if (!yields.ContainsKey(secondary.type))
                        {
                            yields[secondary.type] = new ResourceYield
                            {
                                resourceType = secondary.type,
                                baseAmount = secondary.amount,
                                chancePercentage = secondary.chance * 100f
                            };
                        }
                    }
                }
                
                // If no primary/secondary resources, use the legacy single resource
                if (yields.Count == 0 && resourceYield.resourceType != ResourceType.None)
                {
                    yields[resourceYield.resourceType] = resourceYield;
                }
            }
            
            return yields;
        }
    }
    
    /// <summary>
    /// Legacy ResourceYield property for backward compatibility
    /// </summary>
    public ResourceYield ResourceYield
    {
        get => resourceYield;
        set => resourceYield = value;
    }
    
    /// <summary>
    /// Constructor for creating a new UpdatedWasteItem
    /// </summary>
    public UpdatedWasteItem(string name, WasteType type, int quantity = 1)
    {
        Id = System.Guid.NewGuid().ToString("N")[..8];
        Name = name;
        Type = type;
        Quantity = quantity;
        TimeAdded = GetSafeTime();
        
        // Initialize default resource yield
        resourceYield = new ResourceYield();
        SetupDefaultYield();
    }
    
    /// <summary>
    /// Constructor for backward compatibility (legacy format)
    /// </summary>
    public UpdatedWasteItem(string name, string dimensionalOrigin, WasteRarity rarity, int quantity = 1)
    {
        Id = System.Guid.NewGuid().ToString("N")[..8];
        Name = name;
        DimensionalOrigin = dimensionalOrigin;
        Rarity = rarity;
        Quantity = quantity;
        TimeAdded = GetSafeTime();
        
        // Map dimensional origin to waste type
        Type = MapDimensionalOriginToType(dimensionalOrigin);
        
        // Initialize default resource yield
        resourceYield = new ResourceYield();
        SetupDefaultYield();
    }
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public UpdatedWasteItem()
    {
        Id = System.Guid.NewGuid().ToString("N")[..8];
        Name = "Unknown Waste";
        Type = WasteType.Unknown;
        Quantity = 1;
        TimeAdded = GetSafeTime();
        resourceYield = new ResourceYield();
    }
    
    /// <summary>
    /// Map dimensional origin to waste type for backward compatibility
    /// </summary>
    private static WasteType MapDimensionalOriginToType(string dimensionalOrigin)
    {
        if (string.IsNullOrEmpty(dimensionalOrigin))
            return WasteType.Unknown;
            
        string origin = dimensionalOrigin.ToLower();
        
        if (origin.Contains("technological") || origin.Contains("tech"))
            return WasteType.Electronics;
        if (origin.Contains("biological") || origin.Contains("bio") || origin.Contains("organic"))
            return WasteType.Organic;
        if (origin.Contains("metal") || origin.Contains("industrial"))
            return WasteType.Metal;
        if (origin.Contains("chemical") || origin.Contains("toxic"))
            return WasteType.Chemical;
        if (origin.Contains("plastic") || origin.Contains("polymer"))
            return WasteType.Plastic;
        if (origin.Contains("glass") || origin.Contains("crystal"))
            return WasteType.Glass;
        
        return WasteType.Unknown;
    }
    
    /// <summary>
    /// Create from existing WasteItem (for migration)
    /// </summary>
    public static UpdatedWasteItem FromWasteItem(WasteItem oldItem)
    {
        var newItem = new UpdatedWasteItem
        {
            Id = oldItem.Id,
            Name = oldItem.Name,
            Description = oldItem.Description,
            Rarity = oldItem.Rarity,
            Quantity = oldItem.Quantity,
            Icon = oldItem.Icon,
            ContaminationLevel = oldItem.ContaminationLevel,
            DimensionalStability = oldItem.DimensionalStability,
            DimensionalOrigin = oldItem.DimensionalOrigin,
            TimeAdded = GetSafeTime()
        };
        
        // Map old waste type to new system
        newItem.Type = MapLegacyWasteType(oldItem);
        
        // Set up processing properties based on old item
        newItem.ProcessingTime = CalculateLegacyProcessingTime(oldItem);
        newItem.SetupDefaultYield();
        
        return newItem;
    }
    
    private static WasteType MapLegacyWasteType(WasteItem oldItem)
    {
        // Map based on name or other properties
        string name = oldItem.Name.ToLower();
        
        if (name.Contains("metal") || name.Contains("scrap")) return WasteType.Metal;
        if (name.Contains("plastic") || name.Contains("polymer")) return WasteType.Plastic;
        if (name.Contains("organic") || name.Contains("bio")) return WasteType.Organic;
        if (name.Contains("electronic") || name.Contains("circuit")) return WasteType.Electronics;
        if (name.Contains("glass")) return WasteType.Glass;
        if (name.Contains("paper") || name.Contains("cardboard")) return WasteType.Paper;
        if (name.Contains("chemical") || name.Contains("toxic")) return WasteType.Chemical;
        
        return WasteType.Unknown;
    }
    
    private static float CalculateLegacyProcessingTime(WasteItem oldItem)
    {
        float baseTime = 1f;
        
        // Adjust based on rarity
        switch (oldItem.Rarity)
        {
            case WasteRarity.Common: baseTime = 1f; break;
            case WasteRarity.Uncommon: baseTime = 1.5f; break;
            case WasteRarity.Rare: baseTime = 2f; break;
            case WasteRarity.Epic: baseTime = 3f; break;
            case WasteRarity.Legendary: baseTime = 5f; break;
        }
        
        // Adjust for contamination
        baseTime *= (1f + oldItem.ContaminationLevel);
        
        return baseTime;
    }
    
    /// <summary>
    /// Set up default resource yield based on waste type and properties
    /// </summary>
    private void SetupDefaultYield()
    {
        var primaryResources = new List<ResourceAmount>();
        var secondaryResources = new List<ResourceChance>();
        
        // Set primary resources based on waste type
        switch (Type)
        {
            case WasteType.Metal:
                primaryResources.Add(new ResourceAmount(ResourceType.MetalScraps, GetBaseYieldAmount()));
                secondaryResources.Add(new ResourceChance(ResourceType.RareMetals, 1, 0.1f));
                break;
                
            case WasteType.Plastic:
                primaryResources.Add(new ResourceAmount(ResourceType.Plastic, GetBaseYieldAmount()));
                secondaryResources.Add(new ResourceChance(ResourceType.Polymer, 1, 0.15f));
                break;
                
            case WasteType.Organic:
                primaryResources.Add(new ResourceAmount(ResourceType.OrganicMatter, GetBaseYieldAmount()));
                secondaryResources.Add(new ResourceChance(ResourceType.Biomass, 1, 0.2f));
                break;
                
            case WasteType.Electronics:
                primaryResources.Add(new ResourceAmount(ResourceType.Electronics, GetBaseYieldAmount()));
                secondaryResources.Add(new ResourceChance(ResourceType.RareMetals, 1, 0.25f));
                secondaryResources.Add(new ResourceChance(ResourceType.CrystalFragments, 1, 0.1f));
                break;
                
            case WasteType.Chemical:
                primaryResources.Add(new ResourceAmount(ResourceType.Chemical, GetBaseYieldAmount()));
                secondaryResources.Add(new ResourceChance(ResourceType.ToxicSludge, 1, 0.3f));
                break;
                
            default:
                primaryResources.Add(new ResourceAmount(ResourceType.RecyclingPoints, GetBaseYieldAmount()));
                break;
        }
        
        resourceYield.primaryResources = primaryResources.ToArray();
        resourceYield.secondaryResources = secondaryResources.ToArray();
        resourceYield.contaminationRisk = ContaminationLevel;
    }
    
    private int GetBaseYieldAmount()
    {
        int baseAmount = 1;
        
        // Adjust based on rarity
        switch (Rarity)
        {
            case WasteRarity.Common: baseAmount = 1; break;
            case WasteRarity.Uncommon: baseAmount = 2; break;
            case WasteRarity.Rare: baseAmount = 3; break;
            case WasteRarity.Epic: baseAmount = 5; break;
            case WasteRarity.Legendary: baseAmount = 8; break;
        }
        
        // Adjust based on condition
        switch (Condition)
        {
            case WasteCondition.Pristine: baseAmount = Mathf.RoundToInt(baseAmount * 1.5f); break;
            case WasteCondition.Good: baseAmount = baseAmount; break;
            case WasteCondition.Damaged: baseAmount = Mathf.RoundToInt(baseAmount * 0.8f); break;
            case WasteCondition.Deteriorated: baseAmount = Mathf.RoundToInt(baseAmount * 0.6f); break;
            case WasteCondition.Corrupted: baseAmount = Mathf.RoundToInt(baseAmount * 0.4f); break;
        }
        
        return Mathf.Max(1, baseAmount);
    }
    
    /// <summary>
    /// Calculate processing complexity based on various factors
    /// </summary>
    private float CalculateProcessingComplexity()
    {
        float complexity = 1f;
        
        // Base complexity by type
        switch (Type)
        {
            case WasteType.Organic: complexity = 0.8f; break;
            case WasteType.Plastic: complexity = 1f; break;
            case WasteType.Metal: complexity = 1.2f; break;
            case WasteType.Electronics: complexity = 1.5f; break;
            case WasteType.Chemical: complexity = 2f; break;
            case WasteType.Hazardous: complexity = 2.5f; break;
            case WasteType.Radioactive: complexity = 3f; break;
        }
        
        // Modify by rarity
        complexity *= (1f + (int)Rarity * 0.2f);
        
        // Modify by contamination
        complexity *= (1f + ContaminationLevel);
        
        // Modify by condition
        switch (Condition)
        {
            case WasteCondition.Corrupted: complexity *= 1.5f; break;
            case WasteCondition.Deteriorated: complexity *= 1.2f; break;
        }
        
        return complexity;
    }
    
    /// <summary>
    /// Calculate base value of this waste item
    /// </summary>
    private float CalculateBaseValue()
    {
        float value = 1f;
        
        // Base value by type
        switch (Type)
        {
            case WasteType.Organic: value = 1f; break;
            case WasteType.Plastic: value = 1.2f; break;
            case WasteType.Metal: value = 1.5f; break;
            case WasteType.Electronics: value = 2f; break;
            case WasteType.Chemical: value = 1.8f; break;
            case WasteType.Glass: value = 0.8f; break;
            case WasteType.Paper: value = 0.6f; break;
        }
        
        // Multiply by rarity
        value *= (1f + (int)Rarity);
        
        // Adjust for condition
        switch (Condition)
        {
            case WasteCondition.Pristine: value *= 1.5f; break;
            case WasteCondition.Good: value *= 1f; break;
            case WasteCondition.Damaged: value *= 0.8f; break;
            case WasteCondition.Deteriorated: value *= 0.6f; break;
            case WasteCondition.Corrupted: value *= 0.4f; break;
        }
        
        return value;
    }
    
    /// <summary>
    /// Check if this item can stack with another
    /// </summary>
    public bool CanStackWith(UpdatedWasteItem other)
    {
        if (other == null) return false;
        
        return Name == other.Name &&
               Type == other.Type &&
               Rarity == other.Rarity &&
               Mathf.Approximately(ContaminationLevel, other.ContaminationLevel) &&
               Condition == other.Condition &&
               Origin == other.Origin;
    }
    
    /// <summary>
    /// Add quantity to this item (for stacking)
    /// </summary>
    public void AddQuantity(int amount)
    {
        Quantity += amount;
    }
    
    /// <summary>
    /// Remove quantity from this item
    /// </summary>
    public bool RemoveQuantity(int amount)
    {
        if (amount > Quantity) return false;
        
        Quantity -= amount;
        return true;
    }
    
    /// <summary>
    /// Set the quantity of this item
    /// </summary>
    public void SetQuantity(int newQuantity)
    {
        Quantity = Mathf.Max(0, newQuantity);
    }
    
    /// <summary>
    /// Create a copy of this waste item (alias for Clone for compatibility)
    /// </summary>
    public UpdatedWasteItem CreateCopy()
    {
        return Clone();
    }
    
    /// <summary>
    /// Split this item into a new item with specified quantity
    /// </summary>
    public UpdatedWasteItem Split(int splitQuantity)
    {
        if (splitQuantity >= Quantity || splitQuantity <= 0) return null;
        
        var newItem = Clone();
        newItem.Quantity = splitQuantity;
        newItem.Id = System.Guid.NewGuid().ToString("N")[..8];
        
        Quantity -= splitQuantity;
        
        return newItem;
    }
    
    /// <summary>
    /// Create a deep copy of this waste item
    /// </summary>
    public UpdatedWasteItem Clone()
    {
        var clone = new UpdatedWasteItem
        {
            Id = this.Id,
            Name = this.Name,
            Description = this.Description,
            Rarity = this.Rarity,
            Type = this.Type,
            Quantity = this.Quantity,
            Icon = this.Icon,
            ProcessingTime = this.ProcessingTime,
            ContaminationLevel = this.ContaminationLevel,
            DimensionalStability = this.DimensionalStability,
            Condition = this.Condition,
            Origin = this.Origin,
            IsHazardous = this.IsHazardous,
            RequiresSpecialHandling = this.RequiresSpecialHandling,
            DecayRate = this.DecayRate,
            TimeAdded = this.TimeAdded,
            DimensionalOrigin = this.DimensionalOrigin
        };
        
        // Deep copy collections
        clone.ContaminationTypes = new List<ContaminationType>(this.ContaminationTypes);
        clone.CustomProperties = new Dictionary<string, object>(this.CustomProperties);
        
        // Clone resource yield
        clone.resourceYield = new ResourceYield
        {
            primaryResources = (ResourceAmount[])this.resourceYield.primaryResources.Clone(),
            secondaryResources = (ResourceChance[])this.resourceYield.secondaryResources.Clone(),
            contaminationRisk = this.resourceYield.contaminationRisk
        };
        
        return clone;
    }
    
    /// <summary>
    /// Update item condition over time (decay)
    /// </summary>
    public void UpdateCondition(float deltaTime)
    {
        if (DecayRate > 0f)
        {
            float decay = DecayRate * deltaTime;
            
            // Increase contamination slightly
            ContaminationLevel = Mathf.Min(1f, ContaminationLevel + decay * 0.1f);
            
            // Potentially degrade condition
            if (UnityEngine.Random.value < decay * 0.01f)
            {
                DegradeCondition();
            }
        }
    }
    
    private void DegradeCondition()
    {
        switch (Condition)
        {
            case WasteCondition.Pristine:
                Condition = WasteCondition.Good;
                break;
            case WasteCondition.Good:
                Condition = WasteCondition.Damaged;
                break;
            case WasteCondition.Damaged:
                Condition = WasteCondition.Deteriorated;
                break;
            case WasteCondition.Deteriorated:
                Condition = WasteCondition.Corrupted;
                break;
        }
        
        // Recalculate yield when condition changes
        SetupDefaultYield();
    }
    
    /// <summary>
    /// Get a formatted description of this item
    /// </summary>
    public string GetFormattedDescription()
    {
        string desc = Description;
        
        if (string.IsNullOrEmpty(desc))
        {
            desc = $"A {Rarity.ToString().ToLower()} {Type.ToString().ToLower()} waste item";
        }
        
        if (IsHazardous)
        {
            desc += "\n⚠️ Hazardous material - requires special handling";
        }
        
        if (ContaminationLevel > 0.5f)
        {
            desc += "\n☢️ Highly contaminated";
        }
        
        return desc;
    }
    
    /// <summary>
    /// Get a preview of the resources this item will yield when processed
    /// </summary>
    /// <returns>A formatted string showing the resource yields</returns>
    public string GetResourcePreview()
    {
        if (resourceYield == null)
            return "No resources available";

        List<string> resourceStrings = new List<string>();

        // Add primary resources
        if (resourceYield.primaryResources != null)
        {
            foreach (var resource in resourceYield.primaryResources)
            {
                resourceStrings.Add($"{resource.amount} {resource.type}");
            }
        }

        // Add secondary resources with chance
        if (resourceYield.secondaryResources != null)
        {
            foreach (var resource in resourceYield.secondaryResources)
            {
                resourceStrings.Add($"{resource.amount} {resource.type} ({resource.chance:P0} chance)");
            }
        }

        return resourceStrings.Count > 0 ? string.Join(", ", resourceStrings) : "No resources available";
    }

    /// <summary>
    /// Initialize calculated properties if they haven't been set yet
    /// </summary>
    public void InitializeProperties()
    {
        if (!propertiesInitialized)
        {
            // Initialize dimensional stability if not set
            if (DimensionalStability <= 0)
            {
                DimensionalStability = CalculateInitialStability();
            }

            // Initialize contamination level if not set
            if (ContaminationLevel <= 0)
            {
                ContaminationLevel = CalculateInitialContamination();
            }

            // Initialize resource yield if not set
            if (resourceYield == null)
            {
                resourceYield = GenerateResourceYield();
            }

            propertiesInitialized = true;
        }
    }

    private float CalculateInitialStability()
    {
        float baseStability = 0.5f + ((int)Rarity * 0.1f);
        if (Application.isPlaying)
        {
            baseStability += UnityEngine.Random.Range(-0.1f, 0.1f);
        }
        return Mathf.Clamp01(baseStability);
    }

    private float CalculateInitialContamination()
    {
        float baseContamination = 0.5f - ((int)Rarity * 0.1f);
        if (Application.isPlaying)
        {
            baseContamination += UnityEngine.Random.Range(-0.1f, 0.1f);
        }
        return Mathf.Clamp01(baseContamination);
    }

    private ResourceYield GenerateResourceYield()
    {
        ResourceYield yield = new ResourceYield();
        
        // Generate primary resources based on waste type and rarity
        List<ResourceAmount> primaryResources = new List<ResourceAmount>();
        
        // Base resource amount increases with rarity
        int baseAmount = 1 + (int)Rarity;
        
        // Add type-specific primary resources
        switch (Type)
        {
            case WasteType.Plastic:
                primaryResources.Add(new ResourceAmount(ResourceType.Plastic, baseAmount));
                break;
            case WasteType.Metal:
                primaryResources.Add(new ResourceAmount(ResourceType.MetalScraps, baseAmount));
                break;
            case WasteType.Organic:
                primaryResources.Add(new ResourceAmount(ResourceType.OrganicMatter, baseAmount));
                break;
            case WasteType.Electronics:
                primaryResources.Add(new ResourceAmount(ResourceType.Parts, baseAmount));
                break;
            case WasteType.Hazardous:
                primaryResources.Add(new ResourceAmount(ResourceType.Energy, baseAmount));
                break;
            default:
                primaryResources.Add(new ResourceAmount(ResourceType.RecyclingPoints, baseAmount * 5));
                break;
        }
        
        yield.primaryResources = primaryResources.ToArray();
        
        // Generate secondary resources based on rarity
        List<ResourceChance> secondaryResources = new List<ResourceChance>();
        
        if (Rarity >= WasteRarity.Uncommon)
        {
            secondaryResources.Add(new ResourceChance(ResourceType.DimensionalPotential, 1, 0.3f));
        }
        
        if (Rarity >= WasteRarity.Rare)
        {
            secondaryResources.Add(new ResourceChance(ResourceType.CrystalFragments, 1, 0.2f));
        }
        
        if (Rarity >= WasteRarity.Epic)
        {
            secondaryResources.Add(new ResourceChance(ResourceType.RareMetals, 1, 0.15f));
        }
        
        if (Rarity == WasteRarity.Legendary)
        {
            secondaryResources.Add(new ResourceChance(ResourceType.QuantumMatter, 1, 0.1f));
        }
        
        yield.secondaryResources = secondaryResources.ToArray();
        
        // Set contamination risk based on contamination level
        yield.contaminationRisk = ContaminationLevel * 0.5f;
        
        return yield;
    }
    
    public override string ToString()
    {
        return $"{Name} ({Type}) x{Quantity} [{Rarity}]";
    }

    /// <summary>
    /// Get time safely without calling Unity API during serialization
    /// </summary>
    private static float GetSafeTime()
    {
        try
        {
            // Only call Time.time if we're in play mode and not during serialization
            if (Application.isPlaying && !Application.isEditor)
            {
                return Time.time;
            }
            else if (Application.isPlaying)
            {
                return Time.time;
            }
            else
            {
                // Use system time as fallback during edit mode or serialization
                return (float)(System.DateTime.Now - System.DateTime.Today).TotalSeconds;
            }
        }
        catch
        {
            // Fallback to system time if Unity API is not available
            return (float)(System.DateTime.Now - System.DateTime.Today).TotalSeconds;
        }
    }
}