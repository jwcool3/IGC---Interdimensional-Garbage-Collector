using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
    [SerializeField] private GameObject facilityTab;
    [SerializeField] private GameObject upgradesTab;
    [SerializeField] private GameObject statsTab;

    private List<GameObject> activeWasteDisplays = new List<GameObject>();
    private List<GameObject> activeUpgradeDisplays = new List<GameObject>();

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
            collectWasteButton.onClick.AddListener(() => GameManager.Instance.CollectWaste());
        }

        // Initialize displays
        UpdateResourceDisplays();
        UpdateWasteCollection(GameManager.Instance.GetCollectedWaste());
        UpdateUpgradeDisplays();

        // Default to collection tab
        ShowCollectionTab();
    }

    private void SubscribeToEvents()
    {
        // Subscribe to GameManager events
        GameManager.Instance.OnWasteCollected += HandleNewWaste;
        GameManager.Instance.OnWasteUpdated += UpdateWasteCollection;
        GameManager.Instance.OnContaminationLevelChanged += UpdateContaminationLevel;

        // Subscribe to FacilityManager events
        FacilityManager.Instance.OnRecyclingPointsChanged += UpdateRecyclingPoints;
        FacilityManager.Instance.OnDimensionalPotentialChanged += UpdateDimensionalPotential;
        FacilityManager.Instance.OnUpgradesPurchased += UpdateUpgradeDisplays;
    }

    private void UpdateResourceDisplays()
    {
        if (recyclingPointsText != null)
        {
            recyclingPointsText.text = $"Recycling Points: {FacilityManager.Instance.CurrentRecyclingPoints:F1}";
        }

        if (dimensionalPotentialText != null)
        {
            dimensionalPotentialText.text = $"Dimensional Potential: {FacilityManager.Instance.CurrentDimensionalPotential:F1}";
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
        var upgrades = FacilityManager.Instance.GetAvailableUpgrades();
        foreach (var upgrade in upgrades)
        {
            var upgradeDisplay = Instantiate(upgradeItemPrefab, upgradeContainer);
            var displayComponent = upgradeDisplay.GetComponent<UpgradeDisplay>();
            if (displayComponent != null)
            {
                displayComponent.Initialize(upgrade);
            }
            activeUpgradeDisplays.Add(upgradeDisplay);
        }
    }

    // Tab management
    public void ShowCollectionTab()
    {
        collectionTab.SetActive(true);
        facilityTab.SetActive(false);
        upgradesTab.SetActive(false);
        statsTab.SetActive(false);
    }

    public void ShowFacilityTab()
    {
        collectionTab.SetActive(false);
        facilityTab.SetActive(true);
        upgradesTab.SetActive(false);
        statsTab.SetActive(false);
    }

    public void ShowUpgradesTab()
    {
        collectionTab.SetActive(false);
        facilityTab.SetActive(false);
        upgradesTab.SetActive(true);
        statsTab.SetActive(false);
    }

    public void ShowStatsTab()
    {
        collectionTab.SetActive(false);
        facilityTab.SetActive(false);
        upgradesTab.SetActive(false);
        statsTab.SetActive(true);
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

        if (FacilityManager.Instance != null)
        {
            FacilityManager.Instance.OnRecyclingPointsChanged -= UpdateRecyclingPoints;
            FacilityManager.Instance.OnDimensionalPotentialChanged -= UpdateDimensionalPotential;
            FacilityManager.Instance.OnUpgradesPurchased -= UpdateUpgradeDisplays;
        }
    }
} 