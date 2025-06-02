using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Data structure for ships discoverable through the Scanner system
/// This is separate from combat enemies and focuses on interaction/trading
/// Regular class (not ScriptableObject) for use with JSON import system
/// </summary>
[System.Serializable]
public class ScannerShipModel
{
    [Header("Basic Information")]
    public string shipName = "";
    public string shipClass = "";
    [TextArea(2, 4)]
    public string description = "";
    public ShipRarity rarity = ShipRarity.Common;
    
    [Header("Visual")]
    public Sprite shipIcon;
    public Color shipColor = Color.white;
    
    [Header("Availability")]
    public List<string> availableLocations = new List<string>();
    public float spawnWeight = 1f; // Higher = more likely to appear
    
    [Header("Trading Properties")]
    public int minTradeValue = 10;
    public int maxTradeValue = 25;
    public List<string> availableTradeGoods = new List<string>();
    
    [Header("Combat Properties")]
    public int minLevel = 1;
    public int maxLevel = 5;
    public float attackMultiplier = 1f;
    public float defenseMultiplier = 1f;
    public float healthMultiplier = 1f;
    
    [Header("Special Properties")]
    public bool canAlwaysTrade = true;
    public bool canAlwaysFight = true;
    public bool isUnique = false; // Can only be encountered once
    public List<string> specialAbilities = new List<string>();
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public ScannerShipModel()
    {
        // Initialize with default values
    }
    
    /// <summary>
    /// Get a random trade value within the ship's range
    /// </summary>
    public int GetRandomTradeValue()
    {
        return Random.Range(minTradeValue, maxTradeValue + 1);
    }
    
    /// <summary>
    /// Get a random level within the ship's range
    /// </summary>
    public int GetRandomLevel()
    {
        return Random.Range(minLevel, maxLevel + 1);
    }
    
    /// <summary>
    /// Get random trade goods from available options
    /// </summary>
    public string[] GetRandomTradeGoods(int count = -1)
    {
        if (availableTradeGoods.Count == 0)
        {
            return new string[] { "Basic Components" };
        }
        
        if (count <= 0)
        {
            // Determine count based on rarity
            count = GetTradeGoodsCountByRarity();
        }
        
        count = Mathf.Min(count, availableTradeGoods.Count);
        
        List<string> selectedGoods = new List<string>();
        List<string> availableGoods = new List<string>(availableTradeGoods);
        
        for (int i = 0; i < count; i++)
        {
            if (availableGoods.Count == 0) break;
            
            int randomIndex = Random.Range(0, availableGoods.Count);
            selectedGoods.Add(availableGoods[randomIndex]);
            availableGoods.RemoveAt(randomIndex);
        }
        
        return selectedGoods.ToArray();
    }
    
    /// <summary>
    /// Get appropriate number of trade goods based on ship rarity
    /// </summary>
    private int GetTradeGoodsCountByRarity()
    {
        switch (rarity)
        {
            case ShipRarity.VeryCommon:
                return 1;
            case ShipRarity.Common:
                return Random.Range(1, 3);
            case ShipRarity.SlightlyRare:
                return Random.Range(1, 3);
            case ShipRarity.Rare:
                return Random.Range(2, 4);
            case ShipRarity.Epic:
                return Random.Range(3, 5);
            case ShipRarity.Legendary:
                return Random.Range(4, 6);
            case ShipRarity.Anomaly:
                return Random.Range(3, 7);
            default:
                return 1;
        }
    }
    
    /// <summary>
    /// Check if this ship can appear in the specified location
    /// </summary>
    public bool IsAvailableInLocation(string location)
    {
        return availableLocations.Contains(location) || 
               availableLocations.Contains("All Locations") ||
               availableLocations.Count == 0;
    }
    
    /// <summary>
    /// Get ship color based on rarity if no custom color is set
    /// </summary>
    public Color GetDisplayColor()
    {
        if (shipColor != Color.white)
        {
            return shipColor;
        }
        
        // Return rarity-based color
        switch (rarity)
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
    /// Create a DiscoveredShip instance from this scanner ship model
    /// </summary>
    public DiscoveredShip CreateDiscoveredShip()
    {
        DiscoveredShip discoveredShip = new DiscoveredShip();
        
        // Basic information
        discoveredShip.ShipName = shipName;
        discoveredShip.ShipType = shipClass;
        discoveredShip.Rarity = rarity;
        discoveredShip.Level = GetRandomLevel();
        discoveredShip.DiscoveryTime = System.DateTime.Now;
        
        // Visual
        discoveredShip.ShipIcon = shipIcon;
        discoveredShip.ShipColor = GetDisplayColor();
        
        // Calculate combat stats based on level and multipliers
        float levelMultiplier = 1f + (discoveredShip.Level - 1) * 0.2f;
        
        discoveredShip.AttackPower = CalculateBaseStat(10f, levelMultiplier, attackMultiplier);
        discoveredShip.Defense = CalculateBaseStat(5f, levelMultiplier, defenseMultiplier);
        discoveredShip.Health = CalculateBaseStat(50f, levelMultiplier, healthMultiplier);
        discoveredShip.CurrentHealth = discoveredShip.Health;
        
        // Trading properties
        discoveredShip.TradeValue = GetRandomTradeValue();
        discoveredShip.AvailableTradeItems = GetRandomTradeGoods();
        
        // Set interaction availability
        discoveredShip.HasFought = false;
        discoveredShip.HasTraded = false;
        discoveredShip.WasDefeated = false;
        
        return discoveredShip;
    }
    
    /// <summary>
    /// Calculate a base stat with level and type multipliers
    /// </summary>
    private float CalculateBaseStat(float baseStat, float levelMultiplier, float typeMultiplier)
    {
        float rarityMultiplier = GetRarityStatMultiplier();
        float randomVariation = Random.Range(0.85f, 1.15f);
        
        return baseStat * levelMultiplier * typeMultiplier * rarityMultiplier * randomVariation;
    }
    
    /// <summary>
    /// Get stat multiplier based on ship rarity
    /// </summary>
    private float GetRarityStatMultiplier()
    {
        switch (rarity)
        {
            case ShipRarity.VeryCommon:
                return 0.8f;
            case ShipRarity.Common:
                return 1.0f;
            case ShipRarity.SlightlyRare:
                return 1.3f;
            case ShipRarity.Rare:
                return 1.7f;
            case ShipRarity.Epic:
                return 2.2f;
            case ShipRarity.Legendary:
                return 3.0f;
            case ShipRarity.Anomaly:
                return 2.5f;
            default:
                return 1.0f;
        }
    }
    
    /// <summary>
    /// Get a description of the ship's special properties
    /// </summary>
    public string GetSpecialPropertiesText()
    {
        List<string> properties = new List<string>();
        
        if (isUnique)
            properties.Add("Unique Encounter");
            
        if (!canAlwaysFight)
            properties.Add("Non-Hostile");
            
        if (!canAlwaysTrade)
            properties.Add("No Trading");
            
        foreach (string ability in specialAbilities)
        {
            properties.Add(ability);
        }
        
        return properties.Count > 0 ? string.Join(", ", properties) : "Standard Vessel";
    }
    
    /// <summary>
    /// Validate the ship model data
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(shipName)) return false;
        if (string.IsNullOrEmpty(shipClass)) return false;
        if (minTradeValue < 0) return false;
        if (maxTradeValue < minTradeValue) return false;
        if (minLevel < 1) return false;
        if (maxLevel < minLevel) return false;
        
        return true;
    }
}