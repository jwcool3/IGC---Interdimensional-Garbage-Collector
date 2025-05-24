using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Manages the ship scanning system with energy management and discovery mechanics
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
    /// Generate a discovered ship based on scan type
    /// </summary>
    private DiscoveredShip GenerateDiscoveredShip(bool isCommonScan)
    {
        ShipRarity rarity;
        
        if (isCommonScan)
        {
            // Common category rarities
            float roll = UnityEngine.Random.value;
            if (roll <= 0.70f)
                rarity = ShipRarity.VeryCommon;
            else if (roll <= 0.90f)
                rarity = ShipRarity.Common;
            else
                rarity = ShipRarity.SlightlyRare;
        }
        else
        {
            // Rare category rarities
            float roll = UnityEngine.Random.value;
            if (roll <= 0.60f)
                rarity = ShipRarity.Rare;
            else if (roll <= 0.90f)
                rarity = ShipRarity.Epic;
            else if (roll <= 0.99f)
                rarity = ShipRarity.Legendary;
            else
                rarity = ShipRarity.Anomaly;
        }
        
        // For now, generate a basic ship - we'll expand this with ShipDatabase later
        DiscoveredShip ship = new DiscoveredShip
        {
            ShipName = GenerateShipName(rarity),
            Rarity = rarity,
            Level = CalculateShipLevel(rarity),
            ShipType = GenerateShipType(rarity),
            DiscoveryTime = DateTime.Now,
            
            // Basic stats based on rarity
            AttackPower = CalculateShipStat(rarity, 10f, 50f),
            Defense = CalculateShipStat(rarity, 5f, 25f),
            Health = CalculateShipStat(rarity, 50f, 200f),
            
            // Trade values
            TradeValue = CalculateTradeValue(rarity),
            AvailableTradeItems = GenerateTradeItems(rarity)
        };
        
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
}