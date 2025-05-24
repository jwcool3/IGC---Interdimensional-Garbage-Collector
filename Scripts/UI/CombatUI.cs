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
    public Image playerHPBarFill;  // Reference to the fill part of the HP bar
    public Image enemyHPBarFill;   // Reference to the fill part of the HP bar
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
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private TextMeshProUGUI enemyLevelText;
    [SerializeField] private TextMeshProUGUI enemyTypeText;
    [SerializeField] private Image enemyTypeIcon; // Add dedicated type icon
    
    [Header("Zone Progress")]
    public TextMeshProUGUI zoneNameText;
    public TextMeshProUGUI zoneProgressText;
    public Image zoneIconImage;
    [SerializeField] private TMP_Dropdown zoneSelector;
    
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
    public GameObject laserBeamEffect;
    public GameObject impactEffect;
    public float attackEffectDuration = 0.2f;
    public float impactEffectDuration = 0.3f;
    
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
        // Better dropdown handling
        if (zoneSelector == null)
        {
            // Try to find it first by name
            GameObject selectorObj = GameObject.Find("ZoneSelector");
            if (selectorObj != null)
            {
                zoneSelector = selectorObj.GetComponent<TMP_Dropdown>();
                Debug.Log("Found zone selector by name");
            }
            
            // If still null, try to find by type
            if (zoneSelector == null)
            {
                // Try looking for any dropdown in the combat panel
                if (combatPanel != null)
                {
                    zoneSelector = combatPanel.GetComponentInChildren<TMP_Dropdown>();
                    if (zoneSelector != null)
                    {
                        Debug.Log("Found zone selector in combat panel");
                    }
                }
                
                // If still null, create a simplified zone selection
                if (zoneSelector == null)
                {
                    Debug.LogWarning("Could not find zone selector dropdown. Will use simplified zone navigation.");
                    CreateSimplifiedZoneNavigation();
                }
            }
        }
        
        // Set up button listeners
        if (attackButton != null)
            attackButton.onClick.AddListener(OnAttackButtonClicked);
        else
            Debug.LogError("Attack button reference is missing!");
            
        if (autoToggle != null)
            autoToggle.onValueChanged.AddListener(OnAutoToggleChanged);
        else
            Debug.LogError("Auto toggle reference is missing!");
            
        if (retreatButton != null)
            retreatButton.onClick.AddListener(OnRetreatButtonClicked);
        else
            Debug.LogError("Retreat button reference is missing!");
        
        // Initialize zone selector if found
        if (zoneSelector != null)
        {
            PopulateZoneSelector();
            zoneSelector.onValueChanged.AddListener(OnZoneSelected);
        }
        
        // Hide secondary panels
        if (victoryPanel != null) 
            victoryPanel.SetActive(false);
        else
            Debug.LogWarning("Victory panel reference is missing!");
            
        if (defeatPanel != null) 
            defeatPanel.SetActive(false);
        else
            Debug.LogWarning("Defeat panel reference is missing!");
        
        // Subscribe to resource changes
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourcesChanged += UpdateAllDisplays;
        }
        else
        {
            Debug.LogError("ResourceManager instance not found!");
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
        
        // Update player HP bar using RectTransform
        if (playerHPBarFill != null)
        {
            RectTransform rect = playerHPBarFill.rectTransform;
            rect.anchorMax = new Vector2(playerHPPercent, 1f);
        }
        
        // Update player HP text
        if (playerHPText != null)
            playerHPText.text = $"{Mathf.RoundToInt(CombatManager.Instance.currentHP)}/{Mathf.RoundToInt(CombatManager.Instance.maxHP)}";
        
        // Update enemy HP if there is an enemy
        if (CombatManager.Instance.currentEnemy != null)
        {
            float enemyHPPercent = CombatManager.Instance.currentEnemy.currentHP / CombatManager.Instance.currentEnemy.maxHP;
            
            // Update enemy HP bar using RectTransform
            if (enemyHPBarFill != null)
            {
                RectTransform rect = enemyHPBarFill.rectTransform;
                rect.anchorMax = new Vector2(enemyHPPercent, 1f);
            }
            
            // Update enemy HP text
            if (enemyHPText != null)
                enemyHPText.text = $"{Mathf.RoundToInt(CombatManager.Instance.currentEnemy.currentHP)}/{Mathf.RoundToInt(CombatManager.Instance.currentEnemy.maxHP)}";
        }
        else
        {
            // Reset enemy HP display when no enemy
            if (enemyHPBarFill != null)
            {
                enemyHPBarFill.rectTransform.anchorMax = new Vector2(0f, 1f);
            }
            if (enemyHPText != null)
                enemyHPText.text = "N/A";
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
        if (CombatManager.Instance == null || CombatManager.Instance.currentEnemy == null)
        {
            // Hide enemy display elements when no enemy
            if (enemyShipImage != null) enemyShipImage.enabled = false;
            if (enemyNameText != null) enemyNameText.text = "No Enemy";
            if (enemyLevelText != null) enemyLevelText.text = "";
            if (enemyTypeText != null) enemyTypeText.text = "";
            if (enemyTypeIcon != null) enemyTypeIcon.enabled = false; // Hide type icon
            return;
        }
        
        var enemy = CombatManager.Instance.currentEnemy;
        
        // Update text displays
        if (enemyNameText != null)
            enemyNameText.text = enemy.name;
            
        if (enemyLevelText != null)
            enemyLevelText.text = $"Level: {enemy.level:F1}";
            
        if (enemyTypeText != null)
            enemyTypeText.text = $"Type: {enemy.type}";
        
        // Update type icon
        if (enemyTypeIcon != null && EnemyIconManager.Instance != null)
        {
            Sprite typeIcon = EnemyIconManager.Instance.GetTypeIcon(enemy.type);
            if (typeIcon != null)
            {
                enemyTypeIcon.sprite = typeIcon;
                enemyTypeIcon.enabled = true;
                enemyTypeIcon.color = Color.white;
            }
            else
            {
                // If no type icon, use color instead
                enemyTypeIcon.enabled = true;
                enemyTypeIcon.sprite = null;
                enemyTypeIcon.color = EnemyIconManager.Instance.GetColorForEnemyType(enemy.type);
            }
        }
    
        // Update enemy ship image
        if (enemyShipImage != null)
        {
            Sprite shipIcon = null;
            
            // Approach 1: Try getting ship icon directly by model name if available
            if (!string.IsNullOrEmpty(enemy.shipModelName))
            {
                if (EnemyIconManager.Instance != null)
                {
                    shipIcon = EnemyIconManager.Instance.GetIconForShipModel(enemy.shipModelName);
                }
            }
            
            // Approach 2: If no icon found by model name, try getting by type and sector
            if (shipIcon == null && EnemyIconManager.Instance != null)
            {
                shipIcon = EnemyIconManager.Instance.GetIconForEnemy(
                    enemy.type,
                    enemy.sectorNumber,
                    0  // Default variation
                );
            }
            
            // Set the icon if found
            if (shipIcon != null)
            {
                enemyShipImage.sprite = shipIcon;
                enemyShipImage.color = Color.white;
                enemyShipImage.enabled = true;
            }
            else
            {
                // Use a default shape with color coding as absolute fallback
                enemyShipImage.enabled = true;
                
                if (EnemyIconManager.Instance != null)
                {
                    enemyShipImage.color = EnemyIconManager.Instance.GetColorForEnemyType(enemy.type);
                }
                else
                {
                    // Hardcoded fallback colors if EnemyIconManager is missing
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
            }
        }
    }
    
    public void UpdateZoneDisplay()
    {
        if (CombatManager.Instance == null) return;
        
        var zone = CombatManager.Instance.currentZone;
        var progress = CombatManager.Instance.enemiesDefeatedInZone;
        
        if (zoneNameText != null)
        {
            if (CombatManager.Instance.isSingleShipMode)
            {
                zoneNameText.text = $"⚔️ {zone.zoneName}";
                zoneNameText.color = Color.yellow; // Highlight single ship encounters
            }
            else
            {
                zoneNameText.text = zone.zoneName;
                zoneNameText.color = Color.white;
            }
        }
            
        if (zoneProgressText != null)
        {
            if (CombatManager.Instance.isSingleShipMode)
            {
                zoneProgressText.text = "Single Ship Encounter";
                zoneProgressText.color = Color.yellow;
            }
            else
            {
                zoneProgressText.text = $"{progress}/{zone.enemiesInZone} Enemies";
                zoneProgressText.color = Color.white;
            }
        }
        
        // Update zone icon
        if (zoneIconImage != null)
        {
            Sprite zoneIcon = zone.GetZoneIcon();
            
            if (zoneIcon != null)
            {
                zoneIconImage.sprite = zoneIcon;
                zoneIconImage.color = CombatManager.Instance.isSingleShipMode ? Color.yellow : Color.white;
                zoneIconImage.enabled = true;
            }
            else
            {
                // No icon found, use a color based on sector
                zoneIconImage.enabled = true;
                Color baseColor = GetColorForSector(zone.sectorNumber);
                zoneIconImage.color = CombatManager.Instance.isSingleShipMode ? 
                    Color.Lerp(baseColor, Color.yellow, 0.5f) : baseColor;
            }
        }
    }
    
    private void PopulateZoneSelector()
    {
        if (CombatManager.Instance == null || zoneSelector == null)
        {
            Debug.LogWarning("Cannot populate zone selector - CombatManager or Dropdown not found");
            return;
        }
        
        zoneSelector.ClearOptions();
        
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (var zone in CombatManager.Instance.availableZones)
        {
            string optionText = zone.isLocked ? $"LOCKED: {zone.zoneName}" : zone.zoneName;
            
            // Create option with icon if available
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
            option.text = optionText;
            
            // Try to get icon
            Sprite zoneIcon = zone.GetZoneIcon();
            if (zoneIcon != null)
            {
                option.image = zoneIcon;
            }
            
            options.Add(option);
        }
        
        zoneSelector.AddOptions(options);
        
        // Set current zone as selected
        int currentIndex = CombatManager.Instance.availableZones.IndexOf(CombatManager.Instance.currentZone);
        if (currentIndex >= 0 && currentIndex < options.Count)
        {
            zoneSelector.value = currentIndex;
        }
        else
        {
            Debug.LogWarning($"Invalid zone index: {currentIndex}. Setting to 0.");
            zoneSelector.value = 0;
        }
        
        zoneSelector.RefreshShownValue();
    }
    
    private Color GetColorForSector(int sectorNumber)
    {
        switch (sectorNumber)
        {
            case 1:
                return new Color(0.2f, 0.6f, 1f); // Blue
            case 2:
                return new Color(0.2f, 1f, 0.4f); // Green
            case 3:
                return new Color(1f, 0.6f, 0.2f); // Orange
            case 4:
                return new Color(1f, 0.2f, 0.6f); // Pink
            default:
                return new Color(0.7f, 0.7f, 0.7f); // Gray
        }
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
        if (CombatManager.Instance != null && index >= 0 && 
            index < CombatManager.Instance.availableZones.Count)
        {
            CombatManager.Instance.ChangeZone(index);
        }
        else
        {
            Debug.LogWarning($"Invalid zone index selected: {index}");
        }
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
        if (laserBeamEffect == null || impactEffect == null)
        {
            Debug.LogWarning("Attack effects not assigned!");
            return;
        }
        
        // Get RectTransforms for positioning
        RectTransform beamRect = laserBeamEffect.GetComponent<RectTransform>();
        RectTransform impactRect = impactEffect.GetComponent<RectTransform>();
        
        if (beamRect == null || impactRect == null)
        {
            Debug.LogError("Attack effects must have RectTransform components!");
            return;
        }
        
        if (fromPlayerToEnemy)
        {
            // Player shooting right
            beamRect.anchoredPosition = playerShipImage.rectTransform.anchoredPosition;
            beamRect.anchoredPosition += new Vector2(100, 0); // Offset from player ship
            beamRect.sizeDelta = new Vector2(200, 5); // Width and height of beam
            
            // Position impact at enemy
            impactRect.anchoredPosition = enemyShipImage.rectTransform.anchoredPosition;
            impactRect.anchoredPosition += new Vector2(-50, 0); // Offset to hit enemy ship
        }
        else
        {
            // Enemy shooting left
            beamRect.anchoredPosition = enemyShipImage.rectTransform.anchoredPosition;
            beamRect.anchoredPosition += new Vector2(-100, 0); // Offset from enemy ship
            beamRect.sizeDelta = new Vector2(200, 5); // Width and height of beam
            
            // Position impact at player
            impactRect.anchoredPosition = playerShipImage.rectTransform.anchoredPosition;
            impactRect.anchoredPosition += new Vector2(50, 0); // Offset to hit player ship
        }
        
        // Show effects
        laserBeamEffect.SetActive(true);
        impactEffect.SetActive(true);
        
        // Hide after delay
        StartCoroutine(HideAfterDelay(laserBeamEffect, attackEffectDuration));
        StartCoroutine(HideAfterDelay(impactEffect, impactEffectDuration));
        
        // Play damage effect on target
        ShowDamageEffect(!fromPlayerToEnemy);
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
    
    /// <summary>
    /// Creates a simplified zone navigation system when dropdown is not available
    /// </summary>
    private void CreateSimplifiedZoneNavigation()
    {
        if (combatPanel == null) return;
        
        // Create a horizontal layout group for zone buttons
        GameObject buttonGroup = new GameObject("ZoneButtonGroup");
        buttonGroup.transform.SetParent(combatPanel.transform, false);
        
        // Add layout components
        HorizontalLayoutGroup layout = buttonGroup.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 5, 5);
        layout.childAlignment = TextAnchor.MiddleCenter;
        
        // Position the button group
        RectTransform rect = buttonGroup.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(0, 40);
        
        // Create Previous and Next zone buttons
        CreateZoneButton("Previous Zone", -1, buttonGroup.transform);
        CreateZoneButton("Next Zone", 1, buttonGroup.transform);
    }
    
    /// <summary>
    /// Creates a navigation button for zone selection
    /// </summary>
    private void CreateZoneButton(string text, int direction, Transform parent)
    {
        GameObject buttonObj = new GameObject(text);
        buttonObj.transform.SetParent(parent, false);
        
        // Add button component
        Button button = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f);
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        
        // Set up button rect transform
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(120, 30);
        
        // Set up text rect transform
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        // Add click handler
        button.onClick.AddListener(() => OnZoneNavigationClicked(direction));
    }
    
    /// <summary>
    /// Handles navigation button clicks
    /// </summary>
    private void OnZoneNavigationClicked(int direction)
    {
        if (CombatManager.Instance == null) return;
        
        var zones = CombatManager.Instance.availableZones;
        int currentIndex = zones.IndexOf(CombatManager.Instance.currentZone);
        int newIndex = Mathf.Clamp(currentIndex + direction, 0, zones.Count - 1);
        
        if (newIndex != currentIndex)
        {
            CombatManager.Instance.ChangeZone(newIndex);
        }
    }
}