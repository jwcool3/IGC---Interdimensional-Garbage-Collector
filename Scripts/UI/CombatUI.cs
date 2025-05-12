using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CombatUI : MonoBehaviour
{
    // Singleton instance
    public static CombatUI Instance { get; private set; }
    
    [Header("Main Panels")]
    public GameObject combatPanel;
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    
    [Header("Combat Display")]
    public Image playerShipImage;
    public Image enemyShipImage;
    public Image playerHPBar;
    public Image enemyHPBar;
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI enemyHPText;
    public GameObject attackEffectPrefab;
    
    [Header("Ship Stats")]
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI critText;
    
    [Header("Enemy Info")]
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemyLevelText;
    public TextMeshProUGUI enemyTypeText;
    
    [Header("Zone Progress")]
    public TextMeshProUGUI zoneNameText;
    public TextMeshProUGUI zoneProgressText;
    public Dropdown zoneSelector;
    
    [Header("Controls")]
    public Button attackButton;
    public Toggle autoToggle;
    public Button retreatButton;
    
    [Header("Effects")]
    public GameObject criticalHitEffect;
    public GameObject victoryEffect;
    public GameObject defeatEffect;
    
    [Header("Resource Display")]
    [SerializeField] private TextMeshProUGUI shipPartsText;
    [SerializeField] private TextMeshProUGUI alienTechText;
    [SerializeField] private TextMeshProUGUI combatDataText;
    
    [Header("Combat Effects")]
    public GameObject attackEffectLine;
    public float attackEffectDuration = 0.2f;
    
    private void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Set up button listeners
        attackButton.onClick.AddListener(OnAttackButtonClicked);
        autoToggle.onValueChanged.AddListener(OnAutoToggleChanged);
        retreatButton.onClick.AddListener(OnRetreatButtonClicked);
        
        // Initialize zone selector
        PopulateZoneSelector();
        zoneSelector.onValueChanged.AddListener(OnZoneSelected);
        
        // Hide secondary panels
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        
        // Subscribe to resource changes
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourcesChanged += UpdateAllDisplays;
        }
        else
        {
            Debug.LogError("CombatUI: ResourceManager instance not found!");
        }
        
        // Initial update
        UpdateAllDisplays();
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourcesChanged -= UpdateAllDisplays;
        }
    }
    
    /// <summary>
    /// Update all UI displays
    /// </summary>
    public void UpdateAllDisplays()
    {
        UpdateCombatDisplay();
        UpdateStatsDisplay();
        UpdateEnemyDisplay();
        UpdateZoneDisplay();
        UpdateResourceDisplay();
    }
    
    public void UpdateCombatDisplay()
    {
        if (CombatManager.Instance == null) return;
        
        // Update player HP
        float playerHPPercent = CombatManager.Instance.currentHP / CombatManager.Instance.maxHP;
        playerHPBar.fillAmount = playerHPPercent;
        playerHPText.text = $"{Mathf.RoundToInt(CombatManager.Instance.currentHP)}/{Mathf.RoundToInt(CombatManager.Instance.maxHP)}";
        
        // Update enemy HP if there is an enemy
        if (CombatManager.Instance.currentEnemy != null)
        {
            float enemyHPPercent = CombatManager.Instance.currentEnemy.currentHP / CombatManager.Instance.currentEnemy.maxHP;
            enemyHPBar.fillAmount = enemyHPPercent;
            enemyHPText.text = $"{Mathf.RoundToInt(CombatManager.Instance.currentEnemy.currentHP)}/{Mathf.RoundToInt(CombatManager.Instance.currentEnemy.maxHP)}";
        }
    }
    
    public void UpdateStatsDisplay()
    {
        if (CombatManager.Instance == null) return;
        
        attackText.text = $"Attack: {CombatManager.Instance.attackPower:F1}";
        defenseText.text = $"Defense: {CombatManager.Instance.defense:F1}";
        hpText.text = $"HP: {CombatManager.Instance.currentHP:F0}/{CombatManager.Instance.maxHP:F0}";
        speedText.text = $"Speed: {CombatManager.Instance.attackSpeed:F1}x";
        critText.text = $"Crit: {CombatManager.Instance.criticalChance * 100:F1}%";
    }
    
    public void UpdateEnemyDisplay()
    {
        if (CombatManager.Instance == null || CombatManager.Instance.currentEnemy == null) return;
        
        var enemy = CombatManager.Instance.currentEnemy;
        
        enemyNameText.text = enemy.name;
        enemyLevelText.text = $"Level: {enemy.level:F1}";
        enemyTypeText.text = $"Type: {enemy.type}";
        
        // Update enemy sprite based on type
        switch (enemy.type)
        {
            case EnemyType.Scavenger:
                enemyShipImage.color = new Color(0.5f, 0.5f, 0.5f); // Gray
                break;
                
            case EnemyType.Rival:
                enemyShipImage.color = new Color(0.2f, 0.6f, 1f); // Blue
                break;
                
            case EnemyType.Anomaly:
                enemyShipImage.color = new Color(1f, 0.4f, 0.8f); // Pink
                break;
                
            case EnemyType.Boss:
                enemyShipImage.color = new Color(1f, 0.2f, 0.2f); // Red
                break;
        }
    }
    
    public void UpdateZoneDisplay()
    {
        if (CombatManager.Instance == null) return;
        
        var zone = CombatManager.Instance.currentZone;
        var progress = CombatManager.Instance.enemiesDefeatedInZone;
        
        zoneNameText.text = zone.zoneName;
        zoneProgressText.text = $"{progress}/{zone.enemiesInZone} Enemies";
    }
    
    private void PopulateZoneSelector()
    {
        if (CombatManager.Instance == null) return;
        
        zoneSelector.ClearOptions();
        
        List<string> options = new List<string>();
        foreach (var zone in CombatManager.Instance.availableZones)
        {
            string option = zone.isLocked ? $"LOCKED: {zone.zoneName}" : zone.zoneName;
            options.Add(option);
        }
        
        zoneSelector.AddOptions(options);
        
        // Set current zone as selected
        int currentIndex = CombatManager.Instance.availableZones.IndexOf(CombatManager.Instance.currentZone);
        zoneSelector.value = currentIndex;
    }
    
    // UI Callbacks
    private void OnAttackButtonClicked()
    {
        CombatManager.Instance?.ManualAttack();
    }
    
    private void OnAutoToggleChanged(bool isOn)
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.autoCombatEnabled = isOn;
        }
    }
    
    private void OnRetreatButtonClicked()
    {
        // Return to first zone
        CombatManager.Instance?.ChangeZone(0);
    }
    
    private void OnZoneSelected(int index)
    {
        CombatManager.Instance?.ChangeZone(index);
    }
    
    // Effects and Notifications
    public void ShowCriticalHit()
    {
        if (criticalHitEffect != null)
        {
            criticalHitEffect.SetActive(true);
            StartCoroutine(HideAfterDelay(criticalHitEffect, 0.5f));
        }
    }
    
    public void ShowVictoryEffect()
    {
        if (victoryEffect != null)
        {
            victoryEffect.SetActive(true);
            StartCoroutine(HideAfterDelay(victoryEffect, 1f));
        }
    }
    
    public void ShowDefeat()
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
            
            // Auto hide after delay
            StartCoroutine(HideAfterDelay(defeatPanel, 3f));
        }
        
        if (defeatEffect != null)
        {
            defeatEffect.SetActive(true);
            StartCoroutine(HideAfterDelay(defeatEffect, 2f));
        }
    }
    
    public void ShowZoneComplete(string zoneName)
    {
        // Update zone dropdown as zones may have been unlocked
        PopulateZoneSelector();
        
        // Show victory panel with zone info
        if (victoryPanel != null)
        {
            TextMeshProUGUI victoryText = victoryPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (victoryText != null)
            {
                victoryText.text = $"Zone Completed: {zoneName}!";
            }
            
            victoryPanel.SetActive(true);
            StartCoroutine(HideAfterDelay(victoryPanel, 3f));
        }
    }
    
    public void UpdateAutoButtonState(bool isEnabled)
    {
        if (autoToggle != null)
        {
            autoToggle.isOn = isEnabled;
        }
    }
    
    private IEnumerator HideAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            obj.SetActive(false);
        }
    }
    
    /// <summary>
    /// Update the resource display text
    /// </summary>
    public void UpdateResourceDisplay()
    {
        if (ResourceManager.Instance == null)
        {
            Debug.LogWarning("Cannot update resource display - ResourceManager not found");
            return;
        }

        if (shipPartsText != null)
            shipPartsText.text = $"Ship Parts: {ResourceManager.Instance.ShipParts:F0}";
            
        if (alienTechText != null)
            alienTechText.text = $"Alien Tech: {ResourceManager.Instance.AlienTech:F0}";
            
        if (combatDataText != null)
            combatDataText.text = $"Combat Data: {ResourceManager.Instance.CombatData:F0}";
    }
    
    /// <summary>
    /// Show attack effect animation
    /// </summary>
    public void ShowAttackEffect(bool fromPlayerToEnemy)
    {
        if (attackEffectLine == null)
        {
            Debug.LogWarning("Attack effect line is not assigned!");
            return;
        }
        
        // Set the line direction based on who's attacking
        RectTransform rect = attackEffectLine.GetComponent<RectTransform>();
        if (rect == null)
        {
            Debug.LogError("Attack effect line must have a RectTransform component!");
            return;
        }
        
        if (fromPlayerToEnemy)
        {
            rect.anchoredPosition = playerShipImage.rectTransform.anchoredPosition;
            rect.anchoredPosition += Vector2.right * 100; // Offset to start from front of ship
        }
        else
        {
            rect.anchoredPosition = enemyShipImage.rectTransform.anchoredPosition;
            rect.anchoredPosition += Vector2.left * 100; // Offset to start from front of ship
        }
        
        // Show the effect briefly
        attackEffectLine.gameObject.SetActive(true);
        StartCoroutine(HideAfterDelay(attackEffectLine.gameObject, attackEffectDuration));
    }
    
    /// <summary>
    /// Play a quick flash effect on a ship
    /// </summary>
    public void ShowDamageEffect(bool onPlayer)
    {
        Image targetImage = onPlayer ? playerShipImage : enemyShipImage;
        if (targetImage == null) return;
        
        StartCoroutine(FlashEffect(targetImage));
    }
    
    private IEnumerator FlashEffect(Image image)
    {
        Color originalColor = image.color;
        image.color = Color.red;
        
        yield return new WaitForSeconds(0.1f);
        
        image.color = originalColor;
    }
}