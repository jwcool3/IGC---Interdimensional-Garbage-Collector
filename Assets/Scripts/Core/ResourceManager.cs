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
    
    // Properties with public getters
    public float RecyclingPoints => recyclingPoints;
    public float DimensionalPotential => dimensionalPotential;
    public float ContaminationLevel => contamination;
    
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
        recyclingPoints += amount;
        Debug.Log($"Added {amount:F1} recycling points. Total: {recyclingPoints:F1}");
        OnRecyclingPointsChanged?.Invoke(recyclingPoints);
    }
    
    // Spend recycling points if enough are available
    public bool SpendRecyclingPoints(float amount)
    {
        if (recyclingPoints >= amount)
        {
            recyclingPoints -= amount;
            OnRecyclingPointsChanged?.Invoke(recyclingPoints);
            return true;
        }
        return false;
    }
    
    // Add dimensional potential and trigger event
    public void AddDimensionalPotential(float amount)
    {
        dimensionalPotential += amount;
        Debug.Log($"Added {amount:F1} dimensional potential. Total: {dimensionalPotential:F1}");
        OnDimensionalPotentialChanged?.Invoke(dimensionalPotential);
    }
    
    // Spend dimensional potential if enough is available
    public bool SpendDimensionalPotential(float amount)
    {
        if (dimensionalPotential >= amount)
        {
            dimensionalPotential -= amount;
            OnDimensionalPotentialChanged?.Invoke(dimensionalPotential);
            return true;
        }
        return false;
    }
    
    // Increase contamination level
    public void IncreaseContamination(float amount)
    {
        contamination = Mathf.Min(contamination + amount, maxContamination);
        Debug.Log($"Contamination increased by {amount:F1}. Total: {contamination:F1}");
        OnContaminationChanged?.Invoke(contamination);
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
}