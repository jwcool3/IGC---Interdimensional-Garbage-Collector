using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Enhanced Scanner UI with combat preview integration
/// </summary>
public class ScannerUI : MonoBehaviour
{
    [Header("Energy Display")]
    [SerializeField] private Slider energySlider;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI energyRegenText;
    
    [Header("Scan Buttons")]
    [SerializeField] private Button commonScanButton;
    [SerializeField] private Button rareScanButton;
    [SerializeField] private TextMeshProUGUI commonScanCostText;
    [SerializeField] private TextMeshProUGUI rareScanCostText;
    [SerializeField] private TextMeshProUGUI commonSuccessRateText;
    [SerializeField] private TextMeshProUGUI rareSuccessRateText;
    
    [Header("Scan Status")]
    [SerializeField] private GameObject scanningIndicator;
    [SerializeField] private TextMeshProUGUI scanStatusText;
    [SerializeField] private Slider scanProgressSlider;
    
    [Header("Results Display")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button viewContactsButton;
    [SerializeField] private Button fightButton;          // Immediate combat (green)
    [SerializeField] private Button analyzeButton;        // Combat preview first (blue)
    
    [Header("Ship Image Display")]
    [SerializeField] private Image discoveredShipImage;
    [SerializeField] private GameObject shipImageContainer;
    [SerializeField] private TextMeshProUGUI shipImageLabel;
    [SerializeField] private Image rarityBorder;
    
    [Header("Combat Integration")]
    [SerializeField] private CombatPreviewUI combatPreview;
    [SerializeField] private Button previewCombatButton;
    
    [Header("Scanner Info")]
    [SerializeField] private TextMeshProUGUI scannerLevelText;
    [SerializeField] private TextMeshProUGUI locationBonusText;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject scanningEffect;
    [SerializeField] private GameObject successEffect;
    [SerializeField] private GameObject failureEffect;
    
    [Header("Fallback Ship Images")]
    [SerializeField] private Sprite defaultShipIcon;
    [SerializeField] private Sprite[] rarityBasedIcons = new Sprite[7];
    
    private Coroutine scanProgressCoroutine;
    
    private void Start()
    {
        Debug.Log("ScannerUI: Starting initialization");
        
        SetupButtonListeners();
        SubscribeToScannerEvents();
        SetupCombatPreview();
        UpdateAllDisplays();
        HideUIElements();
        
        Debug.Log("ScannerUI: Initialization complete");
    }
    
    private void SetupButtonListeners()
    {
        if (commonScanButton != null)
            commonScanButton.onClick.AddListener(OnCommonScanClicked);
            
        if (rareScanButton != null)
            rareScanButton.onClick.AddListener(OnRareScanClicked);
            
        if (viewContactsButton != null)
            viewContactsButton.onClick.AddListener(OnViewContactsClicked);
            
        if (fightButton != null)
            fightButton.onClick.AddListener(OnFightButtonClicked);
            
        if (analyzeButton != null)
            analyzeButton.onClick.AddListener(OnAnalyzeButtonClicked);
            
        if (previewCombatButton != null)
            previewCombatButton.onClick.AddListener(OnAnalyzeButtonClicked); // Same as analyze
    }
    
    private void SetupCombatPreview()
    {
        if (combatPreview != null)
        {
            combatPreview.OnCombatConfirmed += OnCombatConfirmed;
            combatPreview.OnCombatCancelled += OnCombatCancelled;
        }
        else
        {
            Debug.LogWarning("ScannerUI: Combat preview component not assigned!");
        }
    }
    
    private void SubscribeToScannerEvents()
    {
        if (ShipScanner.Instance != null)
        {
            ShipScanner.Instance.OnEnergyChanged += UpdateEnergyDisplay;
            ShipScanner.Instance.OnScanStarted += OnScanStarted;
            ShipScanner.Instance.OnScanCompleted += OnScanCompleted;
            ShipScanner.Instance.OnShipDiscovered += OnShipDiscovered;
            ShipScanner.Instance.OnScanFailed += OnScanFailed;
            ShipScanner.Instance.OnShipInteracted += OnShipInteracted;
            
            Debug.Log("ScannerUI: Subscribed to scanner events");
        }
        else
        {
            Debug.LogError("ScannerUI: ShipScanner instance not found!");
        }
    }
    
    private void HideUIElements()
    {
        if (scanningIndicator != null)
            scanningIndicator.SetActive(false);
            
        if (resultPanel != null)
            resultPanel.SetActive(false);
            
        if (shipImageContainer != null)
            shipImageContainer.SetActive(false);
    }
    
    /// <summary>
    /// Update all UI displays
    /// </summary>
    public void UpdateAllDisplays()
    {
        UpdateEnergyDisplay(ShipScanner.Instance?.CurrentEnergy ?? 0);
        UpdateScannerInfo();
        UpdateButtonStates();
        UpdateScanCosts();
        UpdateSuccessRates();
        UpdateShipDisplay();
    }
    
    /// <summary>
    /// Update ship display and action buttons
    /// </summary>
    private void UpdateShipDisplay()
    {
        if (ShipScanner.Instance?.HasDiscoveredShip == true)
        {
            ShowShipImage(ShipScanner.Instance.CurrentShip);
            UpdateActionButtons(ShipScanner.Instance.CurrentShip);
        }
        else
        {
            HideShipImage();
            HideActionButtons();
        }
    }
    
    /// <summary>
    /// Update action buttons based on ship status
    /// </summary>
    private void UpdateActionButtons(DiscoveredShip ship)
    {
        if (ship == null)
        {
            HideActionButtons();
            return;
        }
        
        bool canFight = ship.CanFight;
        
        // Fight button - immediate combat
        if (fightButton != null)
        {
            fightButton.interactable = canFight;
            fightButton.gameObject.SetActive(true);
            
            var fightText = fightButton.GetComponentInChildren<TextMeshProUGUI>();
            if (fightText != null)
            {
                if (ship.IsDestroyed)
                    fightText.text = "DESTROYED";
                else if (ship.HasFought)
                    fightText.text = "FOUGHT";
                else
                    fightText.text = "FIGHT";
            }
            
            // Color coding for difficulty
            var fightImage = fightButton.GetComponent<Image>();
            if (fightImage != null && canFight)
            {
                float winChance = CalculateQuickWinChance(ship);
                if (winChance >= 0.7f)
                    fightImage.color = Color.green;      // Easy fight
                else if (winChance >= 0.4f)
                    fightImage.color = Color.yellow;     // Moderate fight
                else
                    fightImage.color = Color.red;        // Hard fight
            }
        }
        
        // Analyze button - combat preview first
        if (analyzeButton != null)
        {
            analyzeButton.interactable = canFight;
            analyzeButton.gameObject.SetActive(true);
            
            var analyzeText = analyzeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (analyzeText != null)
            {
                if (ship.IsDestroyed)
                    analyzeText.text = "DESTROYED";
                else if (ship.HasFought)
                    analyzeText.text = "FOUGHT";
                else
                    analyzeText.text = "ANALYZE";
            }
            
            // Keep analyze button blue
            var analyzeImage = analyzeButton.GetComponent<Image>();
            if (analyzeImage != null && canFight)
            {
                analyzeImage.color = new Color(0.2f, 0.6f, 1f); // Blue
            }
        }
        
        // View contacts button
        if (viewContactsButton != null)
        {
            viewContactsButton.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide action buttons when no ship
    /// </summary>
    private void HideActionButtons()
    {
        if (fightButton != null)
            fightButton.gameObject.SetActive(false);
            
        if (analyzeButton != null)
            analyzeButton.gameObject.SetActive(false);
            
        if (viewContactsButton != null)
            viewContactsButton.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Quick win chance calculation for button coloring
    /// </summary>
    private float CalculateQuickWinChance(DiscoveredShip ship)
    {
        if (CombatManager.Instance == null) return 0.5f;
        
        float playerPower = CombatManager.Instance.attackPower + CombatManager.Instance.defense + (CombatManager.Instance.currentHP * 0.1f);
        float enemyPower = ship.AttackPower + ship.Defense + (ship.CurrentHealth * 0.1f);
        
        return playerPower / (playerPower + enemyPower);
    }
    
    /// <summary>
    /// Show the discovered ship's image and info
    /// </summary>
    private void ShowShipImage(DiscoveredShip ship)
    {
        if (ship == null) 
        {
            Debug.LogWarning("ScannerUI: Attempted to show ship image with null ship");
            return;
        }
        
        Debug.Log($"ScannerUI: Showing ship image for {ship.ShipName}");
        
        // Show the ship image container
        if (shipImageContainer != null)
            shipImageContainer.SetActive(true);
        
        // Update ship image
        if (discoveredShipImage != null)
        {
            Sprite shipSprite = GetShipSprite(ship);
            
            if (shipSprite != null)
            {
                discoveredShipImage.sprite = shipSprite;
                discoveredShipImage.color = Color.white;
                discoveredShipImage.enabled = true;
            }
            else
            {
                discoveredShipImage.sprite = GetFallbackSprite(ship.Rarity);
                discoveredShipImage.color = ship.GetRarityColor();
                discoveredShipImage.enabled = true;
            }
        }
        
        // Update ship image label
        if (shipImageLabel != null)
        {
            shipImageLabel.text = $"{ship.ShipName}\n{ship.GetRarityDisplayText()}\nLevel {ship.Level}";
            shipImageLabel.color = ship.GetRarityColor();
        }
        
        // Update rarity border
        if (rarityBorder != null)
        {
            rarityBorder.color = ship.GetRarityColor();
            rarityBorder.enabled = true;
        }
    }
    
    /// <summary>
    /// Hide the ship image display
    /// </summary>
    private void HideShipImage()
    {
        if (shipImageContainer != null)
            shipImageContainer.SetActive(false);
        
        if (discoveredShipImage != null)
            discoveredShipImage.enabled = false;
        
        if (rarityBorder != null)
            rarityBorder.enabled = false;
    }
    
    // ===== BUTTON EVENT HANDLERS =====
    
    /// <summary>
    /// Handle fight button - immediate combat without preview
    /// </summary>
    private void OnFightButtonClicked()
    {
        if (ShipScanner.Instance?.CurrentShip == null)
        {
            Debug.LogWarning("ScannerUI: No ship available for fight!");
            return;
        }
        
        var ship = ShipScanner.Instance.CurrentShip;
        if (!ship.CanFight)
        {
            ShowScanStatus("Cannot fight this ship", Color.red);
            return;
        }
        
        Debug.Log($"ScannerUI: Immediate fight with {ship.ShipName}");
        StartCombatWithShip(ship);
    }
    
    /// <summary>
    /// Handle analyze button - shows combat preview first
    /// </summary>
    private void OnAnalyzeButtonClicked()
    {
        if (ShipScanner.Instance?.CurrentShip == null)
        {
            Debug.LogWarning("ScannerUI: No ship available for analysis!");
            return;
        }
        
        var ship = ShipScanner.Instance.CurrentShip;
        if (!ship.CanFight)
        {
            ShowScanStatus("Cannot fight this ship", Color.red);
            return;
        }
        
        Debug.Log($"ScannerUI: Analyzing combat with {ship.ShipName}");
        
        if (combatPreview != null)
        {
            combatPreview.ShowCombatPreview(ship);
        }
        else
        {
            Debug.LogError("ScannerUI: Combat preview component not found!");
            // Fallback to immediate fight
            StartCombatWithShip(ship);
        }
    }
    
    /// <summary>
    /// Handle combat confirmation from preview
    /// </summary>
    private void OnCombatConfirmed(DiscoveredShip ship)
    {
        Debug.Log($"ScannerUI: Combat confirmed for {ship.ShipName}");
        StartCombatWithShip(ship);
    }
    
    /// <summary>
    /// Handle combat cancellation from preview
    /// </summary>
    private void OnCombatCancelled()
    {
        Debug.Log("ScannerUI: Combat cancelled by user");
        ShowScanStatus("Combat cancelled", Color.yellow);
    }
    
    /// <summary>
    /// Actually start combat with the ship
    /// </summary>
    private void StartCombatWithShip(DiscoveredShip ship)
    {
        if (ShipInteractionManager.Instance != null)
        {
            bool success = ShipInteractionManager.Instance.StartCombatWithShip(ship);
            if (success)
            {
                ShowScanStatus("Entering combat...", Color.red);
                Debug.Log($"ScannerUI: Combat started with {ship.ShipName}");
                
                // Switch to combat tab after a brief delay
                StartCoroutine(SwitchToCombatTabDelayed(1f));
            }
            else
            {
                ShowScanStatus("Failed to start combat", Color.red);
                Debug.LogWarning("ScannerUI: Failed to start combat");
            }
        }
        else
        {
            ShowScanStatus("Combat system unavailable", Color.red);
            Debug.LogError("ScannerUI: ShipInteractionManager not found!");
        }
    }
    
    /// <summary>
    /// Switch to combat tab after a delay
    /// </summary>
    private IEnumerator SwitchToCombatTabDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        TabSystem tabSystem = FindObjectOfType<TabSystem>();
        if (tabSystem != null)
        {
            tabSystem.ShowCombatTab();
            Debug.Log("ScannerUI: Switched to combat tab");
        }
        else
        {
            Debug.LogWarning("ScannerUI: TabSystem not found for tab switching");
        }
    }
    
    /// <summary>
    /// Handle common scan button click
    /// </summary>
    private void OnCommonScanClicked()
    {
        Debug.Log("ScannerUI: Common scan button clicked");
        
        if (ShipScanner.Instance != null)
        {
            bool success = ShipScanner.Instance.TryScanForCommonShip();
            if (!success)
            {
                ShowScanStatus("Cannot start scan", Color.red);
            }
        }
    }
    
    /// <summary>
    /// Handle rare scan button click
    /// </summary>
    private void OnRareScanClicked()
    {
        Debug.Log("ScannerUI: Rare scan button clicked");
        
        if (ShipScanner.Instance != null)
        {
            bool success = ShipScanner.Instance.TryScanForRareShip();
            if (!success)
            {
                ShowScanStatus("Cannot start scan", Color.red);
            }
        }
    }
    
    /// <summary>
    /// Handle view contacts button click
    /// </summary>
    private void OnViewContactsClicked()
    {
        Debug.Log("ScannerUI: View contacts clicked - switching to contacts tab");
        
        TabSystem tabSystem = FindObjectOfType<TabSystem>();
        if (tabSystem != null)
        {
            tabSystem.ShowContactsTab();
        }
        else
        {
            ShowScanStatus("Contacts tab unavailable", Color.yellow);
        }
    }
    
    // ===== SCANNER EVENT HANDLERS =====
    
    /// <summary>
    /// Handle scan started event
    /// </summary>
    private void OnScanStarted()
    {
        Debug.Log("ScannerUI: Scan started");
        
        // Hide ship image and action buttons during scan
        HideShipImage();
        HideActionButtons();
        
        // Show scanning indicator
        if (scanningIndicator != null)
            scanningIndicator.SetActive(true);
            
        // Start progress bar animation
        if (scanProgressSlider != null)
        {
            scanProgressCoroutine = StartCoroutine(AnimateScanProgress());
        }
        
        // Show scanning effect
        if (scanningEffect != null)
            scanningEffect.SetActive(true);
        
        // Update status
        ShowScanStatus("Scanning for ships...", Color.cyan);
        
        // Update button states
        UpdateButtonStates();
    }
    
    /// <summary>
    /// Handle scan completed event
    /// </summary>
    private void OnScanCompleted()
    {
        Debug.Log("ScannerUI: Scan completed");
        
        // Hide scanning indicator
        if (scanningIndicator != null)
            scanningIndicator.SetActive(false);
            
        // Hide scanning effect
        if (scanningEffect != null)
            scanningEffect.SetActive(false);
        
        // Stop progress animation
        if (scanProgressCoroutine != null)
        {
            StopCoroutine(scanProgressCoroutine);
            scanProgressCoroutine = null;
        }
        
        // Reset progress bar
        if (scanProgressSlider != null)
            scanProgressSlider.value = 0;
        
        // Update displays
        UpdateAllDisplays();
    }
    
    /// <summary>
    /// Handle ship discovered event
    /// </summary>
    private void OnShipDiscovered(DiscoveredShip ship)
    {
        Debug.Log($"ScannerUI: Ship discovered - {ship.ShipName}");
        
        // Show success effect
        if (successEffect != null)
        {
            successEffect.SetActive(true);
            StartCoroutine(HideEffectAfterDelay(successEffect, 2f));
        }
        
        // Show ship image and action buttons
        ShowShipImage(ship);
        UpdateActionButtons(ship);
        
        // Show result panel
        ShowDiscoveryResult(ship, true);
        
        // Update button states
        UpdateButtonStates();
    }
    
    /// <summary>
    /// Handle scan failed event
    /// </summary>
    private void OnScanFailed()
    {
        Debug.Log("ScannerUI: Scan failed");
        
        // Show failure effect
        if (failureEffect != null)
        {
            failureEffect.SetActive(true);
            StartCoroutine(HideEffectAfterDelay(failureEffect, 2f));
        }
        
        // Show failure result
        ShowDiscoveryResult(null, false);
        
        // Update button states
        UpdateButtonStates();
    }
    
    /// <summary>
    /// Handle ship interacted event
    /// </summary>
    private void OnShipInteracted()
    {
        Debug.Log("ScannerUI: Ship interaction completed");
        
        // Hide result panel and ship image
        if (resultPanel != null)
            resultPanel.SetActive(false);
            
        HideShipImage();
        HideActionButtons();
        
        // Update button states
        UpdateButtonStates();
        
        // Show ready status
        ShowScanStatus("Ready to scan", Color.green);
    }
    
    // ===== HELPER METHODS =====
    
    /// <summary>
    /// Get the appropriate sprite for a discovered ship
    /// </summary>
    private Sprite GetShipSprite(DiscoveredShip ship)
    {
        // Priority 1: Ship has its own icon assigned
        if (ship.ShipIcon != null)
            return ship.ShipIcon;
        
        // Priority 2: Try to get icon from EnemyIconManager
        if (EnemyIconManager.Instance != null)
        {
            EnemyType enemyType = ConvertRarityToEnemyType(ship.Rarity);
            Sprite typeIcon = EnemyIconManager.Instance.GetTypeIcon(enemyType);
            if (typeIcon != null)
                return typeIcon;
            
            if (!string.IsNullOrEmpty(ship.ShipType))
            {
                Sprite modelIcon = EnemyIconManager.Instance.GetIconForShipModel(ship.ShipType);
                if (modelIcon != null)
                    return modelIcon;
            }
        }
        
        // Priority 3: Try to load from Resources
        if (!string.IsNullOrEmpty(ship.ShipName))
        {
            string cleanName = ship.ShipName.Replace(" ", "");
            Sprite resourceIcon = Resources.Load<Sprite>($"ShipIcons/{cleanName}");
            if (resourceIcon != null)
                return resourceIcon;
        }
        
        // Priority 4: Use rarity-based fallback
        return GetFallbackSprite(ship.Rarity);
    }
    
    /// <summary>
    /// Get fallback sprite based on rarity
    /// </summary>
    private Sprite GetFallbackSprite(ShipRarity rarity)
    {
        int rarityIndex = (int)rarity;
        
        if (rarityBasedIcons != null && rarityIndex < rarityBasedIcons.Length && rarityBasedIcons[rarityIndex] != null)
            return rarityBasedIcons[rarityIndex];
        
        return defaultShipIcon;
    }
    
    /// <summary>
    /// Convert ship rarity to enemy type for icon lookup
    /// </summary>
    private EnemyType ConvertRarityToEnemyType(ShipRarity rarity)
    {
        switch (rarity)
        {
            case ShipRarity.VeryCommon:
            case ShipRarity.Common:
                return EnemyType.Scavenger;
                
            case ShipRarity.SlightlyRare:
            case ShipRarity.Rare:
                return EnemyType.Rival;
                
            case ShipRarity.Epic:
            case ShipRarity.Legendary:
                return EnemyType.Boss;
                
            case ShipRarity.Anomaly:
                return EnemyType.Anomaly;
                
            default:
                return EnemyType.Scavenger;
        }
    }
    
    /// <summary>
    /// Update energy display
    /// </summary>
    private void UpdateEnergyDisplay(float currentEnergy)
    {
        if (ShipScanner.Instance == null) return;
        
        if (energySlider != null)
        {
            energySlider.maxValue = ShipScanner.Instance.MaxEnergy;
            energySlider.value = currentEnergy;
        }
        
        if (energyText != null)
            energyText.text = $"Energy: {currentEnergy:F0}/{ShipScanner.Instance.MaxEnergy:F0}";
        
        if (energyRegenText != null)
            energyRegenText.text = "Regenerating...";
    }
    
    /// <summary>
    /// Update scanner information display
    /// </summary>
    private void UpdateScannerInfo()
    {
        if (scannerLevelText != null)
        {
            int scannerLevel = 1;
            if (ShipManager.Instance != null)
            {
                var scannerCompartment = ShipManager.Instance.GetAllCompartments()
                    .Find(c => c.Type == CompartmentType.Scanner);
                if (scannerCompartment != null)
                    scannerLevel = scannerCompartment.CurrentLevel;
            }
            scannerLevelText.text = $"Scanner Level: {scannerLevel}";
        }
        
        if (locationBonusText != null)
        {
            string locationName = "Unknown";
            float dangerLevel = 0f;
            
            if (LocationManager.Instance?.GetCurrentLocation() != null)
            {
                var location = LocationManager.Instance.GetCurrentLocation();
                locationName = location.displayName;
                dangerLevel = location.dangerLevel;
            }
            
            locationBonusText.text = $"Location: {locationName}\nDanger Level: {dangerLevel:P0}";
        }
    }
    
    /// <summary>
    /// Update button interaction states
    /// </summary>
    private void UpdateButtonStates()
    {
        if (ShipScanner.Instance == null) return;
        
        bool isScanning = ShipScanner.Instance.IsScanning;
        bool hasShip = ShipScanner.Instance.HasDiscoveredShip;
        
        // Common scan button
        if (commonScanButton != null)
        {
            bool canCommonScan = ShipScanner.Instance.CanScan(true);
            commonScanButton.interactable = canCommonScan && !isScanning && !hasShip;
            
            Image buttonImage = commonScanButton.GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = canCommonScan && !hasShip ? Color.green : Color.gray;
        }
        
        // Rare scan button
        if (rareScanButton != null)
        {
            bool canRareScan = ShipScanner.Instance.CanScan(false);
            rareScanButton.interactable = canRareScan && !isScanning && !hasShip;
            
            Image buttonImage = rareScanButton.GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = canRareScan && !hasShip ? Color.blue : Color.gray;
        }
    }
    
    /// <summary>
    /// Update scan cost displays
    /// </summary>
    private void UpdateScanCosts()
    {
        if (ShipScanner.Instance == null) return;
        
        if (commonScanCostText != null)
        {
            float cost = ShipScanner.Instance.GetScanCost(true);
            commonScanCostText.text = $"Cost: {cost:F0} Energy";
        }
        
        if (rareScanCostText != null)
        {
            float cost = ShipScanner.Instance.GetScanCost(false);
            rareScanCostText.text = $"Cost: {cost:F0} Energy";
        }
    }
    
    /// <summary>
    /// Update success rate displays
    /// </summary>
    private void UpdateSuccessRates()
    {
        if (ShipScanner.Instance == null) return;
        
        if (commonSuccessRateText != null)
        {
            float rate = ShipScanner.Instance.GetSuccessRate(true);
            commonSuccessRateText.text = $"Success: {rate:P0}";
        }
        
        if (rareSuccessRateText != null)
        {
            float rate = ShipScanner.Instance.GetSuccessRate(false);
            rareSuccessRateText.text = $"Success: {rate:P0}";
        }
    }
    
    /// <summary>
    /// Show discovery result
    /// </summary>
    private void ShowDiscoveryResult(DiscoveredShip ship, bool success)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            
            if (resultText != null)
            {
                if (success && ship != null)
                {
                    resultText.text = $"SHIP DISCOVERED!\n\n" +
                                    $"Name: {ship.ShipName}\n" +
                                    $"Type: {ship.ShipType}\n" +
                                    $"Rarity: {ship.GetRarityDisplayText()}\n" +
                                    $"Level: {ship.Level}\n\n" +
                                    $"Actions: {ship.GetAvailableActionsText()}";
                    resultText.color = ship.GetRarityColor();
                }
                else
                {
                    resultText.text = "SCAN FAILED\n\nNo ships detected in this area.\nTry again or scan in a different location.";
                    resultText.color = Color.red;
                }
            }
        }
        
        // Auto-hide result after delay if failed
        if (!success)
        {
            StartCoroutine(HideResultAfterDelay(3f));
        }
    }
    
    /// <summary>
    /// Show scan status message
    /// </summary>
    private void ShowScanStatus(string message, Color color)
    {
        if (scanStatusText != null)
        {
            scanStatusText.text = message;
            scanStatusText.color = color;
        }
    }
    
    /// <summary>
    /// Animate scan progress bar
    /// </summary>
    private IEnumerator AnimateScanProgress()
    {
        if (scanProgressSlider == null) yield break;
        
        float duration = 3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            scanProgressSlider.value = progress;
            yield return null;
        }
        
        scanProgressSlider.value = 1f;
    }
    
    /// <summary>
    /// Hide effect after delay
    /// </summary>
    private IEnumerator HideEffectAfterDelay(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (effect != null)
            effect.SetActive(false);
    }
    
    /// <summary>
    /// Hide result panel after delay
    /// </summary>
    private IEnumerator HideResultAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }
    
    /// <summary>
    /// Force refresh all displays (public method for external calls)
    /// </summary>
    public void RefreshDisplay()
    {
        UpdateAllDisplays();
    }
    
    /// <summary>
    /// Debug method to test ship image display
    /// </summary>
    [ContextMenu("Test Ship Image Display")]
    public void TestShipImageDisplay()
    {
        Debug.Log("=== TESTING SHIP IMAGE DISPLAY ===");
        
        Debug.Log($"Ship Image Container: {(shipImageContainer != null ? "✅ Assigned" : "❌ NULL")}");
        Debug.Log($"Discovered Ship Image: {(discoveredShipImage != null ? "✅ Assigned" : "❌ NULL")}");
        Debug.Log($"Ship Image Label: {(shipImageLabel != null ? "✅ Assigned" : "❌ NULL")}");
        Debug.Log($"Rarity Border: {(rarityBorder != null ? "✅ Assigned" : "❌ NULL")}");
        Debug.Log($"Combat Preview: {(combatPreview != null ? "✅ Assigned" : "❌ NULL")}");
        
        if (ShipScanner.Instance?.HasDiscoveredShip == true)
        {
            var ship = ShipScanner.Instance.CurrentShip;
            Debug.Log($"Current ship: {ship.ShipName} ({ship.Rarity})");
            ShowShipImage(ship);
            UpdateActionButtons(ship);
        }
        else
        {
            Debug.Log("No ship currently discovered. Creating test ship...");
            
            var testShip = new DiscoveredShip
            {
                ShipName = "Test Ship",
                ShipType = "Test Vessel", 
                Rarity = ShipRarity.Rare,
                Level = 5,
                AttackPower = 25,
                Defense = 15,
                Health = 100,
                CurrentHealth = 100
            };
            
            ShowShipImage(testShip);
            UpdateActionButtons(testShip);
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (ShipScanner.Instance != null)
        {
            ShipScanner.Instance.OnEnergyChanged -= UpdateEnergyDisplay;
            ShipScanner.Instance.OnScanStarted -= OnScanStarted;
            ShipScanner.Instance.OnScanCompleted -= OnScanCompleted;
            ShipScanner.Instance.OnShipDiscovered -= OnShipDiscovered;
            ShipScanner.Instance.OnScanFailed -= OnScanFailed;
            ShipScanner.Instance.OnShipInteracted -= OnShipInteracted;
        }
        
        // Unsubscribe from combat preview
        if (combatPreview != null)
        {
            combatPreview.OnCombatConfirmed -= OnCombatConfirmed;
            combatPreview.OnCombatCancelled -= OnCombatCancelled;
        }
        
        // Clean up coroutines
        if (scanProgressCoroutine != null)
        {
            StopCoroutine(scanProgressCoroutine);
        }
    }
}