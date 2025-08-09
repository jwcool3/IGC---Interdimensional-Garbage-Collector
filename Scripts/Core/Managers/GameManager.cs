using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }

    // Events for game state changes - Updated to use UpdatedWasteItem
    public event Action<UpdatedWasteItem> OnWasteCollected;
    public event Action<List<UpdatedWasteItem>> OnWasteUpdated;
    public event Action<float> OnContaminationLevelChanged;
    public event Action<float> OnWasteDetailLevelChanged;

    // Core game systems
    private WasteGenerator wasteGenerator;
    private List<UpdatedWasteItem> collectedWaste;

    // Game state
    public int TotalWasteCollected
    {
        get
        {
            if (WasteInventoryManager.Instance != null)
            {
                return WasteInventoryManager.Instance.GetInventoryCount();
            }
            return 0;
        }
    }
    public float TotalRecyclingPoints { get; private set; }
    public float FacilityContaminationLevel { get; private set; }

    // Scanner detail level from ship compartment
    private float wasteDetailLevel = 0f;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize core systems
            InitializeSystems();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSystems()
    {
        Debug.Log("Initializing GameManager systems");

        // Initialize the waste collection list
        collectedWaste = new List<UpdatedWasteItem>();

        // Get or add WasteGenerator component
        wasteGenerator = GetComponent<WasteGenerator>();
        if (wasteGenerator == null)
        {
            Debug.Log("Adding WasteGenerator component to GameManager");
            wasteGenerator = gameObject.AddComponent<WasteGenerator>();
        }

        // Initialize ResourceManager if it doesn't exist
        if (ResourceManager.Instance == null)
        {
            Debug.Log("Creating ResourceManager");
            GameObject resourceManagerObj = new GameObject("ResourceManager");
            resourceManagerObj.AddComponent<ResourceManager>();
        }

        // Initialize FacilityManager if it doesn't exist
        if (FacilityManager.Instance == null)
        {
            Debug.Log("Creating FacilityManager");
            GameObject facilityManagerObj = new GameObject("FacilityManager");
            facilityManagerObj.AddComponent<FacilityManager>();
        }

        // Initialize WasteInventoryManager if it doesn't exist
        if (WasteInventoryManager.Instance == null)
        {
            Debug.Log("Creating WasteInventoryManager");
            GameObject inventoryManagerObj = new GameObject("WasteInventoryManager");
            inventoryManagerObj.AddComponent<WasteInventoryManager>();
        }

        // Set initial values
        TotalRecyclingPoints = 0f;
        FacilityContaminationLevel = 0.1f;

        // Notify listeners of initial contamination level
        OnContaminationLevelChanged?.Invoke(FacilityContaminationLevel);

        // Subscribe to facility events
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnContaminationChanged += UpdateContaminationLevel;
        }
        else
        {
            Debug.LogError("ResourceManager.Instance is null after initialization!");
        }

        // Initial waste collection (delayed to ensure all managers are ready)
        Invoke("CollectInitialWaste", 0.5f);
    }

    // Collect initial set of waste
    private void CollectInitialWaste()
    {
        Debug.Log("Collecting initial waste items");

        if (wasteGenerator == null)
        {
            Debug.LogError("WasteGenerator is null when collecting initial waste!");
            return;
        }

        if (WasteInventoryManager.Instance == null)
        {
            Debug.LogError("WasteInventoryManager.Instance is null when collecting initial waste!");
            return;
        }

        // Generate starting waste
        try
        {
            var initialWasteItems = wasteGenerator.GenerateMultipleWaste(5);
            Debug.Log($"Generated {initialWasteItems.Count} initial waste items");

            foreach (var wasteItem in initialWasteItems)
            {
                // Convert WasteItem to UpdatedWasteItem
                var updatedWaste = UpdatedWasteItem.FromWasteItem(wasteItem);
                
                // Add to inventory
                WasteInventoryManager.Instance.AddWasteItem(updatedWaste);
            }

            // Notify systems about initial waste
            OnWasteUpdated?.Invoke(WasteInventoryManager.Instance.GetAllWaste());
        }
        catch (Exception e)
        {
            Debug.LogError($"Error generating initial waste: {e.Message}\n{e.StackTrace}");
        }
    }

    // Collect new waste
    public void CollectWaste()
    {
        DebugManager.Log("Collecting new waste item", DebugCategory.WasteGeneration);

        if (wasteGenerator == null)
        {
            DebugManager.LogError("WasteGenerator is null when collecting waste!", DebugCategory.WasteGeneration);
            return;
        }

        if (WasteInventoryManager.Instance == null)
        {
            DebugManager.LogError("WasteInventoryManager.Instance is null when collecting waste!", DebugCategory.WasteGeneration);
            return;
        }

        try
        {
            // Use new enhanced waste generation
            var newWaste = wasteGenerator.GenerateUpdatedWasteItem();

            if (newWaste == null)
            {
                Debug.LogError("Generated waste item is null!");
                return;
            }

            // Ensure resource yield is properly set up
            if (newWaste.ResourceYield == null || newWaste.ResourceYield.IsEmpty)
            {
                newWaste.SetupDefaultYield();
            }

            // Add to inventory
            WasteInventoryManager.Instance.AddWasteItem(newWaste);

            // Notify systems about new waste
            OnWasteCollected?.Invoke(newWaste);
            OnWasteUpdated?.Invoke(WasteInventoryManager.Instance.GetAllWaste());

            // Check for unlocks after each collection
            if (LocationManager.Instance != null)
            {
                LocationManager.Instance.CheckForLocationUnlocks();
            }

            // Update contamination
            UpdateFacilityContamination(newWaste);
            
            // Log resource preview for debugging
            var preview = newWaste.GetResourcePreview();
            if (!string.IsNullOrEmpty(preview))
            {
                DebugManager.Log($"Collected waste with resources: {preview}", DebugCategory.WasteGeneration);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error collecting waste: {e.Message}\n{e.StackTrace}");
        }
    }

    // Update facility contamination based on new waste
    private void UpdateFacilityContamination(UpdatedWasteItem waste)
    {
        if (waste == null)
        {
            Debug.LogWarning("Attempted to update facility contamination with null waste item");
            return;
        }

        // Contamination increases with unstable waste
        float contaminationIncrease = waste.ContaminationLevel * 0.1f;
        FacilityContaminationLevel = Mathf.Min(1f, FacilityContaminationLevel + contaminationIncrease);

        Debug.Log($"Updating facility contamination: +{contaminationIncrease:F2}, new total: {FacilityContaminationLevel:F2}");

        // Notify listeners
        OnContaminationLevelChanged?.Invoke(FacilityContaminationLevel);
    }

    // Update contamination level from facility upgrades
    private void UpdateContaminationLevel(float newLevel)
    {
        FacilityContaminationLevel = newLevel;
        Debug.Log($"Contamination level updated to: {FacilityContaminationLevel:F2}");
        OnContaminationLevelChanged?.Invoke(FacilityContaminationLevel);
    }

    // Get current waste collection
    public List<UpdatedWasteItem> GetCollectedWaste()
    {
        if (WasteInventoryManager.Instance == null)
        {
            Debug.LogWarning("WasteInventoryManager.Instance is null when getting collected waste");
            return new List<UpdatedWasteItem>();
        }

        return WasteInventoryManager.Instance.GetAllWaste();
    }

    // Get waste by dimension type
    public List<UpdatedWasteItem> GetWasteByDimension(string dimensionType)
    {
        if (WasteInventoryManager.Instance == null)
        {
            Debug.LogWarning("WasteInventoryManager.Instance is null when getting waste by dimension");
            return new List<UpdatedWasteItem>();
        }

        return WasteInventoryManager.Instance.GetWasteByDimension(dimensionType);
    }

    #region Waste Processing - Updated for New Resource System

    [Header("Resource System Settings")]
    [SerializeField] private bool _useNewResourceSystem = true;
    [SerializeField] private bool _enableHybridMode = false; // Use both old and new systems during transition
    [SerializeField] private bool showProcessingDetails = true;

    // Public properties for access
    public bool useNewResourceSystem 
    { 
        get => _useNewResourceSystem; 
        set => _useNewResourceSystem = value; 
    }
    
    public bool enableHybridMode 
    { 
        get => _enableHybridMode; 
        set => _enableHybridMode = value; 
    }

    /// <summary>
    /// Process waste for recycling - Updated to support new resource system
    /// </summary>
    /// <param name="waste">Waste item to process</param>
    /// <returns>Legacy RP value for compatibility</returns>
    public float ProcessWaste(UpdatedWasteItem waste)
    {
        if (waste == null)
        {
            Debug.LogWarning("Attempted to process null waste item");
            return 0f;
        }

        if (WasteInventoryManager.Instance == null)
        {
            Debug.LogError("WasteInventoryManager.Instance is null when processing waste");
            return 0f;
        }

        // Check if the waste item exists in inventory
        if (!WasteInventoryManager.Instance.HasWasteItem(waste))
        {
            Debug.LogWarning($"Waste item {waste.Name} not found in inventory");
            return 0f;
        }

        float legacyRP = 0f;

        if (useNewResourceSystem)
        {
            // Use new resource system
            legacyRP = ProcessWasteWithNewSystem(waste);
        }
        else if (enableHybridMode)
        {
            // Use both systems during transition
            legacyRP = ProcessWasteHybrid(waste);
        }
        else
        {
            // Use legacy system only
            legacyRP = ProcessWasteLegacy(waste);
        }

        // Update waste collection UI
        OnWasteUpdated?.Invoke(WasteInventoryManager.Instance.GetAllWaste());

        return legacyRP;
    }

    /// <summary>
    /// Process waste using the new resource system
    /// </summary>
    private float ProcessWasteWithNewSystem(UpdatedWasteItem waste)
    {
        if (WasteInventoryManager.Instance == null || ResourceManager.Instance == null)
        {
            Debug.LogError("Required managers not available for new resource processing");
            return 0f;
        }

        // Calculate legacy RP for return value (for UI compatibility)
        float legacyRP = waste.TotalValue;

        // Process through the new system
        var generatedResources = WasteInventoryManager.Instance.ProcessWasteToResources(waste);

        if (showProcessingDetails && generatedResources.Count > 0)
        {
            Debug.Log($"Processed {waste.Name}:");
            foreach (var resource in generatedResources)
            {
                Debug.Log($"  Generated {resource.Value} {resource.Key}");
            }
        }

        return legacyRP;
    }

    /// <summary>
    /// Process waste using hybrid mode (both systems)
    /// </summary>
    private float ProcessWasteHybrid(UpdatedWasteItem waste)
    {
        if (WasteInventoryManager.Instance == null)
        {
            Debug.LogError("WasteInventoryManager.Instance is null");
            return 0f;
        }

        // Use the hybrid processing method
        var result = WasteInventoryManager.Instance.ProcessWasteHybrid(waste);

        if (result.success)
        {
            if (showProcessingDetails)
            {
                Debug.Log($"Hybrid processing: {result.GetSummary()}");
            }
            return result.legacyRP;
        }

        return 0f;
    }

    /// <summary>
    /// Process waste using legacy system only
    /// </summary>
    private float ProcessWasteLegacy(UpdatedWasteItem waste)
    {
        if (ResourceManager.Instance == null)
        {
            Debug.LogError("ResourceManager.Instance is null when processing waste");
            return 0f;
        }

        float recyclingPoints = waste.TotalValue;
        WasteInventoryManager.Instance.RemoveWasteItem(waste);

        // Add recycling points to legacy resource manager
        ResourceManager.Instance.AddRecyclingPoints(recyclingPoints);

        // Add dimensional potential based on stability
        float potentialGain = waste.DimensionalStability * 10f;
        ResourceManager.Instance.AddDimensionalPotential(potentialGain);

        if (showProcessingDetails)
        {
            Debug.Log($"Legacy processing: {waste.Name} -> {recyclingPoints} RP, {potentialGain} DP");
        }

        return recyclingPoints;
    }

    /// <summary>
    /// Batch process multiple waste items
    /// </summary>
    /// <param name="wasteItems">List of waste items to process</param>
    /// <returns>Total legacy RP generated</returns>
    public float BatchProcessWaste(List<UpdatedWasteItem> wasteItems)
    {
        if (wasteItems == null || wasteItems.Count == 0)
        {
            Debug.LogWarning("No waste items provided for batch processing");
            return 0f;
        }

        float totalRP = 0f;

        if (useNewResourceSystem && WasteInventoryManager.Instance != null)
        {
            // Use new system batch processing
            var totalResources = WasteInventoryManager.Instance.BatchProcessWaste(wasteItems);
            
            // Calculate equivalent RP for compatibility
            foreach (var waste in wasteItems)
            {
                totalRP += waste.TotalValue;
            }

            if (showProcessingDetails)
            {
                Debug.Log($"Batch processed {wasteItems.Count} items -> {totalResources.Count} resource types");
            }
        }
        else
        {
            // Process individually using legacy system
            foreach (var waste in wasteItems)
            {
                totalRP += ProcessWaste(waste);
            }
        }

        return totalRP;
    }

    /// <summary>
    /// Process all waste of a specific type
    /// </summary>
    /// <param name="wasteType">Type of waste to process</param>
    /// <param name="maxQuantity">Maximum quantity to process</param>
    /// <returns>Total RP generated</returns>
    public float ProcessAllWasteOfType(WasteType wasteType, int maxQuantity = int.MaxValue)
    {
        if (WasteInventoryManager.Instance == null)
        {
            Debug.LogError("WasteInventoryManager.Instance is null");
            return 0f;
        }

        float totalRP = 0f;

        if (useNewResourceSystem)
        {
            // Use new system
            var totalResources = WasteInventoryManager.Instance.ProcessAllWasteOfType(wasteType, maxQuantity);
            
            // Estimate RP for compatibility (this is approximate)
            foreach (var resource in totalResources)
            {
                totalRP += resource.Value * 2f; // Rough conversion factor
            }
        }
        else
        {
            // Use legacy system
            var wasteItems = WasteInventoryManager.Instance.GetWasteByType(wasteType, maxQuantity);
            totalRP = BatchProcessWaste(wasteItems);
        }

        // Update UI
        OnWasteUpdated?.Invoke(WasteInventoryManager.Instance.GetAllWaste());

        return totalRP;
    }

    /// <summary>
    /// Get total estimated value of all waste in inventory
    /// </summary>
    /// <returns>Total estimated RP value</returns>
    public float GetTotalWasteValue()
    {
        if (WasteInventoryManager.Instance == null) return 0f;

        float totalValue = 0f;
        var allWaste = WasteInventoryManager.Instance.GetAllWaste();

        foreach (var waste in allWaste)
        {
            totalValue += waste.TotalValue * waste.Quantity;
        }

        return totalValue;
    }

    /// <summary>
    /// Get resource preview for all waste in inventory
    /// </summary>
    /// <returns>Dictionary of estimated resources</returns>
    public Dictionary<ResourceType, int> GetInventoryResourcePreview()
    {
        if (!useNewResourceSystem || WasteInventoryManager.Instance == null)
        {
            return new Dictionary<ResourceType, int>();
        }

        return WasteInventoryManager.Instance.GetInventoryResourcePreview();
    }

    #endregion

    #region Context Menu Test Methods (for development)

    [ContextMenu("Test Process Random Waste")]
    private void TestProcessRandomWaste()
    {
        if (WasteInventoryManager.Instance == null) return;

        var allWaste = WasteInventoryManager.Instance.GetAllWaste();
        if (allWaste.Count > 0)
        {
            var randomWaste = allWaste[UnityEngine.Random.Range(0, allWaste.Count)];
            float rp = ProcessWaste(randomWaste);
            Debug.Log($"Test processed {randomWaste.Name} for {rp} RP");
        }
    }

    [ContextMenu("Test Batch Process Plastic")]
    private void TestBatchProcessPlastic()
    {
        float totalRP = ProcessAllWasteOfType(WasteType.Plastic, 5);
        Debug.Log($"Batch processed Plastic waste for {totalRP} total RP");
    }

    [ContextMenu("Show Inventory Resource Preview")]
    private void ShowInventoryResourcePreview()
    {
        var preview = GetInventoryResourcePreview();
        Debug.Log("Inventory Resource Preview:");
        foreach (var resource in preview)
        {
            Debug.Log($"  {resource.Key}: {resource.Value}");
        }
    }

    #endregion

    /// <summary>
    /// Sets the waste detail level from the Scanner compartment
    /// </summary>
    public void SetWasteDetailLevel(float detailLevel)
    {
        wasteDetailLevel = Mathf.Clamp01(detailLevel);
        Debug.Log($"GameManager: Waste detail level set to {wasteDetailLevel:P0}");

        // Notify listeners of the change
        OnWasteDetailLevelChanged?.Invoke(wasteDetailLevel);
    }

    /// <summary>
    /// Gets the current waste detail level
    /// </summary>
    public float GetWasteDetailLevel()
    {
        return wasteDetailLevel;
    }

    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnContaminationChanged -= UpdateContaminationLevel;
        }
    }
}