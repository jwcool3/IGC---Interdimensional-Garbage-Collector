using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Dedicated ship database for the Scanner system - separate from combat enemies
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
    
    // Lookup dictionaries for quick access
    private Dictionary<ShipRarity, List<ScannerShipModel>> shipsByRarity = new Dictionary<ShipRarity, List<ScannerShipModel>>();
    private Dictionary<string, List<ScannerShipModel>> shipsByLocation = new Dictionary<string, List<ScannerShipModel>>();
    private Dictionary<string, ScannerShipModel> shipsByName = new Dictionary<string, ScannerShipModel>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (autoPopulateOnAwake)
            {
                AutoPopulateDatabase();
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
    /// Initialize lookup dictionaries for fast ship retrieval
    /// </summary>
    private void InitializeLookups()
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
        
        // Process each ship model
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
        
        Debug.Log($"ScannerShipDatabase: Lookups initialized for {scannerShips.Count} ships");
    }
    
    /// <summary>
    /// Auto-populate the database from default data
    /// </summary>
    public void AutoPopulateDatabase()
    {
        Debug.Log("ScannerShipDatabase: Starting auto-population...");
        
        // Clear existing ships
        scannerShips.Clear();
        
        // Add default scanner ships
        AddDefaultScannerShips();
        
        // Try to load additional ships from Resources
        LoadShipsFromResources();
        
        // Reinitialize lookups
        InitializeLookups();
        
        Debug.Log($"ScannerShipDatabase: Auto-population complete. {scannerShips.Count} ships available.");
    }
    
    /// <summary>
    /// Add default scanner ships to the database
    /// </summary>
    private void AddDefaultScannerShips()
    {
        // COMMENTED OUT - Now using JSON imports instead of hardcoded ships
        // Default ships are now loaded from StreamingAssets/ScannerShips/ folder
        
        Debug.Log("ScannerShipDatabase: Skipping default ships - using JSON imports only");
        
        /* ORIGINAL DEFAULT SHIPS - NOW IN JSON FILES
        
        // Very Common Ships (Common Scans)
        scannerShips.Add(CreateScannerShip("Rust Bucket", ShipRarity.VeryCommon, "Scavenger Pod", 
            "A cobbled-together vessel held together by hope and duct tape.", 
            new string[] { "Outer Rim", "Debris Fields", "Abandoned Sectors" },
            5, 15, new string[] { "Scrap Metal", "Basic Components" }));
            
        scannerShips.Add(CreateScannerShip("Junk Hauler", ShipRarity.VeryCommon, "Cargo Vessel",
            "Transports salvaged materials between stations.",
            new string[] { "Trade Routes", "Outer Rim", "Mining Zones" },
            8, 20, new string[] { "Raw Materials", "Ship Parts" }));
            
        scannerShips.Add(CreateScannerShip("Drift Runner", ShipRarity.VeryCommon, "Fast Transport",
            "Quick courier ship for urgent deliveries.",
            new string[] { "Trade Routes", "Core Systems", "Outer Rim" },
            6, 18, new string[] { "Navigation Data", "Communication Equipment" }));
        
        // Common Ships
        scannerShips.Add(CreateScannerShip("Sector Patrol", ShipRarity.Common, "Security Vessel",
            "Standard patrol craft maintaining order in outer sectors.",
            new string[] { "Patrol Routes", "Core Systems", "Trade Routes" },
            12, 25, new string[] { "Security Equipment", "Ship Parts", "Energy Cells" }));
            
        scannerShips.Add(CreateScannerShip("Mining Barge", ShipRarity.Common, "Industrial Ship",
            "Heavy-duty vessel equipped for asteroid mining operations.",
            new string[] { "Mining Zones", "Asteroid Fields", "Industrial Sectors" },
            15, 30, new string[] { "Mining Equipment", "Rare Minerals", "Industrial Components" }));
        
        // Slightly Rare Ships
        scannerShips.Add(CreateScannerShip("Corporate Escort", ShipRarity.SlightlyRare, "Guard Ship",
            "Well-armed vessel protecting corporate interests.",
            new string[] { "Core Systems", "Trade Routes", "Corporate Zones" },
            20, 35, new string[] { "Advanced Weaponry", "Corporate Tech", "Energy Shields" }));
            
        scannerShips.Add(CreateScannerShip("Deep Explorer", ShipRarity.SlightlyRare, "Research Vessel",
            "Long-range exploration ship equipped with advanced sensors.",
            new string[] { "Unknown Regions", "Deep Space", "Anomaly Zones" },
            18, 40, new string[] { "Sensor Equipment", "Research Data", "Exotic Samples" }));
        
        // Rare Ships (Rare Scans)
        scannerShips.Add(CreateScannerShip("Battle Cruiser", ShipRarity.Rare, "Warship",
            "Heavy combat vessel with military-grade weapons and armor.",
            new string[] { "War Zones", "Military Sectors", "Contested Space" },
            30, 50, new string[] { "Military Hardware", "Advanced Weapons", "Tactical Systems", "Ship Parts" }));
            
        scannerShips.Add(CreateScannerShip("Science Vessel", ShipRarity.Rare, "Research Ship",
            "Advanced laboratory ship conducting cutting-edge research.",
            new string[] { "Research Stations", "Anomaly Zones", "Deep Space" },
            25, 45, new string[] { "Research Data", "Scientific Equipment", "Alien Tech", "Quantum Processors" }));
        
        // Epic Ships
        scannerShips.Add(CreateScannerShip("Dreadnought", ShipRarity.Epic, "Capital Ship",
            "Massive warship capable of devastating entire fleets.",
            new string[] { "War Zones", "Military HQ", "Capital Defense" },
            50, 75, new string[] { "Capital Ship Parts", "Advanced Weapons", "Military Tech", "Alien Tech", "Rare Alloys" }));
            
        scannerShips.Add(CreateScannerShip("Ark Ship", ShipRarity.Epic, "Colony Vessel",
            "Enormous vessel designed to transport entire populations.",
            new string[] { "Deep Space", "Colony Routes", "Frontier Zones" },
            40, 80, new string[] { "Life Support Systems", "Advanced Tech", "Population Data", "Rare Resources" }));
        
        // Legendary Ships
        scannerShips.Add(CreateScannerShip("Titan's Pride", ShipRarity.Legendary, "Flagship",
            "Legendary flagship of the Titan Corporation's fleet.",
            new string[] { "Corporate HQ", "High Security Zones", "Capital Systems" },
            75, 100, new string[] { "Corporate Secrets", "Prototype Tech", "Alien Tech", "Quantum Cores", "Reality Stabilizers" }));
            
        scannerShips.Add(CreateScannerShip("Void Walker", ShipRarity.Legendary, "Dimensional Ship",
            "Mysterious vessel capable of traveling between dimensions.",
            new string[] { "Anomaly Zones", "Dimensional Rifts", "Unknown Regions" },
            60, 120, new string[] { "Dimensional Tech", "Quantum Processors", "Alien Tech", "Reality Fragments" }));
        
        // Anomaly Ships
        scannerShips.Add(CreateScannerShip("The Harbinger", ShipRarity.Anomaly, "??? Entity",
            "An incomprehensible entity that defies classification or understanding.",
            new string[] { "Dimensional Rifts", "Anomaly Zones", "Corrupted Space" },
            100, 150, new string[] { "Unknown Technology", "Dimensional Artifacts", "Reality Cores", "Chaos Elements", "Void Essence" }));
            
        scannerShips.Add(CreateScannerShip("Echo of Tomorrow", ShipRarity.Anomaly, "Temporal Anomaly",
            "A ship that exists in multiple timelines simultaneously.",
            new string[] { "Temporal Anomalies", "Time Distortions", "Paradox Zones" },
            80, 200, new string[] { "Temporal Tech", "Chronodyne Crystals", "Paradox Engines", "Future Knowledge" }));
        */
    }
    
    /// <summary>
    /// Helper method to create scanner ship models
    /// </summary>
    private ScannerShipModel CreateScannerShip(string name, ShipRarity rarity, string shipClass, 
        string description, string[] locations, int minTradeValue, int maxTradeValue, string[] tradeGoods)
    {
        ScannerShipModel ship = new ScannerShipModel
        {
            shipName = name,
            rarity = rarity,
            shipClass = shipClass,
            description = description,
            availableLocations = locations.ToList(),
            minTradeValue = minTradeValue,
            maxTradeValue = maxTradeValue,
            availableTradeGoods = tradeGoods.ToList()
        };
        
        return ship;
    }
    
    /// <summary>
    /// Load additional ships from Resources folder (future expansion)
    /// </summary>
    private void LoadShipsFromResources()
    {
        // For now, we'll focus on the JSON import system instead of Resources loading
        // This method is here for future expansion if needed
        
        // Future implementation could load ship data from JSON files in Resources
        // or from ScriptableObjects, but since we're using the AI import system,
        // we'll leave this empty for now
        
        Debug.Log("ScannerShipDatabase: Resources loading disabled - using JSON import system instead");
    }
    
    /// <summary>
    /// Get a random ship of the specified rarity
    /// </summary>
    public ScannerShipModel GetRandomShipByRarity(ShipRarity rarity)
    {
        if (!shipsByRarity.ContainsKey(rarity) || shipsByRarity[rarity].Count == 0)
        {
            Debug.LogWarning($"ScannerShipDatabase: No ships found for rarity {rarity}");
            return null;
        }
        
        var availableShips = shipsByRarity[rarity];
        return availableShips[Random.Range(0, availableShips.Count)];
    }
    
    /// <summary>
    /// Get a random ship of the specified rarity that's available in the current location
    /// </summary>
    public ScannerShipModel GetRandomShipByRarityAndLocation(ShipRarity rarity, string currentLocation)
    {
        if (!shipsByRarity.ContainsKey(rarity) || shipsByRarity[rarity].Count == 0)
        {
            Debug.LogWarning($"ScannerShipDatabase: No ships found for rarity {rarity}");
            return GetRandomShipByRarity(rarity); // Fallback to any ship of this rarity
        }
        
        // Filter ships by location
        var availableShips = shipsByRarity[rarity]
            .Where(ship => ship.availableLocations.Contains(currentLocation) || 
                          ship.availableLocations.Contains("All Locations"))
            .ToList();
        
        if (availableShips.Count == 0)
        {
            Debug.LogWarning($"ScannerShipDatabase: No {rarity} ships available in {currentLocation}");
            return GetRandomShipByRarity(rarity); // Fallback to any ship of this rarity
        }
        
        return availableShips[Random.Range(0, availableShips.Count)];
    }
    
    /// <summary>
    /// Get a ship by its exact name
    /// </summary>
    public ScannerShipModel GetShipByName(string shipName)
    {
        return shipsByName.TryGetValue(shipName, out ScannerShipModel ship) ? ship : null;
    }
    
    /// <summary>
    /// Get all ships available in a specific location
    /// </summary>
    public List<ScannerShipModel> GetShipsInLocation(string location)
    {
        if (shipsByLocation.ContainsKey(location))
        {
            return new List<ScannerShipModel>(shipsByLocation[location]);
        }
        
        return new List<ScannerShipModel>();
    }
    
    /// <summary>
    /// Get all ships of a specific rarity
    /// </summary>
    public List<ScannerShipModel> GetShipsByRarity(ShipRarity rarity)
    {
        if (shipsByRarity.ContainsKey(rarity))
        {
            return new List<ScannerShipModel>(shipsByRarity[rarity]);
        }
        
        return new List<ScannerShipModel>();
    }
    
    /// <summary>
    /// Get total number of ships in database
    /// </summary>
    public int GetTotalShipCount()
    {
        return scannerShips.Count;
    }
    
    /// <summary>
    /// Get breakdown of ships by rarity
    /// </summary>
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
    
    /// <summary>
    /// Add a new ship to the database (for runtime expansion)
    /// </summary>
    public void AddShip(ScannerShipModel ship)
    {
        if (ship != null && !string.IsNullOrEmpty(ship.shipName))
        {
            scannerShips.Add(ship);
            InitializeLookups(); // Refresh lookups
            Debug.Log($"ScannerShipDatabase: Added new ship - {ship.shipName}");
        }
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
            InitializeLookups(); // Refresh lookups
            Debug.Log($"ScannerShipDatabase: Removed ship - {shipName}");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Refresh the database lookups after importing new ships
    /// </summary>
    public void RefreshDatabase()
    {
        InitializeLookups();
        Debug.Log($"ScannerShipDatabase: Database refreshed with {scannerShips.Count} ships");
    }

    /// <summary>
    /// Clear all ships and reload from default + imported ships
    /// </summary>
    [ContextMenu("Reload Database")]
    public void ReloadDatabase()
    {
        scannerShips.Clear();
        
        // Add default ships
        AddDefaultScannerShips();
        
        // Import any JSON ships
        var importer = FindObjectOfType<ScannerShipImporter>();
        if (importer != null)
        {
            importer.ImportAllShips();
        }
        
        InitializeLookups();
        Debug.Log($"ScannerShipDatabase: Database reloaded with {scannerShips.Count} ships");
    }

    /// <summary>
    /// Export current database to JSON format (for backup/sharing)
    /// </summary>
    [ContextMenu("Export Database to JSON")]
    public void ExportDatabaseToJson()
    {
        string folderPath = System.IO.Path.Combine(Application.streamingAssetsPath, "ScannerShips", "Exports");
        System.IO.Directory.CreateDirectory(folderPath);
        
        foreach (var ship in scannerShips)
        {
            try
            {
                string json = ConvertShipToJson(ship);
                string fileName = $"{ship.shipName.Replace(" ", "_").Replace("/", "_")}.json";
                string filePath = System.IO.Path.Combine(folderPath, fileName);
                
                System.IO.File.WriteAllText(filePath, json);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to export ship {ship.shipName}: {e.Message}");
            }
        }
        
        Debug.Log($"Exported {scannerShips.Count} ships to {folderPath}");
    }

    /// <summary>
    /// Convert a ScannerShipModel to JSON format
    /// </summary>
    private string ConvertShipToJson(ScannerShipModel ship)
    {
        // Create the JSON structure
        var jsonData = new {
            scannerShip = new {
                basicInfo = new {
                    shipName = ship.shipName,
                    shipClass = ship.shipClass,
                    description = ship.description,
                    rarity = ship.rarity.ToString(),
                    originStory = $"Part of the {ship.shipClass} fleet, this vessel represents the cutting edge of its era."
                },
                visual = new {
                    primaryColor = "FFFFFF", // Default color since not in model
                    secondaryColor = "FFFFFF",
                    colorDescription = "Standard hull coloration",
                    designStyle = DetermineDesignStyle(ship),
                    sizeClass = DetermineSizeClass(ship),
                    distinctiveFeatures = new string[] { "Standard Configuration" }
                },
                availability = new {
                    preferredLocations = ship.availableLocations.ToArray(),
                    avoidedLocations = new string[0],
                    spawnWeight = 1.0f,
                    isUnique = false,
                    timeOfDayPreference = "any"
                },
                trading = new {
                    tradeProfile = DetermineTradeProfile(ship),
                    minTradeValue = ship.minTradeValue,
                    maxTradeValue = ship.maxTradeValue,
                    primaryTradeGoods = ship.availableTradeGoods.Take(3).ToArray(),
                    secondaryTradeGoods = ship.availableTradeGoods.Skip(3).ToArray(),
                    tradePersonality = "Professional and business-focused"
                },
                combat = new {
                    combatRole = DetermineDesignStyle(ship),
                    levelRange = new {
                        min = ship.minTradeValue / 2, // Approximate level from trade value
                        max = ship.maxTradeValue / 2
                    },
                    statMultipliers = new {
                        attack = 1.0f,
                        defense = 1.0f,
                        health = 1.0f,
                        speed = 1.0f
                    },
                    combatPersonality = "Engages according to tactical doctrine",
                    specialAbilities = new string[0]
                },
                metadata = new {
                    createdBy = "Database Export",
                    sourceImage = "generated_ship.png",
                    analysisDate = System.DateTime.Now.ToString("yyyy-MM-dd"),
                    tags = new string[] { ship.rarity.ToString().ToLower(), ship.shipClass.ToLower() },
                    difficulty = DetermineDifficulty(ship),
                    recommendedPlayerLevel = DeterminePlayerLevel(ship)
                }
            }
        };
        
        return JsonUtility.ToJson(jsonData, true);
    }

    /// <summary>
    /// Helper methods for JSON conversion
    /// </summary>
    private string DetermineDesignStyle(ScannerShipModel ship)
    {
        if (ship.shipClass.ToLower().Contains("military") || ship.shipClass.ToLower().Contains("combat"))
            return "Military";
        if (ship.shipClass.ToLower().Contains("research") || ship.shipClass.ToLower().Contains("science"))
            return "Scientific";
        if (ship.shipClass.ToLower().Contains("corporate") || ship.shipClass.ToLower().Contains("executive"))
            return "Corporate";
        return "Industrial";
    }

    private string DetermineSizeClass(ScannerShipModel ship)
    {
        if (ship.maxTradeValue > 75) return "Large";
        if (ship.maxTradeValue > 40) return "Medium";
        return "Small";
    }

    private string DetermineTradeProfile(ScannerShipModel ship)
    {
        if (ship.availableTradeGoods.Any(g => g.ToLower().Contains("weapon") || g.ToLower().Contains("military")))
            return "Military Contractor";
        if (ship.availableTradeGoods.Any(g => g.ToLower().Contains("research") || g.ToLower().Contains("data")))
            return "Information Broker";
        if (ship.availableTradeGoods.Any(g => g.ToLower().Contains("tech") || g.ToLower().Contains("alien")))
            return "Technology Dealer";
        return "General Merchant";
    }

    private string DetermineDifficulty(ScannerShipModel ship)
    {
        if (ship.maxTradeValue > 75) return "Hard";
        if (ship.maxTradeValue > 40) return "Medium";
        return "Easy";
    }

    private string DeterminePlayerLevel(ScannerShipModel ship)
    {
        switch (ship.rarity)
        {
            case ShipRarity.VeryCommon:
            case ShipRarity.Common:
                return "Early-game";
            case ShipRarity.SlightlyRare:
            case ShipRarity.Rare:
                return "Mid-game";
            case ShipRarity.Epic:
            case ShipRarity.Legendary:
            case ShipRarity.Anomaly:
                return "Late-game";
            default:
                return "Mid-game";
        }
    }

    /// <summary>
    /// Get statistics about the current database
    /// </summary>
    [ContextMenu("Show Database Statistics")]
    public void ShowDatabaseStatistics()
    {
        Debug.Log("=== SCANNER SHIP DATABASE STATISTICS ===");
        Debug.Log($"Total Ships: {scannerShips.Count}");
        
        var rarityBreakdown = GetShipCountByRarity();
        foreach (var kvp in rarityBreakdown)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value} ships");
        }
        
        Debug.Log($"Average Trade Value: {scannerShips.Average(s => (s.minTradeValue + s.maxTradeValue) / 2f):F1}");
        Debug.Log($"Locations Covered: {scannerShips.SelectMany(s => s.availableLocations).Distinct().Count()}");
    }
}