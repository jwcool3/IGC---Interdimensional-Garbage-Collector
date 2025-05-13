using UnityEngine;
using System.Collections.Generic;

public class EnemyIconManager : MonoBehaviour
{
    // Singleton pattern
    public static EnemyIconManager Instance { get; private set; }
    
    // Cache for loaded sprites
    private readonly Dictionary<string, Sprite> iconCache = new Dictionary<string, Sprite>();
    
    // Fallback icons for when specific icons aren't found
    [Header("Fallback Icons")]
    [SerializeField] private Sprite defaultScavengerIcon;
    [SerializeField] private Sprite defaultRivalIcon;
    [SerializeField] private Sprite defaultAnomalyIcon;
    [SerializeField] private Sprite defaultBossIcon;
    
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
        if (defaultScavengerIcon == null)
            defaultScavengerIcon = Resources.Load<Sprite>("EnemyIcons/Default/Scavenger");
            
        if (defaultRivalIcon == null)
            defaultRivalIcon = Resources.Load<Sprite>("EnemyIcons/Default/Rival");
            
        if (defaultAnomalyIcon == null)
            defaultAnomalyIcon = Resources.Load<Sprite>("EnemyIcons/Default/Anomaly");
            
        if (defaultBossIcon == null)
            defaultBossIcon = Resources.Load<Sprite>("EnemyIcons/Default/Boss");
    }
    
    /// <summary>
    /// Get an icon for an enemy using the ShipDatabase when available
    /// </summary>
    public Sprite GetIconForEnemy(EnemyType type, int sector, int variation)
    {
        // First try to get from ShipDatabase if available
        if (ShipDatabase.Instance != null)
        {
            // Get a ship model appropriate for this enemy type and sector
            ShipModel shipModel = ShipDatabase.Instance.GetRandomShipForTypeAndSector(type, sector);
            
            if (shipModel != null && shipModel.shipIcon != null)
            {
                return shipModel.shipIcon;
            }
        }
        
        // If that fails, fall back to our default icons
        return GetDefaultIconForType(type);
    }
    
    /// <summary>
    /// Get an icon for a specific ship model by name
    /// </summary>
    public Sprite GetIconForShipModel(string modelName)
    {
        if (string.IsNullOrEmpty(modelName) || ShipDatabase.Instance == null)
            return null;
            
        return ShipDatabase.Instance.GetIconForShip(modelName);
    }
    
    /// <summary>
    /// Get the default icon for an enemy type
    /// </summary>
    public Sprite GetDefaultIconForType(EnemyType type)
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
} 