using UnityEngine;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }

    // Events for game state changes
    public event Action<WasteItem> OnWasteCollected;
    public event Action<List<WasteItem>> OnWasteUpdated;
    public event Action<float> OnContaminationLevelChanged;

    // Core game systems
    private WasteGenerator wasteGenerator;
    private List<WasteItem> collectedWaste;

    // Game state
    public int TotalWasteCollected => collectedWaste != null ? collectedWaste.Count : 0;
    public float TotalRecyclingPoints { get; private set; }
    public float FacilityContaminationLevel { get; private set; }

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
        collectedWaste = new List<WasteItem>();

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
            var initialWaste = wasteGenerator.GenerateMultipleWaste(5);
            Debug.Log($"Generated {initialWaste.Count} initial waste items");

            foreach (var waste in initialWaste)
            {
                // Add to inventory
                WasteInventoryManager.Instance.AddWasteItem(waste);
            }

            // Notify systems about initial waste
            OnWasteUpdated?.Invoke(WasteInventoryManager.Instance.GetAllWaste());
        }
        catch (Exception e)
        {
            Debug.LogError($"Error generating initial waste: {e.Message}\n{e.StackTrace}");
        }
    }

    // Method to collect new waste
    public void CollectWaste()
    {
        Debug.Log("Collecting new waste item");

        if (wasteGenerator == null)
        {
            Debug.LogError("WasteGenerator is null when collecting waste!");
            return;
        }

        if (WasteInventoryManager.Instance == null)
        {
            Debug.LogError("WasteInventoryManager.Instance is null when collecting waste!");
            return;
        }

        try
        {
            var newWaste = wasteGenerator.GenerateWasteItem();

            if (newWaste == null)
            {
                Debug.LogError("Generated waste item is null!");
                return;
            }

            // Add to inventory
            WasteInventoryManager.Instance.AddWasteItem(newWaste);

            // Notify systems about new waste
            OnWasteCollected?.Invoke(newWaste);
            OnWasteUpdated?.Invoke(WasteInventoryManager.Instance.GetAllWaste());

            // Update contamination
            UpdateFacilityContamination(newWaste);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error collecting waste: {e.Message}\n{e.StackTrace}");
        }
    }

    // Update facility contamination based on new waste
    private void UpdateFacilityContamination(WasteItem waste)
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
    public List<WasteItem> GetCollectedWaste()
    {
        if (WasteInventoryManager.Instance == null)
        {
            Debug.LogWarning("WasteInventoryManager.Instance is null when getting collected waste");
            return new List<WasteItem>();
        }

        return WasteInventoryManager.Instance.GetAllWaste();
    }

    // Get waste by dimension type
    public List<WasteItem> GetWasteByDimension(string dimensionType)
    {
        if (WasteInventoryManager.Instance == null)
        {
            Debug.LogWarning("WasteInventoryManager.Instance is null when getting waste by dimension");
            return new List<WasteItem>();
        }

        return WasteInventoryManager.Instance.GetWasteByDimension(dimensionType);
    }

    // Process waste for recycling
    public float ProcessWaste(WasteItem waste)
    {
        if (waste == null)
        {
            Debug.LogWarning("Attempted to process null waste item");
            return 0f;
        }

        if (WasteInventoryManager.Instance == null || ResourceManager.Instance == null)
        {
            Debug.LogError("Required manager instance is null when processing waste");
            return 0f;
        }

        if (WasteInventoryManager.Instance.HasWasteItem(waste))
        {
            float recyclingPoints = waste.RecyclingValue;
            WasteInventoryManager.Instance.RemoveWasteItem(waste);

            // Add recycling points to resource manager
            ResourceManager.Instance.AddRecyclingPoints(recyclingPoints);

            // Add dimensional potential based on stability
            float potentialGain = waste.WasteStability * 10f;
            ResourceManager.Instance.AddDimensionalPotential(potentialGain);

            // Update waste collection
            OnWasteUpdated?.Invoke(WasteInventoryManager.Instance.GetAllWaste());

            return recyclingPoints;
        }
        return 0f;
    }

    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnContaminationChanged -= UpdateContaminationLevel;
        }
    }
}