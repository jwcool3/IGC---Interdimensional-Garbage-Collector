using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace ResourceSystem.UI
{
    /// <summary>
    /// Main UI controller for the resource inventory display
    /// Manages the resource grid, filtering, sorting, and updates
    /// </summary>
    public class ResourceInventoryUI : MonoBehaviour
    {
        // Singleton instance
        public static ResourceInventoryUI Instance { get; private set; }
        
        [Header("UI References")]
        [SerializeField] private Transform resourceGrid;
        [SerializeField] private GameObject resourceDisplayPrefab;
        [SerializeField] private TextMeshProUGUI totalResourcesText;
        [SerializeField] private Slider storageIndicator;
        [SerializeField] private TextMeshProUGUI emptyMessage;

        [Header("Crafted Items Tab")]
        [SerializeField] private GameObject craftedItemsTab;
        [SerializeField] private Button rawResourcesTabButton;
        [SerializeField] private Button craftedItemsTabButton;
        [SerializeField] private Transform craftedItemsContainer;
        [SerializeField] private GameObject craftedItemDisplayPrefab;
        
        [Header("Filter Controls")]
        [SerializeField] private Toggle rawResourcesToggle;
        [SerializeField] private Toggle processedResourcesToggle;
        [SerializeField] private Button toggleAllButton;
        
        [Header("Sorting Controls")]
        [SerializeField] private TMP_Dropdown sortDropdown;
        
        [Header("Display Settings")]
        [SerializeField] private bool hideEmptyResources = true;
        [SerializeField] private bool showZeroQuantityResources = false;
        [SerializeField] private int maxDisplayedResources = 50;
        
        // Resource display management
        private Dictionary<ResourceType, ResourceDisplayItem> activeDisplays = new Dictionary<ResourceType, ResourceDisplayItem>();
        private List<ResourceType> filteredResourceTypes = new List<ResourceType>();
        
        // Add this to your existing ResourceInventoryUI class
        private Dictionary<ResourceType, GameObject> activeCraftedDisplays = new Dictionary<ResourceType, GameObject>();
        private bool showingCraftedItems = false;
        
        // Filter states
        private bool showRawResources = true;
        private bool showProcessedResources = true;
        
        // Sorting
        private int currentSortMode = 0;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            InitializeUI();
            InitializeTabs();
            SubscribeToEvents();
            
            // Subscribe to crafted inventory events
            if (CraftedItemInventory.Instance != null)
            {
                CraftedItemInventory.Instance.OnCraftedInventoryUpdated += UpdateCraftedItemsDisplay;
            }
            
            RefreshDisplay();
        }
        
        private void OnEnable()
        {
            RefreshDisplay();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeUI()
        {
            SetupFilterControls();
            SetupSortingControls();
            SetupToggleAllButton();
            
            // Initialize empty message
            if (emptyMessage != null)
            {
                emptyMessage.gameObject.SetActive(false);
            }
        }
        
        private void InitializeTabs()
        {
            if (rawResourcesTabButton != null)
            {
                rawResourcesTabButton.onClick.AddListener(OnRawResourcesTabClicked);
            }
            if (craftedItemsTabButton != null)
            {
                craftedItemsTabButton.onClick.AddListener(OnCraftedItemsTabClicked);
            }
            UpdateTabButtons();
        }

        private void UpdateTabButtons()
        {
            if (rawResourcesTabButton != null)
            {
                rawResourcesTabButton.interactable = !showingCraftedItems;
            }
            if (craftedItemsTabButton != null)
            {
                craftedItemsTabButton.interactable = showingCraftedItems;
            }
        }

        private void OnRawResourcesTabClicked()
        {
            showingCraftedItems = false;
            UpdateTabButtons();
            RefreshDisplay();
        }

        private void OnCraftedItemsTabClicked()
        {
            showingCraftedItems = true;
            UpdateTabButtons();
            RefreshDisplay();
        }
        
        private void SetupFilterControls()
        {
            if (rawResourcesToggle != null)
            {
                rawResourcesToggle.isOn = showRawResources;
                rawResourcesToggle.onValueChanged.AddListener(OnRawResourcesToggleChanged);
            }
            
            if (processedResourcesToggle != null)
            {
                processedResourcesToggle.isOn = showProcessedResources;
                processedResourcesToggle.onValueChanged.AddListener(OnProcessedResourcesToggleChanged);
            }
        }
        
        private void SetupSortingControls()
        {
            if (sortDropdown != null)
            {
                sortDropdown.ClearOptions();
                
                List<string> sortOptions = new List<string>
                {
                    "Name (A-Z)",
                    "Name (Z-A)",
                    "Quantity (High to Low)",
                    "Quantity (Low to High)",
                    "Type",
                    "Storage Level",
                    "Value (High to Low)"
                };
                
                sortDropdown.AddOptions(sortOptions);
                sortDropdown.value = currentSortMode;
                sortDropdown.onValueChanged.AddListener(OnSortModeChanged);
            }
        }
        
        private void SetupToggleAllButton()
        {
            if (toggleAllButton != null)
            {
                toggleAllButton.onClick.AddListener(OnToggleAllClicked);
                UpdateToggleAllButtonText();
            }
        }
        
        #endregion
        
        #region Event Management
        
        private void SubscribeToEvents()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged += OnResourceChanged;
                ResourceManager.Instance.OnResourceAdded += OnResourceAdded;
                // Note: OnStorageLimitChanged doesn't exist in ResourceManager, so we'll comment it out for now
                // ResourceManager.Instance.OnStorageLimitChanged += OnStorageLimitChanged;
            }
            else
            {
                Debug.LogWarning("ResourceInventoryUI: ResourceManager.Instance not found!");
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged -= OnResourceChanged;
                ResourceManager.Instance.OnResourceAdded -= OnResourceAdded;
                // ResourceManager.Instance.OnStorageLimitChanged -= OnStorageLimitChanged;
            }
        }
        
        #endregion
        
        #region Resource Display Management
        
        private void RefreshDisplay()
        {
            // Show/hide appropriate containers based on current tab
            if (resourceGrid != null)
                resourceGrid.gameObject.SetActive(!showingCraftedItems);
            if (craftedItemsContainer != null)
                craftedItemsContainer.gameObject.SetActive(showingCraftedItems);
                
            if (showingCraftedItems)
            {
                UpdateCraftedItemsDisplay();
            }
            else
            {
                UpdateResourceList();
                UpdateSummaryInfo();
                UpdateEmptyMessage();
            }
        }
        
        private void UpdateResourceList()
        {
            if (ResourceManager.Instance == null)
            {
                Debug.LogWarning("ResourceInventoryUI: ResourceManager.Instance is null!");
                return;
            }
            
            // Get all resources from the manager
            var allResources = GetAllResourcesWithQuantities();
            
            // Apply filters
            var filteredResources = ApplyFilters(allResources);
            
            // Apply sorting
            var sortedResources = ApplySorting(filteredResources);
            
            // Update the display
            UpdateResourceDisplays(sortedResources);
        }
        
        private Dictionary<ResourceType, int> GetAllResourcesWithQuantities()
        {
            var resources = new Dictionary<ResourceType, int>();
            
            foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
            {
                int amount = ResourceManager.Instance.GetResourceAmount(resourceType);
                
                // Include zero quantities if the setting is enabled
                if (amount > 0 || showZeroQuantityResources)
                {
                    resources[resourceType] = amount;
                }
            }
            
            return resources;
        }
        
        private Dictionary<ResourceType, int> ApplyFilters(Dictionary<ResourceType, int> resources)
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
        
        private bool ShouldShowResource(ResourceType resourceType)
        {
            // Check if we should show this resource based on filter settings
            bool isProcessed = IsProcessedResource(resourceType);
            
            if (isProcessed && !showProcessedResources) return false;
            if (!isProcessed && !showRawResources) return false;
            
            return true;
        }
        
        private bool IsProcessedResource(ResourceType resourceType)
        {
            // Get from config if available, otherwise use fallback logic
            var config = ResourceManager.Instance?.GetResourceConfig(resourceType);
            if (config != null)
            {
                return config.isProcessedResource;
            }
            
            // Fallback classification
            return resourceType == ResourceType.Fuel ||
                   resourceType == ResourceType.Food ||
                   resourceType == ResourceType.Parts ||
                   resourceType == ResourceType.Energy;
        }
        
        private Dictionary<ResourceType, int> ApplySorting(Dictionary<ResourceType, int> resources)
        {
            var resourceList = resources.ToList();
            
            switch (currentSortMode)
            {
                case 0: // Name (A-Z)
                    resourceList.Sort((a, b) => a.Key.ToString().CompareTo(b.Key.ToString()));
                    break;
                    
                case 1: // Name (Z-A)
                    resourceList.Sort((a, b) => b.Key.ToString().CompareTo(a.Key.ToString()));
                    break;
                    
                case 2: // Quantity (High to Low)
                    resourceList.Sort((a, b) => b.Value.CompareTo(a.Value));
                    break;
                    
                case 3: // Quantity (Low to High)
                    resourceList.Sort((a, b) => a.Value.CompareTo(b.Value));
                    break;
                    
                case 4: // Type
                    resourceList.Sort((a, b) => GetResourceCategory(a.Key).CompareTo(GetResourceCategory(b.Key)));
                    break;
                    
                case 5: // Storage Level
                    resourceList.Sort((a, b) => GetStorageRatio(b.Key).CompareTo(GetStorageRatio(a.Key)));
                    break;
                    
                case 6: // Value (High to Low)
                    resourceList.Sort((a, b) => GetResourceValue(b.Key, b.Value).CompareTo(GetResourceValue(a.Key, a.Value)));
                    break;
            }
            
            return resourceList.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        
        private string GetResourceCategory(ResourceType resourceType)
        {
            if (IsProcessedResource(resourceType)) return "Processed";
            return "Raw";
        }
        
        private float GetStorageRatio(ResourceType resourceType)
        {
            int current = ResourceManager.Instance.GetResourceAmount(resourceType);
            int max = ResourceManager.Instance.GetStorageLimit(resourceType);
            return max > 0 ? (float)current / max : 0f;
        }
        
        private int GetResourceValue(ResourceType resourceType, int amount)
        {
            var config = ResourceManager.Instance?.GetResourceConfig(resourceType);
            int baseValue = config?.baseValue ?? 1;
            return baseValue * amount;
        }
        
        private void UpdateResourceDisplays(Dictionary<ResourceType, int> resources)
        {
            // Remove displays that are no longer needed
            var resourcesToRemove = activeDisplays.Keys.Except(resources.Keys).ToList();
            foreach (var resourceType in resourcesToRemove)
            {
                RemoveResourceDisplay(resourceType);
            }
            
            // Update or create displays for current resources
            foreach (var kvp in resources.Take(maxDisplayedResources))
            {
                UpdateOrCreateResourceDisplay(kvp.Key, kvp.Value);
            }
        }
        
        private void UpdateOrCreateResourceDisplay(ResourceType resourceType, int amount)
        {
            if (activeDisplays.TryGetValue(resourceType, out ResourceDisplayItem existingDisplay))
            {
                // Update existing display
                existingDisplay.UpdateAmount(amount);
            }
            else
            {
                // Create new display
                CreateResourceDisplay(resourceType, amount);
            }
        }
        
        private void CreateResourceDisplay(ResourceType resourceType, int amount)
        {
            if (resourceDisplayPrefab == null || resourceGrid == null)
            {
                Debug.LogError("ResourceInventoryUI: Missing prefab or grid reference!");
                return;
            }
            
            GameObject displayObject = Instantiate(resourceDisplayPrefab, resourceGrid);
            ResourceDisplayItem displayItem = displayObject.GetComponent<ResourceDisplayItem>();
            
            if (displayItem != null)
            {
                var config = ResourceManager.Instance.GetResourceConfig(resourceType);
                displayItem.Initialize(resourceType, amount, config);
                activeDisplays[resourceType] = displayItem;
            }
            else
            {
                Debug.LogError("ResourceInventoryUI: ResourceDisplayItem component not found on prefab!");
                Destroy(displayObject);
            }
        }
        
        private void RemoveResourceDisplay(ResourceType resourceType)
        {
            if (activeDisplays.TryGetValue(resourceType, out ResourceDisplayItem display))
            {
                activeDisplays.Remove(resourceType);
                if (display != null && display.gameObject != null)
                {
                    Destroy(display.gameObject);
                }
            }
        }
        
        #endregion
        
        #region UI Updates
        
        private void UpdateSummaryInfo()
        {
            if (ResourceManager.Instance == null) return;
            
            // Update total resources count
            if (totalResourcesText != null)
            {
                int totalTypes = activeDisplays.Count;
                int totalQuantity = activeDisplays.Keys.Sum(type => ResourceManager.Instance.GetResourceAmount(type));
                totalResourcesText.text = $"Resources: {totalTypes} types, {totalQuantity:N0} total";
            }
            
            // Update storage indicator
            if (storageIndicator != null)
            {
                float averageStorageRatio = CalculateAverageStorageRatio();
                storageIndicator.value = averageStorageRatio;
                
                // Update storage indicator color
                Image fillImage = storageIndicator.fillRect?.GetComponent<Image>();
                if (fillImage != null)
                {
                    if (averageStorageRatio >= 0.9f)
                        fillImage.color = Color.red;
                    else if (averageStorageRatio >= 0.7f)
                        fillImage.color = Color.yellow;
                    else
                        fillImage.color = Color.green;
                }
            }
        }
        
        private float CalculateAverageStorageRatio()
        {
            if (activeDisplays.Count == 0) return 0f;
            
            float totalRatio = 0f;
            foreach (var resourceType in activeDisplays.Keys)
            {
                totalRatio += GetStorageRatio(resourceType);
            }
            
            return totalRatio / activeDisplays.Count;
        }
        
        private void UpdateEmptyMessage()
        {
            if (emptyMessage != null)
            {
                bool isEmpty = activeDisplays.Count == 0;
                emptyMessage.gameObject.SetActive(isEmpty);
                
                if (isEmpty)
                {
                    emptyMessage.text = "No resources to display.\nCollect waste to generate resources!";
                }
            }
        }
        
        private void UpdateToggleAllButtonText()
        {
            if (toggleAllButton != null)
            {
                var buttonText = toggleAllButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    if (showRawResources && showProcessedResources)
                    {
                        buttonText.text = "Hide All";
                    }
                    else if (!showRawResources && !showProcessedResources)
                    {
                        buttonText.text = "Show All";
                    }
                    else
                    {
                        buttonText.text = "Toggle All";
                    }
                }
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnResourceChanged(ResourceType resourceType, int oldAmount, int newAmount)
        {
            // Update specific resource display
            if (ShouldShowResource(resourceType))
            {
                if (newAmount > 0 || showZeroQuantityResources)
                {
                    UpdateOrCreateResourceDisplay(resourceType, newAmount);
                }
                else
                {
                    RemoveResourceDisplay(resourceType);
                }
            }
            else
            {
                RemoveResourceDisplay(resourceType);
            }
            
            UpdateSummaryInfo();
            UpdateEmptyMessage();
        }
        
        private void OnResourceAdded(ResourceType resourceType, int amount)
        {
            // Highlight the resource that was added
            if (activeDisplays.TryGetValue(resourceType, out ResourceDisplayItem display))
            {
                display.Highlight(2f); // Highlight for 2 seconds
            }
        }
        
        private void OnStorageLimitChanged(ResourceType resourceType, int newLimit)
        {
            // Update the display for this resource type
            if (activeDisplays.TryGetValue(resourceType, out ResourceDisplayItem display))
            {
                int currentAmount = ResourceManager.Instance.GetResourceAmount(resourceType);
                var config = ResourceManager.Instance.GetResourceConfig(resourceType);
                display.Initialize(resourceType, currentAmount, config);
            }
        }
        
        private void OnRawResourcesToggleChanged(bool isOn)
        {
            showRawResources = isOn;
            RefreshDisplay();
            UpdateToggleAllButtonText();
        }
        
        private void OnProcessedResourcesToggleChanged(bool isOn)
        {
            showProcessedResources = isOn;
            RefreshDisplay();
            UpdateToggleAllButtonText();
        }
        
        private void OnToggleAllClicked()
        {
            if (showRawResources && showProcessedResources)
            {
                // Hide all
                showRawResources = false;
                showProcessedResources = false;
            }
            else
            {
                // Show all
                showRawResources = true;
                showProcessedResources = true;
            }
            
            // Update toggle UI
            if (rawResourcesToggle != null) rawResourcesToggle.isOn = showRawResources;
            if (processedResourcesToggle != null) processedResourcesToggle.isOn = showProcessedResources;
            
            RefreshDisplay();
            UpdateToggleAllButtonText();
        }
        
        private void OnSortModeChanged(int newSortMode)
        {
            currentSortMode = newSortMode;
            RefreshDisplay();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Highlight a specific resource type in the display
        /// </summary>
        /// <param name="resourceType">Resource type to highlight</param>
        /// <param name="duration">Duration of highlight in seconds</param>
        public void HighlightResource(ResourceType resourceType, float duration = 2f)
        {
            if (activeDisplays.TryGetValue(resourceType, out ResourceDisplayItem display))
            {
                if (display != null)
                {
                    display.Highlight(duration);
                }
            }
            else
            {
                // If the resource isn't currently displayed, refresh to show it
                RefreshDisplay();
                
                // Try to highlight after refresh
                if (activeDisplays.TryGetValue(resourceType, out display))
                {
                    display?.Highlight(duration);
                }
            }
        }
        
        /// <summary>
        /// Set filter to show only specific resource types
        /// </summary>
        public void SetResourceFilter(bool showRaw, bool showProcessed)
        {
            showRawResources = showRaw;
            showProcessedResources = showProcessed;
            
            if (rawResourcesToggle != null) rawResourcesToggle.isOn = showRaw;
            if (processedResourcesToggle != null) processedResourcesToggle.isOn = showProcessed;
            
            RefreshDisplay();
            UpdateToggleAllButtonText();
        }
        
        /// <summary>
        /// Set sorting mode
        /// </summary>
        public void SetSortMode(int sortMode)
        {
            if (sortMode >= 0 && sortMode < 7)
            {
                currentSortMode = sortMode;
                if (sortDropdown != null) sortDropdown.value = sortMode;
                RefreshDisplay();
            }
        }
        
        /// <summary>
        /// Get the current number of displayed resources
        /// </summary>
        public int GetDisplayedResourceCount()
        {
            return activeDisplays.Count;
        }
        
        /// <summary>
        /// Check if a specific resource is currently displayed
        /// </summary>
        public bool IsResourceDisplayed(ResourceType resourceType)
        {
            return activeDisplays.ContainsKey(resourceType);
        }
        
        /// <summary>
        /// Force refresh the entire display (useful for debugging)
        /// </summary>
        public void ForceRefresh()
        {
            RefreshDisplay();
        }
        
        #endregion
        
        #region Crafted Items Management
        
        private void UpdateCraftedItemsDisplay()
        {
            if (!showingCraftedItems || CraftedItemInventory.Instance == null) return;
            
            var allCraftedItems = CraftedItemInventory.Instance.GetAllCraftedItems();
            
            // Remove displays for items no longer in inventory
            var toRemove = new List<ResourceType>();
            foreach (var kvp in activeCraftedDisplays)
            {
                if (!allCraftedItems.ContainsKey(kvp.Key) || allCraftedItems[kvp.Key] <= 0)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            
            foreach (var resourceType in toRemove)
            {
                RemoveCraftedItemDisplay(resourceType);
            }
            
            // Add or update displays for current items
            foreach (var kvp in allCraftedItems)
            {
                if (kvp.Value > 0)
                {
                    UpdateCraftedItemDisplay(kvp.Key, kvp.Value);
                }
            }
        }
        
        private void UpdateCraftedItemDisplay(ResourceType itemType, int amount)
        {
            if (!activeCraftedDisplays.TryGetValue(itemType, out GameObject displayObject))
            {
                // Create new display
                displayObject = Instantiate(craftedItemDisplayPrefab, craftedItemsContainer);
                activeCraftedDisplays[itemType] = displayObject;
            }
            
            // Update the display
            var displayComponent = displayObject.GetComponent<CraftedItemDisplayItem>();
            if (displayComponent != null)
            {
                displayComponent.UpdateDisplay(itemType, amount);
            }
        }
        
        private void RemoveCraftedItemDisplay(ResourceType itemType)
        {
            if (activeCraftedDisplays.TryGetValue(itemType, out GameObject displayObject))
            {
                activeCraftedDisplays.Remove(itemType);
                Destroy(displayObject);
            }
        }
        
        #endregion
        
        #region Debug Methods
        
        [ContextMenu("Debug: Add Test Resources")]
        private void DebugAddTestResources()
        {
            if (ResourceManager.Instance == null)
            {
                Debug.LogWarning("ResourceManager.Instance not found!");
                return;
            }
            
            // Add some test resources
            ResourceManager.Instance.AddResource(ResourceType.Plastic, 150);
            ResourceManager.Instance.AddResource(ResourceType.MetalScraps, 75);
            ResourceManager.Instance.AddResource(ResourceType.OrganicMatter, 200);
            ResourceManager.Instance.AddResource(ResourceType.CrystalFragments, 25);
            ResourceManager.Instance.AddResource(ResourceType.Fuel, 50);
            
            Debug.Log("Added test resources!");
        }
        
        [ContextMenu("Debug: Clear All Resources")]
        private void DebugClearAllResources()
        {
            if (ResourceManager.Instance == null)
            {
                Debug.LogWarning("ResourceManager.Instance not found!");
                return;
            }
            
            // Clear all resources
            foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
            {
                ResourceManager.Instance.SetResource(resourceType, 0);
            }
            
            Debug.Log("Cleared all resources!");
        }
        
        [ContextMenu("Debug: Log Current State")]
        private void DebugLogCurrentState()
        {
            Debug.Log($"ResourceInventoryUI State:");
            Debug.Log($"- Active Displays: {activeDisplays.Count}");
            Debug.Log($"- Show Raw: {showRawResources}");
            Debug.Log($"- Show Processed: {showProcessedResources}");
            Debug.Log($"- Sort Mode: {currentSortMode}");
            Debug.Log($"- Resource Manager: {(ResourceManager.Instance != null ? "Found" : "Not Found")}");
        }
        
        #endregion
    }
}