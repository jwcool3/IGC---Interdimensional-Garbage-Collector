using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Compactor Facility - Compresses waste to reduce volume and create compressed materials
/// Specializes in volume reduction and creating dense material blocks
/// </summary>
public class CompactorFacility : ProcessingFacilityBase
{
    [Header("Compactor Specific Settings")]
    [SerializeField] private float compressionRatio = 0.3f; // How much volume is reduced
    [SerializeField] private float compressionPressure = 1000f; // PSI
    [SerializeField] private float maxPressure = 5000f;
    [SerializeField] private float pressureBuildupRate = 200f;
    [SerializeField] private float currentPressure = 0f;
    
    [Header("Volume Processing")]
    [SerializeField] private float volumeReductionBonus = 0.8f;
    [SerializeField] private float densityMultiplier = 3f;
    [SerializeField] private int maxVolumePerCycle = 100;
    [SerializeField] private float cycleTime = 15f;
    
    [Header("Material Specialization")]
    [SerializeField] private List<WasteType> preferredWasteTypes = new List<WasteType>
    {
        WasteType.Metal,
        WasteType.Plastic,
        WasteType.Paper,
        WasteType.Cardboard
    };
    
    [Header("Compressed Output")]
    [SerializeField] private float compressedMaterialBonus = 0.5f;
    [SerializeField] private bool canCreateBlocks = true;
    [SerializeField] private int itemsPerBlock = 10;
    
    // Compression cycle management
    private List<UpdatedWasteItem> compressionQueue = new List<UpdatedWasteItem>();
    private float cycleTimer = 0f;
    private bool isCompressing = false;
    private float totalVolumeProcessed = 0f;
    private int blocksCreated = 0;
    
    protected override void Awake()
    {
        // Set default values for compactor facility
        facilityName = "Compactor Facility";
        facilityType = ProcessingType.Compaction;
        baseEfficiency = 1.1f;
        maxConcurrentJobs = 3;
        energyConsumption = 1.2f;
        
        // Compactors specialize in structural components
        maxRecipeComplexity = 3;
        craftingEfficiencyBonus = 0.2f;
        specializedOutputs.AddRange(new[]
        {
            ResourceType.StructuralBeam,
            ResourceType.ArmorPlate,
            ResourceType.WallSection,
            ResourceType.ReinforcedBulkhead
        });
        
        // Set supported waste types
        supportedWasteTypes = new List<WasteType>(preferredWasteTypes);
        
        base.Awake();
    }
    
    protected override void Update()
    {
        base.Update();
        
        UpdatePressure();
        UpdateCompressionCycle();
    }
    
    protected override void InitializeFacilityData()
    {
        base.InitializeFacilityData();
        
        // Add compactor-specific data
        facilityData.facilityName = "Compactor Facility";
        facilityData.facilityType = ProcessingType.Compaction;
    }
    
    public override bool CanProcessWasteItem(UpdatedWasteItem wasteItem)
    {
        if (!base.CanProcessWasteItem(wasteItem)) return false;
        
        // Check if waste is compressible
        if (!IsCompressible(wasteItem.Type))
            return false;
        
        // Check volume capacity
        float estimatedVolume = wasteItem.Weight * 0.1f; // Rough volume estimate
        if (GetCurrentQueueVolume() + estimatedVolume > maxVolumePerCycle)
            return false;
        
        // Check pressure requirements
        if (currentPressure < GetRequiredPressure(wasteItem.Type))
            return false;
        
        return true;
    }
    
    public override bool ProcessWasteItem(UpdatedWasteItem wasteItem, int quantity = 1)
    {
        if (!CanProcessWasteItem(wasteItem))
        {
            Debug.LogWarning($"Cannot compact {wasteItem.Name} - not compressible or insufficient pressure");
            return false;
        }
        
        // Add to compression queue instead of immediate processing
        AddToCompressionQueue(wasteItem, quantity);
        return true;
    }
    
    protected override float GetProcessingModifier(UpdatedWasteItem wasteItem)
    {
        float modifier = base.GetProcessingModifier(wasteItem);
        
        // Apply compression efficiency bonus
        float pressureEfficiency = Mathf.Clamp01(currentPressure / maxPressure);
        modifier += pressureEfficiency * 0.3f;
        
        // Bonus for preferred waste types
        if (preferredWasteTypes.Contains(wasteItem.Type))
        {
            modifier += 0.2f;
        }
        
        // Volume reduction bonus
        modifier += volumeReductionBonus;
        
        // Density bonus for creating compressed materials
        if (canCreateBlocks)
        {
            modifier += compressedMaterialBonus;
        }
        
        // Penalty for contamination (affects compression quality)
        float contaminationPenalty = wasteItem.ContaminationLevel * 0.2f;
        modifier -= contaminationPenalty;
        
        return Mathf.Max(0.1f, modifier);
    }
    
    protected override void OnFacilityUpgraded()
    {
        base.OnFacilityUpgraded();
        
        // Compactor-specific upgrades
        maxPressure += 1000f;
        pressureBuildupRate += 50f;
        maxVolumePerCycle += 20;
        
        // Improve compression ratio
        compressionRatio = Mathf.Max(0.1f, compressionRatio - 0.05f);
        
        // Reduce cycle time
        cycleTime = Mathf.Max(5f, cycleTime - 2f);
        
        // Unlock block creation at level 2
        if (facilityLevel >= 2)
        {
            canCreateBlocks = true;
            itemsPerBlock = Mathf.Max(5, itemsPerBlock - 1);
        }
        
        // Increase density multiplier
        if (facilityLevel >= 3)
        {
            densityMultiplier += 0.5f;
        }
        
        Debug.Log($"Compactor upgraded - Max Pressure: {maxPressure} PSI, Cycle Time: {cycleTime}s");
    }
    
    protected override void OnProcessingCompleted(ProcessingJob job, Dictionary<ResourceType, int> yield)
    {
        base.OnProcessingCompleted(job, yield);
        
        // Apply volume reduction to yield
        ApplyVolumeReduction(yield);
        
        // Create compressed materials
        CreateCompressedMaterials(job.wasteItem, yield);
        
        // Update statistics
        totalVolumeProcessed += job.wasteItem.Weight * job.quantity;
        
        // Special compaction completion effects
        PlayCompressionEffects();
    }
    
    #region Pressure Management
    
    private void UpdatePressure()
    {
        if (isCompressing)
        {
            // Build up pressure during compression
            currentPressure += pressureBuildupRate * Time.deltaTime;
            currentPressure = Mathf.Min(currentPressure, maxPressure);
        }
        else if (compressionQueue.Count == 0)
        {
            // Release pressure when idle
            currentPressure -= pressureBuildupRate * 0.5f * Time.deltaTime;
            currentPressure = Mathf.Max(0f, currentPressure);
        }
    }
    
    private float GetRequiredPressure(WasteType wasteType)
    {
        switch (wasteType)
        {
            case WasteType.Paper:
            case WasteType.Cardboard:
                return 200f;
            case WasteType.Plastic:
                return 500f;
            case WasteType.Metal:
                return 1500f;
            case WasteType.Glass:
                return 800f;
            case WasteType.Organic:
                return 300f;
            default:
                return 400f;
        }
    }
    
    #endregion
    
    #region Compression Cycle Management
    
    private void UpdateCompressionCycle()
    {
        if (compressionQueue.Count > 0)
        {
            if (!isCompressing)
            {
                StartCompressionCycle();
            }
            
            cycleTimer += Time.deltaTime;
            
            if (cycleTimer >= cycleTime)
            {
                CompleteCompressionCycle();
            }
        }
    }
    
    private void StartCompressionCycle()
    {
        isCompressing = true;
        cycleTimer = 0f;
        
        Debug.Log($"Starting compression cycle with {compressionQueue.Count} items");
        PlayCompressionStartEffects();
    }
    
    private void CompleteCompressionCycle()
    {
        if (compressionQueue.Count == 0) return;
        
        Debug.Log($"Completing compression cycle - processing {compressionQueue.Count} items");
        
        // Group items by type for efficient processing
        var groupedItems = compressionQueue.GroupBy(item => item.Type).ToList();
        
        foreach (var group in groupedItems)
        {
            var representativeItem = group.First();
            int count = group.Count();
            
            // Process the compressed batch
            if (processingManager != null)
            {
                processingManager.ProcessWasteItem(representativeItem, count, facilityType);
            }
        }
        
        compressionQueue.Clear();
        isCompressing = false;
        cycleTimer = 0f;
        
        PlayCompressionCompleteEffects();
    }
    
    private void AddToCompressionQueue(UpdatedWasteItem wasteItem, int quantity)
    {
        for (int i = 0; i < quantity && compressionQueue.Count < maxVolumePerCycle; i++)
        {
            compressionQueue.Add(wasteItem);
        }
        
        Debug.Log($"Added {wasteItem.Name} to compression queue ({compressionQueue.Count} items)");
    }
    
    private float GetCurrentQueueVolume()
    {
        return compressionQueue.Sum(item => item.Weight * 0.1f);
    }
    
    #endregion
    
    #region Material Processing
    
    private bool IsCompressible(WasteType wasteType)
    {
        // Most materials can be compressed, except liquids and gases
        switch (wasteType)
        {
            case WasteType.Liquid:
            case WasteType.Gas:
                return false;
            case WasteType.Radioactive:
                return false; // Safety concern
            default:
                return true;
        }
    }
    
    private void ApplyVolumeReduction(Dictionary<ResourceType, int> yield)
    {
        // Compaction reduces the total volume of output
        var keys = yield.Keys.ToList();
        foreach (var key in keys)
        {
            int originalAmount = yield[key];
            int compressedAmount = Mathf.FloorToInt(originalAmount * compressionRatio);
            yield[key] = Mathf.Max(1, compressedAmount); // Always yield at least 1
        }
    }
    
    private void CreateCompressedMaterials(UpdatedWasteItem wasteItem, Dictionary<ResourceType, int> yield)
    {
        if (!canCreateBlocks) return;
        
        // Create compressed blocks for certain materials
        ResourceType compressedType = GetCompressedResourceType(wasteItem.Type);
        if (compressedType != ResourceType.None)
        {
            int blockCount = Mathf.FloorToInt(compressionQueue.Count / (float)itemsPerBlock);
            if (blockCount > 0)
            {
                if (yield.ContainsKey(compressedType))
                    yield[compressedType] += blockCount;
                else
                    yield[compressedType] = blockCount;
                
                blocksCreated += blockCount;
                Debug.Log($"Created {blockCount} compressed {compressedType} blocks");
            }
        }
    }
    
    private ResourceType GetCompressedResourceType(WasteType wasteType)
    {
        switch (wasteType)
        {
            case WasteType.Metal:
                return ResourceType.CompressedMetal;
            case WasteType.Plastic:
                return ResourceType.CompressedPlastic;
            case WasteType.Paper:
            case WasteType.Cardboard:
                return ResourceType.CompressedFiber;
            default:
                return ResourceType.None;
        }
    }
    
    #endregion
    
    #region Visual Effects
    
    private void PlayCompressionEffects()
    {
        // Play compression-specific particle effects
        if (processingEffect != null)
        {
            var main = processingEffect.main;
            main.startColor = Color.blue;
            processingEffect.Emit(25);
        }
        
        // Play compression sound
        if (processingAudio != null && processingAudio.clip != null)
        {
            processingAudio.pitch = Random.Range(0.7f, 0.9f); // Lower pitch for compression
            processingAudio.Play();
        }
    }
    
    private void PlayCompressionStartEffects()
    {
        // Visual feedback for cycle start
        if (statusLight != null)
        {
            statusLight.color = Color.yellow;
            statusLight.intensity = 1.5f;
        }
    }
    
    private void PlayCompressionCompleteEffects()
    {
        // Special effects for cycle completion
        if (processingEffect != null)
        {
            var main = processingEffect.main;
            main.startColor = Color.cyan;
            processingEffect.Emit(40);
        }
        
        Debug.Log("Compression cycle completed!");
    }
    
    protected override void UpdateVisualEffects()
    {
        base.UpdateVisualEffects();
        
        // Update pressure-based visual effects
        if (statusLight != null && isCompressing)
        {
            // Pulse based on pressure
            float pressureRatio = currentPressure / maxPressure;
            float pulse = Mathf.Sin(Time.time * (2f + pressureRatio * 3f)) * 0.3f + 0.7f;
            statusLight.intensity = pulse * (1f + pressureRatio);
            
            // Color changes with pressure
            statusLight.color = Color.Lerp(Color.blue, Color.red, pressureRatio);
        }
        
        // Update compression effects
        if (processingEffect != null && isCompressing)
        {
            var emission = processingEffect.emission;
            emission.rateOverTime = (currentPressure / maxPressure) * 30f;
        }
    }
    
    #endregion
    
    #region Public Interface Extensions
    
    /// <summary>
    /// Get current pressure status
    /// </summary>
    public string GetPressureStatus()
    {
        float pressureRatio = currentPressure / maxPressure;
        return $"Pressure: {currentPressure:F0} / {maxPressure:F0} PSI ({pressureRatio:P})";
    }
    
    /// <summary>
    /// Get compression cycle status
    /// </summary>
    public string GetCycleStatus()
    {
        if (!isCompressing)
            return $"Queue: {compressionQueue.Count} items";
        
        float progress = cycleTimer / cycleTime;
        return $"Compressing: {progress:P} complete ({compressionQueue.Count} items)";
    }
    
    /// <summary>
    /// Get volume processing statistics
    /// </summary>
    public string GetVolumeStats()
    {
        return $"Volume Processed: {totalVolumeProcessed:F1} | Blocks Created: {blocksCreated}";
    }
    
    /// <summary>
    /// Force complete current compression cycle
    /// </summary>
    public void ForceCompleteCycle()
    {
        if (isCompressing && compressionQueue.Count > 0)
        {
            CompleteCompressionCycle();
            Debug.Log("Forced compression cycle completion");
        }
    }
    
    /// <summary>
    /// Clear compression queue
    /// </summary>
    public void ClearQueue()
    {
        if (compressionQueue.Count > 0)
        {
            Debug.Log($"Cleared compression queue of {compressionQueue.Count} items");
            compressionQueue.Clear();
            isCompressing = false;
            cycleTimer = 0f;
        }
    }
    
    /// <summary>
    /// Check if waste type can be compressed
    /// </summary>
    public bool CanCompressWasteType(WasteType wasteType)
    {
        return IsCompressible(wasteType) && 
               maxPressure >= GetRequiredPressure(wasteType);
    }
    
    /// <summary>
    /// Get compression efficiency for waste type
    /// </summary>
    public float GetCompressionEfficiency(WasteType wasteType)
    {
        if (!IsCompressible(wasteType)) return 0f;
        
        float efficiency = 1f - compressionRatio; // Higher compression = higher efficiency
        
        if (preferredWasteTypes.Contains(wasteType))
        {
            efficiency += 0.2f;
        }
        
        return efficiency;
    }
    
    public override string GetStatusInfo()
    {
        string baseStatus = base.GetStatusInfo();
        string cycleStatus = isCompressing ? $"Compressing ({cycleTimer/cycleTime:P})" : $"Queue: {compressionQueue.Count}";
        
        return $"{baseStatus} | {cycleStatus}";
    }
    
    #endregion
} 