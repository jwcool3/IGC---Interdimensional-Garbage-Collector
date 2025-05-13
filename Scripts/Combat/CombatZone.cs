using UnityEngine;
using System.Collections.Generic;

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
    
    [Header("Zone Visuals")]
    public Sprite zoneIcon;
    
    [Header("Enemy Variations")]
    public int[] scavengerVariations = new int[] { 0 }; // Default to variation 0
    public int[] rivalVariations = new int[] { 0 };
    public int[] anomalyVariations = new int[] { 0 };
    public int bossVariation = 0;
    
    [Header("Enemy Distribution")]
    [Range(0f, 1f)] public float scavengerProbability = 0.6f;
    [Range(0f, 1f)] public float rivalProbability = 0.25f;
    [Range(0f, 1f)] public float anomalyProbability = 0.15f;
    
    /// <summary>
    /// Constructor with all fields
    /// </summary>
    public CombatZone(string name, int level, int sector, int enemies, bool locked = true)
    {
        zoneName = name;
        zoneLevel = level;
        sectorNumber = sector;
        enemiesInZone = enemies;
        isLocked = locked;
        
        // Try to load zone icon from resources if not set
        LoadZoneIcon();
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
    /// Get a random enemy type based on zone probabilities
    /// </summary>
    public EnemyType GetRandomEnemyType()
    {
        ValidateProbabilities();
        
        float roll = Random.value;
        float cumulative = 0f;
        
        cumulative += scavengerProbability;
        if (roll < cumulative)
            return EnemyType.Scavenger;
            
        cumulative += rivalProbability;
        if (roll < cumulative)
            return EnemyType.Rival;
            
        return EnemyType.Anomaly;
    }
    
    /// <summary>
    /// Ensure probabilities sum to 1
    /// </summary>
    public void ValidateProbabilities()
    {
        float total = scavengerProbability + rivalProbability + anomalyProbability;
        
        if (Mathf.Abs(total - 1f) > 0.01f)
        {
            Debug.LogWarning($"Zone {zoneName} probabilities don't sum to 1 (total: {total}). Normalizing...");
            
            // Normalize probabilities
            float scale = 1f / total;
            scavengerProbability *= scale;
            rivalProbability *= scale;
            anomalyProbability *= scale;
        }
    }
    
    /// <summary>
    /// Try to load the zone icon from resources if not already set
    /// </summary>
    private void LoadZoneIcon()
    {
        if (zoneIcon != null) return;
        
        // Try loading by zone name first
        string iconName = zoneName.Replace(" ", "");
        zoneIcon = Resources.Load<Sprite>($"LocationIcons/{iconName}");
        
        // If not found, try by sector number
        if (zoneIcon == null)
        {
            zoneIcon = Resources.Load<Sprite>($"LocationIcons/Sector{sectorNumber}");
        }
        
        if (zoneIcon == null)
        {
            Debug.LogWarning($"No icon found for zone {zoneName} (Sector {sectorNumber})");
        }
    }
    
    /// <summary>
    /// Get the zone's icon, loading from resources if necessary
    /// </summary>
    public Sprite GetZoneIcon()
    {
        if (zoneIcon == null)
        {
            LoadZoneIcon();
        }
        return zoneIcon;
    }
}