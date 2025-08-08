using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Simplified coordinator for the resource management system
/// Orchestrates core components without advanced features
/// </summary>
public class ResourceSystemCoordinator : MonoBehaviour
{
    public static ResourceSystemCoordinator Instance { get; private set; }
    
    [Header("System Components")]
    [SerializeField] private bool autoInitializeOnStart = true;
    [SerializeField] private bool enableLegacyCompatibility = true;
    
    [Header("System Status")]
    [SerializeField] private SystemStatus currentStatus = SystemStatus.Uninitialized;
    [SerializeField] private float initializationProgress = 0f;
    [SerializeField] private bool isSystemHealthy = false;
    
    // Component references
    private NewResourceManager newResourceManager;
    private ResourceManagerBridge bridge;
    private ResourceConfigManager configManager;
    
    // Events
    public event Action<SystemStatus> OnSystemStatusChanged;
    public event Action<float> OnInitializationProgress;
    public event Action OnSystemInitialized;
    public event Action<string> OnSystemError;
    
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
    
    private void Start()
    {
        if (autoInitializeOnStart)
        {
            StartCoroutine(InitializeSystem());
        }
    }
    
    /// <summary>
    /// Initialize the core resource management system
    /// </summary>
    public IEnumerator InitializeSystem()
    {
        Debug.Log("Starting resource system initialization...");
        SetSystemStatus(SystemStatus.Initializing);
        
        yield return StartCoroutine(InitializeComponents());
        yield return StartCoroutine(LoadConfigurations());
        yield return StartCoroutine(FinalizeInitialization());
        
        SetSystemStatus(SystemStatus.Running);
        OnSystemInitialized?.Invoke();
        
        Debug.Log("Resource system initialization complete!");
    }
    
    private IEnumerator InitializeComponents()
    {
        Debug.Log("Initializing system components...");
        UpdateProgress(0.2f);
        
        // Initialize or find existing components
        yield return StartCoroutine(InitializeResourceManager());
        yield return StartCoroutine(InitializeConfigManager());
        yield return StartCoroutine(InitializeBridge());
        
        UpdateProgress(0.6f);
        yield return new WaitForSeconds(0.1f);
    }
    
    private IEnumerator InitializeResourceManager()
    {
        newResourceManager = NewResourceManager.Instance;
        if (newResourceManager == null)
        {
            Debug.LogWarning("NewResourceManager not found in scene");
        }
        else
        {
            Debug.Log("Found NewResourceManager instance");
        }
        yield return null;
    }
    
    private IEnumerator InitializeConfigManager()
    {
        configManager = ResourceConfigManager.Instance;
        if (configManager == null)
        {
            Debug.LogWarning("ResourceConfigManager not found in scene");
        }
        else
        {
            Debug.Log("Found ResourceConfigManager instance");
        }
        yield return null;
    }
    
    private IEnumerator InitializeBridge()
    {
        bridge = ResourceManagerBridge.Instance;
        if (bridge == null && enableLegacyCompatibility)
        {
            Debug.LogWarning("ResourceManagerBridge not found in scene");
        }
        else if (bridge != null)
        {
            Debug.Log("Found ResourceManagerBridge instance");
        }
        yield return null;
    }
    
    private IEnumerator LoadConfigurations()
    {
        Debug.Log("Loading system configurations...");
        UpdateProgress(0.8f);
        
        if (configManager != null)
        {
            // Basic configuration loading
            yield return new WaitForSeconds(0.2f); // Simulate loading time
        }
        
        yield return new WaitForSeconds(0.1f);
    }
    
    private IEnumerator FinalizeInitialization()
    {
        Debug.Log("Finalizing system initialization...");
        UpdateProgress(0.9f);
        
        // Perform basic system checks
        isSystemHealthy = ValidateBasicSystemHealth();
        
        UpdateProgress(1.0f);
        yield return new WaitForSeconds(0.1f);
    }
    
    private bool ValidateBasicSystemHealth()
    {
        try
        {
            // Check critical components
            if (newResourceManager == null || configManager == null)
            {
                Debug.LogWarning("Critical components missing");
                return false;
            }
            
            // Check system status
            if (currentStatus == SystemStatus.Error)
            {
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            OnSystemError?.Invoke($"System health check failed: {ex.Message}");
            return false;
        }
    }
    
    #region Public API
    
    /// <summary>
    /// Manually initialize the system
    /// </summary>
    public void ManualInitialize()
    {
        if (currentStatus == SystemStatus.Uninitialized)
        {
            StartCoroutine(InitializeSystem());
        }
    }
    
    /// <summary>
    /// Get current system status information
    /// </summary>
    /// <returns>Simple system status data</returns>
    public SimpleSystemStatus GetSystemStatus()
    {
        return new SimpleSystemStatus
        {
            status = currentStatus,
            isHealthy = isSystemHealthy,
            initializationProgress = initializationProgress,
            hasNewResourceManager = newResourceManager != null,
            hasConfigManager = configManager != null,
            hasBridge = bridge != null
        };
    }
    
    /// <summary>
    /// Enable or disable legacy compatibility
    /// </summary>
    /// <param name="enabled">Whether to enable legacy compatibility</param>
    public void SetLegacyCompatibility(bool enabled)
    {
        enableLegacyCompatibility = enabled;
        Debug.Log($"Legacy compatibility: {(enabled ? "Enabled" : "Disabled")}");
    }
    
    #endregion
    
    #region Helper Methods
    
    private void SetSystemStatus(SystemStatus status)
    {
        if (currentStatus != status)
        {
            currentStatus = status;
            OnSystemStatusChanged?.Invoke(status);
            Debug.Log($"System status changed to: {status}");
        }
    }
    
    private void UpdateProgress(float progress)
    {
        initializationProgress = progress;
        OnInitializationProgress?.Invoke(progress);
    }
    
    #endregion
}

/// <summary>
/// Simple system status data structure
/// </summary>
[System.Serializable]
public class SimpleSystemStatus
{
    public SystemStatus status;
    public bool isHealthy;
    public float initializationProgress;
    public bool hasNewResourceManager;
    public bool hasConfigManager;
    public bool hasBridge;
    
    public override string ToString()
    {
        return $"System Status: {status}\n" +
               $"Healthy: {isHealthy}\n" +
               $"Progress: {initializationProgress:P1}\n" +
               $"Components: NRM({hasNewResourceManager}) CFG({hasConfigManager}) BRG({hasBridge})";
    }
}