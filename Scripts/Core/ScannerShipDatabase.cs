using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Fixed ScannerShipDatabase - simplified static implementation to prevent loading issues
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
    [SerializeField] private bool clearOnAutoPopulate = false;
    
    // SIMPLIFIED: Non-static lookup dictionaries first, static later
    private Dictionary<ShipRarity, List<ScannerShipModel>> shipsByRarity = new Dictionary<ShipRarity, List<ScannerShipModel>>();
    private Dictionary<string, List<ScannerShipModel>> shipsByLocation = new Dictionary<string, List<ScannerShipModel>>();
    private Dictionary<string, ScannerShipModel> shipsByName = new Dictionary<string, ScannerShipModel>();
    
    // Static lookups - only populated when needed
    private static Dictionary<ShipRarity, List<ScannerShipModel>> staticShipsByRarity;
    private static Dictionary<string, List<ScannerShipModel>> staticShipsByLocation;
    private static Dictionary<string, ScannerShipModel> staticShipsByName;
    private static bool staticDataReady = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // SIMPLIFIED: Just initialize instance data first
            if (autoPopulateOnAwake)
            {
                SimpleAutoPopulate();
            }
            
            InitializeLookups();
            Debug.Log($"ScannerShipDatabase: Initialized with {scannerShips.Count} ships");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// SIMPLIFIED: Basic auto-populate without complex logic
    /// </summary>
    private void SimpleAutoPopulate()
    {
        if (clearOnAutoPopulate)
        {
            scannerShips.Clear();
        }
        
        // Add defaults if empty
        if (scannerShips.Count == 0)
        {
            AddBasicDefaultShips();
        }
    }
    
    /// <summary>
    /// SIMPLIFIED: Just add 3 basic ships, no complex logic
    /// </summary>
    private void AddBasicDefaultShips()
    {
        scannerShips.Add(CreateBasicShip("Basic Scavenger", ShipRarity.Common, "Scavenger Vessel", 10, 20));
        scannerShips.Add(CreateBasicShip("Trade Runner", ShipRarity.Common, "Trading Vessel", 15, 25));
        scannerShips.Add(CreateBasicShip("Research Probe", ShipRarity.Rare, "Science Vessel", 30, 50));
        
        Debug.Log($"ScannerShipDatabase: Added {scannerShips.Count} default ships");
    }
    
    /// <summary>
    /// Helper to create basic ships
    /// </summary>
    private ScannerShipModel CreateBasicShip(string name, ShipRarity rarity, string shipClass, int minTrade, int maxTrade)
    {
        return new ScannerShipModel
        {
            shipName = name,
            rarity = rarity,
            shipClass = shipClass,
            description = $"A {rarity} {shipClass} found in space.",
            availableLocations = new List<string> { "All Locations" },
            minTradeValue = minTrade,
            maxTradeValue = maxTrade,
            availableTradeGoods = new List<string> { "Basic Components", "Ship Parts" }
        };
    }
    
    /// <summary>
    /// SIMPLIFIED: Build instance lookups only
    /// </summary>
    private void InitializeLookups()
    {
        shipsByRarity.Clear();
        shipsByLocation.Clear();
        shipsByName.Clear();
        
        // Initialize rarity dictionary
        foreach (ShipRarity rarity in System.Enum.GetValues(typeof(ShipRarity)))
        {
            shipsByRarity[rarity] = new List<ScannerShipModel>();
        }
        
        // Process ships
        foreach (var ship in scannerShips)
        {
            if (ship == null || string.IsNullOrEmpty(ship.shipName)) continue;
            
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
        }
        
        Debug.Log($"ScannerShipDatabase: Lookups built for {scannerShips.Count} ships");
    }
    
    /// <summary>
    /// Copy instance data to static lookups when needed
    /// </summary>
    private void EnsureStaticData()
    {
        if (staticDataReady) return;
        
        staticShipsByRarity = new Dictionary<ShipRarity, List<ScannerShipModel>>();
        staticShipsByLocation = new Dictionary<string, List<ScannerShipModel>>();
        staticShipsByName = new Dictionary<string, ScannerShipModel>();
        
        // Copy from instance data
        foreach (var kvp in shipsByRarity)
        {
            staticShipsByRarity[kvp.Key] = new List<ScannerShipModel>(kvp.Value);
        }
        foreach (var kvp in shipsByLocation)
        {
            staticShipsByLocation[kvp.Key] = new List<ScannerShipModel>(kvp.Value);
        }
        foreach (var kvp in shipsByName)
        {
            staticShipsByName[kvp.Key] = kvp.Value;
        }
        
        staticDataReady = true;
    }
    
    // ===== STATIC METHODS (simplified) =====
    
    /// <summary>
    /// STATIC: Get total ship count
    /// </summary>
    public static int GetTotalShipCountStatic()
    {
        if (Instance == null) return 0;
        Instance.EnsureStaticData();
        return staticShipsByRarity?.SelectMany(kvp => kvp.Value).Count() ?? 0;
    }
    
    /// <summary>
    /// STATIC: Get random ship by rarity
    /// </summary>
    public static ScannerShipModel GetRandomShipByRarityStatic(ShipRarity rarity)
    {
        if (Instance == null) return null;
        Instance.EnsureStaticData();
        
        if (staticShipsByRarity?.ContainsKey(rarity) == true && staticShipsByRarity[rarity].Count > 0)
        {
            var ships = staticShipsByRarity[rarity];
            return ships[Random.Range(0, ships.Count)];
        }
        
        return null;
    }
    
    /// <summary>
    /// STATIC: Get random ship by rarity and location
    /// </summary>
    public static ScannerShipModel GetRandomShipByRarityAndLocationStatic(ShipRarity rarity, string currentLocation)
    {
        if (Instance == null) return null;
        Instance.EnsureStaticData();
        
        if (staticShipsByRarity?.ContainsKey(rarity) == true && staticShipsByRarity[rarity].Count > 0)
        {
            var availableShips = staticShipsByRarity[rarity]
                .Where(ship => ship.availableLocations.Contains(currentLocation) || 
                              ship.availableLocations.Contains("All Locations") ||
                              ship.availableLocations.Count == 0)
                .ToList();
            
            if (availableShips.Count > 0)
            {
                return availableShips[Random.Range(0, availableShips.Count)];
            }
        }
        
        // Fallback to any ship of this rarity
        return GetRandomShipByRarityStatic(rarity);
    }
    
    /// <summary>
    /// STATIC: Get ship by name
    /// </summary>
    public static ScannerShipModel GetShipByNameStatic(string shipName)
    {
        if (Instance == null) return null;
        Instance.EnsureStaticData();
        
        return staticShipsByName?.TryGetValue(shipName, out ScannerShipModel ship) == true ? ship : null;
    }
    
    /// <summary>
    /// STATIC: Add ship
    /// </summary>
    public static void AddShipStatic(ScannerShipModel ship)
    {
        if (Instance == null || ship == null) return;
        
        Instance.scannerShips.Add(ship);
        Instance.InitializeLookups();
        staticDataReady = false; // Force refresh
    }
    
    // ===== INSTANCE METHODS (for backwards compatibility) =====
    
    public ScannerShipModel GetRandomShipByRarity(ShipRarity rarity)
    {
        return GetRandomShipByRarityStatic(rarity);
    }
    
    public ScannerShipModel GetRandomShipByRarityAndLocation(ShipRarity rarity, string currentLocation)
    {
        return GetRandomShipByRarityAndLocationStatic(rarity, currentLocation);
    }
    
    public ScannerShipModel GetShipByName(string shipName)
    {
        return GetShipByNameStatic(shipName);
    }
    
    public int GetTotalShipCount()
    {
        return scannerShips.Count;
    }
    
    public void AddShip(ScannerShipModel ship)
    {
        AddShipStatic(ship);
    }
    
    public bool RemoveShip(string shipName)
    {
        var ship = scannerShips.FirstOrDefault(s => s.shipName == shipName);
        if (ship != null)
        {
            scannerShips.Remove(ship);
            InitializeLookups();
            staticDataReady = false;
            return true;
        }
        return false;
    }
    
    public void RefreshDatabase()
    {
        InitializeLookups();
        staticDataReady = false;
    }
    
    public Dictionary<ShipRarity, int> GetShipCountByRarity()
    {
        Dictionary<ShipRarity, int> counts = new Dictionary<ShipRarity, int>();
        foreach (var rarity in System.Enum.GetValues(typeof(ShipRarity)))
        {
            ShipRarity rarityEnum = (ShipRarity)rarity;
            counts[rarityEnum] = shipsByRarity.ContainsKey(rarityEnum) ? shipsByRarity[rarityEnum].Count : 0;
        }
        return counts;
    }
    
    [ContextMenu("Show Database Statistics")]
    public void ShowDatabaseStatistics()
    {
        Debug.Log("=== SCANNER SHIP DATABASE STATISTICS ===");
        Debug.Log($"Total Ships: {scannerShips.Count}");
        Debug.Log($"Static Data Ready: {staticDataReady}");
        
        var rarityBreakdown = GetShipCountByRarity();
        foreach (var kvp in rarityBreakdown)
        {
            if (kvp.Value > 0)
                Debug.Log($"{kvp.Key}: {kvp.Value} ships");
        }
    }

    /// <summary>
    /// ADDED: Initialize static database - can be called from anywhere
    /// </summary>
    public static void InitializeStaticDatabase()
    {
        if (Instance == null)
        {
            Debug.LogWarning("ScannerShipDatabase: No instance found, creating minimal database");
            var go = new GameObject("ScannerShipDatabase");
            Instance = go.AddComponent<ScannerShipDatabase>();
            Instance.SimpleAutoPopulate();
        }
        
        Instance.InitializeLookups();
        Instance.EnsureStaticData();
        Debug.Log($"ScannerShipDatabase: Static database initialized with {Instance.GetTotalShipCount()} ships");
    }

    /// <summary>
    /// ADDED: Get detailed database statistics
    /// </summary>
    public static Dictionary<string, object> GetDatabaseStatistics()
    {
        if (Instance == null) return new Dictionary<string, object>();
        Instance.EnsureStaticData();
        
        var stats = new Dictionary<string, object>();
        
        // Basic counts
        stats["TotalShips"] = Instance.GetTotalShipCount();
        stats["UniqueLocations"] = Instance.shipsByLocation.Keys.Count;
        
        // Rarity breakdown
        stats["RarityBreakdown"] = Instance.GetShipCountByRarity();
        
        // Level range analysis
        var allShips = Instance.scannerShips;
        if (allShips.Any())
        {
            stats["MinLevel"] = allShips.Min(s => s.minLevel);
            stats["MaxLevel"] = allShips.Max(s => s.maxLevel);
            stats["AvgTradeValue"] = allShips.Average(s => (s.minTradeValue + s.maxTradeValue) / 2f);
        }
        
        // Location distribution
        var locationStats = new Dictionary<string, int>();
        foreach (var kvp in Instance.shipsByLocation)
        {
            locationStats[kvp.Key] = kvp.Value.Count;
        }
        stats["LocationDistribution"] = locationStats;
        
        return stats;
    }

    /// <summary>
    /// ADDED: Reload the entire database
    /// </summary>
    [ContextMenu("Reload Database")]
    public void ReloadDatabase()
    {
        Debug.Log("ScannerShipDatabase: Reloading database...");
        
        // Clear existing data
        scannerShips.Clear();
        shipsByRarity.Clear();
        shipsByLocation.Clear();
        shipsByName.Clear();
        
        // Reset static data
        staticDataReady = false;
        staticShipsByRarity = null;
        staticShipsByLocation = null;
        staticShipsByName = null;
        
        // Repopulate
        SimpleAutoPopulate();
        InitializeLookups();
        EnsureStaticData();
        
        Debug.Log($"ScannerShipDatabase: Database reloaded with {scannerShips.Count} ships");
    }
}