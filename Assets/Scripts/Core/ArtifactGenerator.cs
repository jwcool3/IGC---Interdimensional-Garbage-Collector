using System.Collections.Generic;
using UnityEngine;

public class ArtifactGenerator : MonoBehaviour
{
    // Dimension types for artifact generation
    private readonly string[] DimensionTypes = new string[] 
    {
        "Technological", 
        "Magical", 
        "Biological", 
        "Philosophical", 
        "Quantum Divergent",
        "Temporal",
        "Ethereal",
        "Primordial"
    };

    // Artifact name components for procedural generation
    private readonly string[] NamePrefixes = { "Ancient", "Mysterious", "Quantum", "Ethereal", "Temporal", "Prismatic", "Void-touched", "Reality-bent" };
    private readonly string[] NameNouns = { "Codex", "Crystal", "Device", "Relic", "Artifact", "Fragment", "Resonator", "Matrix" };

    // Generate a single artifact with enhanced naming
    public ArtifactBase GenerateArtifact()
    {
        string dimensionOrigin = GetRandomDimensionType();
        float quantumStability = CalculateQuantumStability();
        string name = GenerateArtifactName();

        return new ArtifactBase(
            dimensionOrigin,
            quantumStability,
            name
        );
    }

    // Generate multiple artifacts
    public List<ArtifactBase> GenerateArtifacts(int count)
    {
        var artifacts = new List<ArtifactBase>();
        for (int i = 0; i < count; i++)
        {
            artifacts.Add(GenerateArtifact());
        }
        return artifacts;
    }

    // Enhanced random name generation
    private string GenerateArtifactName()
    {
        string prefix = NamePrefixes[Random.Range(0, NamePrefixes.Length)];
        string noun = NameNouns[Random.Range(0, NameNouns.Length)];
        return $"{prefix} {noun}";
    }

    // Get a random dimension type
    private string GetRandomDimensionType()
    {
        return DimensionTypes[Random.Range(0, DimensionTypes.Length)];
    }

    // Calculate quantum stability with some interesting variance
    private float CalculateQuantumStability()
    {
        // Base stability
        float baseStability = Random.Range(0.1f, 1f);
        
        // Add some quantum fluctuation
        float fluctuation = Random.Range(-0.1f, 0.1f);
        
        return Mathf.Clamp01(baseStability + fluctuation);
    }
} 