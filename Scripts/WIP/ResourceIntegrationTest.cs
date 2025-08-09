using UnityEngine;
using System.Linq;

/// <summary>
/// Integration test for resource-waste system
/// TEST SETUP IN UNITY:
/// 1. Create a GameObject named "ResourceIntegrationTester"
/// 2. Add the ResourceIntegrationTest script to it
/// 3. Set runTestOnStart = true in inspector
/// 4. Play the scene and check the console for test results
/// 5. Press T during play to run the test again
/// 
/// Expected output:
/// - "Created waste: Test Plastic Bottle"
/// - "Resource yields count: 2" (or similar)
/// - Lists of generated resources
/// - "Current Resource Inventory" showing accumulated resources
/// 
/// If you see errors about missing methods, ensure you've added all the
/// enhancement code snippets to your existing files.
/// </summary>
public class ResourceIntegrationTest : MonoBehaviour
{
    [Header("Test Controls")]
    public bool runTestOnStart = false;
    public KeyCode testKey = KeyCode.T;
    
    private void Start()
    {
        if (runTestOnStart)
        {
            Invoke("RunIntegrationTest", 1f); // Delay to ensure managers are ready
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            RunIntegrationTest();
        }
    }
    
    public void RunIntegrationTest()
    {
        Debug.Log("=== ENHANCED RESOURCE-WASTE INTEGRATION TEST ===");
        
        // Test 1: Create a waste item using enhanced generation
        Debug.Log("--- Test 1: Enhanced Waste Generation ---");
        if (WasteGenerator.Instance != null)
        {
            var enhancedWaste = WasteGenerator.Instance.GenerateUpdatedWasteItem();
            if (enhancedWaste != null)
            {
                Debug.Log($"Enhanced waste: {enhancedWaste.Name}");
                Debug.Log($"Quality: {enhancedWaste.Quality:F2}");
                Debug.Log($"Rarity: {enhancedWaste.Rarity} (Color: {enhancedWaste.RarityColor})");
                
                // Test resource preview
                var preview = enhancedWaste.GetResourcePreview();
                if (!string.IsNullOrEmpty(preview))
                {
                    Debug.Log($"Resource preview: {preview}");
                }
            }
        }
        
        // Test 2: Create a manual test waste item
        Debug.Log("--- Test 2: Manual Waste Creation ---");
        var testWaste = new UpdatedWasteItem("Test Plastic Bottle", WasteType.Plastic, 1);
        testWaste.Rarity = WasteRarity.Uncommon;
        testWaste.DimensionalOrigin = "Earth - Residential";
        
        Debug.Log($"Created waste: {testWaste.Name}");
        Debug.Log($"Waste type: {testWaste.Type}, Rarity: {testWaste.Rarity}");
        Debug.Log($"Quality factor: {testWaste.Quality:F2}");
        
        // Test 3: Check resource yield
        Debug.Log("--- Test 3: Resource Yield Analysis ---");
        var yields = testWaste.ResourceYields;
        Debug.Log($"Resource yields count: {yields.Count}");
        foreach (var yield in yields)
        {
            Debug.Log($"  {yield.Key}: {yield.Value.baseAmount} base (multiplier: {yield.Value.yieldMultiplier:F2}, chance: {yield.Value.chancePercentage}%)");
        }
        
        // Test 4: Enhanced processing with yield rolls
        Debug.Log("--- Test 4: Enhanced Processing Test ---");
        if (WasteInventoryManager.Instance != null)
        {
            WasteInventoryManager.Instance.AddWasteItem(testWaste);
            Debug.Log("Added to inventory");
            
            var resources = WasteInventoryManager.Instance.ProcessWasteToResources(testWaste);
            Debug.Log($"Processing result: {resources.Count} resource types generated");
            
            foreach (var resource in resources)
            {
                Debug.Log($"  Generated: {resource.Value} {resource.Key}");
            }
        }
        
        // Test 5: Check ResourceManager state
        Debug.Log("--- Test 5: Resource Manager State ---");
        if (ResourceManager.Instance != null)
        {
            Debug.Log("=== Current Resource Inventory ===");
            foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
            {
                int amount = ResourceManager.Instance.GetResourceAmount(resourceType);
                if (amount > 0)
                {
                    Debug.Log($"  {resourceType}: {amount}");
                }
            }
            
            Debug.Log($"Recycling multiplier: {ResourceManager.Instance.RecyclingMultiplier:F2}");
        }
        
        // Test 6: Location integration
        Debug.Log("--- Test 6: Location Integration ---");
        if (LocationManager.Instance != null)
        {
            var currentLocation = LocationManager.Instance.GetCurrentLocation();
            if (currentLocation != null)
            {
                Debug.Log($"Current location: {currentLocation.displayName}");
                Debug.Log($"Danger level: {currentLocation.dangerLevel:F2}");
                Debug.Log($"Value multiplier: {currentLocation.averageValueMultiplier:F2}");
            }
        }
        
        Debug.Log("=== ENHANCED INTEGRATION TEST COMPLETE ===");
    }
} 