using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UI Controller for the Scanner tab - Updated with ship image display
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
    [SerializeField] private Button fightNowButton;
    
    [Header("Ship Image Display")]
    [SerializeField] private Image discoveredShipImage;
    [SerializeField] private GameObject shipImageContainer;
    [SerializeField] private TextMeshProUGUI shipImageLabel;
    [SerializeField] private Image rarityBorder;
    
    [Header("Scanner Info")]
    [SerializeField] private TextMeshProUGUI scannerLevelText;
    [SerializeField] private TextMeshProUGUI locationBonusText;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject scanningEffect;
    [SerializeField] private GameObject successEffect;
    [SerializeField] private GameObject failureEffect;
    
    [Header("Fallback Ship Images")]
    [SerializeField] private Sprite defaultShipIcon;
    [SerializeField] private Sprite[] rarityBasedIcons = new Sprite[7]; // One for each rarity
    
    private Coroutine scanProgressCoroutine;
    
    private void Start()
    {
        Debug.Log("ScannerUI: Starting initialization");
        
        // Set up button listeners
        SetupButtonListeners();
        
        // Subscribe to scanner events
        SubscribeToScannerEvents();
        
        // Initialize displays
        UpdateAllDisplays();
        
        // Hide scanning indicator initially
        HideUIElements();
        
        Debug.Log("ScannerUI: Initialization complete");
    }
    
    private void SetupButtonListeners()
    {
        if (commonScanButton != null)
            commonScanButton.onClick.AddListener(OnCommonScanClicked);
        else
            Debug.LogError("ScannerUI: Common scan button not assigned!");
            
        if (rareScanButton != null)
            rareScanButton.onClick.AddListener(OnRareScanClicked);
        else
            Debug.LogError("ScannerUI: Rare scan button not assigned!");
            
        if (viewContactsButton != null)
            viewContactsButton.onClick.AddListener(OnViewContactsClicked);
            
        if (fightNowButton != null)
            fightNowButton.onClick.AddListener(OnFightNowClicked);
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
        
        // Clean up coroutines
        if (scanProgressCoroutine != null)
        {
            StopCoroutine(scanProgressCoroutine);
        }
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
    /// Update ship image display based on current discovered ship
    /// </summary>
    private void UpdateShipDisplay()
    {
        if (ShipScanner.Instance?.HasDiscoveredShip == true)
        {
            ShowShipImage(ShipScanner.Instance.CurrentShip);
        }
        else
        {
            HideShipImage();
        }
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
        
        Debug.Log($"ScannerUI: Attempting to show ship image for {ship.ShipName}");
        
        // Show the ship image container
        if (shipImageContainer != null)
        {
            shipImageContainer.SetActive(true);
            Debug.Log("ScannerUI: Ship image container activated");
        }
        else
        {
            Debug.LogError("ScannerUI: Ship image container is null! Please assign it in the inspector.");
        }
        
        // Update ship image
        if (discoveredShipImage != null)
        {
            Sprite shipSprite = GetShipSprite(ship);
            
            if (shipSprite != null)
            {
                discoveredShipImage.sprite = shipSprite;
                discoveredShipImage.color = Color.white;
                discoveredShipImage.enabled = true;
                
                Debug.Log($"ScannerUI: Displaying ship sprite for {ship.ShipName}: {shipSprite.name}");
            }
            else
            {
                // Use color-coded fallback
                discoveredShipImage.sprite = GetFallbackSprite(ship.Rarity);
                discoveredShipImage.color = ship.GetRarityColor();
                discoveredShipImage.enabled = true;
                
                Debug.Log($"ScannerUI: Using fallback image for {ship.ShipName} with color {ship.GetRarityColor()}");
            }
        }
        else
        {
            Debug.LogError("ScannerUI: Discovered ship image is null! Please assign it in the inspector.");
        }
        
        // Update ship image label
        if (shipImageLabel != null)
        {
            shipImageLabel.text = $"{ship.ShipName}\n{ship.GetRarityDisplayText()}";
            shipImageLabel.color = ship.GetRarityColor();
            Debug.Log($"ScannerUI: Updated ship label for {ship.ShipName}");
        }
        else
        {
            Debug.LogWarning("ScannerUI: Ship image label is null");
        }
        
        // Update rarity border
        if (rarityBorder != null)
        {
            rarityBorder.color = ship.GetRarityColor();
            rarityBorder.enabled = true;
            Debug.Log($"ScannerUI: Updated rarity border with color {ship.GetRarityColor()}");
        }
        else
        {
            Debug.LogWarning("ScannerUI: Rarity border is null");
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
    
    /// <summary>
    /// Get the appropriate sprite for a discovered ship
    /// </summary>
    private Sprite GetShipSprite(DiscoveredShip ship)
    {
        // Priority 1: Ship has its own icon assigned
        if (ship.ShipIcon != null)
        {
            return ship.ShipIcon;
        }
        
        // Priority 2: Try to get icon from EnemyIconManager based on ship type
        if (EnemyIconManager.Instance != null)
        {
            // Convert ship rarity to enemy type for icon lookup
            EnemyType enemyType = ConvertRarityToEnemyType(ship.Rarity);
            
            // Try to get type icon
            Sprite typeIcon = EnemyIconManager.Instance.GetTypeIcon(enemyType);
            if (typeIcon != null)
            {
                return typeIcon;
            }
            
            // Try to get ship icon by model name if available
            if (!string.IsNullOrEmpty(ship.ShipType))
            {
                Sprite modelIcon = EnemyIconManager.Instance.GetIconForShipModel(ship.ShipType);
                if (modelIcon != null)
                {
                    return modelIcon;
                }
            }
        }
        
        // Priority 3: Try to load from Resources based on ship name
        if (!string.IsNullOrEmpty(ship.ShipName))
        {
            string cleanName = ship.ShipName.Replace(" ", "");
            Sprite resourceIcon = Resources.Load<Sprite>($"ShipIcons/{cleanName}");
            if (resourceIcon != null)
            {
                return resourceIcon;
            }
        }
        
        // Priority 4: Use rarity-based fallback icon
        return GetFallbackSprite(ship.Rarity);
    }
    
    /// <summary>
    /// Get fallback sprite based on rarity
    /// </summary>
    private Sprite GetFallbackSprite(ShipRarity rarity)
    {
        int rarityIndex = (int)rarity;
        
        // Check if we have a rarity-specific icon
        if (rarityBasedIcons != null && rarityIndex < rarityBasedIcons.Length && rarityBasedIcons[rarityIndex] != null)
        {
            return rarityBasedIcons[rarityIndex];
        }
        
        // Use default ship icon
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
        
        // Update slider
        if (energySlider != null)
        {
            energySlider.maxValue = ShipScanner.Instance.MaxEnergy;
            energySlider.value = currentEnergy;
        }
        
        // Update text
        if (energyText != null)
        {
            energyText.text = $"Energy: {currentEnergy:F0}/{ShipScanner.Instance.MaxEnergy:F0}";
        }
        
        // Update regen rate text
        if (energyRegenText != null)
        {
            energyRegenText.text = "Regenerating...";
        }
    }
    
    /// <summary>
    /// Update scanner information display
    /// </summary>
    private void UpdateScannerInfo()
    {
        // Update scanner level display
        if (scannerLevelText != null)
        {
            int scannerLevel = 1;
            if (ShipManager.Instance != null)
            {
                var scannerCompartment = ShipManager.Instance.GetAllCompartments()
                    .Find(c => c.Type == CompartmentType.Scanner);
                if (scannerCompartment != null)
                {
                    scannerLevel = scannerCompartment.CurrentLevel;
                }
            }
            scannerLevelText.text = $"Scanner Level: {scannerLevel}";
        }
        
        // Update location bonus
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
            
            // Update button color
            Image buttonImage = commonScanButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = canCommonScan && !hasShip ? Color.green : Color.gray;
            }
        }
        
        // Rare scan button
        if (rareScanButton != null)
        {
            bool canRareScan = ShipScanner.Instance.CanScan(false);
            rareScanButton.interactable = canRareScan && !isScanning && !hasShip;
            
            // Update button color
            Image buttonImage = rareScanButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = canRareScan && !hasShip ? Color.blue : Color.gray;
            }
        }
        
        // View contacts button
        if (viewContactsButton != null)
        {
            viewContactsButton.interactable = hasShip;
            viewContactsButton.gameObject.SetActive(hasShip);
        }
        
        // Fight now button
        if (fightNowButton != null)
        {
            bool canFight = hasShip && ShipScanner.Instance.CurrentShip.CanFight;
            fightNowButton.interactable = canFight;
            fightNowButton.gameObject.SetActive(hasShip);
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
    
    // ===== EVENT HANDLERS =====
    
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
                Debug.Log("ScannerUI: Common scan failed to start");
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
                Debug.Log("ScannerUI: Rare scan failed to start");
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
        
        // Switch to contacts tab
        TabSystem tabSystem = FindObjectOfType<TabSystem>();
        if (tabSystem != null)
        {
            tabSystem.ShowContactsTab();
        }
        else
        {
            ShowScanStatus("Contacts tab coming soon!", Color.yellow);
        }
    }
    
    /// <summary>
    /// Handle fight now button click
    /// </summary>
    private void OnFightNowClicked()
    {
        if (ShipScanner.Instance?.CurrentShip == null)
        {
            Debug.LogWarning("ScannerUI: No ship to fight!");
            return;
        }
        
        var ship = ShipScanner.Instance.CurrentShip;
        if (!ship.CanFight)
        {
            Debug.LogWarning("ScannerUI: Cannot fight this ship!");
            ShowScanStatus("Cannot fight this ship", Color.red);
            return;
        }
        
        Debug.Log($"ScannerUI: Fighting {ship.ShipName} directly from scanner");
        
        // Use ShipInteractionManager to start combat
        if (ShipInteractionManager.Instance != null)
        {
            bool success = ShipInteractionManager.Instance.StartCombatWithShip(ship);
            if (success)
            {
                Debug.Log("ScannerUI: Combat started successfully from scanner");
                ShowScanStatus("Entering combat...", Color.red);
            }
            else
            {
                Debug.LogWarning("ScannerUI: Failed to start combat from scanner");
                ShowScanStatus("Failed to start combat", Color.red);
            }
        }
        else
        {
            Debug.LogError("ScannerUI: ShipInteractionManager not found!");
            ShowScanStatus("Combat system unavailable", Color.red);
        }
    }
    
    /// <summary>
    /// Handle scan started event
    /// </summary>
    private void OnScanStarted()
    {
        Debug.Log("ScannerUI: Scan started");
        
        // Hide ship image during scan
        HideShipImage();
        
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
        {
            scanningEffect.SetActive(true);
        }
        
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
        {
            scanningEffect.SetActive(false);
        }
        
        // Stop progress animation
        if (scanProgressCoroutine != null)
        {
            StopCoroutine(scanProgressCoroutine);
            scanProgressCoroutine = null;
        }
        
        // Reset progress bar
        if (scanProgressSlider != null)
        {
            scanProgressSlider.value = 0;
        }
        
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
        
        // Show ship image
        ShowShipImage(ship);
        
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
        
        // Update button states
        UpdateButtonStates();
        
        // Show ready status
        ShowScanStatus("Ready to scan", Color.green);
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
        
        float duration = 3f; // Should match scan duration
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
        
        // Check if all references are assigned
        Debug.Log($"Ship Image Container: {(shipImageContainer != null ? "✅ Assigned" : "❌ NULL")}");
        Debug.Log($"Discovered Ship Image: {(discoveredShipImage != null ? "✅ Assigned" : "❌ NULL")}");
        Debug.Log($"Ship Image Label: {(shipImageLabel != null ? "✅ Assigned" : "❌ NULL")}");
        Debug.Log($"Rarity Border: {(rarityBorder != null ? "✅ Assigned" : "❌ NULL")}");
        Debug.Log($"Default Ship Icon: {(defaultShipIcon != null ? "✅ Assigned" : "❌ NULL")}");
        
        // Check if we have a discovered ship
        if (ShipScanner.Instance?.HasDiscoveredShip == true)
        {
            var ship = ShipScanner.Instance.CurrentShip;
            Debug.Log($"Current ship: {ship.ShipName} ({ship.Rarity})");
            Debug.Log($"Ship has icon: {(ship.ShipIcon != null ? "✅ Yes" : "❌ No")}");
            
            // Force show the ship
            ShowShipImage(ship);
        }
        else
        {
            Debug.Log("No ship currently discovered. Showing test ship...");
            
            // Create a test ship for display testing
            var testShip = new DiscoveredShip
            {
                ShipName = "Test Ship",
                ShipType = "Test Vessel",
                Rarity = ShipRarity.Rare,
                Level = 5
            };
            
            ShowShipImage(testShip);
        }
    }
}