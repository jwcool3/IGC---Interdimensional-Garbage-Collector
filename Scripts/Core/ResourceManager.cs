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
        if (amount < 0)
        {
            Debug.LogWarning($"Attempting to add negative recycling points: {amount}");
            return;
        }

        float previousValue = recyclingPoints;
        recyclingPoints += amount;
        Debug.Log($"Adding recycling points: {amount:F1} (Previous: {previousValue:F1}, New: {recyclingPoints:F1})");

        if (OnRecyclingPointsChanged != null)
        {
            OnRecyclingPointsChanged.Invoke(recyclingPoints);
            Debug.Log("OnRecyclingPointsChanged event fired");
        }
        else
        {
            Debug.LogWarning("No listeners for OnRecyclingPointsChanged event");
        }
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
        if (amount < 0)
        {
            Debug.LogWarning($"Attempting to add negative dimensional potential: {amount}");
            return;
        }

        float previousValue = dimensionalPotential;
        dimensionalPotential += amount;
        Debug.Log($"Adding dimensional potential: {amount:F1} (Previous: {previousValue:F1}, New: {dimensionalPotential:F1})");

        if (OnDimensionalPotentialChanged != null)
        {
            OnDimensionalPotentialChanged.Invoke(dimensionalPotential);
            Debug.Log("OnDimensionalPotentialChanged event fired");
        }
        else
        {
            Debug.LogWarning("No listeners for OnDimensionalPotentialChanged event");
        }
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
}