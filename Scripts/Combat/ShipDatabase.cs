using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Manages the database of ship models and handles auto-population from resources
/// </summary>
public class ShipDatabase : MonoBehaviour
{
    // Singleton pattern
    public static ShipDatabase Instance { get; private set; }
    
    [SerializeField] 
    public List<ShipModel> shipModels = new List<ShipModel>();
    
    // Path settings for auto-population
    [Header("Auto-Population Settings")]
    [SerializeField] public string resourceBasePath = "EnemyIcons";
    [SerializeField] private bool autoPopulateOnAwake = false;
    
    // Lookup dictionaries for quick access
    private Dictionary<string, ShipModel> shipsByName = new Dictionary<string, ShipModel>(System.StringComparer.OrdinalIgnoreCase);
    private Dictionary<EnemyType, List<ShipModel>> shipsByType = new Dictionary<EnemyType, List<ShipModel>>();
    private Dictionary<int, List<ShipModel>> shipsBySector = new Dictionary<int, List<ShipModel>>();
    
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
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeLookups()
    {
        // Clear existing lookups
        shipsByName.Clear();
        shipsByType.Clear();
        shipsBySector.Clear();
        
        // Initialize type dictionary with empty lists
        foreach (EnemyType type in System.Enum.GetValues(typeof(EnemyType)))
        {
            shipsByType[type] = new List<ShipModel>();
        }
        
        // Process each ship model
        foreach (var ship in shipModels)
        {
            if (ship == null || string.IsNullOrEmpty(ship.modelName)) continue;
            
            // Add to name lookup
            shipsByName[ship.modelName] = ship;
            
            // Add to type lookup
            if (!shipsByType.ContainsKey(ship.shipType))
            {
                shipsByType[ship.shipType] = new List<ShipModel>();
            }
            shipsByType[ship.shipType].Add(ship);
            
            // Add to sector lookup
            if (!shipsBySector.ContainsKey(ship.minSectorLevel))
            {
                shipsBySector[ship.minSectorLevel] = new List<ShipModel>();
            }
            shipsBySector[ship.minSectorLevel].Add(ship);
        }
        
        Debug.Log($"Ship Database initialized with {shipModels.Count} ships");
    }
    
    /// <summary>
    /// Auto-populate the ship database from resources with better error handling 
    /// </summary>
    public void AutoPopulateDatabase()
    {
        Debug.Log("Starting auto-population of ship database...");
        
        // Check if ANY sprites exist in Resources
        Sprite[] allSprites = Resources.LoadAll<Sprite>("");
        if (allSprites.Length == 0)
        {
            Debug.LogError("No sprites found anywhere in Resources folder! Check your project structure.");
            return;
        }
        else
        {
            Debug.Log($"Found {allSprites.Length} total sprites in Resources folder.");
            // List first 10 as examples
            for (int i = 0; i < Mathf.Min(10, allSprites.Length); i++)
            {
                string path = UnityEditor.AssetDatabase.GetAssetPath(allSprites[i]);
                Debug.Log($"Example sprite {i+1}: {allSprites[i].name} at {path}");
            }
        }
        
        // Updated to match your actual folder structure
        string basePath = resourceBasePath;
        
        // Keep track of existing models to avoid duplicates
        Dictionary<string, ShipModel> existingModels = new Dictionary<string, ShipModel>(System.StringComparer.OrdinalIgnoreCase);
        
        // Store all existing models in the dictionary for quick lookup
        Debug.Log($"Current ship database has {shipModels.Count} entries");
        foreach (var ship in shipModels)
        {
            if (ship != null && !string.IsNullOrEmpty(ship.modelName))
            {
                existingModels[ship.modelName] = ship;
                Debug.Log($"Registered existing model: {ship.modelName} (Type: {ship.shipType}, Sector: {ship.minSectorLevel})");
            }
        }
        
        // Temporary list to store new models
        List<ShipModel> newModels = new List<ShipModel>();
        int updatedCount = 0;
        
        // Try structured folder approach first
        bool foundStructuredSprites = false;
        
        // Check if there's a Default folder
        string defaultPath = $"{basePath}/Default";
        if (ResourcePathExists(defaultPath))
        {
            foundStructuredSprites = true;
            ProcessDefaultFolder(defaultPath, existingModels, newModels, ref updatedCount);
        }
        
        // Check if there are sector folders (Sector1, Sector2, etc.)
        for (int sector = 1; sector <= 5; sector++)
        {
            string sectorPath = $"{basePath}/Sector{sector}";
            if (ResourcePathExists(sectorPath))
            {
                foundStructuredSprites = true;
                ProcessSectorFolder(sectorPath, sector, existingModels, newModels, ref updatedCount);
            }
        }
        
        // If no sprites were found in the structured folders, try using sprites directly
        if (!foundStructuredSprites)
        {
            Debug.Log("No sprites found in structured folders, attempting to use sprites directly...");
            AutoPopulateFromSprites(allSprites);
        }
        else
        {
            // Add all new models to the database
            if (newModels.Count > 0)
            {
                Debug.Log($"Adding {newModels.Count} new models to database");
                shipModels.AddRange(newModels);
            }
            else
            {
                Debug.Log("No new models found to add");
            }
        }
        
        // Reinitialize lookups after adding new models
        InitializeLookups();
        
        Debug.Log($"Auto-population complete. Updated {updatedCount} existing models. Added {newModels.Count} new models. Ship database now has {shipModels.Count} models.");
    }

    /// <summary>
    /// Populate the database using sprites found directly
    /// </summary>
    private void AutoPopulateFromSprites(Sprite[] allSprites)
    {
        List<ShipModel> newModels = new List<ShipModel>();
        Dictionary<string, ShipModel> existingModels = new Dictionary<string, ShipModel>(System.StringComparer.OrdinalIgnoreCase);
        
        // Track existing models
        foreach (var ship in shipModels)
        {
            if (ship != null && !string.IsNullOrEmpty(ship.modelName))
            {
                existingModels[ship.modelName] = ship;
            }
        }
        
        foreach (var sprite in allSprites)
        {
            if (sprite == null) continue;
            
            // Skip if already exists
            if (existingModels.ContainsKey(sprite.name)) continue;
            
            // Try to determine type and sector from asset path
            string path = UnityEditor.AssetDatabase.GetAssetPath(sprite);
            path = path.ToLower();
            
            EnemyType type = EnemyType.Scavenger; // Default
            int sector = 1; // Default
            
            // Get type from path or name
            if (path.Contains("rival") || sprite.name.ToLower().Contains("rival"))
                type = EnemyType.Rival;
            else if (path.Contains("anomaly") || sprite.name.ToLower().Contains("anomaly"))
                type = EnemyType.Anomaly;
            else if (path.Contains("boss") || sprite.name.ToLower().Contains("boss"))
                type = EnemyType.Boss;
            
            // Get sector from path or name
            for (int i = 1; i <= 5; i++)
            {
                if (path.Contains($"sector{i}") || sprite.name.ToLower().Contains($"sector{i}"))
                {
                    sector = i;
                    break;
                }
            }
            
            // Create new model
            ShipModel newModel = new ShipModel
            {
                modelName = sprite.name,
                shipType = type,
                minSectorLevel = sector,
                shipIcon = sprite,
                description = "" // Empty description by default
            };
            
            newModels.Add(newModel);
            Debug.Log($"Added new ship from sprite: {sprite.name} (Type: {type}, Sector: {sector})");
        }
        
        // Add all models to database
        if (newModels.Count > 0)
        {
            shipModels.AddRange(newModels);
            Debug.Log($"Added {newModels.Count} new models from sprites.");
        }
        else
        {
            Debug.Log("No new models were found to add.");
        }
    }
    
    /// <summary>
    /// Process the Default folder containing default enemy type icons
    /// </summary>
    private void ProcessDefaultFolder(string defaultPath, Dictionary<string, ShipModel> existingModels,
                                    List<ShipModel> newModels, ref int updatedCount)
    {
        if (!ResourcePathExists(defaultPath))
        {
            Debug.LogWarning($"Default folder not found: Resources/{defaultPath}");
            return;
        }
        
        Debug.Log($"Processing default folder: {defaultPath}");
        
        // Load all sprites in the default folder
        Sprite[] sprites = Resources.LoadAll<Sprite>(defaultPath);
        
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"No sprites found in {defaultPath}");
            return;
        }
        
        Debug.Log($"Found {sprites.Length} sprites in Default folder");
        
        // Process each sprite in the default folder
        foreach (var sprite in sprites)
        {
            if (sprite == null) continue;
            
            string modelName = sprite.name;
            Debug.Log($"Processing default sprite: {modelName}");
            
            // Try to determine enemy type from name
            EnemyType type = GetEnemyTypeFromName(modelName);
            
            // Check if this model already exists
            if (existingModels.TryGetValue(modelName, out ShipModel existingModel))
            {
                bool updated = false;
                
                // Update icon if missing
                if (existingModel.shipIcon == null)
                {
                    existingModel.shipIcon = sprite;
                    updated = true;
                    Debug.Log($"Updated existing model: {modelName} with new sprite");
                }
                
                if (updated) updatedCount++;
            }
            else
            {
                // Create new ship model
                ShipModel newModel = new ShipModel
                {
                    modelName = modelName,
                    shipType = type,
                    minSectorLevel = 1, // Default sector
                    shipIcon = sprite,
                    description = "" // Empty description by default
                };
                
                // Add to new models list
                newModels.Add(newModel);
                
                // Add to existing models dictionary to avoid duplicates
                existingModels[modelName] = newModel;
                
                Debug.Log($"Created new ship model: {modelName} (Type: {type}, Default folder)");
            }
        }
    }
    
    /// <summary>
    /// Process a sector folder containing enemy type subfolders or direct sprites
    /// </summary>
    private void ProcessSectorFolder(string sectorPath, int sector, Dictionary<string, ShipModel> existingModels,
                                   List<ShipModel> newModels, ref int updatedCount)
    {
        if (!ResourcePathExists(sectorPath))
        {
            //Debug.LogWarning($"Sector folder not found: Resources/{sectorPath}");
            return;
        }
        
        Debug.Log($"Processing sector folder: {sectorPath}");
        
        // First, check if this folder contains enemy type subfolders
        bool hasTypeSubfolders = false;
        foreach (EnemyType type in System.Enum.GetValues(typeof(EnemyType)))
        {
            string typePath = $"{sectorPath}/{type}";
            if (ResourcePathExists(typePath))
            {
                hasTypeSubfolders = true;
                ProcessTypeFolder(typePath, type, sector, existingModels, newModels, ref updatedCount);
            }
        }
        
        // If no type subfolders, treat sprites directly in sector folder
        if (!hasTypeSubfolders)
        {
            // Load all sprites in this sector folder
            Sprite[] sprites = Resources.LoadAll<Sprite>(sectorPath);
            
            if (sprites == null || sprites.Length == 0)
            {
                Debug.LogWarning($"No sprites found in {sectorPath}");
                return;
            }
            
            Debug.Log($"Found {sprites.Length} sprites directly in Sector{sector} folder");
            
            // Process each sprite
            foreach (var sprite in sprites)
            {
                if (sprite == null) continue;
                
                string modelName = sprite.name;
                Debug.Log($"Processing sprite: {modelName}");
                
                // Try to determine enemy type from name
                EnemyType type = GetEnemyTypeFromName(modelName);
                
                // Check if this model already exists
                if (existingModels.TryGetValue(modelName, out ShipModel existingModel))
                {
                    bool updated = false;
                    
                    // Update icon if missing
                    if (existingModel.shipIcon == null)
                    {
                        existingModel.shipIcon = sprite;
                        updated = true;
                        Debug.Log($"Updated existing model: {modelName} with new sprite");
                    }
                    
                    // Update sector/type if needed
                    if (existingModel.minSectorLevel != sector || existingModel.shipType != type)
                    {
                        existingModel.minSectorLevel = sector;
                        existingModel.shipType = type;
                        updated = true;
                        Debug.Log($"Updated existing model: {modelName} with new sector/type: Sector{sector}/{type}");
                    }
                    
                    if (updated) updatedCount++;
                }
                else
                {
                    // Create new ship model
                    ShipModel newModel = new ShipModel
                    {
                        modelName = modelName,
                        shipType = type,
                        minSectorLevel = sector,
                        shipIcon = sprite,
                        description = "" // Empty description by default
                    };
                    
                    // Add to new models list
                    newModels.Add(newModel);
                    
                    // Add to existing models dictionary to avoid duplicates
                    existingModels[modelName] = newModel;
                    
                    Debug.Log($"Created new ship model: {modelName} (Type: {type}, Sector: {sector})");
                }
            }
        }
    }
    
    /// <summary>
    /// Process a type folder within a sector folder
    /// </summary>
    private void ProcessTypeFolder(string typePath, EnemyType type, int sector, Dictionary<string, ShipModel> existingModels,
                                 List<ShipModel> newModels, ref int updatedCount)
    {
        Debug.Log($"Processing type folder: {typePath} for {type} in Sector{sector}");
        
        // Load all sprites in this type folder
        Sprite[] sprites = Resources.LoadAll<Sprite>(typePath);
        
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"No sprites found in {typePath}");
            return;
        }
        
        Debug.Log($"Found {sprites.Length} sprites for {type} in Sector{sector}");
        
        // Process each sprite
        foreach (var sprite in sprites)
        {
            if (sprite == null) continue;
            
            string modelName = sprite.name;
            Debug.Log($"Processing sprite: {modelName}");
            
            // Check if this model already exists
            if (existingModels.TryGetValue(modelName, out ShipModel existingModel))
            {
                bool updated = false;
                
                // Update icon if missing
                if (existingModel.shipIcon == null)
                {
                    existingModel.shipIcon = sprite;
                    updated = true;
                    Debug.Log($"Updated existing model: {modelName} with new sprite");
                }
                
                // Update sector/type if needed
                if (existingModel.minSectorLevel != sector || existingModel.shipType != type)
                {
                    existingModel.minSectorLevel = sector;
                    existingModel.shipType = type;
                    updated = true;
                    Debug.Log($"Updated existing model: {modelName} with new sector/type: Sector{sector}/{type}");
                }
                
                if (updated) updatedCount++;
            }
            else
            {
                // Create new ship model
                ShipModel newModel = new ShipModel
                {
                    modelName = modelName,
                    shipType = type,
                    minSectorLevel = sector,
                    shipIcon = sprite,
                    description = "" // Empty description by default
                };
                
                // Add to new models list
                newModels.Add(newModel);
                
                // Add to existing models dictionary to avoid duplicates
                existingModels[modelName] = newModel;
                
                Debug.Log($"Created new ship model: {modelName} (Type: {type}, Sector: {sector})");
            }
        }
    }
    
    /// <summary>
    /// Try to determine enemy type from name
    /// </summary>
    private EnemyType GetEnemyTypeFromName(string name)
    {
        name = name.ToLower();
        
        if (name.Contains("scavenger"))
            return EnemyType.Scavenger;
        else if (name.Contains("rival"))
            return EnemyType.Rival;
        else if (name.Contains("anomaly"))
            return EnemyType.Anomaly;
        else if (name.Contains("boss"))
            return EnemyType.Boss;
        
        // Default to Scavenger if can't determine
        return EnemyType.Scavenger;
    }
    
    /// <summary>
    /// Check if a resource path exists
    /// </summary>
    private bool ResourcePathExists(string path)
    {
        // We can test if a path exists by trying to load something from it
        // LoadAll returns an empty array if the path exists but has no resources
        // Returns null if the path doesn't exist
        return Resources.LoadAll(path) != null;
    }
    
    /// <summary>
    /// Get a ship model by its name
    /// </summary>
    public ShipModel GetShipByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        
        // Use the lookup dictionary first (case-insensitive)
        if (shipsByName.TryGetValue(name, out ShipModel model))
        {
            return model;
        }
        
        // Fallback: Look through the ship models list for a matching model name
        foreach (var ship in shipModels)
        {
            if (ship != null && 
                !string.IsNullOrEmpty(ship.modelName) && 
                ship.modelName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                // Add to lookup for future queries
                shipsByName[ship.modelName] = ship;
                return ship;
            }
        }
        
        Debug.LogWarning($"Ship model not found: {name}");
        return null;
    }
    
    /// <summary>
    /// Get all ships of a specific type
    /// </summary>
    public List<ShipModel> GetShipsByType(EnemyType type)
    {
        return shipsByType.TryGetValue(type, out List<ShipModel> ships) ? ships : new List<ShipModel>();
    }
    
    /// <summary>
    /// Get all ships available in a specific sector
    /// </summary>
    public List<ShipModel> GetShipsBySector(int sector)
    {
        List<ShipModel> result = new List<ShipModel>();
        foreach (var ship in shipModels)
        {
            if (ship.minSectorLevel <= sector)
            {
                result.Add(ship);
            }
        }
        return result;
    }
    
    /// <summary>
    /// Get all ships of a specific type available in a sector
    /// </summary>
    public List<ShipModel> GetShipsForTypeAndSector(EnemyType type, int sector)
    {
        List<ShipModel> result = new List<ShipModel>();
        var typeShips = GetShipsByType(type);
        foreach (var ship in typeShips)
        {
            if (ship.minSectorLevel <= sector)
            {
                result.Add(ship);
            }
        }
        return result;
    }
    
    /// <summary>
    /// Get a random ship model of a specific type for a specific sector
    /// </summary>
    public ShipModel GetRandomShipForTypeAndSector(EnemyType type, int sector)
    {
        // Use the lookup system first
        var availableShips = GetShipsForTypeAndSector(type, sector);
        if (availableShips.Count > 0)
        {
            return availableShips[Random.Range(0, availableShips.Count)];
        }
        
        // Fallback: Manual search
        List<ShipModel> fallbackShips = new List<ShipModel>();
        foreach (var ship in shipModels)
        {
            if (ship != null && 
                ship.shipType == type && 
                ship.minSectorLevel <= sector)
            {
                fallbackShips.Add(ship);
            }
        }
        
        if (fallbackShips.Count > 0)
        {
            // Refresh lookups since we found ships that weren't in the lookup
            InitializeLookups();
            return fallbackShips[Random.Range(0, fallbackShips.Count)];
        }
        
        Debug.LogWarning($"No ships found for type {type} in sector {sector}");
        return null;
    }

    /// <summary>
    /// Get the icon for a specific ship model
    /// </summary>
    public Sprite GetIconForShip(string modelName)
    {
        if (string.IsNullOrEmpty(modelName)) return null;
        
        // Use the lookup dictionary first (case-insensitive)
        if (shipsByName.TryGetValue(modelName, out ShipModel model))
        {
            return model.shipIcon;
        }
        
        // Fallback: Look through the ship models list for a matching model name
        foreach (var ship in shipModels)
        {
            if (ship != null && 
                !string.IsNullOrEmpty(ship.modelName) && 
                ship.modelName.Equals(modelName, System.StringComparison.OrdinalIgnoreCase))
            {
                // Add to lookup for future queries
                shipsByName[ship.modelName] = ship;
                return ship.shipIcon;
            }
        }
        
        Debug.LogWarning($"Ship model not found: {modelName}");
        return null;
    }

    /// <summary>
    /// Get all available ship models for a type and sector
    /// </summary>
    public List<ShipModel> GetAllShipsForTypeAndSector(EnemyType type, int sector)
    {
        // This is an alias for GetShipsForTypeAndSector to maintain compatibility
        return GetShipsForTypeAndSector(type, sector);
    }

    // Add a public method to access the ship models
    public List<ShipModel> GetShipModels()
    {
        return shipModels;
    }
    
    // Add a method to add a ship model
    public void AddShipModel(ShipModel model)
    {
        if (model != null)
        {
            shipModels.Add(model);
            InitializeLookups(); // Refresh lookups when adding a model
        }
    }
    
    // Add a method to add multiple ship models
    public void AddShipModels(List<ShipModel> models)
    {
        if (models != null)
        {
            shipModels.AddRange(models);
            InitializeLookups(); // Refresh lookups when adding models
        }
    }
}