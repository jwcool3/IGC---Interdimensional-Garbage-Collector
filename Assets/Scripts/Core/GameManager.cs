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

        collectedWaste = new List<WasteItem>();
        TotalRecyclingPoints = 0f;
        FacilityContaminationLevel = 0.1f;

        // Initial waste collection
        CollectInitialWaste();

        // Subscribe to facility events
        FacilityManager.Instance.OnUpgradeCompleted += HandleUpgradeCompleted;
    }

    private void HandleUpgradeCompleted(string upgradeName)
    {
        // React to upgrades being completed
        Debug.Log($"Upgrade completed: {upgradeName}");
        
        // You might want to update UI or game state here
        UpdateContaminationLevel(FacilityContaminationLevel);
    }

    // Collect initial set of waste
    private void CollectInitialWaste()
    {
        // Generate a few initial waste items
        for (int i = 0; i < 3; i++)
        {
            var waste = wasteGenerator.GenerateWasteItem();
            collectedWaste.Add(waste);
        }

        // Notify systems about initial waste
        OnWasteUpdated?.Invoke(collectedWaste);
    }

    // Method to collect new waste
    public void CollectWaste()
    {
        var newWaste = wasteGenerator.GenerateWasteItem();
        AddWasteToCollection(newWaste);
        
        // Notify systems about new waste
        OnWasteCollected?.Invoke(newWaste);
        OnWasteUpdated?.Invoke(collectedWaste);

        // Update contamination
        UpdateFacilityContamination(newWaste);
    }

    // Add waste to collection and update stats
    private void AddWasteToCollection(WasteItem waste)
    {
        collectedWaste.Add(waste);
        TotalRecyclingPoints += waste.RecyclingValue;
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
        ResourceManager.Instance.IncreaseContamination(newLevel);
        OnContaminationLevelChanged?.Invoke(FacilityContaminationLevel);
    }

    // Get current waste collection
    public List<WasteItem> GetCollectedWaste()
    {
        return new List<WasteItem>(collectedWaste);
    }

    // Get waste by dimension type
    public List<WasteItem> GetWasteByDimension(string dimensionType)
    {
        return collectedWaste.FindAll(w => w.DimensionalOrigin == dimensionType);
    }

    // Process waste for recycling
    public float ProcessWaste(WasteItem waste)
    {
        if (collectedWaste.Contains(waste))
        {
            float recyclingPoints = waste.RecyclingValue;
            collectedWaste.Remove(waste);
            
            // Add recycling points to facility
            FacilityManager.Instance.AddRecyclingPoints(recyclingPoints);
            
            // Add dimensional potential based on stability
            float potentialGain = waste.WasteStability * 10f;
            FacilityManager.Instance.AddDimensionalPotential(potentialGain);

            // Update waste collection
            OnWasteUpdated?.Invoke(collectedWaste);

            return recyclingPoints;
        }
        return 0f;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (FacilityManager.Instance != null)
        {
            FacilityManager.Instance.OnUpgradeCompleted -= HandleUpgradeCompleted;
        }
    }
} 