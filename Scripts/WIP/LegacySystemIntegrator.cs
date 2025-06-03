using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Manages the integration and transition from legacy systems to new resource management
/// Handles data migration, system coordination, and backward compatibility
/// </summary>
public class LegacySystemIntegrator : MonoBehaviour
{
    public static LegacySystemIntegrator Instance { get; private set; }
    
    [Header("Integration Settings")]
    [SerializeField] private bool autoMigrateOnStart = true;
    [SerializeField] private bool enableProgressiveTransition = true;
    [SerializeField] private float migrationBatchSize = 50f;
    
    [Header("System Status")]
    [SerializeField] private IntegrationPhase currentPhase = IntegrationPhase.NotStarted;
    [SerializeField] private float migrationProgress = 0f;
    [SerializeField] private bool legacySystemsActive = true;
    [SerializeField] private bool newSystemsActive = false;
    
    [Header("Migration Statistics")]
    [SerializeField] private int totalItemsToMigrate = 0;
    [SerializeField] private int itemsMigrated = 0;
    [SerializeField] private int migrationErrors = 0;
    
    // Events for tracking migration progress
    public event Action<IntegrationPhase> OnPhaseChanged;
    public event Action<float> OnMigrationProgress;
    public event Action OnMigrationComplete;
    public event Action<string> OnMigrationError;
    
    // System references
    private ResourceManager legacyResourceManager;
    private WasteInventoryManager legacyInventoryManager;
    private NewResourceManager newResourceManager;
    private ResourceManagerBridge bridge;
    
    // Migration data
    private List<WasteItem> pendingMigrationItems = new List<WasteItem>();
    private Dictionary<string, UpdatedWasteItem> migratedItems = new Dictionary<string, UpdatedWasteItem>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSystemReferences();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (autoMigrateOnStart)
        {
            StartCoroutine(BeginMigrationProcess());
        }
    }
    
    private void InitializeSystemReferences()
    {
        // Find existing system references
        legacyResourceManager = ResourceManager.Instance;
        legacyInventoryManager = WasteInventoryManager.Instance;
        newResourceManager = NewResourceManager.Instance;
        bridge = ResourceManagerBridge.Instance;
        
        if (bridge == null)
        {
            // Create bridge if it doesn't exist
            var bridgeGO = new GameObject("ResourceManagerBridge");
            bridge = bridgeGO.AddComponent<ResourceManagerBridge>();
        }
    }
    
    /// <summary>
    /// Begin the complete migration process
    /// </summary>
    public IEnumerator BeginMigrationProcess()
    {
        Debug.Log("Starting legacy system integration...");
        
        yield return StartCoroutine(ExecutePhase(IntegrationPhase.PreMigration));
        yield return StartCoroutine(ExecutePhase(IntegrationPhase.DataAnalysis));
        yield return StartCoroutine(ExecutePhase(IntegrationPhase.ResourceMigration));
        yield return StartCoroutine(ExecutePhase(IntegrationPhase.InventoryMigration));
        yield return StartCoroutine(ExecutePhase(IntegrationPhase.SystemValidation));
        yield return StartCoroutine(ExecutePhase(IntegrationPhase.PostMigration));
        
        SetPhase(IntegrationPhase.Complete);
        OnMigrationComplete?.Invoke();
        
        Debug.Log("Legacy system integration complete!");
    }
    
    private IEnumerator ExecutePhase(IntegrationPhase phase)
    {
        SetPhase(phase);
        
        switch (phase)
        {
            case IntegrationPhase.PreMigration:
                yield return StartCoroutine(PreMigrationPhase());
                break;
            case IntegrationPhase.DataAnalysis:
                yield return StartCoroutine(DataAnalysisPhase());
                break;
            case IntegrationPhase.ResourceMigration:
                yield return StartCoroutine(ResourceMigrationPhase());
                break;
            case IntegrationPhase.InventoryMigration:
                yield return StartCoroutine(InventoryMigrationPhase());
                break;
            case IntegrationPhase.SystemValidation:
                yield return StartCoroutine(SystemValidationPhase());
                break;
            case IntegrationPhase.PostMigration:
                yield return StartCoroutine(PostMigrationPhase());
                break;
        }
    }
    
    private IEnumerator PreMigrationPhase()
    {
        Debug.Log("Phase 1: Pre-migration setup");
        
        // Backup existing data
        yield return StartCoroutine(BackupLegacyData());
        
        // Initialize new systems
        if (newResourceManager == null)
        {
            Debug.LogError("NewResourceManager not found! Creating instance...");
            var newManagerGO = new GameObject("NewResourceManager");
            newResourceManager = newManagerGO.AddComponent<NewResourceManager>();
        }
        
        // Validate system integrity
        bool systemsReady = ValidateSystemReadiness();
        if (!systemsReady)
        {
            OnMigrationError?.Invoke("Systems not ready for migration");
            yield break;
        }
        
        UpdateProgress(0.1f);
        yield return new WaitForSeconds(0.5f);
    }
    
    private IEnumerator DataAnalysisPhase()
    {
        Debug.Log("Phase 2: Analyzing legacy data");
        
        // Analyze legacy resources
        if (legacyResourceManager != null)
        {
            float legacyRP = legacyResourceManager.GetRecyclingPoints();
            float legacyDP = legacyResourceManager.GetDimensionalPotential();
            
            Debug.Log($"Legacy Resources: {legacyRP} RP, {legacyDP} DP");
        }
        
        // Analyze legacy inventory
        if (legacyInventoryManager != null)
        {
            var legacyItems = legacyInventoryManager.GetAllWaste();
            totalItemsToMigrate = legacyItems.Count;
            pendingMigrationItems.AddRange(legacyItems);
            
            var stats = WasteItemConverter.GetConversionStats(legacyItems);
            Debug.Log($"Inventory Analysis: {stats}");
        }
        
        UpdateProgress(0.2f);
        yield return new WaitForSeconds(0.3f);
    }
    
    private IEnumerator ResourceMigrationPhase()
    {
        Debug.Log("Phase 3: Migrating legacy resources");
        
        if (legacyResourceManager != null && bridge != null)
        {
            // Migrate recycling points
            float legacyRP = legacyResourceManager.GetRecyclingPoints();
            if (legacyRP > 0)
            {
                bridge.AddRecyclingPoints(legacyRP);
                Debug.Log($"Migrated {legacyRP} recycling points");
            }
            
            // Migrate dimensional potential
            float legacyDP = legacyResourceManager.GetDimensionalPotential();
            if (legacyDP > 0)
            {
                bridge.AddDimensionalPotential(legacyDP);
                Debug.Log($"Migrated {legacyDP} dimensional potential");
            }
            
            // Migrate combat resources
            int shipParts = legacyResourceManager.ShipParts;
            int alienTech = legacyResourceManager.AlienTech;
            int combatData = legacyResourceManager.CombatData;
            
            if (shipParts > 0) bridge.AddShipParts(shipParts);
            if (alienTech > 0) bridge.AddAlienTech(alienTech);
            if (combatData > 0) bridge.AddCombatData(combatData);
            
            Debug.Log($"Migrated combat resources: {shipParts} parts, {alienTech} tech, {combatData} data");
        }
        
        UpdateProgress(0.4f);
        yield return new WaitForSeconds(0.5f);
    }
    
    private IEnumerator InventoryMigrationPhase()
    {
        Debug.Log("Phase 4: Migrating inventory items");
        
        int batchSize = Mathf.RoundToInt(migrationBatchSize);
        int processed = 0;
        
        for (int i = 0; i < pendingMigrationItems.Count; i += batchSize)
        {
            int endIndex = Mathf.Min(i + batchSize, pendingMigrationItems.Count);
            
            for (int j = i; j < endIndex; j++)
            {
                try
                {
                    var legacyItem = pendingMigrationItems[j];
                    var convertedItem = WasteItemConverter.ConvertToUpdatedWasteItem(legacyItem);
                    
                    if (convertedItem != null)
                    {
                        migratedItems[legacyItem.Id] = convertedItem;
                        itemsMigrated++;
                    }
                    else
                    {
                        migrationErrors++;
                        OnMigrationError?.Invoke($"Failed to convert item: {legacyItem.Name}");
                    }
                }
                catch (Exception ex)
                {
                    migrationErrors++;
                    OnMigrationError?.Invoke($"Migration error: {ex.Message}");
                }
                
                processed++;
            }
            
            // Update progress
            float progress = 0.4f + (0.4f * processed / totalItemsToMigrate);
            UpdateProgress(progress);
            
            // Yield control to prevent frame drops
            yield return null;
        }
        
        Debug.Log($"Inventory migration complete: {itemsMigrated} items migrated, {migrationErrors} errors");
    }
    
    private IEnumerator SystemValidationPhase()
    {
        Debug.Log("Phase 5: Validating migrated systems");
        
        bool validationPassed = true;
        
        // Validate resource totals
        if (legacyResourceManager != null && newResourceManager != null)
        {
            float legacyTotal = CalculateLegacyResourceValue();
            float newTotal = CalculateNewResourceValue();
            
            float difference = Mathf.Abs(legacyTotal - newTotal);
            float tolerance = legacyTotal * 0.1f; // 10% tolerance
            
            if (difference > tolerance)
            {
                validationPassed = false;
                OnMigrationError?.Invoke($"Resource value mismatch: Legacy={legacyTotal}, New={newTotal}");
            }
        }
        
        // Validate item counts
        if (totalItemsToMigrate > 0)
        {
            float successRate = (float)itemsMigrated / totalItemsToMigrate;
            if (successRate < 0.95f) // 95% success rate required
            {
                validationPassed = false;
                OnMigrationError?.Invoke($"Low migration success rate: {successRate:P1}");
            }
        }
        
        // Test system integration
        yield return StartCoroutine(TestSystemIntegration());
        
        UpdateProgress(0.9f);
        
        if (!validationPassed)
        {
            Debug.LogWarning("System validation completed with warnings");
        }
        else
        {
            Debug.Log("System validation passed");
        }
        
        yield return new WaitForSeconds(0.3f);
    }
    
    private IEnumerator PostMigrationPhase()
    {
        Debug.Log("Phase 6: Post-migration cleanup");
        
        // Enable new systems
        newSystemsActive = true;
        
        // Optionally disable legacy systems
        if (!enableProgressiveTransition)
        {
            legacySystemsActive = false;
            DisableLegacySystems();
        }
        
        // Clean up temporary data
        pendingMigrationItems.Clear();
        
        // Save migration report
        SaveMigrationReport();
        
        UpdateProgress(1.0f);
        yield return new WaitForSeconds(0.2f);
    }
    
    #region Helper Methods
    
    private void SetPhase(IntegrationPhase phase)
    {
        currentPhase = phase;
        OnPhaseChanged?.Invoke(phase);
        Debug.Log($"Integration Phase: {phase}");
    }
    
    private void UpdateProgress(float progress)
    {
        migrationProgress = progress;
        OnMigrationProgress?.Invoke(progress);
    }
    
    private bool ValidateSystemReadiness()
    {
        if (newResourceManager == null)
        {
            Debug.LogError("NewResourceManager not available");
            return false;
        }
        
        if (bridge == null)
        {
            Debug.LogError("ResourceManagerBridge not available");
            return false;
        }
        
        return true;
    }
    
    private IEnumerator BackupLegacyData()
    {
        Debug.Log("Creating backup of legacy data...");
        
        // In a real implementation, you'd save this data to persistent storage
        // For now, we'll just log the backup creation
        
        if (legacyResourceManager != null)
        {
            PlayerPrefs.SetFloat("Backup_RecyclingPoints", legacyResourceManager.GetRecyclingPoints());
            PlayerPrefs.SetFloat("Backup_DimensionalPotential", legacyResourceManager.GetDimensionalPotential());
        }
        
        if (legacyInventoryManager != null)
        {
            PlayerPrefs.SetInt("Backup_InventoryCount", legacyInventoryManager.GetAllWaste().Count);
        }
        
        PlayerPrefs.Save();
        
        yield return new WaitForSeconds(0.1f);
        Debug.Log("Legacy data backup complete");
    }
    
    private float CalculateLegacyResourceValue()
    {
        if (legacyResourceManager == null) return 0f;
        
        float total = 0f;
        total += legacyResourceManager.GetRecyclingPoints();
        total += legacyResourceManager.GetDimensionalPotential() * 2f; // DP worth more
        total += legacyResourceManager.ShipParts * 5f;
        total += legacyResourceManager.AlienTech * 10f;
        total += legacyResourceManager.CombatData * 3f;
        
        return total;
    }
    
    private float CalculateNewResourceValue()
    {
        if (newResourceManager == null) return 0f;
        
        float total = 0f;
        
        // Calculate based on resource configs
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            int amount = newResourceManager.GetResourceAmount(type);
            var config = newResourceManager.GetResourceConfig(type);
            int baseValue = config?.baseValue ?? 1;
            total += amount * baseValue;
        }
        
        return total;
    }
    
    private IEnumerator TestSystemIntegration()
    {
        Debug.Log("Testing system integration...");
        
        // Test resource operations through bridge
        if (bridge != null)
        {
            float testAmount = 10f;
            
            // Test adding resources
            bridge.AddRecyclingPoints(testAmount);
            bridge.AddDimensionalPotential(testAmount);
            
            yield return new WaitForSeconds(0.1f);
            
            // Test spending resources
            bool spendSuccess = bridge.SpendRecyclingPoints(testAmount);
            bool spendDPSuccess = bridge.SpendDimensionalPotential(testAmount);
            
            if (!spendSuccess || !spendDPSuccess)
            {
                OnMigrationError?.Invoke("Bridge resource operations failed");
            }
        }
        
        yield return new WaitForSeconds(0.2f);
        Debug.Log("System integration test complete");
    }
    
    private void DisableLegacySystems()
    {
        Debug.Log("Disabling legacy systems...");
        
        if (legacyResourceManager != null)
        {
            legacyResourceManager.enabled = false;
        }
        
        if (legacyInventoryManager != null)
        {
            legacyInventoryManager.enabled = false;
        }
    }
    
    private void SaveMigrationReport()
    {
        var report = new MigrationReport
        {
            timestamp = DateTime.Now,
            totalItemsToMigrate = totalItemsToMigrate,
            itemsMigrated = itemsMigrated,
            migrationErrors = migrationErrors,
            finalPhase = currentPhase,
            legacySystemsActive = legacySystemsActive,
            newSystemsActive = newSystemsActive
        };
        
        string reportJson = JsonUtility.ToJson(report, true);
        Debug.Log($"Migration Report:\n{reportJson}");
        
        // Save to PlayerPrefs for persistence
        PlayerPrefs.SetString("LastMigrationReport", reportJson);
        PlayerPrefs.Save();
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Manually start the migration process
    /// </summary>
    public void StartMigration()
    {
        if (currentPhase == IntegrationPhase.NotStarted || currentPhase == IntegrationPhase.Complete)
        {
            StartCoroutine(BeginMigrationProcess());
        }
    }
    
    /// <summary>
    /// Get current migration status
    /// </summary>
    public MigrationStatus GetMigrationStatus()
    {
        return new MigrationStatus
        {
            phase = currentPhase,
            progress = migrationProgress,
            itemsMigrated = itemsMigrated,
            totalItems = totalItemsToMigrate,
            errors = migrationErrors,
            isComplete = currentPhase == IntegrationPhase.Complete
        };
    }
    
    /// <summary>
    /// Force enable/disable legacy systems
    /// </summary>
    public void SetLegacySystemsActive(bool active)
    {
        legacySystemsActive = active;
        
        if (legacyResourceManager != null)
            legacyResourceManager.enabled = active;
        
        if (legacyInventoryManager != null)
            legacyInventoryManager.enabled = active;
    }
    
    /// <summary>
    /// Get a migrated item by its legacy ID
    /// </summary>
    public UpdatedWasteItem GetMigratedItem(string legacyId)
    {
        migratedItems.TryGetValue(legacyId, out UpdatedWasteItem item);
        return item;
    }
    
    #endregion
}

/// <summary>
/// Integration phases
/// </summary>
public enum IntegrationPhase
{
    NotStarted,
    PreMigration,
    DataAnalysis,
    ResourceMigration,
    InventoryMigration,
    SystemValidation,
    PostMigration,
    Complete
}

/// <summary>
/// Migration status data
/// </summary>
[System.Serializable]
public class MigrationStatus
{
    public IntegrationPhase phase;
    public float progress;
    public int itemsMigrated;
    public int totalItems;
    public int errors;
    public bool isComplete;
}

/// <summary>
/// Migration report data
/// </summary>
[System.Serializable]
public class MigrationReport
{
    public DateTime timestamp;
    public int totalItemsToMigrate;
    public int itemsMigrated;
    public int migrationErrors;
    public IntegrationPhase finalPhase;
    public bool legacySystemsActive;
    public bool newSystemsActive;
} 