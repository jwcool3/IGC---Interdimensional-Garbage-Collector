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

    public bool TryChangeLocation(string locationID)
    {
        LocationData newLocation = GetLocationByID(locationID);

        if (newLocation != null && unlockedLocations.Contains(newLocation))
        {
            currentLocation = newLocation;
            OnLocationChanged?.Invoke(currentLocation);
            return true;
        }

        return false;
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
}