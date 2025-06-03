# 🚀 Resource System Implementation - Your Project Structure

## 📁 File Placement in Your Existing Structure

### **Core System Files**

#### **Core/Data/Resources/** (New folder)
```
Assets/Scripts/Core/Data/Resources/
├── ResourceType.cs               # Enum + data structures
├── ResourceConfig.cs             # ScriptableObject for resource configs
├── ResourceYield.cs              # Resource yield calculations
└── ProcessingRecipe.cs           # Processing recipe definitions
```

#### **Core/Managers/** (Update existing)
```
Assets/Scripts/Core/Managers/
├── ResourceManager.cs            # REPLACE with NewResourceManager
└── ResourceProcessingManager.cs  # NEW - handles waste→resource conversion
```

#### **Core/Data/Waste/** (Update existing)
```
Assets/Scripts/Core/Data/Waste/
├── WasteItem.cs                  # UPDATE to include ResourceYield
├── WasteItemData.cs              # UPDATE to include resource definitions
└── WasteResourceMapping.cs       # NEW - maps origins to resource types
```

#### **Core/Systems/Waste/** (Update existing)
```
Assets/Scripts/Core/Systems/Waste/
├── WasteGenerator.cs             # UPDATE to generate resource yields
├── WasteProcessor.cs             # NEW - replaces old processing logic
└── ResourceYieldCalculator.cs    # NEW - handles yield calculations
```

### **UI System Files**

#### **UI/Panels/Resources/** (New folder)
```
Assets/Scripts/UI/Panels/Resources/
├── ResourceInventoryUI.cs        # Main resource display panel
└── ProcessingFacilityUI.cs       # Resource processing interface
```

#### **UI/Components/Resources/** (New folder)
```
Assets/Scripts/UI/Components/Resources/
├── ResourceDisplayItem.cs        # Individual resource display
├── ResourceRequirementDisplay.cs # Shows recipe requirements
└── ProcessingProgressDisplay.cs  # Shows processing animation
```

#### **UI/Components/Displays/** (Update existing)
```
Assets/Scripts/UI/Components/Displays/
├── ResourceDisplay.cs            # UPDATE to handle multiple resources
└── WasteResourcePreview.cs       # NEW - shows what waste yields
```

## 🔧 Integration with Existing Managers

### **Step 1: Update ResourceManager.cs**

Replace your existing `ResourceManager.cs` with this updated version that maintains compatibility:

```csharp
// Assets/Scripts/Core/Managers/ResourceManager.cs
using UnityEngine;
using System;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    
    [Header("Legacy Support")]
    [SerializeField] private bool enableLegacyMode = true;
    
    [Header("Resource Storage")]
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    
    [Header("Resource Configurations")]
    [SerializeField] private ResourceConfig[] resourceConfigs;
    
    // Legacy properties (maintain existing API)
    public float RecyclingPoints => GetLegacyRecyclingPoints();
    public float DimensionalPotential => GetLegacyDimensionalPotential();
    public float ContaminationLevel { get; private set; }
    
    // Legacy events (maintain existing API)
    public event Action<float> OnRecyclingPointsChanged;
    public event Action<float> OnDimensionalPotentialChanged;
    public event Action<float> OnContaminationChanged;
    
    // New resource events
    public event Action<ResourceType, int> OnResourceChanged;
    public event Action OnResourceInventoryChanged;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeResources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeResources()
    {
        // Initialize all resource types
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = 0;
        }
    }
    
    #region New Resource System
    
    public bool AddResource(ResourceType type, int amount)
    {
        if (amount <= 0) return false;
        
        resources[type] = resources.GetValueOrDefault(type, 0) + amount;
        OnResourceChanged?.Invoke(type, resources[type]);
        OnResourceInventoryChanged?.Invoke();
        
        // Fire legacy events for compatibility
        if (enableLegacyMode)
        {
            TriggerLegacyEvents();
        }
        
        return true;
    }
    
    public bool SpendResource(ResourceType type, int amount)
    {
        int current = GetResourceAmount(type);
        if (current >= amount)
        {
            resources[type] = current - amount;
            OnResourceChanged?.Invoke(type, resources[type]);
            OnResourceInventoryChanged?.Invoke();
            
            if (enableLegacyMode)
            {
                TriggerLegacyEvents();
            }
            return true;
        }
        return false;
    }
    
    public int GetResourceAmount(ResourceType type)
    {
        return resources.GetValueOrDefault(type, 0);
    }
    
    public bool AddResourceYield(ResourceYield yield)
    {
        bool anyAdded = false;
        
        foreach (var resource in yield.primaryResources)
        {
            if (AddResource(resource.type, resource.amount))
            {
                anyAdded = true;
            }
        }
        
        foreach (var chance in yield.secondaryResources)
        {
            if (UnityEngine.Random.value <= chance.chance)
            {
                if (AddResource(chance.type, chance.amount))
                {
                    anyAdded = true;
                }
            }
        }
        
        return anyAdded;
    }
    
    #endregion
    
    #region Legacy Compatibility
    
    // Keep these methods for backward compatibility with existing code
    public void AddRecyclingPoints(float amount)
    {
        // Convert to plastic resources (or distribute among basic resources)
        int resourceAmount = Mathf.RoundToInt(amount / 10f);
        AddResource(ResourceType.Plastic, resourceAmount);
    }
    
    public void AddDimensionalPotential(float amount)
    {
        // Convert to crystal fragments
        int resourceAmount = Mathf.RoundToInt(amount / 5f);
        AddResource(ResourceType.CrystalFragments, resourceAmount);
    }
    
    public bool SpendRecyclingPoints(float amount)
    {
        int plasticNeeded = Mathf.RoundToInt(amount / 10f);
        return SpendResource(ResourceType.Plastic, plasticNeeded);
    }
    
    private float GetLegacyRecyclingPoints()
    {
        // Convert current resources back to legacy RP for UI compatibility
        return GetResourceAmount(ResourceType.Plastic) * 10f + 
               GetResourceAmount(ResourceType.MetalScraps) * 20f;
    }
    
    private float GetLegacyDimensionalPotential()
    {
        // Convert crystal fragments to legacy DP
        return GetResourceAmount(ResourceType.CrystalFragments) * 5f;
    }
    
    private void TriggerLegacyEvents()
    {
        OnRecyclingPointsChanged?.Invoke(GetLegacyRecyclingPoints());
        OnDimensionalPotentialChanged?.Invoke(GetLegacyDimensionalPotential());
    }
    
    #endregion
    
    // Keep existing contamination system unchanged
    public void IncreaseContamination(float amount)
    {
        ContaminationLevel += amount;
        OnContaminationChanged?.Invoke(ContaminationLevel);
    }
    
    public void DecreaseContamination(float amount)
    {
        ContaminationLevel = Mathf.Max(0, ContaminationLevel - amount);
        OnContaminationChanged?.Invoke(ContaminationLevel);
    }
}
```

### **Step 2: Create ResourceProcessingManager.cs**

```csharp
// Assets/Scripts/Core/Managers/ResourceProcessingManager.cs
using UnityEngine;
using System.Collections.Generic;

public class ResourceProcessingManager : MonoBehaviour
{
    public static ResourceProcessingManager Instance { get; private set; }
    
    [Header("Processing Settings")]
    [SerializeField] private float baseProcessingTime = 1f;
    
    private Queue<ProcessingJob> processingQueue = new Queue<ProcessingJob>();
    private bool isProcessing = false;
    
    private struct ProcessingJob
    {
        public WasteItem wasteItem;
        public System.Action<bool> onComplete;
    }
    
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
    
    public bool ProcessWasteItem(WasteItem wasteItem)
    {
        if (wasteItem == null || ResourceManager.Instance == null) return false;
        
        // Calculate resource yield based on waste properties
        ResourceYield yield = CalculateResourceYield(wasteItem);
        
        // Add resources to inventory
        bool success = ResourceManager.Instance.AddResourceYield(yield);
        
        if (success)
        {
            // Add contamination based on waste properties
            float contamination = wasteItem.ContaminationLevel * 0.1f;
            ResourceManager.Instance.IncreaseContamination(contamination);
            
            Debug.Log($"Processed {wasteItem.Name} → {GetYieldSummary(yield)}");
        }
        
        return success;
    }
    
    private ResourceYield CalculateResourceYield(WasteItem wasteItem)
    {
        ResourceYield yield = new ResourceYield();
        List<ResourceAmount> primaryResources = new List<ResourceAmount>();
        
        // Get base resources for this dimensional origin
        ResourceType[] baseTypes = GetResourceTypesForOrigin(wasteItem.DimensionalOrigin);
        
        // Calculate total value based on rarity and properties
        int totalValue = GetValueForRarity(wasteItem.Rarity);
        float efficiency = wasteItem.WasteStability * wasteItem.RecyclingPotential;
        int adjustedValue = Mathf.RoundToInt(totalValue * efficiency);
        
        // Distribute value across resource types
        if (baseTypes.Length > 0)
        {
            ResourceType primaryType = baseTypes[Random.Range(0, baseTypes.Length)];
            primaryResources.Add(new ResourceAmount(primaryType, adjustedValue));
        }
        
        yield.primaryResources = primaryResources.ToArray();
        yield.contaminationRisk = wasteItem.ContaminationLevel;
        
        return yield;
    }
    
    private ResourceType[] GetResourceTypesForOrigin(string origin)
    {
        return origin.ToLower() switch
        {
            var x when x.Contains("earth") => new[] { ResourceType.Plastic, ResourceType.MetalScraps, ResourceType.OrganicMatter },
            var x when x.Contains("technological") => new[] { ResourceType.MetalScraps, ResourceType.CrystalFragments },
            var x when x.Contains("biological") => new[] { ResourceType.OrganicMatter, ResourceType.NeuralResidue },
            var x when x.Contains("quantum") => new[] { ResourceType.CrystalFragments, ResourceType.ToxicSludge },
            var x when x.Contains("philosophical") => new[] { ResourceType.NeuralResidue },
            _ => new[] { ResourceType.Plastic }
        };
    }
    
    private int GetValueForRarity(WasteRarity rarity)
    {
        return rarity switch
        {
            WasteRarity.Common => Random.Range(1, 3),
            WasteRarity.Uncommon => Random.Range(2, 5),
            WasteRarity.Rare => Random.Range(4, 7),
            WasteRarity.Epic => Random.Range(6, 10),
            WasteRarity.Legendary => Random.Range(8, 15),
            _ => 1
        };
    }
    
    private string GetYieldSummary(ResourceYield yield)
    {
        List<string> summary = new List<string>();
        foreach (var resource in yield.primaryResources)
        {
            summary.Add($"{resource.amount} {resource.type}");
        }
        return string.Join(", ", summary);
    }
}
```

### **Step 3: Update WasteItem.cs**

Add resource yield capability to your existing WasteItem:

```csharp
// Add this to your existing Assets/Scripts/Core/Data/Waste/WasteItem.cs

public class WasteItem
{
    // ... existing properties ...
    
    // NEW: Resource yield potential
    public ResourceYield GetResourceYield()
    {
        // This will be calculated when the item is processed
        return ResourceProcessingManager.Instance?.CalculateResourceYield(this) ?? new ResourceYield();
    }
    
    // NEW: Preview what resources this item would yield
    public string GetResourcePreview()
    {
        var yield = GetResourceYield();
        if (yield.primaryResources.Length == 0) return "No resources";
        
        List<string> preview = new List<string>();
        foreach (var resource in yield.primaryResources)
        {
            preview.Add($"{resource.amount} {resource.type}");
        }
        return string.Join(", ", preview);
    }
    
    // ... rest of existing code unchanged ...
}
```

## 🎮 Update Existing UI Components

### **Step 4: Update WasteDisplay.cs**

```csharp
// Add this to your existing Assets/Scripts/UI/Components/Items/WasteDisplay.cs

public class WasteDisplay : MonoBehaviour
{
    // ... existing fields ...
    
    [Header("Resource Preview")]
    [SerializeField] private TextMeshProUGUI resourcePreviewText;
    [SerializeField] private Button previewToggle;
    
    public void Initialize(WasteItem waste)
    {
        // ... existing initialization code ...
        
        // NEW: Show resource preview
        if (resourcePreviewText != null)
        {
            resourcePreviewText.text = waste.GetResourcePreview();
        }
    }
    
    private void RecycleWaste()
    {
        if (currentWaste == null) return;
        
        // Use new processing system instead of old ResourceManager.ProcessWasteItem
        bool success = ResourceProcessingManager.Instance.ProcessWasteItem(currentWaste);
        
        if (success)
        {
            // Remove from inventory
            if (WasteInventoryManager.Instance != null)
            {
                WasteInventoryManager.Instance.RemoveWasteItem(currentWaste);
            }
            
            // Destroy the display
            Destroy(gameObject);
        }
    }
    
    // ... rest of existing code unchanged ...
}
```

### **Step 5: Update ResourceDisplay.cs**

```csharp
// Update your existing Assets/Scripts/UI/Components/Displays/ResourceDisplay.cs

public class ResourceDisplay : MonoBehaviour
{
    // ... existing fields for RP/DP display ...
    
    [Header("New Resource Display")]
    [SerializeField] private Transform resourceContainer;
    [SerializeField] private GameObject resourceItemPrefab;
    
    private Dictionary<ResourceType, GameObject> resourceDisplays = new Dictionary<ResourceType, GameObject>();
    
    private void Start()
    {
        // ... existing initialization ...
        
        // Subscribe to new resource events
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourceChanged += UpdateResourceDisplay;
            ResourceManager.Instance.OnResourceInventoryChanged += RefreshAllResourceDisplays;
        }
        
        RefreshAllResourceDisplays();
    }
    
    private void UpdateResourceDisplay(ResourceType type, int amount)
    {
        if (resourceDisplays.TryGetValue(type, out GameObject display))
        {
            var displayComponent = display.GetComponent<ResourceDisplayItem>();
            displayComponent?.UpdateAmount(amount);
        }
        else if (amount > 0)
        {
            CreateResourceDisplay(type, amount);
        }
    }
    
    private void RefreshAllResourceDisplays()
    {
        // Clear existing displays
        foreach (var display in resourceDisplays.Values)
        {
            Destroy(display);
        }
        resourceDisplays.Clear();
        
        // Create displays for non-zero resources
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            int amount = ResourceManager.Instance.GetResourceAmount(type);
            if (amount > 0)
            {
                CreateResourceDisplay(type, amount);
            }
        }
    }
    
    private void CreateResourceDisplay(ResourceType type, int amount)
    {
        if (resourceItemPrefab == null || resourceContainer == null) return;
        
        GameObject display = Instantiate(resourceItemPrefab, resourceContainer);
        var displayComponent = display.GetComponent<ResourceDisplayItem>();
        
        if (displayComponent != null)
        {
            displayComponent.Initialize(type, amount);
            resourceDisplays[type] = display;
        }
    }
    
    // ... keep existing RP/DP display code for compatibility ...
}
```

## 🔧 Migration Strategy

### **Phase 1: Backward Compatible (Week 1)**
1. **Keep existing ResourceManager API** working
2. **Add new resource types** alongside RP/DP
3. **Update waste processing** to generate both old and new resources
4. **Test existing systems** still work

### **Phase 2: Hybrid Mode (Week 2)**
1. **Add resource preview** to waste items
2. **Create new resource UI** alongside existing displays
3. **Add processing facilities** for resource conversion
4. **Begin using resources** for some new features

### **Phase 3: Full Migration (Week 3)**
1. **Replace upgrade costs** with specific resources
2. **Update ship compartments** to consume resources
3. **Phase out RP/DP displays** (but keep backend for compatibility)
4. **Full resource-based economy**

## 🧪 Testing Integration

### **Create Test Script**

```csharp
// Assets/Scripts/Testing/Gameplay/ResourceSystemTester.cs
using UnityEngine;

public class ResourceSystemTester : MonoBehaviour
{
    [Header("Test Controls")]
    public KeyCode addResourcesKey = KeyCode.R;
    public KeyCode processWasteKey = KeyCode.T;
    public KeyCode showResourcesKey = KeyCode.Y;
    
    private void Update()
    {
        if (Input.GetKeyDown(addResourcesKey))
        {
            TestAddResources();
        }
        
        if (Input.GetKeyDown(processWasteKey))
        {
            TestProcessWaste();
        }
        
        if (Input.GetKeyDown(showResourcesKey))
        {
            ShowCurrentResources();
        }
    }
    
    private void TestAddResources()
    {
        ResourceManager.Instance.AddResource(ResourceType.Plastic, 5);
        ResourceManager.Instance.AddResource(ResourceType.MetalScraps, 3);
        Debug.Log("Added test resources");
    }
    
    private void TestProcessWaste()
    {
        var testWaste = new WasteItem("Test Waste", "Earth", WasteRarity.Common);
        ResourceProcessingManager.Instance.ProcessWasteItem(testWaste);
        Debug.Log("Processed test waste");
    }
    
    private void ShowCurrentResources()
    {
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            int amount = ResourceManager.Instance.GetResourceAmount(type);
            if (amount > 0)
            {
                Debug.Log($"{type}: {amount}");
            }
        }
    }
}
```

This implementation maintains **full backward compatibility** with your existing systems while adding the new resource-based economy. Your current UI and game logic will continue working while you gradually migrate to the new system! 🚀