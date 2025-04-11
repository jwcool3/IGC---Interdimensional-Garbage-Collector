using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FacilityUpgrade
{
    public string Name { get; set; }
    public int MaxLevel { get; set; }
    public int CurrentLevel { get; set; }
    public float BaseRecyclingPointCost { get; set; }
    public float BaseDimensionalPotentialCost { get; set; }
    public float CostMultiplier { get; set; }

    public float CurrentRecyclingPointCost => BaseRecyclingPointCost * Mathf.Pow(CostMultiplier, CurrentLevel);
    public float CurrentDimensionalPotentialCost => BaseDimensionalPotentialCost * Mathf.Pow(CostMultiplier, CurrentLevel);

    public bool CanAfford(float currentRecyclingPoints, float currentDimensionalPotential)
    {
        if (CurrentLevel >= MaxLevel)
            return false;

        return currentRecyclingPoints >= CurrentRecyclingPointCost &&
               currentDimensionalPotential >= CurrentDimensionalPotentialCost;
    }

    public bool IsMaxLevel => CurrentLevel >= MaxLevel;

    // Benefits specific to this upgrade
    public Dictionary<string, float> CurrentBenefits = new Dictionary<string, float>();

    public FacilityUpgrade(string name, int maxLevel, float baseRPCost, float baseDPCost)
    {
        Name = name;
        MaxLevel = maxLevel;
        CurrentLevel = 0;
        BaseRecyclingPointCost = baseRPCost;
        BaseDimensionalPotentialCost = baseDPCost;

        UpdateBenefits();
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
        
        switch (Name)
        {
            case "WasteStorage":
                // Increase storage capacity
                CurrentBenefits["StorageCapacity"] = 50 + (CurrentLevel * 50); // 50, 100, 150, etc.
                break;
                
            case "RecyclingLab":
                // Increase recycling efficiency
                CurrentBenefits["RecyclingEfficiency"] = 1.0f + (CurrentLevel * 0.25f); // 1.0, 1.25, 1.5, etc.
                break;
                
            case "StabilizationChamber":
                // Reduce contamination
                CurrentBenefits["ContaminationReduction"] = 0.1f + (CurrentLevel * 0.1f); // 0.1, 0.2, 0.3, etc.
                break;
                
            case "ExpeditionCenter":
                // Increase waste generation quality
                CurrentBenefits["WasteQuality"] = 1.0f + (CurrentLevel * 0.2f); // 1.0, 1.2, 1.4, etc.
                break;
        }
    }
}