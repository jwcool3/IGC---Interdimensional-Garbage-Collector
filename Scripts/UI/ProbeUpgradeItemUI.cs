using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProbeUpgradeItemUI : MonoBehaviour
{
    // Match your prefab structure
    [SerializeField] private TextMeshProUGUI upgradeName;         // ProbeUpgradeItemPrefab/UpgradeInfo/UpgradeName
    [SerializeField] private TextMeshProUGUI upgradeDescription;  // ProbeUpgradeItemPrefab/UpgradeInfo/UpgradeDescription
    [SerializeField] private TextMeshProUGUI levelText;           // ProbeUpgradeItemPrefab/UpgradeInfo/LevelText
    [SerializeField] private TextMeshProUGUI costText;            // ProbeUpgradeItemPrefab/UpgradeInfo/CostText
    [SerializeField] private Button upgradeButton;                // ProbeUpgradeItemPrefab/ButtonsContainer/UpgradeButton
    [SerializeField] private Image progressBar;                   // ProbeUpgradeItemPrefab/ProgressBar
    
    [SerializeField] private Color canAffordColor = Color.green;
    [SerializeField] private Color cannotAffordColor = Color.gray;
    [SerializeField] private Color maxLevelColor = Color.yellow;
    
    private string upgradeType;
    private float rpCost;
    private float dpCost;
    private int currentLevel;
    private int maxLevel;
    
    private void Awake()
    {
        // Find components if not assigned
        if (upgradeName == null)
            upgradeName = transform.Find("UpgradeInfo/UpgradeName")?.GetComponent<TextMeshProUGUI>();
            
        if (upgradeDescription == null)
            upgradeDescription = transform.Find("UpgradeInfo/UpgradeDescription")?.GetComponent<TextMeshProUGUI>();
            
        if (levelText == null)
            levelText = transform.Find("UpgradeInfo/LevelText")?.GetComponent<TextMeshProUGUI>();
            
        if (costText == null)
            costText = transform.Find("UpgradeInfo/CostText")?.GetComponent<TextMeshProUGUI>();
            
        if (upgradeButton == null)
            upgradeButton = transform.Find("ButtonsContainer/UpgradeButton")?.GetComponent<Button>();
            
        if (progressBar == null)
            progressBar = transform.Find("ProgressBar")?.GetComponent<Image>();
    }
    
    public void Initialize(string type, string name, string description, int curLevel, int maxLvl, float recyclingPointCost, float dimensionalPotentialCost)
    {
        upgradeType = type;
        currentLevel = curLevel;
        maxLevel = maxLvl;
        rpCost = recyclingPointCost;
        dpCost = dimensionalPotentialCost;
        
        if (upgradeName != null)
            upgradeName.text = name;
            
        if (upgradeDescription != null)
            upgradeDescription.text = description;
            
        if (levelText != null)
            levelText.text = $"Level {currentLevel}/{maxLevel}";
            
        if (costText != null)
        {
            if (currentLevel >= maxLevel)
                costText.text = "MAX LEVEL";
            else
                costText.text = $"Cost: {rpCost:F0} RP, {dpCost:F0} DP";
        }
        
        if (progressBar != null)
            progressBar.fillAmount = (float)currentLevel / maxLevel;
            
        // Set up button
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
            UpdateButtonState();
        }
    }
    
    private void Update()
    {
        // Continuously update button state based on resources
        UpdateButtonState();
    }
    
    private void UpdateButtonState()
    {
        if (upgradeButton == null) return;
        
        // At max level
        if (currentLevel >= maxLevel)
        {
            upgradeButton.interactable = false;
            // Using different variable names to avoid scoping issues
            ColorBlock maxLevelBlock = upgradeButton.colors;
            maxLevelBlock.normalColor = maxLevelColor;
            upgradeButton.colors = maxLevelBlock;
            return;
        }
        
        // Check if we can afford it
        bool canAfford = false;
        
        if (ProbeUpgradeManager.Instance != null && ResourceManager.Instance != null)
        {
            canAfford = ResourceManager.Instance.RecyclingPoints >= rpCost &&
                      ResourceManager.Instance.DimensionalPotential >= dpCost;
        }
        
        upgradeButton.interactable = canAfford;
        
        // Update color - using different variable name
        ColorBlock affordBlock = upgradeButton.colors;
        affordBlock.normalColor = canAfford ? canAffordColor : cannotAffordColor;
        upgradeButton.colors = affordBlock;
    }
    
    private void OnUpgradeClicked()
    {
        Debug.Log($"ProbeUpgradeItemUI: Upgrade button clicked for {upgradeType}");
        
        if (ProbeUpgradeManager.Instance == null)
        {
            Debug.LogError("ProbeUpgradeItemUI: ProbeUpgradeManager.Instance is null!");
            return;
        }
        
        if (ProbeUpgradeManager.Instance.PurchaseUpgrade(upgradeType))
        {
            Debug.Log($"ProbeUpgradeItemUI: Successfully purchased {upgradeType} upgrade");
            
            // Update UI after purchase
            currentLevel = ProbeUpgradeManager.Instance.GetUpgradeLevel(upgradeType);
            
            if (levelText != null)
                levelText.text = $"Level {currentLevel}/{maxLevel}";
                
            // Update costs
            rpCost = ProbeUpgradeManager.Instance.GetUpgradeCost(upgradeType, false);
            dpCost = ProbeUpgradeManager.Instance.GetUpgradeCost(upgradeType, true);
            
            if (costText != null)
            {
                if (currentLevel >= maxLevel)
                    costText.text = "MAX LEVEL";
                else
                    costText.text = $"Cost: {rpCost:F0} RP, {dpCost:F0} DP";
            }
            
            if (progressBar != null)
                progressBar.fillAmount = (float)currentLevel / maxLevel;
        }
        else
        {
            Debug.LogWarning($"ProbeUpgradeItemUI: Failed to purchase {upgradeType} upgrade");
        }
    }
    
    private void OnDestroy()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
    }
}