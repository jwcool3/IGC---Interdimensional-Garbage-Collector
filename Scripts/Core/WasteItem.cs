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
    
    // Gameplay properties
    public float WasteStability { get; set; }
    public float ContaminationLevel { get; set; }
    public float RecyclingValue { get; set; }
    public float RecyclingPotential { get; private set; }
    public int Quantity { get; private set; }

    // Visual properties
    public Color RarityColor => GetRarityColor();
    public Sprite Icon { get; set; }
    public Sprite ItemIcon => Icon; // Alias for compatibility

    public WasteItem()
    {
        Id = Guid.NewGuid().ToString();
        Quantity = 1;
        CalculateRecyclingPotential();
    }

    public WasteItem(string name, string dimensionalOrigin, WasteRarity rarity = WasteRarity.Common)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        DimensionalOrigin = dimensionalOrigin;
        Rarity = rarity;
        Quantity = 1;
        
        // Generate some random properties
        WasteStability = UnityEngine.Random.Range(0.1f, 1.0f);
        ContaminationLevel = UnityEngine.Random.Range(0.0f, 0.5f);
        
        // Calculate derived values
        CalculateRecyclingPotential();
        RecyclingValue = CalculateRecyclingValue();
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

    private void CalculateRecyclingPotential()
    {
        // Higher stability and rarity increase recycling potential
        float rarityBonus = ((int)Rarity + 1) * 0.2f;
        float stabilityFactor = WasteStability * (1 - ContaminationLevel);
        RecyclingPotential = Mathf.Clamp01(stabilityFactor * (1 + rarityBonus));
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