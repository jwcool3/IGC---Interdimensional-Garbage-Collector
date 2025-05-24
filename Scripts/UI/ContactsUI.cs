using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI Controller for the Contacts tab - manages discovered ships
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
    
    [Header("Action Buttons")]
    [SerializeField] private Button fightButton;
    [SerializeField] private Button tradeButton;
    [SerializeField] private Button dismissButton;
    
    [Header("Trade Info")]
    [SerializeField] private TextMeshProUGUI tradeValueText;
    [SerializeField] private TextMeshProUGUI tradeItemsText;
    
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
        
        // Set up button listeners
        SetupButtons();
        
        // Subscribe to scanner events
        SubscribeToEvents();
        
        // Initialize display
        RefreshContactsList();
        
        // Initially hide details panel
        if (contactDetailsPanel != null)
            contactDetailsPanel.SetActive(false);
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        UnsubscribeFromEvents();
    }
    
    /// <summary>
    /// Set up button event listeners
    /// </summary>
    private void SetupButtons()
    {
        if (fightButton != null)
            fightButton.onClick.AddListener(OnFightButtonClicked);
        else
            Debug.LogError("ContactsUI: Fight button not assigned!");
            
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
    }
    
    /// <summary>
    /// Handle new ship discovered
    /// </summary>
    private void OnShipDiscovered(DiscoveredShip ship)
    {
        Debug.Log($"ContactsUI: New ship discovered - {ship.ShipName}");
        
        // Add to our list if not already there
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
        
        // Refresh the display to update ship status
        RefreshContactsList();
        RefreshDetailsPanel();
    }
    
    /// <summary>
    /// Refresh the contacts list display
    /// </summary>
    public void RefreshContactsList()
    {
        Debug.Log($"ContactsUI: Refreshing contacts list with {discoveredShips.Count} ships");
        
        // Clear existing items
        ClearContactItems();
        
        // Update header
        UpdateHeader();
        
        // Check if we have any contacts
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
        
        // Instantiate the prefab
        GameObject contactItem = Instantiate(contactItemPrefab, contactsListContent);
        contactItemObjects.Add(contactItem);
        
        // Get the ContactItemUI component (we'll create this script next)
        ContactItemUI itemUI = contactItem.GetComponent<ContactItemUI>();
        if (itemUI == null)
            itemUI = contactItem.AddComponent<ContactItemUI>();
            
        // Initialize the contact item
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
        
        // Update stats
        if (healthText != null)
            healthText.text = $"Health: {selectedShip.CurrentHealth:F0}/{selectedShip.Health:F0}";
            
        if (attackText != null)
            attackText.text = $"Attack: {selectedShip.AttackPower:F0}";
            
        if (defenseText != null)
            defenseText.text = $"Defense: {selectedShip.Defense:F0}";
        
        // Update trade info
        if (tradeValueText != null)
            tradeValueText.text = $"Wants: {selectedShip.TradeValue} waste items";
            
        if (tradeItemsText != null)
            tradeItemsText.text = $"Offers: {selectedShip.GetTradeItemsText()}";
        
        // Update button states
        UpdateActionButtons();
    }
    
    /// <summary>
    /// Update action button states based on ship status
    /// </summary>
    private void UpdateActionButtons()
    {
        if (selectedShip == null) return;
        
        // Fight button
        if (fightButton != null)
        {
            bool canFight = selectedShip.CanFight;
            fightButton.interactable = canFight;
            
            TextMeshProUGUI fightText = fightButton.GetComponentInChildren<TextMeshProUGUI>();
            if (fightText != null)
            {
                if (selectedShip.IsDestroyed)
                    fightText.text = "DESTROYED";
                else if (selectedShip.HasFought)
                    fightText.text = "FOUGHT";
                else
                    fightText.text = "FIGHT";
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
    
    /// <summary>
    /// Handle fight button click
    /// </summary>
    private void OnFightButtonClicked()
    {
        if (selectedShip == null || !selectedShip.CanFight)
        {
            Debug.LogWarning("ContactsUI: Cannot fight selected ship");
            return;
        }
        
        Debug.Log($"ContactsUI: Fighting {selectedShip.ShipName}");
        
        // Use ShipInteractionManager to start combat
        if (ShipInteractionManager.Instance != null)
        {
            bool success = ShipInteractionManager.Instance.StartCombatWithShip(selectedShip);
            if (success)
            {
                Debug.Log("ContactsUI: Combat started successfully");
                // Optionally switch to combat tab
                // FindObjectOfType<TabSystem>()?.ShowCombatTab();
            }
            else
            {
                Debug.LogWarning("ContactsUI: Failed to start combat");
            }
        }
        else
        {
            Debug.LogError("ContactsUI: ShipInteractionManager not found!");
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
        
        // Use ShipInteractionManager to perform trade
        if (ShipInteractionManager.Instance != null)
        {
            bool success = ShipInteractionManager.Instance.TryTradeWithShip(selectedShip);
            if (success)
            {
                Debug.Log("ContactsUI: Trade completed successfully");
                RefreshDetailsPanel(); // Update display after trade
            }
            else
            {
                Debug.LogWarning("ContactsUI: Trade failed - insufficient waste items");
                // Show trade requirements
                string tradeInfo = ShipInteractionManager.Instance.GetTradeInfoText(selectedShip);
                Debug.Log($"Trade Info: {tradeInfo}");
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
}