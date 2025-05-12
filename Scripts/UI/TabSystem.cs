using UnityEngine;
using UnityEngine.UI;

public class TabSystem : MonoBehaviour
{
    [Header("Tab Buttons")]
    [SerializeField] private Button wasteCollectionButton;
    [SerializeField] private Button upgradesButton;
    [SerializeField] private Button locationsButton;
    [SerializeField] private Button probesTabButton;
    [SerializeField] private Button shipTabButton;
    [SerializeField] private Button combatTabButton;
    
    [Header("Tab Content")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject upgradesPanel;
    [SerializeField] private GameObject locationsPanel;
    [SerializeField] private GameObject probesPanel;
    [SerializeField] private GameObject shipPanel;
    [SerializeField] private GameObject combatPanel;
    
    [Header("Persistent UI")]
    [SerializeField] private GameObject actionPanel; // Always visible
    [SerializeField] private GameObject resourcePanel; // Always visible
    
    [Header("Visual Settings")]
    [SerializeField] private Color activeTabColor = Color.white;
    [SerializeField] private Color inactiveTabColor = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] private float activeTabScale = 1.1f;
    [SerializeField] private float inactiveTabScale = 1.0f;
    
    // Reference to currently active tab
    private GameObject currentActivePanel;
    private Button currentActiveButton;
    
    private void Start()
    {
        // Set up button listeners
        if (wasteCollectionButton != null)
            wasteCollectionButton.onClick.AddListener(() => SwitchToTab(inventoryPanel, wasteCollectionButton));
            
        if (upgradesButton != null)
            upgradesButton.onClick.AddListener(() => SwitchToTab(upgradesPanel, upgradesButton));
            
        if (locationsButton != null)
            locationsButton.onClick.AddListener(() => SwitchToTab(locationsPanel, locationsButton));

        if (probesTabButton != null)
            probesTabButton.onClick.AddListener(() => SwitchToTab(probesPanel, probesTabButton));
            
        if (shipTabButton != null)
            shipTabButton.onClick.AddListener(() => SwitchToTab(shipPanel, shipTabButton));
            
        if (combatTabButton != null)
            combatTabButton.onClick.AddListener(() => ShowCombatTab());
        
        // Activate default tab (waste collection)
        if (inventoryPanel != null && wasteCollectionButton != null)
            SwitchToTab(inventoryPanel, wasteCollectionButton);
        else if (inventoryPanel != null)
            SwitchToTab(inventoryPanel, null);
            
        Debug.Log("TabSystem initialized");
    }
    
    public void SwitchToTab(GameObject targetPanel, Button targetButton)
    {
        if (targetPanel == null)
        {
            Debug.LogError("Target panel is null!");
            return;
        }
        
        Debug.Log($"Switching to tab: {targetPanel.name}");
        
        // First, deactivate ALL content panels
        DeactivateAllContentPanels();
        
        // Update button visuals
        UpdateButtonVisuals(targetButton);
        
        // Activate new panel
        targetPanel.SetActive(true);
        currentActivePanel = targetPanel;
        
        // CRITICAL: Make sure persistent UI stays active
        EnsurePersistentUIActive();
        
        Debug.Log($"Successfully switched to {targetPanel.name}");
    }
    
    private void DeactivateAllContentPanels()
    {
        // Only deactivate content panels, not UI elements
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (upgradesPanel != null) upgradesPanel.SetActive(false);
        if (locationsPanel != null) locationsPanel.SetActive(false);
        if (probesPanel != null) probesPanel.SetActive(false);
        if (shipPanel != null) shipPanel.SetActive(false);
        if (combatPanel != null) combatPanel.SetActive(false);
    }
    
    private void EnsurePersistentUIActive()
    {
        // Keep UI panels visible
        if (actionPanel != null) actionPanel.SetActive(true);
        if (resourcePanel != null) resourcePanel.SetActive(true);
        
        // Ensure all tab buttons remain active
        if (wasteCollectionButton != null) wasteCollectionButton.gameObject.SetActive(true);
        if (upgradesButton != null) upgradesButton.gameObject.SetActive(true);
        if (locationsButton != null) locationsButton.gameObject.SetActive(true);
        if (probesTabButton != null) probesTabButton.gameObject.SetActive(true);
        if (shipTabButton != null) shipTabButton.gameObject.SetActive(true);
        if (combatTabButton != null) combatTabButton.gameObject.SetActive(true);
    }
    
    private void UpdateButtonVisuals(Button newActiveButton)
    {
        // Reset previous button
        if (currentActiveButton != null)
        {
            Image btnImage = currentActiveButton.GetComponent<Image>();
            if (btnImage != null)
                btnImage.color = inactiveTabColor;
                
            currentActiveButton.transform.localScale = Vector3.one * inactiveTabScale;
        }
        
        // Set new active button
        currentActiveButton = newActiveButton;
        if (newActiveButton != null)
        {
            Image btnImage = newActiveButton.GetComponent<Image>();
            if (btnImage != null)
                btnImage.color = activeTabColor;
                
            newActiveButton.transform.localScale = Vector3.one * activeTabScale;
        }
    }
    
    // Public methods to activate specific tabs from other scripts
    public void ShowInventoryTab()
    {
        SwitchToTab(inventoryPanel, wasteCollectionButton);
    }
    
    public void ShowUpgradesTab()
    {
        SwitchToTab(upgradesPanel, upgradesButton);
    }
    
    public void ShowLocationsTab()
    {
        // Switch to the location tab
        SwitchToTab(locationsPanel, locationsButton);
        
        // Refresh the location display with current location
        LocationImageDisplay display = locationsPanel.GetComponentInChildren<LocationImageDisplay>();
        LocationSelectionManager manager = locationsPanel.GetComponentInChildren<LocationSelectionManager>();
        
        if (display != null && LocationManager.Instance != null)
        {
            display.UpdateLocationDisplay(LocationManager.Instance.GetCurrentLocation());
        }
        
        if (manager != null)
        {
            // Refresh selection
            manager.SelectLocation(LocationManager.Instance.GetCurrentLocation());
        }
    }
    
    public void ShowProbesTab()
    {
        SwitchToTab(probesPanel, probesTabButton);
    }
    
    public void ShowShipTab()
    {
        SwitchToTab(shipPanel, shipTabButton);
        
        // Optional: Refresh ship display if needed
        ShipUI shipUI = shipPanel.GetComponentInChildren<ShipUI>();
        if (shipUI != null)
        {
            // Update any ship UI elements that need refreshing
            shipUI.UpdateDetailPanel();
        }
    }
    
    public void ShowCombatTab()
    {
        SwitchToTab(combatPanel, combatTabButton);
        
        // Update combat display
        CombatUI.Instance?.UpdateAllDisplays();
    }
}