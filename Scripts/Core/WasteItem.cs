using System;
using UnityEngine;

public enum WasteRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[Serializable]
public class WasteItem
{
    // Core properties
    public string Id { get; private set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public WasteRarity Rarity { get; set; }
    public string DimensionalOrigin { get; set; }
    public int Quantity { get; set; } = 1;
    public Sprite ItemIcon { get; set; }

    // Gameplay properties
    public float WasteStability { get; set; }
    public float ContaminationLevel { get; set; }
    public float RecyclingValue { get; set; }
    public float RecyclingPotential { get; set; }

    // Visual properties
    public Color RarityColor => GetRarityColor();

    public WasteItem()
    {
        Id = Guid.NewGuid().ToString();
        CalculateRecyclingPotential();
    }

    public WasteItem(string name, string dimensionalOrigin, WasteRarity rarity = WasteRarity.Common)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        DimensionalOrigin = dimensionalOrigin;
        Rarity = rarity;
        
        // Calculate initial properties
        WasteStability = CalculateInitialStability();
        ContaminationLevel = CalculateInitialContamination();
        RecyclingPotential = CalculateRecyclingPotential();
    }

    public void SetQuantity(int newQuantity)
    {
        Quantity = Mathf.Max(0, newQuantity);
    }

    public void AddQuantity(int amount = 1)
    {
        Quantity = Mathf.Max(0, Quantity + amount);
    }

    public bool RemoveQuantity(int amount = 1)
    {
        if (Quantity >= amount)
        {
            Quantity -= amount;
            return true;
        }
        return false;
    }

    private float CalculateInitialStability()
    {
        // Base calculation based on rarity
        float baseStability = UnityEngine.Random.Range(0.3f, 1f);
        float rarityBonus = ((int)Rarity + 1) * 0.1f;
        return Mathf.Clamp01(baseStability + rarityBonus);
    }

    private float CalculateInitialContamination()
    {
        // More rare items have lower contamination
        float baseContamination = UnityEngine.Random.Range(0f, 0.5f);
        float rarityReduction = ((int)Rarity + 1) * 0.05f;
        return Mathf.Clamp01(baseContamination - rarityReduction);
    }

    private float CalculateRecyclingPotential()
    {
        // Higher rarity and stability increase recycling potential
        float basePotential = UnityEngine.Random.Range(0.2f, 0.8f);
        float rarityBonus = ((int)Rarity + 1) * 0.08f;
        float stabilityBonus = WasteStability * 0.2f;
        return Mathf.Clamp01(basePotential + rarityBonus + stabilityBonus);
    }

    private float CalculateRecyclingValue()
    {
        // Base value modified by rarity, stability, and recycling potential
        float baseValue = 10f * (1 + (int)Rarity);
        return baseValue * RecyclingPotential * (1 - ContaminationLevel);
    }

    public Color GetRarityColor()
    {
        switch (Rarity)
        {
            case WasteRarity.Common:
                return Color.gray;
            case WasteRarity.Uncommon:
                return Color.green;
            case WasteRarity.Rare:
                return Color.blue;
            case WasteRarity.Epic:
                return new Color(0.5f, 0f, 0.5f); // Purple
            case WasteRarity.Legendary:
                return Color.yellow;
            default:
                return Color.white;
        }
    }

    public WasteData GetWasteData()
    {
        return new WasteData
        {
            Id = this.Id,
            Name = this.Name,
            Description = this.Description,
            Rarity = this.Rarity,
            DimensionalOrigin = this.DimensionalOrigin,
            WasteStability = this.WasteStability,
            ContaminationLevel = this.ContaminationLevel,
            RecyclingValue = this.RecyclingValue,
            RecyclingPotential = this.RecyclingPotential,
            Quantity = this.Quantity
        };
    }
} 