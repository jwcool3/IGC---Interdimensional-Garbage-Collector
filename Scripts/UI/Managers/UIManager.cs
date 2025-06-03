using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    // Singleton pattern
    public static UIManager Instance { get; private set; }

    [Header("Resource Displays")]
    [SerializeField] private TextMeshProUGUI recyclingPointsText;
    [SerializeField] private TextMeshProUGUI dimensionalPotentialText;
    [SerializeField] private TextMeshProUGUI contaminationLevelText;
    [SerializeField] private Slider contaminationLevelSlider;

    [Header("Waste Collection")]
    [SerializeField] private Transform wasteCollectionContainer;
    [SerializeField] private GameObject wasteItemPrefab;
    [SerializeField] private Button collectWasteButton;
    [SerializeField] private TextMeshProUGUI totalWasteText;

    [Header("Facility Management")]
    [SerializeField] private Transform upgradeContainer;
    [SerializeField] private GameObject upgradeItemPrefab;

    [Header("Tabs")]
    [SerializeField] private GameObject collectionTab;
    [SerializeField] private GameObject upgradesTab;
    // [SerializeField] private GameObject statsTab;

    [Header("Tab Buttons")]
    [SerializeField] private Button collectionTabButton;
    [SerializeField] private Button upgradesTabButton;
    // [SerializeField] private Button statsTabButton;

    [Header("Tab Button Visual Settings")]
    [SerializeField] private Color activeTabColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color inactiveTabColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    [SerializeField] private float activeTabScale = 1.1f;
    [SerializeField] private float inactiveTabScale = 1.0f;

    private List<GameObject> activeWasteDisplays = new List<GameObject>();
    private List<GameObject> activeUpgradeDisplays = new List<GameObject>();

    private Button currentActiveTabButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeUI()
    {
        // Set up button listeners
        if (collectWasteButton != null)
        {
            collectWasteButton.onClick.AddListener(() => {
                Debug.Log("Generate Waste button clicked!");
                if (GameManager.Instance != null)
                    GameManager.Instance.CollectWaste();
                else
                    Debug.LogError("GameManager.Instance is null!");
            });
        }

        // Set up tab button listeners
        if (collectionTabButton != null)
            collectionTabButton.onClick.AddListener(ShowCollectionTab);
        if (upgradesTabButton != null)
            upgradesTabButton.onClick.AddListener(ShowUpgradesTab);
        // if (statsTabButton != null)
        //     statsTabButton.onClick.AddListener(ShowStatsTab);

        // Initialize displays
        UpdateResourceDisplays();

        // Subscribe to events
        SubscribeToEvents();

        // Setup initial waste and upgrades
        if (GameManager.Instance != null)
            UpdateWasteCollection(GameManager.Instance.GetCollectedWaste());
        UpdateUpgradeDisplays();

        // Initialize all tabs to inactive state first
        InitializeTabStates();

        // Explicitly call ShowCollectionTab to activate it
        ShowCollectionTab();

        // Force Canvas update to ensure proper display
        Canvas.ForceUpdateCanvases();

        Debug.Log("UI Initialization complete. Collection tab should be visible.");
    }

    private void InitializeTabStates()
    {
        // Ensure all tabs are initially inactive
        if (collectionTab != null) SetTabState(collectionTab, collectionTab.GetComponent<CanvasGroup>(), false);
        if (upgradesTab != null) SetTabState(upgradesTab, upgradesTab.GetComponent<CanvasGroup>(), false);
        // if (statsTab != null) SetTabState(statsTab, statsTab.GetComponent<CanvasGroup>(), false);

        // Reset all button states to inactive
        if (collectionTabButton != null) SetButtonState(collectionTabButton, false);
        if (upgradesTabButton != null) SetButtonState(upgradesTabButton, false);
        // if (statsTabButton != null) SetButtonState(statsTabButton, false);

        // Reset current active button reference
        currentActiveTabButton = null;
    }

    private void SubscribeToEvents()
    {
        // Subscribe to GameManager events
        GameManager.Instance.OnWasteCollected += HandleNewWaste;
        GameManager.Instance.OnWasteUpdated += UpdateWasteCollection;
        GameManager.Instance.OnContaminationLevelChanged += UpdateContaminationLevel;

        // Subscribe to ResourceManager events
        ResourceManager.Instance.OnRecyclingPointsChanged += UpdateRecyclingPoints;
        ResourceManager.Instance.OnDimensionalPotentialChanged += UpdateDimensionalPotential;

        // Subscribe to FacilityManager events
        FacilityManager.Instance.OnUpgradeCompleted += (upgradeName) => UpdateUpgradeDisplays();
    }

    public void UpdateResourceDisplays()
    {
        if (recyclingPointsText != null && ResourceManager.Instance != null)
        {
            recyclingPointsText.text = $"Recycling Points: {ResourceManager.Instance.RecyclingPoints:F1}";
        }

        if (dimensionalPotentialText != null && ResourceManager.Instance != null)
        {
            dimensionalPotentialText.text = $"Dimensional Potential: {ResourceManager.Instance.DimensionalPotential:F1}";
        }

        UpdateContaminationLevel(GameManager.Instance.FacilityContaminationLevel);
    }

    private void UpdateContaminationLevel(float level)
    {
        if (contaminationLevelText != null)
        {
            contaminationLevelText.text = $"Contamination: {(level * 100):F1}%";
        }

        if (contaminationLevelSlider != null)
        {
            contaminationLevelSlider.value = level;
        }
    }

    private void UpdateRecyclingPoints(float points)
    {
        if (recyclingPointsText != null)
        {
            recyclingPointsText.text = $"Recycling Points: {points:F1}";
        }
    }

    private void UpdateDimensionalPotential(float potential)
    {
        if (dimensionalPotentialText != null)
        {
            dimensionalPotentialText.text = $"Dimensional Potential: {potential:F1}";
        }
    }

    private void HandleNewWaste(WasteItem waste)
    {
        if (totalWasteText != null)
        {
            totalWasteText.text = $"Total Waste: {GameManager.Instance.TotalWasteCollected}";
        }
    }

    private void UpdateWasteCollection(List<WasteItem> wasteItems)
    {
        // Clear existing displays
        foreach (var display in activeWasteDisplays)
        {
            Destroy(display);
        }
        activeWasteDisplays.Clear();

        // Create new displays
        foreach (var waste in wasteItems)
        {
            var wasteDisplay = Instantiate(wasteItemPrefab, wasteCollectionContainer);
            var displayComponent = wasteDisplay.GetComponent<WasteDisplay>();
            if (displayComponent != null)
            {
                displayComponent.Initialize(waste);
            }
            activeWasteDisplays.Add(wasteDisplay);
        }

        if (totalWasteText != null)
        {
            totalWasteText.text = $"Total Waste: {wasteItems.Count}";
        }
    }

    private void UpdateUpgradeDisplays()
    {
        // Clear existing displays
        foreach (var display in activeUpgradeDisplays)
        {
            Destroy(display);
        }
        activeUpgradeDisplays.Clear();

        // Create new displays for each upgrade
        if (FacilityManager.Instance != null)
        {
            var upgrades = FacilityManager.Instance.GetAvailableUpgrades();
            foreach (var upgrade in upgrades)
            {
                var upgradeDisplay = Instantiate(upgradeItemPrefab, upgradeContainer);
                var displayComponent = upgradeDisplay.GetComponent<UpgradeItemUI>();
                if (displayComponent != null)
                {
                    displayComponent.SetUpgrade(upgrade.Value);
                    displayComponent.SetUpgradeType(upgrade.Key);
                }
                activeUpgradeDisplays.Add(upgradeDisplay);
            }
        }
    }

    // Tab management
    public void ShowCollectionTab()
    {
        Debug.Log("ShowCollectionTab called");

        // Check if the collection tab exists
        if (collectionTab == null)
        {
            Debug.LogError("Collection tab is null!");
            return;
        }

        SetTabVisibility(collectionTab);
        UpdateTabButtonHighlight(collectionTabButton);

        // Force Canvas update
        Canvas.ForceUpdateCanvases();

        // Log the state for debugging
        CanvasGroup cg = collectionTab?.GetComponent<CanvasGroup>();
        Debug.Log($"Collection Tab - Active: {collectionTab.activeSelf}, " +
                  $"Interactable: {cg?.interactable}, BlocksRaycasts: {cg?.blocksRaycasts}");
    }

    public void ShowUpgradesTab()
    {
        SetTabVisibility(upgradesTab);
        UpdateTabButtonHighlight(upgradesTabButton);
    }

    // public void ShowStatsTab()
    // {
    //     SetTabVisibility(statsTab);
    //     UpdateTabButtonHighlight(statsTabButton);
    // }

    private void SetTabVisibility(GameObject activeTab)
    {
        // Get references to all tab CanvasGroups (with null checks)
        CanvasGroup collectionCG = collectionTab?.GetComponent<CanvasGroup>();
        CanvasGroup upgradesCG = upgradesTab?.GetComponent<CanvasGroup>();
        // CanvasGroup statsCG = statsTab?.GetComponent<CanvasGroup>();

        // Set active tab (with null checks)
        if (collectionTab != null)
            SetTabState(collectionTab, collectionCG, activeTab == collectionTab);

        if (upgradesTab != null)
            SetTabState(upgradesTab, upgradesCG, activeTab == upgradesTab);

        // if (statsTab != null)
        //     SetTabState(statsTab, statsCG, activeTab == statsTab);
    }

    private void SetTabState(GameObject tab, CanvasGroup canvasGroup, bool isActive)
    {
        if (tab == null) return;

        tab.SetActive(isActive);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = isActive ? 1f : 0f;
            canvasGroup.interactable = isActive;
            canvasGroup.blocksRaycasts = isActive;
        }
    }

    private void UpdateTabButtonHighlight(Button activeButton)
    {
        // Reset previous active button
        if (currentActiveTabButton != null)
        {
            SetButtonState(currentActiveTabButton, false);
        }

        // Set new active button
        SetButtonState(activeButton, true);
        currentActiveTabButton = activeButton;
    }

    private void SetButtonState(Button button, bool isActive)
    {
        if (button == null) return;

        // Update color
        var buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = isActive ? activeTabColor : inactiveTabColor;
        }

        // Update scale
        button.transform.localScale = Vector3.one * (isActive ? activeTabScale : inactiveTabScale);

        // Update text color if present
        var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.color = isActive ? activeTabColor : inactiveTabColor;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnWasteCollected -= HandleNewWaste;
            GameManager.Instance.OnWasteUpdated -= UpdateWasteCollection;
            GameManager.Instance.OnContaminationLevelChanged -= UpdateContaminationLevel;
        }

        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnRecyclingPointsChanged -= UpdateRecyclingPoints;
            ResourceManager.Instance.OnDimensionalPotentialChanged -= UpdateDimensionalPotential;
        }

        if (FacilityManager.Instance != null)
        {
            FacilityManager.Instance.OnUpgradeCompleted -= (upgradeName) => UpdateUpgradeDisplays();
        }
    }

    private void Update()
    {
        // Check if mouse is over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Mouse is over UI");
        }

        // Press space to force waste generation (for testing)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed - forcing waste generation");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CollectWaste();
                Debug.Log("Waste generation triggered via space key");
            }
            else
            {
                Debug.LogError("Cannot generate waste: GameManager.Instance is null!");
            }
        }
    }
}