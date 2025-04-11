using UnityEngine;
using System.Collections.Generic;
using System;

public class FacilityManager : MonoBehaviour
{
    // Singleton pattern
    public static FacilityManager Instance { get; private set; }

    // Resources
    public float CurrentRecyclingPoints { get; private set; }
    public float CurrentDimensionalPotential { get; private set; }
    public float ContaminationLevel { get; private set; }

    // Events
    public event Action<string> OnUpgradeCompleted;
    public event Action<float> OnRecyclingPointsChanged;
    public event Action<float> OnDimensionalPotentialChanged;
    public event Action<float> OnContaminationChanged;

    // Facility sections and their upgrades
    private Dictionary<string, FacilityUpgrade> facilityUpgrades;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFacility();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFacility()
    {
        facilityUpgrades = new Dictionary<string, FacilityUpgrade>();
        CurrentRecyclingPoints = 1000f; // Starting resources
        CurrentDimensionalPotential = 50f;
        ContaminationLevel = 0.1f;

        // Initialize facility sections
        CreateUpgrade("WasteStorage", "Stores and processes collected waste", 3, 500f, 25f, 1.5f);
        CreateUpgrade("RecyclingLab", "Analyzes and recycles waste", 3, 750f, 35f, 1.6f);
        CreateUpgrade("StabilizationChamber", "Controls dimensional contamination", 3, 1000f, 45f, 1.7f);
        CreateUpgrade("ExpeditionCenter", "Manages waste collection expeditions", 3, 1250f, 55f, 1.8f);
    }

    private void CreateUpgrade(string name, string description, int maxLevel, float baseRP, float basePotential, float multiplier)
    {
        facilityUpgrades[name] = new FacilityUpgrade(name, description, maxLevel, baseRP, basePotential, multiplier);
    }

    public bool TryUpgradeFacility(string upgradeName)
    {
        if (!facilityUpgrades.ContainsKey(upgradeName))
            return false;

        var upgrade = facilityUpgrades[upgradeName];
        float currentRP = CurrentRecyclingPoints;
        float currentDP = CurrentDimensionalPotential;

        if (upgrade.TryUpgrade(ref currentRP, ref currentDP))
        {
            CurrentRecyclingPoints = currentRP;
            CurrentDimensionalPotential = currentDP;
            
            // Update contamination based on upgrades
            UpdateContaminationLevel();
            
            OnUpgradeCompleted?.Invoke(upgradeName);
            OnRecyclingPointsChanged?.Invoke(CurrentRecyclingPoints);
            OnDimensionalPotentialChanged?.Invoke(CurrentDimensionalPotential);
            
            return true;
        }

        return false;
    }

    private void UpdateContaminationLevel()
    {
        float totalStabilization = 0f;
        foreach (var upgrade in facilityUpgrades.Values)
        {
            if (upgrade.UpgradeName == "StabilizationChamber")
            {
                totalStabilization = upgrade.CurrentLevel * 0.2f;
                break;
            }
        }

        ContaminationLevel = Mathf.Max(0.1f, 0.5f - totalStabilization);
        OnContaminationChanged?.Invoke(ContaminationLevel);
    }

    public void AddRecyclingPoints(float amount)
    {
        CurrentRecyclingPoints += amount;
        OnRecyclingPointsChanged?.Invoke(CurrentRecyclingPoints);
    }

    public void AddDimensionalPotential(float amount)
    {
        CurrentDimensionalPotential += amount;
        OnDimensionalPotentialChanged?.Invoke(CurrentDimensionalPotential);
    }

    public FacilityUpgrade GetUpgrade(string upgradeName)
    {
        return facilityUpgrades.ContainsKey(upgradeName) ? facilityUpgrades[upgradeName] : null;
    }

    public Dictionary<string, float> GetFacilityBenefits()
    {
        var totalBenefits = new Dictionary<string, float>();
        
        foreach (var upgrade in facilityUpgrades.Values)
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

    public IReadOnlyDictionary<string, FacilityUpgrade> GetAllUpgrades()
    {
        return facilityUpgrades;
    }
} 