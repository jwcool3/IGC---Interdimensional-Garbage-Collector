using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FacilityUpgrade
{
    public string UpgradeName;
    public string Description;
    public int CurrentLevel;
    public int MaxLevel;

    // Cost increases with each level
    public float BaseRecyclingPointCost;
    public float BaseDimensionalPotentialCost;
    public float CostMultiplier = 1.5f; // Each level costs 1.5x more

    // Benefits specific to this upgrade
    public Dictionary<string, float> CurrentBenefits = new Dictionary<string, float>();

    // Current costs based on level
    public float CurrentRecyclingPointCost => BaseRecyclingPointCost * Mathf.Pow(CostMultiplier, CurrentLevel);
    public float CurrentDimensionalPotentialCost => BaseDimensionalPotentialCost * Mathf.Pow(CostMultiplier, CurrentLevel);

    // Constructor that takes 5 arguments as expected by FacilityManager
    public FacilityUpgrade(string name, string description, int maxLevel, float baseRPCost, float baseDPCost)
    {
        UpgradeName = name;
        Description = description;
        MaxLevel = maxLevel;
        CurrentLevel = 0;
        BaseRecyclingPointCost = baseRPCost;
        BaseDimensionalPotentialCost = baseDPCost;

        UpdateBenefits();
    }

    public bool CanAfford(float currentRP, float currentDP)
    {
        return CurrentLevel < MaxLevel &&
               currentRP >= CurrentRecyclingPointCost &&
               currentDP >= CurrentDimensionalPotentialCost;
    }

    public bool TryUpgrade(ref float currentRP, ref float currentDP)
    {
        if (!CanAfford(currentRP, currentDP))
            return false;

        // Apply costs
        currentRP -= CurrentRecyclingPointCost;
        currentDP -= CurrentDimensionalPotentialCost;

        // Increase level
        CurrentLevel++;

        // Update benefits
        UpdateBenefits();

        return true;
    }

    // Calculate current benefits based on level
    private void UpdateBenefits()
    {
        CurrentBenefits.Clear();

        switch (UpgradeName)
        {
            case "Waste Storage Wing":
                // Increase storage capacity
                CurrentBenefits["StorageCapacity"] = 50 + (CurrentLevel * 50); // 50, 100, 150, etc.
                break;

            case "Recycling Laboratory":
                // Increase recycling efficiency
                CurrentBenefits["RecyclingEfficiency"] = 1.0f + (CurrentLevel * 0.25f); // 1.0, 1.25, 1.5, etc.
                break;

            case "Dimensional Stabilization":
                // Reduce contamination
                CurrentBenefits["ContaminationReduction"] = 0.1f + (CurrentLevel * 0.1f); // 0.1, 0.2, 0.3, etc.
                break;

            case "Expedition Center":
                // Increase waste generation quality
                CurrentBenefits["WasteQuality"] = 1.0f + (CurrentLevel * 0.2f); // 1.0, 1.2, 1.4, etc.
                break;
        }
    }
}