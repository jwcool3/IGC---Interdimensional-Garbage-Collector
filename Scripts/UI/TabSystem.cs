using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
    
    [Header("Visual Settings")]
    [SerializeField] private Color activeTabColor = Color.white;
    [SerializeField] private Color inactiveTabColor = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] private float activeTabScale = 1.1f;
    [SerializeField] private float inactiveTabScale = 1.0f;
    
    // Reference to currently active tab
    private GameObject currentActivePanel;
    private Button currentActiveButton;
    
    private List<GameObject> allPanels;
    
    private void Awake()
    {
        // Create list of all panels
        allPanels = new List<GameObject> {
            inventoryPanel, upgradesPanel, locationsPanel,
            probesPanel, shipPanel, combatPanel
        };
    }
    
    private void Start()
    {
        Debug.Log("TabSystem: Start method called");
        
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
            combatTabButton.onClick.AddListener(() => SwitchToTab(combatPanel, combatTabButton));
        
        // Make sure all panels have CanvasGroup components
        SetupPanelCanvasGroups();
        
        // Start with inventory panel active and hide all others
        Debug.Log("TabSystem: Activating default tab (inventory)");
        SwitchToTab(inventoryPanel, wasteCollectionButton);
    }
    
    private void SetupPanelCanvasGroups()
    {
        foreach (var panel in allPanels)
        {
            if (panel == null) 
            {
                Debug.LogWarning("TabSystem: One of the panels is null!");
                continue;
            }
            
            // Ensure panel has CanvasGroup
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.AddComponent<CanvasGroup>();
                Debug.Log($"TabSystem: Added CanvasGroup to {panel.name}");
            }
        }
    }
    
    public void SwitchToTab(GameObject targetPanel, Button targetButton)
    {
        if (targetPanel == null)
        {
            Debug.LogError("TabSystem: Target panel is null!");
            return;
        }
        
        Debug.Log($"TabSystem: Switching to tab: {targetPanel.name}");
        
        // Hide all panels first
        foreach (var panel in allPanels)
        {
            if (panel == null) continue;
            
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            
            if (panel == targetPanel)
            {
                // Show this panel
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                Debug.Log($"TabSystem: Showing panel {panel.name}");
            }
            else
            {
                // Hide other panels
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
        
        // Update button visuals
        UpdateButtonVisuals(targetButton);
        
        // Update current active panel reference
        currentActivePanel = targetPanel;
        
        Debug.Log($"TabSystem: Successfully switched to {targetPanel.name}");
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
        SwitchToTab(locationsPanel, locationsButton);
    }
    
    public void ShowProbesTab()
    {
        SwitchToTab(probesPanel, probesTabButton);
    }
    
    public void ShowShipTab()
    {
        SwitchToTab(shipPanel, shipTabButton);
    }
    
    public void ShowCombatTab()
    {
        SwitchToTab(combatPanel, combatTabButton);
    }
}