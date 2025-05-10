using UnityEngine;
using System.Collections.Generic;
using System;

public class LocationManager : MonoBehaviour
{
    public static LocationManager Instance { get; private set; }

    // Events
    public event Action<LocationData> OnLocationChanged;
    public event Action<LocationData> OnLocationUnlocked;

    [Header("Current State")]
    [SerializeField] private LocationData currentLocation;
    [SerializeField] private List<LocationData> unlockedLocations = new List<LocationData>();
    [SerializeField] private List<LocationData> allLocations = new List<LocationData>();

    // Discovery bonus from Bridge compartment
    private float discoveryBonus = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLocations();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeLocations()
    {
        // Load all locations from Resources
        allLocations.AddRange(Resources.LoadAll<LocationData>("Locations"));

        // Load location images
        LoadLocationImages();

        // Find and set the starting location
        foreach (var location in allLocations)
        {
            if (location.isStartingLocation)
            {
                currentLocation = location;
                unlockedLocations.Add(location);
                break;
            }
        }

        if (currentLocation == null)
        {
            Debug.LogError("No starting location found!");
        }
    }

    private void LoadLocationImages()
    {
        foreach (var location in allLocations)
        {
            if (location.locationIcon == null)
            {
                // Try to load image from Resources
                string imagePath = $"Locations/Images/{location.locationID}";
                Sprite locationSprite = Resources.Load<Sprite>(imagePath);
                
                if (locationSprite != null)
                {
                    location.locationIcon = locationSprite;
                }
                else
                {
                    Debug.LogWarning($"No image found for location: {location.displayName}");
                    // Assign default image
                    location.locationIcon = Resources.Load<Sprite>("Locations/Images/Default");
                }
            }

            // Load background image if not set
            if (location.backgroundImage == null)
            {
                string bgPath = $"Locations/Images/{location.locationID}_bg";
                Sprite bgSprite = Resources.Load<Sprite>(bgPath);
                
                if (bgSprite != null)
                {
                    location.backgroundImage = bgSprite;
                }
            }
        }
    }

    public bool TryChangeLocation(string locationID)
    {
        LocationData newLocation = GetLocationByID(locationID);

        if (newLocation == null)
        {
            Debug.LogError($"Location with ID '{locationID}' not found!");
            return false;
        }

        if (!unlockedLocations.Contains(newLocation))
        {
            Debug.LogWarning($"Location '{newLocation.displayName}' is not yet unlocked!");
            return false;
        }

        // Check if this is actually a change
        if (currentLocation == newLocation)
        {
            Debug.Log($"Already at location: {newLocation.displayName}");
            return false;
        }

        // Only proceed if it's a real location change
        LocationData oldLocation = currentLocation;
        currentLocation = newLocation;
        Debug.Log($"Location changed from {oldLocation?.displayName ?? "None"} to {newLocation.displayName}");
        OnLocationChanged?.Invoke(currentLocation);
        return true;
    }

    public LocationData GetCurrentLocation()
    {
        return currentLocation;
    }

    public List<LocationData> GetUnlockedLocations()
    {
        return new List<LocationData>(unlockedLocations);
    }

    public List<LocationData> GetAllLocations()
    {
        return new List<LocationData>(allLocations);
    }

    private LocationData GetLocationByID(string id)
    {
        return allLocations.Find(loc => loc.locationID == id);
    }

    public void CheckForLocationUnlocks()
    {
        Debug.Log($"Checking for location unlocks. Total waste collected: {GameManager.Instance.TotalWasteCollected}");

        foreach (var location in allLocations)
        {
            if (!unlockedLocations.Contains(location))
            {
                Debug.Log($"Checking unlock requirements for {location.displayName}");
                Debug.Log($"Required waste: {location.requiredWasteCollected}, Current: {GameManager.Instance.TotalWasteCollected}");

                if (CanUnlockLocation(location))
                {
                    Debug.Log($"Location {location.displayName} can be unlocked!");
                    UnlockLocation(location);
                }
                else
                {
                    Debug.Log($"Location {location.displayName} cannot be unlocked yet");
                }
            }
        }
    }

    private bool CanUnlockLocation(LocationData location)
    {
        // Check waste collection requirement
        if (GameManager.Instance.TotalWasteCollected < location.requiredWasteCollected)
            return false;

        // Check prerequisite location
        if (location.prerequisiteLocation != null && !unlockedLocations.Contains(location.prerequisiteLocation))
            return false;

        // Check required items
        if (location.requiredItems != null && location.requiredItems.Length > 0)
        {
            // TODO: Implement after inventory system is updated
            // For now, we'll skip this check
        }

        return true;
    }

    private void UnlockLocation(LocationData location)
    {
        unlockedLocations.Add(location);
        OnLocationUnlocked?.Invoke(location);
        Debug.Log($"Location unlocked: {location.displayName}");
    }

    /// <summary>
    /// Sets the discovery bonus from the Bridge compartment
    /// </summary>
    public void SetDiscoveryBonus(float bonus)
    {
        discoveryBonus = Mathf.Clamp01(bonus);
        Debug.Log($"LocationManager: Discovery bonus set to {discoveryBonus:P0}");

        // Apply the bonus to discovery chances
        UpdateDiscoveryChances();
    }

    /// <summary>
    /// Updates discovery chances based on current bonus
    /// </summary>
    private void UpdateDiscoveryChances()
    {
        foreach (var location in allLocations)
        {
            if (!location.isDiscovered)
            {
                // Increase base discovery chance by the bonus
                location.discoveryChance = Mathf.Min(
                    location.baseDiscoveryChance * (1f + discoveryBonus),
                    1f
                );
            }
        }
    }
}