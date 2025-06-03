using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Event bridge that translates between legacy and new resource system events
/// Ensures UI and other systems continue to work during the transition
/// </summary>
public class ResourceEventBridge : MonoBehaviour
{
    public static ResourceEventBridge Instance { get; private set; }
    
    [Header("Bridge Settings")]
    [SerializeField] private bool enableLegacyEvents = true;
    [SerializeField] private bool enableNewEvents = true;
    [SerializeField] private bool logEventTranslations = false;
    
    [Header("Event Translation Settings")]
    [SerializeField] private float eventThrottleTime = 0.1f; // Prevent event spam
    [SerializeField] private bool batchSimilarEvents = true;
    
    // Legacy event delegates (maintain compatibility)
    public event Action<float> OnLegacyRecyclingPointsChanged;
    public event Action<float> OnLegacyDimensionalPotentialChanged;
    public event Action<float> OnLegacyContaminationChanged;
    public event Action OnLegacyResourcesChanged;
    public event Action OnLegacyInventoryChanged;
    
    // New system event delegates
    public event Action<ResourceType, int, int> OnResourceAmountChanged; // type, oldAmount, newAmount
    public event Action<ResourceType, int> OnResourceAdded;
    public event Action<ResourceType, int> OnResourceSpent;
    public event Action<Dictionary<ResourceType, int>> OnMultipleResourcesChanged;
    
    // Event batching
    private Dictionary<ResourceType, ResourceChangeData> pendingChanges = new Dictionary<ResourceType, ResourceChangeData>();
    private float lastEventTime = 0f;
    private bool hasPendingBatch = false;
    
    // System references
    private ResourceManager legacyResourceManager;
    private NewResourceManager newResourceManager;
    private ResourceManagerBridge bridge;
    
    // Cached values for change detection
    private float lastRecyclingPoints = 0f;
    private float lastDimensionalPotential = 0f;
    private float lastContamination = 0f;
    private Dictionary<ResourceType, int> lastResourceAmounts = new Dictionary<ResourceType, int>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEventBridge();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        SubscribeToEvents();
        InitializeCachedValues();
    }
    
    private void Update()
    {
        // Process batched events
        if (hasPendingBatch && Time.time - lastEventTime >= eventThrottleTime)
        {
            ProcessBatchedEvents();
        }
    }
    
    private void InitializeEventBridge()
    {
        // Find system references
        legacyResourceManager = ResourceManager.Instance;
        newResourceManager = NewResourceManager.Instance;
        bridge = ResourceManagerBridge.Instance;
        
        Debug.Log("ResourceEventBridge initialized");
    }
    
    private void SubscribeToEvents()
    {
        // Subscribe to new resource manager events
        if (newResourceManager != null)
        {
            newResourceManager.OnResourceChanged += HandleNewResourceChanged;
            newResourceManager.OnResourceAdded += HandleNewResourceAdded;
            newResourceManager.OnResourceSpent += HandleNewResourceSpent;
            newResourceManager.OnResourceInventoryChanged += HandleNewInventoryChanged;
        }
        
        // Subscribe to legacy resource manager events
        if (legacyResourceManager != null)
        {
            legacyResourceManager.OnRecyclingPointsChanged += HandleLegacyRPChanged;
            legacyResourceManager.OnDimensionalPotentialChanged += HandleLegacyDPChanged;
            legacyResourceManager.OnContaminationChanged += HandleLegacyContaminationChanged;
            legacyResourceManager.OnResourcesChanged += HandleLegacyResourcesChanged;
        }
        
        // Subscribe to bridge events
        if (bridge != null)
        {
            bridge.OnRecyclingPointsChanged += HandleBridgeRPChanged;
            bridge.OnDimensionalPotentialChanged += HandleBridgeDPChanged;
            bridge.OnContaminationChanged += HandleBridgeContaminationChanged;
            bridge.OnResourcesChanged += HandleBridgeResourcesChanged;
        }
    }
    
    private void InitializeCachedValues()
    {
        // Initialize cached values for change detection
        if (bridge != null)
        {
            lastRecyclingPoints = bridge.RecyclingPoints;
            lastDimensionalPotential = bridge.DimensionalPotential;
            lastContamination = bridge.ContaminationLevel;
        }
        
        if (newResourceManager != null)
        {
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                lastResourceAmounts[type] = newResourceManager.GetResourceAmount(type);
            }
        }
    }
    
    #region New System Event Handlers
    
    private void HandleNewResourceChanged(ResourceType type, int newAmount)
    {
        if (!enableNewEvents) return;
        
        int oldAmount = lastResourceAmounts.TryGetValue(type, out int cached) ? cached : 0;
        lastResourceAmounts[type] = newAmount;
        
        if (logEventTranslations)
        {
            Debug.Log($"New Resource Changed: {type} {oldAmount} → {newAmount}");
        }
        
        // Fire new system events
        OnResourceAmountChanged?.Invoke(type, oldAmount, newAmount);
        
        // Translate to legacy events if enabled
        if (enableLegacyEvents)
        {
            TranslateNewResourceToLegacy(type, oldAmount, newAmount);
        }
        
        // Batch for multi-resource event
        if (batchSimilarEvents)
        {
            BatchResourceChange(type, oldAmount, newAmount);
        }
    }
    
    private void HandleNewResourceAdded(ResourceType type, int amount)
    {
        if (!enableNewEvents) return;
        
        if (logEventTranslations)
        {
            Debug.Log($"New Resource Added: {type} +{amount}");
        }
        
        OnResourceAdded?.Invoke(type, amount);
        
        // Update cached value
        if (lastResourceAmounts.ContainsKey(type))
        {
            lastResourceAmounts[type] += amount;
        }
        else
        {
            lastResourceAmounts[type] = amount;
        }
    }
    
    private void HandleNewResourceSpent(ResourceType type, int amount)
    {
        if (!enableNewEvents) return;
        
        if (logEventTranslations)
        {
            Debug.Log($"New Resource Spent: {type} -{amount}");
        }
        
        OnResourceSpent?.Invoke(type, amount);
        
        // Update cached value
        if (lastResourceAmounts.ContainsKey(type))
        {
            lastResourceAmounts[type] = Mathf.Max(0, lastResourceAmounts[type] - amount);
        }
    }
    
    private void HandleNewInventoryChanged()
    {
        if (!enableNewEvents) return;
        
        if (logEventTranslations)
        {
            Debug.Log("New Inventory Changed");
        }
        
        // Translate to legacy inventory event
        if (enableLegacyEvents)
        {
            OnLegacyInventoryChanged?.Invoke();
        }
    }
    
    #endregion
    
    #region Legacy System Event Handlers
    
    private void HandleLegacyRPChanged(float newAmount)
    {
        if (!enableLegacyEvents) return;
        
        float oldAmount = lastRecyclingPoints;
        lastRecyclingPoints = newAmount;
        
        if (logEventTranslations)
        {
            Debug.Log($"Legacy RP Changed: {oldAmount} → {newAmount}");
        }
        
        OnLegacyRecyclingPointsChanged?.Invoke(newAmount);
    }
    
    private void HandleLegacyDPChanged(float newAmount)
    {
        if (!enableLegacyEvents) return;
        
        float oldAmount = lastDimensionalPotential;
        lastDimensionalPotential = newAmount;
        
        if (logEventTranslations)
        {
            Debug.Log($"Legacy DP Changed: {oldAmount} → {newAmount}");
        }
        
        OnLegacyDimensionalPotentialChanged?.Invoke(newAmount);
    }
    
    private void HandleLegacyContaminationChanged(float newAmount)
    {
        if (!enableLegacyEvents) return;
        
        float oldAmount = lastContamination;
        lastContamination = newAmount;
        
        if (logEventTranslations)
        {
            Debug.Log($"Legacy Contamination Changed: {oldAmount} → {newAmount}");
        }
        
        OnLegacyContaminationChanged?.Invoke(newAmount);
    }
    
    private void HandleLegacyResourcesChanged()
    {
        if (!enableLegacyEvents) return;
        
        if (logEventTranslations)
        {
            Debug.Log("Legacy Resources Changed");
        }
        
        OnLegacyResourcesChanged?.Invoke();
    }
    
    #endregion
    
    #region Bridge Event Handlers
    
    private void HandleBridgeRPChanged(float newAmount)
    {
        if (logEventTranslations)
        {
            Debug.Log($"Bridge RP Changed: {newAmount}");
        }
        
        // Forward to legacy event
        if (enableLegacyEvents)
        {
            OnLegacyRecyclingPointsChanged?.Invoke(newAmount);
        }
    }
    
    private void HandleBridgeDPChanged(float newAmount)
    {
        if (logEventTranslations)
        {
            Debug.Log($"Bridge DP Changed: {newAmount}");
        }
        
        // Forward to legacy event
        if (enableLegacyEvents)
        {
            OnLegacyDimensionalPotentialChanged?.Invoke(newAmount);
        }
    }
    
    private void HandleBridgeContaminationChanged(float newAmount)
    {
        if (logEventTranslations)
        {
            Debug.Log($"Bridge Contamination Changed: {newAmount}");
        }
        
        // Forward to legacy event
        if (enableLegacyEvents)
        {
            OnLegacyContaminationChanged?.Invoke(newAmount);
        }
    }
    
    private void HandleBridgeResourcesChanged()
    {
        if (logEventTranslations)
        {
            Debug.Log("Bridge Resources Changed");
        }
        
        // Forward to legacy event
        if (enableLegacyEvents)
        {
            OnLegacyResourcesChanged?.Invoke();
        }
    }
    
    #endregion
    
    #region Event Translation
    
    private void TranslateNewResourceToLegacy(ResourceType type, int oldAmount, int newAmount)
    {
        // Convert new resource changes to legacy RP/DP changes
        int amountDiff = newAmount - oldAmount;
        
        if (amountDiff == 0) return;
        
        // Calculate legacy value change
        float rpChange = 0f;
        float dpChange = 0f;
        
        switch (type)
        {
            case ResourceType.Plastic:
            case ResourceType.MetalScraps:
            case ResourceType.OrganicMatter:
            case ResourceType.ToxicSludge:
                rpChange = amountDiff * LegacyResourceConverter.GetRPConversionRate(type);
                break;
                
            case ResourceType.CrystalFragments:
            case ResourceType.NeuralResidue:
            case ResourceType.Energy:
                dpChange = amountDiff * LegacyResourceConverter.GetDPConversionRate(type);
                break;
                
            case ResourceType.Fuel:
            case ResourceType.Food:
            case ResourceType.Parts:
                rpChange = amountDiff * LegacyResourceConverter.GetRPConversionRate(type);
                break;
        }
        
        // Fire legacy events if there's a significant change
        if (Mathf.Abs(rpChange) > 0.1f)
        {
            float newRP = lastRecyclingPoints + rpChange;
            OnLegacyRecyclingPointsChanged?.Invoke(newRP);
            lastRecyclingPoints = newRP;
        }
        
        if (Mathf.Abs(dpChange) > 0.1f)
        {
            float newDP = lastDimensionalPotential + dpChange;
            OnLegacyDimensionalPotentialChanged?.Invoke(newDP);
            lastDimensionalPotential = newDP;
        }
        
        // Always fire general resources changed event
        OnLegacyResourcesChanged?.Invoke();
    }
    
    #endregion
    
    #region Event Batching
    
    private void BatchResourceChange(ResourceType type, int oldAmount, int newAmount)
    {
        if (!batchSimilarEvents) return;
        
        pendingChanges[type] = new ResourceChangeData
        {
            oldAmount = oldAmount,
            newAmount = newAmount,
            timestamp = Time.time
        };
        
        hasPendingBatch = true;
        lastEventTime = Time.time;
    }
    
    private void ProcessBatchedEvents()
    {
        if (pendingChanges.Count == 0)
        {
            hasPendingBatch = false;
            return;
        }
        
        // Create batched event data
        var batchedChanges = new Dictionary<ResourceType, int>();
        
        foreach (var kvp in pendingChanges)
        {
            batchedChanges[kvp.Key] = kvp.Value.newAmount;
        }
        
        if (logEventTranslations)
        {
            Debug.Log($"Processing batched events for {batchedChanges.Count} resources");
        }
        
        // Fire batched event
        OnMultipleResourcesChanged?.Invoke(batchedChanges);
        
        // Clear pending changes
        pendingChanges.Clear();
        hasPendingBatch = false;
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Enable or disable legacy event forwarding
    /// </summary>
    /// <param name="enabled">Whether to enable legacy events</param>
    public void SetLegacyEventsEnabled(bool enabled)
    {
        enableLegacyEvents = enabled;
        Debug.Log($"Legacy events: {(enabled ? "Enabled" : "Disabled")}");
    }
    
    /// <summary>
    /// Enable or disable new system event forwarding
    /// </summary>
    /// <param name="enabled">Whether to enable new events</param>
    public void SetNewEventsEnabled(bool enabled)
    {
        enableNewEvents = enabled;
        Debug.Log($"New events: {(enabled ? "Enabled" : "Disabled")}");
    }
    
    /// <summary>
    /// Enable or disable event translation logging
    /// </summary>
    /// <param name="enabled">Whether to log event translations</param>
    public void SetEventLoggingEnabled(bool enabled)
    {
        logEventTranslations = enabled;
        Debug.Log($"Event logging: {(enabled ? "Enabled" : "Disabled")}");
    }
    
    /// <summary>
    /// Force process any pending batched events
    /// </summary>
    public void FlushBatchedEvents()
    {
        if (hasPendingBatch)
        {
            ProcessBatchedEvents();
        }
    }
    
    /// <summary>
    /// Manually trigger a legacy resource change event
    /// </summary>
    /// <param name="recyclingPoints">New recycling points value</param>
    /// <param name="dimensionalPotential">New dimensional potential value</param>
    public void TriggerLegacyResourceChange(float recyclingPoints, float dimensionalPotential)
    {
        if (enableLegacyEvents)
        {
            OnLegacyRecyclingPointsChanged?.Invoke(recyclingPoints);
            OnLegacyDimensionalPotentialChanged?.Invoke(dimensionalPotential);
            OnLegacyResourcesChanged?.Invoke();
        }
    }
    
    /// <summary>
    /// Manually trigger a new resource change event
    /// </summary>
    /// <param name="type">Resource type</param>
    /// <param name="oldAmount">Previous amount</param>
    /// <param name="newAmount">New amount</param>
    public void TriggerNewResourceChange(ResourceType type, int oldAmount, int newAmount)
    {
        if (enableNewEvents)
        {
            OnResourceAmountChanged?.Invoke(type, oldAmount, newAmount);
        }
    }
    
    /// <summary>
    /// Get current event bridge status
    /// </summary>
    /// <returns>Event bridge status information</returns>
    public EventBridgeStatus GetStatus()
    {
        return new EventBridgeStatus
        {
            legacyEventsEnabled = enableLegacyEvents,
            newEventsEnabled = enableNewEvents,
            eventLoggingEnabled = logEventTranslations,
            pendingBatchedEvents = pendingChanges.Count,
            lastEventTime = lastEventTime,
            hasPendingBatch = hasPendingBatch
        };
    }
    
    #endregion
    
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (newResourceManager != null)
        {
            newResourceManager.OnResourceChanged -= HandleNewResourceChanged;
            newResourceManager.OnResourceAdded -= HandleNewResourceAdded;
            newResourceManager.OnResourceSpent -= HandleNewResourceSpent;
            newResourceManager.OnResourceInventoryChanged -= HandleNewInventoryChanged;
        }
        
        if (legacyResourceManager != null)
        {
            legacyResourceManager.OnRecyclingPointsChanged -= HandleLegacyRPChanged;
            legacyResourceManager.OnDimensionalPotentialChanged -= HandleLegacyDPChanged;
            legacyResourceManager.OnContaminationChanged -= HandleLegacyContaminationChanged;
            legacyResourceManager.OnResourcesChanged -= HandleLegacyResourcesChanged;
        }
        
        if (bridge != null)
        {
            bridge.OnRecyclingPointsChanged -= HandleBridgeRPChanged;
            bridge.OnDimensionalPotentialChanged -= HandleBridgeDPChanged;
            bridge.OnContaminationChanged -= HandleBridgeContaminationChanged;
            bridge.OnResourcesChanged -= HandleBridgeResourcesChanged;
        }
    }
}

/// <summary>
/// Data structure for batched resource changes
/// </summary>
[System.Serializable]
public class ResourceChangeData
{
    public int oldAmount;
    public int newAmount;
    public float timestamp;
}

/// <summary>
/// Event bridge status information
/// </summary>
[System.Serializable]
public class EventBridgeStatus
{
    public bool legacyEventsEnabled;
    public bool newEventsEnabled;
    public bool eventLoggingEnabled;
    public int pendingBatchedEvents;
    public float lastEventTime;
    public bool hasPendingBatch;
    
    public override string ToString()
    {
        return $"EventBridge Status:\n" +
               $"Legacy Events: {(legacyEventsEnabled ? "Enabled" : "Disabled")}\n" +
               $"New Events: {(newEventsEnabled ? "Enabled" : "Disabled")}\n" +
               $"Event Logging: {(eventLoggingEnabled ? "Enabled" : "Disabled")}\n" +
               $"Pending Batched Events: {pendingBatchedEvents}\n" +
               $"Has Pending Batch: {hasPendingBatch}";
    }
} 