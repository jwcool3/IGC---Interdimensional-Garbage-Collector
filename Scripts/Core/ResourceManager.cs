using UnityEngine;
using System;

public class ResourceManager : MonoBehaviour
{
    // Singleton pattern
    public static ResourceManager Instance { get; private set; }

    // Resource values
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

    // Add recycling points and trigger event
    public void AddRecyclingPoints(float amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"Attempting to add negative recycling points: {amount}");
            return;
        }

        float previousValue = recyclingPoints;
        recyclingPoints += amount;
        Debug.Log($"Adding recycling points: {amount:F1} (Previous: {previousValue:F1}, New: {recyclingPoints:F1})");

        OnRecyclingPointsChanged?.Invoke(recyclingPoints);
        OnResourcesChanged?.Invoke();
        
        Debug.Log("Resource change events fired");
    }

    // Spend recycling points if enough are available
    public bool SpendRecyclingPoints(float amount)
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

    // Add dimensional potential and trigger event
    public void AddDimensionalPotential(float amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"Attempting to add negative dimensional potential: {amount}");
            return;
        }

        float previousValue = dimensionalPotential;
        dimensionalPotential += amount;
        Debug.Log($"Adding dimensional potential: {amount:F1} (Previous: {previousValue:F1}, New: {dimensionalPotential:F1})");

        OnDimensionalPotentialChanged?.Invoke(dimensionalPotential);
        OnResourcesChanged?.Invoke();
        
        Debug.Log("Resource change events fired");
    }

    // Spend dimensional potential if enough is available
    public bool SpendDimensionalPotential(float amount)
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
        recyclingPoints = value;
        OnRecyclingPointsChanged?.Invoke(recyclingPoints);
    }

    public void SetDimensionalPotential(float value)
    {
        dimensionalPotential = value;
        OnDimensionalPotentialChanged?.Invoke(dimensionalPotential);
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

        // Add contamination (affected by danger level and reduction modifier)
        float contaminationAmount = item.ContaminationLevel * 0.1f;
        if (currentLocation != null)
        {
            contaminationAmount += currentLocation.dangerLevel * 0.05f;
        }
        
        // Apply contamination reduction
        contaminationAmount *= (1f - contaminationReductionModifier);
        IncreaseContamination(contaminationAmount);

        Debug.Log($"Processed {item.Name} for {finalRecyclingPoints:F1} RP (location: {locationMultiplier:F1}x, recycling: {recyclingMultiplier:F1}x)");
    }
}