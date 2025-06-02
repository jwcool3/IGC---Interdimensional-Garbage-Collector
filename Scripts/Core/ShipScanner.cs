using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the ship scanning system with energy management and discovery mechanics
/// Updated to use static ScannerShipDatabase methods
/// </summary>
public class ShipScanner : MonoBehaviour
{
    // Singleton pattern
    public static ShipScanner Instance { get; private set; }
    
    [Header("Energy System")]
    [SerializeField] private float maxScannerEnergy = 100f;
    [SerializeField] private float currentScannerEnergy = 100f;
    [SerializeField] private float energyRegenRate = 5f; // Energy per second
    [SerializeField] private float commonScanCost = 15f;
    [SerializeField] private float rareScanCost = 35f;
    
    [Header("Success Rates")]
    [SerializeField] private float baseCommonSuccessRate = 0.8f; // 80% base
    [SerializeField] private float baseRareSuccessRate = 0.3f;   // 30% base
    [SerializeField] private float scannerLevelBonus = 0.1f;     // 10% per scanner level
    [SerializeField] private float locationDifficultyModifier = 1f; // Adjusted by current location
    
    [Header("Scan Timing")]
    [SerializeField] private float scanDuration = 3f; // How long a scan takes
    [SerializeField] private bool isScanning = false;
    
    // Current discovered ship
    private DiscoveredShip currentDiscoveredShip;
    
    // Events
    public event Action<float> OnEnergyChanged;
    public event Action<DiscoveredShip> OnShipDiscovered;
    public event Action OnScanStarted;
    public event Action OnScanCompleted;
    public event Action OnScanFailed;
    public event Action OnShipInteracted; // When ship is fought/traded with
    
    // Properties
    public float CurrentEnergy => currentScannerEnergy;
    public float MaxEnergy => maxScannerEnergy;
    public bool IsScanning => isScanning;
    public bool HasDiscoveredShip => currentDiscoveredShip != null;
    public DiscoveredShip CurrentShip => currentDiscoveredShip;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("ShipScanner: Initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Start energy regeneration
        StartCoroutine(RegenerateEnergy());
        
        // Initialize energy to full
        currentScannerEnergy = maxScannerEnergy;
        OnEnergyChanged?.Invoke(currentScannerEnergy);
        
        // Ensure the static database is initialized
        ScannerShipDatabase.InitializeStaticDatabase();
    }
    
    private void Update()
    {
        // Update location difficulty modifier based on current location
        UpdateLocationDifficulty();
    }
    
    /// <summary>
    /// Attempt to scan for a common ship
    /// </summary>
    public bool TryScanForCommonShip()
    {
        if (!CanScan(true))
            return false;
            
        StartCoroutine(PerformScan(true));
        return true;
    }
    
    /// <summary>
    /// Attempt to scan for a rare ship
    /// </summary>
    public bool TryScanForRareShip()
    {
        if (!CanScan(false))
            return false;
            
        StartCoroutine(PerformScan(false));
        return true;
    }
    
    /// <summary>
    /// Check if we can perform a scan
    /// </summary>
    public bool CanScan(bool isCommonScan)
    {
        if (isScanning)
        {
            Debug.Log("Already scanning!");
            return false;
        }
        
        if (HasDiscoveredShip)
        {
            Debug.Log("Must interact with current ship before scanning again!");
            return false;
        }
        
        float requiredEnergy = isCommonScan ? commonScanCost : rareScanCost;
        if (currentScannerEnergy < requiredEnergy)
        {
            Debug.Log($"Insufficient energy! Need {requiredEnergy}, have {currentScannerEnergy}");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Perform the actual scanning process
    /// </summary>
    private IEnumerator PerformScan(bool isCommonScan)
    {
        // Start scanning
        isScanning = true;
        OnScanStarted?.Invoke();
        
        // Consume energy
        float energyCost = isCommonScan ? commonScanCost : rareScanCost;
        ConsumeEnergy(energyCost);
        
        Debug.Log($"Starting {(isCommonScan ? "common" : "rare")} scan... Energy cost: {energyCost}");
        
        // Wait for scan duration
        yield return new WaitForSeconds(scanDuration);
        
        // Calculate success chance
        float successRate = CalculateSuccessRate(isCommonScan);
        bool success = UnityEngine.Random.value <= successRate;
        
        isScanning = false;
        OnScanCompleted?.Invoke();
        
        if (success)
        {
            // Generate discovered ship
            currentDiscoveredShip = GenerateDiscoveredShip(isCommonScan);
            OnShipDiscovered?.Invoke(currentDiscoveredShip);
            
            Debug.Log($"Scan successful! Discovered: {currentDiscoveredShip.ShipName} ({currentDiscoveredShip.Rarity})");
        }
        else
        {
            OnScanFailed?.Invoke();
            Debug.Log("Scan failed - no ships detected");
        }
    }
    
    /// <summary>
    /// Calculate success rate based on scan type, scanner level, and location
    /// </summary>
    private float CalculateSuccessRate(bool isCommonScan)
    {
        float baseRate = isCommonScan ? baseCommonSuccessRate : baseRareSuccessRate;
        
        // Add scanner compartment bonus
        float scannerBonus = 0f;
        if (ShipManager.Instance != null)
        {
            var scannerCompartment = ShipManager.Instance.GetAllCompartments()
                .Find(c => c.Type == CompartmentType.Scanner);
            if (scannerCompartment != null)
            {
                scannerBonus = (scannerCompartment.CurrentLevel - 1) * scannerLevelBonus;
            }
        }
        
        // Apply location difficulty
        float finalRate = (baseRate + scannerBonus) * locationDifficultyModifier;
        
        // Clamp between 0 and 1
        finalRate = Mathf.Clamp01(finalRate);
        
        Debug.Log($"Success rate calculation: Base={baseRate:P1}, Scanner Bonus={scannerBonus:P1}, Location Modifier={locationDifficultyModifier:F2}, Final={finalRate:P1}");
        
        return finalRate;
    }
    
    /// <summary>
    /// IMPROVED: Enhanced ship generation with better fallback handling
    /// </summary>
    private DiscoveredShip GenerateDiscoveredShip(bool isCommonScan)
    {
        ShipRarity rarity = DetermineShipRarity(isCommonScan);
        string currentLocation = GetCurrentLocationName();
        
        // Try multiple approaches to find a suitable ship
        ScannerShipModel shipModel = null;
        
        // Approach 1: Try exact location match
        shipModel = ScannerShipDatabase.GetRandomShipByRarityAndLocationStatic(rarity, currentLocation);
        
        // Approach 2: Try with "All Locations" if no location-specific ship found
        if (shipModel == null)
        {
            shipModel = ScannerShipDatabase.GetRandomShipByRarityAndLocationStatic(rarity, "All Locations");
        }
        
        // Approach 3: Try any ship of this rarity regardless of location
        if (shipModel == null)
        {
            shipModel = ScannerShipDatabase.GetRandomShipByRarityStatic(rarity);
        }
        
        // Approach 4: Try a different rarity in the same category (common/rare)
        if (shipModel == null)
        {
            Debug.LogWarning($"ShipScanner: No {rarity} ships found, trying fallback rarities");
            
            if (isCommonScan)
            {
                // Try other common rarities
                ShipRarity[] commonRarities = { ShipRarity.Common, ShipRarity.VeryCommon, ShipRarity.SlightlyRare };
                foreach (var fallbackRarity in commonRarities)
                {
                    if (fallbackRarity != rarity)
                    {
                        shipModel = ScannerShipDatabase.GetRandomShipByRarityStatic(fallbackRarity);
                        if (shipModel != null)
                        {
                            Debug.Log($"ShipScanner: Using fallback rarity {fallbackRarity} instead of {rarity}");
                            rarity = fallbackRarity; // Update rarity for the discovered ship
                            break;
                        }
                    }
                }
            }
            else
            {
                // Try other rare rarities
                ShipRarity[] rareRarities = { ShipRarity.Rare, ShipRarity.Epic, ShipRarity.Legendary, ShipRarity.Anomaly };
                foreach (var fallbackRarity in rareRarities)
                {
                    if (fallbackRarity != rarity)
                    {
                        shipModel = ScannerShipDatabase.GetRandomShipByRarityStatic(fallbackRarity);
                        if (shipModel != null)
                        {
                            Debug.Log($"ShipScanner: Using fallback rarity {fallbackRarity} instead of {rarity}");
                            rarity = fallbackRarity;
                            break;
                        }
                    }
                }
            }
        }
        
        // Final approach: Generate procedurally if all else fails
        if (shipModel != null)
        {
            Debug.Log($"ShipScanner: Using scanner ship model '{shipModel.shipName}' from database");
            return shipModel.CreateDiscoveredShip();
        }
        else
        {
            Debug.LogWarning($"ShipScanner: No suitable ships found in database, generating procedural ship");
            return GenerateFallbackShip(rarity);
        }
    }
    
    /// <summary>
    /// IMPROVED: Cleaner rarity determination
    /// </summary>
    private ShipRarity DetermineShipRarity(bool isCommonScan)
    {
        if (isCommonScan)
        {
            // Common category rarities
            float roll = UnityEngine.Random.value;
            if (roll <= 0.70f) return ShipRarity.VeryCommon;
            if (roll <= 0.90f) return ShipRarity.Common;
            return ShipRarity.SlightlyRare;
        }
        else
        {
            // Rare category rarities
            float roll = UnityEngine.Random.value;
            if (roll <= 0.60f) return ShipRarity.Rare;
            if (roll <= 0.90f) return ShipRarity.Epic;
            if (roll <= 0.99f) return ShipRarity.Legendary;
            return ShipRarity.Anomaly;
        }
    }
    
    /// <summary>
    /// Get the current location name for ship filtering
    /// </summary>
    private string GetCurrentLocationName()
    {
        if (LocationManager.Instance?.GetCurrentLocation() != null)
        {
            return LocationManager.Instance.GetCurrentLocation().displayName;
        }
        
        return "Unknown Sector";
    }
    
    /// <summary>
    /// Fallback ship generation when database has no suitable ships
    /// </summary>
    private DiscoveredShip GenerateFallbackShip(ShipRarity rarity)
    {
        Debug.Log($"ShipScanner: Generating fallback ship for rarity {rarity}");
        
        DiscoveredShip ship = new DiscoveredShip
        {
            ShipName = GenerateShipName(rarity),
            Rarity = rarity,
            Level = CalculateShipLevel(rarity),
            ShipType = GenerateShipType(rarity),
            DiscoveryTime = System.DateTime.Now,
            
            // Basic stats based on rarity
            AttackPower = CalculateShipStat(rarity, 10f, 50f),
            Defense = CalculateShipStat(rarity, 5f, 25f),
            Health = CalculateShipStat(rarity, 50f, 200f),
            
            // Trade values
            TradeValue = CalculateTradeValue(rarity),
            AvailableTradeItems = GenerateTradeItems(rarity)
        };
        
        ship.CurrentHealth = ship.Health;
        return ship;
    }
    
    /// <summary>
    /// Generate a ship name based on rarity
    /// </summary>
    private string GenerateShipName(ShipRarity rarity)
    {
        string[] prefixes = { "Stellar", "Void", "Quantum", "Nebula", "Cosmic", "Solar", "Galactic", "Orbital" };
        string[] names = { "Wanderer", "Collector", "Explorer", "Traveler", "Merchant", "Seeker", "Drifter", "Hunter" };
        string[] suffixes = { "VII", "Prime", "Alpha", "Beta", "Omega", "One", "Zero", "X" };
        
        string baseName = $"{prefixes[UnityEngine.Random.Range(0, prefixes.Length)]} {names[UnityEngine.Random.Range(0, names.Length)]}";
        
        // Add suffix for higher rarities
        if ((int)rarity >= (int)ShipRarity.Rare)
        {
            baseName += $" {suffixes[UnityEngine.Random.Range(0, suffixes.Length)]}";
        }
        
        return baseName;
    }
    
    /// <summary>
    /// Calculate ship level based on rarity and current location
    /// </summary>
    private int CalculateShipLevel(ShipRarity rarity)
    {
        int baseLevel = 1;
        
        // Add level based on rarity
        switch (rarity)
        {
            case ShipRarity.VeryCommon: baseLevel = UnityEngine.Random.Range(1, 3); break;
            case ShipRarity.Common: baseLevel = UnityEngine.Random.Range(2, 5); break;
            case ShipRarity.SlightlyRare: baseLevel = UnityEngine.Random.Range(4, 7); break;
            case ShipRarity.Rare: baseLevel = UnityEngine.Random.Range(6, 10); break;
            case ShipRarity.Epic: baseLevel = UnityEngine.Random.Range(9, 15); break;
            case ShipRarity.Legendary: baseLevel = UnityEngine.Random.Range(14, 20); break;
            case ShipRarity.Anomaly: baseLevel = UnityEngine.Random.Range(18, 25); break;
        }
        
        // Could add location-based level modifiers here later
        
        return baseLevel;
    }
    
    /// <summary>
    /// Generate ship type based on rarity
    /// </summary>
    private string GenerateShipType(ShipRarity rarity)
    {
        string[] commonTypes = { "Scavenger Vessel", "Trading Pod", "Exploration Craft", "Mining Ship" };
        string[] rareTypes = { "Battle Cruiser", "Research Vessel", "Dimensional Carrier", "War Frigate", "Command Ship" };
        
        if ((int)rarity <= (int)ShipRarity.SlightlyRare)
        {
            return commonTypes[UnityEngine.Random.Range(0, commonTypes.Length)];
        }
        else
        {
            return rareTypes[UnityEngine.Random.Range(0, rareTypes.Length)];
        }
    }
    
    /// <summary>
    /// Calculate ship stats based on rarity
    /// </summary>
    private float CalculateShipStat(ShipRarity rarity, float baseStat, float maxStat)
    {
        float rarityMultiplier = (int)rarity / 6f; // 0 to 1 based on rarity
        float randomVariation = UnityEngine.Random.Range(0.8f, 1.2f);
        
        return Mathf.Lerp(baseStat, maxStat, rarityMultiplier) * randomVariation;
    }
    
    /// <summary>
    /// Calculate trade value based on rarity
    /// </summary>
    private int CalculateTradeValue(ShipRarity rarity)
    {
        int baseValue = 10;
        int rarityMultiplier = (int)rarity + 1;
        return baseValue * rarityMultiplier * UnityEngine.Random.Range(1, 3);
    }
    
    /// <summary>
    /// Generate available trade items
    /// </summary>
    private string[] GenerateTradeItems(ShipRarity rarity)
    {
        // Placeholder for now - will expand with actual ship part system
        string[] basicItems = { "Basic Hull Plating", "Standard Engine Parts", "Navigation Components" };
        string[] advancedItems = { "Advanced Alloys", "Quantum Processors", "Exotic Matter Cores", "Reality Stabilizers" };
        
        if ((int)rarity <= (int)ShipRarity.SlightlyRare)
        {
            return new string[] { basicItems[UnityEngine.Random.Range(0, basicItems.Length)] };
        }
        else
        {
            return new string[] { 
                basicItems[UnityEngine.Random.Range(0, basicItems.Length)],
                advancedItems[UnityEngine.Random.Range(0, advancedItems.Length)]
            };
        }
    }
    
    /// <summary>
    /// Update location difficulty modifier
    /// </summary>
    private void UpdateLocationDifficulty()
    {
        if (LocationManager.Instance?.GetCurrentLocation() != null)
        {
            var location = LocationManager.Instance.GetCurrentLocation();
            // Higher danger = harder to find rare ships, easier to find common ones
            locationDifficultyModifier = 1f - (location.dangerLevel * 0.3f);
            locationDifficultyModifier = Mathf.Clamp(locationDifficultyModifier, 0.3f, 1.2f);
        }
        else
        {
            locationDifficultyModifier = 1f;
        }
    }
    
    /// <summary>
    /// Consume scanner energy
    /// </summary>
    private void ConsumeEnergy(float amount)
    {
        currentScannerEnergy = Mathf.Max(0, currentScannerEnergy - amount);
        OnEnergyChanged?.Invoke(currentScannerEnergy);
    }
    
    /// <summary>
    /// Regenerate energy over time
    /// </summary>
    private IEnumerator RegenerateEnergy()
    {
        while (true)
        {
            if (currentScannerEnergy < maxScannerEnergy)
            {
                currentScannerEnergy = Mathf.Min(maxScannerEnergy, currentScannerEnergy + energyRegenRate * Time.deltaTime);
                OnEnergyChanged?.Invoke(currentScannerEnergy);
            }
            
            yield return null; // Wait one frame
        }
    }
    
    /// <summary>
    /// Remove current discovered ship (called after interaction)
    /// </summary>
    public void ClearCurrentShip()
    {
        currentDiscoveredShip = null;
        OnShipInteracted?.Invoke();
        Debug.Log("Current ship cleared - ready for new scan");
    }
    
    /// <summary>
    /// Get scan cost for UI display
    /// </summary>
    public float GetScanCost(bool isCommonScan)
    {
        return isCommonScan ? commonScanCost : rareScanCost;
    }
    
    /// <summary>
    /// Get success rate for UI display
    /// </summary>
    public float GetSuccessRate(bool isCommonScan)
    {
        return CalculateSuccessRate(isCommonScan);
    }
    
    /// <summary>
    /// IMPROVED: Enhanced debugging with more detailed statistics
    /// </summary>
    [ContextMenu("Show Scanner Database Stats")]
    public void ShowDatabaseStats()
    {
        Debug.Log("=== SCANNER DATABASE STATISTICS ===");
        
        var stats = ScannerShipDatabase.GetDatabaseStatistics();
        Debug.Log($"Total Ships Available: {stats["TotalShips"]}");
        Debug.Log($"Unique Locations: {stats["UniqueLocations"]}");
        
        string currentLocation = GetCurrentLocationName();
        Debug.Log($"Current Location: {currentLocation}");
        
        // Show rarity breakdown
        if (stats["RarityBreakdown"] is Dictionary<ShipRarity, int> rarityBreakdown)
        {
            Debug.Log("=== RARITY BREAKDOWN ===");
            foreach (var kvp in rarityBreakdown)
            {
                if (kvp.Value > 0)
                    Debug.Log($"  {kvp.Key}: {kvp.Value} ships");
            }
        }
        
        // Show location distribution  
        if (stats["LocationDistribution"] is Dictionary<string, int> locationDist)
        {
            Debug.Log("=== LOCATION DISTRIBUTION ===");
            foreach (var kvp in locationDist)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value} ships");
            }
        }
        
        // Show level and trade ranges
        if (stats.ContainsKey("MinLevel"))
        {
            Debug.Log($"Level Range: {stats["MinLevel"]} - {stats["MaxLevel"]}");
            Debug.Log($"Average Trade Value: {stats["AvgTradeValue"]:F1}");
        }
        
        // Test actual ship generation for current location
        Debug.Log("=== SHIP AVAILABILITY TEST ===");
        foreach (ShipRarity rarity in System.Enum.GetValues(typeof(ShipRarity)))
        {
            var ship = ScannerShipDatabase.GetRandomShipByRarityAndLocationStatic(rarity, currentLocation);
            if (ship != null)
            {
                Debug.Log($"  {rarity}: {ship.shipName} (Level {ship.minLevel}-{ship.maxLevel}, Trade {ship.minTradeValue}-{ship.maxTradeValue})");
            }
            else
            {
                Debug.Log($"  {rarity}: No ships available");
            }
        }
    }
    
    /// <summary>
    /// NEW: Validate scanner system health
    /// </summary>
    [ContextMenu("Validate Scanner System")]
    public void ValidateScannerSystem()
    {
        Debug.Log("=== SCANNER SYSTEM VALIDATION ===");
        
        List<string> issues = new List<string>();
        List<string> warnings = new List<string>();
        
        // Check database initialization
        if (ScannerShipDatabase.GetTotalShipCountStatic() == 0)
        {
            issues.Add("No ships in database");
        }
        
        // Check energy settings
        if (maxScannerEnergy <= 0)
            issues.Add("Max energy must be positive");
        if (energyRegenRate <= 0)
            issues.Add("Energy regen rate must be positive");
        if (commonScanCost <= 0 || rareScanCost <= 0)
            issues.Add("Scan costs must be positive");
        if (commonScanCost >= maxScannerEnergy)
            warnings.Add("Common scan cost is very high compared to max energy");
        if (rareScanCost >= maxScannerEnergy)
            warnings.Add("Rare scan cost is very high compared to max energy");
        
        // Check success rates
        if (baseCommonSuccessRate <= 0 || baseCommonSuccessRate > 1)
            issues.Add("Common success rate must be between 0 and 1");
        if (baseRareSuccessRate <= 0 || baseRareSuccessRate > 1)
            issues.Add("Rare success rate must be between 0 and 1");
        
        // Check scanner compartment integration
        if (ShipManager.Instance != null)
        {
            var scannerCompartment = ShipManager.Instance.GetAllCompartments()
                .Find(c => c.Type == CompartmentType.Scanner);
            if (scannerCompartment == null)
            {
                warnings.Add("No Scanner compartment found in Ship Manager");
            }
        }
        else
        {
            warnings.Add("ShipManager instance not found");
        }
        
        // Check location manager integration
        if (LocationManager.Instance == null)
        {
            warnings.Add("LocationManager instance not found");
        }
        
        // Output results
        if (issues.Count == 0 && warnings.Count == 0)
        {
            Debug.Log("✅ Scanner system validation passed - no issues found!");
        }
        else
        {
            if (issues.Count > 0)
            {
                Debug.LogError($"❌ Found {issues.Count} critical issues:");
                foreach (var issue in issues)
                {
                    Debug.LogError($"  - {issue}");
                }
            }
            
            if (warnings.Count > 0)
            {
                Debug.LogWarning($"⚠️ Found {warnings.Count} warnings:");
                foreach (var warning in warnings)
                {
                    Debug.LogWarning($"  - {warning}");
                }
            }
        }
    }
}