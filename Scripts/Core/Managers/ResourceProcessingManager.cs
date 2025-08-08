// Assets/Scripts/Core/Managers/ResourceProcessingManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Manages the conversion of waste items to resources
/// Handles processing queues, facility management, and yield calculations
/// Updated to use UpdatedWasteItem and enhanced resource system
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
    
    [Header("Recipe Processing")]
    [SerializeField] private List<ProcessingRecipeData> availableRecipes = new List<ProcessingRecipeData>();
    [SerializeField] private bool enableRecipeProcessing = true;
    
    [Header("Processing Queue")]
    [SerializeField] private List<ProcessingJob> activeJobs = new List<ProcessingJob>();
    [SerializeField] private Queue<ProcessingRequest> pendingRequests = new Queue<ProcessingRequest>();
    
    // Component references
    private ResourceManager resourceManager;
    private ResourceConfigManager configManager;
    private WasteInventoryManager wasteInventory;
    
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
    public event Action<ProcessingRecipeData, Dictionary<ResourceType, int>> OnRecipeProcessed;
    
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
        resourceManager = ResourceManager.Instance;
        configManager = ResourceConfigManager.Instance;
        wasteInventory = WasteInventoryManager.Instance;
        
        if (resourceManager == null)
        {
            Debug.LogError("ResourceManager.Instance is null! ResourceProcessingManager requires it.");
        }
    }
    
    private void InitializeProcessing()
    {
        // Initialize processing facilities if none exist
        if (availableFacilities.Count == 0)
        {
            CreateDefaultFacilities();
        }
        
        // Load available recipes from ResourceConfigManager
        LoadAvailableRecipes();
        
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
            supportedWasteTypes = new List<WasteType> { WasteType.Plastic, WasteType.Metals, WasteType.Organic },
            energyConsumption = 1f,
            isOperational = true
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
            supportedWasteTypes = new List<WasteType> { WasteType.Electronics, WasteType.Chemical, WasteType.Toxic },
            energyConsumption = 2f,
            isOperational = true
        };
        availableFacilities.Add(advancedFacility);
        
        // Specialized facilities
        var fuelSynthesizer = new ProcessingFacility
        {
            facilityName = "Fuel Synthesizer",
            facilityType = ProcessingType.Synthesis,
            level = 2,
            efficiency = 1.1f,
            maxConcurrentJobs = 1,
            supportedWasteTypes = new List<WasteType> { WasteType.Organic, WasteType.Chemical },
            energyConsumption = 1.5f,
            isOperational = true,
            specialization = "Fuel Production"
        };
        availableFacilities.Add(fuelSynthesizer);
        
        var metalFabricator = new ProcessingFacility
        {
            facilityName = "Metal Fabricator",
            facilityType = ProcessingType.Fabrication,
            level = 3,
            efficiency = 1.3f,
            maxConcurrentJobs = 1,
            supportedWasteTypes = new List<WasteType> { WasteType.Metals, WasteType.Electronics },
            energyConsumption = 2.5f,
            isOperational = true,
            specialization = "Metal Processing"
        };
        availableFacilities.Add(metalFabricator);
    }
    
    private void LoadAvailableRecipes()
    {
        if (configManager != null)
        {
            // Load recipes from ResourceConfigManager
            // This would be implemented when ResourceConfigManager has recipe loading
            Debug.Log("Loading processing recipes from ResourceConfigManager");
        }
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
    /// Process a specific waste item using direct breakdown
    /// </summary>
    /// <param name="wasteItem">UpdatedWasteItem to process</param>
    /// <param name="quantity">Quantity to process</param>
    /// <param name="processingType">Type of processing to apply</param>
    /// <returns>True if processing was queued successfully</returns>
    public bool ProcessWasteItem(UpdatedWasteItem wasteItem, int quantity = 1, ProcessingType processingType = ProcessingType.Recycling)
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
                requestTime = Time.time,
                isRecipeProcessing = false
            };
            
            pendingRequests.Enqueue(request);
            OnQueueChanged?.Invoke(pendingRequests.Count);
            return true;
        }
        
        return StartProcessingJob(wasteItem, quantity, processingType);
    }
    
    /// <summary>
    /// Process using a specific recipe
    /// </summary>
    /// <param name="recipe">Recipe to use for processing</param>
    /// <param name="facilityName">Specific facility to use (optional)</param>
    /// <returns>True if processing was queued successfully</returns>
    public bool ProcessRecipe(ProcessingRecipeData recipe, string facilityName = null)
    {
        if (recipe == null || !recipe.CanUse() || !recipe.CanAfford())
        {
            Debug.LogWarning("Cannot process recipe: not available or insufficient resources");
            return false;
        }
        
        // Find suitable facility
        var facility = FindSuitableFacilityForRecipe(recipe, facilityName);
        if (facility == null)
        {
            Debug.LogWarning($"No suitable facility found for recipe: {recipe.recipeName}");
            return false;
        }
        
        // Check if facility has capacity
        if (GetFacilityActiveJobs(facility) >= facility.maxConcurrentJobs)
        {
            // Queue the recipe request
            var request = new ProcessingRequest
            {
                recipe = recipe,
                processingType = ProcessingType.Recipe,
                requestTime = Time.time,
                isRecipeProcessing = true,
                preferredFacility = facilityName
            };
            
            pendingRequests.Enqueue(request);
            OnQueueChanged?.Invoke(pendingRequests.Count);
            return true;
        }
        
        return StartRecipeProcessingJob(recipe, facility);
    }
    
    /// <summary>
    /// Process multiple waste items
    /// </summary>
    /// <param name="wasteItems">List of UpdatedWasteItems to process</param>
    /// <param name="processingType">Type of processing to apply</param>
    /// <returns>Number of items successfully queued</returns>
    public int ProcessWasteItems(List<UpdatedWasteItem> wasteItems, ProcessingType processingType = ProcessingType.Recycling)
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
        if (wasteInventory == null) return 0;
        
        var allWaste = wasteInventory.GetAllSlots().Select(slot => slot.wasteItem).ToList();
        return ProcessWasteItems(allWaste, processingType);
    }
    
    private bool StartProcessingJob(UpdatedWasteItem wasteItem, int quantity, ProcessingType processingType)
    {
        // Find suitable facility
        var facility = FindSuitableFacility(wasteItem, processingType);
        if (facility == null)
        {
            Debug.LogWarning($"No suitable facility found for {wasteItem.Name}");
            return false;
        }
        
        // Calculate expected yield using ResourceYieldCalculator
        var expectedYield = ResourceYieldCalculator.CalculateYield(wasteItem, facility.efficiency);
        
        // Create processing job
        var job = new ProcessingJob
        {
            jobId = System.Guid.NewGuid().ToString("N")[..8],
            wasteItem = wasteItem,
            quantity = quantity,
            processingType = processingType,
            facility = facility,
            startTime = Time.time,
            processingDuration = CalculateProcessingTime(wasteItem, quantity, facility),
            status = ProcessingStatus.InProgress,
            expectedYield = expectedYield,
            isRecipeProcessing = false
        };
        
        job.completionTime = job.startTime + job.processingDuration;
        
        // Add to active jobs
        activeJobs.Add(job);
        
        // Trigger events
        OnProcessingStarted?.Invoke(job);
        
        Debug.Log($"Started processing {quantity}x {wasteItem.Name} in {facility.facilityName}");
        return true;
    }
    
    private bool StartRecipeProcessingJob(ProcessingRecipeData recipe, ProcessingFacility facility)
    {
        // Consume input resources
        if (!resourceManager.SpendResources(recipe.inputResources))
        {
            Debug.LogWarning($"Failed to consume resources for recipe: {recipe.recipeName}");
            return false;
        }
        
        // Create processing job for recipe
        var job = new ProcessingJob
        {
            jobId = System.Guid.NewGuid().ToString("N")[..8],
            recipe = recipe,
            processingType = ProcessingType.Recipe,
            facility = facility,
            startTime = Time.time,
            processingDuration = recipe.processingTime / facility.efficiency,
            status = ProcessingStatus.InProgress,
            isRecipeProcessing = true
        };
        
        job.completionTime = job.startTime + job.processingDuration;
        
        // Calculate expected yield from recipe
        var expectedYield = new Dictionary<ResourceType, int>();
        foreach (var output in recipe.outputResources)
        {
            expectedYield[output.type] = Mathf.RoundToInt(output.amount * facility.efficiency);
        }
        job.expectedYield = expectedYield;
        
        // Add to active jobs
        activeJobs.Add(job);
        
        // Trigger events
        OnProcessingStarted?.Invoke(job);
        
        Debug.Log($"Started recipe processing: {recipe.recipeName} in {facility.facilityName}");
        return true;
    }
    
    private ProcessingFacility FindSuitableFacility(UpdatedWasteItem wasteItem, ProcessingType processingType)
    {
        return availableFacilities
            .Where(f => f.isOperational && 
                       f.facilityType == processingType &&
                       f.supportedWasteTypes.Contains(wasteItem.Type) &&
                       GetFacilityActiveJobs(f) < f.maxConcurrentJobs)
            .OrderByDescending(f => f.efficiency)
            .FirstOrDefault();
    }
    
    private ProcessingFacility FindSuitableFacilityForRecipe(ProcessingRecipeData recipe, string preferredFacilityName = null)
    {
        var suitableFacilities = availableFacilities
            .Where(f => f.isOperational &&
                       f.level >= recipe.minimumFacilityLevel &&
                       (string.IsNullOrEmpty(recipe.requiredFacility) || f.facilityName.Contains(recipe.requiredFacility)) &&
                       GetFacilityActiveJobs(f) < f.maxConcurrentJobs);
        
        if (!string.IsNullOrEmpty(preferredFacilityName))
        {
            var preferred = suitableFacilities.FirstOrDefault(f => f.facilityName == preferredFacilityName);
            if (preferred != null) return preferred;
        }
        
        return suitableFacilities.OrderByDescending(f => f.efficiency).FirstOrDefault();
    }
    
    private int GetFacilityActiveJobs(ProcessingFacility facility)
    {
        return activeJobs.Count(job => job.facility == facility && job.status == ProcessingStatus.InProgress);
    }
    
    private float CalculateProcessingTime(UpdatedWasteItem wasteItem, int quantity, ProcessingFacility facility)
    {
        float baseTime = wasteItem.ProcessingTime * quantity;
        float facilityModifier = 1f / facility.efficiency;
        float complexityModifier = GetComplexityModifier(wasteItem);
        
        return baseTime * facilityModifier * complexityModifier * baseProcessingSpeed;
    }
    
    private float GetComplexityModifier(UpdatedWasteItem wasteItem)
    {
        // More complex items take longer to process
        float modifier = 1f;
        
        if (wasteItem.ContaminationLevel > 0.5f) modifier += 0.3f;
        if (wasteItem.Rarity == WasteRarity.Rare) modifier += 0.2f;
        if (wasteItem.Rarity == WasteRarity.Epic) modifier += 0.4f;
        if (wasteItem.Rarity == WasteRarity.Legendary) modifier += 0.6f;
        
        return modifier;
    }
    
    private IEnumerator ProcessingLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(processingCheckInterval);
            
            ProcessPendingRequests();
            CheckCompletedJobs();
            UpdateFacilityEfficiency();
        }
    }
    
    private void ProcessPendingRequests()
    {
        while (pendingRequests.Count > 0 && GetAvailableProcessingSlots() > 0)
        {
            var request = pendingRequests.Dequeue();
            
            if (request.isRecipeProcessing)
            {
                if (request.recipe != null && request.recipe.CanAfford())
                {
                    var facility = FindSuitableFacilityForRecipe(request.recipe, request.preferredFacility);
                    if (facility != null)
                    {
                        StartRecipeProcessingJob(request.recipe, facility);
                    }
                    else
                    {
                        // Re-queue if no facility available
                        pendingRequests.Enqueue(request);
                        break;
                    }
                }
            }
            else
            {
                if (request.wasteItem != null)
                {
                    if (!StartProcessingJob(request.wasteItem, request.quantity, request.processingType))
                    {
                        // Re-queue if processing failed
                        pendingRequests.Enqueue(request);
                        break;
                    }
                }
            }
        }
        
        OnQueueChanged?.Invoke(pendingRequests.Count);
    }
    
    private void CheckCompletedJobs()
    {
        var completedJobs = activeJobs.Where(job => Time.time >= job.completionTime).ToList();
        
        foreach (var job in completedJobs)
        {
            CompleteProcessingJob(job);
        }
    }
    
    private void CompleteProcessingJob(ProcessingJob job)
    {
        activeJobs.Remove(job);
        job.status = ProcessingStatus.Completed;
        
        try
        {
            Dictionary<ResourceType, int> actualYield;
            
            if (job.isRecipeProcessing)
            {
                // Recipe processing
                actualYield = ProcessRecipeYield(job);
                OnRecipeProcessed?.Invoke(job.recipe, actualYield);
            }
            else
            {
                // Direct waste processing
                actualYield = ProcessWasteYield(job);
            }
            
            job.actualYield = actualYield;
            
            // Add resources to inventory
            foreach (var resource in actualYield)
            {
                resourceManager.AddResource(resource.Key, resource.Value);
            }
            
            // Update statistics
            totalItemsProcessed += job.quantity;
            totalProcessingTime += job.processingDuration;
            
            foreach (var resource in actualYield)
            {
                if (totalResourcesGenerated.ContainsKey(resource.Key))
                {
                    totalResourcesGenerated[resource.Key] += resource.Value;
                }
                else
                {
                    totalResourcesGenerated[resource.Key] = resource.Value;
                }
            }
            
            // Trigger completion event
            OnProcessingCompleted?.Invoke(job, actualYield);
            
            Debug.Log($"Completed processing job {job.jobId}: {GetYieldSummary(actualYield)}");
        }
        catch (Exception ex)
        {
            job.status = ProcessingStatus.Failed;
            job.errorMessage = ex.Message;
            OnProcessingFailed?.Invoke(job, ex.Message);
            Debug.LogError($"Processing job {job.jobId} failed: {ex.Message}");
        }
    }
    
    private Dictionary<ResourceType, int> ProcessWasteYield(ProcessingJob job)
    {
        // Use ResourceYieldCalculator for final yield calculation
        return ResourceYieldCalculator.CalculateYield(
            job.wasteItem, 
            job.facility.efficiency,
            1f, // location multiplier (could be enhanced later)
            0f  // player skill bonus (could be enhanced later)
        );
    }
    
    private Dictionary<ResourceType, int> ProcessRecipeYield(ProcessingJob job)
    {
        var yield = new Dictionary<ResourceType, int>();
        
        // Base outputs
        foreach (var output in job.recipe.outputResources)
        {
            int amount = Mathf.RoundToInt(output.amount * job.facility.efficiency);
            yield[output.type] = amount;
        }
        
        // Bonus outputs (chance-based)
        if (job.recipe.bonusOutputs != null)
        {
            foreach (var bonus in job.recipe.bonusOutputs)
            {
                if (UnityEngine.Random.value <= bonus.chance)
                {
                    int amount = Mathf.RoundToInt(bonus.amount * job.facility.efficiency);
                    if (yield.ContainsKey(bonus.type))
                    {
                        yield[bonus.type] += amount;
                    }
                    else
                    {
                        yield[bonus.type] = amount;
                    }
                }
            }
        }
        
        return yield;
    }
    
    private string GetYieldSummary(Dictionary<ResourceType, int> yield)
    {
        if (yield == null || yield.Count == 0) return "No resources";
        
        return string.Join(", ", yield.Select(kvp => $"{kvp.Value} {kvp.Key}"));
    }
    
    private void UpdateFacilityEfficiency()
    {
        // Update facility efficiency based on usage, maintenance, etc.
        float totalEfficiency = 0f;
        int operationalFacilities = 0;
        
        foreach (var facility in availableFacilities)
        {
            if (facility.isOperational)
            {
                totalEfficiency += facility.efficiency;
                operationalFacilities++;
            }
        }
        
        float newEfficiency = operationalFacilities > 0 ? totalEfficiency / operationalFacilities : 0f;
        
        if (Mathf.Abs(newEfficiency - facilityEfficiency) > 0.01f)
        {
            facilityEfficiency = newEfficiency;
            OnEfficiencyChanged?.Invoke(facilityEfficiency);
        }
    }
    
    #region Public API Methods
    
    /// <summary>
    /// Get number of available processing slots
    /// </summary>
    public int GetAvailableProcessingSlots()
    {
        int totalSlots = availableFacilities.Where(f => f.isOperational).Sum(f => f.maxConcurrentJobs);
        int usedSlots = activeJobs.Count(job => job.status == ProcessingStatus.InProgress);
        return Mathf.Max(0, totalSlots - usedSlots);
    }
    
    /// <summary>
    /// Get list of active processing jobs
    /// </summary>
    public List<ProcessingJob> GetActiveJobs()
    {
        return new List<ProcessingJob>(activeJobs);
    }
    
    /// <summary>
    /// Get list of available facilities
    /// </summary>
    public List<ProcessingFacility> GetAvailableFacilities()
    {
        return new List<ProcessingFacility>(availableFacilities);
    }
    
    /// <summary>
    /// Get list of available recipes
    /// </summary>
    public List<ProcessingRecipeData> GetAvailableRecipes()
    {
        return availableRecipes.Where(r => r.CanUse()).ToList();
    }
    
    /// <summary>
    /// Get number of pending requests
    /// </summary>
    public int GetPendingRequestCount()
    {
        return pendingRequests.Count;
    }
    
    /// <summary>
    /// Add a new processing facility
    /// </summary>
    public void AddFacility(ProcessingFacility facility)
    {
        if (facility != null && !availableFacilities.Contains(facility))
        {
            availableFacilities.Add(facility);
            Debug.Log($"Added processing facility: {facility.facilityName}");
        }
    }
    
    /// <summary>
    /// Remove a processing facility
    /// </summary>
    public bool RemoveFacility(ProcessingFacility facility)
    {
        if (facility != null && availableFacilities.Contains(facility))
        {
            // Cancel any active jobs for this facility
            var facilityJobs = activeJobs.Where(job => job.facility == facility).ToList();
            foreach (var job in facilityJobs)
            {
                CancelProcessingJob(job.jobId);
            }
            
            availableFacilities.Remove(facility);
            Debug.Log($"Removed processing facility: {facility.facilityName}");
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Cancel a specific processing job
    /// </summary>
    public bool CancelProcessingJob(string jobId)
    {
        var job = activeJobs.FirstOrDefault(j => j.jobId == jobId);
        if (job != null)
        {
            activeJobs.Remove(job);
            job.status = ProcessingStatus.Cancelled;
            
            // Refund resources if it was a recipe job
            if (job.isRecipeProcessing && job.recipe != null)
            {
                foreach (var input in job.recipe.inputResources)
                {
                    resourceManager.AddResource(input.type, input.amount);
                }
            }
            
            Debug.Log($"Cancelled processing job: {jobId}");
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Cancel all active processing jobs
    /// </summary>
    public int CancelAllJobs()
    {
        int cancelledCount = 0;
        var jobsToCancel = new List<ProcessingJob>(activeJobs);
        
        foreach (var job in jobsToCancel)
        {
            if (CancelProcessingJob(job.jobId))
            {
                cancelledCount++;
            }
        }
        
        return cancelledCount;
    }
    
    /// <summary>
    /// Upgrade a facility to the next level
    /// </summary>
    public bool UpgradeFacility(int facilityIndex)
    {
        if (facilityIndex >= 0 && facilityIndex < availableFacilities.Count)
        {
            var facility = availableFacilities[facilityIndex];
            facility.level++;
            facility.efficiency += 0.1f; // 10% efficiency boost per level
            
            Debug.Log($"Upgraded {facility.facilityName} to level {facility.level}");
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Get processing statistics
    /// </summary>
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
/// Processing facility configuration
/// Updated to work with UpdatedWasteItem and enhanced features
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
    public string specialization = ""; // e.g., "Fuel Production", "Metal Processing"
    
    /// <summary>
    /// Check if this facility can process the given waste item
    /// </summary>
    public bool CanProcess(UpdatedWasteItem wasteItem)
    {
        return isOperational && 
               (supportedWasteTypes.Count == 0 || supportedWasteTypes.Contains(wasteItem.Type));
    }
    
    /// <summary>
    /// Check if this facility can handle the given recipe
    /// </summary>
    public bool CanProcessRecipe(ProcessingRecipeData recipe)
    {
        return isOperational && 
               level >= recipe.minimumFacilityLevel &&
               (string.IsNullOrEmpty(recipe.requiredFacility) || facilityName.Contains(recipe.requiredFacility));
    }
    
    /// <summary>
    /// Get facility status description
    /// </summary>
    public string GetStatusDescription()
    {
        if (!isOperational) return "Offline";
        return $"Level {level} - {efficiency:P0} Efficiency";
    }
}

/// <summary>
/// Individual processing job
/// Updated to use UpdatedWasteItem and support recipe processing
/// </summary>
[System.Serializable]
public class ProcessingJob
{
    public string jobId;
    public UpdatedWasteItem wasteItem; // For direct waste processing
    public ProcessingRecipeData recipe; // For recipe processing
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
    public bool isRecipeProcessing = false;
    
    public float Progress => status == ProcessingStatus.InProgress ? 
        Mathf.Clamp01((Time.time - startTime) / processingDuration) : 
        (status == ProcessingStatus.Completed ? 1f : 0f);
    
    public float RemainingTime => status == ProcessingStatus.InProgress ? 
        Mathf.Max(0f, completionTime - Time.time) : 0f;
    
    public string GetDisplayName()
    {
        if (isRecipeProcessing && recipe != null)
        {
            return recipe.recipeName;
        }
        else if (wasteItem != null)
        {
            return $"{quantity}x {wasteItem.Name}";
        }
        return "Unknown Job";
    }
}

/// <summary>
/// Processing request for queuing
/// Updated to support both waste items and recipes
/// </summary>
[System.Serializable]
public class ProcessingRequest
{
    public UpdatedWasteItem wasteItem; // For direct waste processing
    public ProcessingRecipeData recipe; // For recipe processing
    public int quantity;
    public ProcessingType processingType;
    public float requestTime;
    public bool isRecipeProcessing = false;
    public string preferredFacility; // Optional preferred facility name
}

/// <summary>
/// Processing statistics data
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
        return $"Processed: {totalItemsProcessed} items, Efficiency: {currentEfficiency:P0}, Active: {activeJobs}, Pending: {pendingRequests}";
    }
}