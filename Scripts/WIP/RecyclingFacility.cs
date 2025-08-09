using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Recycling Facility - Converts waste items into basic resources
/// Specializes in processing common waste types with good efficiency
/// </summary>
public class RecyclingFacility : ProcessingFacilityBase
{
    [Header("Recycling Specific Settings")]
    [SerializeField] private float recyclingEfficiencyBonus = 0.2f;
    [SerializeField] private float contaminationReduction = 0.15f;
    [SerializeField] private bool canProcessContaminatedWaste = true;
    [SerializeField] private float batchProcessingBonus = 0.1f;
    [SerializeField] private int batchSize = 5;
    
    [Header("Supported Materials")]
    [SerializeField] private List<WasteType> preferredWasteTypes = new List<WasteType>
    {
        WasteType.Plastic,
        WasteType.Metal,
        WasteType.Paper,
        WasteType.Glass
    };
    
    [Header("Resource Output Modifiers")]
    [SerializeField] private float metalYieldBonus = 0.25f;
    [SerializeField] private float plasticYieldBonus = 0.3f;
    [SerializeField] private float paperYieldBonus = 0.15f;
    [SerializeField] private float glassYieldBonus = 0.2f;
    
    // Batch processing
    private List<UpdatedWasteItem> batchQueue = new List<UpdatedWasteItem>();
    private float batchTimer = 0f;
    private float batchProcessingTime = 10f;
    
    protected override void Awake()
    {
        // Set default values for recycling facility
        facilityName = "Recycling Facility";
        facilityType = ProcessingType.Recycling;
        baseEfficiency = 1.2f;
        maxConcurrentJobs = 2;
        energyConsumption = 0.8f;
        
        // Recycling facilities specialize in basic components
        maxRecipeComplexity = 2;
        craftingEfficiencyBonus = 0.15f;
        specializedOutputs.AddRange(new[]
        {
            ResourceType.HullPiece,
            ResourceType.PowerCell,
            ResourceType.JointConnector
        });
        
        // Set supported waste types
        supportedWasteTypes = new List<WasteType>(preferredWasteTypes);
        
        base.Awake();
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Handle batch processing
        if (batchQueue.Count > 0)
        {
            batchTimer += Time.deltaTime;
            
            if (batchTimer >= batchProcessingTime || batchQueue.Count >= batchSize)
            {
                ProcessBatch();
                batchTimer = 0f;
            }
        }
    }
    
    protected override void InitializeFacilityData()
    {
        base.InitializeFacilityData();
        
        // Add recycling-specific data
        facilityData.facilityName = "Recycling Facility";
        facilityData.facilityType = ProcessingType.Recycling;
    }
    
    public override bool CanProcessWasteItem(UpdatedWasteItem wasteItem)
    {
        if (!base.CanProcessWasteItem(wasteItem)) return false;
        
        // Recycling facilities can handle contaminated waste better
        if (wasteItem.ContaminationLevel > 0.7f && !canProcessContaminatedWaste)
            return false;
        
        // Prefer certain waste types
        if (preferredWasteTypes.Contains(wasteItem.Type))
            return true;
        
        // Can process other types but with reduced efficiency
        return wasteItem.Type != WasteType.Hazardous && wasteItem.Type != WasteType.Biological;
    }
    
    public override bool ProcessWasteItem(UpdatedWasteItem wasteItem, int quantity = 1)
    {
        if (!CanProcessWasteItem(wasteItem))
            return false;
        
        // Check if we should add to batch queue for better efficiency
        if (ShouldBatchProcess(wasteItem))
        {
            AddToBatch(wasteItem, quantity);
            return true;
        }
        
        return base.ProcessWasteItem(wasteItem, quantity);
    }
    
    protected override float GetProcessingModifier(UpdatedWasteItem wasteItem)
    {
        float modifier = base.GetProcessingModifier(wasteItem);
        
        // Apply recycling efficiency bonus
        modifier += recyclingEfficiencyBonus;
        
        // Bonus for preferred waste types
        if (preferredWasteTypes.Contains(wasteItem.Type))
        {
            modifier += 0.15f;
        }
        
        // Apply material-specific bonuses
        switch (wasteItem.Type)
        {
            case WasteType.Metal:
                modifier += metalYieldBonus;
                break;
            case WasteType.Plastic:
                modifier += plasticYieldBonus;
                break;
            case WasteType.Paper:
                modifier += paperYieldBonus;
                break;
            case WasteType.Glass:
                modifier += glassYieldBonus;
                break;
        }
        
        // Reduce contamination penalty
        float contaminationPenalty = wasteItem.ContaminationLevel * 0.3f;
        contaminationPenalty *= (1f - contaminationReduction);
        modifier -= contaminationPenalty;
        
        // Batch processing bonus
        if (batchQueue.Count > 1)
        {
            modifier += batchProcessingBonus * (batchQueue.Count / (float)batchSize);
        }
        
        return Mathf.Max(0.1f, modifier);
    }
    
    protected override void OnFacilityUpgraded()
    {
        base.OnFacilityUpgraded();
        
        // Recycling-specific upgrades
        recyclingEfficiencyBonus += 0.05f;
        contaminationReduction += 0.05f;
        
        // Increase batch size every 2 levels
        if (facilityLevel % 2 == 0)
        {
            batchSize++;
            batchProcessingTime = Mathf.Max(5f, batchProcessingTime - 1f);
        }
        
        // Unlock contaminated waste processing at level 3
        if (facilityLevel >= 3)
        {
            canProcessContaminatedWaste = true;
        }
        
        Debug.Log($"Recycling Facility upgraded - Efficiency: {recyclingEfficiencyBonus:P}, Batch Size: {batchSize}");
    }
    
    protected override void OnProcessingCompleted(ProcessingJob job, Dictionary<ResourceType, int> yield)
    {
        base.OnProcessingCompleted(job, yield);
        
        // Recycling facilities produce cleaner resources
        if (yield.ContainsKey(ResourceType.ScrapMetal))
        {
            // Convert some scrap metal to refined metal
            int scrapAmount = yield[ResourceType.ScrapMetal];
            int refinedAmount = Mathf.FloorToInt(scrapAmount * 0.3f);
            
            if (refinedAmount > 0)
            {
                yield[ResourceType.ScrapMetal] -= refinedAmount;
                if (yield.ContainsKey(ResourceType.RefinedMetal))
                    yield[ResourceType.RefinedMetal] += refinedAmount;
                else
                    yield[ResourceType.RefinedMetal] = refinedAmount;
            }
        }
        
        // Special recycling completion effects
        PlayRecyclingEffects();
    }
    
    #region Batch Processing
    
    private bool ShouldBatchProcess(UpdatedWasteItem wasteItem)
    {
        // Batch process if we have space and the item is suitable
        return batchQueue.Count < batchSize && 
               preferredWasteTypes.Contains(wasteItem.Type) &&
               AvailableSlots > 0;
    }
    
    private void AddToBatch(UpdatedWasteItem wasteItem, int quantity)
    {
        for (int i = 0; i < quantity && batchQueue.Count < batchSize; i++)
        {
            batchQueue.Add(wasteItem);
        }
        
        Debug.Log($"Added {wasteItem.Name} to recycling batch ({batchQueue.Count}/{batchSize})");
    }
    
    private void ProcessBatch()
    {
        if (batchQueue.Count == 0) return;
        
        Debug.Log($"Processing recycling batch of {batchQueue.Count} items");
        
        // Group items by type for more efficient processing
        var groupedItems = batchQueue.GroupBy(item => item.Type).ToList();
        
        foreach (var group in groupedItems)
        {
            var representativeItem = group.First();
            int count = group.Count();
            
            // Process the batch with bonus efficiency
            if (processingManager != null)
            {
                processingManager.ProcessWasteItem(representativeItem, count, facilityType);
            }
        }
        
        batchQueue.Clear();
        
        // Play batch completion effects
        PlayBatchCompletionEffects();
    }
    
    #endregion
    
    #region Visual Effects
    
    private void PlayRecyclingEffects()
    {
        // Play recycling-specific particle effects
        if (processingEffect != null)
        {
            var main = processingEffect.main;
            main.startColor = Color.green;
            processingEffect.Emit(20);
        }
        
        // Play recycling sound
        if (processingAudio != null && processingAudio.clip != null)
        {
            processingAudio.pitch = Random.Range(0.9f, 1.1f);
            processingAudio.Play();
        }
    }
    
    private void PlayBatchCompletionEffects()
    {
        // Special effects for batch completion
        if (processingEffect != null)
        {
            var main = processingEffect.main;
            main.startColor = Color.cyan;
            processingEffect.Emit(50);
        }
        
        Debug.Log("Batch recycling completed with efficiency bonus!");
    }
    
    protected override void UpdateVisualEffects()
    {
        base.UpdateVisualEffects();
        
        // Update batch processing indicator
        if (statusLight != null && batchQueue.Count > 0)
        {
            // Pulse the light when batch processing
            float pulse = Mathf.Sin(Time.time * 3f) * 0.3f + 0.7f;
            statusLight.intensity = pulse;
        }
    }
    
    #endregion
    
    #region Public Interface Extensions
    
    /// <summary>
    /// Get current batch status
    /// </summary>
    public string GetBatchStatus()
    {
        if (batchQueue.Count == 0)
            return "No items in batch queue";
        
        float progress = batchTimer / batchProcessingTime;
        return $"Batch: {batchQueue.Count}/{batchSize} items ({progress:P} complete)";
    }
    
    /// <summary>
    /// Force process current batch
    /// </summary>
    public void ForceProcessBatch()
    {
        if (batchQueue.Count > 0)
        {
            ProcessBatch();
            Debug.Log("Forced batch processing");
        }
    }
    
    /// <summary>
    /// Clear current batch queue
    /// </summary>
    public void ClearBatch()
    {
        if (batchQueue.Count > 0)
        {
            Debug.Log($"Cleared batch queue of {batchQueue.Count} items");
            batchQueue.Clear();
            batchTimer = 0f;
        }
    }
    
    /// <summary>
    /// Get recycling efficiency for a specific waste type
    /// </summary>
    public float GetRecyclingEfficiency(WasteType wasteType)
    {
        float efficiency = recyclingEfficiencyBonus;
        
        if (preferredWasteTypes.Contains(wasteType))
        {
            efficiency += 0.15f;
            
            switch (wasteType)
            {
                case WasteType.Metal:
                    efficiency += metalYieldBonus;
                    break;
                case WasteType.Plastic:
                    efficiency += plasticYieldBonus;
                    break;
                case WasteType.Paper:
                    efficiency += paperYieldBonus;
                    break;
                case WasteType.Glass:
                    efficiency += glassYieldBonus;
                    break;
            }
        }
        
        return efficiency;
    }
    
    public override string GetStatusInfo()
    {
        string baseStatus = base.GetStatusInfo();
        
        if (batchQueue.Count > 0)
        {
            baseStatus += $" | Batch: {batchQueue.Count}/{batchSize}";
        }
        
        return baseStatus;
    }
    
    #endregion
} 