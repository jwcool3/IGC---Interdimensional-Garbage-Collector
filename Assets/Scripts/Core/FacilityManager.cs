using System;
using System.Collections.Generic;
using UnityEngine;

public class FacilityManager : MonoBehaviour
{
    // Singleton pattern
    public static FacilityManager Instance { get; private set; }

    // Available upgrades
    private Dictionary<string, FacilityUpgrade> upgrades = new Dictionary<string, FacilityUpgrade>();
    
    // Events
    public event Action<string> OnUpgradeCompleted;
    public event Action OnUpgradesFetched;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUpgrades();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeUpgrades()
    {
        // Initialize basic facility upgrades
        upgrades.Add("WasteStorage", new FacilityUpgrade
        {
            Name = "Waste Storage",
            MaxLevel = 3,
            CurrentLevel = 0,
            BaseRecyclingPointCost = 100f,
            BaseDimensionalPotentialCost = 10f,
            CostMultiplier = 1.5f
        });

        upgrades.Add("RecyclingLab", new FacilityUpgrade
        {
            Name = "Recycling Laboratory",
            MaxLevel = 3,
            CurrentLevel = 0,
            BaseRecyclingPointCost = 150f,
            BaseDimensionalPotentialCost = 15f,
            CostMultiplier = 1.75f
        });

        upgrades.Add("StabilizationChamber", new FacilityUpgrade
        {
            Name = "Stabilization Chamber",
            MaxLevel = 3,
            CurrentLevel = 0,
            BaseRecyclingPointCost = 200f,
            BaseDimensionalPotentialCost = 20f,
            CostMultiplier = 2f
        });
    }

    // Try to upgrade a specific facility section
    public bool TryUpgrade(string upgradeName)
    {
        if (!upgrades.ContainsKey(upgradeName))
            return false;
            
        var upgrade = upgrades[upgradeName];
        
        // Check if player can afford the upgrade
        bool canAfford = upgrade.CanAfford(
            ResourceManager.Instance.GetRecyclingPoints(),
            ResourceManager.Instance.GetDimensionalPotential()
        );
        
        if (!canAfford)
            return false;
        
        // Spend resources
        ResourceManager.Instance.SpendRecyclingPoints(upgrade.CurrentRecyclingPointCost);
        ResourceManager.Instance.SpendDimensionalPotential(upgrade.CurrentDimensionalPotentialCost);
        
        // Increase upgrade level
        upgrade.CurrentLevel++;
        
        // Apply upgrade effects
        ApplyUpgradeEffects(upgradeName);
        
        // Notify listeners
        OnUpgradeCompleted?.Invoke(upgradeName);
        
        return true;
    }
    
    // Get all available upgrades
    public Dictionary<string, FacilityUpgrade> GetAllUpgrades()
    {
        return upgrades;
    }
    
    // Get a specific upgrade
    public FacilityUpgrade GetUpgrade(string upgradeName)
    {
        return upgrades.ContainsKey(upgradeName) ? upgrades[upgradeName] : null;
    }
    
    // Apply effects from an upgrade
    private void ApplyUpgradeEffects(string upgradeName)
    {
        var upgrade = upgrades[upgradeName];
        
        switch (upgradeName)
        {
            case "WasteStorage":
                // Implement storage capacity increase
                break;
                
            case "RecyclingLab":
                // Implement recycling efficiency boost
                break;
                
            case "StabilizationChamber":
                // Implement contamination reduction
                break;
        }
    }
    
    // Get total benefits across all upgrades
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
}