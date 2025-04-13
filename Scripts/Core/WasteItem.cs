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
    public Sprite Icon { get; set; }

    // Gameplay properties
    public float WasteStability { get; set; }
    public float ContaminationLevel { get; set; }
    public float RecyclingValue { get; set; }
    public float RecyclingPotential { get; set; }

    // Visual properties
    public Color RarityColor => GetRarityColor();

    private bool propertiesInitialized = false;

    public WasteItem()
    {
        Id = Guid.NewGuid().ToString();
    }

    public WasteItem(string name, string dimensionalOrigin, WasteRarity rarity = WasteRarity.Common, Sprite icon = null)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        DimensionalOrigin = dimensionalOrigin;
        Rarity = rarity;
        Icon = icon;
        
        InitializeProperties();
    }

    public void InitializeProperties()
    {
        if (!propertiesInitialized)
        {
            WasteStability = CalculateInitialStability();
            ContaminationLevel = CalculateInitialContamination();
            RecyclingPotential = CalculateRecyclingPotential();
            RecyclingValue = CalculateRecyclingValue();
            propertiesInitialized = true;
        }
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

    public void SetIcon(Sprite newIcon)
    {
        Icon = newIcon;
    }

    private float CalculateInitialStability()
    {
        float baseStability = 0.5f + ((int)Rarity * 0.1f);
        if (Application.isPlaying)
        {
            baseStability += UnityEngine.Random.Range(-0.1f, 0.1f);
        }
        return Mathf.Clamp01(baseStability);
    }

    private float CalculateInitialContamination()
    {
        float baseContamination = 0.5f - ((int)Rarity * 0.1f);
        if (Application.isPlaying)
        {
            baseContamination += UnityEngine.Random.Range(-0.1f, 0.1f);
        }
        return Mathf.Clamp01(baseContamination);
    }

    private float CalculateRecyclingPotential()
    {
        float basePotential = 0.3f + ((int)Rarity * 0.15f);
        if (Application.isPlaying)
        {
            basePotential += UnityEngine.Random.Range(-0.1f, 0.1f);
        }
        return Mathf.Clamp01(basePotential);
    }

    private float CalculateRecyclingValue()
    {
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
        InitializeProperties(); // Ensure properties are initialized before getting data
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