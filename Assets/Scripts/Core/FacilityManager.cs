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
    // Removed unused event: public event Action OnUpgradesFetched;

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
        // Create initial upgrade options
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

    // Try to upgrade a specific facility section
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

    // Get all available upgrades
    public Dictionary<string, FacilityUpgrade> GetAvailableUpgrades()
    {
        return upgrades;
    }

    // Get a specific upgrade
    public FacilityUpgrade GetUpgrade(string name)
    {
        if (upgrades.ContainsKey(name))
            return upgrades[name];
        return null;
    }

    // Apply effects from an upgrade
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

                // Other upgrade effects can be added here
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