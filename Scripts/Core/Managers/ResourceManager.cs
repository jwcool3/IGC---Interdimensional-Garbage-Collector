using UnityEngine;
using System;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    // Singleton pattern
    public static ResourceManager Instance { get; private set; }

    // Resource values (legacy)
    private float recyclingPoints;
    private float dimensionalPotential;
    private float contamination;

    // Resource limits
    [SerializeField] private float maxContamination = 1.0f;

    // Events for UI updates
    public event Action<float> OnRecyclingPointsChanged;
    public event Action<float> OnDimensionalPotentialChanged;
    public event Action<float> OnContaminationChanged;
    public event Action OnResourcesChanged; // General event for any resource change

    // Properties with public getters
    public float RecyclingPoints => recyclingPoints;
    public float DimensionalPotential => dimensionalPotential;
    public float ContaminationLevel => contamination;

    // Effect modifiers
    private float recyclingMultiplier = 1f;
    private float contaminationReductionModifier = 0f;

    // Combat resources
    public int ShipParts { get; private set; }
    public int AlienTech { get; private set; }
    public int CombatData { get; private set; }

    // Bridge to new resource system
    private NewResourceManager newResourceManager;
    private bool useNewResourceSystem = false;

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

    private void Start()
    {
        // Try to connect to new resource system
        InitializeNewResourceSystem();
    }

    private void InitializeNewResourceSystem()
    {
        // Check if NewResourceManager exists
        newResourceManager = NewResourceManager.Instance;
        
        if (newResourceManager != null)
        {
            useNewResourceSystem = true;
            
            // Sync existing values to new system
            SyncToNewResourceSystem();
            
            // Subscribe to new system events
            newResourceManager.OnResourceChanged += OnNewResourceChanged;
            
            Debug.Log("ResourceManager: Connected to new resource system");
        }
        else
        {
            Debug.Log("ResourceManager: Using legacy resource system");
        }
    }

    private void SyncToNewResourceSystem()
    {
        if (!useNewResourceSystem) return;

        // Transfer existing resources to new system
        if (recyclingPoints > 0)
        {
            newResourceManager.AddResource(ResourceType.RecyclingPoints, Mathf.FloorToInt(recyclingPoints));
        }
        
        if (dimensionalPotential > 0)
        {
            newResourceManager.AddResource(ResourceType.DimensionalPotential, Mathf.FloorToInt(dimensionalPotential));
        }
        
        if (ShipParts > 0)
        {
            newResourceManager.AddResource(ResourceType.ShipParts, ShipParts);
        }
        
        if (AlienTech > 0)
        {
            newResourceManager.AddResource(ResourceType.AlienTech, AlienTech);
        }
        
        if (CombatData > 0)
        {
            newResourceManager.AddResource(ResourceType.CombatData, CombatData);
        }
    }

    private void OnNewResourceChanged(ResourceType resourceType, int oldAmount, int newAmount)
    {
        // Sync changes from new system back to legacy values
        switch (resourceType)
        {
            case ResourceType.RecyclingPoints:
                recyclingPoints = newAmount;
                OnRecyclingPointsChanged?.Invoke(recyclingPoints);
                break;
                
            case ResourceType.DimensionalPotential:
                dimensionalPotential = newAmount;
                OnDimensionalPotentialChanged?.Invoke(dimensionalPotential);
                break;
                
            case ResourceType.ShipParts:
                ShipParts = newAmount;
                break;
                
            case ResourceType.AlienTech:
                AlienTech = newAmount;
                break;
                
            case ResourceType.CombatData:
                CombatData = newAmount;
                break;
        }
        
        OnResourcesChanged?.Invoke();
    }

    // Add recycling points and trigger event
    public void AddRecyclingPoints(float amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"Attempting to add negative recycling points: {amount}");
            return;
        }

        if (useNewResourceSystem)
        {
            newResourceManager.AddResource(ResourceType.RecyclingPoints, Mathf.FloorToInt(amount));
        }
        else
        {
            float previousValue = recyclingPoints;
            recyclingPoints += amount;
            Debug.Log($"Adding recycling points: {amount:F1} (Previous: {previousValue:F1}, New: {recyclingPoints:F1})");

            OnRecyclingPointsChanged?.Invoke(recyclingPoints);
            OnResourcesChanged?.Invoke();
        }
    }

    // Spend recycling points if enough are available
    public bool SpendRecyclingPoints(float amount)
    {
        if (useNewResourceSystem)
        {
            return newResourceManager.SpendResource(ResourceType.RecyclingPoints, Mathf.FloorToInt(amount));
        }
        else
        {
            if (recyclingPoints >= amount)
            {
                recyclingPoints -= amount;
                OnRecyclingPointsChanged?.Invoke(recyclingPoints);
                OnResourcesChanged?.Invoke();
                return true;
            }
            return false;
        }
    }

    // Add dimensional potential and trigger event
    public void AddDimensionalPotential(float amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"Attempting to add negative dimensional potential: {amount}");
            return;
        }

        if (useNewResourceSystem)
        {
            newResourceManager.AddResource(ResourceType.DimensionalPotential, Mathf.FloorToInt(amount));
        }
        else
        {
            float previousValue = dimensionalPotential;
            dimensionalPotential += amount;
            Debug.Log($"Adding dimensional potential: {amount:F1} (Previous: {previousValue:F1}, New: {dimensionalPotential:F1})");

            OnDimensionalPotentialChanged?.Invoke(dimensionalPotential);
            OnResourcesChanged?.Invoke();
        }
    }

    // Spend dimensional potential if enough is available
    public bool SpendDimensionalPotential(float amount)
    {
        if (useNewResourceSystem)
        {
            return newResourceManager.SpendResource(ResourceType.DimensionalPotential, Mathf.FloorToInt(amount));
        }
        else
        {
            if (dimensionalPotential >= amount)
            {
                dimensionalPotential -= amount;
                OnDimensionalPotentialChanged?.Invoke(dimensionalPotential);
                OnResourcesChanged?.Invoke();
                return true;
            }
            return false;
        }
    }

    // Increase contamination level
    public void IncreaseContamination(float amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"Attempting to increase contamination by negative amount: {amount}");
            return;
        }

        float previousValue = contamination;
        contamination = Mathf.Min(contamination + amount, maxContamination);
        Debug.Log($"Increasing contamination: {amount:F1} (Previous: {previousValue:F1}, New: {contamination:F1}, Max: {maxContamination:F1})");

        if (OnContaminationChanged != null)
        {
            OnContaminationChanged.Invoke(contamination);
            Debug.Log("OnContaminationChanged event fired");
        }
        else
        {
            Debug.LogWarning("No listeners for OnContaminationChanged event");
        }
    }

    // Decrease contamination level
    public void DecreaseContamination(float amount)
    {
        contamination = Mathf.Max(contamination - amount, 0f);
        OnContaminationChanged?.Invoke(contamination);
    }

    public float GetRecyclingPoints() => recyclingPoints;
    public float GetDimensionalPotential() => dimensionalPotential;
    public float GetContamination() => contamination;

    public void SetRecyclingPoints(float value)
    {
        if (useNewResourceSystem)
        {
            newResourceManager.SetResource(ResourceType.RecyclingPoints, Mathf.FloorToInt(value));
        }
        else
        {
            recyclingPoints = value;
            OnRecyclingPointsChanged?.Invoke(recyclingPoints);
        }
    }

    public void SetDimensionalPotential(float value)
    {
        if (useNewResourceSystem)
        {
            newResourceManager.SetResource(ResourceType.DimensionalPotential, Mathf.FloorToInt(value));
        }
        else
        {
            dimensionalPotential = value;
            OnDimensionalPotentialChanged?.Invoke(dimensionalPotential);
        }
    }

    /// <summary>
    /// Sets the recycling multiplier from the Lab compartment
    /// </summary>
    public void SetRecyclingMultiplier(float multiplier)
    {
        recyclingMultiplier = Mathf.Max(1f, multiplier);
        Debug.Log($"ResourceManager: Recycling multiplier set to {recyclingMultiplier:F2}x");
    }

    /// <summary>
    /// Sets the contamination reduction modifier from the Recycling compartment
    /// </summary>
    public void SetContaminationReductionModifier(float modifier)
    {
        contaminationReductionModifier = Mathf.Clamp01(modifier);
        Debug.Log($"ResourceManager: Contamination reduction set to {contaminationReductionModifier:P0}");
    }

    // Modify the existing ProcessWasteItem method to use the recycling multiplier
    public void ProcessWasteItem(WasteItem item)
    {
        if (item == null) return;

        // Get current location for multipliers
        LocationData currentLocation = LocationManager.Instance?.GetCurrentLocation();
        float locationMultiplier = currentLocation?.averageValueMultiplier ?? 1f;

        // Calculate base values with recycling multiplier
        float baseRecyclingPoints = item.RecyclingValue * 10f * recyclingMultiplier;
        float baseDimensionalPotential = item.RecyclingPotential * 5f * recyclingMultiplier;

        // Apply location multiplier
        float finalRecyclingPoints = baseRecyclingPoints * locationMultiplier;
        float finalDimensionalPotential = baseDimensionalPotential * locationMultiplier;

        // Add resources
        AddRecyclingPoints(finalRecyclingPoints);
        AddDimensionalPotential(finalDimensionalPotential);

        // Apply contamination reduction
        float contaminationIncrease = item.ContaminationLevel * 0.1f;
        contaminationIncrease *= (1f - contaminationReductionModifier);
        IncreaseContamination(contaminationIncrease);

        Debug.Log($"Processed {item.Name}: +{finalRecyclingPoints:F1} RP, +{finalDimensionalPotential:F1} DP, +{contaminationIncrease:F3} contamination");
    }

    // Process UpdatedWasteItem (new system integration)
    public void ProcessWasteItem(UpdatedWasteItem item)
    {
        if (item == null) return;

        if (useNewResourceSystem)
        {
            // Use the new processing system
            var processingManager = ResourceProcessingManager.Instance;
            if (processingManager != null)
            {
                processingManager.ProcessWasteItem(item, item.Quantity);
            }
        }
        else
        {
            // Convert to legacy processing
            ProcessLegacyWasteItem(item);
        }
    }

    private void ProcessLegacyWasteItem(UpdatedWasteItem item)
    {
        // Convert UpdatedWasteItem to legacy processing
        float baseRecyclingPoints = item.EstimatedValue * recyclingMultiplier;
        float baseDimensionalPotential = item.EstimatedValue * 0.5f * recyclingMultiplier;

        // Apply quality and condition modifiers
        float qualityModifier = (float)item.Quality / 4f; // 0.25 to 1.0
        float conditionModifier = 1f - (item.ContaminationLevel * 0.5f);

        float finalRecyclingPoints = baseRecyclingPoints * qualityModifier * conditionModifier;
        float finalDimensionalPotential = baseDimensionalPotential * qualityModifier * conditionModifier;

        // Add resources
        AddRecyclingPoints(finalRecyclingPoints * item.Quantity);
        AddDimensionalPotential(finalDimensionalPotential * item.Quantity);

        // Apply contamination
        float contaminationIncrease = item.ContaminationLevel * 0.1f * item.Quantity;
        contaminationIncrease *= (1f - contaminationReductionModifier);
        IncreaseContamination(contaminationIncrease);

        Debug.Log($"Processed {item.Name} (x{item.Quantity}): +{finalRecyclingPoints * item.Quantity:F1} RP, +{finalDimensionalPotential * item.Quantity:F1} DP");
    }

    public void AddShipParts(int amount)
    {
        if (useNewResourceSystem)
        {
            newResourceManager.AddResource(ResourceType.ShipParts, amount);
        }
        else
        {
            ShipParts += amount;
            OnResourcesChanged?.Invoke();
        }
        Debug.Log($"Added {amount} Ship Parts. Total: {ShipParts}");
    }

    public void AddAlienTech(int amount)
    {
        if (useNewResourceSystem)
        {
            newResourceManager.AddResource(ResourceType.AlienTech, amount);
        }
        else
        {
            AlienTech += amount;
            OnResourcesChanged?.Invoke();
        }
        Debug.Log($"Added {amount} Alien Tech. Total: {AlienTech}");
    }

    public void AddCombatData(int amount)
    {
        if (useNewResourceSystem)
        {
            newResourceManager.AddResource(ResourceType.CombatData, amount);
        }
        else
        {
            CombatData += amount;
            OnResourcesChanged?.Invoke();
        }
        Debug.Log($"Added {amount} Combat Data. Total: {CombatData}");
    }

    public bool SpendShipParts(int amount)
    {
        if (useNewResourceSystem)
        {
            return newResourceManager.SpendResource(ResourceType.ShipParts, amount);
        }
        else
        {
            if (ShipParts >= amount)
            {
                ShipParts -= amount;
                OnResourcesChanged?.Invoke();
                Debug.Log($"Spent {amount} Ship Parts. Remaining: {ShipParts}");
                return true;
            }
            Debug.LogWarning($"Not enough Ship Parts. Required: {amount}, Available: {ShipParts}");
            return false;
        }
    }

    public bool SpendAlienTech(int amount)
    {
        if (useNewResourceSystem)
        {
            return newResourceManager.SpendResource(ResourceType.AlienTech, amount);
        }
        else
        {
            if (AlienTech >= amount)
            {
                AlienTech -= amount;
                OnResourcesChanged?.Invoke();
                Debug.Log($"Spent {amount} Alien Tech. Remaining: {AlienTech}");
                return true;
            }
            Debug.LogWarning($"Not enough Alien Tech. Required: {amount}, Available: {AlienTech}");
            return false;
        }
    }

    public bool SpendCombatData(int amount)
    {
        if (useNewResourceSystem)
        {
            return newResourceManager.SpendResource(ResourceType.CombatData, amount);
        }
        else
        {
            if (CombatData >= amount)
            {
                CombatData -= amount;
                OnResourcesChanged?.Invoke();
                Debug.Log($"Spent {amount} Combat Data. Remaining: {CombatData}");
                return true;
            }
            Debug.LogWarning($"Not enough Combat Data. Required: {amount}, Available: {CombatData}");
            return false;
        }
    }

    // New resource system integration methods
    public bool HasResource(ResourceType resourceType, int amount)
    {
        if (useNewResourceSystem)
        {
            return newResourceManager.HasResource(resourceType, amount);
        }
        else
        {
            // Legacy fallback
            switch (resourceType)
            {
                case ResourceType.RecyclingPoints:
                    return recyclingPoints >= amount;
                case ResourceType.DimensionalPotential:
                    return dimensionalPotential >= amount;
                case ResourceType.ShipParts:
                    return ShipParts >= amount;
                case ResourceType.AlienTech:
                    return AlienTech >= amount;
                case ResourceType.CombatData:
                    return CombatData >= amount;
                default:
                    return false;
            }
        }
    }

    public int GetResourceAmount(ResourceType resourceType)
    {
        if (useNewResourceSystem)
        {
            return newResourceManager.GetResourceAmount(resourceType);
        }
        else
        {
            // Legacy fallback
            switch (resourceType)
            {
                case ResourceType.RecyclingPoints:
                    return Mathf.FloorToInt(recyclingPoints);
                case ResourceType.DimensionalPotential:
                    return Mathf.FloorToInt(dimensionalPotential);
                case ResourceType.ShipParts:
                    return ShipParts;
                case ResourceType.AlienTech:
                    return AlienTech;
                case ResourceType.CombatData:
                    return CombatData;
                default:
                    return 0;
            }
        }
    }

    public Dictionary<ResourceType, int> GetAllResources()
    {
        if (useNewResourceSystem)
        {
            return newResourceManager.GetAllResources();
        }
        else
        {
            // Legacy fallback
            return new Dictionary<ResourceType, int>
            {
                { ResourceType.RecyclingPoints, Mathf.FloorToInt(recyclingPoints) },
                { ResourceType.DimensionalPotential, Mathf.FloorToInt(dimensionalPotential) },
                { ResourceType.ShipParts, ShipParts },
                { ResourceType.AlienTech, AlienTech },
                { ResourceType.CombatData, CombatData }
            };
        }
    }

    private void OnDestroy()
    {
        if (newResourceManager != null)
        {
            newResourceManager.OnResourceChanged -= OnNewResourceChanged;
        }
    }
}