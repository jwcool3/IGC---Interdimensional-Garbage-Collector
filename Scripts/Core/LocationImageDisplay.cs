using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocationImageDisplay : MonoBehaviour
{
    [Header("Location Image")]
    [SerializeField] private Image locationImage;
    [SerializeField] private RectTransform viewportFrame;
    
    [Header("Location Details")]
    [SerializeField] private TextMeshProUGUI locationNameText;
    [SerializeField] private TextMeshProUGUI locationDescriptionText;
    [SerializeField] private TextMeshProUGUI dangerLevelText;
    
    [Header("Visual Effects")]
    [SerializeField] private float transitionSpeed = 0.5f;
    [SerializeField] private bool useTransitionEffect = true;
    
    // Optional image overlay effects
    [SerializeField] private Image dimensionalDistortionOverlay;
    [SerializeField] private float distortionIntensity = 0.2f;
    
    private Sprite currentLocationSprite;
    private Coroutine imageTransitionCoroutine;
    
    private void Start()
    {
        // Subscribe to location changed event
        if (LocationManager.Instance != null)
        {
            LocationManager.Instance.OnLocationChanged += UpdateLocationDisplay;
            
            // Initialize with current location
            UpdateLocationDisplay(LocationManager.Instance.GetCurrentLocation());
        }
        else
        {
            Debug.LogError("LocationManager instance not found!");
        }
    }
    
    public void UpdateLocationDisplay(LocationData location)
    {
        if (location == null)
        {
            Debug.LogWarning("Tried to update with null location data");
            return;
        }
        
        // Update location image
        if (locationImage != null)
        {
            if (useTransitionEffect && gameObject.activeInHierarchy)
            {
                if (imageTransitionCoroutine != null)
                    StopCoroutine(imageTransitionCoroutine);
                    
                imageTransitionCoroutine = StartCoroutine(TransitionLocationImage(location.locationIcon));
            }
            else
            {
                // Simple direct update without transition
                locationImage.sprite = location.locationIcon;
            }
        }
        
        // Update text information
        if (locationNameText != null)
            locationNameText.text = location.displayName;
            
        if (locationDescriptionText != null)
            locationDescriptionText.text = location.description;
            
        if (dangerLevelText != null)
            dangerLevelText.text = $"Danger Level: {(location.dangerLevel * 100):F0}%";
        
        // Update distortion effect based on danger level
        if (dimensionalDistortionOverlay != null)
        {
            Color overlayColor = dimensionalDistortionOverlay.color;
            overlayColor.a = location.dangerLevel * distortionIntensity;
            dimensionalDistortionOverlay.color = overlayColor;
        }
        
        Debug.Log($"Updated location display to: {location.displayName}");
    }
    
    private System.Collections.IEnumerator TransitionLocationImage(Sprite newLocationSprite)
    {
        // Fade out current image
        CanvasGroup canvasGroup = locationImage.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = locationImage.gameObject.AddComponent<CanvasGroup>();
        
        float startTime = Time.time;
        float startAlpha = canvasGroup.alpha;
        
        // Fade out
        while (Time.time < startTime + transitionSpeed * 0.5f)
        {
            float progress = (Time.time - startTime) / (transitionSpeed * 0.5f);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
            yield return null;
        }
        
        // Change sprite
        locationImage.sprite = newLocationSprite;
        
        // Fade in
        startTime = Time.time;
        while (Time.time < startTime + transitionSpeed * 0.5f)
        {
            float progress = (Time.time - startTime) / (transitionSpeed * 0.5f);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (LocationManager.Instance != null)
        {
            LocationManager.Instance.OnLocationChanged -= UpdateLocationDisplay;
        }
    }
}