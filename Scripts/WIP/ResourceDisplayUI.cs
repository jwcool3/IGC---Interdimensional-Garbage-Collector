using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ResourceDisplayUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Transform resourceContainer;
    [SerializeField] private GameObject resourceItemPrefab;
    
    [Header("Resource Categories")]
    [SerializeField] private Toggle showRawResourcesToggle;
    [SerializeField] private Toggle showProcessedResourcesToggle;
    [SerializeField] private Button toggleAllButton;
    
    [Header("Sorting Options")]
    [SerializeField] private TMP_Dropdown sortDropdown;
    
    // Active resource displays
    private Dictionary<ResourceType, ResourceItemDisplay> activeDisplays = new Dictionary<ResourceType, ResourceItemDisplay>();
    
    // Filtering options
    private bool showRawResources = true;
    private bool showProcessedResources = true;
    
    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();
        RefreshAllDisplays();
    }
    
    private void InitializeUI()
    {
        // Set up toggle listeners
        if (showRawResourcesToggle != null)
        {
            showRawResourcesToggle.isOn = showRawResources;
            showRawResourcesToggle.onValueChanged.AddListener(OnRawResourcesToggle);
        }
        
        if (showProcessedResourcesToggle != null)
        {
            showProcessedResourcesToggle.isOn = showProcessedResources;
            showProcessedResourcesToggle.onValueChanged.AddListener(OnProcessedResourcesToggle);
        }
        
        if (toggleAllButton != null)
        {
            toggleAllButton.onClick.AddListener(OnToggleAll);
        }
        
        // Set up sorting dropdown
        if (sortDropdown != null)
        {
            sortDropdown.ClearOptions();
            sortDropdown.AddOptions(new List<string> 
            { 
                "By Type", 
                "By Amount (High to Low)", 
                "By Amount (Low to High)", 
                "By Value" 
            });
            sortDropdown.onValueChanged.AddListener(OnSortChanged);
        }
    }
    
    private void SubscribeToEvents()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourceChanged += OnResourceChanged;
            ResourceManager.Instance.OnResourcesUpdated += RefreshAllDisplays;
        }
        else
        {
            Debug.LogWarning("ResourceManager.Instance not found! UI will not update automatically.");
        }
    }
    
    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourceChanged -= OnResourceChanged;
            ResourceManager.Instance.OnResourcesUpdated -= RefreshAllDisplays;
        }
    }
    
    #region Event Handlers
    
    private void OnResourceChanged(ResourceType type, int oldAmount, int newAmount)
    {
        UpdateResourceDisplay(type, newAmount);
    }
    
    private void OnRawResourcesToggle(bool value)
    {
        showRawResources = value;
        RefreshAllDisplays();
    }
    
    private void OnProcessedResourcesToggle(bool value)
    {
        showProcessedResources = value;
        RefreshAllDisplays();
    }
    
    private void OnToggleAll()
    {
        bool newState = !showRawResources || !showProcessedResources;
        showRawResources = newState;
        showProcessedResources = newState;
        
        if (showRawResourcesToggle != null) showRawResourcesToggle.isOn = newState;
        if (showProcessedResourcesToggle != null) showProcessedResourcesToggle.isOn = newState;
        
        RefreshAllDisplays();
    }
    
    private void OnSortChanged(int sortIndex)
    {
        RefreshAllDisplays();
    }
    
    #endregion
    
    #region Display Management
    
    private void RefreshAllDisplays()
    {
        if (ResourceManager.Instance == null) return;
        
        // Clear existing displays
        ClearAllDisplays();
        
        // Get all non-zero resources
        var resources = ResourceManager.Instance.GetAllNonZeroResources();
        
        // Filter resources based on toggles
        var filteredResources = FilterResources(resources);
        
        // Sort resources
        var sortedResources = SortResources(filteredResources);
        
        // Create displays for each resource
        foreach (var kvp in sortedResources)
        {
            CreateResourceDisplay(kvp.Key, kvp.Value);
        }
    }
    
    private void UpdateResourceDisplay(ResourceType type, int amount)
    {
        if (activeDisplays.TryGetValue(type, out ResourceItemDisplay display))
        {
            if (amount > 0)
            {
                display.UpdateAmount(amount);
                display.gameObject.SetActive(ShouldShowResource(type));
            }
            else
            {
                // Remove display if amount is 0
                Destroy(display.gameObject);
                activeDisplays.Remove(type);
            }
        }
        else if (amount > 0)
        {
            // Create new display
            CreateResourceDisplay(type, amount);
        }
    }
    
    private void CreateResourceDisplay(ResourceType type, int amount)
    {
        if (resourceItemPrefab == null || resourceContainer == null)
        {
            Debug.LogError("ResourceDisplayUI: Missing prefab or container!");
            return;
        }
        
        GameObject displayObj = Instantiate(resourceItemPrefab, resourceContainer);
        ResourceItemDisplay display = displayObj.GetComponent<ResourceItemDisplay>();
        
        if (display != null)
        {
            var config = ResourceManager.Instance.GetResourceConfig(type);
            display.Initialize(type, amount, config);
            activeDisplays[type] = display;
            
            displayObj.SetActive(ShouldShowResource(type));
        }
        else
        {
            Debug.LogError("ResourceDisplayUI: ResourceItemDisplay component not found on prefab!");
            Destroy(displayObj);
        }
    }
    
    private void ClearAllDisplays()
    {
        foreach (var display in activeDisplays.Values)
        {
            if (display != null && display.gameObject != null)
            {
                Destroy(display.gameObject);
            }
        }
        activeDisplays.Clear();
    }
    
    #endregion
    
    #region Filtering and Sorting
    
    private Dictionary<ResourceType, int> FilterResources(Dictionary<ResourceType, int> resources)
    {
        var filtered = new Dictionary<ResourceType, int>();
        
        foreach (var kvp in resources)
        {
            if (ShouldShowResource(kvp.Key))
            {
                filtered[kvp.Key] = kvp.Value;
            }
        }
        
        return filtered;
    }
    
    private bool ShouldShowResource(ResourceType type)
    {
        var config = ResourceManager.Instance?.GetResourceConfig(type);
        bool isProcessed = config?.isProcessedResource ?? IsProcessedResource(type);
        
        if (isProcessed && !showProcessedResources) return false;
        if (!isProcessed && !showRawResources) return false;
        
        return true;
    }
    
    private bool IsProcessedResource(ResourceType type)
    {
        // Fallback if no config is available
        return type == ResourceType.Fuel || 
               type == ResourceType.Food || 
               type == ResourceType.Parts || 
               type == ResourceType.Energy;
    }
    
    private Dictionary<ResourceType, int> SortResources(Dictionary<ResourceType, int> resources)
    {
        if (sortDropdown == null) return resources;
        
        var sorted = resources.ToList();
        
        switch (sortDropdown.value)
        {
            case 0: // By Type
                sorted.Sort((a, b) => a.Key.ToString().CompareTo(b.Key.ToString()));
                break;
                
            case 1: // By Amount (High to Low)
                sorted.Sort((a, b) => b.Value.CompareTo(a.Value));
                break;
                
            case 2: // By Amount (Low to High)
                sorted.Sort((a, b) => a.Value.CompareTo(b.Value));
                break;
                
            case 3: // By Value
                sorted.Sort((a, b) => {
                    var configA = ResourceManager.Instance?.GetResourceConfig(a.Key);
                    var configB = ResourceManager.Instance?.GetResourceConfig(b.Key);
                    int valueA = (configA?.baseValue ?? 1) * a.Value;
                    int valueB = (configB?.baseValue ?? 1) * b.Value;
                    return valueB.CompareTo(valueA);
                });
                break;
        }
        
        return sorted.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Force refresh the display (useful for debugging)
    /// </summary>
    public void ForceRefresh()
    {
        RefreshAllDisplays();
    }
    
    /// <summary>
    /// Highlight a specific resource type (useful for tutorials)
    /// </summary>
    public void HighlightResource(ResourceType type)
    {
        if (activeDisplays.TryGetValue(type, out ResourceItemDisplay display))
        {
            display.Highlight();
        }
    }
    
    /// <summary>
    /// Get the display for a specific resource (useful for animations)
    /// </summary>
    public ResourceItemDisplay GetResourceDisplay(ResourceType type)
    {
        return activeDisplays.TryGetValue(type, out ResourceItemDisplay display) ? display : null;
    }
    
    #endregion
}

/// <summary>
/// Component for individual resource display items
/// </summary>
public class ResourceItemDisplay : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image resourceIcon;
    [SerializeField] private TextMeshProUGUI resourceNameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI storageText;
    [SerializeField] private Slider storageSlider;
    [SerializeField] private Image backgroundImage;
    
    [Header("Visual Effects")]
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem changeEffect;
    
    private ResourceType currentType;
    private int currentAmount;
    private ResourceConfig config;
    
    public void Initialize(ResourceType type, int amount, ResourceConfig resourceConfig)
    {
        currentType = type;
        currentAmount = amount;
        config = resourceConfig;
        
        UpdateDisplay();
    }
    
    public void UpdateAmount(int newAmount)
    {
        int previousAmount = currentAmount;
        currentAmount = newAmount;
        
        UpdateDisplay();
        
        // Trigger visual effects for changes
        if (newAmount > previousAmount)
        {
            TriggerIncreaseEffect();
        }
        else if (newAmount < previousAmount)
        {
            TriggerDecreaseEffect();
        }
    }
    
    private void UpdateDisplay()
    {
        // Update icon
        if (resourceIcon != null && config?.icon != null)
        {
            resourceIcon.sprite = config.icon;
            resourceIcon.color = Color.white;
        }
        else if (resourceIcon != null)
        {
            resourceIcon.color = Color.gray; // No icon available
        }
        
        // Update name
        if (resourceNameText != null)
        {
            string displayName = config?.displayName ?? currentType.ToString();
            resourceNameText.text = displayName;
        }
        
        // Update amount
        if (amountText != null)
        {
            amountText.text = FormatAmount(currentAmount);
        }
        
        // Update storage info
        if (ResourceManager.Instance != null)
        {
            int storageLimit = ResourceManager.Instance.GetStorageLimit(currentType);
            
            if (storageText != null)
            {
                storageText.text = $"{currentAmount}/{storageLimit}";
            }
            
            if (storageSlider != null)
            {
                storageSlider.value = (float)currentAmount / storageLimit;
            }
        }
        
        // Update background color
        if (backgroundImage != null && config != null)
        {
            Color bgColor = config.resourceColor;
            bgColor.a = 0.3f; // Semi-transparent
            backgroundImage.color = bgColor;
        }
    }
    
    private string FormatAmount(int amount)
    {
        if (amount >= 1000000)
            return $"{amount / 1000000f:F1}M";
        else if (amount >= 1000)
            return $"{amount / 1000f:F1}K";
        else
            return amount.ToString();
    }
    
    private void TriggerIncreaseEffect()
    {
        if (animator != null)
        {
            animator.SetTrigger("Increase");
        }
        
        if (changeEffect != null)
        {
            var main = changeEffect.main;
            main.startColor = Color.green;
            changeEffect.Play();
        }
    }
    
    private void TriggerDecreaseEffect()
    {
        if (animator != null)
        {
            animator.SetTrigger("Decrease");
        }
        
        if (changeEffect != null)
        {
            var main = changeEffect.main;
            main.startColor = Color.red;
            changeEffect.Play();
        }
    }
    
    public void Highlight()
    {
        if (animator != null)
        {
            animator.SetTrigger("Highlight");
        }
    }
}