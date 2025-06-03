using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Container for sector-specific enemy icons
/// </summary>
[System.Serializable]
public class SectorEnemyIcons
{
    public int sectorNumber;
    public string sectorName;
    
    // One list per enemy type
    public List<Sprite> scavengerIcons = new List<Sprite>();
    public List<Sprite> rivalIcons = new List<Sprite>();
    public List<Sprite> anomalyIcons = new List<Sprite>();
    public List<Sprite> bossIcons = new List<Sprite>();
    
    /// <summary>
    /// Get the list of icons for a specific enemy type
    /// </summary>
    public List<Sprite> GetIconsForType(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Scavenger: return scavengerIcons;
            case EnemyType.Rival: return rivalIcons;
            case EnemyType.Anomaly: return anomalyIcons;
            case EnemyType.Boss: return bossIcons;
            default:
                Debug.LogWarning($"Unknown enemy type: {type}");
                return new List<Sprite>();
        }
    }
} 