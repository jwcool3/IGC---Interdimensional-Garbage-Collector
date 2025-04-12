using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image progressBar;
    [SerializeField] private Button upgradeButton;

    private FacilityUpgrade upgrade;

    private void Start()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(TryUpgrade);
        }
    }

    public void SetUpgrade(FacilityUpgrade facilityUpgrade)
    {
        upgrade = facilityUpgrade;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (upgrade == null) return;
        
        if (nameText != null)
            nameText.text = upgrade.Name;
            
        if (descriptionText != null)
            descriptionText.text = GetUpgradeDescription();
            
        if (levelText != null)
            levelText.text = $"Level: {upgrade.CurrentLevel}/{upgrade.MaxLevel}";
            
        if (costText != null)
        {
            if (upgrade.IsMaxLevel)
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

    private string GetUpgradeDescription()
    {
        switch (upgrade.Name)
        {
            case "Waste Storage":
                return "Increases waste storage capacity";
            case "Recycling Laboratory":
                return "Improves recycling efficiency";
            case "Stabilization Chamber":
                return "Reduces contamination from waste";
            default:
                return "Unknown upgrade type";
        }
    }

    private void UpdateAffordability()
    {
        if (upgradeButton != null)
        {
            bool canAfford = upgrade.CanAfford(
                ResourceManager.Instance.GetRecyclingPoints(),
                ResourceManager.Instance.GetDimensionalPotential()
            );
            
            upgradeButton.interactable = !upgrade.IsMaxLevel && canAfford;
        }
    }

    private void TryUpgrade()
    {
        if (FacilityManager.Instance != null && upgrade != null)
        {
            if (FacilityManager.Instance.TryUpgrade(upgrade.Name))
            {
                UpdateUI();
            }
        }
    }

    private void OnEnable()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnRecyclingPointsChanged += OnResourcesChanged;
            ResourceManager.Instance.OnDimensionalPotentialChanged += OnResourcesChanged;
        }
    }

    private void OnDisable()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnRecyclingPointsChanged -= OnResourcesChanged;
            ResourceManager.Instance.OnDimensionalPotentialChanged -= OnResourcesChanged;
        }
    }

    private void OnResourcesChanged(float _)
    {
        UpdateAffordability();
    }

    private void OnDestroy()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(TryUpgrade);
        }
    }
} 