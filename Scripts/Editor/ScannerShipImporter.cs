using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Collections;

/// <summary>
/// Imports scanner ships from JSON files created by AI analysis
/// </summary>
public class ScannerShipImporter : MonoBehaviour
{
    [Header("Import Settings")]
    [SerializeField] private string importFolderPath = "StreamingAssets/ScannerShips";
    [SerializeField] private bool importOnStart = false;
    [SerializeField] private bool replaceExistingShips = false;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private void Start()
    {
        if (importOnStart)
        {
            ImportAllShips();
        }
    }
    
    /// <summary>
    /// Import all JSON files from the import folder
    /// </summary>
    [ContextMenu("Import All Ships")]
    public void ImportAllShips()
    {
        string fullPath = Path.Combine(Application.streamingAssetsPath, "ScannerShips");
        
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
            LogDebug($"Created import directory: {fullPath}");
            LogDebug("Place your ship JSON files in this folder and try again.");
            return;
        }
        
        string[] jsonFiles = Directory.GetFiles(fullPath, "*.json");
        
        if (jsonFiles.Length == 0)
        {
            LogDebug("No JSON files found in import folder.");
            return;
        }
        
        int successCount = 0;
        int errorCount = 0;
        
        foreach (string filePath in jsonFiles)
        {
            try
            {
                if (ImportShipFromFile(filePath))
                {
                    successCount++;
                }
                else
                {
                    errorCount++;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to import {Path.GetFileName(filePath)}: {e.Message}");
                errorCount++;
            }
        }
        
        LogDebug($"Import complete: {successCount} ships imported, {errorCount} errors");
        
        // Refresh the database
        if (ScannerShipDatabase.Instance != null && successCount > 0)
        {
            ScannerShipDatabase.Instance.RefreshDatabase();
        }
    }
    
    /// <summary>
    /// Import a single ship from a JSON file
    /// </summary>
    public bool ImportShipFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return false;
        }
        
        string jsonContent = File.ReadAllText(filePath);
        return ImportShipFromJson(jsonContent, Path.GetFileName(filePath));
    }
    
    /// <summary>
    /// Import a ship from JSON string
    /// </summary>
    public bool ImportShipFromJson(string jsonContent, string sourceName = "Unknown")
    {
        try
        {
            ShipJsonData shipData = JsonUtility.FromJson<ShipJsonData>(jsonContent);
            
            if (shipData?.scannerShip == null)
            {
                Debug.LogError($"Invalid JSON format in {sourceName}");
                return false;
            }
            
            ScannerShipModel ship = ConvertJsonToShipModel(shipData.scannerShip);
            
            if (ship == null)
            {
                Debug.LogError($"Failed to convert JSON to ship model: {sourceName}");
                return false;
            }
            
            // Check if ship already exists
            if (ScannerShipDatabase.Instance != null)
            {
                if (ScannerShipDatabase.Instance.GetShipByName(ship.shipName) != null)
                {
                    if (!replaceExistingShips)
                    {
                        LogDebug($"Ship '{ship.shipName}' already exists, skipping import");
                        return false;
                    }
                    else
                    {
                        ScannerShipDatabase.Instance.RemoveShip(ship.shipName);
                        LogDebug($"Replaced existing ship: {ship.shipName}");
                    }
                }
                
                ScannerShipDatabase.Instance.AddShip(ship);
                LogDebug($"Successfully imported ship: {ship.shipName} ({ship.rarity})");
                return true;
            }
            else
            {
                Debug.LogError("ScannerShipDatabase not found!");
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error importing ship from {sourceName}: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Convert JSON data to ScannerShipModel
    /// </summary>
    private ScannerShipModel ConvertJsonToShipModel(ScannerShipJson jsonShip)
    {
        ScannerShipModel ship = new ScannerShipModel();
        
        // Basic Info
        ship.shipName = jsonShip.basicInfo.shipName;
        ship.shipClass = jsonShip.basicInfo.shipClass;
        ship.description = jsonShip.basicInfo.description;
        ship.rarity = ParseRarity(jsonShip.basicInfo.rarity);
        
        // Visual
        ship.shipColor = ParseColor(jsonShip.visual.primaryColor);
        
        // Availability
        ship.availableLocations = jsonShip.availability.preferredLocations ?? new List<string>();
        ship.spawnWeight = jsonShip.availability.spawnWeight;
        ship.isUnique = jsonShip.availability.isUnique;
        
        // Trading
        ship.minTradeValue = jsonShip.trading.minTradeValue;
        ship.maxTradeValue = jsonShip.trading.maxTradeValue;
        ship.availableTradeGoods = new List<string>();
        if (jsonShip.trading.primaryTradeGoods != null)
            ship.availableTradeGoods.AddRange(jsonShip.trading.primaryTradeGoods);
        if (jsonShip.trading.secondaryTradeGoods != null)
            ship.availableTradeGoods.AddRange(jsonShip.trading.secondaryTradeGoods);
        
        // Combat
        ship.minLevel = jsonShip.combat.levelRange.min;
        ship.maxLevel = jsonShip.combat.levelRange.max;
        ship.attackMultiplier = jsonShip.combat.statMultipliers.attack;
        ship.defenseMultiplier = jsonShip.combat.statMultipliers.defense;
        ship.healthMultiplier = jsonShip.combat.statMultipliers.health;
        
        // Special abilities
        ship.specialAbilities = jsonShip.combat.specialAbilities ?? new List<string>();
        
        return ship;
    }
    
    /// <summary>
    /// Parse rarity string to enum
    /// </summary>
    private ShipRarity ParseRarity(string rarityString)
    {
        if (System.Enum.TryParse<ShipRarity>(rarityString, true, out ShipRarity rarity))
        {
            return rarity;
        }
        
        Debug.LogWarning($"Unknown rarity: {rarityString}, defaulting to Common");
        return ShipRarity.Common;
    }
    
    /// <summary>
    /// Parse hex color string to Color
    /// </summary>
    private Color ParseColor(string hexColor)
    {
        if (string.IsNullOrEmpty(hexColor))
            return Color.white;
            
        if (hexColor.StartsWith("#"))
            hexColor = hexColor.Substring(1);
            
        if (ColorUtility.TryParseHtmlString("#" + hexColor, out Color color))
        {
            return color;
        }
        
        Debug.LogWarning($"Invalid color format: {hexColor}, using white");
        return Color.white;
    }
    
    /// <summary>
    /// Create a sample JSON file for reference
    /// </summary>
    [ContextMenu("Create Sample JSON")]
    public void CreateSampleJson()
    {
        string sampleJson = @"{
  ""scannerShip"": {
    ""basicInfo"": {
      ""shipName"": ""Sample Ship"",
      ""shipClass"": ""Example Class"",
      ""description"": ""This is a sample ship for testing the import system."",
      ""rarity"": ""Common"",
      ""originStory"": ""Created as a test case for the ship importer.""
    },
    ""visual"": {
      ""primaryColor"": ""#4A90E2"",
      ""secondaryColor"": ""#2C5F7F"",
      ""colorDescription"": ""Blue and gray hull"",
      ""designStyle"": ""Industrial"",
      ""sizeClass"": ""Medium"",
      ""distinctiveFeatures"": [""Sample feature 1"", ""Sample feature 2""]
    },
    ""availability"": {
      ""preferredLocations"": [""Test Sector"", ""Sample Zone""],
      ""avoidedLocations"": [""Restricted Area""],
      ""spawnWeight"": 1.0,
      ""isUnique"": false,
      ""timeOfDayPreference"": ""any""
    },
    ""trading"": {
      ""tradeProfile"": ""General Trader"",
      ""minTradeValue"": 10,
      ""maxTradeValue"": 20,
      ""primaryTradeGoods"": [""Basic Parts"", ""Ship Components""],
      ""secondaryTradeGoods"": [""Rare Materials""],
      ""tradePersonality"": ""Friendly and professional""
    },
    ""combat"": {
      ""combatRole"": ""Balanced"",
      ""levelRange"": {
        ""min"": 3,
        ""max"": 8
      },
      ""statMultipliers"": {
        ""attack"": 1.0,
        ""defense"": 1.0,
        ""health"": 1.0,
        ""speed"": 1.0
      },
      ""combatPersonality"": ""Defensive when threatened"",
      ""specialAbilities"": [""None""]
    },
    ""metadata"": {
      ""createdBy"": ""Manual Example"",
      ""sourceImage"": ""sample_ship.png"",
      ""analysisDate"": ""2025-01-15"",
      ""tags"": [""sample"", ""test"", ""example""],
      ""difficulty"": ""Easy"",
      ""recommendedPlayerLevel"": ""Early-game""
    }
  }
}";
        
        string folderPath = Path.Combine(Application.streamingAssetsPath, "ScannerShips");
        Directory.CreateDirectory(folderPath);
        
        string filePath = Path.Combine(folderPath, "sample_ship.json");
        File.WriteAllText(filePath, sampleJson);
        
        Debug.Log($"Sample JSON created at: {filePath}");
    }
    
    /// <summary>
    /// Debug the import system and show detailed information about the current state
    /// </summary>
    [ContextMenu("Debug Import System")]
    public void DebugImportSystem()
    {
        Debug.Log("=== IMPORT SYSTEM DEBUG ===");
        
        // Check if database exists
        if (ScannerShipDatabase.Instance != null)
        {
            Debug.Log($"✅ ScannerShipDatabase found with {ScannerShipDatabase.Instance.GetTotalShipCount()} ships");
        }
        else
        {
            Debug.LogError("❌ ScannerShipDatabase.Instance is NULL!");
            return;
        }
        
        // Check file path
        string fullPath = Path.Combine(Application.streamingAssetsPath, "ScannerShips");
        Debug.Log($"Looking for files in: {fullPath}");
        Debug.Log($"StreamingAssets path: {Application.streamingAssetsPath}");
        
        if (Directory.Exists(fullPath))
        {
            Debug.Log("✅ Directory exists");
            string[] jsonFiles = Directory.GetFiles(fullPath, "*.json");
            Debug.Log($"Found {jsonFiles.Length} JSON files:");
            foreach (string file in jsonFiles)
            {
                Debug.Log($"  - {Path.GetFileName(file)} ({new FileInfo(file).Length} bytes)");
            }
            
            // Test reading one file
            if (jsonFiles.Length > 0)
            {
                try
                {
                    string testContent = File.ReadAllText(jsonFiles[0]);
                    Debug.Log($"First file content preview: {testContent.Substring(0, Math.Min(200, testContent.Length))}...");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error reading first file: {e.Message}");
                }
            }
        }
        else
        {
            Debug.LogError($"❌ Directory does not exist: {fullPath}");
        }
        
        // Check what ships are currently in database
        Debug.Log("Current ships in database:");
        var rarityBreakdown = ScannerShipDatabase.Instance.GetShipCountByRarity();
        foreach (var kvp in rarityBreakdown)
        {
            if (kvp.Value > 0)
                Debug.Log($"  {kvp.Key}: {kvp.Value} ships");
        }
    }
    
    /// <summary>
    /// Test the import system by manually clearing and reloading the database
    /// </summary>
    [ContextMenu("Manual Import Test")]
    public void ManualImportTest()
    {
        Debug.Log("=== MANUAL IMPORT TEST ===");
        
        // First clear the database to see pure import results
        if (ScannerShipDatabase.Instance != null)
        {
            Debug.Log("Clearing existing ships from database...");
            // We need to access the private field, so let's use the ReloadDatabase method instead
            // This will clear and re-add defaults, then import JSON
            ScannerShipDatabase.Instance.ReloadDatabase();
        }
        else
        {
            Debug.LogError("ScannerShipDatabase not found!");
            return;
        }
        
        Debug.Log($"After reload: {ScannerShipDatabase.Instance.GetTotalShipCount()} ships in database");
        
        // Show what we have
        Debug.Log("Ships after reload:");
        var rarityBreakdown = ScannerShipDatabase.Instance.GetShipCountByRarity();
        foreach (var kvp in rarityBreakdown)
        {
            if (kvp.Value > 0)
                Debug.Log($"  {kvp.Key}: {kvp.Value} ships");
        }
    }
    
    /// <summary>
    /// Wait for database to initialize before importing
    /// </summary>
    private IEnumerator WaitForDatabaseAndImport()
    {
        // Wait up to 5 seconds for database to initialize
        float timeout = 5f;
        float elapsed = 0f;
        
        while (ScannerShipDatabase.Instance == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (ScannerShipDatabase.Instance != null)
        {
            Debug.Log("Database found! Starting import...");
            ImportAllShips();
        }
        else
        {
            Debug.LogError("Timeout waiting for ScannerShipDatabase to initialize!");
        }
    }

    /// <summary>
    /// Safely import all ships, waiting for database if needed
    /// </summary>
    [ContextMenu("Safe Import All Ships")]
    public void SafeImportAllShips()
    {
        if (ScannerShipDatabase.Instance != null)
        {
            ImportAllShips();
        }
        else
        {
            Debug.Log("Database not ready, waiting...");
            StartCoroutine(WaitForDatabaseAndImport());
        }
    }
    
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[ScannerShipImporter] {message}");
        }
    }
}

// JSON data structures for parsing
[System.Serializable]
public class ShipJsonData
{
    public ScannerShipJson scannerShip;
}

[System.Serializable]
public class ScannerShipJson
{
    public BasicInfoJson basicInfo;
    public VisualJson visual;
    public AvailabilityJson availability;
    public TradingJson trading;
    public CombatJson combat;
    public MetadataJson metadata;
}

[System.Serializable]
public class BasicInfoJson
{
    public string shipName;
    public string shipClass;
    public string description;
    public string rarity;
    public string originStory;
}

[System.Serializable]
public class VisualJson
{
    public string primaryColor;
    public string secondaryColor;
    public string colorDescription;
    public string designStyle;
    public string sizeClass;
    public List<string> distinctiveFeatures;
}

[System.Serializable]
public class AvailabilityJson
{
    public List<string> preferredLocations;
    public List<string> avoidedLocations;
    public float spawnWeight;
    public bool isUnique;
    public string timeOfDayPreference;
}

[System.Serializable]
public class TradingJson
{
    public string tradeProfile;
    public int minTradeValue;
    public int maxTradeValue;
    public List<string> primaryTradeGoods;
    public List<string> secondaryTradeGoods;
    public string tradePersonality;
}

[System.Serializable]
public class CombatJson
{
    public string combatRole;
    public LevelRangeJson levelRange;
    public StatMultipliersJson statMultipliers;
    public string combatPersonality;
    public List<string> specialAbilities;
}

[System.Serializable]
public class LevelRangeJson
{
    public int min;
    public int max;
}

[System.Serializable]
public class StatMultipliersJson
{
    public float attack;
    public float defense;
    public float health;
    public float speed;
}

[System.Serializable]
public class MetadataJson
{
    public string createdBy;
    public string sourceImage;
    public string analysisDate;
    public List<string> tags;
    public string difficulty;
    public string recommendedPlayerLevel;
}