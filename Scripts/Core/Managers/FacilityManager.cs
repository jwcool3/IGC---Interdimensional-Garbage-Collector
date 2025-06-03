using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FacilityManager : MonoBehaviour
{
    // Singleton pattern
    public static FacilityManager Instance { get; private set; }

    // Available upgrades (legacy system)
    private Dictionary<string, FacilityUpgrade> upgrades = new Dictionary<string, FacilityUpgrade>();
    
    // New processing facilities
    private List<ProcessingFacilityBase> processingFacilities = new List<ProcessingFacilityBase>();
    private Dictionary<string, ProcessingFacilityBase> facilitiesByName = new Dictionary<string, ProcessingFacilityBase>();

    // Events
    public event Action<string> OnUpgradeCompleted;
    public event Action<ProcessingFacilityBase> OnProcessingFacilityAdded;
    public event Action<ProcessingFacilityBase> OnProcessingFacilityRemoved;
    public event Action<ProcessingFacilityBase> OnProcessingFacilityUpgraded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUpgrades();
            InitializeProcessingFacilities();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Find and register any existing processing facilities in the scene
        RegisterExistingFacilities();
    }

    private void InitializeUpgrades()
    {
        // Create initial upgrade options (legacy system)
        upgrades.Add("WasteStorage", new FacilityUpgrade(
            "Waste Storage Wing",
            "Increases your waste storage capacity.",
            3, 100f, 10f));

        upgrades.Add("RecyclingLab", new FacilityUpgrade(
            "Recycling Laboratory",
            "Improves recycling efficiency and point generation.",
            3, 150f, 15f));

        upgrades.Add("StabilizationChamber", new FacilityUpgrade(
            "Dimensional Stabilization",
            "Reduces contamination from collected waste.",
            3, 200f, 20f));

        upgrades.Add("ExpeditionCenter", new FacilityUpgrade(
            "Expedition Center",
            "Improves the quality of collected waste items.",
            3, 250f, 25f));
    }

    private void InitializeProcessingFacilities()
    {
        // Initialize processing facility system
        Debug.Log("FacilityManager: Processing facility system initialized");
    }

    private void RegisterExistingFacilities()
    {
        // Find all processing facilities in the scene
        ProcessingFacilityBase[] facilities = FindObjectsOfType<ProcessingFacilityBase>();
        
        foreach (var facility in facilities)
        {
            RegisterProcessingFacility(facility);
        }
        
        Debug.Log($"FacilityManager: Registered {facilities.Length} processing facilities");
    }

    #region Legacy Upgrade System

    // Try to upgrade a specific facility section (legacy)
    public bool TryUpgrade(string upgradeName)
    {
        if (!upgrades.ContainsKey(upgradeName))
            return false;

        var upgrade = upgrades[upgradeName];
        float currentRP = ResourceManager.Instance.RecyclingPoints;
        float currentDP = ResourceManager.Instance.DimensionalPotential;

        if (upgrade.TryUpgrade(ref currentRP, ref currentDP))
        {
            // Update resources
            ResourceManager.Instance.SetRecyclingPoints(currentRP);
            ResourceManager.Instance.SetDimensionalPotential(currentDP);

            // Apply upgrade effects
            ApplyUpgradeEffects(upgradeName);

            // Notify listeners
            OnUpgradeCompleted?.Invoke(upgradeName);

            return true;
        }

        return false;
    }

    // Get all available upgrades (legacy)
    public Dictionary<string, FacilityUpgrade> GetAvailableUpgrades()
    {
        return upgrades;
    }

    // Get a specific upgrade (legacy)
    public FacilityUpgrade GetUpgrade(string name)
    {
        if (upgrades.ContainsKey(name))
            return upgrades[name];
        return null;
    }

    // Apply effects from an upgrade (legacy)
    private void ApplyUpgradeEffects(string upgradeName)
    {
        if (!upgrades.ContainsKey(upgradeName))
            return;

        var upgrade = upgrades[upgradeName];

        // Apply different effects based on upgrade type
        switch (upgradeName)
        {
            case "StabilizationChamber":
                if (upgrade.CurrentBenefits.ContainsKey("ContaminationReduction"))
                {
                    float reduction = upgrade.CurrentBenefits["ContaminationReduction"];
                    float currentContamination = ResourceManager.Instance.ContaminationLevel;
                    ResourceManager.Instance.DecreaseContamination(currentContamination * reduction);
                }
                break;

            case "RecyclingLab":
                // Apply recycling efficiency bonus to all recycling facilities
                ApplyEfficiencyBonusToFacilities(ProcessingType.Recycling, 0.1f);
                break;

            case "WasteStorage":
                // Increase waste inventory capacity
                if (WasteInventoryManager.Instance != null)
                {
                    WasteInventoryManager.Instance.IncreaseCapacity(10);
                }
                break;

                // Other upgrade effects can be added here
        }
    }

    // Get total benefits across all upgrades (legacy)
    public Dictionary<string, float> GetTotalBenefits()
    {
        Dictionary<string, float> totalBenefits = new Dictionary<string, float>();

        foreach (var upgrade in upgrades.Values)
        {
            foreach (var benefit in upgrade.CurrentBenefits)
            {
                if (totalBenefits.ContainsKey(benefit.Key))
                    totalBenefits[benefit.Key] += benefit.Value;
                else
                    totalBenefits[benefit.Key] = benefit.Value;
            }
        }

        return totalBenefits;
    }

    #endregion

    #region Processing Facility Management

    /// <summary>
    /// Register a processing facility with the manager
    /// </summary>
    public void RegisterProcessingFacility(ProcessingFacilityBase facility)
    {
        if (facility == null) return;

        if (!processingFacilities.Contains(facility))
        {
            processingFacilities.Add(facility);
            facilitiesByName[facility.FacilityName] = facility;
            
            // Subscribe to facility events
            facility.OnFacilityActivated += OnFacilityActivated;
            facility.OnFacilityDeactivated += OnFacilityDeactivated;
            
            OnProcessingFacilityAdded?.Invoke(facility);
            Debug.Log($"FacilityManager: Registered processing facility - {facility.FacilityName}");
        }
    }

    /// <summary>
    /// Unregister a processing facility
    /// </summary>
    public void UnregisterProcessingFacility(ProcessingFacilityBase facility)
    {
        if (facility == null) return;

        if (processingFacilities.Contains(facility))
        {
            processingFacilities.Remove(facility);
            facilitiesByName.Remove(facility.FacilityName);
            
            // Unsubscribe from facility events
            facility.OnFacilityActivated -= OnFacilityActivated;
            facility.OnFacilityDeactivated -= OnFacilityDeactivated;
            
            OnProcessingFacilityRemoved?.Invoke(facility);
            Debug.Log($"FacilityManager: Unregistered processing facility - {facility.FacilityName}");
        }
    }

    /// <summary>
    /// Get all processing facilities
    /// </summary>
    public List<ProcessingFacilityBase> GetProcessingFacilities()
    {
        return new List<ProcessingFacilityBase>(processingFacilities);
    }

    /// <summary>
    /// Get processing facilities by type
    /// </summary>
    public List<ProcessingFacilityBase> GetProcessingFacilitiesByType(ProcessingType type)
    {
        return processingFacilities.Where(f => f.FacilityType == type).ToList();
    }

    /// <summary>
    /// Get a processing facility by name
    /// </summary>
    public ProcessingFacilityBase GetProcessingFacility(string name)
    {
        facilitiesByName.TryGetValue(name, out ProcessingFacilityBase facility);
        return facility;
    }

    /// <summary>
    /// Get operational processing facilities
    /// </summary>
    public List<ProcessingFacilityBase> GetOperationalFacilities()
    {
        return processingFacilities.Where(f => f.IsOperational).ToList();
    }

    /// <summary>
    /// Upgrade a processing facility
    /// </summary>
    public bool UpgradeProcessingFacility(ProcessingFacilityBase facility)
    {
        if (facility == null || !processingFacilities.Contains(facility))
            return false;

        // Check if upgrade is possible (could add resource costs here)
        if (facility.UpgradeFacility())
        {
            OnProcessingFacilityUpgraded?.Invoke(facility);
            Debug.Log($"FacilityManager: Upgraded facility - {facility.FacilityName} to level {facility.Level}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Apply efficiency bonus to facilities of a specific type
    /// </summary>
    private void ApplyEfficiencyBonusToFacilities(ProcessingType type, float bonus)
    {
        var facilities = GetProcessingFacilitiesByType(type);
        foreach (var facility in facilities)
        {
            // This would require adding a method to ProcessingFacilityBase to apply bonuses
            Debug.Log($"Applied {bonus:P} efficiency bonus to {facility.FacilityName}");
        }
    }

    /// <summary>
    /// Get facility statistics
    /// </summary>
    public Dictionary<string, object> GetFacilityStatistics()
    {
        var stats = new Dictionary<string, object>();
        
        stats["TotalFacilities"] = processingFacilities.Count;
        stats["OperationalFacilities"] = GetOperationalFacilities().Count;
        stats["FacilitiesByType"] = processingFacilities.GroupBy(f => f.FacilityType)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());
        stats["AverageLevel"] = processingFacilities.Count > 0 ? 
            processingFacilities.Average(f => f.Level) : 0;
        stats["TotalActiveJobs"] = processingFacilities.Sum(f => f.ActiveJobs);
        
        return stats;
    }

    #endregion

    #region Event Handlers

    private void OnFacilityActivated(ProcessingFacilityBase facility)
    {
        Debug.Log($"FacilityManager: Facility activated - {facility.FacilityName}");
    }

    private void OnFacilityDeactivated(ProcessingFacilityBase facility)
    {
        Debug.Log($"FacilityManager: Facility deactivated - {facility.FacilityName}");
    }

    #endregion

    #region Public API

    /// <summary>
    /// Check if a facility type is available
    /// </summary>
    public bool HasFacilityType(ProcessingType type)
    {
        return processingFacilities.Any(f => f.FacilityType == type && f.IsOperational);
    }

    /// <summary>
    /// Get the best facility for a specific processing type
    /// </summary>
    public ProcessingFacilityBase GetBestFacilityForType(ProcessingType type)
    {
        return processingFacilities
            .Where(f => f.FacilityType == type && f.IsOperational && f.AvailableSlots > 0)
            .OrderByDescending(f => f.Efficiency)
            .ThenByDescending(f => f.Level)
            .FirstOrDefault();
    }

    /// <summary>
    /// Get total processing capacity for a type
    /// </summary>
    public int GetTotalProcessingCapacity(ProcessingType type)
    {
        return processingFacilities
            .Where(f => f.FacilityType == type && f.IsOperational)
            .Sum(f => f.AvailableSlots);
    }

    /// <summary>
    /// Shutdown all facilities (emergency)
    /// </summary>
    public void EmergencyShutdownAll()
    {
        foreach (var facility in processingFacilities)
        {
            facility.SetOperational(false);
        }
        Debug.Log("FacilityManager: Emergency shutdown of all facilities");
    }

    /// <summary>
    /// Restart all facilities
    /// </summary>
    public void RestartAllFacilities()
    {
        foreach (var facility in processingFacilities)
        {
            facility.SetOperational(true);
        }
        Debug.Log("FacilityManager: Restarted all facilities");
    }

    #endregion
}