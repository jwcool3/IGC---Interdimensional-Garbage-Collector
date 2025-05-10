using UnityEngine;
using System;

/// <summary>
/// Defines the different types of compartments that can exist on the player's ship.
/// Each compartment type provides different bonuses and functionality.
/// </summary>
public enum CompartmentType
{
    /// <summary>
    /// Engine compartment - Improves probe speed and collection rate
    /// </summary>
    [Tooltip("Improves probe speed and collection rates")]
    Engine = 0,
    
    /// <summary>
    /// Laboratory compartment - Improves recycling efficiency and research
    /// </summary>
    [Tooltip("Improves recycling efficiency and research")]
    Lab = 1,
    
    /// <summary>
    /// Storage compartment - Increases waste inventory capacity
    /// </summary>
    [Tooltip("Increases waste inventory capacity")]
    Storage = 2,
    
    /// <summary>
    /// Bridge compartment - Improves location discovery and navigation
    /// </summary>
    [Tooltip("Improves location discovery and navigation")]
    Bridge = 3,
    
    /// <summary>
    /// Recycling compartment - Reduces contamination from processing waste
    /// </summary>
    [Tooltip("Reduces contamination from processing waste")]
    Recycling = 4,
    
    /// <summary>
    /// Stabilizer compartment - Improves waste stability, increasing value
    /// </summary>
    [Tooltip("Improves waste stability, increasing value")]
    Stabilizer = 5,
    
    /// <summary>
    /// Communications compartment - Increases rare waste chance
    /// </summary>
    [Tooltip("Increases rare waste chance")]
    Communications = 6,
    
    /// <summary>
    /// Scanner compartment - Reveals hidden properties of waste items
    /// </summary>
    [Tooltip("Reveals hidden properties of waste items")]
    Scanner = 7
}

/// <summary>
/// Extension methods for CompartmentType to provide additional functionality
/// </summary>
public static class CompartmentTypeExtensions
{
    /// <summary>
    /// Get a user-friendly display name for a compartment type
    /// </summary>
    /// <param name="type">The compartment type</param>
    /// <returns>A formatted display name</returns>
    public static string GetDisplayName(this CompartmentType type)
    {
        switch (type)
        {
            case CompartmentType.Engine:
                return "Engine Room";
            case CompartmentType.Lab:
                return "Research Laboratory";
            case CompartmentType.Storage:
                return "Storage Bay";
            case CompartmentType.Bridge:
                return "Command Bridge";
            case CompartmentType.Recycling:
                return "Recycling Center";
            case CompartmentType.Stabilizer:
                return "Dimensional Stabilizer";
            case CompartmentType.Communications:
                return "Comms Array";
            case CompartmentType.Scanner:
                return "Matter Scanner";
            default:
                return type.ToString();
        }
    }
    
    /// <summary>
    /// Get a description of what this compartment type does
    /// </summary>
    /// <param name="type">The compartment type</param>
    /// <returns>A detailed description</returns>
    public static string GetDescription(this CompartmentType type)
    {
        switch (type)
        {
            case CompartmentType.Engine:
                return "Powers the ship's systems and improves the speed and efficiency of waste collection probes.";
                
            case CompartmentType.Lab:
                return "Advanced research facility that increases the efficiency of waste recycling processes, yielding more resources.";
                
            case CompartmentType.Storage:
                return "Specialized containment system that increases the amount of interdimensional waste you can store.";
                
            case CompartmentType.Bridge:
                return "Command center for navigation and operations, improving the discovery of new dimensional locations.";
                
            case CompartmentType.Recycling:
                return "State-of-the-art processing facility that reduces contamination when recycling waste materials.";
                
            case CompartmentType.Stabilizer:
                return "Reality anchoring technology that improves the stability of collected waste, increasing its value.";
                
            case CompartmentType.Communications:
                return "Long-range dimensional scanner that increases the chances of finding rare and valuable waste types.";
                
            case CompartmentType.Scanner:
                return "Advanced analysis equipment that reveals hidden properties and potential of collected waste items.";
                
            default:
                return "Unknown compartment type.";
        }
    }
    
    /// <summary>
    /// Get the base recycling point cost for upgrading this compartment type
    /// </summary>
    /// <param name="type">The compartment type</param>
    /// <returns>Base RP cost</returns>
    public static float GetBaseRPCost(this CompartmentType type)
    {
        switch (type)
        {
            case CompartmentType.Engine:
                return 100f;
            case CompartmentType.Lab:
                return 150f;
            case CompartmentType.Storage:
                return 125f;
            case CompartmentType.Bridge:
                return 200f;
            case CompartmentType.Recycling:
                return 175f;
            case CompartmentType.Stabilizer:
                return 225f;
            case CompartmentType.Communications:
                return 150f;
            case CompartmentType.Scanner:
                return 175f;
            default:
                return 100f;
        }
    }
    
    /// <summary>
    /// Get the base dimensional potential cost for upgrading this compartment type
    /// </summary>
    /// <param name="type">The compartment type</param>
    /// <returns>Base DP cost</returns>
    public static float GetBaseDPCost(this CompartmentType type)
    {
        switch (type)
        {
            case CompartmentType.Engine:
                return 10f;
            case CompartmentType.Lab:
                return 15f;
            case CompartmentType.Storage:
                return 12f;
            case CompartmentType.Bridge:
                return 20f;
            case CompartmentType.Recycling:
                return 17f;
            case CompartmentType.Stabilizer:
                return 22f;
            case CompartmentType.Communications:
                return 15f;
            case CompartmentType.Scanner:
                return 18f;
            default:
                return 10f;
        }
    }
    
    /// <summary>
    /// Get a color associated with this compartment type
    /// </summary>
    /// <param name="type">The compartment type</param>
    /// <returns>A color representing this compartment type</returns>
    public static Color GetThemeColor(this CompartmentType type)
    {
        switch (type)
        {
            case CompartmentType.Engine:
                return new Color(1.0f, 0.6f, 0.0f);  // Orange
            case CompartmentType.Lab:
                return new Color(0.7f, 0.0f, 1.0f);  // Purple
            case CompartmentType.Storage:
                return new Color(0.4f, 0.4f, 0.4f);  // Gray
            case CompartmentType.Bridge:
                return new Color(0.0f, 0.7f, 1.0f);  // Cyan
            case CompartmentType.Recycling:
                return new Color(0.0f, 0.8f, 0.3f);  // Green
            case CompartmentType.Stabilizer:
                return new Color(1.0f, 0.8f, 0.0f);  // Gold
            case CompartmentType.Communications:
                return new Color(0.0f, 0.4f, 0.8f);  // Blue
            case CompartmentType.Scanner:
                return new Color(1.0f, 0.0f, 0.4f);  // Pink
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Get the maximum level for this compartment type
    /// </summary>
    /// <param name="type">The compartment type</param>
    /// <returns>Maximum upgrade level</returns>
    public static int GetMaxLevel(this CompartmentType type)
    {
        // All compartments have the same max level in this implementation
        // You could customize this if some compartments should have different max levels
        return 5;
    }
    
    /// <summary>
    /// Get a list of special unlocks for this compartment type
    /// </summary>
    /// <param name="type">The compartment type</param>
    /// <returns>Array of special unlocks with their levels</returns>
    public static (int level, string name, string description)[] GetSpecialUnlocks(this CompartmentType type)
    {
        switch (type)
        {
            case CompartmentType.Engine:
                return new[]
                {
                    (3, "Auto-Collection", "Probes will now collect waste automatically without manual activation"),
                    (5, "Warp Drive", "Unlock instant travel between discovered locations")
                };
                
            case CompartmentType.Lab:
                return new[]
                {
                    (3, "Advanced Analysis", "Automatically identify valuable components in waste"),
                    (5, "Dimensional Synthesis", "Create synthetic waste from existing resources")
                };
                
            case CompartmentType.Storage:
                return new[]
                {
                    (3, "Auto-Sort", "Automatically organize waste by type and rarity"),
                    (5, "Compression Field", "Double the effective capacity of storage")
                };
                
            case CompartmentType.Bridge:
                return new[]
                {
                    (3, "Advanced Navigation", "Reveal additional details about locations"),
                    (5, "Dimensional Mapping", "Chance to discover hidden locations")
                };
                
            case CompartmentType.Recycling:
                return new[]
                {
                    (3, "Batch Processing", "Recycle multiple waste items simultaneously"),
                    (5, "Quantum Recycling", "Chance to duplicate recycled materials")
                };
                
            case CompartmentType.Stabilizer:
                return new[]
                {
                    (3, "Contamination Shield", "Reduce facility contamination from unstable waste"),
                    (5, "Reality Anchor", "Prevent dimensional decay in rare items")
                };
                
            case CompartmentType.Communications:
                return new[]
                {
                    (3, "Dimensional Beacon", "Attract unique waste types periodically"),
                    (5, "Trade Network", "Access to interdimensional market for rare materials")
                };
                
            case CompartmentType.Scanner:
                return new[]
                {
                    (3, "Deep Scanning", "Reveal hidden properties of all waste items"),
                    (5, "Predictive Analysis", "Forecast optimal collection times for specific waste types")
                };
                
            default:
                return new (int, string, string)[0];
        }
    }
}

/// <summary>
/// Attribute for storing compartment type icons
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class CompartmentIconAttribute : PropertyAttribute
{
    public string IconName { get; private set; }
    
    public CompartmentIconAttribute(string iconName)
    {
        IconName = iconName;
    }
}