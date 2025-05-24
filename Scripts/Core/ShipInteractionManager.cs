using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages interactions with discovered ships (combat, trading, etc.)
/// </summary>
public class ShipInteractionManager : MonoBehaviour
{
    // Singleton pattern
    public static ShipInteractionManager Instance { get; private set; }

    [Header("Trading Settings")]
    [SerializeField] private float baseTradeRatio = 2f; // How much waste per ship part
    [SerializeField] private float rarityTradeMultiplier = 1.5f;

    // Events
    public event Action<DiscoveredShip> OnCombatStarted;
    public event Action<DiscoveredShip, bool> OnCombatEnded; // ship, playerWon
    public event Action<DiscoveredShip, string[]> OnTradeCompleted; // ship, itemsReceived
    public event Action<DiscoveredShip> OnShipDestroyed;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("ShipInteractionManager: Initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Note: We don't subscribe to combat events since we're working with existing combat system
        // Instead, we'll check combat state in Update()
    }

    /// <summary>
    /// Start combat with a discovered ship
    /// </summary>
    public bool StartCombatWithShip(DiscoveredShip ship)
    {
        if (ship == null)
        {
            Debug.LogError("Cannot start combat: ship is null");
            return false;
        }

        if (!ship.CanFight)
        {
            Debug.LogWarning($"Cannot fight {ship.ShipName}: already fought or destroyed");
            return false;
        }

        if (CombatManager.Instance == null)
        {
            Debug.LogError("Cannot start combat: CombatManager not found");
            return false;
        }

        // Check if player can fight (has HP)
        if (CombatManager.Instance.currentHP <= 0)
        {
            Debug.LogWarning("Cannot start combat: player has no health");
            return false;
        }

        // Convert ship to EnemyShip
        EnemyShip enemyShip = ship.ToEnemyShip();

        // Store reference to the ship for later
        currentInteractionShip = ship;

        // Set the enemy in combat manager (replacing current enemy)
        CombatManager.Instance.currentEnemy = enemyShip;

        // Disable auto-combat for manual ship fights
        bool wasAutoCombat = CombatManager.Instance.autoCombatEnabled;
        CombatManager.Instance.autoCombatEnabled = false;

        // Store the previous auto-combat state to restore later
        previousAutoCombatState = wasAutoCombat;

        // Notify listeners
        OnCombatStarted?.Invoke(ship);

        Debug.Log($"Started combat with {ship.ShipName} - Auto-combat disabled for ship fight");
        return true;
    }

    /// <summary>
    /// Attempt to trade with a discovered ship
    /// </summary>
    public bool TryTradeWithShip(DiscoveredShip ship)
    {
        if (ship == null)
        {
            Debug.LogError("Cannot trade: ship is null");
            return false;
        }

        if (!ship.CanTrade)
        {
            Debug.LogWarning($"Cannot trade with {ship.ShipName}: already traded or destroyed");
            return false;
        }

        if (WasteInventoryManager.Instance == null)
        {
            Debug.LogError("Cannot trade: WasteInventoryManager not found");
            return false;
        }

        // Check if player has enough waste items
        var wasteItems = WasteInventoryManager.Instance.GetAllItems();
        int totalWasteValue = CalculateTotalWasteValue(wasteItems);

        if (totalWasteValue < ship.TradeValue)
        {
            Debug.LogWarning($"Insufficient waste for trade. Need {ship.TradeValue}, have {totalWasteValue}");
            return false;
        }

        // Perform the trade
        return ExecuteTrade(ship, wasteItems);
    }

    /// <summary>
    /// Calculate total value of waste items for trading
    /// </summary>
    private int CalculateTotalWasteValue(List<WasteItem> wasteItems)
    {
        int totalValue = 0;

        foreach (var waste in wasteItems)
        {
            // Base value from recycling value
            float itemValue = waste.RecyclingValue;

            // Bonus for rarity
            float rarityMultiplier = 1f + ((int)waste.Rarity * 0.2f);

            // Final value
            int finalValue = Mathf.RoundToInt(itemValue * rarityMultiplier);
            totalValue += finalValue * waste.Quantity;
        }

        return totalValue;
    }

    /// <summary>
    /// Execute the trade with a ship
    /// </summary>
    private bool ExecuteTrade(DiscoveredShip ship, List<WasteItem> wasteItems)
    {
        // Calculate what waste to consume
        List<WasteItem> wasteToRemove = new List<WasteItem>();
        int remainingTradeValue = ship.TradeValue;

        // Select waste items to trade (prioritize lower value items first)
        var sortedWaste = new List<WasteItem>(wasteItems);
        sortedWaste.Sort((a, b) => a.RecyclingValue.CompareTo(b.RecyclingValue));

        foreach (var waste in sortedWaste)
        {
            if (remainingTradeValue <= 0) break;

            float itemValue = waste.RecyclingValue * (1f + ((int)waste.Rarity * 0.2f));
            int itemTradeValue = Mathf.RoundToInt(itemValue);

            if (itemTradeValue <= remainingTradeValue)
            {
                wasteToRemove.Add(waste);
                remainingTradeValue -= itemTradeValue;
            }
        }

        // Remove waste items from inventory
        foreach (var waste in wasteToRemove)
        {
            WasteInventoryManager.Instance.RemoveWasteItem(waste);
        }

        // Give ship parts to player
        if (ResourceManager.Instance != null)
        {
            int shipPartsToGive = CalculateShipPartsReward(ship);
            ResourceManager.Instance.AddShipParts(shipPartsToGive);

            Debug.Log($"Trade completed! Gave {wasteToRemove.Count} waste items, received {shipPartsToGive} ship parts");
        }

        // Mark ship as traded
        ship.MarkAsTraded();

        // Notify listeners
        OnTradeCompleted?.Invoke(ship, ship.AvailableTradeItems);

        // Clear the ship from scanner
        if (ShipScanner.Instance != null)
        {
            ShipScanner.Instance.ClearCurrentShip();
        }

        return true;
    }

    /// <summary>
    /// Calculate ship parts reward from trading
    /// </summary>
    private int CalculateShipPartsReward(DiscoveredShip ship)
    {
        int baseReward = 1;

        // Bonus based on ship rarity
        switch (ship.Rarity)
        {
            case ShipRarity.VeryCommon:
                baseReward = 1;
                break;
            case ShipRarity.Common:
                baseReward = 2;
                break;
            case ShipRarity.SlightlyRare:
                baseReward = 3;
                break;
            case ShipRarity.Rare:
                baseReward = 5;
                break;
            case ShipRarity.Epic:
                baseReward = 8;
                break;
            case ShipRarity.Legendary:
                baseReward = 12;
                break;
            case ShipRarity.Anomaly:
                baseReward = 20;
                break;
        }

        // Add some randomness
        return UnityEngine.Random.Range(baseReward, baseReward * 2);
    }

    /// <summary>
    /// Get trading information for UI display
    /// </summary>
    public string GetTradeInfoText(DiscoveredShip ship)
    {
        if (ship == null || !ship.CanTrade)
            return "No trade available";

        int playerWasteValue = 0;
        if (WasteInventoryManager.Instance != null)
        {
            playerWasteValue = CalculateTotalWasteValue(WasteInventoryManager.Instance.GetAllItems());
        }

        int shipPartsReward = CalculateShipPartsReward(ship);

        string tradeInfo = $"TRADE OFFER:\n\n";
        tradeInfo += $"They Want: {ship.TradeValue} waste value\n";
        tradeInfo += $"You Have: {playerWasteValue} waste value\n\n";
        tradeInfo += $"They Offer: {shipPartsReward} Ship Parts\n";
        tradeInfo += $"Items: {ship.GetTradeItemsText()}\n\n";

        if (playerWasteValue >= ship.TradeValue)
        {
            tradeInfo += "✓ Trade Available";
        }
        else
        {
            tradeInfo += "✗ Insufficient Waste";
        }

        return tradeInfo;
    }

    /// <summary>
    /// Get combat information for UI display
    /// </summary>
    public string GetCombatInfoText(DiscoveredShip ship)
    {
        if (ship == null || !ship.CanFight)
            return "Cannot fight this ship";

        string combatInfo = $"COMBAT INFO:\n\n";
        combatInfo += $"Enemy: {ship.ShipName}\n";
        combatInfo += $"Type: {ship.ShipType}\n";
        combatInfo += $"Level: {ship.Level}\n";
        combatInfo += $"Rarity: {ship.GetRarityDisplayText()}\n\n";

        combatInfo += $"Enemy Stats:\n";
        combatInfo += $"HP: {ship.Health:F0}\n";
        combatInfo += $"Attack: {ship.AttackPower:F1}\n";
        combatInfo += $"Defense: {ship.Defense:F1}\n\n";

        if (CombatManager.Instance != null)
        {
            combatInfo += $"Your Stats:\n";
            combatInfo += $"HP: {CombatManager.Instance.currentHP:F0}/{CombatManager.Instance.maxHP:F0}\n";
            combatInfo += $"Attack: {CombatManager.Instance.attackPower:F1}\n";
            combatInfo += $"Defense: {CombatManager.Instance.defense:F1}\n\n";

            // Simple difficulty assessment
            float playerPower = CombatManager.Instance.attackPower + CombatManager.Instance.defense + CombatManager.Instance.currentHP;
            float enemyPower = ship.AttackPower + ship.Defense + ship.Health;

            if (playerPower > enemyPower * 1.2f)
                combatInfo += "Difficulty: Easy";
            else if (playerPower > enemyPower * 0.8f)
                combatInfo += "Difficulty: Moderate";
            else
                combatInfo += "Difficulty: Hard";
        }

        return combatInfo;
    }

    // Store current ship being interacted with for combat callbacks
    private DiscoveredShip currentInteractionShip;
    private bool previousAutoCombatState;

    /// <summary>
    /// Check if the current enemy is our discovered ship and handle victory/defeat
    /// </summary>
    private void Update()
    {
        // Check if we're tracking a ship combat and the enemy was defeated
        if (currentInteractionShip != null && CombatManager.Instance != null)
        {
            var currentEnemy = CombatManager.Instance.currentEnemy;

            // Check if our tracked enemy is defeated or replaced
            if (currentEnemy == null || currentEnemy.currentHP <= 0)
            {
                // Enemy was defeated - player won
                HandleCombatEnd(true);
            }
            else if (CombatManager.Instance.currentHP <= 0)
            {
                // Player was defeated
                HandleCombatEnd(false);
            }
        }
    }

    /// <summary>
    /// Handle end of combat with discovered ship
    /// </summary>
    private void HandleCombatEnd(bool playerWon)
    {
        if (currentInteractionShip == null) return;

        Debug.Log($"Ship combat ended. Player won: {playerWon}");

        // Mark ship based on outcome
        if (playerWon)
        {
            currentInteractionShip.MarkAsFought(true); // Ship defeated
            OnShipDestroyed?.Invoke(currentInteractionShip);

            // Clear the ship from scanner
            if (ShipScanner.Instance != null)
            {
                ShipScanner.Instance.ClearCurrentShip();
            }
        }
        else
        {
            currentInteractionShip.MarkAsFought(false); // Player defeated, ship survives
        }

        // Restore previous auto-combat state
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.autoCombatEnabled = previousAutoCombatState;
        }

        // Notify listeners
        OnCombatEnded?.Invoke(currentInteractionShip, playerWon);

        // Clear reference
        currentInteractionShip = null;
    }

    /// <summary>
    /// Check if player can start combat (has health)
    /// </summary>
    public bool CanStartCombat()
    {
        return CombatManager.Instance != null && CombatManager.Instance.currentHP > 0;
    }

    /// <summary>
    /// Check if player can afford to trade with a ship
    /// </summary>
    public bool CanAffordTrade(DiscoveredShip ship)
    {
        if (ship == null || !ship.CanTrade) return false;

        if (WasteInventoryManager.Instance == null) return false;

        var wasteItems = WasteInventoryManager.Instance.GetAllItems();
        int totalWasteValue = CalculateTotalWasteValue(wasteItems);

        return totalWasteValue >= ship.TradeValue;
    }

    /// <summary>
    /// Get available interaction options for a ship
    /// </summary>
    public string[] GetAvailableInteractions(DiscoveredShip ship)
    {
        if (ship == null) return new string[0];

        List<string> interactions = new List<string>();

        if (ship.CanFight && CanStartCombat())
        {
            interactions.Add("Fight");
        }

        if (ship.CanTrade && CanAffordTrade(ship))
        {
            interactions.Add("Trade");
        }

        if (interactions.Count == 0)
        {
            interactions.Add("No Actions Available");
        }

        return interactions.ToArray();
    }

    /// <summary>
    /// Force end any current interaction (for cleanup)
    /// </summary>
    public void ForceEndInteraction()
    {
        if (currentInteractionShip != null)
        {
            Debug.Log("Forcing end of ship interaction");
            currentInteractionShip = null;
        }

        // Note: Your existing CombatManager handles combat state automatically
        // No need to manually end combat as it's managed by the existing system
    }

    /// <summary>
    /// Get detailed ship information for display
    /// </summary>
    public string GetShipDetailsText(DiscoveredShip ship)
    {
        if (ship == null) return "No ship data";

        string details = $"{ship.ShipName}\n";
        details += $"{ship.ShipType}\n";
        details += $"Level {ship.Level} {ship.GetRarityDisplayText()}\n\n";

        details += $"Combat Stats:\n";
        details += $"Health: {ship.Health:F0}\n";
        details += $"Attack: {ship.AttackPower:F1}\n";
        details += $"Defense: {ship.Defense:F1}\n\n";

        if (ship.CanTrade)
        {
            details += $"Trade Value: {ship.TradeValue}\n";
            details += $"Offers: {ship.GetTradeItemsText()}\n\n";
        }

        details += $"Status: {ship.GetAvailableActionsText()}\n";
        details += $"Discovered: {ship.DiscoveryTime:HH:mm}";

        return details;
    }

    private void OnDestroy()
    {
        // No event subscriptions to clean up since we're using Update() method
    }
}