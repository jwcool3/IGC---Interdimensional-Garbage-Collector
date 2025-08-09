using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurrentLocationDisplay : MonoBehaviour
{
    [SerializeField] private Image locationImageDisplay;
    [SerializeField] private TextMeshProUGUI locationNameText;

    // Optional border/background that changes based on danger level
    [SerializeField] private Image dangerIndicatorBorder;
    [SerializeField] private Gradient dangerColorGradient;

    private void Start()
    {
        // Subscribe to location changed event
        if (LocationManager.Instance != null)
        {
            LocationManager.Instance.OnLocationChanged += UpdateCurrentLocationDisplay;

            // Initialize with current location
            UpdateCurrentLocationDisplay(LocationManager.Instance.GetCurrentLocation());
        }
        else
        {
            Debug.LogError("LocationManager instance not found!");
        }
    }

    public void UpdateCurrentLocationDisplay(LocationData location)
    {
        if (location == null)
        {
            Debug.LogWarning("Tried to update current location display with null data");
            return;
        }

        // Update the image
        if (locationImageDisplay != null && location.locationIcon != null)
        {
            locationImageDisplay.sprite = location.locationIcon;
            locationImageDisplay.enabled = true;
        }

        // Update the text
        if (locationNameText != null)
        {
            locationNameText.text = location.displayName;
        }

        // Update danger indicator if available
        if (dangerIndicatorBorder != null)
        {
            dangerIndicatorBorder.color = dangerColorGradient.Evaluate(location.dangerLevel);
        }

        Debug.Log($"Updated current location display: {location.displayName}");
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (LocationManager.Instance != null)
        {
            LocationManager.Instance.OnLocationChanged -= UpdateCurrentLocationDisplay;
        }
    }
}