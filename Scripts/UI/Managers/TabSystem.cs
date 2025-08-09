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
    [SerializeField] private Button scannerTabButton;
    [SerializeField] private Button contactsTabButton;
    [SerializeField] private Button resourcesTabButton;

    [Header("Tab Content")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject upgradesPanel;
    [SerializeField] private GameObject locationsPanel;
    [SerializeField] private GameObject probesPanel;
    [SerializeField] private GameObject shipPanel;
    [SerializeField] private GameObject combatPanel;
    [SerializeField] private GameObject scannerPanel;
    [SerializeField] private GameObject contactsPanel;
    [SerializeField] private GameObject resourcesPanel;

    [Header("Visual Settings")]
    [SerializeField] private Color activeTabColor = Color.white;
    [SerializeField] private Color inactiveTabColor = new Color(0.7f, 0.7f, 0.7f);
    [SerializeField] private float activeTabScale = 1.1f;
    [SerializeField] private float inactiveTabScale = 1.0f;

    // Reference to currently active tab
    private GameObject currentActivePanel;
    private Button currentActiveButton;

    private List<GameObject> allPanels;
    private bool initialized = false;

    private void OnEnable()
    {
        if (!initialized)
        {
            Initialize();
        }
    }

    private void Start()
    {
        if (!initialized)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        Debug.Log("TabSystem: Initializing...");

        // Verify panel references
        VerifyPanelReferences();

        // Create list of valid panels
        allPanels = new List<GameObject>();

        if (inventoryPanel != null) allPanels.Add(inventoryPanel);
        if (upgradesPanel != null) allPanels.Add(upgradesPanel);
        if (locationsPanel != null) allPanels.Add(locationsPanel);
        if (probesPanel != null) allPanels.Add(probesPanel);
        if (shipPanel != null) allPanels.Add(shipPanel);
        if (combatPanel != null) allPanels.Add(combatPanel);
        if (scannerPanel != null) allPanels.Add(scannerPanel);
        if (contactsPanel != null) allPanels.Add(contactsPanel);
        if (resourcesPanel != null) allPanels.Add(resourcesPanel);

        Debug.Log($"TabSystem: Found {allPanels.Count} valid panels");

        // Set up button listeners
        SetupButtonListeners();

        // Make sure all panels have CanvasGroup components
        SetupPanelCanvasGroups();

        // Start with inventory panel active if available, otherwise use the first valid panel
        GameObject defaultPanel = inventoryPanel;
        Button defaultButton = wasteCollectionButton;

        if (defaultPanel == null && allPanels.Count > 0)
        {
            defaultPanel = allPanels[0];
            Debug.Log($"TabSystem: Using {defaultPanel.name} as default panel because inventoryPanel is null");
        }

        if (defaultPanel != null)
        {
            Debug.Log($"TabSystem: Setting default panel to {defaultPanel.name}");
            SwitchToTab(defaultPanel, defaultButton);
        }
        else
        {
            Debug.LogError("TabSystem: No valid panels found! Cannot initialize tab system.");
        }

        initialized = true;
    }

    private void VerifyPanelReferences()
    {
        if (inventoryPanel == null) Debug.LogWarning("TabSystem: inventoryPanel reference is missing!");
        if (upgradesPanel == null) Debug.LogWarning("TabSystem: upgradesPanel reference is missing!");
        if (locationsPanel == null) Debug.LogWarning("TabSystem: locationsPanel reference is missing!");
        if (probesPanel == null) Debug.LogWarning("TabSystem: probesPanel reference is missing!");
        if (shipPanel == null) Debug.LogWarning("TabSystem: shipPanel reference is missing!");
        if (combatPanel == null) Debug.LogWarning("TabSystem: combatPanel reference is missing!");
        if (scannerPanel == null) Debug.LogWarning("TabSystem: scannerPanel reference is missing!");
        if (contactsPanel == null) Debug.LogWarning("TabSystem: contactsPanel reference is missing!");
        if (resourcesPanel == null) Debug.LogWarning("TabSystem: resourcesPanel reference is missing!");
    }

    private void SetupButtonListeners()
    {
        if (wasteCollectionButton != null)
            wasteCollectionButton.onClick.AddListener(() => SwitchToTab(inventoryPanel, wasteCollectionButton));
        else
            Debug.LogWarning("TabSystem: wasteCollectionButton reference is missing!");

        if (upgradesButton != null)
            upgradesButton.onClick.AddListener(() => SwitchToTab(upgradesPanel, upgradesButton));
        else
            Debug.LogWarning("TabSystem: upgradesButton reference is missing!");

        if (locationsButton != null)
            locationsButton.onClick.AddListener(() => SwitchToTab(locationsPanel, locationsButton));
        else
            Debug.LogWarning("TabSystem: locationsButton reference is missing!");

        if (probesTabButton != null)
            probesTabButton.onClick.AddListener(() => SwitchToTab(probesPanel, probesTabButton));
        else
            Debug.LogWarning("TabSystem: probesTabButton reference is missing!");

        if (shipTabButton != null)
            shipTabButton.onClick.AddListener(() => SwitchToTab(shipPanel, shipTabButton));
        else
            Debug.LogWarning("TabSystem: shipTabButton reference is missing!");

        if (combatTabButton != null)
            combatTabButton.onClick.AddListener(() => SwitchToTab(combatPanel, combatTabButton));
        else
            Debug.LogWarning("TabSystem: combatTabButton reference is missing!");

        if (scannerTabButton != null)
            scannerTabButton.onClick.AddListener(() => SwitchToTab(scannerPanel, scannerTabButton));
        else
            Debug.LogWarning("TabSystem: scannerTabButton reference is missing!");

        if (contactsTabButton != null)
            contactsTabButton.onClick.AddListener(() => SwitchToTab(contactsPanel, contactsTabButton));
        else
            Debug.LogWarning("TabSystem: contactsTabButton reference is missing!");

        if (resourcesTabButton != null)
            resourcesTabButton.onClick.AddListener(() => SwitchToTab(resourcesPanel, resourcesTabButton));
        else
            Debug.LogWarning("TabSystem: resourcesTabButton reference is missing!");
    }

    private void SetupPanelCanvasGroups()
    {
        foreach (var panel in allPanels)
        {
            if (panel == null) continue;

            // Ensure panel has CanvasGroup
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.AddComponent<CanvasGroup>();
                Debug.Log($"TabSystem: Added CanvasGroup to {panel.name}");
            }

            // Initially hide all panels
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
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
            if (canvasGroup == null) continue;

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
        if (inventoryPanel != null)
            SwitchToTab(inventoryPanel, wasteCollectionButton);
        else
            Debug.LogError("TabSystem: Cannot show inventory tab - inventoryPanel is null!");
    }

    public void ShowUpgradesTab()
    {
        if (upgradesPanel != null)
            SwitchToTab(upgradesPanel, upgradesButton);
        else
            Debug.LogError("TabSystem: Cannot show upgrades tab - upgradesPanel is null!");
    }

    public void ShowLocationsTab()
    {
        if (locationsPanel != null)
            SwitchToTab(locationsPanel, locationsButton);
        else
            Debug.LogError("TabSystem: Cannot show locations tab - locationsPanel is null!");
    }

    public void ShowProbesTab()
    {
        if (probesPanel != null)
            SwitchToTab(probesPanel, probesTabButton);
        else
            Debug.LogError("TabSystem: Cannot show probes tab - probesPanel is null!");
    }

    public void ShowShipTab()
    {
        if (shipPanel != null)
            SwitchToTab(shipPanel, shipTabButton);
        else
            Debug.LogError("TabSystem: Cannot show ship tab - shipPanel is null!");
    }

    public void ShowCombatTab()
    {
        if (combatPanel != null)
            SwitchToTab(combatPanel, combatTabButton);
        else
            Debug.LogError("TabSystem: Cannot show combat tab - combatPanel is null!");
    }

    public void ShowScannerTab()
    {
        if (scannerPanel != null)
            SwitchToTab(scannerPanel, scannerTabButton);
        else
            Debug.LogError("TabSystem: Cannot show scanner tab - scannerPanel is null!");
    }

    public void ShowContactsTab()
    {
        if (contactsPanel != null)
            SwitchToTab(contactsPanel, contactsTabButton);
        else
            Debug.LogError("TabSystem: Cannot show contacts tab - contactsPanel is null!");
    }

    public void ShowResourcesTab()
    {
        if (resourcesPanel != null)
            SwitchToTab(resourcesPanel, resourcesTabButton);
        else
            Debug.LogError("TabSystem: Cannot show resources tab - resourcesPanel is null!");
    }
}