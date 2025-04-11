using System.Collections.Generic;
using UnityEngine;

public class WasteGenerator : MonoBehaviour
{
    // Dimension types for waste generation
    private readonly string[] DimensionTypes = new string[] 
    {
        "Technological Waste",
        "Biological Remnants",
        "Quantum Residue",
        "Philosophical Byproducts",
        "Cosmic Debris",
        "Temporal Refuse",
        "Ethereal Scrap",
        "Industrial Waste"
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

    // Generate a single waste item
    public WasteItem GenerateWasteItem()
    {
        string dimensionOrigin = GetRandomDimensionType();
        float wasteStability = CalculateWasteStability();
        string name = GenerateWasteName();

        return new WasteItem(
            dimensionOrigin,
            wasteStability,
            name
        );
    }

    // Generate multiple waste items
    public List<WasteItem> GenerateWasteItems(int count)
    {
        var wasteItems = new List<WasteItem>();
        for (int i = 0; i < count; i++)
        {
            wasteItems.Add(GenerateWasteItem());
        }
        return wasteItems;
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

        return new WasteItem(
            dimensionType,
            stability
        );
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