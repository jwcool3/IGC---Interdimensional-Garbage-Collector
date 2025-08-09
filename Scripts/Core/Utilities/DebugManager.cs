using UnityEngine;

/// <summary>
/// Centralized debug manager for controlling debug output throughout the game
/// Provides category-based toggle controls to reduce console spam
/// </summary>
public class DebugManager : MonoBehaviour
{
    [Header("Debug Categories")]
    [SerializeField] private bool enableWasteGeneration = false;
    [SerializeField] private bool enableWasteProcessing = false;
    [SerializeField] private bool enableLocationSystem = false;
    [SerializeField] private bool enableInventoryManagement = false;
    [SerializeField] private bool enableResourceSystem = false;
    [SerializeField] private bool enableUIDebug = false;
    [SerializeField] private bool enableProbeSystem = false;
    [SerializeField] private bool enableFacilitySystem = false;
    [SerializeField] private bool enableTradeSystem = false;
    [SerializeField] private bool enableGeneralGameplay = false;
    
    [Header("Global Controls")]
    [SerializeField] private bool enableAllDebugLogs = false;
    [SerializeField] private bool enableErrorLogsOnly = false;
    [SerializeField] private bool enableWarningsAndErrors = true;
    
    [Header("Performance")]
    [SerializeField] private int maxLogsPerFrame = 10;
    private int logsThisFrame = 0;
    
    // Singleton instance
    public static DebugManager Instance { get; private set; }
    
    // Public properties for easy access
    public static bool WasteGeneration => Instance?.enableWasteGeneration ?? false;
    public static bool WasteProcessing => Instance?.enableWasteProcessing ?? false;
    public static bool LocationSystem => Instance?.enableLocationSystem ?? false;
    public static bool InventoryManagement => Instance?.enableInventoryManagement ?? false;
    public static bool ResourceSystem => Instance?.enableResourceSystem ?? false;
    public static bool UIDebug => Instance?.enableUIDebug ?? false;
    public static bool ProbeSystem => Instance?.enableProbeSystem ?? false;
    public static bool FacilitySystem => Instance?.enableFacilitySystem ?? false;
    public static bool TradeSystem => Instance?.enableTradeSystem ?? false;
    public static bool GeneralGameplay => Instance?.enableGeneralGameplay ?? false;
    
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
    
    private void LateUpdate()
    {
        // Reset log counter each frame
        logsThisFrame = 0;
    }
    
    /// <summary>
    /// Log a debug message if the category is enabled
    /// </summary>
    /// <param name="message">Message to log</param>
    /// <param name="category">Debug category</param>
    /// <param name="logType">Type of log (Info, Warning, Error)</param>
    public static void Log(string message, DebugCategory category, LogType logType = LogType.Log)
    {
        // Auto-create DebugManager if it doesn't exist
        if (Instance == null)
        {
            CreateDebugManager();
        }
        
        if (Instance == null) 
        {
            // Fallback to regular Debug.Log if creation failed
            Debug.Log($"[{category}] {message}");
            return;
        }
        
        // Check if we've exceeded max logs per frame
        if (Instance.logsThisFrame >= Instance.maxLogsPerFrame && logType == LogType.Log)
            return;
            
        // Check global controls first
        if (Instance.enableErrorLogsOnly && logType != LogType.Error)
            return;
            
        if (!Instance.enableWarningsAndErrors && logType == LogType.Warning)
            return;
            
        // If all debug logs are disabled and it's not an error/warning, skip
        if (!Instance.enableAllDebugLogs && logType == LogType.Log && !Instance.IsCategoryEnabled(category))
            return;
            
        // If all debug logs are enabled, always log
        if (Instance.enableAllDebugLogs || Instance.IsCategoryEnabled(category))
        {
            string formattedMessage = $"[{category}] {message}";
            
            switch (logType)
            {
                case LogType.Log:
                    Debug.Log(formattedMessage);
                    Instance.logsThisFrame++;
                    break;
                case LogType.Warning:
                    Debug.LogWarning(formattedMessage);
                    break;
                case LogType.Error:
                    Debug.LogError(formattedMessage);
                    break;
            }
        }
    }
    
    /// <summary>
    /// Log a warning message
    /// </summary>
    public static void LogWarning(string message, DebugCategory category)
    {
        Log(message, category, LogType.Warning);
    }
    
    /// <summary>
    /// Log an error message
    /// </summary>
    public static void LogError(string message, DebugCategory category)
    {
        Log(message, category, LogType.Error);
    }
    
    /// <summary>
    /// Auto-create DebugManager if it doesn't exist
    /// </summary>
    private static void CreateDebugManager()
    {
        if (Instance != null) return;
        
        GameObject debugManagerObj = new GameObject("DebugManager");
        debugManagerObj.AddComponent<DebugManager>();
        
        // Set it to not be destroyed on load
        DontDestroyOnLoad(debugManagerObj);
    }
    
    /// <summary>
    /// Check if a specific debug category is enabled
    /// </summary>
    private bool IsCategoryEnabled(DebugCategory category)
    {
        return category switch
        {
            DebugCategory.WasteGeneration => enableWasteGeneration,
            DebugCategory.WasteProcessing => enableWasteProcessing,
            DebugCategory.LocationSystem => enableLocationSystem,
            DebugCategory.InventoryManagement => enableInventoryManagement,
            DebugCategory.ResourceSystem => enableResourceSystem,
            DebugCategory.UIDebug => enableUIDebug,
            DebugCategory.ProbeSystem => enableProbeSystem,
            DebugCategory.FacilitySystem => enableFacilitySystem,
            DebugCategory.TradeSystem => enableTradeSystem,
            DebugCategory.GeneralGameplay => enableGeneralGameplay,
            _ => false
        };
    }
    
    /// <summary>
    /// Enable/disable all debug categories
    /// </summary>
    public void SetAllCategories(bool enabled)
    {
        enableWasteGeneration = enabled;
        enableWasteProcessing = enabled;
        enableLocationSystem = enabled;
        enableInventoryManagement = enabled;
        enableResourceSystem = enabled;
        enableUIDebug = enabled;
        enableProbeSystem = enabled;
        enableFacilitySystem = enabled;
        enableTradeSystem = enabled;
        enableGeneralGameplay = enabled;
    }
    
    /// <summary>
    /// Toggle a specific category
    /// </summary>
    public void ToggleCategory(DebugCategory category)
    {
        switch (category)
        {
            case DebugCategory.WasteGeneration:
                enableWasteGeneration = !enableWasteGeneration;
                break;
            case DebugCategory.WasteProcessing:
                enableWasteProcessing = !enableWasteProcessing;
                break;
            case DebugCategory.LocationSystem:
                enableLocationSystem = !enableLocationSystem;
                break;
            case DebugCategory.InventoryManagement:
                enableInventoryManagement = !enableInventoryManagement;
                break;
            case DebugCategory.ResourceSystem:
                enableResourceSystem = !enableResourceSystem;
                break;
            case DebugCategory.UIDebug:
                enableUIDebug = !enableUIDebug;
                break;
            case DebugCategory.ProbeSystem:
                enableProbeSystem = !enableProbeSystem;
                break;
            case DebugCategory.FacilitySystem:
                enableFacilitySystem = !enableFacilitySystem;
                break;
            case DebugCategory.TradeSystem:
                enableTradeSystem = !enableTradeSystem;
                break;
            case DebugCategory.GeneralGameplay:
                enableGeneralGameplay = !enableGeneralGameplay;
                break;
        }
        
        Debug.Log($"DebugManager: {category} debug logging {(IsCategoryEnabled(category) ? "enabled" : "disabled")}");
    }
    
    #region Context Menu Controls
    
    [ContextMenu("Enable All Debug Logs")]
    private void EnableAllDebugLogs()
    {
        SetAllCategories(true);
        enableAllDebugLogs = true;
        Debug.Log("DebugManager: All debug logging enabled");
    }
    
    [ContextMenu("Disable All Debug Logs")]
    private void DisableAllDebugLogs()
    {
        SetAllCategories(false);
        enableAllDebugLogs = false;
        Debug.Log("DebugManager: All debug logging disabled");
    }
    
    [ContextMenu("Errors Only")]
    private void ErrorsOnly()
    {
        SetAllCategories(false);
        enableAllDebugLogs = false;
        enableErrorLogsOnly = true;
        enableWarningsAndErrors = false;
        Debug.Log("DebugManager: Showing errors only");
    }
    
    [ContextMenu("Errors and Warnings")]
    private void ErrorsAndWarnings()
    {
        SetAllCategories(false);
        enableAllDebugLogs = false;
        enableErrorLogsOnly = false;
        enableWarningsAndErrors = true;
        Debug.Log("DebugManager: Showing errors and warnings");
    }
    
    [ContextMenu("Test Debug System")]
    private void TestDebugSystem()
    {
        Debug.Log("DebugManager: Testing debug system...");
        
        // Test each category
        DebugManager.Log("Testing WasteGeneration logs", DebugCategory.WasteGeneration);
        DebugManager.Log("Testing LocationSystem logs", DebugCategory.LocationSystem);
        DebugManager.LogWarning("Testing warning message", DebugCategory.GeneralGameplay);
        DebugManager.LogError("Testing error message", DebugCategory.GeneralGameplay);
        
        Debug.Log("DebugManager: Test complete. Check console for categorized messages.");
    }
    
    #endregion
}

/// <summary>
/// Categories for organizing debug messages
/// </summary>
public enum DebugCategory
{
    WasteGeneration,
    WasteProcessing,
    LocationSystem,
    InventoryManagement,
    ResourceSystem,
    UIDebug,
    ProbeSystem,
    FacilitySystem,
    TradeSystem,
    GeneralGameplay
} 