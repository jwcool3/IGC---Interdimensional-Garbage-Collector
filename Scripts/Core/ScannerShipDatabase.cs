using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Updated ScannerShipDatabase that uses a ScriptableObject for persistence
/// This acts as a bridge between the persistent data and the game systems
/// </summary>
public class ScannerShipDatabase : MonoBehaviour
{
    // Singleton pattern
    public static ScannerShipDatabase Instance { get; private set; }
    
    [Header("Database Reference")]
    [SerializeField] private ScannerShipDatabaseSO databaseAsset;
    
    [Header("Auto-Setup")]
    [SerializeField] private bool autoFindDatabase = true;
    [SerializeField] private bool initializeOnAwake = true;
    
    // Static reference to the database for easy access
    private static ScannerShipDatabaseSO staticDatabase;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (initializeOnAwake)
            {
                InitializeDatabase();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initialize the database system
    /// </summary>
    public void InitializeDatabase()
    {
        // Try to find the database asset if not assigned
        if (databaseAsset == null && autoFindDatabase)
        {
            FindDatabaseAsset();
        }
        
        if (databaseAsset == null)
        {
            Debug.LogError("ScannerShipDatabase: No database asset found! Please assign it in the inspector or create one.");
            return;
        }
        
        // Set the static reference
        staticDatabase = databaseAsset;
        
        // Initialize the database
        databaseAsset.Initialize();
        
        Debug.Log($"ScannerShipDatabase: Initialized with {databaseAsset.GetTotalShipCount()} ships from persistent storage");
    }
    
    /// <summary>
    /// Try to automatically find the database asset
    /// </summary>
    private void FindDatabaseAsset()
    {
        // Look for the database asset in the Resources folder
        databaseAsset = Resources.Load<ScannerShipDatabaseSO>("ScannerShipDatabase");
        
        #if UNITY_EDITOR
        if (databaseAsset == null)
        {
            // Try a broader search (Editor only)
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ScannerShipDatabaseSO");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                databaseAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<ScannerShipDatabaseSO>(path);
                Debug.Log($"Found database asset at: {path}");
            }
        }
        #endif
        
        if (databaseAsset == null)
        {
            Debug.LogWarning("ScannerShipDatabase: Could not auto-find database asset. Please assign it manually or create a new one.");
        }
    }
    
    /// <summary>
    /// Create a new database asset if none exists
    /// </summary>
    [ContextMenu("Create New Database Asset")]
    public void CreateNewDatabaseAsset()
    {
        #if UNITY_EDITOR
        // Create a new database asset
        var newDatabase = ScriptableObject.CreateInstance<ScannerShipDatabaseSO>();
        
        // Create the Databases folder if it doesn't exist
        if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Databases"))
        {
            UnityEditor.AssetDatabase.CreateFolder("Assets", "Databases");
        }
        
        // Save the asset
        string assetPath = "Assets/Databases/ScannerShipDatabase.asset";
        UnityEditor.AssetDatabase.CreateAsset(newDatabase, assetPath);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        
        // Assign it to this component
        databaseAsset = newDatabase;
        
        // Initialize it
        databaseAsset.Initialize();
        
        Debug.Log($"Created new database asset at: {assetPath}");
        #endif
    }
    
    // ===== STATIC METHODS FOR EASY ACCESS =====
    
    /// <summary>
    /// Initialize static database - can be called from anywhere
    /// </summary>
    public static void InitializeStaticDatabase()
    {
        if (Instance == null)
        {
            Debug.LogWarning("ScannerShipDatabase: No instance found, creating one...");
            var go = new GameObject("ScannerShipDatabase");
            Instance = go.AddComponent<ScannerShipDatabase>();
        }
        
        Instance.InitializeDatabase();
    }
    
    /// <summary>
    /// Get total ship count
    /// </summary>
    public static int GetTotalShipCountStatic()
    {
        if (staticDatabase == null) InitializeStaticDatabase();
        return staticDatabase?.GetTotalShipCount() ?? 0;
    }
    
    /// <summary>
    /// Get random ship by rarity
    /// </summary>
    public static ScannerShipModel GetRandomShipByRarityStatic(ShipRarity rarity)
    {
        if (staticDatabase == null) InitializeStaticDatabase();
        return staticDatabase?.GetRandomShipByRarity(rarity);
    }
    
    /// <summary>
    /// Get random ship by rarity and location
    /// </summary>
    public static ScannerShipModel GetRandomShipByRarityAndLocationStatic(ShipRarity rarity, string currentLocation)
    {
        if (staticDatabase == null) InitializeStaticDatabase();
        return staticDatabase?.GetRandomShipByRarityAndLocation(rarity, currentLocation);
    }
    
    /// <summary>
    /// Get ship by name
    /// </summary>
    public static ScannerShipModel GetShipByNameStatic(string shipName)
    {
        if (staticDatabase == null) InitializeStaticDatabase();
        return staticDatabase?.GetShipByName(shipName);
    }
    
    /// <summary>
    /// Add ship to database
    /// </summary>
    public static void AddShipStatic(ScannerShipModel ship)
    {
        if (staticDatabase == null) InitializeStaticDatabase();
        staticDatabase?.AddShip(ship);
    }
    
    /// <summary>
    /// Get database statistics
    /// </summary>
    public static Dictionary<string, object> GetDatabaseStatistics()
    {
        if (staticDatabase == null) InitializeStaticDatabase();
        
        var stats = new Dictionary<string, object>();
        if (staticDatabase != null)
        {
            stats["TotalShips"] = staticDatabase.GetTotalShipCount();
            stats["RarityBreakdown"] = staticDatabase.GetShipCountByRarity();
        }
        return stats;
    }
    
    // ===== INSTANCE METHODS FOR BACKWARDS COMPATIBILITY =====
    
    public ScannerShipModel GetRandomShipByRarity(ShipRarity rarity)
    {
        return databaseAsset?.GetRandomShipByRarity(rarity);
    }
    
    public ScannerShipModel GetRandomShipByRarityAndLocation(ShipRarity rarity, string currentLocation)
    {
        return databaseAsset?.GetRandomShipByRarityAndLocation(rarity, currentLocation);
    }
    
    public ScannerShipModel GetShipByName(string shipName)
    {
        return databaseAsset?.GetShipByName(shipName);
    }
    
    public int GetTotalShipCount()
    {
        return databaseAsset?.GetTotalShipCount() ?? 0;
    }
    
    public void AddShip(ScannerShipModel ship)
    {
        databaseAsset?.AddShip(ship);
    }
    
    public bool RemoveShip(string shipName)
    {
        return databaseAsset?.RemoveShip(shipName) ?? false;
    }
    
    public void RefreshDatabase()
    {
        databaseAsset?.BuildLookups();
    }
    
    public Dictionary<ShipRarity, int> GetShipCountByRarity()
    {
        return databaseAsset?.GetShipCountByRarity() ?? new Dictionary<ShipRarity, int>();
    }
    
    /// <summary>
    /// Reload the entire database
    /// </summary>
    [ContextMenu("Reload Database")]
    public void ReloadDatabase()
    {
        if (databaseAsset != null)
        {
            databaseAsset.Initialize();
            Debug.Log($"ScannerShipDatabase: Database reloaded with {databaseAsset.GetTotalShipCount()} ships");
        }
    }
    
    /// <summary>
    /// Show database statistics
    /// </summary>
    [ContextMenu("Show Database Statistics")]
    public void ShowDatabaseStatistics()
    {
        if (databaseAsset != null)
        {
            databaseAsset.ShowDatabaseStatistics();
        }
        else
        {
            Debug.LogError("No database asset assigned!");
        }
    }
}