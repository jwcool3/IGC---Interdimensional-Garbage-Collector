using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LocationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject locationPanel;
    [SerializeField] private Transform locationButtonContainer;
    [SerializeField] private GameObject locationButtonPrefab;
    
    [Header("Current Location Display")]
    [SerializeField] private TextMeshProUGUI currentLocationText;
    
    [Header("Location Info Display")]
    [SerializeField] private GameObject locationInfoPanel;
    [SerializeField] private TextMeshProUGUI locationNameText;
    [SerializeField] private TextMeshProUGUI locationDescriptionText;
    [SerializeField] private TextMeshProUGUI dangerLevelText;
    
    [Header("Travel Button")]
    [SerializeField] private Button travelButton;
    [SerializeField] private TextMeshProUGUI travelButtonText;

    private List<LocationButton> locationButtons = new List<LocationButton>();
    private LocationData selectedLocation;

    private void Start()
    {
        // Subscribe to events
        if (LocationManager.Instance != null)
        {
            LocationManager.Instance.OnLocationChanged += UpdateCurrentLocationDisplay;
            LocationManager.Instance.OnLocationUnlocked += OnLocationUnlocked;
        }

        // Set up buttons
        if (travelButton != null)
            travelButton.onClick.AddListener(OnTravelButtonClick);

        // Initialize display
        RefreshLocationList();
        UpdateCurrentLocationDisplay(LocationManager.Instance?.GetCurrentLocation());
    }

    public void ToggleLocationPanel()
    {
        locationPanel.SetActive(!locationPanel.activeSelf);
    }

    private void RefreshLocationList()
    {
        // Clear existing buttons
        foreach (var button in locationButtons)
        {
            Destroy(button.gameObject);
        }
        locationButtons.Clear();

        // Create buttons for all locations
        var allLocations = LocationManager.Instance.GetAllLocations();
        var unlockedLocations = LocationManager.Instance.GetUnlockedLocations();

        foreach (var location in allLocations)
        {
            GameObject buttonObj = Instantiate(locationButtonPrefab, locationButtonContainer);
            LocationButton locationButton = buttonObj.GetComponent<LocationButton>();
            
            if (locationButton != null)
            {
                bool isUnlocked = unlockedLocations.Contains(location);
                locationButton.Initialize(location, isUnlocked, this);
                locationButtons.Add(locationButton);
            }
        }
    }

    public void SelectLocation(LocationData location)
    {
        selectedLocation = location;
        UpdateTravelButton();
        
        // Update selected state on buttons
        foreach (var button in locationButtons)
        {
            button.SetSelected(button.gameObject.GetComponent<LocationButton>().GetLocationData() == location);
        }
    }

    private void UpdateTravelButton()
    {
        if (travelButton != null && selectedLocation != null)
        {
            bool canTravel = selectedLocation != LocationManager.Instance.GetCurrentLocation();
            travelButton.interactable = canTravel;
            travelButtonText.text = canTravel ? "Travel" : "Current Location";
        }
    }

    private void OnTravelButtonClick()
    {
        if (selectedLocation != null && LocationManager.Instance != null)
        {
            if (LocationManager.Instance.TryChangeLocation(selectedLocation.locationID))
            {
                Debug.Log($"Traveled to {selectedLocation.displayName}");
            }
        }
    }

    public void ShowLocationInfo(LocationData location)
    {
        if (locationInfoPanel != null)
        {
            locationInfoPanel.SetActive(true);
            locationNameText.text = location.displayName;
            locationDescriptionText.text = location.description;
            dangerLevelText.text = $"Danger Level: {location.dangerLevel:P0}";
        }
    }

    public void HideLocationInfo()
    {
        if (locationInfoPanel != null)
        {
            locationInfoPanel.SetActive(false);
        }
    }

    private void UpdateCurrentLocationDisplay(LocationData location)
    {
        if (currentLocationText != null && location != null)
        {
            currentLocationText.text = $"Current Location: {location.displayName}";
        }
    }

    private void OnLocationUnlocked(LocationData location)
    {
        RefreshLocationList();
    }

    private void OnDestroy()
    {
        if (LocationManager.Instance != null)
        {
            LocationManager.Instance.OnLocationChanged -= UpdateCurrentLocationDisplay;
            LocationManager.Instance.OnLocationUnlocked -= OnLocationUnlocked;
        }

        if (travelButton != null)
            travelButton.onClick.RemoveListener(OnTravelButtonClick);
    }
}