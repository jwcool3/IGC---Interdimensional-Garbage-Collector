using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Image upgradeIcon;
    [SerializeField] private Image progressBar;

    [Header("Visual Settings")]
    [SerializeField] private Color canAffordColor = new Color(0.2f, 1f, 0.2f);
    [SerializeField] private Color cannotAffordColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private Color maxLevelColor = new Color(0.8f, 0.8f, 0.2f);

    private FacilityUpgrade currentUpgrade;
    private bool isInitialized;

    public void Initialize(FacilityUpgrade upgrade)
    {
        currentUpgrade = upgrade;
        isInitialized = true;

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        }

        UpdateDisplay();
    }

    private void Update()
    {
        if (isInitialized && gameObject.activeInHierarchy && ResourceManager.Instance != null)
        {
            UpdateAffordability();
        }
    }

    private void UpdateDisplay()
    {
        if (currentUpgrade == null) return;

        // Update basic info
        if (nameText != null)
            nameText.text = currentUpgrade.UpgradeName;

        if (descriptionText != null)
            descriptionText.text = currentUpgrade.Description;

        if (levelText != null)
            levelText.text = $"Level {currentUpgrade.CurrentLevel}/{currentUpgrade.MaxLevel}";

        UpdateCostDisplay();
        UpdateProgressBar();
        UpdateAffordability();
    }

    private void UpdateCostDisplay()
    {
        if (costText == null || currentUpgrade == null) return;

        if (currentUpgrade.CurrentLevel >= currentUpgrade.MaxLevel)
        {
            costText.text = "MAX LEVEL";
            costText.color = maxLevelColor;
        }
        else
        {
            costText.text = $"Cost:\nRP: {currentUpgrade.CurrentRecyclingPointCost:F0}\nDP: {currentUpgrade.CurrentDimensionalPotentialCost:F1}";
        }
    }

    private void UpdateProgressBar()
    {
        if (progressBar != null && currentUpgrade != null)
        {
            float progress = (float)currentUpgrade.CurrentLevel / currentUpgrade.MaxLevel;
            progressBar.fillAmount = progress;
        }
    }

    private void UpdateAffordability()
    {
        if (upgradeButton == null || currentUpgrade == null || ResourceManager.Instance == null) return;

        bool isMaxLevel = currentUpgrade.CurrentLevel >= currentUpgrade.MaxLevel;
        bool canAfford = currentUpgrade.CanAfford(
            ResourceManager.Instance.RecyclingPoints,
            ResourceManager.Instance.DimensionalPotential
        );

        upgradeButton.interactable = !isMaxLevel && canAfford;

        // Update button color
        var buttonImage = upgradeButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            if (isMaxLevel)
                buttonImage.color = maxLevelColor;
            else
                buttonImage.color = canAfford ? canAffordColor : cannotAffordColor;
        }
    }

    private void OnUpgradeButtonClicked()
    {
        if (currentUpgrade == null || FacilityManager.Instance == null) return;

        // Extract the upgrade type from the upgrade name (or another method if you prefer)
        string upgradeType = DetermineUpgradeType(currentUpgrade.UpgradeName);
        if (!string.IsNullOrEmpty(upgradeType))
        {
            FacilityManager.Instance.TryUpgrade(upgradeType);
        }
        UpdateDisplay();
    }

    private string DetermineUpgradeType(string upgradeName)
    {
        // Map upgrade names to upgrade types based on your system
        switch (upgradeName)
        {
            case "Waste Storage Wing":
                return "WasteStorage";
            case "Recycling Laboratory":
                return "RecyclingLab";
            case "Dimensional Stabilization":
                return "StabilizationChamber";
            case "Expedition Center":
                return "ExpeditionCenter";
            default:
                return "";
        }
    }

    private void OnDestroy()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveAllListeners();
    }
}