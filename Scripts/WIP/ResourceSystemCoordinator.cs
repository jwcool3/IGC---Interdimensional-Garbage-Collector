using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Master coordinator for the resource management system
/// Orchestrates all components and provides a single entry point for system management
/// </summary>
public class ResourceSystemCoordinator : MonoBehaviour
{
    public static ResourceSystemCoordinator Instance { get; private set; }
    
    [Header("System Components")]
    [SerializeField] private bool autoInitializeOnStart = true;
    [SerializeField] private bool enableSystemValidation = true;
    [SerializeField] private bool enablePerformanceMonitoring = true;
    
    [Header("Integration Settings")]
    [SerializeField] private bool enableLegacyCompatibility = true;
    [SerializeField] private bool autoMigrateLegacyData = true;
    [SerializeField] private float systemCheckInterval = 5f;
    
    [Header("System Status")]
    [SerializeField] private SystemStatus currentStatus = SystemStatus.NotInitialized;
    [SerializeField] private float initializationProgress = 0f;
    [SerializeField] private bool isSystemHealthy = false;
    
    // Component references
    private NewResourceManager newResourceManager;
    private ResourceManagerBridge bridge;
    private ResourceEventBridge eventBridge;
    private ResourceConfigManager configManager;
    private LegacySystemIntegrator integrator;
    
    // System monitoring
    private Coroutine systemMonitoringCoroutine;
    private SystemValidationReport lastValidationReport;
    private float lastSystemCheckTime;
    
    // Events
    public event Action<SystemStatus> OnSystemStatusChanged;
    public event Action<float> OnInitializationProgress;
    public event Action OnSystemInitialized;
    public event Action<SystemValidationReport> OnSystemValidated;
    public event Action<string> OnSystemError;
    
    // Performance tracking
    private Dictionary<string, float> performanceMetrics = new Dictionary<string, float>();
    private Queue<float> frameTimeHistory = new Queue<float>();
    private const int FRAME_HISTORY_SIZE = 60;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePerformanceTracking();
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
    
    private void Update()
    {
        if (enablePerformanceMonitoring)
        {
            TrackPerformance();
        }
    }
    
    /// <summary>
    /// Initialize the complete resource management system
    /// </summary>
    public IEnumerator InitializeSystem()
    {
        Debug.Log("Starting resource system initialization...");
        SetSystemStatus(SystemStatus.Initializing);
        
        yield return StartCoroutine(InitializeComponents());
        yield return StartCoroutine(LoadConfigurations());
        yield return StartCoroutine(SetupIntegration());
        yield return StartCoroutine(ValidateSystem());
        
        if (autoMigrateLegacyData)
        {
            yield return StartCoroutine(MigrateLegacyData());
        }
        
        yield return StartCoroutine(FinalizeInitialization());
        
        SetSystemStatus(SystemStatus.Running);
        OnSystemInitialized?.Invoke();
        
        if (enableSystemValidation)
        {
            StartSystemMonitoring();
        }
        
        Debug.Log("Resource system initialization complete!");
    }
    
    private IEnumerator InitializeComponents()
    {
        Debug.Log("Initializing system components...");
        UpdateProgress(0.1f);
        
        // Initialize or find existing components
        yield return StartCoroutine(InitializeResourceManager());
        yield return StartCoroutine(InitializeConfigManager());
        yield return StartCoroutine(InitializeBridge());
        yield return StartCoroutine(InitializeEventBridge());
        yield return StartCoroutine(InitializeIntegrator());
        
        UpdateProgress(0.3f);
        yield return new WaitForSeconds(0.1f);
    }
    
    private IEnumerator InitializeResourceManager()
    {
        newResourceManager = NewResourceManager.Instance;
        if (newResourceManager == null)
        {
            var managerGO = new GameObject("NewResourceManager");
            newResourceManager = managerGO.AddComponent<NewResourceManager>();
            Debug.Log("Created NewResourceManager instance");
        }
        yield return null;
    }
    
    private IEnumerator InitializeConfigManager()
    {
        configManager = ResourceConfigManager.Instance;
        if (configManager == null)
        {
            var configGO = new GameObject("ResourceConfigManager");
            configManager = configGO.AddComponent<ResourceConfigManager>();
            Debug.Log("Created ResourceConfigManager instance");
        }
        yield return null;
    }
    
    private IEnumerator InitializeBridge()
    {
        bridge = ResourceManagerBridge.Instance;
        if (bridge == null)
        {
            var bridgeGO = new GameObject("ResourceManagerBridge");
            bridge = bridgeGO.AddComponent<ResourceManagerBridge>();
            Debug.Log("Created ResourceManagerBridge instance");
        }
        yield return null;
    }
    
    private IEnumerator InitializeEventBridge()
    {
        eventBridge = ResourceEventBridge.Instance;
        if (eventBridge == null)
        {
            var eventGO = new GameObject("ResourceEventBridge");
            eventBridge = eventGO.AddComponent<ResourceEventBridge>();
            Debug.Log("Created ResourceEventBridge instance");
        }
        yield return null;
    }
    
    private IEnumerator InitializeIntegrator()
    {
        integrator = LegacySystemIntegrator.Instance;
        if (integrator == null)
        {
            var integratorGO = new GameObject("LegacySystemIntegrator");
            integrator = integratorGO.AddComponent<LegacySystemIntegrator>();
            Debug.Log("Created LegacySystemIntegrator instance");
        }
        yield return null;
    }
    
    private IEnumerator LoadConfigurations()
    {
        Debug.Log("Loading system configurations...");
        UpdateProgress(0.4f);
        
        if (configManager != null)
        {
            configManager.LoadAllConfigurations();
            
            // Wait for configurations to load
            float timeout = 5f;
            float elapsed = 0f;
            
            while (!configManager.GetConfigurationStats().isLoaded && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (elapsed >= timeout)
            {
                OnSystemError?.Invoke("Configuration loading timed out");
            }
        }
        
        UpdateProgress(0.5f);
        yield return new WaitForSeconds(0.1f);
    }
    
    private IEnumerator SetupIntegration()
    {
        Debug.Log("Setting up system integration...");
        UpdateProgress(0.6f);
        
        // Configure bridge settings
        if (bridge != null && enableLegacyCompatibility)
        {
            // Set up legacy compatibility settings
            yield return null;
        }
        
        // Configure event bridge
        if (eventBridge != null)
        {
            eventBridge.SetLegacyEventsEnabled(enableLegacyCompatibility);
            eventBridge.SetNewEventsEnabled(true);
            yield return null;
        }
        
        UpdateProgress(0.7f);
        yield return new WaitForSeconds(0.1f);
    }
    
    private IEnumerator ValidateSystem()
    {
        Debug.Log("Validating system integrity...");
        UpdateProgress(0.8f);
        
        if (enableSystemValidation)
        {
            yield return StartCoroutine(PerformSystemValidation());
        }
        
        UpdateProgress(0.9f);
        yield return new WaitForSeconds(0.1f);
    }
    
    private IEnumerator MigrateLegacyData()
    {
        Debug.Log("Migrating legacy data...");
        
        if (integrator != null)
        {
            yield return StartCoroutine(integrator.BeginMigrationProcess());
        }
        
        yield return new WaitForSeconds(0.1f);
    }
    
    private IEnumerator FinalizeInitialization()
    {
        Debug.Log("Finalizing system initialization...");
        UpdateProgress(0.95f);
        
        // Perform final system checks
        isSystemHealthy = ValidateSystemHealth();
        
        // Subscribe to system events
        SubscribeToSystemEvents();
        
        UpdateProgress(1.0f);
        yield return new WaitForSeconds(0.1f);
    }
    
    private IEnumerator PerformSystemValidation()
    {
        yield return null; // Allow frame to process
        
        lastValidationReport = SystemValidationUtility.ValidateCompleteSystem();
        OnSystemValidated?.Invoke(lastValidationReport);
        
        if (lastValidationReport.overallStatus == ValidationStatus.Critical)
        {
            OnSystemError?.Invoke($"Critical system validation failures: {lastValidationReport.TotalIssues} issues");
            SetSystemStatus(SystemStatus.Error);
        }
        else if (lastValidationReport.overallStatus == ValidationStatus.Warning)
        {
            Debug.LogWarning($"System validation completed with warnings: {lastValidationReport.TotalIssues} issues");
        }
        
        yield return null;
    }
    
    private void StartSystemMonitoring()
    {
        if (systemMonitoringCoroutine != null)
        {
            StopCoroutine(systemMonitoringCoroutine);
        }
        
        systemMonitoringCoroutine = StartCoroutine(SystemMonitoringLoop());
    }
    
    private IEnumerator SystemMonitoringLoop()
    {
        while (currentStatus == SystemStatus.Running)
        {
            yield return new WaitForSeconds(systemCheckInterval);
            
            // Perform periodic system health checks
            yield return StartCoroutine(PerformSystemValidation());
            
            // Update system health status
            isSystemHealthy = ValidateSystemHealth();
            
            // Check for performance issues
            CheckPerformanceMetrics();
            
            lastSystemCheckTime = Time.time;
        }
    }
    
    private bool ValidateSystemHealth()
    {
        try
        {
            // Check critical components
            if (newResourceManager == null || configManager == null)
                return false;
            
            // Check system status
            if (currentStatus == SystemStatus.Error)
                return false;
            
            // Check validation report
            if (lastValidationReport != null && lastValidationReport.overallStatus == ValidationStatus.Critical)
                return false;
            
            return true;
        }
        catch (Exception ex)
        {
            OnSystemError?.Invoke($"System health check failed: {ex.Message}");
            return false;
        }
    }
    
    private void SubscribeToSystemEvents()
    {
        // Subscribe to component events for monitoring
        if (integrator != null)
        {
            integrator.OnMigrationError += HandleMigrationError;
            integrator.OnMigrationComplete += HandleMigrationComplete;
        }
        
        if (configManager != null)
        {
            configManager.OnConfigurationError += HandleConfigurationError;
        }
    }
    
    private void HandleMigrationError(string error)
    {
        OnSystemError?.Invoke($"Migration error: {error}");
    }
    
    private void HandleMigrationComplete()
    {
        Debug.Log("Legacy data migration completed successfully");
    }
    
    private void HandleConfigurationError(string error)
    {
        OnSystemError?.Invoke($"Configuration error: {error}");
    }
    
    #region Performance Monitoring
    
    private void InitializePerformanceTracking()
    {
        performanceMetrics["FrameTime"] = 0f;
        performanceMetrics["MemoryUsage"] = 0f;
        performanceMetrics["ResourceOperations"] = 0f;
    }
    
    private void TrackPerformance()
    {
        // Track frame time
        float frameTime = Time.deltaTime;
        frameTimeHistory.Enqueue(frameTime);
        
        if (frameTimeHistory.Count > FRAME_HISTORY_SIZE)
        {
            frameTimeHistory.Dequeue();
        }
        
        // Calculate average frame time
        float totalFrameTime = 0f;
        foreach (float time in frameTimeHistory)
        {
            totalFrameTime += time;
        }
        performanceMetrics["FrameTime"] = totalFrameTime / frameTimeHistory.Count;
        
        // Track memory usage
        performanceMetrics["MemoryUsage"] = GC.GetTotalMemory(false) / (1024f * 1024f); // MB
    }
    
    private void CheckPerformanceMetrics()
    {
        // Check for performance issues
        if (performanceMetrics["FrameTime"] > 0.033f) // 30 FPS threshold
        {
            Debug.LogWarning($"Performance warning: Low frame rate detected ({1f / performanceMetrics["FrameTime"]:F1} FPS)");
        }
        
        if (performanceMetrics["MemoryUsage"] > 500f) // 500 MB threshold
        {
            Debug.LogWarning($"Performance warning: High memory usage detected ({performanceMetrics["MemoryUsage"]:F1} MB)");
        }
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Manually initialize the system
    /// </summary>
    public void ManualInitialize()
    {
        if (currentStatus == SystemStatus.NotInitialized)
        {
            StartCoroutine(InitializeSystem());
        }
    }
    
    /// <summary>
    /// Restart the entire system
    /// </summary>
    public void RestartSystem()
    {
        SetSystemStatus(SystemStatus.Restarting);
        StopAllCoroutines();
        StartCoroutine(RestartSystemCoroutine());
    }
    
    private IEnumerator RestartSystemCoroutine()
    {
        // Clean up current state
        SetSystemStatus(SystemStatus.NotInitialized);
        initializationProgress = 0f;
        
        yield return new WaitForSeconds(0.5f);
        
        // Reinitialize
        yield return StartCoroutine(InitializeSystem());
    }
    
    /// <summary>
    /// Perform immediate system validation
    /// </summary>
    public void ValidateSystemNow()
    {
        StartCoroutine(PerformSystemValidation());
    }
    
    /// <summary>
    /// Get current system status information
    /// </summary>
    /// <returns>System status data</returns>
    public SystemStatusData GetSystemStatus()
    {
        return new SystemStatusData
        {
            status = currentStatus,
            isHealthy = isSystemHealthy,
            initializationProgress = initializationProgress,
            lastValidationReport = lastValidationReport,
            performanceMetrics = new Dictionary<string, float>(performanceMetrics),
            lastSystemCheckTime = lastSystemCheckTime,
            uptime = Time.time
        };
    }
    
    /// <summary>
    /// Enable or disable legacy compatibility
    /// </summary>
    /// <param name="enabled">Whether to enable legacy compatibility</param>
    public void SetLegacyCompatibility(bool enabled)
    {
        enableLegacyCompatibility = enabled;
        
        if (eventBridge != null)
        {
            eventBridge.SetLegacyEventsEnabled(enabled);
        }
        
        Debug.Log($"Legacy compatibility: {(enabled ? "Enabled" : "Disabled")}");
    }
    
    /// <summary>
    /// Force garbage collection and system cleanup
    /// </summary>
    public void PerformSystemCleanup()
    {
        GC.Collect();
        Resources.UnloadUnusedAssets();
        Debug.Log("System cleanup performed");
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
    
    private void OnDestroy()
    {
        if (systemMonitoringCoroutine != null)
        {
            StopCoroutine(systemMonitoringCoroutine);
        }
        
        // Unsubscribe from events
        if (integrator != null)
        {
            integrator.OnMigrationError -= HandleMigrationError;
            integrator.OnMigrationComplete -= HandleMigrationComplete;
        }
        
        if (configManager != null)
        {
            configManager.OnConfigurationError -= HandleConfigurationError;
        }
    }
}

/// <summary>
/// System status enumeration
/// </summary>
public enum SystemStatus
{
    NotInitialized,
    Initializing,
    Running,
    Error,
    Restarting
}

/// <summary>
/// Complete system status data
/// </summary>
[System.Serializable]
public class SystemStatusData
{
    public SystemStatus status;
    public bool isHealthy;
    public float initializationProgress;
    public SystemValidationReport lastValidationReport;
    public Dictionary<string, float> performanceMetrics;
    public float lastSystemCheckTime;
    public float uptime;
    
    public override string ToString()
    {
        return $"System Status: {status}\n" +
               $"Healthy: {isHealthy}\n" +
               $"Progress: {initializationProgress:P1}\n" +
               $"Uptime: {uptime:F1}s\n" +
               $"Last Check: {lastSystemCheckTime:F1}s ago";
    }
} 