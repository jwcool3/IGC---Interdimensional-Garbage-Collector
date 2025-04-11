using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
    
    private void Start()
    {
        // Subscribe to resource change events
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnRecyclingPointsChanged += UpdateRecyclingPoints;
            ResourceManager.Instance.OnDimensionalPotentialChanged += UpdateDimensionalPotential;
            ResourceManager.Instance.OnContaminationChanged += UpdateContamination;
            
            // Initialize displays
            UpdateRecyclingPoints(ResourceManager.Instance.RecyclingPoints);
            UpdateDimensionalPotential(ResourceManager.Instance.DimensionalPotential);
            UpdateContamination(ResourceManager.Instance.ContaminationLevel);
        }
    }
    
    private void UpdateRecyclingPoints(float amount)
    {
        if (recyclingPointsText != null)
        {
            recyclingPointsText.text = string.Format(recyclingPointsFormat, amount);
        }
    }
    
    private void UpdateDimensionalPotential(float amount)
    {
        if (dimensionalPotentialText != null)
        {
            dimensionalPotentialText.text = string.Format(dimensionalPotentialFormat, amount);
        }
    }
    
    private void UpdateContamination(float level)
    {
        if (contaminationText != null)
        {
            contaminationText.text = string.Format(contaminationFormat, level);
        }
        
        if (contaminationSlider != null)
        {
            contaminationSlider.value = level;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnRecyclingPointsChanged -= UpdateRecyclingPoints;
            ResourceManager.Instance.OnDimensionalPotentialChanged -= UpdateDimensionalPotential;
            ResourceManager.Instance.OnContaminationChanged -= UpdateContamination;
        }
    }
}