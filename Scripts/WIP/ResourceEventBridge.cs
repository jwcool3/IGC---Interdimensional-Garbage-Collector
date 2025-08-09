using UnityEngine;
using System;

/// <summary>
/// Event bridge for resource system communication
/// Handles events and notifications between resource components
/// </summary>
public class ResourceEventBridge : MonoBehaviour
{
    public static ResourceEventBridge Instance { get; private set; }
    
    [Header("Event Settings")]
    [SerializeField] private bool enableEventLogging = false;
    [SerializeField] private bool enableLegacyEvents = true;
    
    // Modern events
    public event Action<ResourceType, int, int> OnResourceChanged; // type, oldAmount, newAmount
    public event Action<ResourceType, int> OnResourceAdded; // type, amount
    public event Action<ResourceType, int> OnResourceSpent; // type, amount
    public event Action OnResourcesUpdated;
    
    // Legacy events for backwards compatibility
    public event Action<float> OnRecyclingPointsChanged;
    public event Action<float> OnDimensionalPotentialChanged;
    public event Action OnResourceInventoryChanged;
    
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
    
    private void InitializeEventBridge()
    {
        // Subscribe to ResourceManager events
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourceChanged += HandleResourceChanged;
            ResourceManager.Instance.OnResourceAdded += HandleResourceAdded;
            ResourceManager.Instance.OnResourceSpent += HandleResourceSpent;
            ResourceManager.Instance.OnResourcesUpdated += HandleResourcesUpdated;
        }
        
        if (enableEventLogging)
        {
            Debug.Log("ResourceEventBridge initialized successfully");
        }
    }
    
    #region Event Handlers
    
    private void HandleResourceChanged(ResourceType type, int oldAmount, int newAmount)
    {
        OnResourceChanged?.Invoke(type, oldAmount, newAmount);
        
        if (enableEventLogging)
        {
            Debug.Log($"Resource changed: {type} {oldAmount} → {newAmount}");
        }
    }
    
    private void HandleResourceAdded(ResourceType type, int amount)
    {
        OnResourceAdded?.Invoke(type, amount);
        
        if (enableEventLogging)
        {
            Debug.Log($"Resource added: {type} +{amount}");
        }
    }
    
    private void HandleResourceSpent(ResourceType type, int amount)
    {
        OnResourceSpent?.Invoke(type, amount);
        
        if (enableEventLogging)
        {
            Debug.Log($"Resource spent: {type} -{amount}");
        }
    }
    
    private void HandleResourcesUpdated()
    {
        OnResourcesUpdated?.Invoke();
        
        if (enableLegacyEvents)
        {
            OnResourceInventoryChanged?.Invoke();
        }
        
        if (enableEventLogging)
        {
            Debug.Log("Resources updated");
        }
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Manually trigger a resource change event
    /// </summary>
    public void TriggerResourceChanged(ResourceType type, int oldAmount, int newAmount)
    {
        OnResourceChanged?.Invoke(type, oldAmount, newAmount);
    }
    
    /// <summary>
    /// Enable or disable event logging
    /// </summary>
    public void SetEventLogging(bool enabled)
    {
        enableEventLogging = enabled;
        Debug.Log($"Event logging: {(enabled ? "Enabled" : "Disabled")}");
    }
    
    /// <summary>
    /// Enable or disable legacy event compatibility
    /// </summary>
    public void SetLegacyEvents(bool enabled)
    {
        enableLegacyEvents = enabled;
        Debug.Log($"Legacy events: {(enabled ? "Enabled" : "Disabled")}");
    }

    /// <summary>
    /// Get the current status of the event bridge (for validation)
    /// </summary>
    /// <returns>Event bridge status information</returns>
    public EventBridgeStatus GetStatus()
    {
        return new EventBridgeStatus
        {
            legacyEventsEnabled = enableLegacyEvents,
            newEventsEnabled = true, // Modern events are always enabled
            eventLoggingEnabled = enableEventLogging,
            pendingBatchedEvents = 0, // Simple implementation for now
            isHealthy = Instance != null
        };
    }

    #endregion
    
    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnResourceChanged -= HandleResourceChanged;
            ResourceManager.Instance.OnResourceAdded -= HandleResourceAdded;
            ResourceManager.Instance.OnResourceSpent -= HandleResourceSpent;
            ResourceManager.Instance.OnResourcesUpdated -= HandleResourcesUpdated;
        }
    }
}

/// <summary>
/// Event bridge status information for validation
/// </summary>
[System.Serializable]
public struct EventBridgeStatus
{
    public bool legacyEventsEnabled;
    public bool newEventsEnabled;
    public bool eventLoggingEnabled;
    public int pendingBatchedEvents;
    public bool isHealthy;
}