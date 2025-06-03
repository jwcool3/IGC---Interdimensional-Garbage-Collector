using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ResourceDisplay : MonoBehaviour
{
    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI recyclingPointsText;
    [SerializeField] private TextMeshProUGUI dimensionalPotentialText;
    [SerializeField] private TextMeshProUGUI contaminationText;

    [Header("Slider Components")]
    [SerializeField] private Slider contaminationSlider;

    [Header("Formatting")]
    [SerializeField] private string recyclingPointsFormat = "RP: {0:N0}";
    [SerializeField] private string dimensionalPotentialFormat = "DP: {0:N1}";
    [SerializeField] private string contaminationFormat = "Contamination: {0:P0}";

    private void Awake()
    {
        // Check if components are assigned
        if (recyclingPointsText == null) Debug.LogError("ResourceDisplay: recyclingPointsText not assigned!");
        if (dimensionalPotentialText == null) Debug.LogError("ResourceDisplay: dimensionalPotentialText not assigned!");
        if (contaminationText == null) Debug.LogError("ResourceDisplay: contaminationText not assigned!");
    }

    private void Start()
    {
        Debug.Log("ResourceDisplay: Starting initialization...");

        // Try immediate subscription
        TrySubscribeToEvents();

        // Also try with a delay in case managers initialize after this script
        StartCoroutine(DelayedSubscribe());
    }

    private void TrySubscribeToEvents()
    {
        if (ResourceManager.Instance != null)
        {
            Debug.Log("ResourceDisplay: Found ResourceManager instance, subscribing to events");

            // Subscribe to resource change events
            ResourceManager.Instance.OnRecyclingPointsChanged += UpdateRecyclingPoints;
            ResourceManager.Instance.OnDimensionalPotentialChanged += UpdateDimensionalPotential;
            ResourceManager.Instance.OnContaminationChanged += UpdateContamination;

            // Initialize displays with current values
            UpdateRecyclingPoints(ResourceManager.Instance.RecyclingPoints);
            UpdateDimensionalPotential(ResourceManager.Instance.DimensionalPotential);
            UpdateContamination(ResourceManager.Instance.ContaminationLevel);

            Debug.Log($"ResourceDisplay: Initial values - RP: {ResourceManager.Instance.RecyclingPoints}, " +
                     $"DP: {ResourceManager.Instance.DimensionalPotential}, " +
                     $"Contamination: {ResourceManager.Instance.ContaminationLevel}");
        }
        else
        {
            Debug.LogWarning("ResourceDisplay: ResourceManager.Instance is null during initialization!");
        }
    }

    private IEnumerator DelayedSubscribe()
    {
        // Wait a moment to ensure all managers are initialized
        yield return new WaitForSeconds(0.5f);

        if (ResourceManager.Instance != null)
        {
            Debug.Log("ResourceDisplay: Delayed subscription to ResourceManager events");

            // Subscribe to events again (the += operator will avoid duplicate subscriptions)
            ResourceManager.Instance.OnRecyclingPointsChanged += UpdateRecyclingPoints;
            ResourceManager.Instance.OnDimensionalPotentialChanged += UpdateDimensionalPotential;
            ResourceManager.Instance.OnContaminationChanged += UpdateContamination;

            // Refresh displays with current values
            UpdateRecyclingPoints(ResourceManager.Instance.RecyclingPoints);
            UpdateDimensionalPotential(ResourceManager.Instance.DimensionalPotential);
            UpdateContamination(ResourceManager.Instance.ContaminationLevel);
        }
        else
        {
            Debug.LogError("ResourceDisplay: ResourceManager.Instance still null after delay!");
        }
    }

    private void UpdateRecyclingPoints(float amount)
    {
        if (recyclingPointsText != null)
        {
            recyclingPointsText.text = string.Format(recyclingPointsFormat, amount);
            Debug.Log($"ResourceDisplay: Updated recycling points display to {amount}");
        }
        else
        {
            Debug.LogError("ResourceDisplay: Cannot update null recyclingPointsText!");
        }
    }

    private void UpdateDimensionalPotential(float amount)
    {
        if (dimensionalPotentialText != null)
        {
            dimensionalPotentialText.text = string.Format(dimensionalPotentialFormat, amount);
            Debug.Log($"ResourceDisplay: Updated dimensional potential display to {amount}");
        }
        else
        {
            Debug.LogError("ResourceDisplay: Cannot update null dimensionalPotentialText!");
        }
    }

    private void UpdateContamination(float level)
    {
        if (contaminationText != null)
        {
            contaminationText.text = string.Format(contaminationFormat, level);
            Debug.Log($"ResourceDisplay: Updated contamination text to {level:P0}");
        }
        else
        {
            Debug.LogError("ResourceDisplay: Cannot update null contaminationText!");
        }

        if (contaminationSlider != null)
        {
            contaminationSlider.value = level;
            Debug.Log($"ResourceDisplay: Updated contamination slider to {level}");
        }
    }

    // For debugging - force update the display with current values
    public void ForceUpdateDisplay()
    {
        if (ResourceManager.Instance != null)
        {
            Debug.Log("ResourceDisplay: Forcing update of displays");
            UpdateRecyclingPoints(ResourceManager.Instance.RecyclingPoints);
            UpdateDimensionalPotential(ResourceManager.Instance.DimensionalPotential);
            UpdateContamination(ResourceManager.Instance.ContaminationLevel);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (ResourceManager.Instance != null)
        {
            Debug.Log("ResourceDisplay: Unsubscribing from ResourceManager events");
            ResourceManager.Instance.OnRecyclingPointsChanged -= UpdateRecyclingPoints;
            ResourceManager.Instance.OnDimensionalPotentialChanged -= UpdateDimensionalPotential;
            ResourceManager.Instance.OnContaminationChanged -= UpdateContamination;
        }
    }
}