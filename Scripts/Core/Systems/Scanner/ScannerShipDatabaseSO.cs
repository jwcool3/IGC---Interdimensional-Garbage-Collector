using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ScriptableObject version of the ship database that persists in the project
/// This will save your ships even when the game stops
/// </summary>
[CreateAssetMenu(fileName = "ScannerShipDatabase", menuName = "Ship Scanner/Ship Database", order = 1)]
public class ScannerShipDatabaseSO : ScriptableObject
{
    [Header("Ship Database")]
    [SerializeField] private List<ScannerShipModel> scannerShips = new List<ScannerShipModel>();

    [Header("Auto-Population Settings")]
    [SerializeField] private bool autoAddDefaultShips = true;
    [SerializeField] private bool clearBeforeAdding = false;

    // Runtime lookup dictionaries (rebuilt when needed)
    private Dictionary<ShipRarity, List<ScannerShipModel>> shipsByRarity;
    private Dictionary<string, List<ScannerShipModel>> shipsByLocation;
    private Dictionary<string, ScannerShipModel> shipsByName;
    private bool lookupsBuilt = false;

    /// <summary>
    /// Initialize the database - call this when the game starts
    /// </summary>
    public void Initialize()
    {
        // Add default ships if the list is empty
        if (autoAddDefaultShips && scannerShips.Count == 0)
        {
            AddDefaultShips();
        }

        BuildLookups();
        Debug.Log($"ScannerShipDatabaseSO: Initialized with {scannerShips.Count} ships");
    }

    /// <summary>
    /// Add default ships to the database
    /// </summary>
    [ContextMenu("Add Default Ships")]
    public void AddDefaultShips()
    {
        if (clearBeforeAdding)
        {
            scannerShips.Clear();
        }

        // Create default ships
        var defaultShips = new List<ScannerShipModel>
        {
            CreateDefaultShip("Stellar Wanderer", ShipRarity.VeryCommon, "Civilian Vessel", 8, 15),
            CreateDefaultShip("Void Collector", ShipRarity.Common, "Scavenger Ship", 12, 22),
            CreateDefaultShip("Quantum Explorer", ShipRarity.Common, "Research Vessel", 15, 25),
            CreateDefaultShip("Nebula Merchant", ShipRarity.SlightlyRare, "Trading Ship", 20, 35),
            CreateDefaultShip("Cosmic Drifter", ShipRarity.SlightlyRare, "Explorer Craft", 18, 30),

            CreateDefaultShip("Galactic Hunter", ShipRarity.Rare, "Military Vessel", 35, 60),
            CreateDefaultShip("Dimensional Seeker", ShipRarity.Rare, "Science Ship", 40, 65),
            CreateDefaultShip("Omega Battlecruiser", ShipRarity.Epic, "War Ship", 70, 120),
            CreateDefaultShip("Infinity Prime", ShipRarity.Legendary, "Command Vessel", 150, 250),
            CreateDefaultShip("Reality Phantom", ShipRarity.Anomaly, "Unknown Entity", 200, 300)
        };

        foreach (var ship in defaultShips)
        {
            if (!scannerShips.Any(s => s.shipName == ship.shipName))
            {
                scannerShips.Add(ship);
            }
        }

        // Mark the asset as dirty so Unity saves it
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        BuildLookups();
        Debug.Log($"Added default ships. Total ships: {scannerShips.Count}");
    }

    /// <summary>
    /// Create a default ship with specified parameters
    /// </summary>
    private ScannerShipModel CreateDefaultShip(string name, ShipRarity rarity, string shipClass, int minTrade, int maxTrade)
    {
        var ship = new ScannerShipModel
        {
            shipName = name,
            rarity = rarity,
            shipClass = shipClass,
            description = $"A {rarity} {shipClass} encountered in the depths of space.",
            availableLocations = new List<string> { "All Locations" },
            spawnWeight = 1f,
            minTradeValue = minTrade,
            maxTradeValue = maxTrade,
            minLevel = GetDefaultMinLevel(rarity),
            maxLevel = GetDefaultMaxLevel(rarity),
            availableTradeGoods = GetDefaultTradeGoods(rarity),
            canAlwaysFight = true,
            canAlwaysTrade = true,
            isUnique = rarity == ShipRarity.Anomaly,
            attackMultiplier = GetStatMultiplier(rarity, 1.0f),
            defenseMultiplier = GetStatMultiplier(rarity, 1.0f),
            healthMultiplier = GetStatMultiplier(rarity, 1.0f)
        };

        return ship;
    }

    /// <summary>
    /// Get default minimum level for a rarity
    /// </summary>
    private int GetDefaultMinLevel(ShipRarity rarity)
    {
        switch (rarity)
        {
            case ShipRarity.VeryCommon: return 1;
            case ShipRarity.Common: return 2;
            case ShipRarity.SlightlyRare: return 4;
            case ShipRarity.Rare: return 6;
            case ShipRarity.Epic: return 10;
            case ShipRarity.Legendary: return 15;
            case ShipRarity.Anomaly: return 20;
            default: return 1;
        }
    }

    /// <summary>
    /// Get default maximum level for a rarity
    /// </summary>
    private int GetDefaultMaxLevel(ShipRarity rarity)
    {
        switch (rarity)
        {
            case ShipRarity.VeryCommon: return 3;
            case ShipRarity.Common: return 5;
            case ShipRarity.SlightlyRare: return 8;
            case ShipRarity.Rare: return 12;
            case ShipRarity.Epic: return 18;
            case ShipRarity.Legendary: return 25;
            case ShipRarity.Anomaly: return 30;
            default: return 5;
        }
    }

    /// <summary>
    /// Get default trade goods for a rarity
    /// </summary>
    private List<string> GetDefaultTradeGoods(ShipRarity rarity)
    {
        var goods = new List<string>();

        // Basic goods for all ships
        goods.Add("Basic Hull Plating");
        goods.Add("Standard Components");

        // Add advanced goods for higher rarities
        if (rarity >= ShipRarity.SlightlyRare)
        {
            goods.Add("Advanced Alloys");
        }

        if (rarity >= ShipRarity.Rare)
        {
            goods.Add("Quantum Processors");
            goods.Add("Energy Crystals");
        }

        if (rarity >= ShipRarity.Epic)
        {
            goods.Add("Exotic Matter");
            goods.Add("Reality Stabilizers");
        }

        if (rarity >= ShipRarity.Legendary)
        {
            goods.Add("Dimensional Cores");
            goods.Add("Infinity Fragments");
        }

        return goods;
    }

    /// <summary>
    /// Get stat multiplier based on rarity
    /// </summary>
    private float GetStatMultiplier(ShipRarity rarity, float baseMultiplier)
    {
        switch (rarity)
        {
            case ShipRarity.VeryCommon: return baseMultiplier * 0.8f;
            case ShipRarity.Common: return baseMultiplier * 1.0f;
            case ShipRarity.SlightlyRare: return baseMultiplier * 1.3f;
            case ShipRarity.Rare: return baseMultiplier * 1.7f;
            case ShipRarity.Epic: return baseMultiplier * 2.2f;
            case ShipRarity.Legendary: return baseMultiplier * 3.0f;
            case ShipRarity.Anomaly: return baseMultiplier * 2.5f;
            default: return baseMultiplier;
        }
    }

    /// <summary>
    /// Build lookup dictionaries for fast access
    /// </summary>
    public void BuildLookups()
    {
        shipsByRarity = new Dictionary<ShipRarity, List<ScannerShipModel>>();
        shipsByLocation = new Dictionary<string, List<ScannerShipModel>>();
        shipsByName = new Dictionary<string, ScannerShipModel>();

        // Initialize rarity dictionary
        foreach (ShipRarity rarity in System.Enum.GetValues(typeof(ShipRarity)))
        {
            shipsByRarity[rarity] = new List<ScannerShipModel>();
        }

        // Process each ship
        foreach (var ship in scannerShips)
        {
            if (ship == null || string.IsNullOrEmpty(ship.shipName)) continue;

            // Add to rarity lookup
            shipsByRarity[ship.rarity].Add(ship);

            // Add to location lookup
            foreach (var location in ship.availableLocations)
            {
                if (!shipsByLocation.ContainsKey(location))
                {
                    shipsByLocation[location] = new List<ScannerShipModel>();
                }
                shipsByLocation[location].Add(ship);
            }

            // Add to name lookup
            shipsByName[ship.shipName] = ship;
        }

        lookupsBuilt = true;
    }

    /// <summary>
    /// Ensure lookups are built before using them
    /// </summary>
    private void EnsureLookups()
    {
        if (!lookupsBuilt)
        {
            BuildLookups();
        }
    }

    // ===== PUBLIC ACCESS METHODS =====

    /// <summary>
    /// Get total ship count
    /// </summary>
    public int GetTotalShipCount()
    {
        return scannerShips.Count;
    }

    /// <summary>
    /// Get random ship by rarity
    /// </summary>
    public ScannerShipModel GetRandomShipByRarity(ShipRarity rarity)
    {
        EnsureLookups();

        if (shipsByRarity.ContainsKey(rarity) && shipsByRarity[rarity].Count > 0)
        {
            var ships = shipsByRarity[rarity];
            return ships[Random.Range(0, ships.Count)];
        }

        return null;
    }

    /// <summary>
    /// Get random ship by rarity and location
    /// </summary>
    public ScannerShipModel GetRandomShipByRarityAndLocation(ShipRarity rarity, string location)
    {
        EnsureLookups();

        if (shipsByRarity.ContainsKey(rarity) && shipsByRarity[rarity].Count > 0)
        {
            var availableShips = shipsByRarity[rarity]
                .Where(ship => ship.availableLocations.Contains(location) ||
                              ship.availableLocations.Contains("All Locations") ||
                              ship.availableLocations.Count == 0)
                .ToList();

            if (availableShips.Count > 0)
            {
                return availableShips[Random.Range(0, availableShips.Count)];
            }
        }

        // Fallback to any ship of this rarity
        return GetRandomShipByRarity(rarity);
    }

    /// <summary>
    /// Get ship by name
    /// </summary>
    public ScannerShipModel GetShipByName(string shipName)
    {
        EnsureLookups();
        return shipsByName.TryGetValue(shipName, out ScannerShipModel ship) ? ship : null;
    }

    /// <summary>
    /// Add a ship to the database
    /// </summary>
    public void AddShip(ScannerShipModel ship)
    {
        if (ship == null || string.IsNullOrEmpty(ship.shipName)) return;

        // Check for duplicates
        if (scannerShips.Any(s => s.shipName == ship.shipName))
        {
            Debug.LogWarning($"Ship '{ship.shipName}' already exists in database!");
            return;
        }

        scannerShips.Add(ship);
        BuildLookups();

        // Mark the asset as dirty so Unity saves it
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        Debug.Log($"Added ship '{ship.shipName}' to database");
    }

    /// <summary>
    /// Remove a ship from the database
    /// </summary>
    public bool RemoveShip(string shipName)
    {
        var ship = scannerShips.FirstOrDefault(s => s.shipName == shipName);
        if (ship != null)
        {
            scannerShips.Remove(ship);
            BuildLookups();

            // Mark the asset as dirty so Unity saves it
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif

            Debug.Log($"Removed ship '{shipName}' from database");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Get ship count by rarity
    /// </summary>
    public Dictionary<ShipRarity, int> GetShipCountByRarity()
    {
        EnsureLookups();

        var counts = new Dictionary<ShipRarity, int>();
        foreach (var rarity in System.Enum.GetValues(typeof(ShipRarity)))
        {
            ShipRarity rarityEnum = (ShipRarity)rarity;
            counts[rarityEnum] = shipsByRarity.ContainsKey(rarityEnum) ? shipsByRarity[rarityEnum].Count : 0;
        }
        return counts;
    }

    /// <summary>
    /// Clear all ships from the database
    /// </summary>
    [ContextMenu("Clear All Ships")]
    public void ClearAllShips()
    {
        scannerShips.Clear();
        BuildLookups();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        Debug.Log("Cleared all ships from database");
    }

    /// <summary>
    /// Show database statistics in console
    /// </summary>
    [ContextMenu("Show Database Statistics")]
    public void ShowDatabaseStatistics()
    {
        Debug.Log("=== SCANNER SHIP DATABASE STATISTICS ===");
        Debug.Log($"Total Ships: {scannerShips.Count}");

        var rarityBreakdown = GetShipCountByRarity();
        foreach (var kvp in rarityBreakdown)
        {
            if (kvp.Value > 0)
                Debug.Log($"{kvp.Key}: {kvp.Value} ships");
        }

        EnsureLookups();
        Debug.Log($"Unique Locations: {shipsByLocation.Keys.Count}");
        foreach (var location in shipsByLocation.Keys)
        {
            Debug.Log($"  {location}: {shipsByLocation[location].Count} ships");
        }
    }
}