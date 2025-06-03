// Assets/Scripts/Core/Managers/ResourceProcessingManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Manages the conversion of waste items to resources
/// Handles processing queues, facility management, and yield calculations
/// </summary>
public class ResourceProcessingManager : MonoBehaviour
{
    public static ResourceProcessingManager Instance { get; private set; }
    
    [Header("Processing Settings")]
    [SerializeField] private int maxProcessingSlots = 5;
    [SerializeField] private float baseProcessingSpeed = 1f;
    [SerializeField] private bool autoProcessWaste = true;
    [SerializeField] private float processingCheckInterval = 1f;
    
    [Header("Facility Management")]
    [SerializeField] private int facilityLevel = 1;
    [SerializeField] private float facilityEfficiency = 1f;
    [SerializeField] private List<ProcessingFacility> availableFacilities = new List<ProcessingFacility>();
    
    [Header("Processing Queue")]
    [SerializeField] private List<ProcessingJob> activeJobs = new List<ProcessingJob>();
    [SerializeField] private Queue<ProcessingRequest> pendingRequests = new Queue<ProcessingRequest>();
    
    // Component references
    private NewResourceManager resourceManager;
    private ResourceConfigManager configManager;
    private WasteInventoryManager wasteInventory;
    private ResourceYieldCalculator yieldCalculator;
    
    // Processing monitoring
    private Coroutine processingCoroutine;
    private float totalProcessingTime = 0f;
    private int totalItemsProcessed = 0;
    private Dictionary<ResourceType, int> totalResourcesGenerated = new Dictionary<ResourceType, int>();
    
    // Events
    public event Action<ProcessingJob> OnProcessingStarted;
    public event Action<ProcessingJob, Dictionary<ResourceType, int>> OnProcessingCompleted;
    public event Action<ProcessingJob, string> OnProcessingFailed;
    public event Action<int> OnQueueChanged;
    public event Action<float> OnEfficiencyChanged;
    
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
        InitializeComponents();
        InitializeProcessing();
    }
    
    private void InitializeComponents()
    {
        resourceManager = NewResourceManager.Instance;
        configManager = ResourceConfigManager.Instance;
        wasteInventory = WasteInventoryManager.Instance;
        yieldCalculator = FindObjectOfType<ResourceYieldCalculator>();
        
        if (yieldCalculator == null)
        {
            var calculatorGO = new GameObject("ResourceYieldCalculator");
            yieldCalculator = calculatorGO.AddComponent<ResourceYieldCalculator>();
        }
    }
    
    private void InitializeProcessing()
    {
        // Initialize processing facilities if none exist
        if (availableFacilities.Count == 0)
        {
            CreateDefaultFacilities();
        }
        
        // Start processing coroutine
        if (autoProcessWaste)
        {
            StartProcessing();
        }
    }
    
    private void CreateDefaultFacilities()
    {
        // Basic recycling facility
        var basicFacility = new ProcessingFacility
        {
            facilityName = "Basic Recycling Unit",
            facilityType = ProcessingType.Recycling,
            level = 1,
            efficiency = 1f,
            maxConcurrentJobs = 2,
            supportedWasteTypes = new List<WasteType> { WasteType.Plastic, WasteType.Metal, WasteType.Organic }
        };
        availableFacilities.Add(basicFacility);
        
        // Advanced processing facility
        var advancedFacility = new ProcessingFacility
        {
            facilityName = "Advanced Processing Unit",
            facilityType = ProcessingType.Breakdown,
            level = 2,
            efficiency = 1.2f,
            maxConcurrentJobs = 1,
            supportedWasteTypes = new List<WasteType> { WasteType.Electronic, WasteType.Toxic }
        };
        availableFacilities.Add(advancedFacility);
    }
    
    /// <summary>
    /// Start the processing system
    /// </summary>
    public void StartProcessing()
    {
        if (processingCoroutine == null)
        {
            processingCoroutine = StartCoroutine(ProcessingLoop());
            Debug.Log("Resource processing started");
        }
    }
    
    /// <summary>
    /// Stop the processing system
    /// </summary>
    public void StopProcessing()
    {
        if (processingCoroutine != null)
        {
            StopCoroutine(processingCoroutine);
            processingCoroutine = null;
            Debug.Log("Resource processing stopped");
        }
    }
    
    /// <summary>
    /// Process a specific waste item
    /// </summary>
    /// <param name="wasteItem">Waste item to process</param>
    /// <param name="quantity">Quantity to process</param>
    /// <param name="processingType">Type of processing to apply</param>
    /// <returns>True if processing was queued successfully</returns>
    public bool ProcessWasteItem(WasteItem wasteItem, int quantity = 1, ProcessingType processingType = ProcessingType.Recycling)
    {
        if (wasteItem == null || quantity <= 0)
        {
            Debug.LogWarning("Invalid waste item or quantity for processing");
            return false;
        }
        
        // Check if we have available processing capacity
        if (GetAvailableProcessingSlots() <= 0)
        {
            // Queue the request
            var request = new ProcessingRequest
            {
                wasteItem = wasteItem,
                quantity = quantity,
                processingType = processingType,
                requestTime = Time.time
            };
            
            pendingRequests.Enqueue(request);
            OnQueueChanged?.Invoke(pendingRequests.Count);
            return true;
        }
        
        return StartProcessingJob(wasteItem, quantity, processingType);
    }
    
    /// <summary>
    /// Process multiple waste items
    /// </summary>
    /// <param name="wasteItems">List of waste items to process</param>
    /// <param name="processingType">Type of processing to apply</param>
    /// <returns>Number of items successfully queued</returns>
    public int ProcessWasteItems(List<WasteItem> wasteItems, ProcessingType processingType = ProcessingType.Recycling)
    {
        int successCount = 0;
        
        foreach (var item in wasteItems)
        {
            if (ProcessWasteItem(item, item.Quantity, processingType))
            {
                successCount++;
            }
        }
        
        return successCount;
    }
    
    /// <summary>
    /// Process all waste in inventory
    /// </summary>
    /// <param name="processingType">Type of processing to apply</param>
    /// <returns>Number of items queued for processing</returns>
    public int ProcessAllWaste(ProcessingType processingType = ProcessingType.Recycling)
    {
        if (wasteInventory == null)
            return 0;
        
        var allWaste = wasteInventory.GetAllWaste();
        return ProcessWasteItems(allWaste, processingType);
    }
    
    private bool StartProcessingJob(WasteItem wasteItem, int quantity, ProcessingType processingType)
    {
        // Find suitable facility
        var facility = FindSuitableFacility(wasteItem, processingType);
        if (facility == null)
        {
            Debug.LogWarning($"No suitable facility found for processing {wasteItem.Name}");
            return false;
        }
        
        // Calculate processing time and yield
        float processingTime = CalculateProcessingTime(wasteItem, quantity, facility);
        var expectedYield = yieldCalculator.CalculateFinalYield(wasteItem, quantity, facility.efficiency);
        
        // Create processing job
        var job = new ProcessingJob
        {
            jobId = System.Guid.NewGuid().ToString(),
            wasteItem = wasteItem,
            quantity = quantity,
            processingType = processingType,
            facility = facility,
            startTime = Time.time,
            processingDuration = processingTime,
            expectedYield = expectedYield,
            status = ProcessingStatus.InProgress
        };
        
        activeJobs.Add(job);
        OnProcessingStarted?.Invoke(job);
        
        Debug.Log($"Started processing {quantity}x {wasteItem.Name} (Expected completion: {processingTime:F1}s)");
        return true;
    }
    
    private ProcessingFacility FindSuitableFacility(WasteItem wasteItem, ProcessingType processingType)
    {
        return availableFacilities
            .Where(f => f.facilityType == processingType || f.facilityType == ProcessingType.Universal)
            .Where(f => f.supportedWasteTypes.Contains(wasteItem.WasteType) || f.supportedWasteTypes.Contains(WasteType.Universal))
            .Where(f => GetFacilityActiveJobs(f) < f.maxConcurrentJobs)
            .OrderByDescending(f => f.efficiency)
            .FirstOrDefault();
    }
    
    private int GetFacilityActiveJobs(ProcessingFacility facility)
    {
        return activeJobs.Count(j => j.facility == facility && j.status == ProcessingStatus.InProgress);
    }
    
    private float CalculateProcessingTime(WasteItem wasteItem, int quantity, ProcessingFacility facility)
    {
        float baseTime = wasteItem.ProcessingTime * quantity;
        float facilityModifier = 1f / facility.efficiency;
        float speedModifier = 1f / baseProcessingSpeed;
        
        return baseTime * facilityModifier * speedModifier;
    }
    
    private IEnumerator ProcessingLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(processingCheckInterval);
            
            // Process pending requests if slots are available
            ProcessPendingRequests();
            
            // Check for completed jobs
            CheckCompletedJobs();
            
            // Update facility efficiency
            UpdateFacilityEfficiency();
        }
    }
    
    private void ProcessPendingRequests()
    {
        while (pendingRequests.Count > 0 && GetAvailableProcessingSlots() > 0)
        {
            var request = pendingRequests.Dequeue();
            StartProcessingJob(request.wasteItem, request.quantity, request.processingType);
            OnQueueChanged?.Invoke(pendingRequests.Count);
        }
    }
    
    private void CheckCompletedJobs()
    {
        var completedJobs = activeJobs.Where(j => 
            j.status == ProcessingStatus.InProgress && 
            Time.time >= j.startTime + j.processingDuration).ToList();
        
        foreach (var job in completedJobs)
        {
            CompleteProcessingJob(job);
        }
    }
    
    private void CompleteProcessingJob(ProcessingJob job)
    {
        try
        {
            // Calculate actual yield (may vary from expected)
            var actualYield = yieldCalculator.CalculateFinalYield(
                job.wasteItem, 
                job.quantity, 
                job.facility.efficiency,
                UnityEngine.Random.Range(0.9f, 1.1f) // Small random variation
            );
            
            // Add resources to inventory
            foreach (var resource in actualYield)
            {
                resourceManager.AddResource(resource.Key, resource.Value);
                
                // Track statistics
                if (!totalResourcesGenerated.ContainsKey(resource.Key))
                    totalResourcesGenerated[resource.Key] = 0;
                totalResourcesGenerated[resource.Key] += resource.Value;
            }
            
            // Remove waste from inventory
            if (wasteInventory != null)
            {
                wasteInventory.RemoveWaste(job.wasteItem.Id, job.quantity);
            }
            
            // Update statistics
            totalItemsProcessed += job.quantity;
            totalProcessingTime += job.processingDuration;
            
            // Update job status
            job.status = ProcessingStatus.Completed;
            job.actualYield = actualYield;
            job.completionTime = Time.time;
            
            OnProcessingCompleted?.Invoke(job, actualYield);
            
            Debug.Log($"Completed processing {job.quantity}x {job.wasteItem.Name}. " +
                     $"Yield: {string.Join(", ", actualYield.Select(kv => $"{kv.Value} {kv.Key}"))}");
        }
        catch (Exception ex)
        {
            job.status = ProcessingStatus.Failed;
            job.errorMessage = ex.Message;
            OnProcessingFailed?.Invoke(job, ex.Message);
            Debug.LogError($"Processing job failed: {ex.Message}");
        }
        finally
        {
            activeJobs.Remove(job);
        }
    }
    
    private void UpdateFacilityEfficiency()
    {
        // Calculate overall facility efficiency based on usage
        float totalEfficiency = 0f;
        int activeFacilities = 0;
        
        foreach (var facility in availableFacilities)
        {
            int activeJobs = GetFacilityActiveJobs(facility);
            if (activeJobs > 0)
            {
                totalEfficiency += facility.efficiency;
                activeFacilities++;
            }
        }
        
        float newEfficiency = activeFacilities > 0 ? totalEfficiency / activeFacilities : 1f;
        
        if (Mathf.Abs(newEfficiency - facilityEfficiency) > 0.01f)
        {
            facilityEfficiency = newEfficiency;
            OnEfficiencyChanged?.Invoke(facilityEfficiency);
        }
    }
    
    #region Public API
    
    /// <summary>
    /// Get the number of available processing slots
    /// </summary>
    /// <returns>Number of available slots</returns>
    public int GetAvailableProcessingSlots()
    {
        return maxProcessingSlots - activeJobs.Count(j => j.status == ProcessingStatus.InProgress);
    }
    
    /// <summary>
    /// Get all active processing jobs
    /// </summary>
    /// <returns>List of active jobs</returns>
    public List<ProcessingJob> GetActiveJobs()
    {
        return new List<ProcessingJob>(activeJobs);
    }
    
    /// <summary>
    /// Get the number of pending requests
    /// </summary>
    /// <returns>Number of pending requests</returns>
    public int GetPendingRequestCount()
    {
        return pendingRequests.Count;
    }
    
    /// <summary>
    /// Cancel a specific processing job
    /// </summary>
    /// <param name="jobId">ID of the job to cancel</param>
    /// <returns>True if job was cancelled</returns>
    public bool CancelProcessingJob(string jobId)
    {
        var job = activeJobs.FirstOrDefault(j => j.jobId == jobId);
        if (job != null)
        {
            job.status = ProcessingStatus.Cancelled;
            activeJobs.Remove(job);
            Debug.Log($"Cancelled processing job: {jobId}");
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Cancel all processing jobs
    /// </summary>
    /// <returns>Number of jobs cancelled</returns>
    public int CancelAllJobs()
    {
        int cancelledCount = activeJobs.Count;
        activeJobs.Clear();
        pendingRequests.Clear();
        OnQueueChanged?.Invoke(0);
        Debug.Log($"Cancelled {cancelledCount} processing jobs");
        return cancelledCount;
    }
    
    /// <summary>
    /// Upgrade a processing facility
    /// </summary>
    /// <param name="facilityIndex">Index of facility to upgrade</param>
    /// <returns>True if upgrade was successful</returns>
    public bool UpgradeFacility(int facilityIndex)
    {
        if (facilityIndex < 0 || facilityIndex >= availableFacilities.Count)
            return false;
        
        var facility = availableFacilities[facilityIndex];
        facility.level++;
        facility.efficiency *= 1.1f; // 10% efficiency increase per level
        facility.maxConcurrentJobs = Mathf.Min(facility.maxConcurrentJobs + 1, 5);
        
        Debug.Log($"Upgraded {facility.facilityName} to level {facility.level}");
        return true;
    }
    
    /// <summary>
    /// Get processing statistics
    /// </summary>
    /// <returns>Processing statistics</returns>
    public ProcessingStatistics GetStatistics()
    {
        return new ProcessingStatistics
        {
            totalItemsProcessed = totalItemsProcessed,
            totalProcessingTime = totalProcessingTime,
            averageProcessingTime = totalItemsProcessed > 0 ? totalProcessingTime / totalItemsProcessed : 0f,
            totalResourcesGenerated = new Dictionary<ResourceType, int>(totalResourcesGenerated),
            currentEfficiency = facilityEfficiency,
            activeJobs = activeJobs.Count,
            pendingRequests = pendingRequests.Count,
            availableSlots = GetAvailableProcessingSlots()
        };
    }
    
    #endregion
    
    private void OnDestroy()
    {
        StopProcessing();
    }
}

/// <summary>
/// Processing facility definition
/// </summary>
[System.Serializable]
public class ProcessingFacility
{
    public string facilityName;
    public ProcessingType facilityType;
    public int level = 1;
    public float efficiency = 1f;
    public int maxConcurrentJobs = 1;
    public List<WasteType> supportedWasteTypes = new List<WasteType>();
    public float energyConsumption = 1f;
    public bool isOperational = true;
}

/// <summary>
/// Processing job data
/// </summary>
[System.Serializable]
public class ProcessingJob
{
    public string jobId;
    public WasteItem wasteItem;
    public int quantity;
    public ProcessingType processingType;
    public ProcessingFacility facility;
    public float startTime;
    public float processingDuration;
    public float completionTime;
    public ProcessingStatus status;
    public Dictionary<ResourceType, int> expectedYield;
    public Dictionary<ResourceType, int> actualYield;
    public string errorMessage;
    
    public float Progress => status == ProcessingStatus.InProgress ? 
        Mathf.Clamp01((Time.time - startTime) / processingDuration) : 
        (status == ProcessingStatus.Completed ? 1f : 0f);
}

/// <summary>
/// Processing request for queuing
/// </summary>
[System.Serializable]
public class ProcessingRequest
{
    public WasteItem wasteItem;
    public int quantity;
    public ProcessingType processingType;
    public float requestTime;
}

/// <summary>
/// Processing status enumeration
/// </summary>
public enum ProcessingStatus
{
    Queued,
    InProgress,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Processing statistics
/// </summary>
[System.Serializable]
public class ProcessingStatistics
{
    public int totalItemsProcessed;
    public float totalProcessingTime;
    public float averageProcessingTime;
    public Dictionary<ResourceType, int> totalResourcesGenerated;
    public float currentEfficiency;
    public int activeJobs;
    public int pendingRequests;
    public int availableSlots;
    
    public override string ToString()
    {
        return $"Processing Stats:\n" +
               $"Items Processed: {totalItemsProcessed}\n" +
               $"Avg Processing Time: {averageProcessingTime:F1}s\n" +
               $"Current Efficiency: {currentEfficiency:P1}\n" +
               $"Active Jobs: {activeJobs}\n" +
               $"Available Slots: {availableSlots}";
    }
}