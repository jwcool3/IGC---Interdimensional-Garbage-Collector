using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for showing active crafting jobs
/// </summary>
public class CraftingJobDisplay : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI jobNameText;
    [SerializeField] private TextMeshProUGUI facilityNameText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI timeRemainingText;
    [SerializeField] private Button cancelButton;
    
    private CraftingJob currentJob;
    
    public void Initialize(CraftingJob job)
    {
        currentJob = job;
        
        // Setup UI
        if (jobNameText != null)
            jobNameText.text = $"{job.batchSize}x {job.recipe.recipeName}";
            
        if (facilityNameText != null)
            facilityNameText.text = job.facility.FacilityName;
        
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
        }
        
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelJob);
        }
        
        // Subscribe to completion
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.OnCraftingCompleted += OnJobCompleted;
        }
    }
    
    private void Update()
    {
        if (currentJob == null) return;
        
        float progress = currentJob.Progress;
        
        // Update progress bar
        if (progressSlider != null)
            progressSlider.value = progress;
        
        // Update time remaining
        if (timeRemainingText != null)
        {
            float timeRemaining = Mathf.Max(0, currentJob.duration - (Time.time - currentJob.startTime));
            timeRemainingText.text = $"{timeRemaining:F1}s";
        }
        
        // Check if completed
        if (currentJob.IsComplete)
        {
            DestroyJobDisplay();
        }
    }
    
    private void CancelJob()
    {
        // TODO: Implement job cancellation
        Debug.Log($"Cancelling crafting job: {currentJob.recipe.recipeName}");
        DestroyJobDisplay();
    }
    
    private void OnJobCompleted(CraftingJob job, bool success)
    {
        if (job == currentJob)
        {
            string resultText = success ? "Success!" : "Failed!";
            Debug.Log($"Crafting job completed: {job.recipe.recipeName} - {resultText}");
            
            // Show completion effect briefly before destroying
            Invoke("DestroyJobDisplay", 1f);
        }
    }
    
    private void DestroyJobDisplay()
    {
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.OnCraftingCompleted -= OnJobCompleted;
        }
        
        Destroy(gameObject);
    }
    
    private void OnDestroy()
    {
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.OnCraftingCompleted -= OnJobCompleted;
        }
    }
} 