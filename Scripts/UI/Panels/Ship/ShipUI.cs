using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// Ship UI controller
public class ShipUI : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject shipViewPanel;

    [Header("Detail Panel")]
    [SerializeField] private GameObject compartmentDetailPanel;
    [SerializeField] private TextMeshProUGUI compartmentNameText;
    [SerializeField] private TextMeshProUGUI compartmentDescriptionText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private TextMeshProUGUI nextLevelPreviewText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button upgradeButton;

    [Header("Resource Display")]
    [SerializeField] private TextMeshProUGUI recyclingPointsText;
    [SerializeField] private TextMeshProUGUI dimensionalPotentialText;

    // Currently selected compartment
    private ShipCompartment selectedCompartment;

    private void Start()
    {
        Debug.Log("ShipUI: Start method called");

        // Subscribe to events
        if (ShipManager.Instance != null)
        {
            Debug.Log("ShipUI: Found ShipManager instance, subscribing to events");
            ShipManager.Instance.OnCompartmentSelected += OnCompartmentSelected;
            ShipManager.Instance.OnCompartmentUpgraded += OnCompartmentUpgraded;
        }
        else
        {
            Debug.LogError("ShipUI: ShipManager.Instance is null!");
        }

        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourcesChanged += UpdateResourceDisplay;
        }

        // Set up button listener
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        else
            Debug.LogError("ShipUI: upgradeButton is null!");

        // Hide detail panel initially
        if (compartmentDetailPanel != null)
        {
            compartmentDetailPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("ShipUI: compartmentDetailPanel is null!");
        }

        // Initial resource display update
        UpdateResourceDisplay();
    }

    // Handle compartment selection
    private void OnCompartmentSelected(ShipCompartment compartment)
    {
        Debug.Log("ShipUI: OnCompartmentSelected called for " + compartment.DisplayName);

        selectedCompartment = compartment;

        // Show the detail panel
        if (compartmentDetailPanel != null)
        {
            compartmentDetailPanel.SetActive(true);
            Debug.Log("ShipUI: Detail panel activated");
        }
        else
        {
            Debug.LogError("ShipUI: compartmentDetailPanel is null!");
        }

        UpdateDetailPanel();
    }

    // Handle compartment upgrade
    private void OnCompartmentUpgraded(ShipCompartment compartment)
    {
        if (compartment == selectedCompartment)
            UpdateDetailPanel();
    }

    // Update detail panel with compartment info
    public void UpdateDetailPanel()
    {
        if (selectedCompartment == null)
        {
            compartmentDetailPanel.SetActive(false);
            return;
        }

        compartmentDetailPanel.SetActive(true);

        // Update basic info
        compartmentNameText.text = selectedCompartment.DisplayName;
        compartmentDescriptionText.text = selectedCompartment.Description;
        levelText.text = $"Level: {selectedCompartment.CurrentLevel}/{selectedCompartment.MaxLevel}";

        // Update effect text
        effectText.text = selectedCompartment.GetEffectDescription();

        // Update next level preview
        if (selectedCompartment.CurrentLevel < selectedCompartment.MaxLevel)
        {
            nextLevelPreviewText.gameObject.SetActive(true);
            nextLevelPreviewText.text = "Next Level:\n" + selectedCompartment.GetNextLevelPreview();
        }
        else
        {
            nextLevelPreviewText.gameObject.SetActive(false);
        }

        // Update cost text and button state
        if (selectedCompartment.CurrentLevel >= selectedCompartment.MaxLevel)
        {
            costText.text = "MAX LEVEL";
            upgradeButton.interactable = false;
        }
        else
        {
            float rpCost = selectedCompartment.GetUpgradeCost(false);
            float dpCost = selectedCompartment.GetUpgradeCost(true);
            costText.text = $"Upgrade Cost:\n{rpCost:F0} RP\n{dpCost:F0} DP";

            // Check if we can afford the upgrade
            bool canAfford = ResourceManager.Instance != null &&
                           ResourceManager.Instance.RecyclingPoints >= rpCost &&
                           ResourceManager.Instance.DimensionalPotential >= dpCost;

            upgradeButton.interactable = canAfford;
        }
    }

    // Update resource display
    private void UpdateResourceDisplay()
    {
        if (ResourceManager.Instance != null)
        {
            recyclingPointsText.text = $"RP: {ResourceManager.Instance.RecyclingPoints:F0}";
            dimensionalPotentialText.text = $"DP: {ResourceManager.Instance.DimensionalPotential:F0}";
        }
    }

    // Handle upgrade button click
    private void OnUpgradeButtonClicked()
    {
        if (selectedCompartment != null && ShipManager.Instance != null)
        {
            if (ShipManager.Instance.UpgradeCompartment(selectedCompartment))
            {
                // Play upgrade success effects
                PlayUpgradeEffects();
            }
        }
    }

    // Play visual/audio effects for successful upgrade
    private void PlayUpgradeEffects()
    {
        // TODO: Add particle effects, sound effects, or animations
        // This is where you would trigger any upgrade celebration effects
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (ShipManager.Instance != null)
        {
            ShipManager.Instance.OnCompartmentSelected -= OnCompartmentSelected;
            ShipManager.Instance.OnCompartmentUpgraded -= OnCompartmentUpgraded;
        }

        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourcesChanged -= UpdateResourceDisplay;
        }

        // Remove button listener
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);
    }
}