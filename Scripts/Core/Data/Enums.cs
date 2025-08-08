// Enums.cs
using System;

public enum WasteRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

// Unified ResourceType enum that includes both legacy and new resource types
public enum ResourceType
{
    None,

    // Legacy Resources (from existing system)
    Plastic,        // Fuel, basic parts, insulation, storage containers
    MetalScraps,    // Hulls, compartments, weapon frames
    OrganicMatter,  // Biomass, food, weak fuel source
    CrystalFragments, // Energy conduits, rare upgrades, dimensional tools
    NeuralResidue,  // KP generation, scanner enhancements
    ToxicSludge,    // Experimental fuel, high risk/high reward
    Fuel,           // Processed resource for ship operations
    Food,           // Processed organic matter for crew
    Parts,          // Processed metal/plastic for upgrades
    Energy,         // Processed crystals/sludge for advanced systems

    // Legacy compatibility aliases
    Glass,
    Paper,
    Fabric,
    Electronics,
    RareMetals,
    Crystals,
    Biomass,
    Nanomaterials,
    QuantumMatter,
    Information,
    ElectronicWaste,
    ToxicWaste,

    // Basic Resources (new system)
    RecyclingPoints,
    DimensionalPotential,

    // Raw Materials (new system)
    ScrapMetal,
    RefinedMetal,
    Chemical,
    Cardboard,
    Textile,

    // Processed Materials (new system)
    CompressedMetal,
    CompressedPlastic,
    CompressedFiber,
    RefinedChemicals,
    PurifiedWater,

    // Advanced Materials (new system)
    AdvancedAlloy,
    Polymer,
    Composite,

    // Byproducts (new system)
    Ash,
    Slag,
    Waste,

    // Special Resources (new system)
    AlienTech,
    ShipParts,
    CombatData,

    // Energy Types (new system)
    ThermalEnergy,
    ElectricalEnergy,
    DimensionalEnergy,

    // Aliases for compatibility
    Metals = MetalScraps,
    Organic = OrganicMatter
}

// New enums for the updated processing system
public enum WasteType
{
    None,
    Organic,
    Plastic,
    Metal,
    Electronics,
    Glass,
    Paper,
    Cardboard,
    Textile,
    Chemical,
    Biological,
    Hazardous,
    Radioactive,
    Liquid,
    Gas,
    Composite,
    Unknown,
    // Additional waste types for enhanced system
    Crystalline,
    Dimensional,
    Quantum,
    Food,
    Toxic,
    Metals // Alias for Metal for compatibility
}

public enum ProcessingType
{
    None,
    Sorting,
    Recycling,
    Breakdown,
    Synthesis,
    Fabrication,
    Incineration,
    Compaction,
    Recipe,
    Purification,
    Refinement,
    Composting,
    Neutralization
}

public enum ContaminationType
{
    None,
    Chemical,
    Biological,
    Radioactive,
    Dimensional,
    Thermal,
    Electromagnetic,
    Unknown
}

public enum WasteOrigin
{
    Unknown,
    Residential,
    Industrial,
    Medical,
    Electronic,
    Hazardous,
    Alien,
    Dimensional,
    Military,
    Research,
    Agricultural,
    Commercial
}

public enum ProcessingStatus
{
    Queued,
    InProgress,
    Completed,
    Failed,
    Cancelled
}

public enum FacilityType
{
    Basic,
    Advanced,
    Specialized,
    Experimental
}

public enum UpgradeType
{
    Efficiency,
    Capacity,
    Speed,
    Quality,
    Safety,
    Automation,
    Specialization
}

public enum QualityLevel
{
    Poor,
    Fair,
    Good,
    Excellent,
    Perfect
}

public enum ProcessingPriority
{
    Low,
    Normal,
    High,
    Critical
}

public enum ResourceCategory
{
    Basic,
    Processed,
    Advanced,
    Special,
    Energy,
    Byproduct,
    // Legacy categories
    BasicMaterials,
    AdvancedMaterials,
    ExoticMaterials,
    Information,
    Waste,
    RawMaterial,
    Refined
}

public enum WasteCondition
{
    Pristine,
    Good,
    Damaged,
    Deteriorated,
    Corrupted
}

public enum DimensionalStability
{
    Stable,
    Fluctuating,
    Unstable,
    Critical,
    Collapsing
}

public enum ProcessingComplexity
{
    Simple,
    Moderate,
    Complex,
    Advanced,
    Experimental
}

public enum SafetyLevel
{
    Safe,
    Caution,
    Warning,
    Danger,
    Critical
}

public enum AutomationLevel
{
    Manual,
    SemiAutomatic,
    Automatic,
    FullyAutomated,
    AIControlled
}

public enum EfficiencyRating
{
    Poor,
    Below_Average,
    Average,
    Above_Average,
    Excellent,
    Optimal
}

public enum ProcessingMethod
{
    Direct,
    Batch,
    Continuous,
    Hybrid
}

public enum YieldType
{
    Primary,
    Secondary,
    Bonus,
    Byproduct
}

public enum RecipeCategory
{
    Basic,
    Intermediate,
    Advanced,
    Experimental,
    Alien,
    Dimensional,
    // Legacy categories
    BasicRecycling,
    AdvancedProcessing,
    SpecializedConversion,
    ExoticTransformation,
    WasteToEnergy,
    MaterialPurification,
    ComponentExtraction,
    HazardousWasteHandling
}

public enum UnlockCondition
{
    None,
    Level,
    Research,
    Discovery,
    Achievement,
    Resource,
    Facility
}

// Additional enums for compatibility
public enum ResourceRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum RecipeDifficulty
{
    Trivial = 0,
    Easy = 1,
    Normal = 2,
    Hard = 3,
    Expert = 4,
    Master = 5
}

public enum ResourceDistributionPreference
{
    Balanced,
    HighValue,
    Quantity,
    Rarity,
    Specific
}

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

public enum ValidationSeverity
{
    Info,
    Warning,
    Error,
    Critical
}


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