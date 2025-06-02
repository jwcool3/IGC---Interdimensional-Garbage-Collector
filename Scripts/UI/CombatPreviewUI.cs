using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Reusable combat preview panel that can be used in Scanner and Contacts tabs
/// Shows player vs enemy comparison and combat predictions
/// </summary>
public class CombatPreviewUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject previewPanel;
    [SerializeField] private Button engageCombatButton;
    [SerializeField] private Button cancelButton;
    
    [Header("Ship Comparison")]
    [SerializeField] private Image playerShipImage;
    [SerializeField] private Image enemyShipImage;
    [SerializeField] private TextMeshProUGUI playerShipName;
    [SerializeField] private TextMeshProUGUI enemyShipName;
    
    [Header("Stat Comparison")]
    [SerializeField] private TextMeshProUGUI playerAttackText;
    [SerializeField] private TextMeshProUGUI playerDefenseText;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI enemyAttackText;
    [SerializeField] private TextMeshProUGUI enemyDefenseText;
    [SerializeField] private TextMeshProUGUI enemyHealthText;
    
    [Header("Combat Prediction")]
    [SerializeField] private Image winChanceBar;
    [SerializeField] private TextMeshProUGUI winChanceText;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private Image difficultyIndicator;
    
    [Header("Rewards Preview")]
    [SerializeField] private TextMeshProUGUI rewardsText;
    [SerializeField] private GameObject rewardsContainer;
    
    [Header("Visual Styling")]
    [SerializeField] private Color advantageColor = Color.green;
    [SerializeField] private Color disadvantageColor = Color.red;
    [SerializeField] private Color neutralColor = Color.white;
    
    // Current ship being previewed
    private DiscoveredShip currentShip;
    
    // Events
    public System.Action<DiscoveredShip> OnCombatConfirmed;
    public System.Action OnCombatCancelled;
    
    private void Start()
    {
        // Set up button listeners
        if (engageCombatButton != null)
            engageCombatButton.onClick.AddListener(ConfirmCombat);
            
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelCombat);
        
        // Hide panel initially
        HidePreview();
    }
    
    /// <summary>
    /// Show combat preview for a specific ship
    /// </summary>
    public void ShowCombatPreview(DiscoveredShip ship)
    {
        if (ship == null || !ship.CanFight)
        {
            Debug.LogWarning("CombatPreviewUI: Cannot show preview for this ship");
            return;
        }
        
        currentShip = ship;
        
        // Show the panel
        if (previewPanel != null)
            previewPanel.SetActive(true);
        
        // Update ship information
        UpdateShipDisplay();
        
        // Update stat comparison
        UpdateStatComparison();
        
        // Calculate and show combat prediction
        UpdateCombatPrediction();
        
        // Show potential rewards
        UpdateRewardsPreview();
        
        Debug.Log($"CombatPreviewUI: Showing preview for {ship.ShipName}");
    }
    
    /// <summary>
    /// Hide the combat preview panel
    /// </summary>
    public void HidePreview()
    {
        if (previewPanel != null)
            previewPanel.SetActive(false);
            
        currentShip = null;
    }
    
    /// <summary>
    /// Update ship display information
    /// </summary>
    private void UpdateShipDisplay()
    {
        // Player ship info
        if (playerShipName != null)
            playerShipName.text = "Your Ship";
            
        if (playerShipImage != null)
        {
            // You can set a player ship sprite here
            playerShipImage.color = Color.cyan;
        }
        
        // Enemy ship info
        if (enemyShipName != null)
            enemyShipName.text = currentShip.ShipName;
            
        if (enemyShipImage != null)
        {
            if (currentShip.ShipIcon != null)
            {
                enemyShipImage.sprite = currentShip.ShipIcon;
                enemyShipImage.color = Color.white;
            }
            else
            {
                enemyShipImage.color = currentShip.GetRarityColor();
            }
        }
    }
    
    /// <summary>
    /// Update stat comparison display
    /// </summary>
    private void UpdateStatComparison()
    {
        if (CombatManager.Instance == null) return;
        
        // Get player stats
        float playerAttack = CombatManager.Instance.attackPower;
        float playerDefense = CombatManager.Instance.defense;
        float playerHealth = CombatManager.Instance.currentHP;
        
        // Get enemy stats
        float enemyAttack = currentShip.AttackPower;
        float enemyDefense = currentShip.Defense;
        float enemyHealth = currentShip.CurrentHealth;
        
        // Update player stats display
        if (playerAttackText != null)
        {
            playerAttackText.text = $"Attack: {playerAttack:F0}";
            playerAttackText.color = GetComparisonColor(playerAttack, enemyAttack);
        }
        
        if (playerDefenseText != null)
        {
            playerDefenseText.text = $"Defense: {playerDefense:F0}";
            playerDefenseText.color = GetComparisonColor(playerDefense, enemyDefense);
        }
        
        if (playerHealthText != null)
        {
            playerHealthText.text = $"Health: {playerHealth:F0}";
            playerHealthText.color = GetComparisonColor(playerHealth, enemyHealth);
        }
        
        // Update enemy stats display
        if (enemyAttackText != null)
        {
            enemyAttackText.text = $"Attack: {enemyAttack:F0}";
            enemyAttackText.color = GetComparisonColor(enemyAttack, playerAttack);
        }
        
        if (enemyDefenseText != null)
        {
            enemyDefenseText.text = $"Defense: {enemyDefense:F0}";
            enemyDefenseText.color = GetComparisonColor(enemyDefense, playerDefense);
        }
        
        if (enemyHealthText != null)
        {
            enemyHealthText.text = $"Health: {enemyHealth:F0}";
            enemyHealthText.color = GetComparisonColor(enemyHealth, playerHealth);
        }
    }
    
    /// <summary>
    /// Calculate and display combat outcome prediction
    /// </summary>
    private void UpdateCombatPrediction()
    {
        if (CombatManager.Instance == null) return;
        
        // Simple combat simulation
        float winChance = CalculateWinChance();
        
        // Update win chance display
        if (winChanceBar != null)
        {
            winChanceBar.fillAmount = winChance;
            winChanceBar.color = Color.Lerp(disadvantageColor, advantageColor, winChance);
        }
        
        if (winChanceText != null)
        {
            winChanceText.text = $"Win Chance: {winChance:P0}";
            winChanceText.color = Color.Lerp(disadvantageColor, advantageColor, winChance);
        }
        
        // Update difficulty indicator
        string difficulty = GetDifficultyText(winChance);
        if (difficultyText != null)
        {
            difficultyText.text = $"Difficulty: {difficulty}";
            difficultyText.color = GetDifficultyColor(winChance);
        }
        
        if (difficultyIndicator != null)
        {
            difficultyIndicator.color = GetDifficultyColor(winChance);
        }
    }
    
    /// <summary>
    /// Calculate estimated win chance based on stats
    /// </summary>
    private float CalculateWinChance()
    {
        if (CombatManager.Instance == null) return 0.5f;
        
        // Get comparative stats
        float playerPower = CombatManager.Instance.attackPower + CombatManager.Instance.defense + (CombatManager.Instance.currentHP * 0.1f);
        float enemyPower = currentShip.AttackPower + currentShip.Defense + (currentShip.CurrentHealth * 0.1f);
        
        // Calculate ratio
        float ratio = playerPower / (playerPower + enemyPower);
        
        // Add some randomness factor
        ratio = Mathf.Clamp(ratio * 0.8f + 0.1f, 0.05f, 0.95f);
        
        return ratio;
    }
    
    /// <summary>
    /// Get difficulty text based on win chance
    /// </summary>
    private string GetDifficultyText(float winChance)
    {
        if (winChance >= 0.8f) return "Very Easy";
        if (winChance >= 0.6f) return "Easy";
        if (winChance >= 0.4f) return "Moderate";
        if (winChance >= 0.2f) return "Hard";
        return "Very Hard";
    }
    
    /// <summary>
    /// Get color based on difficulty
    /// </summary>
    private Color GetDifficultyColor(float winChance)
    {
        if (winChance >= 0.7f) return Color.green;
        if (winChance >= 0.5f) return Color.yellow;
        if (winChance >= 0.3f) return Color.red;
        return new Color(0.8f, 0f, 0f); // Dark red
    }
    
    /// <summary>
    /// Get comparison color for stats
    /// </summary>
    private Color GetComparisonColor(float playerStat, float enemyStat)
    {
        if (playerStat > enemyStat * 1.2f) return advantageColor;
        if (playerStat < enemyStat * 0.8f) return disadvantageColor;
        return neutralColor;
    }
    
    /// <summary>
    /// Update rewards preview
    /// </summary>
    private void UpdateRewardsPreview()
    {
        if (rewardsText != null && currentShip != null)
        {
            // Calculate potential rewards
            int shipParts = CalculatePotentialShipParts();
            int alienTech = CalculatePotentialAlienTech();
            float recyclingPoints = CalculatePotentialRP();
            
            string rewardsString = $"Potential Rewards:\n";
            rewardsString += $"• Ship Parts: {shipParts}\n";
            if (alienTech > 0)
                rewardsString += $"• Alien Tech: {alienTech}\n";
            rewardsString += $"• Recycling Points: {recyclingPoints:F0}";
            
            rewardsText.text = rewardsString;
        }
        
        if (rewardsContainer != null)
            rewardsContainer.SetActive(true);
    }
    
    /// <summary>
    /// Calculate potential ship parts reward
    /// </summary>
    private int CalculatePotentialShipParts()
    {
        int baseReward = 15;
        int rarityMultiplier = (int)currentShip.Rarity + 1;
        int levelBonus = currentShip.Level * 2;
        return baseReward * rarityMultiplier + levelBonus;
    }
    
    /// <summary>
    /// Calculate potential alien tech reward
    /// </summary>
    private int CalculatePotentialAlienTech()
    {
        if (currentShip.IsRareCategory())
        {
            int baseReward = 5;
            int rarityMultiplier = (int)currentShip.Rarity + 1;
            return baseReward * rarityMultiplier;
        }
        return 0;
    }
    
    /// <summary>
    /// Calculate potential recycling points reward
    /// </summary>
    private float CalculatePotentialRP()
    {
        float baseReward = 25f;
        float rarityMultiplier = (int)currentShip.Rarity + 1;
        float levelBonus = currentShip.Level * 3f;
        return baseReward * rarityMultiplier + levelBonus;
    }
    
    /// <summary>
    /// Handle combat confirmation
    /// </summary>
    private void ConfirmCombat()
    {
        if (currentShip != null)
        {
            Debug.Log($"CombatPreviewUI: Combat confirmed for {currentShip.ShipName}");
            OnCombatConfirmed?.Invoke(currentShip);
            HidePreview();
        }
    }
    
    /// <summary>
    /// Handle combat cancellation
    /// </summary>
    private void CancelCombat()
    {
        Debug.Log("CombatPreviewUI: Combat cancelled");
        OnCombatCancelled?.Invoke();
        HidePreview();
    }
    
    /// <summary>
    /// Public method to check if preview is currently showing
    /// </summary>
    public bool IsPreviewActive()
    {
        return previewPanel != null && previewPanel.activeSelf;
    }
    
    /// <summary>
    /// Get the currently previewed ship
    /// </summary>
    public DiscoveredShip GetCurrentShip()
    {
        return currentShip;
    }
    
    private void OnDestroy()
    {
        // Clean up button listeners
        if (engageCombatButton != null)
            engageCombatButton.onClick.RemoveListener(ConfirmCombat);
            
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(CancelCombat);
    }
}