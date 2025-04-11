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
        if (isInitialized && gameObject.activeInHierarchy)
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
            costText.text = $"Cost:\nDRP: {currentUpgrade.CurrentDRPCost:F0}\nQP: {currentUpgrade.CurrentQuantumPotentialCost:F1}";
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
        if (upgradeButton == null || currentUpgrade == null) return;

        bool isMaxLevel = currentUpgrade.CurrentLevel >= currentUpgrade.MaxLevel;
        bool canAfford = currentUpgrade.CanUpgrade(
            FacilityManager.Instance.CurrentDRP,
            FacilityManager.Instance.CurrentQuantumPotential
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
        if (currentUpgrade == null) return;

        FacilityManager.Instance.TryUpgradeFacility(currentUpgrade.UpgradeName);
        UpdateDisplay();
    }

    private void OnDestroy()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveAllListeners();
    }
} 