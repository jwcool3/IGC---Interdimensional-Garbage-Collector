using System;
using UnityEngine;

/// <summary>
/// Ship rarity levels
/// </summary>
public enum ShipRarity
{
    // Common Category (0-2)
    VeryCommon = 0,
    Common = 1,
    SlightlyRare = 2,

    // Rare Category (3-6)
    Rare = 3,
    Epic = 4,
    Legendary = 5,
    Anomaly = 6
}

/// <summary>
/// Represents a ship discovered through scanning
/// </summary>
[System.Serializable]
public class DiscoveredShip
{
    [Header("Basic Info")]
    public string ShipName;
    public string ShipType;
    public ShipRarity Rarity;
    public int Level;
    public DateTime DiscoveryTime;

    [Header("Combat Stats")]
    public float AttackPower;
    public float Defense;
    public float Health;
    public float CurrentHealth;

    [Header("Visual")]
    public Sprite ShipIcon;
    public Color ShipColor;

    [Header("Trading")]
    public int TradeValue; // How much waste they want
    public string[] AvailableTradeItems; // What they offer
    public bool HasTraded; // Track if already traded

    [Header("Combat")]
    public bool HasFought; // Track if already fought
    public bool WasDefeated; // Track combat outcome

    [Header("Interaction State")]
    public bool CanFight => !HasFought && !WasDefeated;
    public bool CanTrade => !HasTraded && !WasDefeated;
    public bool IsDestroyed => WasDefeated;

    /// <summary>
    /// Constructor to initialize health
    /// </summary>
    public DiscoveredShip()
    {
        CurrentHealth = Health;
    }

    /// <summary>
    /// Get color associated with rarity
    /// </summary>
    public Color GetRarityColor()
    {
        switch (Rarity)
        {
            case ShipRarity.VeryCommon:
                return new Color(0.6f, 0.6f, 0.6f); // Light Gray
            case ShipRarity.Common:
                return new Color(0.8f, 0.8f, 0.8f); // Gray
            case ShipRarity.SlightlyRare:
                return new Color(0.4f, 0.8f, 0.4f); // Light Green
            case ShipRarity.Rare:
                return new Color(0.4f, 0.4f, 1f); // Blue
            case ShipRarity.Epic:
                return new Color(0.8f, 0.4f, 1f); // Purple
            case ShipRarity.Legendary:
                return new Color(1f, 0.8f, 0.2f); // Gold
            case ShipRarity.Anomaly:
                return new Color(1f, 0.2f, 0.2f); // Red
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Get display text for rarity
    /// </summary>
    public string GetRarityDisplayText()
    {
        switch (Rarity)
        {
            case ShipRarity.VeryCommon:
                return "Very Common";
            case ShipRarity.Common:
                return "Common";
            case ShipRarity.SlightlyRare:
                return "Slightly Rare";
            case ShipRarity.Rare:
                return "Rare";
            case ShipRarity.Epic:
                return "Epic";
            case ShipRarity.Legendary:
                return "Legendary";
            case ShipRarity.Anomaly:
                return "Anomaly";
            default:
                return "Unknown";
        }
    }

    /// <summary>
    /// Check if this ship is in the common category
    /// </summary>
    public bool IsCommonCategory()
    {
        return (int)Rarity <= (int)ShipRarity.SlightlyRare;
    }

    /// <summary>
    /// Check if this ship is in the rare category
    /// </summary>
    public bool IsRareCategory()
    {
        return (int)Rarity >= (int)ShipRarity.Rare;
    }

    /// <summary>
    /// Get a description of available actions
    /// </summary>
    public string GetAvailableActionsText()
    {
        if (IsDestroyed)
            return "Ship Destroyed";

        string actions = "";

        if (CanFight)
            actions += "Fight ";
        if (CanTrade)
            actions += "Trade ";

        if (string.IsNullOrEmpty(actions))
            actions = "No Actions Available";

        return actions.Trim();
    }

    /// <summary>
    /// Get trade items as formatted string
    /// </summary>
    public string GetTradeItemsText()
    {
        if (AvailableTradeItems == null || AvailableTradeItems.Length == 0)
            return "No items available";

        return string.Join(", ", AvailableTradeItems);
    }

    /// <summary>
    /// Convert to EnemyShip for combat system
    /// </summary>
    public EnemyShip ToEnemyShip()
    {
        EnemyType enemyType = ConvertToEnemyType();

        // Create enemy ship using your existing constructor
        EnemyShip enemyShip = new EnemyShip(ShipName, ShipType, Level, enemyType, 1);

        // Override stats with our discovered ship's stats
        enemyShip.maxHP = Health;
        enemyShip.currentHP = CurrentHealth;
        enemyShip.attackPower = AttackPower;
        enemyShip.defense = Defense;

        return enemyShip;
    }

    /// <summary>
    /// Convert ship rarity to enemy type for combat
    /// </summary>
    private EnemyType ConvertToEnemyType()
    {
        switch (Rarity)
        {
            case ShipRarity.VeryCommon:
            case ShipRarity.Common:
                return EnemyType.Scavenger;
            case ShipRarity.SlightlyRare:
            case ShipRarity.Rare:
                return EnemyType.Rival;
            case ShipRarity.Epic:
            case ShipRarity.Legendary:
                return EnemyType.Boss;
            case ShipRarity.Anomaly:
                return EnemyType.Anomaly;
            default:
                return EnemyType.Scavenger;
        }
    }

    /// <summary>
    /// Mark ship as fought
    /// </summary>
    public void MarkAsFought(bool wasDefeated = false)
    {
        HasFought = true;
        WasDefeated = wasDefeated;

        if (wasDefeated)
        {
            CurrentHealth = 0;
        }
    }

    /// <summary>
    /// Mark ship as traded with
    /// </summary>
    public void MarkAsTraded()
    {
        HasTraded = true;
    }

    /// <summary>
    /// Reset interaction state (for testing or special cases)
    /// </summary>
    public void ResetInteractionState()
    {
        HasFought = false;
        HasTraded = false;
        WasDefeated = false;
        CurrentHealth = Health;
    }
}