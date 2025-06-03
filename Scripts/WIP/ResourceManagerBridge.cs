using UnityEngine;
using System;

/// <summary>
/// Bridge between old ResourceManager API and new resource system
/// Maintains backward compatibility while transitioning to new system
/// </summary>
public class ResourceManagerBridge : MonoBehaviour
{
    public static ResourceManagerBridge Instance { get; private set; }
    
    [Header("Legacy Compatibility Settings")]
    [SerializeField] private bool enableLegacyMode = true;
    [SerializeField] private float rpToPlasticRatio = 10f; // 10 RP = 1 Plastic
    [SerializeField] private float dpToCrystalRatio = 5f;  // 5 DP = 1 Crystal Fragment
    
    [Header("Conversion Rates")]
    [SerializeField] private float plasticToRpRatio = 10f;
    [SerializeField] private float crystalToDpRatio = 5f;
    [SerializeField] private float metalToRpRatio = 20f;
    
    // Legacy properties that delegate to new system
    public float RecyclingPoints => GetLegacyRecyclingPoints();
    public float DimensionalPotential => GetLegacyDimensionalPotential();
    public float ContaminationLevel { get; private set; }
    
    // Legacy events (maintain existing API)
    public event Action<float> OnRecyclingPointsChanged;
    public event Action<float> OnDimensionalPotentialChanged;
    public event Action<float> OnContaminationChanged;
    public event Action OnResourcesChanged;
    
    // Combat resources (delegate to new system)
    public int ShipParts => NewResourceManager.Instance?.GetResourceAmount(ResourceType.Parts) ?? 0;
    public int AlienTech => NewResourceManager.Instance?.GetResourceAmount(ResourceType.CrystalFragments) ?? 0;
    public int CombatData => NewResourceManager.Instance?.GetResourceAmount(ResourceType.NeuralResidue) ?? 0;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SubscribeToNewResourceEvents();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void SubscribeToNewResourceEvents()
    {
        if (NewResourceManager.Instance != null)
        {
            NewResourceManager.Instance.OnResourceChanged += OnNewResourceChanged;
            NewResourceManager.Instance.OnResourceInventoryChanged += OnNewInventoryChanged;
        }
    }
    
    private void OnNewResourceChanged(ResourceType type, int amount)
    {
        // Fire legacy events when relevant resources change
        if (enableLegacyMode)
        {
            switch (type)
            {
                case ResourceType.Plastic:
                case ResourceType.MetalScraps:
                case ResourceType.OrganicMatter:
                    OnRecyclingPointsChanged?.Invoke(GetLegacyRecyclingPoints());
                    break;
                    
                case ResourceType.CrystalFragments:
                case ResourceType.NeuralResidue:
                    OnDimensionalPotentialChanged?.Invoke(GetLegacyDimensionalPotential());
                    break;
            }
            
            OnResourcesChanged?.Invoke();
        }
    }
    
    private void OnNewInventoryChanged()
    {
        if (enableLegacyMode)
        {
            OnRecyclingPointsChanged?.Invoke(GetLegacyRecyclingPoints());
            OnDimensionalPotentialChanged?.Invoke(GetLegacyDimensionalPotential());
            OnResourcesChanged?.Invoke();
        }
    }
    
    #region Legacy API Methods
    
    /// <summary>
    /// Legacy method: Add recycling points (converts to resources)
    /// </summary>
    public void AddRecyclingPoints(float amount)
    {
        if (NewResourceManager.Instance == null) return;
        
        // Convert RP to plastic resources
        int plasticAmount = Mathf.RoundToInt(amount / rpToPlasticRatio);
        if (plasticAmount > 0)
        {
            NewResourceManager.Instance.AddResource(ResourceType.Plastic, plasticAmount);
        }
        
        Debug.Log($"Legacy: Added {amount} RP → {plasticAmount} Plastic");
    }
    
    /// <summary>
    /// Legacy method: Spend recycling points
    /// </summary>
    public bool SpendRecyclingPoints(float amount)
    {
        if (NewResourceManager.Instance == null) return false;
        
        // Calculate how much plastic we need
        int plasticNeeded = Mathf.CeilToInt(amount / rpToPlasticRatio);
        
        // Try to spend plastic first, then other basic resources
        if (NewResourceManager.Instance.GetResourceAmount(ResourceType.Plastic) >= plasticNeeded)
        {
            return NewResourceManager.Instance.SpendResource(ResourceType.Plastic, plasticNeeded);
        }
        
        // Fallback: spend equivalent value in other resources
        return SpendEquivalentResources(amount);
    }
    
    /// <summary>
    /// Legacy method: Add dimensional potential (converts to crystal fragments)
    /// </summary>
    public void AddDimensionalPotential(float amount)
    {
        if (NewResourceManager.Instance == null) return;
        
        // Convert DP to crystal fragments
        int crystalAmount = Mathf.RoundToInt(amount / dpToCrystalRatio);
        if (crystalAmount > 0)
        {
            NewResourceManager.Instance.AddResource(ResourceType.CrystalFragments, crystalAmount);
        }
        
        Debug.Log($"Legacy: Added {amount} DP → {crystalAmount} Crystal Fragments");
    }
    
    /// <summary>
    /// Legacy method: Spend dimensional potential
    /// </summary>
    public bool SpendDimensionalPotential(float amount)
    {
        if (NewResourceManager.Instance == null) return false;
        
        // Calculate how many crystals we need
        int crystalsNeeded = Mathf.CeilToInt(amount / dpToCrystalRatio);
        
        return NewResourceManager.Instance.SpendResource(ResourceType.CrystalFragments, crystalsNeeded);
    }
    
    /// <summary>
    /// Legacy method: Process waste item (converts to new system)
    /// </summary>
    public void ProcessWasteItem(WasteItem item)
    {
        if (item == null || NewResourceManager.Instance == null) return;
        
        // Convert old WasteItem to UpdatedWasteItem
        var updatedItem = WasteItemConverter.ConvertToUpdatedWasteItem(item);
        
        // Process using new system
        if (UpdatedWasteProcessor.Instance != null)
        {
            UpdatedWasteProcessor.Instance.ProcessWasteItem(updatedItem);
        }
        else
        {
            // Fallback: direct resource conversion
            ProcessLegacyWasteItem(item);
        }
    }
    
    /// <summary>
    /// Legacy contamination methods
    /// </summary>
    public void IncreaseContamination(float amount)
    {
        ContaminationLevel += amount;
        OnContaminationChanged?.Invoke(ContaminationLevel);
    }
    
    public void DecreaseContamination(float amount)
    {
        ContaminationLevel = Mathf.Max(0, ContaminationLevel - amount);
        OnContaminationChanged?.Invoke(ContaminationLevel);
    }
    
    /// <summary>
    /// Legacy combat resource methods
    /// </summary>
    public void AddShipParts(int amount)
    {
        NewResourceManager.Instance?.AddResource(ResourceType.Parts, amount);
    }
    
    public void AddAlienTech(int amount)
    {
        NewResourceManager.Instance?.AddResource(ResourceType.CrystalFragments, amount);
    }
    
    public void AddCombatData(int amount)
    {
        NewResourceManager.Instance?.AddResource(ResourceType.NeuralResidue, amount);
    }
    
    public bool SpendShipParts(int amount)
    {
        return NewResourceManager.Instance?.SpendResource(ResourceType.Parts, amount) ?? false;
    }
    
    public bool SpendAlienTech(int amount)
    {
        return NewResourceManager.Instance?.SpendResource(ResourceType.CrystalFragments, amount) ?? false;
    }
    
    public bool SpendCombatData(int amount)
    {
        return NewResourceManager.Instance?.SpendResource(ResourceType.NeuralResidue, amount) ?? false;
    }
    
    #endregion
    
    #region Helper Methods
    
    private float GetLegacyRecyclingPoints()
    {
        if (NewResourceManager.Instance == null) return 0f;
        
        // Convert current resources back to legacy RP for UI compatibility
        float totalRP = 0f;
        totalRP += NewResourceManager.Instance.GetResourceAmount(ResourceType.Plastic) * plasticToRpRatio;
        totalRP += NewResourceManager.Instance.GetResourceAmount(ResourceType.MetalScraps) * metalToRpRatio;
        totalRP += NewResourceManager.Instance.GetResourceAmount(ResourceType.OrganicMatter) * plasticToRpRatio;
        
        return totalRP;
    }
    
    private float GetLegacyDimensionalPotential()
    {
        if (NewResourceManager.Instance == null) return 0f;
        
        // Convert crystal fragments and neural residue to legacy DP
        float totalDP = 0f;
        totalDP += NewResourceManager.Instance.GetResourceAmount(ResourceType.CrystalFragments) * crystalToDpRatio;
        totalDP += NewResourceManager.Instance.GetResourceAmount(ResourceType.NeuralResidue) * crystalToDpRatio;
        
        return totalDP;
    }
    
    private bool SpendEquivalentResources(float rpAmount)
    {
        // Try to spend equivalent value from available resources
        float remainingValue = rpAmount;
        
        // Try metal scraps (worth more RP)
        int metalAvailable = NewResourceManager.Instance.GetResourceAmount(ResourceType.MetalScraps);
        int metalToSpend = Mathf.Min(metalAvailable, Mathf.FloorToInt(remainingValue / metalToRpRatio));
        if (metalToSpend > 0)
        {
            NewResourceManager.Instance.SpendResource(ResourceType.MetalScraps, metalToSpend);
            remainingValue -= metalToSpend * metalToRpRatio;
        }
        
        // Try organic matter
        if (remainingValue > 0)
        {
            int organicNeeded = Mathf.CeilToInt(remainingValue / plasticToRpRatio);
            int organicAvailable = NewResourceManager.Instance.GetResourceAmount(ResourceType.OrganicMatter);
            if (organicAvailable >= organicNeeded)
            {
                NewResourceManager.Instance.SpendResource(ResourceType.OrganicMatter, organicNeeded);
                remainingValue = 0;
            }
        }
        
        return remainingValue <= 0;
    }
    
    private void ProcessLegacyWasteItem(WasteItem item)
    {
        // Fallback processing for when UpdatedWasteProcessor isn't available
        float rpValue = item.RecyclingValue * 10f;
        float dpValue = item.RecyclingPotential * 5f;
        
        AddRecyclingPoints(rpValue);
        AddDimensionalPotential(dpValue);
        
        // Add contamination
        IncreaseContamination(item.ContaminationLevel * 0.1f);
    }
    
    #endregion
    
    #region Public Utility Methods
    
    public float GetRecyclingPoints() => RecyclingPoints;
    public float GetDimensionalPotential() => DimensionalPotential;
    public float GetContamination() => ContaminationLevel;
    
    public void SetRecyclingPoints(float value)
    {
        // Convert to plastic resources
        int targetPlastic = Mathf.RoundToInt(value / rpToPlasticRatio);
        int currentPlastic = NewResourceManager.Instance?.GetResourceAmount(ResourceType.Plastic) ?? 0;
        
        if (targetPlastic > currentPlastic)
        {
            NewResourceManager.Instance?.AddResource(ResourceType.Plastic, targetPlastic - currentPlastic);
        }
    }
    
    public void SetDimensionalPotential(float value)
    {
        // Convert to crystal fragments
        int targetCrystals = Mathf.RoundToInt(value / dpToCrystalRatio);
        int currentCrystals = NewResourceManager.Instance?.GetResourceAmount(ResourceType.CrystalFragments) ?? 0;
        
        if (targetCrystals > currentCrystals)
        {
            NewResourceManager.Instance?.AddResource(ResourceType.CrystalFragments, targetCrystals - currentCrystals);
        }
    }
    
    /// <summary>
    /// Enable or disable legacy compatibility mode
    /// </summary>
    public void SetLegacyMode(bool enabled)
    {
        enableLegacyMode = enabled;
        Debug.Log($"Legacy compatibility mode: {(enabled ? "Enabled" : "Disabled")}");
    }
    
    #endregion
} 