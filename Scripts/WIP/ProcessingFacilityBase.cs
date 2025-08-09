using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Base class for all processing facility MonoBehaviours
/// Handles the physical representation and interaction of processing facilities
/// </summary>
public abstract class ProcessingFacilityBase : MonoBehaviour
{
    [Header("Facility Configuration")]
    [SerializeField] protected string facilityName = "Processing Facility";
    [SerializeField] protected ProcessingType facilityType = ProcessingType.Recycling;
    [SerializeField] protected int facilityLevel = 1;
    [SerializeField] protected float baseEfficiency = 1f;
    [SerializeField] protected int maxConcurrentJobs = 1;
    [SerializeField] protected float energyConsumption = 1f;
    [SerializeField] protected List<WasteType> supportedWasteTypes = new List<WasteType>();
    
    [Header("Visual Components")]
    [SerializeField] protected GameObject facilityModel;
    [SerializeField] protected ParticleSystem processingEffect;
    [SerializeField] protected AudioSource processingAudio;
    [SerializeField] protected Light statusLight;
    [SerializeField] protected Color operationalColor = Color.green;
    [SerializeField] protected Color processingColor = Color.yellow;
    [SerializeField] protected Color offlineColor = Color.red;
    
    [Header("UI Components")]
    [SerializeField] protected Canvas facilityUI;
    [SerializeField] protected GameObject interactionPrompt;
    
    [Header("Processing Settings")]
    [SerializeField] protected bool autoProcessNearbyWaste = false;
    [SerializeField] protected float detectionRadius = 5f;
    [SerializeField] protected LayerMask wasteLayerMask = -1;
    
    [Header("Crafting Specialization")]
    [SerializeField] protected int maxRecipeComplexity = 3;
    [SerializeField] protected float craftingEfficiencyBonus = 0f;
    [SerializeField] protected List<ResourceType> specializedOutputs = new List<ResourceType>();
    
    // Runtime properties
    protected ProcessingFacility facilityData;
    protected bool isOperational = true;
    protected bool isProcessing = false;
    protected List<string> activeJobIds = new List<string>();
    protected float currentEfficiency;
    
    // Component references
    protected ResourceProcessingManager processingManager;
    protected NewResourceManager resourceManager;
    protected WasteInventoryManager wasteInventory;
    
    // Events
    public event Action<ProcessingFacilityBase> OnFacilityActivated;
    public event Action<ProcessingFacilityBase> OnFacilityDeactivated;
    public event Action<ProcessingFacilityBase, ProcessingJob> OnJobStarted;
    public event Action<ProcessingFacilityBase, ProcessingJob> OnJobCompleted;
    
    // Properties
    public string FacilityName => facilityName;
    public ProcessingType FacilityType => facilityType;
    public int Level => facilityLevel;
    public float Efficiency => currentEfficiency;
    public bool IsOperational => isOperational;
    public bool IsProcessing => isProcessing;
    public int ActiveJobs => activeJobIds.Count;
    public int AvailableSlots => maxConcurrentJobs - activeJobIds.Count;
    public ProcessingFacility FacilityData => facilityData;
    
    /// <summary>
    /// Get the maximum recipe complexity this facility can handle
    /// </summary>
    public virtual int MaxRecipeComplexity => maxRecipeComplexity;

    /// <summary>
    /// Get the crafting efficiency bonus for this facility
    /// </summary>
    public virtual float CraftingEfficiencyBonus => craftingEfficiencyBonus;

    /// <summary>
    /// Check if this facility is specialized for a specific output type
    /// </summary>
    public virtual bool IsSpecializedFor(ResourceType outputType)
    {
        return specializedOutputs.Contains(outputType);
    }

    /// <summary>
    /// Get the efficiency modifier for crafting a specific item type
    /// </summary>
    public virtual float GetCraftingEfficiency(ResourceType outputType)
    {
        float efficiency = baseEfficiency + craftingEfficiencyBonus;
        
        if (IsSpecializedFor(outputType))
        {
            efficiency *= 1.25f; // 25% bonus for specialized items
        }
        
        return efficiency;
    }
    
    protected virtual void Awake()
    {
        InitializeFacilityData();
        InitializeComponents();
    }
    
    protected virtual void Start()
    {
        RegisterWithManager();
        UpdateVisualState();
    }
    
    protected virtual void Update()
    {
        UpdateEfficiency();
        UpdateVisualEffects();
        
        if (autoProcessNearbyWaste && AvailableSlots > 0)
        {
            CheckForNearbyWaste();
        }
    }
    
    protected virtual void InitializeFacilityData()
    {
        facilityData = new ProcessingFacility
        {
            facilityName = this.facilityName,
            facilityType = this.facilityType,
            level = this.facilityLevel,
            efficiency = this.baseEfficiency,
            maxConcurrentJobs = this.maxConcurrentJobs,
            supportedWasteTypes = new List<WasteType>(this.supportedWasteTypes),
            energyConsumption = this.energyConsumption,
            isOperational = this.isOperational
        };
        
        currentEfficiency = baseEfficiency;
    }
    
    protected virtual void InitializeComponents()
    {
        processingManager = ResourceProcessingManager.Instance;
        resourceManager = NewResourceManager.Instance;
        wasteInventory = WasteInventoryManager.Instance;
        
        // Subscribe to processing events
        if (processingManager != null)
        {
            processingManager.OnProcessingStarted += OnProcessingJobStarted;
            processingManager.OnProcessingCompleted += OnProcessingJobCompleted;
            processingManager.OnProcessingFailed += OnProcessingJobFailed;
        }
        
        // Initialize UI
        if (facilityUI != null)
        {
            facilityUI.gameObject.SetActive(false);
        }
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    protected virtual void RegisterWithManager()
    {
        if (processingManager != null)
        {
            processingManager.AddFacility(facilityData);
            Debug.Log($"Registered facility: {facilityName}");
        }
    }
    
    protected virtual void UpdateEfficiency()
    {
        // Base efficiency calculation - can be overridden by subclasses
        float levelBonus = (facilityLevel - 1) * 0.1f;
        float operationalPenalty = isOperational ? 0f : -1f;
        
        currentEfficiency = Mathf.Max(0f, baseEfficiency + levelBonus + operationalPenalty);
        facilityData.efficiency = currentEfficiency;
    }
    
    protected virtual void UpdateVisualState()
    {
        if (statusLight != null)
        {
            if (!isOperational)
            {
                statusLight.color = offlineColor;
            }
            else if (isProcessing)
            {
                statusLight.color = processingColor;
            }
            else
            {
                statusLight.color = operationalColor;
            }
        }
        
        // Update facility model state
        if (facilityModel != null)
        {
            facilityModel.SetActive(isOperational);
        }
    }
    
    protected virtual void UpdateVisualEffects()
    {
        // Update processing effects
        if (processingEffect != null)
        {
            if (isProcessing && isOperational)
            {
                if (!processingEffect.isPlaying)
                {
                    processingEffect.Play();
                }
            }
            else
            {
                if (processingEffect.isPlaying)
                {
                    processingEffect.Stop();
                }
            }
        }
        
        // Update processing audio
        if (processingAudio != null)
        {
            if (isProcessing && isOperational)
            {
                if (!processingAudio.isPlaying)
                {
                    processingAudio.Play();
                }
            }
            else
            {
                if (processingAudio.isPlaying)
                {
                    processingAudio.Stop();
                }
            }
        }
    }
    
    protected virtual void CheckForNearbyWaste()
    {
        // Find nearby waste items that can be processed
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, detectionRadius, wasteLayerMask);
        
        foreach (var obj in nearbyObjects)
        {
            var wasteComponent = obj.GetComponent<UpdatedWasteItem>();
            if (wasteComponent != null && CanProcessWasteItem(wasteComponent))
            {
                ProcessWasteItem(wasteComponent);
                break; // Process one item at a time
            }
        }
    }
    
    #region Public Interface
    
    /// <summary>
    /// Check if this facility can process the given waste item
    /// </summary>
    public virtual bool CanProcessWasteItem(UpdatedWasteItem wasteItem)
    {
        if (!isOperational || wasteItem == null) return false;
        
        // Check if we have available slots
        if (AvailableSlots <= 0) return false;
        
        // Check if waste type is supported
        if (supportedWasteTypes.Count > 0 && !supportedWasteTypes.Contains(wasteItem.Type))
            return false;
        
        return true;
    }
    
    /// <summary>
    /// Check if this facility can process the given recipe
    /// </summary>
    public virtual bool CanProcessRecipe(ProcessingRecipeData recipe)
    {
        if (!isOperational || recipe == null) return false;
        
        // Check if we have available slots
        if (AvailableSlots <= 0) return false;
        
        // Check facility requirements
        if (facilityLevel < recipe.minimumFacilityLevel) return false;
        
        if (!string.IsNullOrEmpty(recipe.requiredFacility) && 
            !facilityName.Contains(recipe.requiredFacility))
            return false;
        
        return true;
    }
    
    /// <summary>
    /// Process a waste item using this facility
    /// </summary>
    public virtual bool ProcessWasteItem(UpdatedWasteItem wasteItem, int quantity = 1)
    {
        if (!CanProcessWasteItem(wasteItem))
        {
            Debug.LogWarning($"Cannot process {wasteItem.Name} in {facilityName}");
            return false;
        }
        
        if (processingManager != null)
        {
            return processingManager.ProcessWasteItem(wasteItem, quantity, facilityType);
        }
        
        return false;
    }
    
    /// <summary>
    /// Process a recipe using this facility
    /// </summary>
    public virtual bool ProcessRecipe(ProcessingRecipeData recipe)
    {
        if (!CanProcessRecipe(recipe))
        {
            Debug.LogWarning($"Cannot process recipe {recipe.recipeName} in {facilityName}");
            return false;
        }
        
        if (processingManager != null)
        {
            return processingManager.ProcessRecipe(recipe, facilityName);
        }
        
        return false;
    }
    
    /// <summary>
    /// Activate/deactivate the facility
    /// </summary>
    public virtual void SetOperational(bool operational)
    {
        bool wasOperational = isOperational;
        isOperational = operational;
        facilityData.isOperational = operational;
        
        if (wasOperational != operational)
        {
            if (operational)
            {
                OnFacilityActivated?.Invoke(this);
                Debug.Log($"Facility {facilityName} activated");
            }
            else
            {
                OnFacilityDeactivated?.Invoke(this);
                Debug.Log($"Facility {facilityName} deactivated");
            }
            
            UpdateVisualState();
        }
    }
    
    /// <summary>
    /// Upgrade the facility to the next level
    /// </summary>
    public virtual bool UpgradeFacility()
    {
        facilityLevel++;
        facilityData.level = facilityLevel;
        
        // Apply upgrade benefits
        OnFacilityUpgraded();
        
        Debug.Log($"Upgraded {facilityName} to level {facilityLevel}");
        return true;
    }
    
    /// <summary>
    /// Get facility status information
    /// </summary>
    public virtual string GetStatusInfo()
    {
        if (!isOperational)
            return "Offline";
        
        if (isProcessing)
            return $"Processing ({ActiveJobs}/{maxConcurrentJobs})";
        
        return $"Ready ({AvailableSlots} slots available)";
    }
    
    /// <summary>
    /// Show facility UI
    /// </summary>
    public virtual void ShowUI()
    {
        if (facilityUI != null)
        {
            facilityUI.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide facility UI
    /// </summary>
    public virtual void HideUI()
    {
        if (facilityUI != null)
        {
            facilityUI.gameObject.SetActive(false);
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    protected virtual void OnProcessingJobStarted(ProcessingJob job)
    {
        if (job.facility == facilityData)
        {
            activeJobIds.Add(job.jobId);
            isProcessing = activeJobIds.Count > 0;
            OnJobStarted?.Invoke(this, job);
            UpdateVisualState();
        }
    }
    
    protected virtual void OnProcessingJobCompleted(ProcessingJob job, Dictionary<ResourceType, int> yield)
    {
        if (job.facility == facilityData && activeJobIds.Contains(job.jobId))
        {
            activeJobIds.Remove(job.jobId);
            isProcessing = activeJobIds.Count > 0;
            OnJobCompleted?.Invoke(this, job);
            UpdateVisualState();
            
            // Trigger completion effects
            OnProcessingCompleted(job, yield);
        }
    }
    
    protected virtual void OnProcessingJobFailed(ProcessingJob job, string error)
    {
        if (job.facility == facilityData && activeJobIds.Contains(job.jobId))
        {
            activeJobIds.Remove(job.jobId);
            isProcessing = activeJobIds.Count > 0;
            UpdateVisualState();
            
            Debug.LogWarning($"Processing job failed in {facilityName}: {error}");
        }
    }
    
    #endregion
    
    #region Virtual Methods for Subclasses
    
    /// <summary>
    /// Called when the facility is upgraded - override for custom upgrade logic
    /// </summary>
    protected virtual void OnFacilityUpgraded()
    {
        // Default upgrade benefits
        baseEfficiency += 0.1f; // 10% efficiency increase
        
        // Some facilities might gain additional slots
        if (facilityLevel % 3 == 0)
        {
            maxConcurrentJobs++;
            facilityData.maxConcurrentJobs = maxConcurrentJobs;
        }
    }
    
    /// <summary>
    /// Called when processing is completed - override for custom effects
    /// </summary>
    protected virtual void OnProcessingCompleted(ProcessingJob job, Dictionary<ResourceType, int> yield)
    {
        // Override in subclasses for facility-specific completion effects
    }
    
    /// <summary>
    /// Get facility-specific processing modifiers
    /// </summary>
    protected virtual float GetProcessingModifier(UpdatedWasteItem wasteItem)
    {
        // Override in subclasses for specialized processing bonuses
        return 1f;
    }
    
    /// <summary>
    /// Get facility-specific recipe modifiers
    /// </summary>
    protected virtual float GetRecipeModifier(ProcessingRecipeData recipe)
    {
        // Override in subclasses for specialized recipe bonuses
        return 1f;
    }
    
    #endregion
    
    #region Interaction System
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ShowInteractionPrompt();
        }
    }
    
    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HideInteractionPrompt();
        }
    }
    
    protected virtual void ShowInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }
    }
    
    protected virtual void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    /// <summary>
    /// Called when player interacts with the facility
    /// </summary>
    public virtual void OnPlayerInteract()
    {
        ShowUI();
    }
    
    #endregion
    
    protected virtual void OnDestroy()
    {
        // Unsubscribe from events
        if (processingManager != null)
        {
            processingManager.OnProcessingStarted -= OnProcessingJobStarted;
            processingManager.OnProcessingCompleted -= OnProcessingJobCompleted;
            processingManager.OnProcessingFailed -= OnProcessingJobFailed;
            
            // Remove facility from manager
            processingManager.RemoveFacility(facilityData);
        }
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        if (autoProcessNearbyWaste)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
} 