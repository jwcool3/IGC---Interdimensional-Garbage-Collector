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
    
    [Header("Default Ship Icons")]
    [SerializeField] private Sprite defaultScavengerShipIcon;
    [SerializeField] private Sprite defaultRivalShipIcon;
    [SerializeField] private Sprite defaultAnomalyShipIcon;
    [SerializeField] private Sprite defaultBossShipIcon;
    
    [Header("Type Icons")]
    [SerializeField] private Sprite scavengerTypeIcon;
    [SerializeField] private Sprite rivalTypeIcon;
    [SerializeField] private Sprite anomalyTypeIcon;
    [SerializeField] private Sprite bossTypeIcon;
    
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
    /// Load default icons for each enemy type if not assigned in inspector
    /// </summary>
    private void LoadDefaultIcons()
    {
        // Load ship icons if not assigned
        if (defaultScavengerShipIcon == null)
            defaultScavengerShipIcon = Resources.Load<Sprite>("EnemyIcons/Default/ScavengerShip");
            
        if (defaultRivalShipIcon == null)
            defaultRivalShipIcon = Resources.Load<Sprite>("EnemyIcons/Default/RivalShip");
            
        if (defaultAnomalyShipIcon == null)
            defaultAnomalyShipIcon = Resources.Load<Sprite>("EnemyIcons/Default/AnomalyShip");
            
        if (defaultBossShipIcon == null)
            defaultBossShipIcon = Resources.Load<Sprite>("EnemyIcons/Default/BossShip");
            
        // Load type icons if not assigned
        if (scavengerTypeIcon == null)
            scavengerTypeIcon = Resources.Load<Sprite>("EnemyIcons/Types/Scavenger");
            
        if (rivalTypeIcon == null)
            rivalTypeIcon = Resources.Load<Sprite>("EnemyIcons/Types/Rival");
            
        if (anomalyTypeIcon == null)
            anomalyTypeIcon = Resources.Load<Sprite>("EnemyIcons/Types/Anomaly");
            
        if (bossTypeIcon == null)
            bossTypeIcon = Resources.Load<Sprite>("EnemyIcons/Types/Boss");
    }
    
    /// <summary>
    /// Get an icon for an enemy ship using multiple approaches
    /// </summary>
    public Sprite GetIconForEnemy(EnemyType type, int sector, int variation)
    {
        // 1. Check cache first
        string cacheKey = $"Ship_{type}_{sector}_{variation}";
        if (iconCache.TryGetValue(cacheKey, out Sprite cachedIcon))
            return cachedIcon;
            
        Sprite icon = null;
        
        // 2. Try ShipDatabase for model-specific icons
        if (ShipDatabase.Instance != null)
        {
            ShipModel shipModel = ShipDatabase.Instance.GetRandomShipForTypeAndSector(type, sector);
            if (shipModel != null && shipModel.shipIcon != null)
                icon = shipModel.shipIcon;
        }
        
        // 3. Try loading from Resources folder hierarchy
        if (icon == null)
        {
            // Try specific sector/variation
            string basePath = $"EnemyIcons/Sector{sector}/{type}";
            string specificPath = $"{basePath}/{type}_{sector}{GetVariationLetter(variation)}";
            icon = Resources.Load<Sprite>(specificPath);
            
            // Try without variation letter
            if (icon == null)
                icon = Resources.Load<Sprite>($"{basePath}/{type}_{sector}");
                
            // Try sector default
            if (icon == null)
                icon = Resources.Load<Sprite>($"{basePath}/Default");
        }
        
        // 4. Use default icon for type if everything else fails
        if (icon == null)
            icon = GetDefaultShipIcon(type);
            
        // Cache result (even if null)
        iconCache[cacheKey] = icon;
        return icon;
    }
    
    /// <summary>
    /// Get an icon for a specific ship model by name
    /// </summary>
    public Sprite GetIconForShipModel(string modelName)
    {
        if (string.IsNullOrEmpty(modelName))
            return null;
            
        // Check cache first
        string cacheKey = $"Model_{modelName}";
        if (iconCache.TryGetValue(cacheKey, out Sprite cachedIcon))
            return cachedIcon;
            
        Sprite icon = null;
        
        // Try ShipDatabase if available
        if (ShipDatabase.Instance != null)
            icon = ShipDatabase.Instance.GetIconForShip(modelName);
            
        // Try loading directly from resources as fallback
        if (icon == null)
            icon = Resources.Load<Sprite>($"ShipModels/{modelName}");
            
        // Cache the result
        iconCache[cacheKey] = icon;
        return icon;
    }
    
    /// <summary>
    /// Get the icon representing an enemy type
    /// </summary>
    public Sprite GetTypeIcon(EnemyType type)
    {
        // First try to get from cache
        string cacheKey = $"TypeIcon_{type}";
        if (iconCache.TryGetValue(cacheKey, out Sprite cachedIcon))
        {
            return cachedIcon;
        }
        
        // Get assigned icon
        Sprite typeIcon = type switch
        {
            EnemyType.Scavenger => scavengerTypeIcon,
            EnemyType.Rival => rivalTypeIcon,
            EnemyType.Anomaly => anomalyTypeIcon,
            EnemyType.Boss => bossTypeIcon,
            _ => null
        };
        
        // If no icon assigned, try to load from resources
        if (typeIcon == null)
        {
            typeIcon = Resources.Load<Sprite>($"EnemyIcons/Types/{type}");
        }
        
        // Cache the result (even if null)
        iconCache[cacheKey] = typeIcon;
        
        return typeIcon;
    }
    
    /// <summary>
    /// Get the default ship icon for a given enemy type
    /// </summary>
    public Sprite GetDefaultShipIcon(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Scavenger:
                return defaultScavengerShipIcon;
                
            case EnemyType.Rival:
                return defaultRivalShipIcon;
                
            case EnemyType.Anomaly:
                return defaultAnomalyShipIcon;
                
            case EnemyType.Boss:
                return defaultBossShipIcon;
                
            default:
                return null;
        }
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
    
    /// <summary>
    /// Convert numeric variation to letter (0=A, 1=B, etc.)
    /// </summary>
    private string GetVariationLetter(int variation)
    {
        return ((char)('A' + variation)).ToString();
    }
    
    private void OnDestroy()
    {
        // Clear cache when destroyed
        iconCache.Clear();
        
        // Unload unused assets
        Resources.UnloadUnusedAssets();
    }
}