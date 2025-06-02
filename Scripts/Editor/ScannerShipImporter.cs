using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Collections;

/// <summary>
/// Imports scanner ships from JSON files created by AI analysis
/// Updated to work with the new persistent ScannerShipDatabaseSO
/// </summary>
public class ScannerShipImporter : MonoBehaviour
{
    [Header("Database Reference")]
    [SerializeField] private ScannerShipDatabaseSO targetDatabase;
    
    [Header("Import Settings")]
    [SerializeField] private string importFolderPath = "StreamingAssets/ScannerShips";
    [SerializeField] private bool importOnStart = false;
    [SerializeField] private bool replaceExistingShips = false;
    [SerializeField] private bool autoFindDatabase = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private void Start()
    {
        // Try to find the database if not assigned
        if (targetDatabase == null && autoFindDatabase)
        {
            FindTargetDatabase();
        }
        
        if (importOnStart)
        {
            ImportAllShips();
        }
    }
    
    /// <summary>
    /// Try to automatically find the database asset
    /// </summary>
    private void FindTargetDatabase()
    {
        // First try Resources folder
        targetDatabase = Resources.Load<ScannerShipDatabaseSO>("Databases/ScannerShipDatabase");
        
        if (targetDatabase == null)
        {
            // Try root Resources folder
            targetDatabase = Resources.Load<ScannerShipDatabaseSO>("ScannerShipDatabase");
        }
        
        #if UNITY_EDITOR
        if (targetDatabase == null)
        {
            // Editor-only: Search entire project
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ScannerShipDatabaseSO");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                targetDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<ScannerShipDatabaseSO>(path);
                LogDebug($"Found database asset at: {path}");
            }
        }
        #endif
        
        if (targetDatabase == null)
        {
            Debug.LogError("ScannerShipImporter: Could not find database asset! Please assign it manually in the inspector.");
        }
        else
        {
            LogDebug($"Found target database: {targetDatabase.name}");
        }
    }
    
    /// <summary>
    /// Import all JSON files from the import folder
    /// </summary>
    [ContextMenu("Import All Ships")]
    public void ImportAllShips()
    {
        if (targetDatabase == null)
        {
            Debug.LogError("No target database assigned! Please assign a ScannerShipDatabaseSO in the inspector.");
            return;
        }
        
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
        
        LogDebug($"Starting import of {jsonFiles.Length} JSON files...");
        
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
        
        // Mark the database as dirty so Unity saves the changes
        #if UNITY_EDITOR
        if (successCount > 0)
        {
            UnityEditor.EditorUtility.SetDirty(targetDatabase);
            UnityEditor.AssetDatabase.SaveAssets();
            LogDebug("Database marked as dirty and saved");
        }
        #endif
        
        // Refresh any runtime instances
        if (ScannerShipDatabase.Instance != null && successCount > 0)
        {
            ScannerShipDatabase.Instance.RefreshDatabase();
            LogDebug("Refreshed runtime database instance");
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
        if (targetDatabase == null)
        {
            Debug.LogError("No target database assigned!");
            return false;
        }
        
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
            
            // Validate the ship model
            if (!ship.IsValid())
            {
                Debug.LogError($"Invalid ship model created from {sourceName}");
                return false;
            }
            
            // Check if ship already exists
            if (targetDatabase.GetShipByName(ship.shipName) != null)
            {
                if (!replaceExistingShips)
                {
                    LogDebug($"Ship '{ship.shipName}' already exists, skipping import");
                    return false;
                }
                else
                {
                    targetDatabase.RemoveShip(ship.shipName);
                    LogDebug($"Replaced existing ship: {ship.shipName}");
                }
            }
            
            // Add the ship to the database
            targetDatabase.AddShip(ship);
            LogDebug($"Successfully imported ship: {ship.shipName} ({ship.rarity})");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error importing ship from {sourceName}: {e.Message}\n{e.StackTrace}");
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
        ship.shipName = jsonShip.basicInfo?.shipName ?? "Unknown Ship";
        ship.shipClass = jsonShip.basicInfo?.shipClass ?? "Unknown Class";
        ship.description = jsonShip.basicInfo?.description ?? "No description available.";
        ship.rarity = ParseRarity(jsonShip.basicInfo?.rarity);
        
        // Visual
        ship.shipColor = ParseColor(jsonShip.visual?.primaryColor);
        
        // Availability
        ship.availableLocations = jsonShip.availability?.preferredLocations ?? new List<string> { "All Locations" };
        ship.spawnWeight = jsonShip.availability?.spawnWeight ?? 1f;
        ship.isUnique = jsonShip.availability?.isUnique ?? false;
        
        // Trading
        ship.minTradeValue = Math.Max(1, jsonShip.trading?.minTradeValue ?? 10);
        ship.maxTradeValue = Math.Max(ship.minTradeValue, jsonShip.trading?.maxTradeValue ?? 20);
        ship.availableTradeGoods = new List<string>();
        
        if (jsonShip.trading?.primaryTradeGoods != null)
            ship.availableTradeGoods.AddRange(jsonShip.trading.primaryTradeGoods);
        if (jsonShip.trading?.secondaryTradeGoods != null)
            ship.availableTradeGoods.AddRange(jsonShip.trading.secondaryTradeGoods);
        
        // Ensure at least one trade good
        if (ship.availableTradeGoods.Count == 0)
        {
            ship.availableTradeGoods.Add("Basic Components");
        }
        
        // Combat
        ship.minLevel = Math.Max(1, jsonShip.combat?.levelRange?.min ?? 1);
        ship.maxLevel = Math.Max(ship.minLevel, jsonShip.combat?.levelRange?.max ?? 5);
        ship.attackMultiplier = Math.Max(0.1f, jsonShip.combat?.statMultipliers?.attack ?? 1f);
        ship.defenseMultiplier = Math.Max(0.1f, jsonShip.combat?.statMultipliers?.defense ?? 1f);
        ship.healthMultiplier = Math.Max(0.1f, jsonShip.combat?.statMultipliers?.health ?? 1f);
        
        // Special abilities
        ship.specialAbilities = jsonShip.combat?.specialAbilities ?? new List<string>();
        
        // Set defaults for interaction flags
        ship.canAlwaysFight = true;
        ship.canAlwaysTrade = true;
        
        return ship;
    }
    
    /// <summary>
    /// Parse rarity string to enum
    /// </summary>
    private ShipRarity ParseRarity(string rarityString)
    {
        if (string.IsNullOrEmpty(rarityString))
        {
            return ShipRarity.Common;
        }
        
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
        if (targetDatabase != null)
        {
            Debug.Log($"✅ Target database found: {targetDatabase.name} with {targetDatabase.GetTotalShipCount()} ships");
        }
        else
        {
            Debug.LogError("❌ Target database is NULL!");
            
            // Try to find it
            if (autoFindDatabase)
            {
                Debug.Log("Attempting to find database...");
                FindTargetDatabase();
                if (targetDatabase != null)
                {
                    Debug.Log($"✅ Found database: {targetDatabase.name}");
                }
            }
            return;
        }
        
        // Check runtime database instance
        if (ScannerShipDatabase.Instance != null)
        {
            Debug.Log($"✅ Runtime database instance found with {ScannerShipDatabase.Instance.GetTotalShipCount()} ships");
        }
        else
        {
            Debug.LogWarning("⚠️ Runtime database instance not found (this may be normal in edit mode)");
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
        Debug.Log("Current ships in target database:");
        var rarityBreakdown = targetDatabase.GetShipCountByRarity();
        foreach (var kvp in rarityBreakdown)
        {
            if (kvp.Value > 0)
                Debug.Log($"  {kvp.Key}: {kvp.Value} ships");
        }
    }
    
    /// <summary>
    /// Test the import system by clearing and importing from JSON
    /// </summary>
    [ContextMenu("Test Import (Clear and Import)")]
    public void TestImport()
    {
        if (targetDatabase == null)
        {
            Debug.LogError("No target database assigned!");
            return;
        }
        
        Debug.Log("=== TEST IMPORT ===");
        
        int initialCount = targetDatabase.GetTotalShipCount();
        Debug.Log($"Initial ship count: {initialCount}");
        
        // Import ships
        ImportAllShips();
        
        int finalCount = targetDatabase.GetTotalShipCount();
        Debug.Log($"Final ship count: {finalCount}");
        Debug.Log($"Ships added: {finalCount - initialCount}");
    }
    
    /// <summary>
    /// Clear database and add fresh default ships
    /// </summary>
    [ContextMenu("Reset Database with Defaults")]
    public void ResetDatabaseWithDefaults()
    {
        if (targetDatabase == null)
        {
            Debug.LogError("No target database assigned!");
            return;
        }
        
        Debug.Log("Clearing database and adding defaults...");
        
        // Clear existing ships
        targetDatabase.ClearAllShips();
        
        // Add default ships
        targetDatabase.AddDefaultShips();
        
        Debug.Log($"Database reset. New ship count: {targetDatabase.GetTotalShipCount()}");
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(targetDatabase);
        UnityEditor.AssetDatabase.SaveAssets();
        #endif
    }
    
    /// <summary>
    /// Wait for database to initialize before importing
    /// </summary>
    private IEnumerator WaitForDatabaseAndImport()
    {
        // Wait up to 5 seconds for database to initialize
        float timeout = 5f;
        float elapsed = 0f;
        
        while (targetDatabase == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            FindTargetDatabase();
            yield return null;
        }
        
        if (targetDatabase != null)
        {
            Debug.Log("Database found! Starting import...");
            ImportAllShips();
        }
        else
        {
            Debug.LogError("Timeout waiting for database to be found!");
        }
    }

    /// <summary>
    /// Safely import all ships, waiting for database if needed
    /// </summary>
    [ContextMenu("Safe Import All Ships")]
    public void SafeImportAllShips()
    {
        if (targetDatabase != null)
        {
            ImportAllShips();
        }
        else
        {
            Debug.Log("Database not ready, waiting...");
            StartCoroutine(WaitForDatabaseAndImport());
        }
    }
    
    /// <summary>
    /// Import specific JSON content (for testing)
    /// </summary>
    public void ImportJsonString(string jsonContent, string name = "Manual Import")
    {
        if (ImportShipFromJson(jsonContent, name))
        {
            LogDebug($"Successfully imported ship from manual JSON: {name}");
            
            #if UNITY_EDITOR
            if (targetDatabase != null)
            {
                UnityEditor.EditorUtility.SetDirty(targetDatabase);
            }
            #endif
        }
        else
        {
            Debug.LogError($"Failed to import ship from manual JSON: {name}");
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

// Keep all the JSON data structures (unchanged)
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