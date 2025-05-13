using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SectorIconSet
{
    public string sectorName;
    public List<Sprite> scavengerIcons = new List<Sprite>();
    public List<Sprite> rivalIcons = new List<Sprite>();
    public List<Sprite> anomalyIcons = new List<Sprite>();
    public List<Sprite> bossIcons = new List<Sprite>();

    // Helper method to get icons based on enemy type
    public List<Sprite> GetIconsForType(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Scavenger:
                return scavengerIcons;
            case EnemyType.Rival:
                return rivalIcons;
            case EnemyType.Anomaly:
                return anomalyIcons;
            case EnemyType.Boss:
                return bossIcons;
            default:
                return null;
        }
    }
}

public class SectorIconContainer : MonoBehaviour
{
    // Singleton pattern
    public static SectorIconContainer Instance { get; private set; }

    [Header("Sector Icon Sets")]
    public List<SectorIconSet> sectorIcons = new List<SectorIconSet>();

    [Header("Default Icons")]
    public Sprite defaultScavengerIcon;
    public Sprite defaultRivalIcon;
    public Sprite defaultAnomalyIcon;
    public Sprite defaultBossIcon;

    private void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Get a specific icon for an enemy type in a sector
    /// </summary>
    public Sprite GetSectorIcon(EnemyType type, int sectorNumber, int variation = 0)
    {
        // Validate sector number
        if (sectorNumber <= 0 || sectorNumber > sectorIcons.Count)
        {
            return GetDefaultIcon(type);
        }

        // Get icon set for this sector (subtract 1 as sector numbers start at 1)
        var sectorSet = sectorIcons[sectorNumber - 1];
        if (sectorSet == null)
        {
            return GetDefaultIcon(type);
        }

        // Get icons for this type
        var icons = sectorSet.GetIconsForType(type);
        if (icons == null || icons.Count == 0)
        {
            return GetDefaultIcon(type);
        }

        // Get specific variation or random if variation is -1
        if (variation == -1)
        {
            variation = Random.Range(0, icons.Count);
        }

        // Validate variation index
        if (variation >= 0 && variation < icons.Count)
        {
            return icons[variation];
        }

        // Return first icon as fallback
        return icons[0];
    }

    /// <summary>
    /// Get the default icon for an enemy type
    /// </summary>
    public Sprite GetDefaultIcon(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Scavenger:
                return defaultScavengerIcon;
            case EnemyType.Rival:
                return defaultRivalIcon;
            case EnemyType.Anomaly:
                return defaultAnomalyIcon;
            case EnemyType.Boss:
                return defaultBossIcon;
            default:
                return null;
        }
    }

    /// <summary>
    /// Get a random icon variation for an enemy type in a sector
    /// </summary>
    public Sprite GetRandomSectorIcon(EnemyType type, int sectorNumber)
    {
        return GetSectorIcon(type, sectorNumber, -1);
    }

    /// <summary>
    /// Check if a sector has custom icons for an enemy type
    /// </summary>
    public bool HasCustomIcons(int sectorNumber, EnemyType type)
    {
        if (sectorNumber <= 0 || sectorNumber > sectorIcons.Count)
            return false;

        var sectorSet = sectorIcons[sectorNumber - 1];
        var icons = sectorSet?.GetIconsForType(type);
        return icons != null && icons.Count > 0;
    }
} 