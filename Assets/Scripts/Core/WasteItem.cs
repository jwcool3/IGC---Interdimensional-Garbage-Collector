using System;
using UnityEngine;

[Serializable]
public class WasteItem 
{
    // Unique identifier for the waste item
    public string Id { get; private set; }

    // Dimensional properties
    public string DimensionalOrigin { get; private set; }
    public float WasteStability { get; private set; }
    public float RecyclingPotential { get; private set; }

    // Waste characteristics
    public string Name { get; private set; }
    public string Description { get; private set; }
    public float ContaminationLevel { get; private set; }

    // Gameplay mechanics
    public float ProcessingDifficulty { get; private set; }
    public float EnvironmentalImpact { get; private set; }
    public float RecyclingValue { get; private set; }

    // Constructor for generating waste items
    public WasteItem(
        string dimensionalOrigin, 
        float wasteStability, 
        string name = null, 
        string description = null)
    {
        Id = Guid.NewGuid().ToString();
        DimensionalOrigin = dimensionalOrigin;
        WasteStability = wasteStability;
        Name = name ?? GenerateName();
        Description = description ?? GenerateDescription();
        
        CalculateProperties();
    }

    // Procedural name generation
    private string GenerateName()
    {
        string[] prefixes = { "Unstable", "Quantum", "Dimensional", "Exotic", "Contaminated" };
        string[] types = { "Waste", "Debris", "Residue", "Refuse", "Scrap" };
        string prefix = prefixes[UnityEngine.Random.Range(0, prefixes.Length)];
        string type = types[UnityEngine.Random.Range(0, types.Length)];
        
        return $"{prefix} {DimensionalOrigin} {type}";
    }

    // Procedural description generation
    private string GenerateDescription()
    {
        return $"A peculiar piece of waste from the {DimensionalOrigin} reality.\n" +
               $"Stability: {WasteStability:P2}\n" +
               $"Contamination Level: {ContaminationLevel:P2}\n" +
               $"Environmental Impact: {GetEnvironmentalImpactDescription()}";
    }

    private void CalculateProperties()
    {
        // Calculate recycling potential based on stability
        RecyclingPotential = WasteStability * UnityEngine.Random.Range(0.5f, 2f);
        
        // Higher stability means lower contamination
        ContaminationLevel = 1f - WasteStability;
        
        // Processing difficulty increases with contamination
        ProcessingDifficulty = Mathf.Lerp(0.5f, 2f, ContaminationLevel);
        
        // Environmental impact is worse with high contamination and low stability
        EnvironmentalImpact = (ContaminationLevel + (1f - WasteStability)) * 0.5f;
        
        // Recycling value is based on potential and difficulty
        RecyclingValue = RecyclingPotential * 100f / ProcessingDifficulty;
    }

    private string GetEnvironmentalImpactDescription()
    {
        if (EnvironmentalImpact >= 0.8f) return "Severe";
        if (EnvironmentalImpact >= 0.6f) return "High";
        if (EnvironmentalImpact >= 0.4f) return "Moderate";
        if (EnvironmentalImpact >= 0.2f) return "Low";
        return "Minimal";
    }

    public string GetStatusDescription()
    {
        return $"Processing Difficulty: {GetDifficultyDescription()}\n" +
               $"Recycling Potential: {RecyclingPotential:P2}\n" +
               $"Value: {RecyclingValue:F0} RP";
    }

    private string GetDifficultyDescription()
    {
        if (ProcessingDifficulty >= 1.8f) return "Extreme";
        if (ProcessingDifficulty >= 1.5f) return "Very Hard";
        if (ProcessingDifficulty >= 1.2f) return "Hard";
        if (ProcessingDifficulty >= 0.8f) return "Moderate";
        return "Easy";
    }
} 