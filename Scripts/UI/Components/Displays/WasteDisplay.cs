using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class WasteDisplay : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI originText;
    public TextMeshProUGUI stabilityText;
    public Image backgroundImage;
    public Image iconImage;
    public Button recycleButton;

    [Header("Stack Display")]
    public TextMeshProUGUI quantityText;
    public GameObject quantityBackground;

    [Header("Rarity Display")]
    public TextMeshProUGUI rarityText;
    public Image borderImage;

    #region Resource System UI Integration

    [Header("Resource Preview")]
    [SerializeField] private GameObject resourcePreviewPanel;
    [SerializeField] private TextMeshProUGUI resourcePreviewText;
    [SerializeField] private Button showResourcesButton;
    [SerializeField] private Transform resourceIconContainer;
    [SerializeField] private GameObject resourceIconPrefab;
    [SerializeField] private bool showResourcePreview = true;

    [Header("Processing Options")]
    [SerializeField] private Button processWithNewSystemButton;
    [SerializeField] private Button processWithLegacyButton;
    [SerializeField] private TextMeshProUGUI processingModeText;

    #endregion

    // Make currentWaste accessible with a public getter
    public UpdatedWasteItem currentWaste { get; private set; }

    private void Awake()
    {
        // Make sure we have all required components
        if (nameText == null) Debug.LogError("NameText is missing on WasteDisplay prefab!");
        if (originText == null) Debug.LogError("OriginText is missing on WasteDisplay prefab!");
        if (stabilityText == null) Debug.LogError("StabilityText is missing on WasteDisplay prefab!");
        if (iconImage == null) Debug.LogError("IconImage is missing on WasteDisplay prefab!");
        if (backgroundImage == null) Debug.LogError("BackgroundImage is missing on WasteDisplay prefab!");
        if (recycleButton == null) Debug.LogError("RecycleButton is missing on WasteDisplay prefab!");
    }

    private void Start()
    {
        if (recycleButton != null)
        {
            recycleButton.onClick.AddListener(RecycleWaste);
        }
    }

    /// <summary>
    /// Initialize the waste display with resource system support
    /// </summary>
    /// <param name="waste">UpdatedWasteItem to display</param>
    public void Initialize(UpdatedWasteItem waste)
    {
        if (waste == null)
        {
            DebugManager.LogError("Cannot initialize WasteDisplay with null waste item", DebugCategory.UIDebug);
            return;
        }

        DebugManager.Log($"Initializing waste display for item: {waste.Name}, Origin: {waste.DimensionalOrigin}", DebugCategory.UIDebug);
        currentWaste = waste;

        // Original initialization code...
        if (nameText != null) nameText.text = waste.Name;
        if (originText != null) originText.text = waste.DimensionalOrigin;
        if (rarityText != null) rarityText.text = waste.Rarity.ToString();
        if (quantityText != null) quantityText.text = $"x{waste.Quantity}";

        // Set stability text
        if (stabilityText != null)
        {
            stabilityText.text = $"Stability: {waste.DimensionalStability:F1}";
        }

        // Set icon
        if (iconImage != null)
        {
            iconImage.sprite = waste.Icon;
            iconImage.gameObject.SetActive(waste.Icon != null);
        }

        // Set colors based on rarity
        SetRarityColors(waste.Rarity);

        // Handle quantity display
        if (quantityBackground != null)
        {
            quantityBackground.SetActive(waste.Quantity > 1);
        }

        // NEW: Initialize resource preview
        InitializeResourcePreview(waste);

        // NEW: Setup processing buttons
        SetupProcessingButtons(waste);

        // Setup button listeners
        SetupButtonListeners();
    }

    /// <summary>
    /// Initialize the resource preview display
    /// </summary>
    private void InitializeResourcePreview(UpdatedWasteItem waste)
    {
        if (!showResourcePreview || resourcePreviewText == null) return;

        // Get resource preview string
        string preview = waste.GetResourcePreview();

        if (string.IsNullOrEmpty(preview))
        {
            if (resourcePreviewPanel != null)
                resourcePreviewPanel.SetActive(false);
            return;
        }

        // Show preview text
        resourcePreviewText.text = $"Resources: {preview}";

        if (resourcePreviewPanel != null)
            resourcePreviewPanel.SetActive(true);

        // Create resource icons if container exists
        CreateResourceIcons(waste);
    }

    /// <summary>
    /// Create visual icons for each resource type this waste generates
    /// </summary>
    private void CreateResourceIcons(UpdatedWasteItem waste)
    {
        if (resourceIconContainer == null || resourceIconPrefab == null) return;

        // Clear existing icons
        foreach (Transform child in resourceIconContainer)
        {
            Destroy(child.gameObject);
        }

        // Create icons for each resource type
        foreach (var yieldPair in waste.ResourceYields)
        {
            var resourceType = yieldPair.Key;
            var resourceYield = yieldPair.Value;

            // Only show if there's a reasonable chance of getting this resource
            if (resourceYield.baseAmount > 0 && resourceYield.chancePercentage > 10f)
            {
                CreateResourceIcon(resourceType, resourceYield);
            }
        }
    }

    /// <summary>
    /// Create a single resource icon
    /// </summary>
    private void CreateResourceIcon(ResourceType resourceType, ResourceYield resourceYield)
    {
        GameObject iconObject = Instantiate(resourceIconPrefab, resourceIconContainer);

        // Get resource config for icon and color
        var config = ResourceManager.Instance?.GetResourceConfig(resourceType);

        // Setup icon
        Image iconImage = iconObject.GetComponent<Image>();
        if (iconImage != null && config?.icon != null)
        {
            iconImage.sprite = config.icon;
            iconImage.color = config.GetDisplayColor();
        }

        // Add hover effects
        AddIconHoverEffects(iconObject, resourceType, resourceYield);
    }

    /// <summary>
    /// Add hover effects to resource icons
    /// </summary>
    private void AddIconHoverEffects(GameObject iconObject, ResourceType resourceType, ResourceYield resourceYield)
    {
        var eventTrigger = iconObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
            eventTrigger = iconObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        // Hover enter
        var hoverEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        hoverEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        hoverEnter.callback.AddListener((data) => {
            ShowResourceTooltip(resourceType, resourceYield);
        });
        eventTrigger.triggers.Add(hoverEnter);

        // Hover exit
        var hoverExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        hoverExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        hoverExit.callback.AddListener((data) => {
            HideResourceTooltip();
        });
        eventTrigger.triggers.Add(hoverExit);
    }

    /// <summary>
    /// Setup processing buttons based on game settings
    /// </summary>
    private void SetupProcessingButtons(UpdatedWasteItem waste)
    {
        bool useNewSystem = true; // Default to true since GameManager uses new system by default
        bool hybridMode = false;

        // Try to get actual settings from GameManager
        var gameManagerType = System.Type.GetType("GameManager");
        if (gameManagerType != null)
        {
            var instanceField = gameManagerType.GetProperty("Instance");
            if (instanceField != null)
            {
                var gameManager = instanceField.GetValue(null);
                if (gameManager != null)
                {
                    var useNewSystemField = gameManagerType.GetField("useNewResourceSystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var hybridModeField = gameManagerType.GetField("enableHybridMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (useNewSystemField != null)
                        useNewSystem = (bool)useNewSystemField.GetValue(gameManager);
                    if (hybridModeField != null)
                        hybridMode = (bool)hybridModeField.GetValue(gameManager);
                }
            }
        }

        if (processingModeText != null)
        {
            if (useNewSystem)
            {
                processingModeText.text = hybridMode ? "Hybrid Mode" : "Resource System";
            }
            else
            {
                processingModeText.text = "Legacy Mode";
            }
        }

        // Show/hide buttons based on mode
        if (processWithNewSystemButton != null)
            processWithNewSystemButton.gameObject.SetActive(useNewSystem || hybridMode);

        if (processWithLegacyButton != null)
            processWithLegacyButton.gameObject.SetActive(!useNewSystem || hybridMode);
    }

    /// <summary>
    /// Setup button listeners
    /// </summary>
    private void SetupButtonListeners()
    {
        // Resource preview toggle
        if (showResourcesButton != null)
        {
            showResourcesButton.onClick.RemoveAllListeners();
            showResourcesButton.onClick.AddListener(ToggleResourcePreview);
        }

        // New system processing
        if (processWithNewSystemButton != null)
        {
            processWithNewSystemButton.onClick.RemoveAllListeners();
            processWithNewSystemButton.onClick.AddListener(ProcessWithNewSystem);
        }

        // Legacy processing
        if (processWithLegacyButton != null)
        {
            processWithLegacyButton.onClick.RemoveAllListeners();
            processWithLegacyButton.onClick.AddListener(ProcessWithLegacySystem);
        }

        // Original recycle button (update to use new system by default)
        if (recycleButton != null)
        {
            recycleButton.onClick.RemoveAllListeners();
            recycleButton.onClick.AddListener(ProcessWasteDefault);
        }
    }

    #region Processing Methods

    /// <summary>
    /// Process waste using the new resource system
    /// </summary>
    private void ProcessWithNewSystem()
    {
        if (currentWaste == null || WasteInventoryManager.Instance == null) return;

        // Process through WasteInventoryManager
        var generatedResources = WasteInventoryManager.Instance.ProcessWasteToResources(currentWaste);

        // Show results
        ShowProcessingResults(generatedResources, true);

        // Remove this display
        Destroy(gameObject);
    }

    /// <summary>
    /// Process waste using the legacy system
    /// </summary>
    private void ProcessWithLegacySystem()
    {
        if (currentWaste == null || WasteInventoryManager.Instance == null) return;

        // Process through legacy system
        float rp = WasteInventoryManager.Instance.ProcessWasteLegacy(currentWaste);

        // Show legacy results
        ShowLegacyProcessingResults(rp);

        // Remove this display
        Destroy(gameObject);
    }

    /// <summary>
    /// Process waste using default system (based on game settings)
    /// </summary>
    private void ProcessWasteDefault()
    {
        if (currentWaste == null || GameManager.Instance == null) return;

        // Use GameManager's ProcessWaste method (respects current system settings)
        float rp = GameManager.Instance.ProcessWaste(currentWaste);

        // Show appropriate results based on system used
        // Get the resources that were generated (this is approximate since we don't have the exact return)
        var estimatedResources = EstimateGeneratedResources();
        ShowProcessingResults(estimatedResources, false);

        // Remove this display
        Destroy(gameObject);
    }

    /// <summary>
    /// Estimate what resources were generated (for display purposes)
    /// </summary>
    private Dictionary<ResourceType, int> EstimateGeneratedResources()
    {
        var estimated = new Dictionary<ResourceType, int>();

        if (currentWaste == null) return estimated;

        foreach (var yieldPair in currentWaste.ResourceYields)
        {
            var yield = yieldPair.Value;
            if (yield.chancePercentage > 50f) // Likely to succeed
            {
                int estimatedAmount = yield.CalculateActualYield(currentWaste.Quality, 1.0f);
                if (estimatedAmount > 0)
                {
                    estimated[yieldPair.Key] = estimatedAmount;
                }
            }
        }

        return estimated;
    }

    #endregion

    #region UI Feedback Methods

    /// <summary>
    /// Show processing results for new resource system
    /// </summary>
    private void ShowProcessingResults(Dictionary<ResourceType, int> resources, bool isActual)
    {
        if (resources == null || resources.Count == 0)
        {
            ShowFeedbackMessage("No resources generated", Color.yellow);
            return;
        }

        string message = isActual ? "Generated: " : "Estimated: ";
        var resourceStrings = new List<string>();

        foreach (var kvp in resources)
        {
            resourceStrings.Add($"{kvp.Value} {kvp.Key}");
        }

        message += string.Join(", ", resourceStrings);
        ShowFeedbackMessage(message, Color.green);

        // Highlight the resources in the ResourceInventoryUI if available
        if (ResourceInventoryUI.Instance != null)
        {
            foreach (var resourceType in resources.Keys)
            {
                ResourceInventoryUI.Instance.HighlightResource(resourceType, 3f);
            }
        }
    }

    /// <summary>
    /// Show processing results for legacy system
    /// </summary>
    private void ShowLegacyProcessingResults(float rp)
    {
        string message = $"Generated: {rp:F0} Recycling Points";
        ShowFeedbackMessage(message, Color.cyan);
    }

    /// <summary>
    /// Show a feedback message to the player
    /// </summary>
    private void ShowFeedbackMessage(string message, Color color)
    {
        // Create floating text or update UI element
        if (processingModeText != null)
        {
            processingModeText.text = message;
            processingModeText.color = color;

            // Fade back to normal after a delay
            StartCoroutine(FadeTextBack());
        }

        Debug.Log($"WasteDisplay: {message}");
    }

    /// <summary>
    /// Fade text back to normal color
    /// </summary>
    private System.Collections.IEnumerator FadeTextBack()
    {
        yield return new WaitForSeconds(2f);

        if (processingModeText != null)
        {
            processingModeText.color = Color.white;
        }
    }

    /// <summary>
    /// Toggle resource preview panel
    /// </summary>
    private void ToggleResourcePreview()
    {
        if (resourcePreviewPanel != null)
        {
            bool isActive = resourcePreviewPanel.activeSelf;
            resourcePreviewPanel.SetActive(!isActive);

            // Update button text if available
            if (showResourcesButton != null)
            {
                var buttonText = showResourcesButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = isActive ? "Show Resources" : "Hide Resources";
                }
            }
        }
    }

    /// <summary>
    /// Show tooltip for resource icon
    /// </summary>
    private void ShowResourceTooltip(ResourceType resourceType, ResourceYield resourceYield)
    {
        // This could show a detailed tooltip
        // For now, just log the information
        string tooltip = $"{resourceType}: {resourceYield.baseAmount} base, {resourceYield.chancePercentage:F0}% chance";
        Debug.Log($"Resource Tooltip: {tooltip}");

        // You could implement a proper tooltip UI here
        // For example, showing a small panel with detailed resource information
    }

    /// <summary>
    /// Hide resource tooltip
    /// </summary>
    private void HideResourceTooltip()
    {
        // Hide tooltip UI if implemented
        Debug.Log("Hide resource tooltip");
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Set colors based on waste rarity
    /// </summary>
    private void SetRarityColors(WasteRarity rarity)
    {
        Color rarityColor = GetRarityColor(rarity);

        // Apply to background
        if (backgroundImage != null)
        {
            backgroundImage.color = Color.Lerp(Color.white, rarityColor, 0.3f);
        }

        // Apply to border
        if (borderImage != null)
        {
            borderImage.color = rarityColor;
        }
    }

    /// <summary>
    /// Get color for waste rarity
    /// </summary>
    private Color GetRarityColor(WasteRarity rarity)
    {
        switch (rarity)
        {
            case WasteRarity.Common: return Color.gray;
            case WasteRarity.Uncommon: return Color.green;
            case WasteRarity.Rare: return Color.blue;
            case WasteRarity.Epic: return Color.magenta;
            case WasteRarity.Legendary: return Color.yellow;
            default: return Color.white;
        }
    }

    /// <summary>
    /// Check if new resource system is available
    /// </summary>
    private bool IsNewResourceSystemAvailable()
    {
        return ResourceManager.Instance != null &&
               ResourceProcessingManager.Instance != null &&
               WasteInventoryManager.Instance != null;
    }

    /// <summary>
    /// Get current processing mode string
    /// </summary>
    private string GetProcessingModeString()
    {
        if (GameManager.Instance == null) return "Unknown";

        // Use reflection to access private fields safely
        var gameManagerType = GameManager.Instance.GetType();
        var useNewSystemField = gameManagerType.GetField("useNewResourceSystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var hybridModeField = gameManagerType.GetField("enableHybridMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        bool useNewSystem = true;
        bool hybridMode = false;

        if (useNewSystemField != null)
            useNewSystem = (bool)useNewSystemField.GetValue(GameManager.Instance);
        if (hybridModeField != null)
            hybridMode = (bool)hybridModeField.GetValue(GameManager.Instance);

        if (useNewSystem)
        {
            return hybridMode ? "Hybrid Mode" : "Resource System";
        }
        else
        {
            return "Legacy System";
        }
    }

    #endregion

    // Legacy method for compatibility - now redirects to new system
    private void RecycleWaste()
    {
        ProcessWasteDefault();
    }

    public void SetIcon(Sprite icon)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
        }
    }

    private Color GetColorForRarity(WasteRarity rarity)
    {
        return GetRarityColor(rarity);
    }

    private Color GetColorForDimension(string dimensionType)
    {
        // Return different colors based on dimension type
        if (string.IsNullOrEmpty(dimensionType))
            return Color.gray;

        dimensionType = dimensionType.ToLower();

        if (dimensionType.Contains("technological"))
            return new Color(0.4f, 0.7f, 0.9f); // Light blue - Fixed incomplete line
        else if (dimensionType.Contains("biological"))
            return new Color(0.2f, 0.8f, 0.4f); // Green
        else if (dimensionType.Contains("quantum"))
            return new Color(0.8f, 0.3f, 0.8f); // Purple
        else if (dimensionType.Contains("temporal"))
            return new Color(0.8f, 0.6f, 0.2f); // Orange
        else if (dimensionType.Contains("cosmic"))
            return new Color(0.1f, 0.1f, 0.3f); // Dark blue
        else if (dimensionType.Contains("ethereal"))
            return new Color(0.9f, 0.9f, 1.0f); // Light blue/white
        else if (dimensionType.Contains("philosophical"))
            return new Color(0.5f, 0.3f, 0.7f); // Purple/blue

        return new Color(0.7f, 0.7f, 0.7f); // Default gray
    }

    private void OnDestroy()
    {
        if (recycleButton != null)
        {
            recycleButton.onClick.RemoveListener(RecycleWaste);
        }
    }
}