using System.Collections.Generic;
using UnityEngine;
using System;

public class ProbeManager : MonoBehaviour
{
    // Singleton
    public static ProbeManager Instance { get; private set; }

    // Collection settings
    [SerializeField] private float baseCollectionRate = 1f; // Items per minute
    [SerializeField] private int maxProbeCount = 10;

    // Debug options
    [Header("Debug Options")]
    [SerializeField] private bool startWithProbe = true; // Set this to true in Inspector
    [SerializeField] private bool logDebugMessages = true;

    // Probe data
    private List<Probe> activeProbes = new List<Probe>();
    private float collectionTimer = 0f;
    private float collectionInterval = 60f; // 60 seconds = 1 minute

    // Reference to WasteGenerator
    private WasteGenerator wasteGenerator;

    // Events
    public event Action<Probe> OnProbeDispatched;
    public event Action<UpdatedWasteItem> OnWasteCollected;
    public event Action<int> OnProbeCountChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Fix for DontDestroyOnLoad warning
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            if (logDebugMessages)
                Debug.Log("ProbeManager: Initialized singleton instance");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (logDebugMessages)
            Debug.Log("ProbeManager: Start method called");

        // Find the WasteGenerator
        wasteGenerator = UnityEngine.Object.FindFirstObjectByType<WasteGenerator>();
        if (wasteGenerator == null)
        {
            Debug.LogError("ProbeManager: WasteGenerator not found in scene!");
        }
        else if (logDebugMessages)
        {
            Debug.Log("ProbeManager: WasteGenerator found");
        }

        // Create initial probe if startWithProbe is true
        if (startWithProbe)
        {
            CreateInitialProbe();
        }
        else
        {
            if (logDebugMessages)
                Debug.Log("ProbeManager: Skipping initial probe creation");
        }

        // Adjust collection interval based on rate
        UpdateCollectionInterval();

        if (logDebugMessages)
            Debug.Log("ProbeManager: Start method completed");
    }

    // Add the missing CreateInitialProbe method
    public void CreateInitialProbe()
    {
        if (LocationManager.Instance == null)
        {
            Debug.LogError("ProbeManager: LocationManager.Instance is null when creating initial probe!");
            return;
        }

        LocationData currentLocation = LocationManager.Instance.GetCurrentLocation();
        if (currentLocation == null)
        {
            Debug.LogError("ProbeManager: Current location is null when creating initial probe!");
            return;
        }

        if (logDebugMessages)
            Debug.Log($"ProbeManager: Creating initial probe at {currentLocation.displayName}");

        // Create and add the probe using DispatchProbe
        Probe probe = DispatchProbe(currentLocation);

        if (probe != null && logDebugMessages)
            Debug.Log($"ProbeManager: Initial probe created with ID: {probe.probeId}");
        else
            Debug.LogError("ProbeManager: Failed to create initial probe!");
    }

    private void Update()
    {
        // Only process if we have active probes
        if (activeProbes.Count > 0)
        {
            collectionTimer += Time.deltaTime;

            // Time to collect waste
            if (collectionTimer >= collectionInterval)
            {
                if (logDebugMessages)
                    Debug.Log($"ProbeManager: Collection timer reached {collectionInterval}s, collecting waste");

                collectionTimer = 0f;
                CollectWasteFromProbes();
            }
        }
    }

    public void UpdateCollectionInterval()
    {
        // Calculate interval from rate (converting from per minute to seconds)
        float totalRate = 0f; // Start with 0, not baseCollectionRate
        foreach (var probe in activeProbes)
        {
            totalRate += probe.collectionRate;
        }

        // Avoid division by zero
        if (totalRate > 0)
        {
            collectionInterval = 60f / totalRate;
        }
        else
        {
            collectionInterval = 60f;
        }

        if (logDebugMessages)
            Debug.Log($"ProbeManager: Collection interval updated: {collectionInterval:F1} seconds");
    }

    public Probe DispatchProbe(LocationData location)
    {
        if (location == null)
        {
            Debug.LogError("ProbeManager: Cannot dispatch probe - location is null!");
            return null;
        }

        if (logDebugMessages)
            Debug.Log($"ProbeManager: Dispatching probe to {location.displayName}");

        // Check if we've reached the max number of probes
        if (activeProbes.Count >= maxProbeCount)
        {
            Debug.LogWarning($"ProbeManager: Maximum number of probes reached! ({maxProbeCount})");
            return null;
        }

        // Create a new probe
        Probe newProbe = new Probe
        {
            probeId = Guid.NewGuid().ToString(),
            location = location,
            level = 1,
            collectionRate = baseCollectionRate,
            efficiencyMultiplier = 1.0f
        };

        activeProbes.Add(newProbe);

        if (logDebugMessages)
            Debug.Log($"ProbeManager: Probe dispatched. ID: {newProbe.probeId}, Total probes: {activeProbes.Count}");

        OnProbeDispatched?.Invoke(newProbe);
        OnProbeCountChanged?.Invoke(activeProbes.Count);

        // Update collection interval when adding a new probe
        UpdateCollectionInterval();

        return newProbe;
    }

    public void RecallProbe(string probeId)
    {
        if (string.IsNullOrEmpty(probeId))
        {
            Debug.LogError("ProbeManager: Cannot recall probe - probeId is null or empty!");
            return;
        }

        int index = activeProbes.FindIndex(p => p.probeId == probeId);
        if (index >= 0)
        {
            string probeName = activeProbes[index].probeId;
            activeProbes.RemoveAt(index);

            if (logDebugMessages)
                Debug.Log($"ProbeManager: Recalled probe {probeName}. Remaining probes: {activeProbes.Count}");

            OnProbeCountChanged?.Invoke(activeProbes.Count);
            UpdateCollectionInterval();
        }
        else
        {
            Debug.LogWarning($"ProbeManager: Failed to recall probe - ID not found: {probeId}");
        }
    }

    public void UpgradeProbe(string probeId)
    {
        Probe probe = activeProbes.Find(p => p.probeId == probeId);
        if (probe != null)
        {
            // Increase level
            probe.level++;

            // Improve collection rate (25% per level)
            probe.collectionRate = baseCollectionRate * (1 + 0.25f * (probe.level - 1));

            // Update collection interval
            UpdateCollectionInterval();

            if (logDebugMessages)
                Debug.Log($"ProbeManager: Upgraded probe {probeId} to level {probe.level} with rate {probe.collectionRate:F2}/min");
        }
        else
        {
            Debug.LogWarning($"ProbeManager: Failed to upgrade probe - ID not found: {probeId}");
        }
    }

    private void CollectWasteFromProbes()
    {
        if (logDebugMessages)
            Debug.Log($"ProbeManager: Collecting waste from {activeProbes.Count} probes");

        // For each active probe, collect waste
        int collectedCount = 0;

        // Collect from each probe
        foreach (var probe in activeProbes)
        {
            // Generate waste based on probe's location
            if (probe.location != null && wasteGenerator != null)
            {
                // Get multiplier based on probe efficiency and location value
                float multiplier = probe.efficiencyMultiplier * probe.location.averageValueMultiplier;

                // Generate waste
                WasteItem waste = wasteGenerator.GenerateWasteItem();

                // Apply probe-specific modifications
                if (waste != null)
                {
                    waste.RecyclingValue *= multiplier;

                    // Convert to UpdatedWasteItem for the new inventory system
                    UpdatedWasteItem updatedWaste = UpdatedWasteItem.FromWasteItem(waste);

                    // Add to inventory
                    if (WasteInventoryManager.Instance != null)
                    {
                        WasteInventoryManager.Instance.AddWasteItem(updatedWaste);
                        OnWasteCollected?.Invoke(updatedWaste);
                        collectedCount++;
                    }
                    else
                    {
                        Debug.LogError("ProbeManager: WasteInventoryManager.Instance is null during collection!");
                    }
                }
                else
                {
                    Debug.LogError("ProbeManager: Generated waste is null!");
                }
            }
            else
            {
                Debug.LogWarning($"ProbeManager: Cannot collect waste - probe location is null or WasteGenerator is null");
            }
        }

        if (logDebugMessages)
            Debug.Log($"ProbeManager: Collected {collectedCount} waste items");

        // Check for location unlocks after collection
        if (LocationManager.Instance != null)
        {
            LocationManager.Instance.CheckForLocationUnlocks();
        }
    }

    public List<Probe> GetActiveProbes()
    {
        return new List<Probe>(activeProbes);
    }

    public int GetMaxProbeCount()
    {
        return maxProbeCount;
    }

    public void IncreaseMaxProbes(int amount)
    {
        maxProbeCount += amount;
        if (logDebugMessages)
            Debug.Log($"ProbeManager: Max probe count increased to {maxProbeCount}");
    }

    // Public method for debugging - force a collection
    public void ForceCollectWaste()
    {
        if (logDebugMessages)
            Debug.Log("ProbeManager: Force collecting waste");

        CollectWasteFromProbes();
    }

    // Public method for debugging - clear all probes
    public void ClearAllProbes()
    {
        activeProbes.Clear();
        OnProbeCountChanged?.Invoke(0);
        if (logDebugMessages)
            Debug.Log("ProbeManager: All probes cleared");
    }
}

[Serializable]
public class Probe
{
    public string probeId;
    public LocationData location;
    public int level;
    public float collectionRate;
    public float efficiencyMultiplier;
}