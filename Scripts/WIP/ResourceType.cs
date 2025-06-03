using UnityEngine;
using System;

/// <summary>
/// Defines the different types of resources that can be obtained from waste
/// </summary>
public enum ResourceType
{
    Plastic,        // Fuel, basic parts, insulation, storage containers
    MetalScraps,    // Hulls, compartments, weapon frames
    OrganicMatter,  // Biomass, food, weak fuel source
    CrystalFragments, // Energy conduits, rare upgrades, dimensional tools
    NeuralResidue,  // KP generation, scanner enhancements
    ToxicSludge,    // Experimental fuel, high risk/high reward
    Fuel,           // Processed resource for ship operations
    Food,           // Processed organic matter for crew
    Parts,          // Processed metal/plastic for upgrades
    Energy          // Processed crystals/sludge for advanced systems
}

/// <summary>
/// Data container for a specific resource amount
/// </summary>
[Serializable]
public class ResourceAmount
{
    public ResourceType type;
    public int amount;
    
    public ResourceAmount(ResourceType type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
}

/// <summary>
/// Resource yield from breaking down waste items
/// </summary>
[Serializable]
public class ResourceYield
{
    [Header("Primary Resources")]
    public ResourceAmount[] primaryResources;
    
    [Header("Secondary Resources (Chance-based)")]
    public ResourceChance[] secondaryResources;
    
    [Header("Contamination Risk")]
    [Range(0f, 1f)]
    public float contaminationRisk = 0.1f;
    
    public ResourceYield()
    {
        primaryResources = new ResourceAmount[0];
        secondaryResources = new ResourceChance[0];
    }
}

[Serializable]
public class ResourceChance
{
    public ResourceType type;
    public int amount;
    [Range(0f, 1f)]
    public float chance;
    
    public ResourceChance(ResourceType type, int amount, float chance)
    {
        this.type = type;
        this.amount = amount;
        this.chance = chance;
    }
}

/// <summary>
/// ScriptableObject to define resource properties and relationships
/// </summary>
[CreateAssetMenu(fileName = "New Resource Config", menuName = "Resources/Resource Configuration")]
public class ResourceConfig : ScriptableObject
{
    [Header("Resource Identity")]
    public ResourceType resourceType;
    public string displayName;
    public string description;
    public Sprite icon;
    public Color resourceColor = Color.white;
    
    [Header("Game Balance")]
    public int baseValue = 1; // Internal weighting for balancing
    public bool isProcessedResource = false; // Fuel, Food, Parts, Energy
    public bool canBeStoredLongTerm = true;
    
    [Header("Processing Information")]
    [Tooltip("What this resource can be converted into")]
    public ProcessingRecipe[] processingOptions;
    
    [Header("Usage Categories")]
    public ResourceUsage[] usageTypes;
}

[Serializable]
public class ProcessingRecipe
{
    public string recipeName;
    public ResourceAmount[] inputs;
    public ResourceAmount[] outputs;
    public float processingTime = 1f;
    public string requiredFacility; // Which compartment/facility is needed
}

[Serializable]
public class ResourceUsage
{
    public string usageCategory; // "Fuel", "Construction", "Food", etc.
    public string description;
    public int efficiencyRating; // How good this resource is for this purpose
}