using UnityEngine;
using System.Linq;

/// <summary>
/// Integration test for resource-waste system
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
        Debug.Log("=== RESOURCE-WASTE INTEGRATION TEST ===");
        
        // Test 1: Create a waste item
        var testWaste = new UpdatedWasteItem("Test Plastic Bottle", WasteType.Plastic, 1);
        testWaste.Rarity = WasteRarity.Uncommon;
        testWaste.DimensionalOrigin = "Earth - Residential";
        
        Debug.Log($"Created waste: {testWaste.Name}");
        Debug.Log($"Waste type: {testWaste.Type}, Rarity: {testWaste.Rarity}");
        
        // Test 2: Check resource yield
        var yields = testWaste.ResourceYields;
        Debug.Log($"Resource yields count: {yields.Count}");
        foreach (var yield in yields)
        {
            Debug.Log($"  {yield.Key}: {yield.Value.baseAmount} (chance: {yield.Value.chancePercentage}%)");
        }
        
        // Test 3: Add to inventory and process
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
        
        // Test 4: Check ResourceManager state
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
        }
        
        Debug.Log("=== INTEGRATION TEST COMPLETE ===");
    }
} 