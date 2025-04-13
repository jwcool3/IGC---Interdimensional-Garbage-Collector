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
    public int TotalWasteCollected => collectedWaste.Count;
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
        wasteGenerator = GetComponent<WasteGenerator>();
        if (wasteGenerator == null)
        {
            wasteGenerator = gameObject.AddComponent<WasteGenerator>();
        }

        // Make sure FacilityManager exists
        if (FacilityManager.Instance == null)
        {
            gameObject.AddComponent<FacilityManager>();
        }

        // Make sure ResourceManager exists
        if (ResourceManager.Instance == null)
        {
            gameObject.AddComponent<ResourceManager>();
        }

        // Make sure WasteInventoryManager exists
        if (WasteInventoryManager.Instance == null)
        {
            gameObject.AddComponent<WasteInventoryManager>();
        }

        collectedWaste = new List<WasteItem>();
        TotalRecyclingPoints = 0f;
        FacilityContaminationLevel = 0.1f;

        // Initial waste collection
        CollectInitialWaste();

        // Subscribe to facility events
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnContaminationChanged += UpdateContaminationLevel;
        }
    }

    // Collect initial set of waste
    private void CollectInitialWaste()
    {
        // Generate starting waste
        var initialWaste = wasteGenerator.GenerateMultipleWaste(5);
        foreach (var waste in initialWaste)
        {
            // Add to inventory instead of direct collection
            if (WasteInventoryManager.Instance != null)
            {
                WasteInventoryManager.Instance.AddWasteItem(waste);
            }
        }

        // Notify systems about initial waste
        OnWasteUpdated?.Invoke(WasteInventoryManager.Instance?.GetAllWaste() ?? new List<WasteItem>());
    }

    // Method to collect new waste
    public void CollectWaste()
    {
        var newWaste = wasteGenerator.GenerateWasteItem();
        
        // Add to inventory instead of direct collection
        if (WasteInventoryManager.Instance != null)
        {
            WasteInventoryManager.Instance.AddWasteItem(newWaste);
            
            // Notify systems about new waste
            OnWasteCollected?.Invoke(newWaste);
            OnWasteUpdated?.Invoke(WasteInventoryManager.Instance.GetAllWaste());

            // Update contamination
            UpdateFacilityContamination(newWaste);
        }
        else
        {
            Debug.LogError("WasteInventoryManager not found! Cannot collect waste.");
        }
    }

    // Update facility contamination based on new waste
    private void UpdateFacilityContamination(WasteItem waste)
    {
        // Contamination increases with unstable waste
        float contaminationIncrease = waste.ContaminationLevel * 0.1f;
        FacilityContaminationLevel = Mathf.Min(1f, FacilityContaminationLevel + contaminationIncrease);
        OnContaminationLevelChanged?.Invoke(FacilityContaminationLevel);
    }

    // Update contamination level from facility upgrades
    private void UpdateContaminationLevel(float newLevel)
    {
        FacilityContaminationLevel = newLevel;
        OnContaminationLevelChanged?.Invoke(FacilityContaminationLevel);
    }

    // Get current waste collection
    public List<WasteItem> GetCollectedWaste()
    {
        return WasteInventoryManager.Instance?.GetAllWaste() ?? new List<WasteItem>();
    }

    // Get waste by dimension type
    public List<WasteItem> GetWasteByDimension(string dimensionType)
    {
        return WasteInventoryManager.Instance?.GetWasteByDimension(dimensionType) ?? new List<WasteItem>();
    }

    // Process waste for recycling
    public float ProcessWaste(WasteItem waste)
    {
        if (WasteInventoryManager.Instance != null && WasteInventoryManager.Instance.HasWasteItem(waste))
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