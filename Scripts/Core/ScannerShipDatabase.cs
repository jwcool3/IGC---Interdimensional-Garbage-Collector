using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Static ship database for the Scanner system - similar to ShipDatabase but for scanner-specific ships
/// Now uses static data management like the combat ShipDatabase
/// </summary>
public class ScannerShipDatabase : MonoBehaviour
{
    // Singleton pattern
    public static ScannerShipDatabase Instance { get; private set; }
    
    [Header("Scanner Ship Database")]
    [SerializeField] private List<ScannerShipModel> scannerShips = new List<ScannerShipModel>();
    
    [Header("Auto-Population Settings")]
    [SerializeField] private string resourceBasePath = "ScannerShips";
    [SerializeField] private bool autoPopulateOnAwake = true;
    [SerializeField] private bool clearOnAutoPopulate = false; // Option to clear existing ships first
    
    // Static lookup dictionaries for quick access (similar to ShipDatabase)
    private static Dictionary<ShipRarity, List<ScannerShipModel>> shipsByRarity = new Dictionary<ShipRarity, List<ScannerShipModel>>();
    private static Dictionary<string, List<ScannerShipModel>> shipsByLocation = new Dictionary<string, List<ScannerShipModel>>();
    private static Dictionary<string, ScannerShipModel> shipsByName = new Dictionary<string, ScannerShipModel>();
    
    // Static flag to track if database has been initialized
    private static bool isStaticDataInitialized = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize static data if not already done
            if (!isStaticDataInitialized || autoPopulateOnAwake)
            {
                InitializeStaticDatabase();
            }
            
            Debug.Log($"ScannerShipDatabase: Initialized with {GetTotalShipCount()} ships");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initialize the static database - can be called from anywhere
    /// </summary>
    public static void InitializeStaticDatabase()
    {
        Debug.Log("ScannerShipDatabase: Initializing static database...");
        
        if (Instance != null)
        {
            // If instance exists, use its settings
            if (Instance.clearOnAutoPopulate)
            {
                Instance.scannerShips.Clear();
            }
            
            // Auto-populate if enabled
            if (Instance.autoPopulateOnAwake)
            {
                Instance.AutoPopulateDatabase();
            }
            
            // Build static lookups from instance data
            BuildStaticLookups(Instance.scannerShips);
        }
        else
        {
            // No instance - create minimal default data
            List<ScannerShipModel> defaultShips = CreateDefaultShips();
            BuildStaticLookups(defaultShips);
        }
        
        isStaticDataInitialized = true;
        Debug.Log($"ScannerShipDatabase: Static initialization complete with {GetTotalShipCountStatic()} ships");
    }
    
    /// <summary>
    /// Build static lookup dictionaries from ship list
    /// </summary>
    private static void BuildStaticLookups(List<ScannerShipModel> ships)
    {
        // Clear existing lookups
        shipsByRarity.Clear();
        shipsByLocation.Clear();
        shipsByName.Clear();
        
        // Initialize rarity dictionary with empty lists
        foreach (ShipRarity rarity in System.Enum.GetValues(typeof(ShipRarity)))
        {
            shipsByRarity[rarity] = new List<ScannerShipModel>();
        }
        
        int validShips = 0;
        int invalidShips = 0;
        
        // Process each ship model with validation
        foreach (var ship in ships)
        {
            if (ship == null || string.IsNullOrEmpty(ship.shipName))
            {
                invalidShips++;
                continue;
            }
            
            // IMPROVEMENT: Validate ship data
            if (!ship.IsValid())
            {
                Debug.LogWarning($"ScannerShipDatabase: Invalid ship data for '{ship.shipName}', skipping");
                invalidShips++;
                continue;
            }
            
            // Add to rarity lookup
            if (!shipsByRarity.ContainsKey(ship.rarity))
            {
                shipsByRarity[ship.rarity] = new List<ScannerShipModel>();
            }
            shipsByRarity[ship.rarity].Add(ship);
            
            // Add to location lookup (handle empty locations list)
            if (ship.availableLocations.Count == 0)
            {
                // If no specific locations, add to "All Locations"
                if (!shipsByLocation.ContainsKey("All Locations"))
                {
                    shipsByLocation["All Locations"] = new List<ScannerShipModel>();
                }
                shipsByLocation["All Locations"].Add(ship);
            }
            else
            {
                foreach (var location in ship.availableLocations)
                {
                    if (!shipsByLocation.ContainsKey(location))
                    {
                        shipsByLocation[location] = new List<ScannerShipModel>();
                    }
                    shipsByLocation[location].Add(ship);
                }
            }
            
            // Add to name lookup (handle duplicates)
            if (shipsByName.ContainsKey(ship.shipName))
            {
                Debug.LogWarning($"ScannerShipDatabase: Duplicate ship name '{ship.shipName}', overwriting previous entry");
            }
            shipsByName[ship.shipName] = ship;
            
            validShips++;
        }
        
        Debug.Log($"ScannerShipDatabase: Static lookups built - {validShips} valid ships, {invalidShips} invalid/skipped");
    }
    
    /// <summary>
    /// Create default ships when no instance data is available
    /// </summary>
    private static List<ScannerShipModel> CreateDefaultShips()
    {
        List<ScannerShipModel> defaultShips = new List<ScannerShipModel>();
        
        // Create a few basic default ships
        defaultShips.Add(CreateDefaultScannerShip("Basic Scavenger", ShipRarity.Common, "Scavenger Vessel", 
            "A basic scavenging ship found in most sectors.", 
            new string[] { "All Locations" }, 10, 20));
            
        defaultShips.Add(CreateDefaultScannerShip("Trade Runner", ShipRarity.Common, "Trading Vessel",
            "Standard trading ship for commercial routes.",
            new string[] { "All Locations" }, 15, 25));
            
        defaultShips.Add(CreateDefaultScannerShip("Research Probe", ShipRarity.Rare, "Science Vessel",
            "Advanced research vessel with valuable data.",
            new string[] { "All Locations" }, 30, 50));
        
        return defaultShips;
    }
    
    /// <summary>
    /// Helper to create default scanner ship models
    /// </summary>
    private static ScannerShipModel CreateDefaultScannerShip(string name, ShipRarity rarity, string shipClass, 
        string description, string[] locations, int minTrade, int maxTrade)
    {
        ScannerShipModel ship = new ScannerShipModel
        {
            shipName = name,
            rarity = rarity,
            shipClass = shipClass,
            description = description,
            availableLocations = locations.ToList(),
            minTradeValue = minTrade,
            maxTradeValue = maxTrade,
            availableTradeGoods = new List<string> { "Basic Components", "Ship Parts" }
        };
        
        return ship;
    }
    
    /// <summary>
    /// Auto-populate the database from various sources
    /// </summary>
    public void AutoPopulateDatabase()
    {
        Debug.Log("ScannerShipDatabase: Starting auto-population...");
        
        // Add default scanner ships if list is empty
        if (scannerShips.Count == 0)
        {
            AddDefaultScannerShips();
        }
        
        // Try to load additional ships from Resources
        LoadShipsFromResources();
        
        // Import from JSON if importer is available
        TryImportFromJSON();
        
        Debug.Log($"ScannerShipDatabase: Auto-population complete. {scannerShips.Count} ships loaded.");
    }
    
    /// <summary>
    /// Add default scanner ships to the instance list
    /// </summary>
    private void AddDefaultScannerShips()
    {
        List<ScannerShipModel> defaults = CreateDefaultShips();
        scannerShips.AddRange(defaults);
        Debug.Log($"ScannerShipDatabase: Added {defaults.Count} default ships");
    }
    
    /// <summary>
    /// Try to import ships from JSON using the importer
    /// </summary>
    private void TryImportFromJSON()
    {
        var importer = FindObjectOfType<ScannerShipImporter>();
        if (importer != null)
        {
            Debug.Log("ScannerShipDatabase: Found importer, importing JSON ships...");
            importer.ImportAllShips();
        }
        else
        {
            Debug.Log("ScannerShipDatabase: No importer found, skipping JSON import");
        }
    }
    
    /// <summary>
    /// Load ships from Resources folder (placeholder for future expansion)
    /// </summary>
    private void LoadShipsFromResources()
    {
        // For now, this is disabled in favor of JSON import system
        // Future implementation could load from ScriptableObject assets
        Debug.Log("ScannerShipDatabase: Resources loading disabled - using JSON import system");
    }
    
    /// <summary>
    /// STATIC METHOD: Get a random ship of the specified rarity
    /// </summary>
    public static ScannerShipModel GetRandomShipByRarityStatic(ShipRarity rarity)
    {
        if (!isStaticDataInitialized)
        {
            InitializeStaticDatabase();
        }
        
        if (!shipsByRarity.ContainsKey(rarity) || shipsByRarity[rarity].Count == 0)
        {
            Debug.LogWarning($"ScannerShipDatabase: No ships found for rarity {rarity}");
            return null;
        }
        
        var availableShips = shipsByRarity[rarity];
        return availableShips[Random.Range(0, availableShips.Count)];
    }
    
    /// <summary>
    /// IMPROVED: More efficient location filtering with caching
    /// </summary>
    public static ScannerShipModel GetRandomShipByRarityAndLocationStatic(ShipRarity rarity, string currentLocation)
    {
        if (!isStaticDataInitialized)
        {
            InitializeStaticDatabase();
        }
        
        if (!shipsByRarity.ContainsKey(rarity) || shipsByRarity[rarity].Count == 0)
        {
            Debug.LogWarning($"ScannerShipDatabase: No ships found for rarity {rarity}");
            return GetRandomShipByRarityStatic(rarity); // Fallback
        }
        
        // OPTIMIZATION: Create a weighted selection list
        List<ScannerShipModel> weightedShips = new List<ScannerShipModel>();
        
        foreach (var ship in shipsByRarity[rarity])
        {
            if (ship.IsAvailableInLocation(currentLocation))
            {
                // Add ship multiple times based on spawn weight for weighted random selection
                int weight = Mathf.RoundToInt(ship.spawnWeight);
                for (int i = 0; i < weight; i++)
                {
                    weightedShips.Add(ship);
                }
            }
        }
        
        if (weightedShips.Count == 0)
        {
            Debug.LogWarning($"ScannerShipDatabase: No {rarity} ships available in {currentLocation}");
            return GetRandomShipByRarityStatic(rarity); // Fallback
        }
        
        return weightedShips[Random.Range(0, weightedShips.Count)];
    }
    
    /// <summary>
    /// STATIC METHOD: Get ship by name
    /// </summary>
    public static ScannerShipModel GetShipByNameStatic(string shipName)
    {
        if (!isStaticDataInitialized)
        {
            InitializeStaticDatabase();
        }
        
        return shipsByName.TryGetValue(shipName, out ScannerShipModel ship) ? ship : null;
    }
    
    /// <summary>
    /// STATIC METHOD: Get all ships in a location
    /// </summary>
    public static List<ScannerShipModel> GetShipsInLocationStatic(string location)
    {
        if (!isStaticDataInitialized)
        {
            InitializeStaticDatabase();
        }
        
        if (shipsByLocation.ContainsKey(location))
        {
            return new List<ScannerShipModel>(shipsByLocation[location]);
        }
        
        return new List<ScannerShipModel>();
    }
    
    /// <summary>
    /// STATIC METHOD: Get total ship count
    /// </summary>
    public static int GetTotalShipCountStatic()
    {
        if (!isStaticDataInitialized)
        {
            InitializeStaticDatabase();
        }
        
        return shipsByRarity.SelectMany(kvp => kvp.Value).Count();
    }
    
    /// <summary>
    /// STATIC METHOD: Add a ship to the static database
    /// </summary>
    public static void AddShipStatic(ScannerShipModel ship)
    {
        if (ship == null || string.IsNullOrEmpty(ship.shipName)) return;
        
        if (!isStaticDataInitialized)
        {
            InitializeStaticDatabase();
        }
        
        // Add to instance list if instance exists
        if (Instance != null)
        {
            Instance.scannerShips.Add(ship);
        }
        
        // Add to static lookups
        if (!shipsByRarity.ContainsKey(ship.rarity))
        {
            shipsByRarity[ship.rarity] = new List<ScannerShipModel>();
        }
        shipsByRarity[ship.rarity].Add(ship);
        
        foreach (var location in ship.availableLocations)
        {
            if (!shipsByLocation.ContainsKey(location))
            {
                shipsByLocation[location] = new List<ScannerShipModel>();
            }
            shipsByLocation[location].Add(ship);
        }
        
        shipsByName[ship.shipName] = ship;
        
        Debug.Log($"ScannerShipDatabase: Added ship to static database - {ship.shipName}");
    }
    
    /// <summary>
    /// STATIC METHOD: Remove a ship from the static database
    /// </summary>
    public static bool RemoveShipStatic(string shipName)
    {
        if (!isStaticDataInitialized)
        {
            InitializeStaticDatabase();
        }
        
        var ship = GetShipByNameStatic(shipName);
        if (ship == null) return false;
        
        // Remove from instance list if instance exists
        if (Instance != null)
        {
            Instance.scannerShips.Remove(ship);
        }
        
        // Remove from static lookups
        if (shipsByRarity.ContainsKey(ship.rarity))
        {
            shipsByRarity[ship.rarity].Remove(ship);
        }
        
        foreach (var location in ship.availableLocations)
        {
            if (shipsByLocation.ContainsKey(location))
            {
                shipsByLocation[location].Remove(ship);
            }
        }
        
        shipsByName.Remove(shipName);
        
        Debug.Log($"ScannerShipDatabase: Removed ship from static database - {shipName}");
        return true;
    }
    
    /// <summary>
    /// STATIC METHOD: Rebuild static lookups (useful after bulk operations)
    /// </summary>
    public static void RefreshStaticDatabase()
    {
        if (Instance != null)
        {
            BuildStaticLookups(Instance.scannerShips);
        }
        Debug.Log("ScannerShipDatabase: Static database refreshed");
    }
    
    // ===== INSTANCE METHODS (for backwards compatibility) =====
    
    /// <summary>
    /// Get a random ship of the specified rarity (instance method)
    /// </summary>
    public ScannerShipModel GetRandomShipByRarity(ShipRarity rarity)
    {
        return GetRandomShipByRarityStatic(rarity);
    }
    
    /// <summary>
    /// Get a random ship by rarity and location (instance method)
    /// </summary>
    public ScannerShipModel GetRandomShipByRarityAndLocation(ShipRarity rarity, string currentLocation)
    {
        return GetRandomShipByRarityAndLocationStatic(rarity, currentLocation);
    }
    
    /// <summary>
    /// Get a ship by its exact name (instance method)
    /// </summary>
    public ScannerShipModel GetShipByName(string shipName)
    {
        return GetShipByNameStatic(shipName);
    }
    
    /// <summary>
    /// Get all ships available in a specific location (instance method)
    /// </summary>
    public List<ScannerShipModel> GetShipsInLocation(string location)
    {
        return GetShipsInLocationStatic(location);
    }
    
    /// <summary>
    /// Get all ships of a specific rarity (instance method)
    /// </summary>
    public List<ScannerShipModel> GetShipsByRarity(ShipRarity rarity)
    {
        if (!isStaticDataInitialized)
        {
            InitializeStaticDatabase();
        }
        
        if (shipsByRarity.ContainsKey(rarity))
        {
            return new List<ScannerShipModel>(shipsByRarity[rarity]);
        }
        
        return new List<ScannerShipModel>();
    }
    
    /// <summary>
    /// Get total number of ships in database (instance method)
    /// </summary>
    public int GetTotalShipCount()
    {
        return GetTotalShipCountStatic();
    }
    
    /// <summary>
    /// Get breakdown of ships by rarity (instance method)
    /// </summary>
    public Dictionary<ShipRarity, int> GetShipCountByRarity()
    {
        if (!isStaticDataInitialized)
        {
            InitializeStaticDatabase();
        }
        
        Dictionary<ShipRarity, int> counts = new Dictionary<ShipRarity, int>();
        
        foreach (var rarity in System.Enum.GetValues(typeof(ShipRarity)))
        {
            ShipRarity rarityEnum = (ShipRarity)rarity;
            counts[rarityEnum] = shipsByRarity.ContainsKey(rarityEnum) ? shipsByRarity[rarityEnum].Count : 0;
        }
        
        return counts;
    }
    
    /// <summary>
    /// Add a new ship to the database (instance method)
    /// </summary>
    public void AddShip(ScannerShipModel ship)
    {
        AddShipStatic(ship);
    }
    
    /// <summary>
    /// Remove a ship from the database (instance method)
    /// </summary>
    public bool RemoveShip(string shipName)
    {
        return RemoveShipStatic(shipName);
    }
    
    /// <summary>
    /// Refresh the database lookups (instance method)
    /// </summary>
    public void RefreshDatabase()
    {
        RefreshStaticDatabase();
    }
    
    /// <summary>
    /// Clear all ships and reload (instance method)
    /// </summary>
    [ContextMenu("Reload Database")]
    public void ReloadDatabase()
    {
        scannerShips.Clear();
        AddDefaultScannerShips();
        TryImportFromJSON();
        RefreshStaticDatabase();
        Debug.Log($"ScannerShipDatabase: Database reloaded with {scannerShips.Count} ships");
    }
    
    /// <summary>
    /// Show database statistics (debugging)
    /// </summary>
    [ContextMenu("Show Database Statistics")]
    public void ShowDatabaseStatistics()
    {
        Debug.Log("=== SCANNER SHIP DATABASE STATISTICS ===");
        Debug.Log($"Total Ships: {GetTotalShipCount()}");
        Debug.Log($"Static Initialization: {isStaticDataInitialized}");
        
        var rarityBreakdown = GetShipCountByRarity();
        foreach (var kvp in rarityBreakdown)
        {
            if (kvp.Value > 0)
                Debug.Log($"{kvp.Key}: {kvp.Value} ships");
        }
        
        if (scannerShips.Count > 0)
        {
            Debug.Log($"Average Trade Value: {scannerShips.Average(s => (s.minTradeValue + s.maxTradeValue) / 2f):F1}");
            Debug.Log($"Locations Covered: {scannerShips.SelectMany(s => s.availableLocations).Distinct().Count()}");
        }
    }
    
    /// <summary>
    /// NEW: Get ships by multiple criteria for advanced filtering
    /// </summary>
    public static List<ScannerShipModel> GetShipsByMultipleCriteria(
        ShipRarity? rarity = null, 
        string location = null, 
        int? minLevel = null, 
        int? maxLevel = null,
        bool? canTrade = null,
        bool? canFight = null)
    {
        if (!isStaticDataInitialized)
        {
            InitializeStaticDatabase();
        }
        
        List<ScannerShipModel> results = new List<ScannerShipModel>();
        
        // Start with all ships or filter by rarity first
        var candidateShips = rarity.HasValue ? 
            (shipsByRarity.ContainsKey(rarity.Value) ? shipsByRarity[rarity.Value] : new List<ScannerShipModel>()) :
            shipsByRarity.SelectMany(kvp => kvp.Value).ToList();
        
        foreach (var ship in candidateShips)
        {
            // Apply filters
            if (location != null && !ship.IsAvailableInLocation(location))
                continue;
            
            if (minLevel.HasValue && ship.maxLevel < minLevel.Value)
                continue;
            
            if (maxLevel.HasValue && ship.minLevel > maxLevel.Value)
                continue;
            
            if (canTrade.HasValue && ship.canAlwaysTrade != canTrade.Value)
                continue;
            
            if (canFight.HasValue && ship.canAlwaysFight != canFight.Value)
                continue;
            
            results.Add(ship);
        }
        
        return results;
    }
    
    /// <summary>
    /// NEW: Get ship statistics for debugging/balancing
    /// </summary>
    public static Dictionary<string, object> GetDatabaseStatistics()
    {
        if (!isStaticDataInitialized)
        {
            InitializeStaticDatabase();
        }
        
        var stats = new Dictionary<string, object>();
        
        // Basic counts
        stats["TotalShips"] = GetTotalShipCountStatic();
        stats["UniqueLocations"] = shipsByLocation.Keys.Count;
        
        // Rarity breakdown
        var rarityStats = new Dictionary<ShipRarity, int>();
        foreach (var kvp in shipsByRarity)
        {
            rarityStats[kvp.Key] = kvp.Value.Count;
        }
        stats["RarityBreakdown"] = rarityStats;
        
        // Level range analysis
        if (shipsByRarity.SelectMany(kvp => kvp.Value).Any())
        {
            var allShips = shipsByRarity.SelectMany(kvp => kvp.Value);
            stats["MinLevel"] = allShips.Min(s => s.minLevel);
            stats["MaxLevel"] = allShips.Max(s => s.maxLevel);
            stats["AvgTradeValue"] = allShips.Average(s => (s.minTradeValue + s.maxTradeValue) / 2f);
        }
        
        // Location distribution
        var locationStats = new Dictionary<string, int>();
        foreach (var kvp in shipsByLocation)
        {
            locationStats[kvp.Key] = kvp.Value.Count;
        }
        stats["LocationDistribution"] = locationStats;
        
        return stats;
    }
}