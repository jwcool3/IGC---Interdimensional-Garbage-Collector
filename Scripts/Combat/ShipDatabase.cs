using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the database of available ship models and their icons
/// </summary>
public class ShipDatabase : MonoBehaviour
{
    // Singleton pattern
    public static ShipDatabase Instance { get; private set; }
    
    [SerializeField] private List<ShipModel> shipModels = new List<ShipModel>();
    
    // Lookup dictionaries
    private Dictionary<string, ShipModel> shipsByName = new Dictionary<string, ShipModel>();
    private Dictionary<EnemyType, List<ShipModel>> shipsByType = new Dictionary<EnemyType, List<ShipModel>>();
    private Dictionary<int, List<ShipModel>> shipsBySector = new Dictionary<int, List<ShipModel>>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLookups();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeLookups()
    {
        // Clear dictionaries
        shipsByName.Clear();
        shipsByType.Clear();
        shipsBySector.Clear();
        
        // Initialize the type dictionary with empty lists for each enemy type
        foreach (EnemyType type in System.Enum.GetValues(typeof(EnemyType)))
        {
            shipsByType[type] = new List<ShipModel>();
        }
        
        // Process all ship models
        foreach (var ship in shipModels)
        {
            // Skip invalid entries
            if (string.IsNullOrEmpty(ship.modelName) || ship.shipIcon == null)
            {
                Debug.LogWarning($"Ship model has missing name or icon: {ship.modelName}");
                continue;
            }
            
            // Add to name lookup (case insensitive)
            string lookupName = ship.modelName.ToLower();
            shipsByName[lookupName] = ship;
            
            // Add to type lookup
            shipsByType[ship.shipType].Add(ship);
            
            // Add to sector lookup
            int sector = ship.minSectorLevel;
            if (!shipsBySector.ContainsKey(sector))
            {
                shipsBySector[sector] = new List<ShipModel>();
            }
            shipsBySector[sector].Add(ship);
        }
        
        // Log what we found
        Debug.Log($"Ship Database initialized with {shipModels.Count} ships:");
        foreach (var type in shipsByType.Keys)
        {
            Debug.Log($"- {type}: {shipsByType[type].Count} models");
        }
    }
    
    /// <summary>
    /// Get a ship model by its exact name
    /// </summary>
    public ShipModel GetShipByName(string modelName)
    {
        if (string.IsNullOrEmpty(modelName)) return null;
        
        string lookupName = modelName.ToLower();
        if (shipsByName.TryGetValue(lookupName, out ShipModel model))
        {
            return model;
        }
        
        Debug.LogWarning($"Ship model not found: {modelName}");
        return null;
    }
    
    /// <summary>
    /// Get a random ship model of a specific type for a specific sector
    /// </summary>
    public ShipModel GetRandomShipForTypeAndSector(EnemyType type, int sector)
    {
        // Get all ships of this type
        if (shipsByType.TryGetValue(type, out List<ShipModel> shipsOfType))
        {
            // Filter by sector level
            List<ShipModel> availableShips = shipsOfType.FindAll(s => s.minSectorLevel <= sector);
            
            if (availableShips.Count > 0)
            {
                // Return a random one
                return availableShips[Random.Range(0, availableShips.Count)];
            }
        }
        
        Debug.LogWarning($"No ships found for type {type} in sector {sector}");
        return null;
    }
    
    /// <summary>
    /// Get all available ship models for a type and sector
    /// </summary>
    public List<ShipModel> GetAllShipsForTypeAndSector(EnemyType type, int sector)
    {
        if (shipsByType.TryGetValue(type, out List<ShipModel> shipsOfType))
        {
            return shipsOfType.FindAll(s => s.minSectorLevel <= sector);
        }
        
        return new List<ShipModel>();
    }
    
    /// <summary>
    /// Get the icon for a specific ship model
    /// </summary>
    public Sprite GetIconForShip(string modelName)
    {
        ShipModel model = GetShipByName(modelName);
        return model?.shipIcon;
    }
    
    /// <summary>
    /// Get a random ship name for a given type
    /// </summary>
    public string GetRandomShipName(EnemyType type)
    {
        string[] prefixes = { "Scavenger", "Waste", "Junk", "Salvage", "Rogue" };
        string[] types = { "Drone", "Hauler", "Collector", "Scrapper", "Vessel" };
        
        string prefix = prefixes[Random.Range(0, prefixes.Length)];
        string typeName = types[Random.Range(0, types.Length)];
        
        return $"{prefix} {typeName}";
    }
} 