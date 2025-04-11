using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FacilityUpgrade
{
    public string UpgradeName { get; private set; }
    public string Description { get; private set; }
    public int CurrentLevel { get; private set; }
    public int MaxLevel { get; private set; }

    // Upgrade costs
    public float BaseDRPCost { get; private set; }
    public float BaseQuantumPotentialCost { get; private set; }
    public float CostMultiplierPerLevel { get; private set; }

    // Current level costs
    public float CurrentDRPCost => BaseDRPCost * Mathf.Pow(CostMultiplierPerLevel, CurrentLevel);
    public float CurrentQuantumPotentialCost => BaseQuantumPotentialCost * Mathf.Pow(CostMultiplierPerLevel, CurrentLevel);

    // Level-specific benefits
    private Dictionary<string, float> currentBenefits;
    public IReadOnlyDictionary<string, float> CurrentBenefits => currentBenefits;

    public FacilityUpgrade(string name, string description, int maxLevel, float baseDRPCost, float baseQuantumCost, float costMultiplier)
    {
        UpgradeName = name;
        Description = description;
        MaxLevel = maxLevel;
        CurrentLevel = 0;
        BaseDRPCost = baseDRPCost;
        BaseQuantumPotentialCost = baseQuantumCost;
        CostMultiplierPerLevel = costMultiplier;
        currentBenefits = new Dictionary<string, float>();
    }

    public bool CanUpgrade(float availableDRP, float availableQuantumPotential)
    {
        return CurrentLevel < MaxLevel &&
               availableDRP >= CurrentDRPCost &&
               availableQuantumPotential >= CurrentQuantumPotentialCost;
    }

    public bool TryUpgrade(ref float currentDRP, ref float currentQuantumPotential)
    {
        if (!CanUpgrade(currentDRP, currentQuantumPotential))
            return false;

        // Apply costs
        currentDRP -= CurrentDRPCost;
        currentQuantumPotential -= CurrentQuantumPotentialCost;
        
        // Increase level
        CurrentLevel++;
        
        // Update benefits
        UpdateBenefits();
        
        return true;
    }

    protected virtual void UpdateBenefits()
    {
        // Base implementation - override in specific upgrade types
        currentBenefits.Clear();
        
        // Example benefit calculation
        float baseMultiplier = 1 + (CurrentLevel * 0.25f);
        currentBenefits["EfficiencyMultiplier"] = baseMultiplier;
    }

    public string GetUpgradeDescription()
    {
        if (CurrentLevel >= MaxLevel)
            return "Maximum level reached";

        return $"Level {CurrentLevel + 1}/{MaxLevel}\n" +
               $"Cost: {CurrentDRPCost} DRP, {CurrentQuantumPotentialCost} Quantum Potential\n" +
               $"Current Benefits: {GetBenefitsDescription()}";
    }

    private string GetBenefitsDescription()
    {
        string benefits = "";
        foreach (var benefit in currentBenefits)
        {
            benefits += $"\n- {benefit.Key}: {benefit.Value:F2}";
        }
        return benefits;
    }
} 