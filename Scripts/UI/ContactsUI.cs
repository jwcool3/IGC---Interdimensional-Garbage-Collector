using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Enhanced UI Controller for the Contacts tab with combat preview integration
/// </summary>
public class ContactsUI : MonoBehaviour
{
    [Header("Main References")]
    [SerializeField] private GameObject contactsPanel;
    [SerializeField] private Transform contactsListContent;
    [SerializeField] private GameObject contactItemPrefab;
    
    [Header("Header")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contactCountText;
    
    [Header("Contact Details")]
    [SerializeField] private GameObject contactDetailsPanel;
    [SerializeField] private TextMeshProUGUI shipNameText;
    [SerializeField] private TextMeshProUGUI shipTypeText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private Image shipDetailImage;
    
    [Header("Action Buttons")]
    [SerializeField] private Button quickFightButton;
    [SerializeField] private Button previewCombatButton;
    [SerializeField] private Button tradeButton;
    [SerializeField] private Button dismissButton;
    
    [Header("Combat Integration")]
    [SerializeField] private CombatPreviewUI combatPreview;
    
    [Header("Trade Info")]
    [SerializeField] private TextMeshProUGUI tradeValueText;
    [SerializeField] private TextMeshProUGUI tradeItemsText;
    [SerializeField] private GameObject tradeInfoPanel;
    
    [Header("Combat Info")]
    [SerializeField] private GameObject combatInfoPanel;
    [SerializeField] private TextMeshProUGUI combatStatusText;
    [SerializeField] private TextMeshProUGUI lastCombatResultText;
    
    [Header("No Contacts Message")]
    [SerializeField] private GameObject noContactsMessage;
    [SerializeField] private TextMeshProUGUI noContactsText;
    
    // Private variables
    private List<GameObject> contactItemObjects = new List<GameObject>();
    private DiscoveredShip selectedShip;
    private List<DiscoveredShip> discoveredShips = new List<DiscoveredShip>();
    
    private void Start()
    {
        Debug.Log("ContactsUI: Starting initialization");
        
        SetupButtons();
        SetupCombatPreview();
        SubscribeToEvents();
        RefreshContactsList();
        
        // Initially hide details panel
        if (contactDetailsPanel != null)
            contactDetailsPanel.SetActive(false);
            
        Debug.Log("ContactsUI: Initialization complete");
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    /// <summary>
    /// Set up button event listeners
    /// </summary>
    private void SetupButtons()
    {
        if (quickFightButton != null)
            quickFightButton.onClick.AddListener(OnQuickFightButtonClicked);
        else
            Debug.LogError("ContactsUI: Quick fight button not assigned!");
            
        if (previewCombatButton != null)
            previewCombatButton.onClick.AddListener(OnPreviewCombatButtonClicked);
        else
            Debug.LogError("ContactsUI: Preview combat button not assigned!");
            
        if (tradeButton != null)
            tradeButton.onClick.AddListener(OnTradeButtonClicked);
        else
            Debug.LogError("ContactsUI: Trade button not assigned!");
            
        if (dismissButton != null)
            dismissButton.onClick.AddListener(OnDismissButtonClicked);
        else
            Debug.LogError("ContactsUI: Dismiss button not assigned!");
    }
    
    /// <summary>
    /// Set up combat preview integration
    /// </summary>
    private void SetupCombatPreview()
    {
        if (combatPreview != null)
        {
            combatPreview.OnCombatConfirmed += OnCombatConfirmed;
            combatPreview.OnCombatCancelled += OnCombatCancelled;
            Debug.Log("ContactsUI: Combat preview configured");
        }
        else
        {
            Debug.LogWarning("ContactsUI: Combat preview component not assigned!");
        }
    }
    
    /// <summary>
    /// Subscribe to relevant events
    /// </summary>
    private void SubscribeToEvents()
    {
        if (ShipScanner.Instance != null)
        {
            ShipScanner.Instance.OnShipDiscovered += OnShipDiscovered;
            ShipScanner.Instance.OnShipInteracted += OnShipInteracted;
            Debug.Log("ContactsUI: Subscribed to ShipScanner events");
        }
        else
        {
            Debug.LogError("ContactsUI: ShipScanner instance not found!");
        }
    }
    
    /// <summary>
    /// Unsubscribe from events
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (ShipScanner.Instance != null)
        {
            ShipScanner.Instance.OnShipDiscovered -= OnShipDiscovered;
            ShipScanner.Instance.OnShipInteracted -= OnShipInteracted;
        }
        
        if (combatPreview != null)
        {
            combatPreview.OnCombatConfirmed -= OnCombatConfirmed;
            combatPreview.OnCombatCancelled -= OnCombatCancelled;
        }
    }
    
    /// <summary>
    /// Handle new ship discovered
    /// </summary>
    private void OnShipDiscovered(DiscoveredShip ship)
    {
        Debug.Log($"ContactsUI: New ship discovered - {ship.ShipName}");
        
        if (!discoveredShips.Contains(ship))
        {
            discoveredShips.Add(ship);
            RefreshContactsList();
        }
    }
    
    /// <summary>
    /// Handle ship interaction completed
    /// </summary>
    private void OnShipInteracted()
    {
        Debug.Log("ContactsUI: Ship interaction completed");
        RefreshContactsList();
        RefreshDetailsPanel();
    }
    
    /// <summary>
    /// Refresh the contacts list display
    /// </summary>
    public void RefreshContactsList()
    {
        Debug.Log($"ContactsUI: Refreshing contacts list with {discoveredShips.Count} ships");
        
        ClearContactItems();
        UpdateHeader();
        
        if (discoveredShips.Count == 0)
        {
            ShowNoContactsMessage();
            return;
        }
        
        // Hide no contacts message
        if (noContactsMessage != null)
            noContactsMessage.SetActive(false);
        
        // Create items for each discovered ship
        foreach (var ship in discoveredShips)
        {
            CreateContactItem(ship);
        }
    }
    
    /// <summary>
    /// Clear all contact item GameObjects
    /// </summary>
    private void ClearContactItems()
    {
        foreach (var item in contactItemObjects)
        {
            if (item != null)
                Destroy(item);
        }
        contactItemObjects.Clear();
    }
    
    /// <summary>
    /// Update header information
    /// </summary>
    private void UpdateHeader()
    {
        if (titleText != null)
            titleText.text = "SHIP CONTACTS";
            
        if (contactCountText != null)
        {
            int activeContacts = 0;
            int totalContacts = discoveredShips.Count;
            
            foreach (var ship in discoveredShips)
            {
                if (!ship.IsDestroyed)
                    activeContacts++;
            }
            
            contactCountText.text = $"Active: {activeContacts} / Total: {totalContacts}";
        }
    }
    
    /// <summary>
    /// Show message when no contacts are available
    /// </summary>
    private void ShowNoContactsMessage()
    {
        if (noContactsMessage != null)
        {
            noContactsMessage.SetActive(true);
            
            if (noContactsText != null)
            {
                noContactsText.text = "No ships discovered yet.\n\nUse the Scanner tab to discover ships in your area.";
                noContactsText.color = Color.gray;
            }
        }
        
        // Hide details panel
        if (contactDetailsPanel != null)
            contactDetailsPanel.SetActive(false);
    }
    
    /// <summary>
    /// Create a contact item for a discovered ship
    /// </summary>
    private void CreateContactItem(DiscoveredShip ship)
    {
        if (contactItemPrefab == null || contactsListContent == null)
        {
            Debug.LogError("ContactsUI: Missing prefab or content references!");
            return;
        }
        
        GameObject contactItem = Instantiate(contactItemPrefab, contactsListContent);
        contactItemObjects.Add(contactItem);
        
        ContactItemUI itemUI = contactItem.GetComponent<ContactItemUI>();
        if (itemUI == null)
            itemUI = contactItem.AddComponent<ContactItemUI>();
            
        itemUI.Initialize(ship, this);
        
        Debug.Log($"ContactsUI: Created contact item for {ship.ShipName}");
    }
    
    /// <summary>
    /// Select a ship to show details
    /// </summary>
    public void SelectShip(DiscoveredShip ship)
    {
        Debug.Log($"ContactsUI: Selected ship - {ship.ShipName}");
        
        selectedShip = ship;
        RefreshDetailsPanel();
        
        // Show details panel
        if (contactDetailsPanel != null)
            contactDetailsPanel.SetActive(true);
    }
    
    /// <summary>
    /// Refresh the details panel for selected ship
    /// </summary>
    private void RefreshDetailsPanel()
    {
        if (selectedShip == null)
        {
            if (contactDetailsPanel != null)
                contactDetailsPanel.SetActive(false);
            return;
        }
        
        // Update ship info
        if (shipNameText != null)
            shipNameText.text = selectedShip.ShipName;
            
        if (shipTypeText != null)
            shipTypeText.text = selectedShip.ShipType;
            
        if (rarityText != null)
        {
            rarityText.text = selectedShip.GetRarityDisplayText();
            rarityText.color = selectedShip.GetRarityColor();
        }
        
        // Update ship image
        if (shipDetailImage != null)
        {
            if (selectedShip.ShipIcon != null)
            {
                shipDetailImage.sprite = selectedShip.ShipIcon;
                shipDetailImage.color = Color.white;
            }
            else
            {
                shipDetailImage.color = selectedShip.GetRarityColor();
            }
        }
        
        // Update stats
        if (healthText != null)
            healthText.text = $"Health: {selectedShip.CurrentHealth:F0}/{selectedShip.Health:F0}";
            
        if (attackText != null)
            attackText.text = $"Attack: {selectedShip.AttackPower:F0}";
            
        if (defenseText != null)
            defenseText.text = $"Defense: {selectedShip.Defense:F0}";
        
        // Update trade info
        UpdateTradeInfo();
        
        // Update combat info
        UpdateCombatInfo();
        
        // Update button states
        UpdateActionButtons();
    }
    
    /// <summary>
    /// Update trade information display
    /// </summary>
    private void UpdateTradeInfo()
    {
        if (tradeInfoPanel != null)
        {
            bool canTrade = selectedShip.CanTrade;
            tradeInfoPanel.SetActive(canTrade || selectedShip.HasTraded);
        }
        
        if (tradeValueText != null)
            tradeValueText.text = $"Wants: {selectedShip.TradeValue} waste items";
            
        if (tradeItemsText != null)
            tradeItemsText.text = $"Offers: {selectedShip.GetTradeItemsText()}";
    }
    
    /// <summary>
    /// Update combat information display
    /// </summary>
    private void UpdateCombatInfo()
    {
        if (combatInfoPanel != null)
        {
            bool showCombatInfo = selectedShip.HasFought || selectedShip.IsDestroyed;
            combatInfoPanel.SetActive(showCombatInfo);
        }
        
        if (combatStatusText != null)
        {
            if (selectedShip.IsDestroyed)
                combatStatusText.text = "Status: DESTROYED";
            else if (selectedShip.HasFought)
                combatStatusText.text = "Status: FOUGHT";
            else if (selectedShip.CanFight)
                combatStatusText.text = "Status: READY FOR COMBAT";
            else
                combatStatusText.text = "Status: NON-HOSTILE";
        }
        
        if (lastCombatResultText != null)
        {
            if (selectedShip.IsDestroyed)
            {
                lastCombatResultText.text = "Result: Victory";
                lastCombatResultText.color = Color.green;
            }
            else if (selectedShip.HasFought)
            {
                lastCombatResultText.text = "Result: Defeated";
                lastCombatResultText.color = Color.red;
            }
            else
            {
                lastCombatResultText.text = "";
            }
        }
    }
    
    /// <summary>
    /// Update action button states based on ship status
    /// </summary>
    private void UpdateActionButtons()
    {
        if (selectedShip == null) return;
        
        // Quick fight button
        if (quickFightButton != null)
        {
            bool canQuickFight = selectedShip.CanFight;
            quickFightButton.interactable = canQuickFight;
            
            TextMeshProUGUI quickFightText = quickFightButton.GetComponentInChildren<TextMeshProUGUI>();
            if (quickFightText != null)
            {
                if (selectedShip.IsDestroyed)
                    quickFightText.text = "DESTROYED";
                else if (selectedShip.HasFought)
                    quickFightText.text = "FOUGHT";
                else
                    quickFightText.text = "QUICK FIGHT";
            }
        }
        
        // Preview combat button
        if (previewCombatButton != null)
        {
            bool canPreviewFight = selectedShip.CanFight;
            previewCombatButton.interactable = canPreviewFight;
            
            TextMeshProUGUI previewText = previewCombatButton.GetComponentInChildren<TextMeshProUGUI>();
            if (previewText != null)
            {
                if (selectedShip.IsDestroyed)
                    previewText.text = "DESTROYED";
                else if (selectedShip.HasFought)
                    previewText.text = "FOUGHT";
                else
                    previewText.text = "COMBAT PREVIEW";
            }
        }
        
        // Trade button
        if (tradeButton != null)
        {
            bool canTrade = selectedShip.CanTrade;
            tradeButton.interactable = canTrade;
            
            TextMeshProUGUI tradeText = tradeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (tradeText != null)
            {
                if (selectedShip.IsDestroyed)
                    tradeText.text = "DESTROYED";
                else if (selectedShip.HasTraded)
                    tradeText.text = "TRADED";
                else
                    tradeText.text = "TRADE";
            }
        }
        
        // Dismiss button - always available for non-destroyed ships
        if (dismissButton != null)
        {
            dismissButton.interactable = !selectedShip.IsDestroyed;
        }
    }
    
    // ===== BUTTON EVENT HANDLERS =====
    
    /// <summary>
    /// Handle quick fight button click - immediate combat
    /// </summary>
    private void OnQuickFightButtonClicked()
    {
        if (selectedShip == null || !selectedShip.CanFight)
        {
            Debug.LogWarning("ContactsUI: Cannot quick fight selected ship");
            return;
        }
        
        Debug.Log($"ContactsUI: Quick fighting {selectedShip.ShipName}");
        StartCombatWithShip(selectedShip);
    }
    
    /// <summary>
    /// Handle preview combat button click - show combat preview
    /// </summary>
    private void OnPreviewCombatButtonClicked()
    {
        if (selectedShip == null || !selectedShip.CanFight)
        {
            Debug.LogWarning("ContactsUI: Cannot preview combat with selected ship");
            return;
        }
        
        Debug.Log($"ContactsUI: Showing combat preview for {selectedShip.ShipName}");
        
        if (combatPreview != null)
        {
            combatPreview.ShowCombatPreview(selectedShip);
        }
        else
        {
            Debug.LogError("ContactsUI: Combat preview component not found!");
            // Fallback to quick fight
            StartCombatWithShip(selectedShip);
        }
    }
    
    /// <summary>
    /// Handle trade button click
    /// </summary>
    private void OnTradeButtonClicked()
    {
        if (selectedShip == null || !selectedShip.CanTrade)
        {
            Debug.LogWarning("ContactsUI: Cannot trade with selected ship");
            return;
        }
        
        Debug.Log($"ContactsUI: Trading with {selectedShip.ShipName}");
        
        if (ShipInteractionManager.Instance != null)
        {
            bool success = ShipInteractionManager.Instance.TryTradeWithShip(selectedShip);
            if (success)
            {
                Debug.Log("ContactsUI: Trade completed successfully");
                RefreshDetailsPanel();
            }
            else
            {
                Debug.LogWarning("ContactsUI: Trade failed - insufficient waste items");
                string tradeInfo = ShipInteractionManager.Instance.GetTradeInfoText(selectedShip);
                Debug.Log($"Trade Info: {tradeInfo}");
                
                // Show trade requirements in UI
                ShowTradeFailureMessage();
            }
        }
        else
        {
            Debug.LogError("ContactsUI: ShipInteractionManager not found!");
        }
    }
    
    /// <summary>
    /// Handle dismiss button click
    /// </summary>
    private void OnDismissButtonClicked()
    {
        if (selectedShip == null)
        {
            Debug.LogWarning("ContactsUI: No ship selected to dismiss");
            return;
        }
        
        Debug.Log($"ContactsUI: Dismissing {selectedShip.ShipName}");
        
        // Remove from discovered ships list
        discoveredShips.Remove(selectedShip);
        
        // Clear selection
        selectedShip = null;
        
        // Refresh display
        RefreshContactsList();
        
        // Hide details panel
        if (contactDetailsPanel != null)
            contactDetailsPanel.SetActive(false);
    }
    
    // ===== COMBAT INTEGRATION =====
    
    /// <summary>
    /// Handle combat confirmation from preview
    /// </summary>
    private void OnCombatConfirmed(DiscoveredShip ship)
    {
        Debug.Log($"ContactsUI: Combat confirmed for {ship.ShipName}");
        StartCombatWithShip(ship);
    }
    
    /// <summary>
    /// Handle combat cancellation from preview
    /// </summary>
    private void OnCombatCancelled()
    {
        Debug.Log("ContactsUI: Combat cancelled by user");
        // No additional action needed - user remains in contacts tab
    }
    
    /// <summary>
    /// Actually start combat with the ship
    /// </summary>
    private void StartCombatWithShip(DiscoveredShip ship)
    {
        if (ShipInteractionManager.Instance != null)
        {
            bool success = ShipInteractionManager.Instance.StartCombatWithShip(ship);
            if (success)
            {
                Debug.Log($"ContactsUI: Combat started with {ship.ShipName}");
                
                // Switch to combat tab after a brief delay
                StartCoroutine(SwitchToCombatTabDelayed(1f));
            }
            else
            {
                Debug.LogWarning("ContactsUI: Failed to start combat");
                ShowCombatFailureMessage();
            }
        }
        else
        {
            Debug.LogError("ContactsUI: ShipInteractionManager not found!");
            ShowCombatFailureMessage();
        }
    }
    
    /// <summary>
    /// Switch to combat tab after a delay
    /// </summary>
    private System.Collections.IEnumerator SwitchToCombatTabDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        TabSystem tabSystem = FindObjectOfType<TabSystem>();
        if (tabSystem != null)
        {
            tabSystem.ShowCombatTab();
            Debug.Log("ContactsUI: Switched to combat tab");
        }
        else
        {
            Debug.LogWarning("ContactsUI: TabSystem not found for tab switching");
        }
    }
    
    // ===== UI FEEDBACK METHODS =====
    
    /// <summary>
    /// Show trade failure message
    /// </summary>
    private void ShowTradeFailureMessage()
    {
        // You can implement a popup or status message here
        Debug.Log("Trade failed: Not enough waste items!");
        
        // Example: Update trade info to show requirements
        if (tradeValueText != null)
        {
            int availableWaste = WasteInventoryManager.Instance?.GetInventoryCount() ?? 0;
            tradeValueText.text = $"Wants: {selectedShip.TradeValue} waste items\nYou have: {availableWaste}";
            tradeValueText.color = Color.red;
            
            // Reset color after delay
            StartCoroutine(ResetTradeTextColor(3f));
        }
    }
    
    /// <summary>
    /// Show combat failure message
    /// </summary>
    private void ShowCombatFailureMessage()
    {
        Debug.Log("Combat failed to start!");
        
        if (combatStatusText != null)
        {
            combatStatusText.text = "Status: COMBAT UNAVAILABLE";
            combatStatusText.color = Color.red;
            
            // Reset after delay
            StartCoroutine(ResetCombatStatusText(3f));
        }
    }
    
    /// <summary>
    /// Reset trade text color after delay
    /// </summary>
    private System.Collections.IEnumerator ResetTradeTextColor(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (tradeValueText != null)
        {
            tradeValueText.color = Color.white;
            UpdateTradeInfo(); // Refresh with normal text
        }
    }
    
    /// <summary>
    /// Reset combat status text after delay
    /// </summary>
    private System.Collections.IEnumerator ResetCombatStatusText(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (combatStatusText != null)
        {
            combatStatusText.color = Color.white;
            UpdateCombatInfo(); // Refresh with normal text
        }
    }
    
    // ===== PUBLIC METHODS =====
    
    /// <summary>
    /// Public method to add a ship (for testing or external use)
    /// </summary>
    public void AddShip(DiscoveredShip ship)
    {
        if (ship != null && !discoveredShips.Contains(ship))
        {
            discoveredShips.Add(ship);
            RefreshContactsList();
        }
    }
    
    /// <summary>
    /// Get current discovered ships (for external access)
    /// </summary>
    public List<DiscoveredShip> GetDiscoveredShips()
    {
        return new List<DiscoveredShip>(discoveredShips);
    }
    
    /// <summary>
    /// Public method to refresh the UI (called from external systems)
    /// </summary>
    public void RefreshUI()
    {
        RefreshContactsList();
        RefreshDetailsPanel();
    }
    
    /// <summary>
    /// Get currently selected ship
    /// </summary>
    public DiscoveredShip GetSelectedShip()
    {
        return selectedShip;
    }
    
    /// <summary>
    /// Force select a specific ship (useful for external navigation)
    /// </summary>
    public void ForceSelectShip(DiscoveredShip ship)
    {
        if (ship != null && discoveredShips.Contains(ship))
        {
            SelectShip(ship);
            
            // Update selection visuals for all contact items
            UpdateContactItemSelection();
        }
    }
    
    /// <summary>
    /// Update selection visuals across all contact items
    /// </summary>
    private void UpdateContactItemSelection()
    {
        ContactItemUI[] allContactItems = FindObjectsOfType<ContactItemUI>();
        
        foreach (var item in allContactItems)
        {
            if (item.GetAssignedShip() == selectedShip)
                item.SetSelected(true);
            else
                item.SetSelected(false);
        }
    }
    
    /// <summary>
    /// Debug method to test combat preview
    /// </summary>
    [ContextMenu("Test Combat Preview")]
    public void TestCombatPreview()
    {
        Debug.Log("=== TESTING COMBAT PREVIEW ===");
        
        if (combatPreview != null)
        {
            Debug.Log("✅ Combat preview component found");
            
            if (selectedShip != null)
            {
                Debug.Log($"Testing with selected ship: {selectedShip.ShipName}");
                combatPreview.ShowCombatPreview(selectedShip);
            }
            else
            {
                Debug.Log("No ship selected. Creating test ship...");
                
                var testShip = new DiscoveredShip
                {
                    ShipName = "Test Combat Ship",
                    ShipType = "Test Vessel",
                    Rarity = ShipRarity.Rare,
                    Level = 8,
                    AttackPower = 30,
                    Defense = 20,
                    Health = 120,
                    CurrentHealth = 120
                };
                
                combatPreview.ShowCombatPreview(testShip);
            }
        }
        else
        {
            Debug.LogError("❌ Combat preview component not assigned!");
        }
    }
    
    /// <summary>
    /// Debug method to show system status
    /// </summary>
    [ContextMenu("Debug Contacts System")]
    public void DebugContactsSystem()
    {
        Debug.Log("=== CONTACTS SYSTEM DEBUG ===");
        
        Debug.Log($"Discovered Ships: {discoveredShips.Count}");
        Debug.Log($"Selected Ship: {(selectedShip != null ? selectedShip.ShipName : "None")}");
        Debug.Log($"Contact Items: {contactItemObjects.Count}");
        Debug.Log($"Combat Preview: {(combatPreview != null ? "✅ Assigned" : "❌ NULL")}");
        Debug.Log($"Ship Scanner: {(ShipScanner.Instance != null ? "✅ Found" : "❌ NULL")}");
        Debug.Log($"Interaction Manager: {(ShipInteractionManager.Instance != null ? "✅ Found" : "❌ NULL")}");
        
        // Show ship details
        foreach (var ship in discoveredShips)
        {
            Debug.Log($"Ship: {ship.ShipName} - Fought: {ship.HasFought}, Traded: {ship.HasTraded}, Destroyed: {ship.IsDestroyed}");
        }
    }
}