using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages enemy icons with runtime loading and caching
/// </summary>
public class EnemyIconManager : MonoBehaviour
{
    // Singleton pattern
    public static EnemyIconManager Instance { get; private set; }
    
    // Cache for loaded sprites
    private readonly Dictionary<string, Sprite> iconCache = new Dictionary<string, Sprite>();
    
    // Fallback icons for when specific variations aren't found
    private readonly Dictionary<EnemyType, Sprite> defaultIcons = new Dictionary<EnemyType, Sprite>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDefaultIcons();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Load default icons for each enemy type
    /// </summary>
    private void LoadDefaultIcons()
    {
        foreach (EnemyType type in System.Enum.GetValues(typeof(EnemyType)))
        {
            string path = $"EnemyIcons/Default/{type}";
            Sprite defaultIcon = Resources.Load<Sprite>(path);
            
            if (defaultIcon != null)
            {
                defaultIcons[type] = defaultIcon;
                Debug.Log($"Loaded default icon for {type}");
            }
            else
            {
                Debug.LogWarning($"No default icon found for {type} at path: {path}");
            }
        }
    }
    
    /// <summary>
    /// Get an icon for a specific enemy type, sector, and variation
    /// </summary>
    public Sprite GetIconForEnemy(EnemyType type, int sector, int variation)
    {
        // Create a unique key for this specific icon
        string iconKey = $"Sector{sector}_{type}_{variation}";
        
        // Check if already cached
        if (iconCache.TryGetValue(iconKey, out Sprite cachedIcon))
        {
            return cachedIcon;
        }
        
        // Try to load the specific variation
        string path = $"EnemyIcons/Sector{sector}/{type}/{type}_{sector}{GetVariationLetter(variation)}";
        Sprite icon = Resources.Load<Sprite>(path);
        
        if (icon == null)
        {
            // Try fallback without variation letter
            path = $"EnemyIcons/Sector{sector}/{type}/{type}_{sector}";
            icon = Resources.Load<Sprite>(path);
            
            if (icon == null)
            {
                // Try sector default
                path = $"EnemyIcons/Sector{sector}/{type}/Default";
                icon = Resources.Load<Sprite>(path);
                
                if (icon == null)
                {
                    // Use type default if available
                    defaultIcons.TryGetValue(type, out icon);
                    
                    if (icon == null)
                    {
                        Debug.LogWarning($"No icon found for {type} in sector {sector} variation {variation}");
                    }
                }
            }
        }
        
        // Cache the result (even if null)
        iconCache[iconKey] = icon;
        return icon;
    }
    
    /// <summary>
    /// Get the number of variations available for an enemy type in a sector
    /// </summary>
    public int GetVariationCount(EnemyType type, int sector)
    {
        int count = 0;
        string basePath = $"EnemyIcons/Sector{sector}/{type}/{type}_{sector}";
        
        // Check variations until we don't find one
        while (true)
        {
            string path = $"{basePath}{GetVariationLetter(count)}";
            if (Resources.Load<Sprite>(path) == null)
            {
                break;
            }
            count++;
        }
        
        // If no variations found, check if base icon exists
        if (count == 0 && Resources.Load<Sprite>(basePath) != null)
        {
            count = 1;
        }
        
        return count;
    }
    
    /// <summary>
    /// Convert numeric variation to letter (0=A, 1=B, etc.)
    /// </summary>
    private string GetVariationLetter(int variation)
    {
        return ((char)('A' + variation)).ToString();
    }
    
    /// <summary>
    /// Get the color associated with an enemy type (fallback when icons unavailable)
    /// </summary>
    public Color GetColorForEnemyType(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Scavenger:
                return new Color(0.5f, 0.5f, 0.5f); // Gray
                
            case EnemyType.Rival:
                return new Color(0.2f, 0.6f, 1f); // Blue
                
            case EnemyType.Anomaly:
                return new Color(1f, 0.4f, 0.8f); // Pink
                
            case EnemyType.Boss:
                return new Color(1f, 0.2f, 0.2f); // Red
                
            default:
                return Color.white;
        }
    }
    
    private void OnDestroy()
    {
        // Clear caches when destroyed
        iconCache.Clear();
        defaultIcons.Clear();
        
        // Unload unused assets
        Resources.UnloadUnusedAssets();
    }
} 