using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UI Controller for the Scanner tab
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
    
    [Header("Scanner Info")]
    [SerializeField] private TextMeshProUGUI scannerLevelText;
    [SerializeField] private TextMeshProUGUI locationBonusText;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject scanningEffect;
    [SerializeField] private GameObject successEffect;
    [SerializeField] private GameObject failureEffect;
    
    private Coroutine scanProgressCoroutine;
    
    private void Start()
    {
        Debug.Log("ScannerUI: Starting initialization");
        
        // Set up button listeners
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
        
        // Subscribe to scanner events
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
        
        // Initialize displays
        UpdateAllDisplays();
        
        // Hide scanning indicator initially
        if (scanningIndicator != null)
            scanningIndicator.SetActive(false);
            
        if (resultPanel != null)
            resultPanel.SetActive(false);
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
        
        // Hide result panel
        if (resultPanel != null)
            resultPanel.SetActive(false);
            
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
}