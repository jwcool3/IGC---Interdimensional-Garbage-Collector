using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Incinerator Facility - Burns waste to generate energy and handle hazardous materials
/// Specializes in processing dangerous waste types and generating power
/// </summary>
public class IncineratorFacility : ProcessingFacilityBase
{
    [Header("Incinerator Specific Settings")]
    [SerializeField] private float energyGenerationRate = 2.5f;
    [SerializeField] private float hazardousWasteBonus = 0.4f;
    [SerializeField] private float temperatureEfficiency = 1.0f;
    [SerializeField] private float maxTemperature = 1200f;
    [SerializeField] private float currentTemperature = 20f;
    [SerializeField] private float heatupRate = 50f;
    [SerializeField] private float cooldownRate = 25f;
    
    [Header("Safety Systems")]
    [SerializeField] private bool hasEmissionFilters = true;
    [SerializeField] private float filterEfficiency = 0.85f;
    [SerializeField] private bool hasAutomaticShutdown = true;
    [SerializeField] private float maxSafeTemperature = 1000f;
    
    [Header("Specialized Processing")]
    [SerializeField] private List<WasteType> specializedWasteTypes = new List<WasteType>
    {
        WasteType.Hazardous,
        WasteType.Biological,
        WasteType.Chemical,
        WasteType.Radioactive
    };
    
    [Header("Energy Output")]
    [SerializeField] private float baseEnergyPerKg = 15f;
    [SerializeField] private float energyEfficiencyBonus = 0.2f;
    [SerializeField] private ResourceType energyResourceType = ResourceType.Energy;
    
    // Temperature and safety management
    private bool isHeating = false;
    private bool isOverheating = false;
    private float lastProcessingTime = 0f;
    private float idleCooldownDelay = 30f;
    
    // Energy generation tracking
    private float totalEnergyGenerated = 0f;
    private float energyGeneratedThisSession = 0f;
    
    protected override void Awake()
    {
        // Set default values for incinerator facility
        facilityName = "Incinerator Facility";
        facilityType = ProcessingType.Incineration;
        baseEfficiency = 1.0f;
        maxConcurrentJobs = 1;
        energyConsumption = 1.5f; // Higher energy consumption for startup
        
        // Set supported waste types (can handle everything)
        supportedWasteTypes = System.Enum.GetValues(typeof(WasteType)).Cast<WasteType>().ToList();
        
        base.Awake();
    }
    
    protected override void Update()
    {
        base.Update();
        
        UpdateTemperature();
        UpdateSafetySystems();
        GenerateIdleEnergy();
    }
    
    protected override void InitializeFacilityData()
    {
        base.InitializeFacilityData();
        
        // Add incinerator-specific data
        facilityData.facilityName = "Incinerator Facility";
        facilityData.facilityType = ProcessingType.Incineration;
    }
    
    public override bool CanProcessWasteItem(UpdatedWasteItem wasteItem)
    {
        if (!base.CanProcessWasteItem(wasteItem)) return false;
        
        // Check temperature requirements
        if (currentTemperature < GetRequiredTemperature(wasteItem.Type))
            return false;
        
        // Check safety systems for hazardous waste
        if (IsHazardousWaste(wasteItem.Type) && !hasEmissionFilters)
            return false;
        
        // Check overheating
        if (isOverheating)
            return false;
        
        return true;
    }
    
    public override bool ProcessWasteItem(UpdatedWasteItem wasteItem, int quantity = 1)
    {
        if (!CanProcessWasteItem(wasteItem))
        {
            Debug.LogWarning($"Cannot incinerate {wasteItem.Name} - insufficient temperature or safety concerns");
            return false;
        }
        
        // Start heating if not already hot enough
        if (currentTemperature < GetRequiredTemperature(wasteItem.Type))
        {
            StartHeating();
        }
        
        lastProcessingTime = Time.time;
        return base.ProcessWasteItem(wasteItem, quantity);
    }
    
    protected override float GetProcessingModifier(UpdatedWasteItem wasteItem)
    {
        float modifier = base.GetProcessingModifier(wasteItem);
        
        // Temperature efficiency bonus
        float tempEfficiency = Mathf.Clamp01(currentTemperature / maxTemperature);
        modifier += tempEfficiency * temperatureEfficiency;
        
        // Bonus for specialized waste types
        if (specializedWasteTypes.Contains(wasteItem.Type))
        {
            modifier += hazardousWasteBonus;
        }
        
        // Energy generation bonus
        modifier += energyEfficiencyBonus;
        
        // Penalty for contamination (less effective than recycling)
        float contaminationPenalty = wasteItem.ContaminationLevel * 0.1f;
        modifier -= contaminationPenalty;
        
        return Mathf.Max(0.1f, modifier);
    }
    
    protected override void OnFacilityUpgraded()
    {
        base.OnFacilityUpgraded();
        
        // Incinerator-specific upgrades
        maxTemperature += 100f;
        heatupRate += 10f;
        energyGenerationRate += 0.5f;
        
        // Improve safety systems
        if (facilityLevel >= 2 && !hasEmissionFilters)
        {
            hasEmissionFilters = true;
            filterEfficiency = 0.75f;
            Debug.Log("Emission filters installed");
        }
        
        if (facilityLevel >= 3)
        {
            filterEfficiency = Mathf.Min(0.95f, filterEfficiency + 0.1f);
        }
        
        if (facilityLevel >= 4 && !hasAutomaticShutdown)
        {
            hasAutomaticShutdown = true;
            Debug.Log("Automatic shutdown system installed");
        }
        
        Debug.Log($"Incinerator upgraded - Max Temp: {maxTemperature}°C, Energy Rate: {energyGenerationRate}");
    }
    
    protected override void OnProcessingCompleted(ProcessingJob job, Dictionary<ResourceType, int> yield)
    {
        base.OnProcessingCompleted(job, yield);
        
        // Generate energy from incineration
        float energyGenerated = CalculateEnergyOutput(job.wasteItem, job.quantity);
        
        if (energyGenerated > 0)
        {
            if (yield.ContainsKey(energyResourceType))
                yield[energyResourceType] += Mathf.FloorToInt(energyGenerated);
            else
                yield[energyResourceType] = Mathf.FloorToInt(energyGenerated);
            
            totalEnergyGenerated += energyGenerated;
            energyGeneratedThisSession += energyGenerated;
        }
        
        // Reduce ash and waste volume
        ReduceWasteVolume(yield);
        
        // Special incineration completion effects
        PlayIncinerationEffects();
    }
    
    #region Temperature Management
    
    private void UpdateTemperature()
    {
        if (isHeating && !isOverheating)
        {
            // Heat up when processing
            currentTemperature += heatupRate * Time.deltaTime;
            
            if (currentTemperature >= maxTemperature)
            {
                currentTemperature = maxTemperature;
                isHeating = false;
            }
        }
        else if (Time.time - lastProcessingTime > idleCooldownDelay)
        {
            // Cool down when idle
            currentTemperature -= cooldownRate * Time.deltaTime;
            currentTemperature = Mathf.Max(20f, currentTemperature);
        }
        
        // Update temperature efficiency
        temperatureEfficiency = Mathf.Clamp01(currentTemperature / maxTemperature);
    }
    
    private void UpdateSafetySystems()
    {
        // Check for overheating
        bool wasOverheating = isOverheating;
        isOverheating = currentTemperature > maxSafeTemperature;
        
        if (isOverheating && !wasOverheating)
        {
            Debug.LogWarning("Incinerator overheating! Automatic shutdown activated.");
            
            if (hasAutomaticShutdown)
            {
                EmergencyShutdown();
            }
        }
        else if (!isOverheating && wasOverheating)
        {
            Debug.Log("Incinerator temperature normalized. Resuming operations.");
        }
    }
    
    private void StartHeating()
    {
        isHeating = true;
        Debug.Log("Incinerator heating up...");
    }
    
    private void EmergencyShutdown()
    {
        isHeating = false;
        isOperational = false;
        
        // Force cooldown
        currentTemperature = Mathf.Min(currentTemperature, maxSafeTemperature);
        
        Debug.Log("Emergency shutdown activated!");
        
        // Auto-restart after cooling down
        Invoke(nameof(RestartAfterCooldown), 60f);
    }
    
    private void RestartAfterCooldown()
    {
        if (currentTemperature < maxSafeTemperature * 0.8f)
        {
            isOperational = true;
            isOverheating = false;
            Debug.Log("Incinerator restarted after cooldown");
        }
        else
        {
            // Try again later
            Invoke(nameof(RestartAfterCooldown), 30f);
        }
    }
    
    #endregion
    
    #region Energy Generation
    
    private float CalculateEnergyOutput(UpdatedWasteItem wasteItem, int quantity)
    {
        float baseEnergy = baseEnergyPerKg * wasteItem.Weight * quantity;
        
        // Apply temperature efficiency
        baseEnergy *= temperatureEfficiency;
        
        // Apply facility efficiency
        baseEnergy *= currentEfficiency;
        
        // Bonus for specialized waste types
        if (specializedWasteTypes.Contains(wasteItem.Type))
        {
            baseEnergy *= (1f + hazardousWasteBonus);
        }
        
        // Apply energy generation rate
        baseEnergy *= energyGenerationRate;
        
        return baseEnergy;
    }
    
    private void GenerateIdleEnergy()
    {
        // Generate small amount of energy when hot but idle
        if (currentTemperature > 200f && !isProcessing)
        {
            float idleEnergy = (currentTemperature / maxTemperature) * 0.1f * Time.deltaTime;
            
            if (resourceManager != null)
            {
                resourceManager.AddResource(energyResourceType, Mathf.FloorToInt(idleEnergy));
                totalEnergyGenerated += idleEnergy;
            }
        }
    }
    
    #endregion
    
    #region Waste Processing
    
    private void ReduceWasteVolume(Dictionary<ResourceType, int> yield)
    {
        // Incineration significantly reduces waste volume
        // Convert some waste to ash
        int totalYield = yield.Values.Sum();
        int ashAmount = Mathf.FloorToInt(totalYield * 0.1f); // 10% becomes ash
        
        if (ashAmount > 0)
        {
            if (yield.ContainsKey(ResourceType.Ash))
                yield[ResourceType.Ash] += ashAmount;
            else
                yield[ResourceType.Ash] = ashAmount;
        }
    }
    
    private float GetRequiredTemperature(WasteType wasteType)
    {
        switch (wasteType)
        {
            case WasteType.Paper:
                return 200f;
            case WasteType.Plastic:
                return 300f;
            case WasteType.Organic:
                return 250f;
            case WasteType.Biological:
                return 800f;
            case WasteType.Chemical:
                return 900f;
            case WasteType.Hazardous:
                return 1000f;
            case WasteType.Radioactive:
                return 1100f;
            default:
                return 400f;
        }
    }
    
    private bool IsHazardousWaste(WasteType wasteType)
    {
        return specializedWasteTypes.Contains(wasteType);
    }
    
    #endregion
    
    #region Visual Effects
    
    private void PlayIncinerationEffects()
    {
        // Play incineration-specific particle effects
        if (processingEffect != null)
        {
            var main = processingEffect.main;
            main.startColor = Color.red;
            processingEffect.Emit(30);
        }
        
        // Play incineration sound
        if (processingAudio != null && processingAudio.clip != null)
        {
            processingAudio.pitch = Random.Range(0.8f, 1.2f);
            processingAudio.Play();
        }
    }
    
    protected override void UpdateVisualEffects()
    {
        base.UpdateVisualEffects();
        
        // Update temperature-based visual effects
        if (statusLight != null)
        {
            if (isOverheating)
            {
                statusLight.color = Color.red;
                statusLight.intensity = 2f;
            }
            else if (currentTemperature > 500f)
            {
                // Hot operation - orange glow
                statusLight.color = Color.Lerp(operationalColor, Color.red, currentTemperature / maxTemperature);
                statusLight.intensity = 1f + (currentTemperature / maxTemperature);
            }
        }
        
        // Update flame effects based on temperature
        if (processingEffect != null && currentTemperature > 100f)
        {
            var emission = processingEffect.emission;
            emission.rateOverTime = (currentTemperature / maxTemperature) * 50f;
            
            var main = processingEffect.main;
            main.startColor = Color.Lerp(Color.yellow, Color.red, currentTemperature / maxTemperature);
        }
    }
    
    #endregion
    
    #region Public Interface Extensions
    
    /// <summary>
    /// Get current temperature status
    /// </summary>
    public string GetTemperatureStatus()
    {
        if (isOverheating)
            return $"OVERHEATING: {currentTemperature:F0}°C (MAX: {maxSafeTemperature:F0}°C)";
        
        return $"Temperature: {currentTemperature:F0}°C / {maxTemperature:F0}°C ({temperatureEfficiency:P})";
    }
    
    /// <summary>
    /// Get energy generation statistics
    /// </summary>
    public string GetEnergyStats()
    {
        return $"Total Energy: {totalEnergyGenerated:F1} | Session: {energyGeneratedThisSession:F1}";
    }
    
    /// <summary>
    /// Force emergency shutdown
    /// </summary>
    public void ForceShutdown()
    {
        EmergencyShutdown();
        Debug.Log("Manual emergency shutdown activated");
    }
    
    /// <summary>
    /// Preheat the incinerator
    /// </summary>
    public void Preheat(float targetTemperature = 0f)
    {
        if (targetTemperature <= 0f)
            targetTemperature = maxTemperature * 0.8f;
        
        StartHeating();
        Debug.Log($"Preheating incinerator to {targetTemperature:F0}°C");
    }
    
    /// <summary>
    /// Check if incinerator can handle specific waste type
    /// </summary>
    public bool CanHandleWasteType(WasteType wasteType)
    {
        float requiredTemp = GetRequiredTemperature(wasteType);
        bool hasRequiredSafety = !IsHazardousWaste(wasteType) || hasEmissionFilters;
        
        return maxTemperature >= requiredTemp && hasRequiredSafety;
    }
    
    public override string GetStatusInfo()
    {
        string baseStatus = base.GetStatusInfo();
        string tempStatus = $"Temp: {currentTemperature:F0}°C";
        
        if (isOverheating)
            tempStatus = "OVERHEATING";
        else if (isHeating)
            tempStatus += " (Heating)";
        
        return $"{baseStatus} | {tempStatus}";
    }
    
    #endregion
} 