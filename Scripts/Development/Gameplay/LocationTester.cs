using UnityEngine;
using System.Collections.Generic;

public class LocationTester : MonoBehaviour
{
    void Update()
    {
        // Press F4 to manually trigger unlock check and see debug info
        if (Input.GetKeyDown(KeyCode.F4))
        {
            Debug.Log($"Manual check - Total waste: {GameManager.Instance.TotalWasteCollected}");
            LocationManager.Instance.CheckForLocationUnlocks();
            
            // Also log current unlocked locations
            var unlockedLocations = LocationManager.Instance.GetUnlockedLocations();
            Debug.Log($"Currently unlocked locations: {unlockedLocations.Count}");
            foreach (var loc in unlockedLocations)
            {
                Debug.Log($"- {loc.displayName}");
            }
        }
    }
} 