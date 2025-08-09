using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Test script for integration between WasteInventoryManager and new Resource System
/// Use this to verify everything is working correctly
/// </summary>
public class ResourceSystemIntegrationTest : MonoBehaviour
{
    [Header("Test Controls")]
    [SerializeField] private Button addTestWasteButton;
    [SerializeField] private Button processOneItemButton;
    [SerializeField] private Button processAllPlasticButton;
    [SerializeField] private Button batchProcessButton;
    [SerializeField] private Button showInventoryPreviewButton;
    [SerializeField] private Button toggleSystemModeButton;
    
    [Header("Display")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI inventoryStatsText;
    [SerializeField] private TextMeshProUGUI systemModeText;
    [SerializeField] private ScrollRect logScrollRect;
    [SerializeField] private TextMeshProUGUI logText;
    
    [Header("Test Settings")]
    [SerializeField] private int testWasteQuantity = 5;
    [SerializeField] private bool logDetailedResults = true;
    [SerializeField] private bool autoRefreshUI = true;
    
    private List<string> logMessages = new List<string>();
    
    private void Start()
    {
        SetupButtons();
        UpdateSystemModeDisplay();
        AddLogMessage("Integration Test initialized");
        
        if (autoRefreshUI)
        {
            InvokeRepeating(nameof(RefreshDisplays), 1f, 2f);
        }
    }
    
    private void SetupButtons()
    {
        if (addTestWasteButton != null)
            addTestWasteButton.onClick.AddListener(AddTestWaste);
            
        if (processOneItemButton != null)
            processOneItemButton.onClick.AddListener(ProcessOneItem);
            
        if (processAllPlasticButton != null)
            processAllPlasticButton.onClick.AddListener(ProcessAllPlastic);
            
        if (batchProcessButton != null)
            batchProcessButton.onClick.AddListener(BatchProcessTest);
            
        if (showInventoryPreviewButton != null)
            showInventoryPreviewButton.onClick.AddListener(ShowInventoryPreview);
            
        if (toggleSystemModeButton != null)
            toggleSystemModeButton.onClick.AddListener(ToggleSystemMode);
    }
    
    #region Test Methods
    
    /// <summary>
    /// Add test waste items to inventory
    /// </summary>
    [ContextMenu("Add Test Waste")]
    public void AddTestWaste()
    {
        if (WasteInventoryManager.Instance == null)
        {
            AddLogMessage("ERROR: WasteInventoryManager.Instance not found!", true);
            return;
        }
        
        // Create various types of test waste
        var testWasteTypes = new[]
        {
            (WasteType.Plastic, WasteRarity.Common, "Test Plastic Bottle"),
            (WasteType.Metal, WasteRarity.Uncommon, "Test Metal Can"),
            (WasteType.Organic, WasteRarity.Common, "Test Organic Waste"),
            (WasteType.Electronics, WasteRarity.Rare, "Test Circuit Board")
        };
        
        int addedCount = 0;
        
        foreach (var (type, rarity, name) in testWasteTypes)
        {
            for (int i = 0; i < testWasteQuantity; i++)
            {
                var testWaste = CreateTestWasteItem(name, type, rarity);
                
                if (WasteInventoryManager.Instance.AddWasteItem(testWaste))
                {
                    addedCount++;
                }
            }
        }
        
        AddLogMessage($"Added {addedCount} test waste items to inventory");
        RefreshDisplays();
    }
    
    /// <summary>
    /// Process one waste item using current system
    /// </summary>
    [ContextMenu("Process One Item")]
    public void ProcessOneItem()
    {
        if (WasteInventoryManager.Instance == null || GameManager.Instance == null)
        {
            AddLogMessage("ERROR: Required managers not found!", true);
            return;
        }
        
        var allWaste = WasteInventoryManager.Instance.GetAllWaste();
        if (allWaste.Count == 0)
        {
            AddLogMessage("No waste items in inventory to process");
            return;
        }
        
        var wasteToProcess = allWaste[0]; // Process first item
        string wasteName = wasteToProcess.Name;
        
        // Get resource preview before processing
        string preview = wasteToProcess.GetResourcePreview();
        
        // Process the waste
        float rp = GameManager.Instance.ProcessWaste(wasteToProcess);
        
        AddLogMessage($"Processed '{wasteName}' -> {rp:F1} RP");
        if (logDetailedResults && !string.IsNullOrEmpty(preview))
        {
            AddLogMessage($"  Expected resources: {preview}");
        }
        
        RefreshDisplays();
    }
    
    /// <summary>
    /// Process all plastic waste in inventory
    /// </summary>
    [ContextMenu("Process All Plastic")]
    public void ProcessAllPlastic()
    {
        if (GameManager.Instance == null)
        {
            AddLogMessage("ERROR: GameManager.Instance not found!", true);
            return;
        }
        
        float totalRP = GameManager.Instance.ProcessAllWasteOfType(WasteType.Plastic);
        AddLogMessage($"Processed all Plastic waste -> {totalRP:F1} total RP");
        
        RefreshDisplays();
    }
    
    /// <summary>
    /// Test batch processing with multiple items
    /// </summary>
    [ContextMenu("Batch Process Test")]
    public void BatchProcessTest()
    {
        if (WasteInventoryManager.Instance == null || GameManager.Instance == null)
        {
            AddLogMessage("ERROR: Required managers not found!", true);
            return;
        }
        
        var allWaste = WasteInventoryManager.Instance.GetAllWaste();
        var itemsToProcess = allWaste.Take(5).ToList(); // Process first 5 items
        
        if (itemsToProcess.Count == 0)
        {
            AddLogMessage("No waste items available for batch processing");
            return;
        }
        
        AddLogMessage($"Batch processing {itemsToProcess.Count} items...");
        
        float totalRP = GameManager.Instance.BatchProcessWaste(itemsToProcess);
        AddLogMessage($"Batch processing complete -> {totalRP:F1} total RP");
        
        RefreshDisplays();
    }
    
    /// <summary>
    /// Show resource preview for entire inventory
    /// </summary>
    [ContextMenu("Show Inventory Preview")]
    public void ShowInventoryPreview()
    {
        if (GameManager.Instance == null)
        {
            AddLogMessage("ERROR: GameManager.Instance not found!", true);
            return;
        }
        
        var preview = GameManager.Instance.GetInventoryResourcePreview();
        
        if (preview.Count == 0)
        {
            AddLogMessage("No resources would be generated from current inventory");
            return;
        }
        
        AddLogMessage("Inventory Resource Preview:");
        foreach (var resource in preview.OrderByDescending(r => r.Value))
        {
            AddLogMessage($"  {resource.Key}: {resource.Value}");
        }
    }
    
    /// <summary>
    /// Toggle between new and legacy systems
    /// </summary>
    [ContextMenu("Toggle System Mode")]
    public void ToggleSystemMode()
    {
        if (GameManager.Instance == null)
        {
            AddLogMessage("ERROR: GameManager.Instance not found!", true);
            return;
        }
        
        // Toggle the system mode
        bool currentMode = GameManager.Instance.useNewResourceSystem;
        GameManager.Instance.useNewResourceSystem = !currentMode;
        
        string newMode = GameManager.Instance.useNewResourceSystem ? "New Resource System" : "Legacy System";
        AddLogMessage($"Switched to: {newMode}");
        
        UpdateSystemModeDisplay();
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Create a test waste item
    /// </summary>
    private UpdatedWasteItem CreateTestWasteItem(string name, WasteType type, WasteRarity rarity)
    {
        var wasteItem = new UpdatedWasteItem(name, "Test-Dimension", rarity);
        
        // Manually set properties for testing
        wasteItem.SetType(type);
        wasteItem.SetQuality(Random.Range(0.5f, 1.0f));
        wasteItem.SetContamination(Random.Range(0f, 0.3f));
        wasteItem.SetQuantity(1);
        
        // Initialize resource yields
        wasteItem.InitializeProperties();
        
        return wasteItem;
    }
    
    /// <summary>
    /// Add message to log display
    /// </summary>
    private void AddLogMessage(string message, bool isError = false)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string fullMessage = $"[{timestamp}] {message}";
        
        logMessages.Add(fullMessage);
        
        // Keep only last 50 messages
        if (logMessages.Count > 50)
        {
            logMessages.RemoveAt(0);
        }
        
        // Update log display
        if (logText != null)
        {
            logText.text = string.Join("\n", logMessages);
            logText.color = isError ? Color.red : Color.white;
        }
        
        // Auto-scroll to bottom
        if (logScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            logScrollRect.verticalNormalizedPosition = 0f;
        }
        
        // Use DebugManager instead of Debug.Log
        if (isError)
        {
            DebugManager.LogError(fullMessage, DebugCategory.ResourceSystem);
        }
        else
        {
            DebugManager.Log(fullMessage, DebugCategory.ResourceSystem);
        }
    }
    
    /// <summary>
    /// Refresh all UI displays
    /// </summary>
    private void RefreshDisplays()
    {
        UpdateStatusDisplay();
        UpdateInventoryStats();
        UpdateSystemModeDisplay();
    }
    
    /// <summary>
    /// Update status display
    /// </summary>
    private void UpdateStatusDisplay()
    {
        if (statusText == null) return;
        
        string status = "System Status: ";
        
        if (NewResourceManager.Instance != null)
            status += "✓ ResourceManager ";
        else
            status += "✗ ResourceManager ";
            
        if (WasteInventoryManager.Instance != null)
            status += "✓ WasteManager ";
        else
            status += "✗ WasteManager ";
            
        if (ResourceProcessingManager.Instance != null)
            status += "✓ ProcessingManager ";
        else
            status += "✗ ProcessingManager ";
        
        statusText.text = status;
        statusText.color = status.Contains("✗") ? Color.red : Color.green;
    }
    
    /// <summary>
    /// Update inventory statistics
    /// </summary>
    private void UpdateInventoryStats()
    {
        if (inventoryStatsText == null) return;
        
        if (WasteInventoryManager.Instance == null)
        {
            inventoryStatsText.text = "Waste Inventory: Not Available";
            return;
        }
        
        var allWaste = WasteInventoryManager.Instance.GetAllWaste();
        int totalItems = allWaste.Count;
        int totalQuantity = allWaste.Sum(w => w.Quantity);
        float totalValue = allWaste.Sum(w => w.TotalValue * w.Quantity);
        
        string stats = $"Waste Inventory:\n";
        stats += $"Items: {totalItems} types\n";
        stats += $"Quantity: {totalQuantity} total\n";
        stats += $"Value: {totalValue:F1} RP worth";
        
        inventoryStatsText.text = stats;
    }
    
    /// <summary>
    /// Update system mode display
    /// </summary>
    private void UpdateSystemModeDisplay()
    {
        if (systemModeText == null) return;
        
        if (GameManager.Instance == null)
        {
            systemModeText.text = "System Mode: Unknown";
            return;
        }
        
        string mode = "System Mode: ";
        
        if (GameManager.Instance.useNewResourceSystem)
        {
            mode += GameManager.Instance.enableHybridMode ? "Hybrid" : "New Resources";
            systemModeText.color = Color.green;
        }
        else
        {
            mode += "Legacy (RP/DP)";
            systemModeText.color = Color.yellow;
        }
        
        systemModeText.text = mode;
    }
    
    #endregion
    
    #region Context Menu Debug Methods
    
    [ContextMenu("Test All Systems")]
    private void TestAllSystems()
    {
        AddLogMessage("=== TESTING ALL SYSTEMS ===");
        
        AddTestWaste();
        ShowInventoryPreview();
        ProcessOneItem();
        ProcessAllPlastic();
        
        AddLogMessage("=== ALL SYSTEMS TEST COMPLETE ===");
    }
    
    [ContextMenu("Clear All Waste")]
    private void ClearAllWaste()
    {
        if (WasteInventoryManager.Instance == null) return;
        
        var allWaste = WasteInventoryManager.Instance.GetAllWaste().ToList();
        foreach (var waste in allWaste)
        {
            WasteInventoryManager.Instance.RemoveWasteItem(waste);
        }
        
        AddLogMessage($"Cleared {allWaste.Count} waste items from inventory");
        RefreshDisplays();
    }
    
    [ContextMenu("Add Resources Directly")]
    private void AddResourcesDirectly()
    {
        if (NewResourceManager.Instance == null) return;
        
        NewResourceManager.Instance.AddResource(ResourceType.Plastic, 50);
        NewResourceManager.Instance.AddResource(ResourceType.MetalScraps, 25);
        NewResourceManager.Instance.AddResource(ResourceType.OrganicMatter, 75);
        
        AddLogMessage("Added resources directly to ResourceManager");
    }
    
    #endregion
}