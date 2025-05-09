using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class LocationSelectionManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform locationButtonContainer;
    [SerializeField] private GameObject locationButtonPrefab;
    [SerializeField] private Button travelButton;
    [SerializeField] private GameObject lockedLocationInfo;
    [SerializeField] private TextMeshProUGUI unlockRequirementsText;

    [Header("Location Display")]
    [SerializeField] private LocationImageDisplay imageDisplay;

    // Currently selected location
    private LocationData selectedLocation;
    private List<LocationListItem> locationButtons = new List<LocationListItem>();

    private void Start()
    {
        // Subscribe to events
        if (LocationManager.Instance != null)
        {
            LocationManager.Instance.OnLocationUnlocked += RefreshLocationList;

            // Setup travel button
            if (travelButton != null)
                travelButton.onClick.AddListener(TravelToSelectedLocation);

            // Initial location list
            PopulateLocationList();

            // Set initial selected location
            SelectLocation(LocationManager.Instance.GetCurrentLocation());
        }
        else
        {
            Debug.LogError("LocationManager instance not found!");
        }
    }

    public void PopulateLocationList()
    {
        // Clear existing buttons
        foreach (var button in locationButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }
        locationButtons.Clear();

        // Get all locations
        List<LocationData> allLocations = LocationManager.Instance.GetAllLocations();
        List<LocationData> unlockedLocations = LocationManager.Instance.GetUnlockedLocations();

        // Create buttons for each location
        foreach (var location in allLocations)
        {
            GameObject buttonObj = Instantiate(locationButtonPrefab, locationButtonContainer);
            LocationListItem listItem = buttonObj.GetComponent<LocationListItem>();

            if (listItem != null)
            {
                bool isUnlocked = unlockedLocations.Contains(location);
                listItem.Initialize(location, isUnlocked, this);
                locationButtons.Add(listItem);
            }
        }

        Debug.Log($"Populated location list with {locationButtons.Count} locations");
    }

    public void SelectLocation(LocationData location)
    {
        if (location == null) return;

        selectedLocation = location;

        // Update location display
        if (imageDisplay != null)
            imageDisplay.UpdateLocationDisplay(location);

        // Update selection state on buttons
        foreach (var button in locationButtons)
        {
            if (button != null)
                button.SetSelected(button.GetLocationData() == location);
        }

        // Update travel button state
        bool canTravel = LocationManager.Instance.GetUnlockedLocations().Contains(location)
                       && location != LocationManager.Instance.GetCurrentLocation();

        if (travelButton != null)
        {
            travelButton.interactable = canTravel;
            travelButton.GetComponentInChildren<TextMeshProUGUI>()?.SetText(
                canTravel ? "TRAVEL" : (location == LocationManager.Instance.GetCurrentLocation() ? "CURRENT LOCATION" : "LOCKED")
            );
        }

        // Show unlock requirements for locked locations
        if (lockedLocationInfo != null)
        {
            bool isLocked = !LocationManager.Instance.GetUnlockedLocations().Contains(location);
            lockedLocationInfo.SetActive(isLocked);

            if (isLocked && unlockRequirementsText != null)
            {
                unlockRequirementsText.text = $"Requirements to unlock:\n" +
                                             $"• Collect {location.requiredWasteCollected} waste items\n" +
                                             (location.prerequisiteLocation != null ?
                                              $"• First visit {location.prerequisiteLocation.displayName}\n" : "");
            }
        }

        Debug.Log($"Selected location: {location.displayName}");
    }

    private void TravelToSelectedLocation()
    {
        if (selectedLocation == null) return;

        if (LocationManager.Instance.TryChangeLocation(selectedLocation.locationID))
        {
            Debug.Log($"Traveling to {selectedLocation.displayName}");

            // Update travel button state after travel
            if (travelButton != null)
            {
                travelButton.interactable = false;
                travelButton.GetComponentInChildren<TextMeshProUGUI>()?.SetText("CURRENT LOCATION");
            }
        }
    }

    private void RefreshLocationList(LocationData newlyUnlockedLocation)
    {
        PopulateLocationList();

        // Optionally highlight the newly unlocked location
        if (newlyUnlockedLocation != null)
            SelectLocation(newlyUnlockedLocation);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (LocationManager.Instance != null)
            LocationManager.Instance.OnLocationUnlocked -= RefreshLocationList;

        if (travelButton != null)
            travelButton.onClick.RemoveListener(TravelToSelectedLocation);
    }
}