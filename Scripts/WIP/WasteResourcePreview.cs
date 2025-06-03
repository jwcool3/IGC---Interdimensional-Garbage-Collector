using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

/// <summary>
/// UI component that shows what resources a waste item can yield
/// Displays resource previews, probabilities, and yield estimates
/// </summary>
public class WasteResourcePreview : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject resourcePreviewPrefab;
    [SerializeField] private Transform resourceContainer;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI totalValueText;
    [SerializeField] private Button processButton;
    [SerializeField] private Slider efficiencySlider;
    [SerializeField] private TextMeshProUGUI efficiencyText;
    
    [Header("Display Settings")]
    [SerializeField] private bool showProbabilities = true;
    [SerializeField] private bool showEstimatedValues = true;
    [SerializeField] private bool showProcessingTime = true;
    [SerializeField] private bool enableInteractivePreview = true;
    [SerializeField] private float updateInterval = 0.5f;
    
    [Header("Visual Settings")]
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color neutralColor = Color.white;
    [SerializeField] private Color negativeColor = Color.red;
    [SerializeField] private AnimationCurve probabilityOpacityCurve = AnimationCurve.Linear(0, 0.3f, 1, 1f);
    
    // Component references
    private WasteResourceMapping resourceMapping;
    private ResourceYieldCalculator yieldCalculator;
    private ResourceConfigManager configManager;
    
    // Current state
    private WasteItem currentWasteItem;
    private float currentEfficiency = 1f;
    private Dictionary<ResourceType, ResourcePreviewData> currentPreviews = new Dictionary<ResourceType, ResourcePreviewData>();
    private List<GameObject> previewObjects = new List<GameObject>();
    
    // Update tracking
    private float lastUpdateTime;
    private bool needsUpdate = false;
    
    private void Awake()
    {
        InitializeComponents();
        SetupUI();
    }
    
    private void Start()
    {
        // Find required components
        resourceMapping = FindObjectOfType<WasteResourceMapping>();
        yieldCalculator = FindObjectOfType<ResourceYieldCalculator>();
        configManager = ResourceConfigManager.Instance;
        
        if (efficiencySlider != null)
        {
            efficiencySlider.onValueChanged.AddListener(OnEfficiencyChanged);
        }
        
        if (processButton != null)
        {
            processButton.onClick.AddListener(OnProcessButtonClicked);
        }
    }
    
    private void Update()
    {
        if (needsUpdate && Time.time - lastUpdateTime >= updateInterval)
        {
            UpdatePreview();
            needsUpdate = false;
            lastUpdateTime = Time.time;
        }
    }
    
    private void InitializeComponents()
    {
        if (resourceContainer == null)
        {
            resourceContainer = transform.Find("ResourceContainer");
        }
        
        if (titleText == null)
        {
            titleText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }
    
    private void SetupUI()
    {
        if (efficiencySlider != null)
        {
            efficiencySlider.minValue = 0.5f;
            efficiencySlider.maxValue = 2f;
            efficiencySlider.value = 1f;
        }
        
        if (processButton != null)
        {
            processButton.interactable = false;
        }
    }
    
    /// <summary>
    /// Set the waste item to preview
    /// </summary>
    /// <param name="wasteItem">Waste item to analyze</param>
    public void SetWasteItem(WasteItem wasteItem)
    {
        currentWasteItem = wasteItem;
        needsUpdate = true;
        
        if (processButton != null)
        {
            processButton.interactable = wasteItem != null;
        }
        
        if (wasteItem == null)
        {
            ClearPreview();
        }
    }
    
    /// <summary>
    /// Set the processing efficiency for preview calculations
    /// </summary>
    /// <param name="efficiency">Processing efficiency multiplier</param>
    public void SetEfficiency(float efficiency)
    {
        currentEfficiency = efficiency;
        
        if (efficiencySlider != null)
        {
            efficiencySlider.value = efficiency;
        }
        
        needsUpdate = true;
    }
    
    /// <summary>
    /// Update the resource preview display
    /// </summary>
    public void UpdatePreview()
    {
        if (currentWasteItem == null || resourceMapping == null)
        {
            ClearPreview();
            return;
        }
        
        // Get resource yields
        var yields = resourceMapping.GetResourceYields(currentWasteItem);
        
        // Calculate preview data
        currentPreviews.Clear();
        float totalEstimatedValue = 0f;
        
        foreach (var yieldPair in yields)
        {
            var previewData = CalculatePreviewData(yieldPair.Key, yieldPair.Value);
            currentPreviews[yieldPair.Key] = previewData;
            totalEstimatedValue += previewData.estimatedValue;
        }
        
        // Update UI
        UpdateTitle();
        UpdateResourcePreviews();
        UpdateTotalValue(totalEstimatedValue);
        UpdateEfficiencyDisplay();
    }
    
    private ResourcePreviewData CalculatePreviewData(ResourceType resourceType, ResourceYield yield)
    {
        var previewData = new ResourcePreviewData
        {
            resourceType = resourceType,
            baseYield = yield
        };
        
        // Calculate with current efficiency
        if (yieldCalculator != null)
        {
            var finalYield = yieldCalculator.CalculateFinalYield(
                currentWasteItem, 
                currentWasteItem.Quantity, 
                currentEfficiency
            );
            
            if (finalYield.ContainsKey(resourceType))
            {
                previewData.expectedAmount = finalYield[resourceType];
            }
        }
        else
        {
            // Fallback calculation
            previewData.expectedAmount = Mathf.RoundToInt(
                yield.baseAmount * yield.yieldMultiplier * currentEfficiency
            );
        }
        
        // Calculate probability
        previewData.successProbability = yield.chancePercentage / 100f;
        
        // Calculate estimated value
        if (configManager != null)
        {
            var config = configManager.GetResourceConfig(resourceType);
            if (config != null)
            {
                previewData.estimatedValue = config.CalculateEffectiveValue(previewData.expectedAmount);
            }
        }
        
        // Calculate processing time
        previewData.processingTime = currentWasteItem.ProcessingTime / currentEfficiency;
        
        return previewData;
    }
    
    private void UpdateTitle()
    {
        if (titleText != null && currentWasteItem != null)
        {
            titleText.text = $"Resource Preview: {currentWasteItem.Name}";
        }
    }
    
    private void UpdateResourcePreviews()
    {
        // Clear existing previews
        ClearPreviewObjects();
        
        if (resourcePreviewPrefab == null || resourceContainer == null)
            return;
        
        // Create new preview objects
        foreach (var previewPair in currentPreviews.OrderByDescending(p => p.Value.estimatedValue))
        {
            var previewObj = CreateResourcePreviewObject(previewPair.Value);
            previewObjects.Add(previewObj);
        }
    }
    
    private GameObject CreateResourcePreviewObject(ResourcePreviewData previewData)
    {
        var previewObj = Instantiate(resourcePreviewPrefab, resourceContainer);
        var previewComponent = previewObj.GetComponent<ResourcePreviewItem>();
        
        if (previewComponent != null)
        {
            previewComponent.SetPreviewData(previewData, showProbabilities, showEstimatedValues);
        }
        else
        {
            // Fallback: set up basic UI elements
            SetupBasicPreviewObject(previewObj, previewData);
        }
        
        // Apply visual effects
        ApplyVisualEffects(previewObj, previewData);
        
        return previewObj;
    }
    
    private void SetupBasicPreviewObject(GameObject previewObj, ResourcePreviewData previewData)
    {
        // Find and set up basic UI components
        var iconImage = previewObj.transform.Find("Icon")?.GetComponent<Image>();
        var nameText = previewObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        var amountText = previewObj.transform.Find("Amount")?.GetComponent<TextMeshProUGUI>();
        var probabilityText = previewObj.transform.Find("Probability")?.GetComponent<TextMeshProUGUI>();
        
        // Get resource config for display info
        var config = configManager?.GetResourceConfig(previewData.resourceType);
        
        if (iconImage != null && config != null)
        {
            iconImage.sprite = config.icon;
            iconImage.color = config.GetDisplayColor();
        }
        
        if (nameText != null)
        {
            nameText.text = config?.displayName ?? previewData.resourceType.ToString();
        }
        
        if (amountText != null)
        {
            amountText.text = previewData.expectedAmount.ToString();
        }
        
        if (probabilityText != null && showProbabilities)
        {
            probabilityText.text = $"{previewData.successProbability:P0}";
        }
    }
    
    private void ApplyVisualEffects(GameObject previewObj, ResourcePreviewData previewData)
    {
        // Apply opacity based on probability
        var canvasGroup = previewObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = previewObj.AddComponent<CanvasGroup>();
        }
        
        float opacity = probabilityOpacityCurve.Evaluate(previewData.successProbability);
        canvasGroup.alpha = opacity;
        
        // Apply color coding based on value
        var backgroundImage = previewObj.GetComponent<Image>();
        if (backgroundImage != null)
        {
            if (previewData.estimatedValue > 10f)
            {
                backgroundImage.color = Color.Lerp(neutralColor, positiveColor, 0.3f);
            }
            else if (previewData.estimatedValue < 1f)
            {
                backgroundImage.color = Color.Lerp(neutralColor, negativeColor, 0.3f);
            }
        }
    }
    
    private void UpdateTotalValue(float totalValue)
    {
        if (totalValueText != null)
        {
            totalValueText.text = $"Est. Total Value: {totalValue:F1}";
            
            // Color code the total value
            if (totalValue > 20f)
            {
                totalValueText.color = positiveColor;
            }
            else if (totalValue < 5f)
            {
                totalValueText.color = negativeColor;
            }
            else
            {
                totalValueText.color = neutralColor;
            }
        }
    }
    
    private void UpdateEfficiencyDisplay()
    {
        if (efficiencyText != null)
        {
            efficiencyText.text = $"Efficiency: {currentEfficiency:P0}";
        }
    }
    
    private void ClearPreview()
    {
        ClearPreviewObjects();
        currentPreviews.Clear();
        
        if (titleText != null)
        {
            titleText.text = "No Waste Item Selected";
        }
        
        if (totalValueText != null)
        {
            totalValueText.text = "Est. Total Value: 0";
        }
    }
    
    private void ClearPreviewObjects()
    {
        foreach (var obj in previewObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        previewObjects.Clear();
    }
    
    private void OnEfficiencyChanged(float value)
    {
        SetEfficiency(value);
    }
    
    private void OnProcessButtonClicked()
    {
        if (currentWasteItem != null)
        {
            // Find and trigger processing
            var processingManager = ResourceProcessingManager.Instance;
            if (processingManager != null)
            {
                processingManager.ProcessWasteItem(currentWasteItem, currentWasteItem.Quantity);
            }
        }
    }
    
    /// <summary>
    /// Enable or disable interactive preview features
    /// </summary>
    /// <param name="enabled">Whether to enable interactive features</param>
    public void SetInteractiveMode(bool enabled)
    {
        enableInteractivePreview = enabled;
        
        if (efficiencySlider != null)
        {
            efficiencySlider.interactable = enabled;
        }
        
        if (processButton != null)
        {
            processButton.gameObject.SetActive(enabled);
        }
    }
    
    /// <summary>
    /// Get the current preview data
    /// </summary>
    /// <returns>Dictionary of current preview data</returns>
    public Dictionary<ResourceType, ResourcePreviewData> GetCurrentPreviews()
    {
        return new Dictionary<ResourceType, ResourcePreviewData>(currentPreviews);
    }
    
    private void OnDestroy()
    {
        if (efficiencySlider != null)
        {
            efficiencySlider.onValueChanged.RemoveListener(OnEfficiencyChanged);
        }
        
        if (processButton != null)
        {
            processButton.onClick.RemoveListener(OnProcessButtonClicked);
        }
    }
}

/// <summary>
/// Data structure for resource preview information
/// </summary>
[System.Serializable]
public class ResourcePreviewData
{
    public ResourceType resourceType;
    public ResourceYield baseYield;
    public int expectedAmount;
    public float successProbability;
    public float estimatedValue;
    public float processingTime;
    
    public override string ToString()
    {
        return $"{resourceType}: {expectedAmount} ({successProbability:P0}) = {estimatedValue:F1} value";
    }
}

/// <summary>
/// Individual resource preview item component
/// </summary>
public class ResourcePreviewItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI probabilityText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private TextMeshProUGUI timeText;
    
    /// <summary>
    /// Set the preview data for this item
    /// </summary>
    /// <param name="data">Preview data to display</param>
    /// <param name="showProbability">Whether to show probability</param>
    /// <param name="showValue">Whether to show estimated value</param>
    public void SetPreviewData(ResourcePreviewData data, bool showProbability = true, bool showValue = true)
    {
        // Get resource config for display info
        var configManager = ResourceConfigManager.Instance;
        var config = configManager?.GetResourceConfig(data.resourceType);
        
        if (iconImage != null && config != null)
        {
            iconImage.sprite = config.icon;
            iconImage.color = config.GetDisplayColor();
        }
        
        if (nameText != null)
        {
            nameText.text = config?.displayName ?? data.resourceType.ToString();
        }
        
        if (amountText != null)
        {
            amountText.text = data.expectedAmount.ToString();
        }
        
        if (probabilityText != null)
        {
            probabilityText.gameObject.SetActive(showProbability);
            if (showProbability)
            {
                probabilityText.text = $"{data.successProbability:P0}";
            }
        }
        
        if (valueText != null)
        {
            valueText.gameObject.SetActive(showValue);
            if (showValue)
            {
                valueText.text = $"{data.estimatedValue:F1}";
            }
        }
        
        if (timeText != null)
        {
            timeText.text = $"{data.processingTime:F1}s";
        }
    }
} 