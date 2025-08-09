using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages interactions with discovered ships (fighting and trading)
/// Updated to integrate with CombatUI for discovered ship image display
/// </summary>
public class ShipInteractionManager : MonoBehaviour
{
    // Singleton pattern
    public static ShipInteractionManager Instance { get; private set; }

    [Header("Combat Settings")]
    [SerializeField] private bool autoSwitchToCombatTab = true;
    [SerializeField] private bool returnToContactsAfterCombat = true;

    [Header("Trade Settings")]
    [SerializeField] private int baseTradeReward = 10; // Base ship parts reward

    // Track the ship we're currently fighting
    private DiscoveredShip currentFightingShip;
    private bool subscribedToCombatEvents = false;

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

    /// <summary>
    /// Start combat with a discovered ship
    /// </summary>
    public bool StartCombatWithShip(DiscoveredShip ship)
    {
        if (ship == null || !ship.CanFight)
        {
            Debug.LogWarning("ShipInteractionManager: Cannot start combat with this ship");
            return false;
        }

        if (CombatManager.Instance == null)
        {
            Debug.LogError("ShipInteractionManager: CombatManager not found!");
            return false;
        }

        Debug.Log($"ShipInteractionManager: Starting combat with {ship.ShipName}");

        // Convert discovered ship to enemy ship for combat system
        EnemyShip enemyShip = ship.ToEnemyShip();

        // Store reference to the discovered ship for post-combat processing
        currentFightingShip = ship;

        // Set up single ship combat mode
        SetupSingleShipCombat(enemyShip);

        // IMPORTANT: Tell CombatUI about the discovered ship so it can use the correct image
        if (CombatUI.Instance != null)
        {
            CombatUI.Instance.SetCurrentDiscoveredShip(ship);
            Debug.Log($"ShipInteractionManager: Notified CombatUI about discovered ship {ship.ShipName}");
        }
        else
        {
            Debug.LogWarning("ShipInteractionManager: CombatUI instance not found!");
        }

        // Switch to combat tab
        if (autoSwitchToCombatTab)
        {
            TabSystem tabSystem = FindObjectOfType<TabSystem>();
            if (tabSystem != null)
            {
                tabSystem.ShowCombatTab();
            }
        }

        Debug.Log($"ShipInteractionManager: Combat started with {ship.ShipName}");
        return true;
    }

    /// <summary>
    /// Set up combat manager for single ship combat
    /// </summary>
    private void SetupSingleShipCombat(EnemyShip enemyShip)
    {
        // Use the new CombatManager method for single ship combat
        CombatManager.Instance.StartSingleShipCombat(enemyShip, $"Encounter: {enemyShip.name}");

        // Subscribe to combat events to handle victory/defeat
        SubscribeToCombatEvents();

        Debug.Log($"ShipInteractionManager: Single ship combat setup complete for {enemyShip.name}");
    }

    /// <summary>
    /// Attempt to trade with a discovered ship
    /// </summary>
    public bool TryTradeWithShip(DiscoveredShip ship)
    {
        if (ship == null || !ship.CanTrade)
        {
            Debug.LogWarning("ShipInteractionManager: Cannot trade with this ship");
            return false;
        }

        if (WasteInventoryManager.Instance == null)
        {
            Debug.LogError("ShipInteractionManager: WasteInventoryManager not found!");
            return false;
        }

        if (ResourceManager.Instance == null)
        {
            Debug.LogError("ShipInteractionManager: ResourceManager not found!");
            return false;
        }

        Debug.Log($"ShipInteractionManager: Attempting trade with {ship.ShipName}");

        // Check if player has enough waste items
        int availableWaste = WasteInventoryManager.Instance.GetInventoryCount();
        if (availableWaste < ship.TradeValue)
        {
            Debug.LogWarning($"ShipInteractionManager: Not enough waste items! Need {ship.TradeValue}, have {availableWaste}");
            return false;
        }

        // Perform the trade
        bool success = PerformTrade(ship);

        if (success)
        {
            // Mark ship as traded
            ship.MarkAsTraded();
            Debug.Log($"ShipInteractionManager: Trade completed with {ship.ShipName}");
        }

        return success;
    }

    /// <summary>
    /// Perform the actual trade transaction
    /// </summary>
    private bool PerformTrade(DiscoveredShip ship)
    {
        // Remove required waste items from inventory
        List<UpdatedWasteItem> allWaste = WasteInventoryManager.Instance.GetAllItems();
        int wasteRemoved = 0;
        int wasteNeeded = ship.TradeValue;

        // Remove waste items until we have enough
        for (int i = allWaste.Count - 1; i >= 0 && wasteRemoved < wasteNeeded; i--)
        {
            UpdatedWasteItem item = allWaste[i];
            int itemQuantity = item.Quantity;
            int toRemove = Mathf.Min(itemQuantity, wasteNeeded - wasteRemoved);

            if (toRemove >= itemQuantity)
            {
                // Remove entire stack
                WasteInventoryManager.Instance.RemoveWasteItem(item);
                wasteRemoved += itemQuantity;
            }
            else
            {
                // Remove partial stack
                WasteInventoryManager.Instance.RemoveQuantity(item.Id, toRemove);
                wasteRemoved += toRemove;
            }
        }

        if (wasteRemoved < wasteNeeded)
        {
            Debug.LogError($"ShipInteractionManager: Failed to remove enough waste! Removed {wasteRemoved}, needed {wasteNeeded}");
            return false;
        }

        // Give rewards
        GiveTradeRewards(ship);

        Debug.Log($"ShipInteractionManager: Trade successful - removed {wasteRemoved} waste items");
        return true;
    }

    /// <summary>
    /// Give rewards for completing a trade
    /// </summary>
    private void GiveTradeRewards(DiscoveredShip ship)
    {
        // Calculate rewards based on ship rarity and level
        int shipPartsReward = CalculateShipPartsReward(ship);
        int alienTechReward = CalculateAlienTechReward(ship);
        float recyclingPointsReward = CalculateRecyclingPointsReward(ship);

        // Give rewards through ResourceManager
        ResourceManager.Instance.AddShipParts(shipPartsReward);
        ResourceManager.Instance.AddAlienTech(alienTechReward);
        ResourceManager.Instance.AddRecyclingPoints(recyclingPointsReward);

        Debug.Log($"ShipInteractionManager: Trade rewards - " +
                 $"Ship Parts: {shipPartsReward}, " +
                 $"Alien Tech: {alienTechReward}, " +
                 $"RP: {recyclingPointsReward}");
    }

    /// <summary>
    /// Calculate ship parts reward based on ship properties
    /// </summary>
    private int CalculateShipPartsReward(DiscoveredShip ship)
    {
        int baseReward = baseTradeReward;
        int rarityMultiplier = (int)ship.Rarity + 1;
        int levelBonus = ship.Level;

        return baseReward * rarityMultiplier + levelBonus;
    }

    /// <summary>
    /// Calculate alien tech reward based on ship properties
    /// </summary>
    private int CalculateAlienTechReward(DiscoveredShip ship)
    {
        // Alien tech is rarer, so smaller amounts
        int baseReward = baseTradeReward / 3;
        int rarityMultiplier = (int)ship.Rarity + 1;

        // Only give alien tech for rare category ships
        if (ship.IsRareCategory())
        {
            return baseReward * rarityMultiplier;
        }

        return 0;
    }

    /// <summary>
    /// Calculate recycling points reward based on ship properties
    /// </summary>
    private float CalculateRecyclingPointsReward(DiscoveredShip ship)
    {
        float baseReward = baseTradeReward * 5f; // More generous with RP
        float rarityMultiplier = (int)ship.Rarity + 1;
        float levelBonus = ship.Level * 2f;

        return baseReward * rarityMultiplier + levelBonus;
    }

    /// <summary>
    /// Get combat information text for UI display
    /// </summary>
    public string GetCombatInfoText(DiscoveredShip ship)
    {
        if (ship == null) return "No ship selected";

        if (ship.IsDestroyed)
            return "Ship has been destroyed";

        if (ship.HasFought)
            return "Already fought this ship";

        if (!ship.CanFight)
            return "Cannot fight this ship";

        return $"Combat Stats:\n" +
               $"Health: {ship.CurrentHealth:F0}/{ship.Health:F0}\n" +
               $"Attack: {ship.AttackPower:F0}\n" +
               $"Defense: {ship.Defense:F0}\n" +
               $"Level: {ship.Level}";
    }

    /// <summary>
    /// Get trade information text for UI display
    /// </summary>
    public string GetTradeInfoText(DiscoveredShip ship)
    {
        if (ship == null) return "No ship selected";

        if (ship.IsDestroyed)
            return "Ship has been destroyed";

        if (ship.HasTraded)
            return "Already traded with this ship";

        if (!ship.CanTrade)
            return "Cannot trade with this ship";

        int availableWaste = WasteInventoryManager.Instance?.GetInventoryCount() ?? 0;
        bool canAffordTrade = availableWaste >= ship.TradeValue;

        string affordText = canAffordTrade ? "✓ Can Afford" : "✗ Cannot Afford";

        return $"Trade Requirements:\n" +
               $"Wants: {ship.TradeValue} waste items\n" +
               $"You have: {availableWaste} items\n" +
               $"Status: {affordText}\n\n" +
               $"Offers: {ship.GetTradeItemsText()}";
    }

    /// <summary>
    /// Subscribe to combat events to handle post-combat processing
    /// </summary>
    private void SubscribeToCombatEvents()
    {
        if (subscribedToCombatEvents) return;

        // We'll create custom events in CombatManager for this
        // For now, we'll use a coroutine to check combat status
        StartCoroutine(MonitorCombat());
        subscribedToCombatEvents = true;
    }

    /// <summary>
    /// Monitor combat progress and handle completion
    /// </summary>
    private System.Collections.IEnumerator MonitorCombat()
    {
        while (currentFightingShip != null && CombatManager.Instance != null)
        {
            // Wait a frame
            yield return null;

            // Check if the enemy is defeated
            if (CombatManager.Instance.currentEnemy == null ||
                CombatManager.Instance.currentEnemy.currentHP <= 0)
            {
                // Player won - mark ship as defeated
                HandleCombatVictory();
                break;
            }

            // Check if player is defeated
            if (CombatManager.Instance.currentHP <= 0)
            {
                // Player lost - ship survives but is marked as fought
                HandleCombatDefeat();
                break;
            }
        }

        subscribedToCombatEvents = false;
    }

    /// <summary>
    /// Handle combat victory
    /// </summary>
    private void HandleCombatVictory()
    {
        if (currentFightingShip == null) return;

        Debug.Log($"ShipInteractionManager: Player defeated {currentFightingShip.ShipName}!");

        // Mark ship as fought and defeated
        currentFightingShip.MarkAsFought(true);

        // Give additional rewards for defeating a discovered ship
        GiveCombatVictoryRewards(currentFightingShip);

        // Clear the current ship from scanner if it's the same one
        if (ShipScanner.Instance != null &&
            ShipScanner.Instance.CurrentShip == currentFightingShip)
        {
            ShipScanner.Instance.ClearCurrentShip();
        }

        // Clear the discovered ship reference from CombatUI
        if (CombatUI.Instance != null)
        {
            CombatUI.Instance.ClearCurrentDiscoveredShip();
            Debug.Log("ShipInteractionManager: Cleared discovered ship from CombatUI after victory");
        }

        // End single ship combat mode
        CombatManager.Instance.EndSingleShipCombat();

        // Return to contacts tab if enabled
        if (returnToContactsAfterCombat)
        {
            StartCoroutine(DelayedTabSwitch("Contacts", 2f));
        }

        // Clear current fighting ship
        currentFightingShip = null;
    }

    /// <summary>
    /// Handle combat defeat
    /// </summary>
    private void HandleCombatDefeat()
    {
        if (currentFightingShip == null) return;

        Debug.Log($"ShipInteractionManager: Player was defeated by {currentFightingShip.ShipName}!");

        // Mark ship as fought but not defeated
        currentFightingShip.MarkAsFought(false);

        // Ship remains available for future interactions (if it had trade options)
        // The player can try again later or choose to trade instead

        // Clear the discovered ship reference from CombatUI
        if (CombatUI.Instance != null)
        {
            CombatUI.Instance.ClearCurrentDiscoveredShip();
            Debug.Log("ShipInteractionManager: Cleared discovered ship from CombatUI after defeat");
        }

        // End single ship combat mode
        CombatManager.Instance.EndSingleShipCombat();

        // Return to contacts tab if enabled
        if (returnToContactsAfterCombat)
        {
            StartCoroutine(DelayedTabSwitch("Contacts", 3f));
        }

        // Clear current fighting ship
        currentFightingShip = null;
    }

    /// <summary>
    /// Give additional rewards for defeating a discovered ship
    /// </summary>
    private void GiveCombatVictoryRewards(DiscoveredShip ship)
    {
        // Calculate bonus rewards for defeating discovered ships
        int bonusShipParts = CalculateCombatShipPartsReward(ship);
        int bonusAlienTech = CalculateCombatAlienTechReward(ship);
        float bonusRP = CalculateCombatRPReward(ship);
        float bonusDP = CalculateCombatDPReward(ship);

        // Give rewards through ResourceManager
        ResourceManager.Instance.AddShipParts(bonusShipParts);
        ResourceManager.Instance.AddAlienTech(bonusAlienTech);
        ResourceManager.Instance.AddRecyclingPoints(bonusRP);
        ResourceManager.Instance.AddDimensionalPotential(bonusDP);

        Debug.Log($"ShipInteractionManager: Combat victory rewards - " +
                 $"Ship Parts: {bonusShipParts}, " +
                 $"Alien Tech: {bonusAlienTech}, " +
                 $"RP: {bonusRP}, " +
                 $"DP: {bonusDP}");
    }

    /// <summary>
    /// Calculate ship parts reward for combat victory
    /// </summary>
    private int CalculateCombatShipPartsReward(DiscoveredShip ship)
    {
        int baseReward = 15; // Higher than trade reward
        int rarityMultiplier = (int)ship.Rarity + 1;
        int levelBonus = ship.Level * 2;

        return baseReward * rarityMultiplier + levelBonus;
    }

    /// <summary>
    /// Calculate alien tech reward for combat victory
    /// </summary>
    private int CalculateCombatAlienTechReward(DiscoveredShip ship)
    {
        int baseReward = 5;
        int rarityMultiplier = (int)ship.Rarity + 1;

        // Give alien tech for rare ships, chance for common ships
        if (ship.IsRareCategory() || UnityEngine.Random.value < 0.3f)
        {
            return baseReward * rarityMultiplier;
        }

        return 0;
    }

    /// <summary>
    /// Calculate recycling points reward for combat victory
    /// </summary>
    private float CalculateCombatRPReward(DiscoveredShip ship)
    {
        float baseReward = 25f;
        float rarityMultiplier = (int)ship.Rarity + 1;
        float levelBonus = ship.Level * 3f;

        return baseReward * rarityMultiplier + levelBonus;
    }

    /// <summary>
    /// Calculate dimensional potential reward for combat victory
    /// </summary>
    private float CalculateCombatDPReward(DiscoveredShip ship)
    {
        float baseReward = 5f;
        float rarityMultiplier = (int)ship.Rarity + 1;
        float levelBonus = ship.Level * 0.5f;

        return baseReward * rarityMultiplier + levelBonus;
    }

    /// <summary>
    /// Switch tabs after a delay
    /// </summary>
    private System.Collections.IEnumerator DelayedTabSwitch(string tabName, float delay)
    {
        yield return new WaitForSeconds(delay);

        TabSystem tabSystem = FindObjectOfType<TabSystem>();
        if (tabSystem != null)
        {
            switch (tabName.ToLower())
            {
                case "contacts":
                    tabSystem.ShowContactsTab();
                    break;
                case "scanner":
                    tabSystem.ShowScannerTab();
                    break;
            }
        }
    }

    /// <summary>
    /// Check if player can afford a trade
    /// </summary>
    public bool CanAffordTrade(DiscoveredShip ship)
    {
        if (ship == null || !ship.CanTrade) return false;

        int availableWaste = WasteInventoryManager.Instance?.GetInventoryCount() ?? 0;
        return availableWaste >= ship.TradeValue;
    }
}