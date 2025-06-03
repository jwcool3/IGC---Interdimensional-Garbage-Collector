using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Handles processing waste items into specific resources instead of abstract currencies
/// </summary>
public class UpdatedWasteProcessor : MonoBehaviour
{
    // Singleton pattern
    public static UpdatedWasteProcessor Instance { get; private set; }
    
    [Header("Processing Settings")]
    [SerializeField] private float baseProcessingTime = 1f;
    [SerializeField] private bool showProcessingEffects = true;
    
    [Header("Processing Modifiers")]
    [SerializeField] private float stabilityLossMultiplier = 0.1f;
    [SerializeField] private float contaminationMultiplier = 1f;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem processingEffect;
    [SerializeField] private AudioSource processingAudio;
    
    // Events
    public event Action<UpdatedWasteItem, ResourceYield> OnWasteProcessed;
    public event Action<ResourceYield> OnResourcesGained;
    public event Action<float> OnContaminationAdded;
    
    // Processing queue for batch operations
    private Queue<WasteProcessingJob> processingQueue = new Queue<WasteProcessingJob>();
    private WasteProcessingJob currentJob;
    private bool isProcessing = false;
    
    private struct WasteProcessingJob
    {
        public UpdatedWasteItem wasteItem;
        public int quantity;
        public Action<bool> onComplete;
        
        public WasteProcessingJob(UpdatedWasteItem item, int qty, Action<bool> callback)
        {
            wasteItem = item;
            quantity = qty;
            onComplete = callback;
        }
    }
    
    private void Awake()
    {
        // Singleton setup
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
    
    private void Update()
    {
        ProcessQueue();
    }
    
    #region Public Processing Methods
    
    /// <summary>
    /// Process a single waste item immediately
    /// </summary>
    public bool ProcessWasteItem(UpdatedWasteItem wasteItem)
    {
        if (wasteItem == null)
        {
            Debug.LogWarning("Cannot process null waste item");
            return false;
        }
        
        return ProcessWasteItemInternal(wasteItem, 1);
    }
    
    /// <summary>
    /// Process multiple quantities of a waste item
    /// </summary>
    public bool ProcessWasteItems(UpdatedWasteItem wasteItem, int quantity)
    {
        if (wasteItem == null || quantity <= 0)
        {
            Debug.LogWarning("Invalid waste item or quantity for processing");
            return false;
        }
        
        bool allSuccessful = true;
        
        for (int i = 0; i < quantity; i++)
        {
            if (!ProcessWasteItemInternal(wasteItem, 1))
            {
                allSuccessful = false;
            }
        }
        
        return allSuccessful;
    }
    
    /// <summary>
    /// Queue waste item for batch processing (useful for large quantities)
    /// </summary>
    public void QueueWasteForProcessing(UpdatedWasteItem wasteItem, int quantity, Action<bool> onComplete = null)
    {
        if (wasteItem == null || quantity <= 0) return;
        
        var job = new WasteProcessingJob(wasteItem, quantity, onComplete);
        processingQueue.Enqueue(job);
        
        Debug.Log($"Queued {quantity}x {wasteItem.Name} for processing. Queue size: {processingQueue.Count}");
    }
    
    /// <summary>
    /// Process all items of a specific type from inventory
    /// </summary>
    public void ProcessAllWasteOfType(string wasteItemName, Action<int> onComplete = null)
    {
        // This would integrate with your inventory system
        // For now, it's a placeholder for the concept
        Debug.Log($"Processing all waste of type: {wasteItemName}");
        onComplete?.Invoke(0);
    }
    
    #endregion
    
    #region Core Processing Logic
    
    /// <summary>
    /// Internal method that handles the actual waste processing
    /// </summary>
    private bool ProcessWasteItemInternal(UpdatedWasteItem wasteItem, int quantity)
    {
        if (NewResourceManager.Instance == null)
        {
            Debug.LogError("NewResourceManager.Instance is null! Cannot process waste.");
            return false;
        }
        
        // Ensure the waste item has initialized properties
        wasteItem.InitializeProperties();
        
        // Calculate resource yield for this processing attempt
        ResourceYield yield = CalculateProcessingYield(wasteItem, quantity);
        
        // Apply processing effects (contamination, stability loss)
        ApplyProcessingEffects(wasteItem);
        
        // Add resources to inventory
        bool resourcesAdded = NewResourceManager.Instance.AddResourceYield(yield);
        
        if (resourcesAdded)
        {
            // Trigger visual and audio effects
            TriggerProcessingEffects(wasteItem, yield);
            
            // Fire events
            OnWasteProcessed?.Invoke(wasteItem, yield);
            OnResourcesGained?.Invoke(yield);
            
            Debug.Log($"Successfully processed {wasteItem.Name} → {GetYieldSummary(yield)}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Failed to add resources from {wasteItem.Name} - storage may be full");
            return false;
        }
    }
    
    /// <summary>
    /// Calculate the actual resource yield from processing this waste item
    /// </summary>
    private ResourceYield CalculateProcessingYield(UpdatedWasteItem wasteItem, int quantity)
    {
        ResourceYield baseYield = wasteItem.ResourceYield;
        ResourceYield scaledYield = new ResourceYield();
        
        // Scale primary resources by quantity
        List<ResourceAmount> scaledPrimary = new List<ResourceAmount>();
        foreach (var resource in baseYield.primaryResources)
        {
            int scaledAmount = resource.amount * quantity;
            scaledPrimary.Add(new ResourceAmount(resource.type, scaledAmount));
        }
        scaledYield.primaryResources = scaledPrimary.ToArray();
        
        // Scale secondary resources by quantity (chance remains the same)
        List<ResourceChance> scaledSecondary = new List<ResourceChance>();
        foreach (var chance in baseYield.secondaryResources)
        {
            int scaledAmount = chance.amount * quantity;
            scaledSecondary.Add(new ResourceChance(chance.type, scaledAmount, chance.chance));
        }
        scaledYield.secondaryResources = scaledSecondary.ToArray();
        
        // Scale contamination risk (but cap it)
        scaledYield.contaminationRisk = Mathf.Min(baseYield.contaminationRisk * quantity * contaminationMultiplier, 0.95f);
        
        return scaledYield;
    }
    
    /// <summary>
    /// Apply side effects of processing (contamination, etc.)
    /// </summary>
    private void ApplyProcessingEffects(UpdatedWasteItem wasteItem)
    {
        // Reduce item stability after processing
        float stabilityLoss = stabilityLossMultiplier * (1f - wasteItem.WasteStability);
        wasteItem.WasteStability = Mathf.Max(0.1f, wasteItem.WasteStability - stabilityLoss);
        
        // Add contamination to ship/environment if needed
        if (wasteItem.ContaminationLevel > 0.5f)
        {
            float contaminationAdded = wasteItem.ContaminationLevel * contaminationMultiplier;
            OnContaminationAdded?.Invoke(contaminationAdded);
            
            Debug.Log($"Processing {wasteItem.Name} added {contaminationAdded:F2} contamination");
        }
    }
    
    #endregion
    
    #region Queue Processing
    
    /// <summary>
    /// Process the queue of waste items over time
    /// </summary>
    private void ProcessQueue()
    {
        if (isProcessing)
        {
            // Handle current job timing
            // For simplicity, we're processing instantly, but you could add timing here
            CompleteCurrentJob();
        }
        else if (processingQueue.Count > 0)
        {
            StartNextJob();
        }
    }
    
    private void StartNextJob()
    {
        if (processingQueue.Count == 0) return;
        
        currentJob = processingQueue.Dequeue();
        isProcessing = true;
        
        Debug.Log($"Starting processing job: {currentJob.quantity}x {currentJob.wasteItem.Name}");
    }
    
    private void CompleteCurrentJob()
    {
        bool success = ProcessWasteItems(currentJob.wasteItem, currentJob.quantity);
        
        currentJob.onComplete?.Invoke(success);
        isProcessing = false;
        
        Debug.Log($"Completed processing job. Success: {success}");
    }
    
    #endregion
    
    #region Visual and Audio Effects
    
    /// <summary>
    /// Trigger visual and audio effects for processing
    /// </summary>
    private void TriggerProcessingEffects(UpdatedWasteItem wasteItem, ResourceYield yield)
    {
        if (!showProcessingEffects) return;
        
        // Particle effects
        if (processingEffect != null)
        {
            var main = processingEffect.main;
            main.startColor = wasteItem.RarityColor;
            processingEffect.Play();
        }
        
        // Audio effects
        if (processingAudio != null)
        {
            // Adjust pitch based on rarity
            float pitchModifier = 1f + ((int)wasteItem.Rarity * 0.1f);
            processingAudio.pitch = pitchModifier;
            processingAudio.Play();
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Get a summary string of what resources were yielded
    /// </summary>
    private string GetYieldSummary(ResourceYield yield)
    {
        List<string> summary = new List<string>();
        
        foreach (var resource in yield.primaryResources)
        {
            summary.Add($"{resource.amount} {resource.type}");
        }
        
        // Note: Secondary resources are chance-based, so they may or may not be included
        
        return string.Join(", ", summary);
    }
    
    /// <summary>
    /// Get processing time for a waste item (for UI display)
    /// </summary>
    public float GetProcessingTime(UpdatedWasteItem wasteItem)
    {
        if (wasteItem == null) return baseProcessingTime;
        
        // More complex items take longer to process
        float complexityMultiplier = 1f + ((int)wasteItem.Rarity * 0.2f);
        float stabilityMultiplier = 2f - wasteItem.WasteStability; // Less stable = takes longer
        
        return baseProcessingTime * complexityMultiplier * stabilityMultiplier;
    }
    
    /// <summary>
    /// Preview what resources would be gained from processing (for UI)
    /// </summary>
    public string GetProcessingPreview(UpdatedWasteItem wasteItem, int quantity = 1)
    {
        if (wasteItem == null) return "No preview available";
        
        ResourceYield previewYield = CalculateProcessingYield(wasteItem, quantity);
        return GetYieldSummary(previewYield);
    }
    
    /// <summary>
    /// Check if the processor can handle a specific waste type
    /// </summary>
    public bool CanProcess(UpdatedWasteItem wasteItem)
    {
        if (wasteItem == null) return false;
        
        // Add any processing restrictions here
        // For example, certain waste types might require upgraded facilities
        
        return true;
    }
    
    /// <summary>
    /// Get current queue status
    /// </summary>
    public int GetQueueSize()
    {
        return processingQueue.Count;
    }
    
    /// <summary>
    /// Clear the processing queue
    /// </summary>
    public void ClearQueue()
    {
        processingQueue.Clear();
        isProcessing = false;
        Debug.Log("Processing queue cleared");
    }
    
    #endregion
    
    #region Legacy Compatibility
    
    /// <summary>
    /// Convert old recycling system calls to new resource system
    /// </summary>
    public void ProcessLegacyWaste(string wasteName, float recyclingPoints, string origin)
    {
        // Create a temporary waste item for legacy compatibility
        var tempWasteItem = new UpdatedWasteItem(wasteName, origin, WasteRarity.Common);
        
        // Convert RP to approximate resource value
        int resourceValue = Mathf.RoundToInt(recyclingPoints / 10f);
        
        // Create a simple yield
        ResourceYield legacyYield = new ResourceYield();
        legacyYield.primaryResources = new ResourceAmount[] 
        { 
            new ResourceAmount(ResourceType.Plastic, resourceValue) 
        };
        legacyYield.contaminationRisk = 0.1f;
        
        // Add to resource manager
        NewResourceManager.Instance?.AddResourceYield(legacyYield);
        
        Debug.Log($"Converted legacy waste: {wasteName} ({recyclingPoints} RP) → {resourceValue} Plastic");
    }
    
    #endregion
}