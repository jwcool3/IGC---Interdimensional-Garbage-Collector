using System;
using UnityEngine;

[Serializable]
public class ArtifactBase 
{
    // Unique identifier for the artifact
    public string Id { get; private set; }

    // Dimensional properties
    public string DimensionalOrigin { get; private set; }
    public float QuantumStability { get; private set; }
    public float RarityScore { get; private set; }

    // Artifact characteristics
    public string Name { get; private set; }
    public string Description { get; private set; }

    // Gameplay mechanics
    public float ResearchValue { get; private set; }
    public float CollectionPotential { get; private set; }

    // Constructor for generating artifacts
    public ArtifactBase(
        string dimensionalOrigin, 
        float quantumStability, 
        string name = null, 
        string description = null)
    {
        Id = Guid.NewGuid().ToString();
        DimensionalOrigin = dimensionalOrigin;
        QuantumStability = quantumStability;
        Name = name ?? GenerateName();
        Description = description ?? GenerateDescription();
        
        CalculateRarityScore();
    }

    // Procedural name generation
    private string GenerateName()
    {
        // Implement naming logic based on dimensional properties
        return $"{DimensionalOrigin} Artifact {Id.Substring(0, 8)}";
    }

    // Procedural description generation
    private string GenerateDescription()
    {
        // Create dynamic descriptions based on artifact properties
        return $"A mysterious artifact from the {DimensionalOrigin} reality. " +
               $"Quantum stability: {QuantumStability:P2}";
    }

    // Calculate rarity based on dimensional properties
    private void CalculateRarityScore()
    {
        // Complex rarity calculation
        RarityScore = QuantumStability * UnityEngine.Random.Range(0.5f, 2f);
        ResearchValue = RarityScore * 100f;
        CollectionPotential = Mathf.Clamp(RarityScore, 0f, 1f);
    }
} 