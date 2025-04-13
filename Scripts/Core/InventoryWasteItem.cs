using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a waste item with additional inventory-specific properties
/// </summary>
[Serializable]
public class InventoryWasteItem
{
    // Unique identifier for the inventory item
    public string InventoryId { get; private set; }

    // Reference to the original WasteItem
    public WasteItem WasteData { get; private set; }

    // Icon for the waste item
    public Sprite ItemIcon { get; private set; }

    // Quantity of this specific waste type in inventory
    public int Quantity { get; private set; }

    // Rarity of the waste item
    public WasteRarity Rarity { get; private set; }

    // Additional inventory-specific metadata
    public DateTime AcquisitionTime { get; private set; }

    // Enum for waste item rarity
    public enum WasteRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    // Constructor
    public InventoryWasteItem(WasteItem wasteData, Sprite icon, int quantity = 1)
    {
        InventoryId = Guid.NewGuid().ToString();
        WasteData = wasteData;
        ItemIcon = icon;
        Quantity = quantity;
        AcquisitionTime = DateTime.Now;

        // Determine rarity based on recycling potential
        DetermineRarity();
    }

    // Increase quantity of this waste item
    public void AddQuantity(int amount = 1)
    {
        Quantity += amount;
    }

    // Decrease quantity of this waste item
    public bool RemoveQuantity(int amount = 1)
    {
        if (Quantity >= amount)
        {
            Quantity -= amount;
            return true;
        }
        return false;
    }

    // Determine rarity based on waste properties
    private void DetermineRarity()
    {
        float recyclingPotential = WasteData.RecyclingPotential;

        if (recyclingPotential >= 0.9f)
            Rarity = WasteRarity.Legendary;
        else if (recyclingPotential >= 0.7f)
            Rarity = WasteRarity.Epic;
        else if (recyclingPotential >= 0.5f)
            Rarity = WasteRarity.Rare;
        else if (recyclingPotential >= 0.3f)
            Rarity = WasteRarity.Uncommon;
        else
            Rarity = WasteRarity.Common;
    }

    // Get color based on rarity
    public Color GetRarityColor()
    {
        return Rarity switch
        {
            WasteRarity.Legendary => new Color(1f, 0.84f, 0f),    // Gold
            WasteRarity.Epic => new Color(0.57f, 0.13f, 0.81f),   // Purple
            WasteRarity.Rare => new Color(0f, 0.5f, 1f),          // Blue
            WasteRarity.Uncommon => new Color(0f, 1f, 0f),        // Green
            _ => new Color(0.5f, 0.5f, 0.5f)                      // Gray for Common
        };
    }
}
