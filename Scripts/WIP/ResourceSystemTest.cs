using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using ResourceSystem.UI;

/// <summary>
/// Enhanced test script for the complete resource management system
/// Tests waste items, resource generation, UI integration, and manager functionality
/// </summary>
public class ResourceSystemTest : MonoBehaviour
{
    [Header("Original Test Configuration")]
    [SerializeField] private UpdatedWasteItemData[] testWasteData;
    [SerializeField] private bool runTestOnStart = true;
    [SerializeField] private bool logDetailedResults = true;
    
    [Header("UI Integration Testing")]
    [SerializeField] private ResourceInventoryUI resourceInventoryUI;
    [SerializeField] private Button testAddResourcesButton;
    [SerializeField] private Button testProcessWasteButton;
    [SerializeField] private Button testClearResourcesButton;
    [SerializeField] private TextMeshProUGUI statusDisplay;
    
    [Header("Manager Testing")]
    [SerializeField] private bool testResourceManager = true;
    [SerializeField] private bool testUIUpdates = true;
    [SerializeField] private bool testWasteProcessing = true;
    
    [Header("Test Results")]
    [SerializeField] private List<UpdatedWasteItem> generatedItems = new List<UpdatedWasteItem>();
    
    private void Start()
    {
        SetupUIButtons();
        
        if (runTestOnStart)
        {
            RunComprehensiveTest();
        }
    }
    
    private void SetupUIButtons()
    {
        if (testAddResourcesButton != null)
            testAddResourcesButton.onClick.AddListener(TestUIResourceAddition);
            
        if (testProcessWasteButton != null)
            testProcessWasteButton.onClick.AddListener(TestUIWasteProcessing);
            
        if (testClearResourcesButton != null)
            testClearResourcesButton.onClick.AddListener(TestUIClearResources);
    }
    
    /// <summary>
    /// Run comprehensive test of the entire resource system
    /// </summary>
    [ContextMenu("Run Comprehensive Test")]
    public void RunComprehensiveTest()
    {
        Debug.Log("=== COMPREHENSIVE RESOURCE SYSTEM TEST STARTED ===");
        
        // Original tests
        RunResourceSystemTest();
        
        // New UI and Manager tests
        if (testResourceManager)
            TestResourceManagerIntegration();
            
        if (testUIUpdates)
            TestUIIntegration();
            
        if (testWasteProcessing)
            TestWasteToResourceProcessing();
        
        UpdateStatusDisplay("Comprehensive test completed!");
        Debug.Log("=== COMPREHENSIVE RESOURCE SYSTEM TEST COMPLETED ===");
    }
    
    #region Original Tests (keeping your existing functionality)
    
    /// <summary>
    /// Run a comprehensive test of the resource system
    /// </summary>
    [ContextMenu("Run Resource System Test")]
    public void RunResourceSystemTest()
    {
        Debug.Log("=== Resource System Test Started ===");
        
        // Clear previous results
        generatedItems.Clear();
        
        // Test 1: Create items from data templates
        TestDataTemplateCreation();
        
        // Test 2: Create legacy items (backward compatibility)
        TestLegacyItemCreation();
        
        // Test 3: Test resource yield generation
        TestResourceYieldGeneration();
        
        // Test 4: Test item stacking
        TestItemStacking();
        
        // Test 5: Test item copying
        TestItemCopying();
        
        Debug.Log("=== Resource System Test Completed ===");
    }
    
    private void TestDataTemplateCreation()
    {
        Debug.Log("--- Testing Data Template Creation ---");
        
        if (testWasteData == null || testWasteData.Length == 0)
        {
            Debug.LogWarning("No test waste data provided. Creating sample data...");
            CreateSampleWasteData();
        }
        
        foreach (var data in testWasteData)
        {
            if (data == null) continue;
            
            // Test different origins
            WasteOrigin[] testOrigins = { WasteOrigin.Residential, WasteOrigin.Commercial, WasteOrigin.Industrial };
            
            foreach (var origin in testOrigins)
            {
                var item = new UpdatedWasteItem(data, origin);
                generatedItems.Add(item);
                
                if (logDetailedResults)
                {
                    Debug.Log($"Created: {item.Name} from {origin} origin");
                    Debug.Log($"  - Quality: {item.Quality:F2}");
                    Debug.Log($"  - Estimated Value: {item.EstimatedValue:F2}");
                    Debug.Log($"  - Resource Preview: {item.GetResourcePreview()}");
                }
            }
        }
        
        Debug.Log($"Created {generatedItems.Count} items from data templates");
    }
    
    private void TestLegacyItemCreation()
    {
        Debug.Log("--- Testing Legacy Item Creation ---");
        
        string[] legacyNames = { "Old Plastic Bottle", "Rusty Metal Can", "Broken Electronics" };
        string[] legacyOrigins = { "Earth-Prime", "Technological Dimension", "Quantum Realm" };
        WasteRarity[] legacyRarities = { WasteRarity.Common, WasteRarity.Uncommon, WasteRarity.Rare };
        
        for (int i = 0; i < legacyNames.Length; i++)
        {
            var legacyItem = new UpdatedWasteItem(legacyNames[i], legacyOrigins[i], legacyRarities[i]);
            generatedItems.Add(legacyItem);
            
            if (logDetailedResults)
            {
                Debug.Log($"Created Legacy: {legacyItem.Name}");
                Debug.Log($"  - Origin: {legacyItem.DimensionalOrigin}");
                Debug.Log($"  - Rarity: {legacyItem.Rarity}");
                Debug.Log($"  - Resource Preview: {legacyItem.GetResourcePreview()}");
            }
        }
        
        Debug.Log($"Created {legacyNames.Length} legacy items");
    }
    
    private void TestResourceYieldGeneration()
    {
        Debug.Log("--- Testing Resource Yield Generation ---");
        
        if (generatedItems.Count == 0)
        {
            Debug.LogWarning("No items to test yields with");
            return;
        }
        
        var testItem = generatedItems[0];
        
        Debug.Log($"Testing yields for: {testItem.Name}");
        
        foreach (var yieldPair in testItem.ResourceYields)
        {
            var yield = yieldPair.Value;
            
            // Test different processing conditions
            float[] qualityLevels = { 0.5f, 0.8f, 1.0f };
            float[] efficiencyLevels = { 0.6f, 0.8f, 1.0f };
            
            foreach (var quality in qualityLevels)
            {
                foreach (var efficiency in efficiencyLevels)
                {
                    int actualYield = yield.CalculateActualYield(quality, efficiency);
                    bool willYield = yield.RollForYield();
                    
                    if (logDetailedResults)
                    {
                        Debug.Log($"  {yield.resourceType}: {actualYield} units " +
                                 $"(Q:{quality:F1}, E:{efficiency:F1}, Will Yield: {willYield})");
                    }
                }
            }
        }
    }
    
    private void TestItemStacking()
    {
        Debug.Log("--- Testing Item Stacking ---");
        
        if (generatedItems.Count < 2)
        {
            Debug.LogWarning("Need at least 2 items to test stacking");
            return;
        }
        
        var item1 = generatedItems[0];
        var item2 = generatedItems[1];
        var item1Copy = item1.CreateCopy();
        
        bool canStackDifferent = item1.CanStackWith(item2);
        bool canStackSame = item1.CanStackWith(item1Copy);
        
        Debug.Log($"Can stack different items ({item1.Name} + {item2.Name}): {canStackDifferent}");
        Debug.Log($"Can stack same items ({item1.Name} + copy): {canStackSame}");
    }
    
    private void TestItemCopying()
    {
        Debug.Log("--- Testing Item Copying ---");
        
        if (generatedItems.Count == 0)
        {
            Debug.LogWarning("No items to test copying with");
            return;
        }
        
        var original = generatedItems[0];
        var copy = original.CreateCopy();
        
        Debug.Log($"Original ID: {original.Id}");
        Debug.Log($"Copy ID: {copy.Id}");
        Debug.Log($"Names match: {original.Name == copy.Name}");
        Debug.Log($"IDs different: {original.Id != copy.Id}");
        Debug.Log($"Can stack with copy: {original.CanStackWith(copy)}");
    }
    
    private void CreateSampleWasteData()
    {
        Debug.Log("Creating sample waste data in code (normally these would be ScriptableObject assets)");
        // Note: In a real implementation, you'd create these as ScriptableObject assets
    }
    
    #endregion
    
    #region New UI and Manager Integration Tests
    
    /// <summary>
    /// Test ResourceManager integration and functionality
    /// </summary>
    [ContextMenu("Test Resource Manager Integration")]
    public void TestResourceManagerIntegration()
    {
        Debug.Log("--- Testing Resource Manager Integration ---");
        
        if (ResourceManager.Instance == null)
        {
            Debug.LogError("ResourceManager.Instance not found!");
            UpdateStatusDisplay("ERROR: ResourceManager not found!");
            return;
        }
        
        // Test adding resources
        Debug.Log("Testing AddResource...");
        bool addResult1 = ResourceManager.Instance.AddResource(ResourceType.Plastic, 50);
        bool addResult2 = ResourceManager.Instance.AddResource(ResourceType.MetalScraps, 30);
        bool addResult3 = ResourceManager.Instance.AddResource(ResourceType.OrganicMatter, 25);
        
        Debug.Log($"Add Plastic: {addResult1}, Metal: {addResult2}, Organic: {addResult3}");
        
        // Test getting resources
        Debug.Log("Testing GetResourceAmount...");
        int plasticAmount = ResourceManager.Instance.GetResourceAmount(ResourceType.Plastic);
        int metalAmount = ResourceManager.Instance.GetResourceAmount(ResourceType.MetalScraps);
        int organicAmount = ResourceManager.Instance.GetResourceAmount(ResourceType.OrganicMatter);
        
        Debug.Log($"Current amounts - Plastic: {plasticAmount}, Metal: {metalAmount}, Organic: {organicAmount}");
        
        // Test consuming resources
        Debug.Log("Testing SpendResource...");
        bool consumeResult = ResourceManager.Instance.SpendResource(ResourceType.Plastic, 20);
        Debug.Log($"Spend 20 Plastic: {consumeResult}");
        
        // Test storage limits
        Debug.Log("Testing storage limits...");
        int maxPlasticStorage = ResourceManager.Instance.GetStorageLimit(ResourceType.Plastic);
        Debug.Log($"Max Plastic storage: {maxPlasticStorage}");
        
        UpdateStatusDisplay("Resource Manager test completed");
    }
    
    /// <summary>
    /// Test UI integration and updates
    /// </summary>
    [ContextMenu("Test UI Integration")]
    public void TestUIIntegration()
    {
        Debug.Log("--- Testing UI Integration ---");
        
        if (resourceInventoryUI == null)
        {
            Debug.LogWarning("ResourceInventoryUI reference not set. Looking for it...");
            resourceInventoryUI = FindFirstObjectByType<ResourceInventoryUI>();
        }
        
        if (resourceInventoryUI == null)
        {
            Debug.LogError("ResourceInventoryUI not found in scene!");
            UpdateStatusDisplay("ERROR: ResourceInventoryUI not found!");
            return;
        }
        
        Debug.Log("ResourceInventoryUI found, testing UI updates...");
        
        // Force refresh the UI
        resourceInventoryUI.ForceRefresh();
        
        // Test highlighting
        resourceInventoryUI.HighlightResource(ResourceType.Plastic, 3f);
        
        // Test filtering
        resourceInventoryUI.SetResourceFilter(true, false); // Show only raw resources
        
        // Wait a bit then show all
        Invoke(nameof(ShowAllResourcesInUI), 2f);
        
        Debug.Log("UI integration test completed");
        UpdateStatusDisplay("UI integration test completed");
    }
    
    private void ShowAllResourcesInUI()
    {
        if (resourceInventoryUI != null)
        {
            resourceInventoryUI.SetResourceFilter(true, true); // Show all resources
        }
    }
    
    /// <summary>
    /// Test waste processing that generates resources
    /// </summary>
    [ContextMenu("Test Waste to Resource Processing")]
    public void TestWasteToResourceProcessing()
    {
        Debug.Log("--- Testing Waste to Resource Processing ---");
        
        if (generatedItems.Count == 0)
        {
            Debug.LogWarning("No waste items to process. Creating test items...");
            TestLegacyItemCreation(); // Create some test items
        }
        
        if (ResourceProcessingManager.Instance == null)
        {
            Debug.LogError("ResourceProcessingManager.Instance not found!");
            UpdateStatusDisplay("ERROR: ResourceProcessingManager not found!");
            return;
        }
        
        foreach (var wasteItem in generatedItems.Take(3)) // Process first 3 items
        {
            Debug.Log($"Processing waste item: {wasteItem.Name}");
            
            // Get resource preview before processing
            string preview = wasteItem.GetResourcePreview();
            Debug.Log($"Expected yield: {preview}");
            
            // Process the item
            bool success = ResourceProcessingManager.Instance.ProcessWasteItem(wasteItem);
            Debug.Log($"Processing result: {success}");
            
            if (success && resourceInventoryUI != null)
            {
                // Highlight any resources that were generated
                foreach (var yieldPair in wasteItem.ResourceYields)
                {
                    resourceInventoryUI.HighlightResource(yieldPair.Key, 2f);
                }
            }
        }
        
        UpdateStatusDisplay("Waste processing test completed");
    }
    
    #endregion
    
    #region UI Button Test Methods
    
    /// <summary>
    /// Test method for UI button - adds various resources
    /// </summary>
    public void TestUIResourceAddition()
    {
        if (ResourceManager.Instance == null)
        {
            Debug.LogError("ResourceManager.Instance not found!");
            return;
        }
        
        // Add a variety of resources
        ResourceManager.Instance.AddResource(ResourceType.Plastic, Random.Range(5, 20));
        ResourceManager.Instance.AddResource(ResourceType.MetalScraps, Random.Range(3, 15));
        ResourceManager.Instance.AddResource(ResourceType.OrganicMatter, Random.Range(8, 25));
        ResourceManager.Instance.AddResource(ResourceType.CrystalFragments, Random.Range(1, 5));
        
        UpdateStatusDisplay("Added random resources via UI button");
        Debug.Log("Added resources via UI button test");
    }
    
    /// <summary>
    /// Test method for UI button - processes waste items
    /// </summary>
    public void TestUIWasteProcessing()
    {
        // Create a test waste item and process it
        var testWaste = new UpdatedWasteItem("UI Test Waste", "Earth", WasteRarity.Uncommon);
        
        if (ResourceProcessingManager.Instance != null)
        {
            bool success = ResourceProcessingManager.Instance.ProcessWasteItem(testWaste);
            UpdateStatusDisplay($"Processed waste via UI: {success}");
            Debug.Log($"UI waste processing test: {success}");
        }
        else
        {
            UpdateStatusDisplay("ERROR: ResourceProcessingManager not found!");
        }
    }
    
    /// <summary>
    /// Test method for UI button - clears all resources
    /// </summary>
    public void TestUIClearResources()
    {
        if (ResourceManager.Instance == null)
        {
            UpdateStatusDisplay("ERROR: ResourceManager not found!");
            return;
        }
        
        // Clear all resources
        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            ResourceManager.Instance.SetResource(resourceType, 0);
        }
        
        UpdateStatusDisplay("Cleared all resources via UI button");
        Debug.Log("Cleared all resources via UI button test");
    }
    
    #endregion
    
    #region Utility Methods
    
    private void UpdateStatusDisplay(string message)
    {
        if (statusDisplay != null)
        {
            statusDisplay.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
        }
    }
    
    /// <summary>
    /// Get test statistics (keeping your original method)
    /// </summary>
    [ContextMenu("Show Test Statistics")]
    public void ShowTestStatistics()
    {
        if (generatedItems.Count == 0)
        {
            Debug.Log("No items generated yet. Run test first.");
            return;
        }
        
        Debug.Log("=== Test Statistics ===");
        Debug.Log($"Total items generated: {generatedItems.Count}");
        
        // Count by rarity
        var rarityCount = new Dictionary<WasteRarity, int>();
        var typeCount = new Dictionary<WasteType, int>();
        var originCount = new Dictionary<WasteOrigin, int>();
        
        float totalValue = 0f;
        int totalResourceTypes = 0;
        
        foreach (var item in generatedItems)
        {
            // Count rarities
            if (!rarityCount.ContainsKey(item.Rarity))
                rarityCount[item.Rarity] = 0;
            rarityCount[item.Rarity]++;
            
            // Count types
            if (!typeCount.ContainsKey(item.Type))
                typeCount[item.Type] = 0;
            typeCount[item.Type]++;
            
            // Count origins
            if (!originCount.ContainsKey(item.Origin))
                originCount[item.Origin] = 0;
            originCount[item.Origin]++;
            
            totalValue += item.EstimatedValue;
            totalResourceTypes += item.ResourceYields.Count;
        }
        
        Debug.Log($"Average estimated value: {totalValue / generatedItems.Count:F2}");
        Debug.Log($"Average resource types per item: {(float)totalResourceTypes / generatedItems.Count:F1}");
        
        // Log resource manager status
        if (ResourceManager.Instance != null)
        {
            Debug.Log("=== Current Resource Manager Status ===");
            foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
            {
                int amount = ResourceManager.Instance.GetResourceAmount(resourceType);
                if (amount > 0)
                {
                    Debug.Log($"{resourceType}: {amount}");
                }
            }
        }
        
        Debug.Log("Rarity distribution:");
        foreach (var kvp in rarityCount)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
        
        Debug.Log("Type distribution:");
        foreach (var kvp in typeCount)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
        
        Debug.Log("Origin distribution:");
        foreach (var kvp in originCount)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
    }
    
    /// <summary>
    /// Quick method to add test resources for development
    /// </summary>
    [ContextMenu("Quick Add Test Resources")]
    public void QuickAddTestResources()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.AddResource(ResourceType.Plastic, 100);
            ResourceManager.Instance.AddResource(ResourceType.MetalScraps, 75);
            ResourceManager.Instance.AddResource(ResourceType.OrganicMatter, 120);
            ResourceManager.Instance.AddResource(ResourceType.CrystalFragments, 15);
            ResourceManager.Instance.AddResource(ResourceType.NeuralResidue, 8);
            
            Debug.Log("Added comprehensive test resources");
            UpdateStatusDisplay("Added comprehensive test resources");
        }
    }
    
    #endregion
}