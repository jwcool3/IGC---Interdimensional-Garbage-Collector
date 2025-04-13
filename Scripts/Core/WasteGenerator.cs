using System.Collections.Generic;
using UnityEngine;

public class WasteGenerator : MonoBehaviour
{
    // Dimension types for waste generation
    private readonly string[] DimensionTypes = new string[] 
    {
        "Technological",
        "Biological",
        "Quantum",
        "Temporal",
        "Philosophical"
    };

    // Waste classification components
    private readonly string[] WasteOrigins = 
    {
        "Abandoned",
        "Discarded",
        "Rejected",
        "Obsolete",
        "Malfunctioning",
        "Corrupted",
        "Unstable",
        "Contaminated"
    };

    private readonly string[] WasteTypes = 
    {
        "Machinery",
        "Biomatter",
        "Energy Core",
        "Circuit Array",
        "Containment Unit",
        "Processing Node",
        "Storage System",
        "Filter Component"
    };

    private readonly string[] wasteNames = new string[]
    {
        "Debris",
        "Residue",
        "Matter",
        "Fragment",
        "Essence"
    };

    // Generate a single waste item
    public WasteItem GenerateWasteItem()
    {
        string dimensionType = DimensionTypes[Random.Range(0, DimensionTypes.Length)];
        string wasteName = wasteNames[Random.Range(0, wasteNames.Length)];
        float stability = Random.Range(0.2f, 1.0f);

        string fullName = $"{dimensionType} {wasteName}";
        string origin = $"{dimensionType} Dimension";

        return new WasteItem(fullName, origin, stability);
    }

    // Generate multiple waste items
    public List<WasteItem> GenerateMultipleWaste(int count)
    {
        List<WasteItem> items = new List<WasteItem>();
        for (int i = 0; i < count; i++)
        {
            items.Add(GenerateWasteItem());
        }
        return items;
    }

    // Enhanced waste name generation
    private string GenerateWasteName()
    {
        string origin = WasteOrigins[Random.Range(0, WasteOrigins.Length)];
        string type = WasteTypes[Random.Range(0, WasteTypes.Length)];
        return $"{origin} {type}";
    }

    // Get a random dimension type
    private string GetRandomDimensionType()
    {
        return DimensionTypes[Random.Range(0, DimensionTypes.Length)];
    }

    // Calculate waste stability with interesting variance
    private float CalculateWasteStability()
    {
        // Base stability
        float baseStability = Random.Range(0.1f, 1f);
        
        // Add dimensional fluctuation
        float fluctuation = Random.Range(-0.2f, 0.2f);
        
        // Add occasional quantum instability spikes
        if (Random.value < 0.1f) // 10% chance
        {
            fluctuation *= 2f;
        }
        
        return Mathf.Clamp01(baseStability + fluctuation);
    }

    // Generate waste with specific characteristics
    public WasteItem GenerateSpecificWaste(string dimensionType, float stabilityRange)
    {
        float stability = Random.Range(
            Mathf.Max(0.1f, stabilityRange - 0.2f),
            Mathf.Min(1f, stabilityRange + 0.2f)
        );

        string wasteName = wasteNames[Random.Range(0, wasteNames.Length)];
        string fullName = $"{dimensionType} {wasteName}";
        string origin = $"{dimensionType} Dimension";

        return new WasteItem(fullName, origin, stability);
    }

    // Generate a batch of similar waste
    public List<WasteItem> GenerateSimilarWaste(int count, string dimensionType)
    {
        var wasteItems = new List<WasteItem>();
        float baseStability = Random.Range(0.3f, 0.7f);

        for (int i = 0; i < count; i++)
        {
            float stabilityVariation = Random.Range(-0.1f, 0.1f);
            wasteItems.Add(GenerateSpecificWaste(
                dimensionType,
                Mathf.Clamp01(baseStability + stabilityVariation)
            ));
        }

        return wasteItems;
    }
} 