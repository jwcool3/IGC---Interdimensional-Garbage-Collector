using UnityEngine;

/// <summary>
/// Centralized enumeration definitions for the resource management system
/// Contains all enums used across different components to ensure consistency
/// </summary>

/// <summary>
/// Types of resources that can be collected and processed
/// </summary>
public enum ResourceType
{
    // Basic Materials
    Plastic = 0,
    MetalScraps = 1,
    OrganicMatter = 2,
    Glass = 3,
    Fabric = 4,
    Paper = 5,
    
    // Advanced Materials
    Electronics = 6,
    RareMetals = 7,
    Crystals = 8,
    Biomass = 9,
    
    // Exotic Materials
    Nanomaterials = 10,
    QuantumMatter = 11,
    
    // Special Resources
    Energy = 12,
    Information = 13,
    
    // Waste-specific
    ElectronicWaste = 14,
    ToxicWaste = 15
}

/// <summary>
/// Categories for organizing resources
/// </summary>
public enum ResourceCategory
{
    BasicMaterials,
    AdvancedMaterials,
    ExoticMaterials,
    Energy,
    Information,
    Waste
}

/// <summary>
/// Rarity levels for waste items
/// </summary>
public enum WasteRarity
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    Epic = 3,
    Legendary = 4
}

/// <summary>
/// Types of waste that can be collected
/// Maps to different processing methods and resource yields
/// </summary>
public enum WasteType
{
    // Organic waste
    Organic = 0,
    Food = 1,
    Biological = 2,
    
    // Manufactured materials
    Plastic = 3,
    Metals = 4,
    Glass = 5,
    Paper = 6,
    Fabric = 7,
    
    // Technology waste
    Electronics = 8,
    Circuits = 9,
    Batteries = 10,
    
    // Hazardous materials
    Chemical = 11,
    Radioactive = 12,
    Toxic = 13,
    
    // Exotic waste
    Dimensional = 14,
    Quantum = 15,
    Crystalline = 16,
    
    // Mixed/Unknown
    Mixed = 17,
    Unknown = 18
}

/// <summary>
/// Origins where waste can be collected from
/// Affects resource yield bonuses and contamination levels
/// </summary>
public enum WasteOrigin
{
    // Residential areas
    Residential = 0,
    Suburban = 1,
    Urban = 2,
    
    // Commercial areas
    Commercial = 3,
    Office = 4,
    Retail = 5,
    Restaurant = 6,
    
    // Industrial areas
    Industrial = 7,
    Manufacturing = 8,
    Chemical = 9,
    Electronic = 10,
    
    // Institutional
    Hospital = 11,
    School = 12,
    Laboratory = 13,
    
    // Special locations
    SpaceStation = 14,
    AlienFacility = 15,
    DimensionalRift = 16,
    QuantumLab = 17,
    
    // Natural/Outdoor
    Park = 18,
    Beach = 19,
    Forest = 20,
    
    // Unknown/Mixed
    Unknown = 21,
    Mixed = 22
}

/// <summary>
/// Types of processing methods available
/// </summary>
public enum ProcessingType
{
    // Basic processing
    Sorting = 0,
    Cleaning = 1,
    Shredding = 2,
    Melting = 3,
    
    // Advanced processing
    Chemical = 4,
    Biological = 5,
    Thermal = 6,
    Mechanical = 7,
    
    // Specialized processing
    Electronic = 8,
    Quantum = 9,
    Dimensional = 10,
    Nano = 11,
    
    // Combination methods
    Hybrid = 12,
    Sequential = 13,
    Parallel = 14
}

/// <summary>
/// Categories for processing recipes
/// </summary>
public enum RecipeCategory
{
    BasicRecycling,
    AdvancedProcessing,
    SpecializedConversion,
    ExoticTransformation,
    WasteToEnergy,
    MaterialPurification,
    ComponentExtraction,
    HazardousWasteHandling
}

/// <summary>
/// Difficulty levels for processing recipes
/// </summary>
public enum RecipeDifficulty
{
    Trivial = 0,
    Easy = 1,
    Normal = 2,
    Hard = 3,
    Expert = 4,
    Master = 5
}

/// <summary>
/// Processing facility types
/// </summary>
public enum FacilityType
{
    BasicRecycler,
    AdvancedProcessor,
    SpecializedUnit,
    HybridSystem,
    QuantumProcessor,
    DimensionalConverter
}

/// <summary>
/// Processing job status
/// </summary>
public enum ProcessingStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Cancelled,
    OnHold
}

/// <summary>
/// Resource distribution preferences for conversion
/// </summary>
public enum ResourceDistributionPreference
{
    Balanced,
    HighValue,
    Quantity,
    Rarity,
    Specific
}

/// <summary>
/// Integration phases for legacy system migration
/// </summary>
public enum IntegrationPhase
{
    PreMigration,
    DataAnalysis,
    ResourceMigration,
    InventoryMigration,
    SystemValidation,
    PostMigrationCleanup,
    Complete
}

/// <summary>
/// System validation severity levels
/// </summary>
public enum ValidationSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Event bridge status
/// </summary>
public enum EventBridgeStatus
{
    Disabled,
    Initializing,
    Active,
    Error,
    Paused
}

/// <summary>
/// System coordinator status
/// </summary>
public enum SystemStatus
{
    Uninitialized,
    Initializing,
    Ready,
    Running,
    Error,
    Paused,
    Shutdown
}

/// <summary>
/// Waste sorting criteria
/// </summary>
public enum WasteSortCriteria
{
    Quality,
    Type,
    Origin,
    AddedTime,
    Value,
    Contamination,
    Rarity,
    ProcessingPotential
} 