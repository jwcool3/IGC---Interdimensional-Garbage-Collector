using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Test script to demonstrate the new resource management system
/// Shows how to create waste items using data templates and generate resource yields
/// </summary>
public class ResourceSystemTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private UpdatedWasteItemData[] testWasteData;
    [SerializeField] private bool runTestOnStart = true;
    [SerializeField] private bool logDetailedResults = true;
    
    [Header("Test Results")]
    [SerializeField] private List<UpdatedWasteItem> generatedItems = new List<UpdatedWasteItem>();
    
    private void Start()
    {
        if (runTestOnStart)
        {
            RunResourceSystemTest();
        }
    }
    
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
    
    /// <summary>
    /// Test creating waste items from data templates
    /// </summary>
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
    
    /// <summary>
    /// Test creating legacy waste items (backward compatibility)
    /// </summary>
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
    
    /// <summary>
    /// Test resource yield generation and calculations
    /// </summary>
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
    
    /// <summary>
    /// Test item stacking functionality
    /// </summary>
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
    
    /// <summary>
    /// Test item copying functionality
    /// </summary>
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
    
    /// <summary>
    /// Create sample waste data for testing if none provided
    /// </summary>
    private void CreateSampleWasteData()
    {
        // This would normally be done through ScriptableObject assets
        // For testing purposes, we'll create them in code
        Debug.Log("Creating sample waste data in code (normally these would be ScriptableObject assets)");
        
        // Note: In a real implementation, you'd create these as ScriptableObject assets
        // and assign them in the inspector. This is just for testing purposes.
    }
    
    /// <summary>
    /// Get test statistics
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
} 