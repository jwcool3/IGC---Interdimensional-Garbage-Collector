using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Image progressBar;

    [SerializeField] private Color availableColor = Color.green;
    [SerializeField] private Color unavailableColor = Color.red;
    [SerializeField] private Color maxLevelColor = Color.yellow;

    private FacilityUpgrade upgrade;
    private string upgradeType;

    public void SetUpgrade(FacilityUpgrade facilityUpgrade)
    {
        upgrade = facilityUpgrade;
        UpdateUI();
    }

    public void SetUpgradeType(string type)
    {
        upgradeType = type;
    }

    private void Start()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }
    }

    private void Update()
    {
        // Update affordability in real-time
        if (upgrade != null && ResourceManager.Instance != null)
        {
            UpdateAffordability();
        }
    }

    private void UpdateUI()
    {
        if (upgrade == null) return;

        if (nameText != null)
            nameText.text = upgrade.UpgradeName;

        if (descriptionText != null)
            descriptionText.text = upgrade.Description;

        if (levelText != null)
            levelText.text = $"Level: {upgrade.CurrentLevel}/{upgrade.MaxLevel}";

        if (costText != null)
        {
            if (upgrade.CurrentLevel >= upgrade.MaxLevel)
            {
                costText.text = "MAX LEVEL";
            }
            else
            {
                costText.text = $"Cost: {upgrade.CurrentRecyclingPointCost:F0} RP, {upgrade.CurrentDimensionalPotentialCost:F0} DP";
            }
        }

        if (progressBar != null)
        {
            progressBar.fillAmount = (float)upgrade.CurrentLevel / upgrade.MaxLevel;
        }

        UpdateAffordability();
    }

    private void UpdateAffordability()
    {
        if (upgrade == null || upgradeButton == null || ResourceManager.Instance == null) return;

        bool isMaxLevel = upgrade.CurrentLevel >= upgrade.MaxLevel;
        bool canAfford = upgrade.CanAfford(
            ResourceManager.Instance.RecyclingPoints,
            ResourceManager.Instance.DimensionalPotential
        );

        upgradeButton.interactable = !isMaxLevel && canAfford;

        // Update button color
        Image buttonImage = upgradeButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            if (isMaxLevel)
                buttonImage.color = maxLevelColor;
            else if (canAfford)
                buttonImage.color = availableColor;
            else
                buttonImage.color = unavailableColor;
        }
    }

    private void OnUpgradeClicked()
    {
        if (upgrade == null || string.IsNullOrEmpty(upgradeType) || FacilityManager.Instance == null) return;

        FacilityManager.Instance.TryUpgrade(upgradeType);
    }

    private void OnDestroy()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
        }
    }
}