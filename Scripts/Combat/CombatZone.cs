using UnityEngine;

/// <summary>
/// Defines a combat zone with specific enemy variations and fleet composition
/// </summary>
[System.Serializable]
public class CombatZone
{
    [Header("Zone Info")]
    public string zoneName;
    public int zoneLevel;
    public int sectorNumber;
    public int enemiesInZone;
    public bool isLocked;
    
    [Header("Enemy Variations")]
    public int[] scavengerVariations = new int[] { 0 }; // Default to variation 0
    public int[] rivalVariations = new int[] { 0 };
    public int[] anomalyVariations = new int[] { 0 };
    public int bossVariation = 0;
    
    [Header("Enemy Fleet Composition")]
    [Range(0f, 1f)]
    public float scavengerProbability = 0.6f;
    [Range(0f, 1f)]
    public float rivalProbability = 0.3f;
    [Range(0f, 1f)]
    public float anomalyProbability = 0.1f;
    
    /// <summary>
    /// Constructor with sector number
    /// </summary>
    public CombatZone(string name, int level, int sector, int enemies, bool locked = true)
    {
        zoneName = name;
        zoneLevel = level;
        sectorNumber = sector;
        enemiesInZone = enemies;
        isLocked = locked;
    }
    
    /// <summary>
    /// Constructor for backward compatibility
    /// </summary>
    public CombatZone(string name, int level, int enemies, bool locked = true)
    {
        zoneName = name;
        zoneLevel = level;
        sectorNumber = level; // Default sector to match level
        enemiesInZone = enemies;
        isLocked = locked;
    }
    
    /// <summary>
    /// Get a random variation for a given enemy type in this zone
    /// </summary>
    public int GetRandomVariationForType(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Scavenger:
                return scavengerVariations.Length > 0 ? 
                    scavengerVariations[Random.Range(0, scavengerVariations.Length)] : 0;
                    
            case EnemyType.Rival:
                return rivalVariations.Length > 0 ? 
                    rivalVariations[Random.Range(0, rivalVariations.Length)] : 0;
                    
            case EnemyType.Anomaly:
                return anomalyVariations.Length > 0 ? 
                    anomalyVariations[Random.Range(0, anomalyVariations.Length)] : 0;
                    
            case EnemyType.Boss:
                return bossVariation;
                
            default:
                Debug.LogWarning($"Unknown enemy type: {type}. Using variation 0.");
                return 0;
        }
    }
    
    /// <summary>
    /// Get a weighted random enemy type based on zone probabilities
    /// </summary>
    public EnemyType GetRandomEnemyType()
    {
        // Normalize probabilities to ensure they sum to 1.0
        float total = scavengerProbability + rivalProbability + anomalyProbability;
        if (Mathf.Abs(total - 1f) > 0.01f)
        {
            Debug.LogWarning($"Enemy probabilities in zone {zoneName} don't sum to 1.0 (sum: {total}). Normalizing...");
            scavengerProbability /= total;
            rivalProbability /= total;
            anomalyProbability /= total;
        }
        
        float roll = Random.value;
        float cumulative = 0;
        
        // Check each probability in sequence
        cumulative += scavengerProbability;
        if (roll < cumulative) return EnemyType.Scavenger;
        
        cumulative += rivalProbability;
        if (roll < cumulative) return EnemyType.Rival;
        
        return EnemyType.Anomaly; // Default if neither of the above
    }
    
    /// <summary>
    /// Validate and fix enemy type probabilities
    /// </summary>
    public void ValidateProbabilities()
    {
        // Ensure probabilities are in valid range
        scavengerProbability = Mathf.Clamp01(scavengerProbability);
        rivalProbability = Mathf.Clamp01(rivalProbability);
        anomalyProbability = Mathf.Clamp01(anomalyProbability);
        
        // Normalize to sum to 1.0
        float total = scavengerProbability + rivalProbability + anomalyProbability;
        if (total > 0)
        {
            scavengerProbability /= total;
            rivalProbability /= total;
            anomalyProbability /= total;
        }
        else
        {
            // If all probabilities are 0, set default distribution
            scavengerProbability = 0.6f;
            rivalProbability = 0.3f;
            anomalyProbability = 0.1f;
        }
    }
}